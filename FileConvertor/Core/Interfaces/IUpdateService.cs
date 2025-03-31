using System;
using System.Threading.Tasks;

namespace FileConvertor.Core.Interfaces
{
    /// <summary>
    /// Interface for the update service
    /// </summary>
    public interface IUpdateService
    {
        /// <summary>
        /// Gets the current application version from the assembly
        /// </summary>
        /// <returns>Current version string</returns>
        string GetCurrentVersion();
        
        /// <summary>
        /// Checks if an update is available
        /// </summary>
        /// <returns>True if an update is available, false otherwise</returns>
        Task<bool> CheckForUpdateAsync();
        
        /// <summary>
        /// Gets the latest available version from GitHub
        /// </summary>
        /// <returns>Latest version string</returns>
        Task<string> GetLatestVersionAsync();
        
        /// <summary>
        /// Opens the download page for the latest version
        /// </summary>
        void DownloadUpdate();
        
        /// <summary>
        /// Event that is raised when an update is available
        /// </summary>
        event EventHandler<UpdateAvailableEventArgs> UpdateAvailable;
    }
    
    /// <summary>
    /// Event arguments for the UpdateAvailable event
    /// </summary>
    public class UpdateAvailableEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the current version
        /// </summary>
        public string CurrentVersion { get; }
        
        /// <summary>
        /// Gets the latest available version
        /// </summary>
        public string LatestVersion { get; }
        
        /// <summary>
        /// Initializes a new instance of the UpdateAvailableEventArgs class
        /// </summary>
        /// <param name="currentVersion">Current version</param>
        /// <param name="latestVersion">Latest available version</param>
        public UpdateAvailableEventArgs(string currentVersion, string latestVersion)
        {
            CurrentVersion = currentVersion;
            LatestVersion = latestVersion;
        }
    }
}
