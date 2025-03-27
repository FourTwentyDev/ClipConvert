using System;
using FileConvertor.Models;

namespace FileConvertor.Core.Interfaces
{
    /// <summary>
    /// Interface for managing application settings
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Gets the current settings
        /// </summary>
        Settings CurrentSettings { get; }

        /// <summary>
        /// Saves settings to the settings file
        /// </summary>
        /// <param name="settings">Settings to save</param>
        /// <returns>True if successful, false otherwise</returns>
        bool SaveSettings(Settings settings);

        /// <summary>
        /// Updates the hotkey settings
        /// </summary>
        /// <param name="modifiers">Key modifiers (Alt, Ctrl, Shift, Win)</param>
        /// <param name="key">The key</param>
        /// <param name="displayText">Display text for the hotkey</param>
        /// <returns>True if successful, false otherwise</returns>
        bool UpdateHotkey(int modifiers, int key, string displayText);
    }
}
