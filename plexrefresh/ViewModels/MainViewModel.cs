using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using plexrefresh.Models;
using plexrefresh.Services;

namespace plexrefresh.ViewModels;

public class RelayCommand : ICommand
{
    private readonly Func<bool>? _can;
    private readonly Action _act;
    public RelayCommand(Action act, Func<bool>? can = null) { _act = act; _can = can; }
    public bool CanExecute(object? parameter) => _can?.Invoke() ?? true;
    public void Execute(object? parameter) => _act();
    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IStorageService _storage;
    private readonly PlexClient _plex;
    private AppState _state = new();

    public ObservableCollection<PlexLibrary> Libraries { get; } = new();
    public ObservableCollection<PlexFolder> Folders { get; } = new();

    private PlexLibrary? _selectedLibrary;
    public PlexLibrary? SelectedLibrary
    {
        get => _selectedLibrary;
        set
        {
            if (Set(ref _selectedLibrary, value))
            {
                Folders.Clear();
                if (value != null)
                {
                    foreach (var f in value.Folders)
                        Folders.Add(f);
                }
                if (RefreshLibraryCommand is RelayCommand rlc) rlc.RaiseCanExecuteChanged();
                if (RefreshFolderCommand is RelayCommand rfc) rfc.RaiseCanExecuteChanged();
            }
        }
    }

    private PlexFolder? _selectedFolder;
    public PlexFolder? SelectedFolder { get => _selectedFolder; set { if (Set(ref _selectedFolder, value)) { if (RefreshFolderCommand is RelayCommand rfc) rfc.RaiseCanExecuteChanged(); } } }

