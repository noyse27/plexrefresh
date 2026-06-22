using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using plexrefresh.Models;

namespace plexrefresh.Services;

public class PlexClient
{
    private readonly HttpClient _http;

    public PlexClient(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromSeconds(30);
        // Identify as a Plex client - minimal headers
        _http.DefaultRequestHeaders.Add("X-Plex-Product", "plexrefresh");
        _http.DefaultRequestHeaders.Add("X-Plex-Version", "1.0");
        _http.DefaultRequestHeaders.Add("X-Plex-Platform", "Windows");
        _http.DefaultRequestHeaders.Add("X-Plex-Client-Identifier", Environment.MachineName);
        _http.DefaultRequestHeaders.Add("X-Plex-Provides", "player,controller");
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
    }

    private string? _token;
    private string? _baseUrl;

    public void Configure(string baseUrl, string token)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) throw new ArgumentException("Base URL is empty");
        
        var trimmed = baseUrl.Trim();
        if (!trimmed.Contains("://"))
        {
            trimmed = "http://" + trimmed;
        }
        if (!trimmed.EndsWith("/")) trimmed += "/";
        
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var tmp))
        {
            if (tmp.IsDefaultPort && !baseUrl.Contains($":{tmp.Port}"))
            {
                var builder = new UriBuilder(tmp) { Port = 32400 };
                trimmed = builder.Uri.ToString();
            }
        }
        
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Ungültige Server URL. Bitte im Format http(s)://host[:port] angeben.");
        }

        _baseUrl = trimmed;
        _token = token;
        
        // We no longer set _http.BaseAddress here because it can only be set once.
        // We will build absolute URIs in the request methods.
    }

    private Uri GetUri(string relativePath)
    {
        if (string.IsNullOrEmpty(_baseUrl)) throw new InvalidOperationException("PlexClient not configured.");
        return new Uri(new Uri(_baseUrl), relativePath);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string relativePath)
    {
        var request = new HttpRequestMessage(method, GetUri(relativePath));
        if (!string.IsNullOrEmpty(_token))
        {
            request.Headers.Add("X-Plex-Token", _token);
        }
        return request;
    }

    public async Task<bool> CheckAuthAsync()
    {
        using var req = CreateRequest(HttpMethod.Get, "library/sections");
        using var resp = await _http.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }

    public async Task<List<PlexLibrary>> GetLibrariesAsync()
    {
        using var req = CreateRequest(HttpMethod.Get, "library/sections");
        using var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        var xml = XDocument.Parse(await resp.Content.ReadAsStringAsync());
        // Plex returns XML. Parse Directory nodes
        var libs = xml.Descendants()
            .Where(e => e.Name.LocalName == "Directory")
            .Select(d => new PlexLibrary
            {
                Key = d.Attribute("key")?.Value ?? string.Empty,
                Title = d.Attribute("title")?.Value ?? string.Empty,
            })
            .ToList();

        // For each library, fetch folders
        foreach (var lib in libs)
        {
            lib.Folders = await GetLibraryFoldersAsync(lib.Key);
        }
        return libs;
    }

    public async Task<List<PlexFolder>> GetLibraryFoldersAsync(string libraryKey)
    {
        // Try the section details first
        using var req = CreateRequest(HttpMethod.Get, $"library/sections/{libraryKey}?includeLocations=1");
        using var resp = await _http.SendAsync(req);
        var locations = new List<PlexFolder>();
        if (resp.IsSuccessStatusCode)
        {
            var xml = XDocument.Parse(await resp.Content.ReadAsStringAsync());
            var dir = xml.Descendants().FirstOrDefault(e => e.Name.LocalName == "Directory");
            var locationElements = dir != null
                ? dir.Elements().Where(e => e.Name.LocalName == "Location")
                : xml.Descendants().Where(e => e.Name.LocalName == "Location");
            locations = locationElements
                .Select(x => new PlexFolder
                {
                    Key = x.Attribute("id")?.Value ?? (x.Attribute("path")?.Value ?? string.Empty),
                    Path = x.Attribute("path")?.Value ?? string.Empty
                })
                .Where(f => !string.IsNullOrWhiteSpace(f.Path))
                .ToList();
        }
        // Fallback: some Plex servers only include Location nodes in the sections listing
        if (locations.Count == 0)
        {
            using var req2 = CreateRequest(HttpMethod.Get, "library/sections");
            using var listResp = await _http.SendAsync(req2);
            listResp.EnsureSuccessStatusCode();
            var listXml = XDocument.Parse(await listResp.Content.ReadAsStringAsync());
            var matchingDir = listXml
                .Descendants().Where(e => e.Name.LocalName == "Directory")
                .FirstOrDefault(e => (string?)e.Attribute("key") == libraryKey);
            if (matchingDir != null)
            {
                locations = matchingDir
                    .Elements().Where(e => e.Name.LocalName == "Location")
                    .Select(x => new PlexFolder
                    {
                        Key = x.Attribute("id")?.Value ?? (x.Attribute("path")?.Value ?? string.Empty),
                        Path = x.Attribute("path")?.Value ?? string.Empty
                    })
                    .Where(f => !string.IsNullOrWhiteSpace(f.Path))
                    .ToList();
            }
        }
        return locations;
    }

    public async Task RefreshLibraryAsync(string libraryKey)
    {
        using var req = CreateRequest(HttpMethod.Get, $"library/sections/{libraryKey}/refresh");
        using var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
    }

    public async Task RefreshFolderAsync(string libraryKey, string folderPath)
    {
        // The metadata/refresh allows a 'path' query to limit scan
        // Fallback to section scan if not supported
        var pathParam = Uri.EscapeDataString(folderPath);
        var requestUri = $"library/sections/{libraryKey}/refresh?path={pathParam}";
        using var req = CreateRequest(HttpMethod.Get, requestUri);
        using var resp = await _http.SendAsync(req);
        if (!resp.IsSuccessStatusCode)
        {
            // Try legacy refresh all
            using var req2 = CreateRequest(HttpMethod.Get, $"library/sections/{libraryKey}/refresh");
            using var fallback = await _http.SendAsync(req2);
            fallback.EnsureSuccessStatusCode();
        }
    }
}