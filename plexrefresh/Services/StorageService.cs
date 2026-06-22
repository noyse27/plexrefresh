using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using plexrefresh.Models;

namespace plexrefresh.Services;

public interface IStorageService
{
    Task<AppState> LoadAsync();
    Task SaveAsync(AppState state);
}

public class FileStorageService : IStorageService
{
    private readonly string _baseDir;
    private readonly string _filePath;

    public FileStorageService()
    {
        _baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "plexrefresh");
        _filePath = Path.Combine(_baseDir, "state.json");
        Directory.CreateDirectory(_baseDir);
    }

    public async Task<AppState> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new AppState();
        try
        {
            await using var fs = File.OpenRead(_filePath);
            var state = await JsonSerializer.DeserializeAsync<AppState>(fs, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            });
            return state ?? new AppState();
        }
        catch
        {
            return new AppState();
        }
    }

    public async Task SaveAsync(AppState state)
    {
        await using var fs = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(fs, state, new JsonSerializerOptions { WriteIndented = true });
    }
}