    public string? ServerUrl
    {
        get => _state.Auth.ServerUrl;
        set
        {
            if (_state.Auth.ServerUrl != value)
            {
                _state.Auth.ServerUrl = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsConfigured));
                (ReloadLibrariesCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CheckAuthCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string? Token
    {
        get => _state.Auth.Token;
        set
        {
            if (_state.Auth.Token != value)
            {
                _state.Auth.Token = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsConfigured));
                (ReloadLibrariesCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CheckAuthCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    private string _status = "";
    private string _configStatus = "";
    private bool _isTokenVisible;

    public string Status { get => _status; set => Set(ref _status, value); }
    public string ConfigStatus { get => _configStatus; set => Set(ref _configStatus, value); }

    public bool IsTokenVisible
    {
        get => _isTokenVisible;
        set => Set(ref _isTokenVisible, value);
    }

    public AppStrings CurrentStrings => _state.Language == "en" ? AppStrings.En : AppStrings.De;

    public static string AppVersion =>
        System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(2) ?? "1.0";

    public ICommand RefreshLibraryCommand { get; }
    public ICommand RefreshFolderCommand { get; }
    public ICommand ReloadLibrariesCommand { get; }
    public ICommand CheckAuthCommand { get; }
    public ICommand SaveConfigCommand { get; }
    public ICommand ToggleTokenVisibilityCommand { get; }
    public ICommand ToggleLanguageCommand { get; }

    public MainViewModel()
    {
        _storage = new FileStorageService();
        _plex = new PlexClient(new HttpClient());
        RefreshLibraryCommand = new RelayCommand(() => _ = RefreshLibraryAsync(), () => SelectedLibrary != null);
        RefreshFolderCommand = new RelayCommand(() => _ = RefreshFolderAsync(), () => SelectedLibrary != null && SelectedFolder != null);
        ReloadLibrariesCommand = new RelayCommand(() => _ = LoadLibrariesAsync(), () => IsConfigured);
        CheckAuthCommand = new RelayCommand(() => _ = CheckAuthAsync(), () => IsConfigured);
        SaveConfigCommand = new RelayCommand(() => _ = SaveConfigAsync());
        ToggleTokenVisibilityCommand = new RelayCommand(() => IsTokenVisible = !IsTokenVisible);
        ToggleLanguageCommand = new RelayCommand(() => _ = ToggleLanguageAsync());
        _ = InitializeAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? prop = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(prop);
        return true;
    }
    private void OnPropertyChanged([CallerMemberName] string? prop = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ServerUrl) && !string.IsNullOrWhiteSpace(Token);

    private void ApplyPlexConfig()
    {
        if (IsConfigured)
            _plex.Configure(ServerUrl!, Token!);
    }

    private async Task ToggleLanguageAsync()
    {
        _state.Language = _state.Language == "en" ? "de" : "en";
        OnPropertyChanged(nameof(CurrentStrings));
        Status = CurrentStrings.StatusReady;
        ConfigStatus = "";
        await _storage.SaveAsync(_state);
    }

    private async Task InitializeAsync()
    {
        try
        {
            _state = await _storage.LoadAsync();

            OnPropertyChanged(nameof(ServerUrl));
            OnPropertyChanged(nameof(Token));
            OnPropertyChanged(nameof(IsConfigured));
            OnPropertyChanged(nameof(CurrentStrings));
            (ReloadLibrariesCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (CheckAuthCommand as RelayCommand)?.RaiseCanExecuteChanged();
            ApplyPlexConfig();

            Libraries.Clear();
            if (_state.Libraries != null)
            {
                foreach (var l in _state.Libraries)
                    Libraries.Add(l);
            }
            if (Libraries.Count > 0)
            {
                SelectedLibrary = Libraries[0];
                if (SelectedLibrary?.Folders?.Count > 0)
                    SelectedFolder = SelectedLibrary.Folders[0];
            }

            Status = IsConfigured
                ? CurrentStrings.StatusConfigLoaded
                : CurrentStrings.StatusNotConfigured;
        }
        catch (Exception ex)
        {
            Status = CurrentStrings.StatusStartupError + ex.Message;
        }
    }

    private async Task SaveConfigAsync()
    {
        try
        {
            ApplyPlexConfig();
            await _storage.SaveAsync(_state);
            ConfigStatus = CurrentStrings.ConfigSaved;
        }
        catch (Exception ex)
        {
            ConfigStatus = CurrentStrings.SaveError + ex.Message;
        }
    }

    private async Task LoadLibrariesAsync()
    {
        if (!IsConfigured) { ConfigStatus = CurrentStrings.ConfigIncomplete; return; }
        try
        {
            ConfigStatus = CurrentStrings.LibrariesLoading;
            ApplyPlexConfig();
            var libs = await _plex.GetLibrariesAsync();
            _state.Libraries = new System.Collections.Generic.List<PlexLibrary>(libs);
            Libraries.Clear();
            foreach (var l in libs) Libraries.Add(l);
            await _storage.SaveAsync(_state);
            ConfigStatus = string.Format(CurrentStrings.LibrariesLoaded, Libraries.Count);
        }
        catch (Exception ex)
        {
            ConfigStatus = CurrentStrings.LibrariesLoadError + ex.Message;
        }
    }

    private async Task CheckAuthAsync()
    {
        if (!IsConfigured) { ConfigStatus = CurrentStrings.AuthPrompt; return; }
        try
        {
            ApplyPlexConfig();
            ConfigStatus = CurrentStrings.AuthChecking;
            var ok = await _plex.CheckAuthAsync();
            ConfigStatus = ok ? CurrentStrings.AuthOk : CurrentStrings.AuthFailed;
        }
        catch (Exception ex)
        {
            ConfigStatus = CurrentStrings.AuthError + ex.Message;
        }
    }

    private async Task RefreshLibraryAsync()
    {
        if (SelectedLibrary == null) return;
        try
        {
            Status = string.Format(CurrentStrings.LibraryRefreshing, SelectedLibrary.Title);
            await _plex.RefreshLibraryAsync(SelectedLibrary.Key);
            Status = CurrentStrings.LibraryRefreshed;
        }
        catch (Exception ex)
        {
            Status = CurrentStrings.LibraryRefreshError + ex.Message;
        }
    }

    private async Task RefreshFolderAsync()
    {
        if (SelectedLibrary == null || SelectedFolder == null) return;
        try
        {
            Status = string.Format(CurrentStrings.FolderRefreshing, SelectedFolder.Path);
            await _plex.RefreshFolderAsync(SelectedLibrary.Key, SelectedFolder.Path);
            Status = CurrentStrings.FolderRefreshed;
        }
        catch (Exception ex)
        {
            Status = CurrentStrings.FolderRefreshError + ex.Message;
        }
    }
}
