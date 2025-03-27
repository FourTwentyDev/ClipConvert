using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FileConvertor.Core.Interfaces;
using FileConvertor.Core.Logging;
using FileConvertor.Models;

namespace FileConvertor.Core.Services
{
    /// <summary>
    /// Service for managing application settings
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private Settings _currentSettings;

        /// <summary>
        /// Gets the current settings
        /// </summary>
        public Settings CurrentSettings => _currentSettings;

        /// <summary>
        /// Initializes a new instance of the SettingsService class
        /// </summary>
        public SettingsService()
        {
            // Create settings directory in AppData
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var settingsDirectory = Path.Combine(appDataPath, "FileConvertor", "Settings");
            
            // Ensure the directory exists
            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }
            
            _settingsFilePath = Path.Combine(settingsDirectory, "settings.json");
            
            // Load settings or create default
            _currentSettings = LoadSettings() ?? Settings.CreateDefault();
            
            // Save settings to ensure the file exists
            SaveSettings(_currentSettings);
        }

        /// <summary>
        /// Loads settings from the settings file
        /// </summary>
        /// <returns>Settings object or null if file doesn't exist or is invalid</returns>
        private Settings? LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    Logger.Log(LogLevel.Info, "SettingsService", "Settings file not found, using defaults");
                    return null;
                }

                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<Settings>(json);
                
                Logger.Log(LogLevel.Info, "SettingsService", "Settings loaded successfully");
                return settings;
            }
            catch (Exception ex)
            {
                Logger.LogException(LogLevel.Error, "SettingsService", "Error loading settings", ex);
                return null;
            }
        }

        /// <summary>
        /// Saves settings to the settings file
        /// </summary>
        /// <param name="settings">Settings to save</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SaveSettings(Settings settings)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_settingsFilePath, json);
                
                // Update current settings
                _currentSettings = settings;
                
                Logger.Log(LogLevel.Info, "SettingsService", "Settings saved successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(LogLevel.Error, "SettingsService", "Error saving settings", ex);
                return false;
            }
        }

        /// <summary>
        /// Updates the hotkey settings
        /// </summary>
        /// <param name="modifiers">Key modifiers (Alt, Ctrl, Shift, Win)</param>
        /// <param name="key">The key</param>
        /// <param name="displayText">Display text for the hotkey</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool UpdateHotkey(int modifiers, int key, string displayText)
        {
            try
            {
                _currentSettings.HotkeyModifiers = modifiers;
                _currentSettings.HotkeyKey = key;
                _currentSettings.HotkeyDisplayText = displayText;
                
                return SaveSettings(_currentSettings);
            }
            catch (Exception ex)
            {
                Logger.LogException(LogLevel.Error, "SettingsService", "Error updating hotkey", ex);
                return false;
            }
        }
    }
}
