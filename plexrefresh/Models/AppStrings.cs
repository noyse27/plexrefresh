namespace plexrefresh.Models;

public class AppStrings
{
    public string MainTabHeader { get; init; } = "";
    public string ConfigTabHeader { get; init; } = "";
    public string LibraryLabel { get; init; } = "";
    public string FolderLabel { get; init; } = "";
    public string RefreshButton { get; init; } = "";
    public string ServerUrlLabel { get; init; } = "";
    public string TokenLabel { get; init; } = "";
    public string ToggleTokenTooltip { get; init; } = "";
    public string SaveButton { get; init; } = "";
    public string CheckAuthButton { get; init; } = "";
    public string ReloadLibsButton { get; init; } = "";
    public string LanguageButton { get; init; } = "";
    public string AboutTabHeader { get; init; } = "";
    public string ContactLabel { get; init; } = "";

    public string StatusReady { get; init; } = "";
    public string StatusNotConfigured { get; init; } = "";
    public string StatusConfigLoaded { get; init; } = "";
    public string StatusStartupError { get; init; } = "";
    public string ConfigSaved { get; init; } = "";
    public string SaveError { get; init; } = "";
    public string ConfigIncomplete { get; init; } = "";
    public string LibrariesLoading { get; init; } = "";
    public string LibrariesLoaded { get; init; } = "";  // format: {0} = count
    public string LibrariesLoadError { get; init; } = "";
    public string AuthPrompt { get; init; } = "";
    public string AuthChecking { get; init; } = "";
    public string AuthOk { get; init; } = "";
    public string AuthFailed { get; init; } = "";
    public string AuthError { get; init; } = "";
    public string LibraryRefreshing { get; init; } = "";  // format: {0} = title
    public string LibraryRefreshed { get; init; } = "";
    public string LibraryRefreshError { get; init; } = "";
    public string FolderRefreshing { get; init; } = "";  // format: {0} = path
    public string FolderRefreshed { get; init; } = "";
    public string FolderRefreshError { get; init; } = "";

    public static readonly AppStrings De = new()
    {
        MainTabHeader = "Plex Refresh",
        ConfigTabHeader = "Konfiguration",
        LibraryLabel = "Bibliothek",
        FolderLabel = "Ordner",
        RefreshButton = "Aktualisieren",
        ServerUrlLabel = "Server URL:",
        TokenLabel = "Token:",
        ToggleTokenTooltip = "Token anzeigen/verstecken",
        SaveButton = "Speichern",
        CheckAuthButton = "Auth prüfen",
        ReloadLibsButton = "Bibliotheken laden",
        LanguageButton = "EN",
        AboutTabHeader = "Über",
        ContactLabel = "Kontakt Email",
        StatusReady = "Bereit",
        StatusNotConfigured = "Bitte Server URL und Token eingeben, dann 'Speichern' und 'Auth prüfen' klicken. Danach 'Bibliotheken laden'.",
        StatusConfigLoaded = "Konfiguration geladen. Bitte 'Auth prüfen' klicken und anschließend 'Bibliotheken laden'.",
        StatusStartupError = "Fehler beim Starten: ",
        ConfigSaved = "Konfiguration gespeichert. Hinweis: Server URL im Format http(s)://host[:port] angeben.",
        SaveError = "Fehler beim Speichern: ",
        ConfigIncomplete = "Konfiguration unvollständig.",
        LibrariesLoading = "Bibliotheken werden geladen...",
        LibrariesLoaded = "{0} Bibliotheken geladen.",
        LibrariesLoadError = "Fehler beim Laden der Bibliotheken: ",
        AuthPrompt = "Bitte Konfiguration eingeben.",
        AuthChecking = "Authentifizierung wird geprüft...",
        AuthOk = "Authentifizierung OK.",
        AuthFailed = "Nicht authentifiziert. Bitte Token prüfen und neu speichern.",
        AuthError = "Fehler bei Auth-Prüfung: ",
        LibraryRefreshing = "Bibliothek '{0}' wird aktualisiert...",
        LibraryRefreshed = "Bibliothek-Aktualisierung angestoßen.",
        LibraryRefreshError = "Fehler bei Aktualisierung: ",
        FolderRefreshing = "Ordner '{0}' wird aktualisiert...",
        FolderRefreshed = "Ordner-Aktualisierung angestoßen.",
        FolderRefreshError = "Fehler bei Ordner-Aktualisierung: "
    };

    public static readonly AppStrings En = new()
    {
        MainTabHeader = "Plex Refresh",
        ConfigTabHeader = "Config",
        LibraryLabel = "Library",
        FolderLabel = "Folder",
        RefreshButton = "Refresh",
        ServerUrlLabel = "Server URL:",
        TokenLabel = "Token:",
        ToggleTokenTooltip = "Show/hide token",
        SaveButton = "Save",
        CheckAuthButton = "Check Auth",
        ReloadLibsButton = "Reload Libs",
        LanguageButton = "DE",
        AboutTabHeader = "About",
        ContactLabel = "Contact Email",
        StatusReady = "Ready",
        StatusNotConfigured = "Please enter Server URL and Token, then click 'Save' and 'Check Auth'. Afterwards click 'Reload Libs'.",
        StatusConfigLoaded = "Configuration loaded. Please click 'Check Auth' and then 'Reload Libs'.",
        StatusStartupError = "Startup error: ",
        ConfigSaved = "Configuration saved. Note: Server URL format is http(s)://host[:port].",
        SaveError = "Save error: ",
        ConfigIncomplete = "Configuration incomplete.",
        LibrariesLoading = "Loading libraries...",
        LibrariesLoaded = "{0} libraries loaded.",
        LibrariesLoadError = "Error loading libraries: ",
        AuthPrompt = "Please enter configuration.",
        AuthChecking = "Checking authentication...",
        AuthOk = "Authentication OK.",
        AuthFailed = "Not authenticated. Please check token and save again.",
        AuthError = "Authentication error: ",
        LibraryRefreshing = "Refreshing library '{0}'...",
        LibraryRefreshed = "Library refresh triggered.",
        LibraryRefreshError = "Refresh error: ",
        FolderRefreshing = "Refreshing folder '{0}'...",
        FolderRefreshed = "Folder refresh triggered.",
        FolderRefreshError = "Folder refresh error: "
    };
}
