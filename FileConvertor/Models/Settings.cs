using System;
using System.Windows.Input;

namespace FileConvertor.Models
{
    /// <summary>
    /// Model class for application settings
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Gets or sets the hotkey modifiers (Alt, Ctrl, Shift, Win)
        /// </summary>
        public int HotkeyModifiers { get; set; }

        /// <summary>
        /// Gets or sets the hotkey key
        /// </summary>
        public int HotkeyKey { get; set; }

        /// <summary>
        /// Gets or sets the hotkey display text
        /// </summary>
        public string HotkeyDisplayText { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether automatic update checking is enabled
        /// </summary>
        public bool AutoCheckForUpdates { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the last time updates were checked
        /// </summary>
        public DateTime LastUpdateCheck { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// Gets or sets the latest available version
        /// </summary>
        public string LatestAvailableVersion { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether an update is available
        /// </summary>
        public bool UpdateAvailable { get; set; } = false;

        /// <summary>
        /// Creates a new instance of the Settings class with default values
        /// </summary>
        /// <returns>Settings instance with default values</returns>
        public static Settings CreateDefault()
        {
            // Default hotkey is Ctrl+Alt+C
            var settings = new Settings
            {
                HotkeyModifiers = Core.Services.HotkeyService.MOD_CONTROL | Core.Services.HotkeyService.MOD_ALT,
                HotkeyKey = (int)'C',
                HotkeyDisplayText = "Ctrl+Alt+C",
                AutoCheckForUpdates = true,
                LastUpdateCheck = DateTime.MinValue,
                LatestAvailableVersion = string.Empty,
                UpdateAvailable = false
            };

            return settings;
        }
    }
}
