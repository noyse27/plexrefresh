using System.Collections.Generic;

namespace plexrefresh.Models;

public class AuthConfig
{
    public string? ServerUrl { get; set; }
    public string? Token { get; set; }
}

public class PlexFolder
{
    public string Key { get; set; } = string.Empty; // path or key used by Plex
    public string Path { get; set; } = string.Empty; // human readable path
}

public class PlexLibrary
{
    public string Key { get; set; } = string.Empty; // library key (section id)
    public string Title { get; set; } = string.Empty;
    public List<PlexFolder> Folders { get; set; } = new();
}

public class AppState
{
    public AuthConfig Auth { get; set; } = new();
    public List<PlexLibrary> Libraries { get; set; } = new();
}