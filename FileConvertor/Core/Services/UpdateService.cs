using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using FileConvertor.Core.Interfaces;
using FileConvertor.Core.Logging;

namespace FileConvertor.Core.Services
{
    /// <summary>
    /// Service for checking and handling application updates
    /// </summary>
    public class UpdateService : IUpdateService
    {
        private readonly ISettingsService _settingsService;
        private readonly HttpClient _httpClient;
        private const string GitHubApiUrl = "https://api.github.com/repos/FourTwentyDev/ClipConvert/releases/latest";
        private const string GitHubReleaseUrl = "https://github.com/FourTwentyDev/ClipConvert/releases/latest";
        private const string UserAgent = "ClipConvert-UpdateChecker";

        /// <summary>
        /// Event that is raised when an update is available
        /// </summary>
        public event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

        /// <summary>
        /// Initializes a new instance of the UpdateService class
        /// </summary>
        /// <param name="settingsService">Settings service</param>
        public UpdateService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            
            // Initialize HttpClient
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        /// <summary>
        /// Gets the current application version from the assembly
        /// </summary>
        /// <returns>Current version string</returns>
        public string GetCurrentVersion()
        {
            try
            {
                // Get the version from the assembly
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                
                // Format as "major.minor.build"
                return $"{version?.Major}.{version?.Minor}.{version?.Build}";
            }
            catch (Exception ex)
            {
                Logger.LogException(LogLevel.Error, "UpdateService", "Error getting current version", ex);
                return "Unknown";
            }
        }

        /// <summary>
        /// Checks if an update is available
        /// </summary>
        /// <returns>True if an update is available, false otherwise</returns>
        public async Task<bool> CheckForUpdateAsync()
        {
            try
            {
                // Get the current settings
                var settings = _settingsService.CurrentSettings;
                
                // Check if automatic updates are enabled
                if (!settings.AutoCheckForUpdates)
                {
                    Logger.Log(LogLevel.Info, "UpdateService", "Automatic update checking is disabled");
                    return false;
                }
                
                // Check if we've already checked recently (within the last 24 hours)
                var lastCheck = settings.LastUpdateCheck;
                if (DateTime.Now - lastCheck < TimeSpan.FromHours(24) && settings.UpdateAvailable)
                {
                    // We've already checked recently and an update is available
                    Logger.Log(LogLevel.Info, "UpdateService", "Using cached update status: Update available");
                    return true;
                }
                
                // Get the current and latest versions
                var currentVersion = GetCurrentVersion();
                var latestVersion = await GetLatestVersionAsync();
                
                // Update the last check time
                settings.LastUpdateCheck = DateTime.Now;
                
                // Compare versions
                var isUpdateAvailable = CompareVersions(currentVersion, latestVersion) < 0;
                
                // Update settings
                settings.LatestAvailableVersion = latestVersion;
                settings.UpdateAvailable = isUpdateAvailable;
                _settingsService.SaveSettings(settings);
                
                // Raise event if an update is available
                if (isUpdateAvailable)
                {
                    Logger.Log(LogLevel.Info, "UpdateService", $"Update available: {currentVersion} -> {latestVersion}");
                    OnUpdateAvailable(new UpdateAvailableEventArgs(currentVersion, latestVersion));
                }
                else
                {
                    Logger.Log(LogLevel.Info, "UpdateService", $"No update available. Current version: {currentVersion}, Latest version: {latestVersion}");
                }
                
                return isUpdateAvailable;
            }
            catch (Exception ex)
            {
                Logger.LogException(LogLevel.Error, "UpdateService", "Error checking for updates", ex);
                return false;
            }
        }

        /// <summary>
        /// Gets the latest available version from GitHub
        /// </summary>
        /// <returns>Latest version string</returns>
        public async Task<string> GetLatestVersionAsync()
        {
            try
            {
                // Get the latest release from GitHub
                var response = await _httpClient.GetStringAsync(GitHubApiUrl);
                
                // Parse the JSON response
                using var document = JsonDocument.Parse(response);
                var root = document.RootElement;
                
                // Get the tag name (e.g., "v1.0.1")
                var tagName = root.GetProperty("tag_name").GetString();
                
                // Remove the "v" prefix if present
                if (tagName != null && tagName.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                {
                    tagName = tagName.Substring(1);
                }
                
                return tagName ?? "Unknown";
            }
            catch (Exception ex)
            {
                Logger.LogException(LogLevel.Error, "UpdateService", "Error getting latest version", ex);
                return "Unknown";
            }
        }

        /// <summary>
        /// Opens the download page for the latest version
        /// </summary>
        public void DownloadUpdate()
        {
            try
            {
                // Open the GitHub releases page in the default browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = GitHubReleaseUrl,
                    UseShellExecute = true
                });
                
                Logger.Log(LogLevel.Info, "UpdateService", $"Opening download page: {GitHubReleaseUrl}");
            }
            catch (Exception ex)
            {
                Logger.LogException(LogLevel.Error, "UpdateService", "Error opening download page", ex);
            }
        }

        /// <summary>
        /// Compares two version strings
        /// </summary>
        /// <param name="version1">First version</param>
        /// <param name="version2">Second version</param>
        /// <returns>-1 if version1 is less than version2, 0 if equal, 1 if greater</returns>
        private int CompareVersions(string version1, string version2)
        {
            // Parse the versions
            if (!Version.TryParse(version1, out var v1))
            {
                v1 = new Version(0, 0, 0);
            }
            
            if (!Version.TryParse(version2, out var v2))
            {
                v2 = new Version(0, 0, 0);
            }
            
            // Compare the versions
            return v1.CompareTo(v2);
        }

        /// <summary>
        /// Raises the UpdateAvailable event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnUpdateAvailable(UpdateAvailableEventArgs e)
        {
            UpdateAvailable?.Invoke(this, e);
        }
    }
}
