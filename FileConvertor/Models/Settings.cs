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
                HotkeyDisplayText = "Ctrl+Alt+C"
            };

            return settings;
        }
    }
}
