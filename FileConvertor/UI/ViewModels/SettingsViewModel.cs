using System;
using System.Collections.Generic;
using System.Windows.Input;
using FileConvertor.Core.Interfaces;
using FileConvertor.Core.Logging;
using FileConvertor.Core.Services;
using FileConvertor.Models;
using FileConvertor.UI.Commands;

namespace FileConvertor.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the settings dialog
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IHotkeyService? _hotkeyService;

        private string _hotkeyDisplayText;
        private bool _isRecordingHotkey;
        private bool _isCtrlModifierActive;
        private bool _isAltModifierActive;
        private bool _isShiftModifierActive;
        private bool _isWinModifierActive;
        private string _statusMessage = string.Empty;
        private bool _isSuccess;
        private bool _isError;

        /// <summary>
        /// Gets or sets the hotkey display text
        /// </summary>
        public string HotkeyDisplayText
        {
            get => _hotkeyDisplayText;
            set => SetProperty(ref _hotkeyDisplayText, value);
        }

        /// <summary>
        /// Gets or sets whether the hotkey recording is active
        /// </summary>
        public bool IsRecordingHotkey
        {
            get => _isRecordingHotkey;
            set
            {
                if (SetProperty(ref _isRecordingHotkey, value))
                {
                    // Update command states
                    RecordHotkeyCommand.RaiseCanExecuteChanged();
                    SaveCommand.RaiseCanExecuteChanged();
                    
                    // Update status message
                    if (value)
                    {
                        StatusMessage = "Press a key combination...";
                    }
                    else
                    {
                        StatusMessage = string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the Ctrl modifier is active
        /// </summary>
        public bool IsCtrlModifierActive
        {
            get => _isCtrlModifierActive;
            set
            {
                if (SetProperty(ref _isCtrlModifierActive, value))
                {
                    UpdateHotkeyDisplayText();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the Alt modifier is active
        /// </summary>
        public bool IsAltModifierActive
        {
            get => _isAltModifierActive;
            set
            {
                if (SetProperty(ref _isAltModifierActive, value))
                {
                    UpdateHotkeyDisplayText();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the Shift modifier is active
        /// </summary>
        public bool IsShiftModifierActive
        {
            get => _isShiftModifierActive;
            set
            {
                if (SetProperty(ref _isShiftModifierActive, value))
                {
                    UpdateHotkeyDisplayText();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the Win modifier is active
        /// </summary>
        public bool IsWinModifierActive
        {
            get => _isWinModifierActive;
            set
            {
                if (SetProperty(ref _isWinModifierActive, value))
                {
                    UpdateHotkeyDisplayText();
                }
            }
        }

        /// <summary>
        /// Gets or sets the status message
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Gets or sets whether the operation was successful
        /// </summary>
        public bool IsSuccess
        {
            get => _isSuccess;
            set => SetProperty(ref _isSuccess, value);
        }

        /// <summary>
        /// Gets or sets whether the operation failed
        /// </summary>
        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }

        /// <summary>
        /// Gets the command to record a hotkey
        /// </summary>
        public RelayCommand RecordHotkeyCommand { get; }

        /// <summary>
        /// Gets the command to save settings
        /// </summary>
        public RelayCommand SaveCommand { get; }

        /// <summary>
        /// Gets the command to close the dialog
        /// </summary>
        public RelayCommand CloseCommand { get; }

        /// <summary>
        /// Gets the command to reset settings to defaults
        /// </summary>
        public RelayCommand ResetCommand { get; }

        /// <summary>
        /// Initializes a new instance of the SettingsViewModel class
        /// </summary>
        /// <param name="settingsService">Settings service</param>
        /// <param name="hotkeyService">Hotkey service (optional)</param>
        public SettingsViewModel(ISettingsService settingsService, IHotkeyService? hotkeyService = null)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _hotkeyService = hotkeyService;
            
            // Initialize from current settings
            var settings = _settingsService.CurrentSettings;
            _hotkeyDisplayText = settings.HotkeyDisplayText;
            
            // Initialize modifier states
            _isCtrlModifierActive = (settings.HotkeyModifiers & HotkeyService.MOD_CONTROL) != 0;
            _isAltModifierActive = (settings.HotkeyModifiers & HotkeyService.MOD_ALT) != 0;
            _isShiftModifierActive = (settings.HotkeyModifiers & HotkeyService.MOD_SHIFT) != 0;
            _isWinModifierActive = (settings.HotkeyModifiers & HotkeyService.MOD_WIN) != 0;
            
            // Initialize commands
            RecordHotkeyCommand = new RelayCommand(_ => StartRecordingHotkey(), _ => !IsRecordingHotkey);
            SaveCommand = new RelayCommand(_ => SaveSettings(), _ => !IsRecordingHotkey);
            CloseCommand = new RelayCommand(_ => CloseDialog());
            ResetCommand = new RelayCommand(_ => ResetToDefaults());
        }

        /// <summary>
        /// Starts recording a hotkey
        /// </summary>
        private void StartRecordingHotkey()
        {
            IsRecordingHotkey = true;
            StatusMessage = "Press a key combination...";
            IsSuccess = false;
            IsError = false;
        }

        /// <summary>
        /// Handles a key press during hotkey recording
        /// </summary>
        /// <param name="key">The key that was pressed</param>
        /// <param name="modifiers">The modifiers that were active</param>
        public void HandleKeyPress(Key key, ModifierKeys modifiers)
        {
            if (!IsRecordingHotkey)
                return;
                
            // Ignore modifier keys when pressed alone
            if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LWin || key == Key.RWin)
            {
                return;
            }
            
            // Update modifier states
            IsCtrlModifierActive = (modifiers & ModifierKeys.Control) != 0;
            IsAltModifierActive = (modifiers & ModifierKeys.Alt) != 0;
            IsShiftModifierActive = (modifiers & ModifierKeys.Shift) != 0;
            IsWinModifierActive = (modifiers & ModifierKeys.Windows) != 0;
            
            // Build the display text
            var displayParts = new List<string>();
            if (IsCtrlModifierActive) displayParts.Add("Ctrl");
            if (IsAltModifierActive) displayParts.Add("Alt");
            if (IsShiftModifierActive) displayParts.Add("Shift");
            if (IsWinModifierActive) displayParts.Add("Win");
            
            // Add the key
            var keyText = key.ToString();
            displayParts.Add(keyText);
            
            // Update the display text
            HotkeyDisplayText = string.Join("+", displayParts);
            
            // Stop recording
            IsRecordingHotkey = false;
            StatusMessage = $"Hotkey set to {HotkeyDisplayText}";
        }

        /// <summary>
        /// Updates the hotkey display text based on the active modifiers
        /// </summary>
        private void UpdateHotkeyDisplayText()
        {
            // Only update if we're not recording
            if (IsRecordingHotkey)
                return;
                
            // Get the current key part (after the last +)
            var currentKey = string.Empty;
            var lastPlusIndex = HotkeyDisplayText.LastIndexOf('+');
            if (lastPlusIndex >= 0 && lastPlusIndex < HotkeyDisplayText.Length - 1)
            {
                currentKey = HotkeyDisplayText.Substring(lastPlusIndex + 1);
            }
            
            // Build the display text
            var displayParts = new List<string>();
            if (IsCtrlModifierActive) displayParts.Add("Ctrl");
            if (IsAltModifierActive) displayParts.Add("Alt");
            if (IsShiftModifierActive) displayParts.Add("Shift");
            if (IsWinModifierActive) displayParts.Add("Win");
            
            // Add the key if we have one
            if (!string.IsNullOrEmpty(currentKey))
            {
                displayParts.Add(currentKey);
            }
            
            // Update the display text
            HotkeyDisplayText = string.Join("+", displayParts);
        }

        /// <summary>
        /// Saves the settings
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                // Validate that we have at least one modifier and a key
                if (!IsCtrlModifierActive && !IsAltModifierActive && !IsShiftModifierActive && !IsWinModifierActive)
                {
                    StatusMessage = "At least one modifier key (Ctrl, Alt, Shift, Win) is required";
                    IsError = true;
                    return;
                }
                
                // Get the key part (after the last +)
                var keyText = string.Empty;
                var lastPlusIndex = HotkeyDisplayText.LastIndexOf('+');
                if (lastPlusIndex >= 0 && lastPlusIndex < HotkeyDisplayText.Length - 1)
                {
                    keyText = HotkeyDisplayText.Substring(lastPlusIndex + 1);
                }
                
                if (string.IsNullOrEmpty(keyText))
                {
                    StatusMessage = "A key is required";
                    IsError = true;
                    return;
                }
                
                // Parse the key
                if (!Enum.TryParse<Key>(keyText, out var key))
                {
                    StatusMessage = $"Invalid key: {keyText}";
                    IsError = true;
                    return;
                }
                
                // Calculate the modifiers
                int modifiers = 0;
                if (IsCtrlModifierActive) modifiers |= HotkeyService.MOD_CONTROL;
                if (IsAltModifierActive) modifiers |= HotkeyService.MOD_ALT;
                if (IsShiftModifierActive) modifiers |= HotkeyService.MOD_SHIFT;
                if (IsWinModifierActive) modifiers |= HotkeyService.MOD_WIN;
                
                // Convert the key to a virtual key code
                int keyCode = KeyInterop.VirtualKeyFromKey(key);
                
                // Save the settings
                if (_settingsService.UpdateHotkey(modifiers, keyCode, HotkeyDisplayText))
                {
                    StatusMessage = "Settings saved successfully";
                    IsSuccess = true;
                    IsError = false;
                    
                    Logger.Log(LogLevel.Info, "SettingsViewModel", $"Hotkey updated to {HotkeyDisplayText}");
                }
                else
                {
                    StatusMessage = "Failed to save settings";
                    IsSuccess = false;
                    IsError = true;
                    
                    Logger.Log(LogLevel.Error, "SettingsViewModel", "Failed to save settings");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                IsSuccess = false;
                IsError = true;
                
                Logger.LogException(LogLevel.Error, "SettingsViewModel", "Error saving settings", ex);
            }
        }

        /// <summary>
        /// Closes the dialog
        /// </summary>
        private void CloseDialog()
        {
            // Raise an event that the view can handle to close the dialog
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Event that is raised when the dialog should be closed
        /// </summary>
        public event EventHandler? CloseRequested;

        /// <summary>
        /// Resets settings to defaults
        /// </summary>
        private void ResetToDefaults()
        {
            try
            {
                // Create default settings
                var defaultSettings = Settings.CreateDefault();
                
                // Update the UI
                HotkeyDisplayText = defaultSettings.HotkeyDisplayText;
                IsCtrlModifierActive = (defaultSettings.HotkeyModifiers & HotkeyService.MOD_CONTROL) != 0;
                IsAltModifierActive = (defaultSettings.HotkeyModifiers & HotkeyService.MOD_ALT) != 0;
                IsShiftModifierActive = (defaultSettings.HotkeyModifiers & HotkeyService.MOD_SHIFT) != 0;
                IsWinModifierActive = (defaultSettings.HotkeyModifiers & HotkeyService.MOD_WIN) != 0;
                
                // Save the settings
                if (_settingsService.SaveSettings(defaultSettings))
                {
                    StatusMessage = "Settings reset to defaults";
                    IsSuccess = true;
                    IsError = false;
                    
                    Logger.Log(LogLevel.Info, "SettingsViewModel", "Settings reset to defaults");
                }
                else
                {
                    StatusMessage = "Failed to reset settings";
                    IsSuccess = false;
                    IsError = true;
                    
                    Logger.Log(LogLevel.Error, "SettingsViewModel", "Failed to reset settings");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                IsSuccess = false;
                IsError = true;
                
                Logger.LogException(LogLevel.Error, "SettingsViewModel", "Error resetting settings", ex);
            }
        }
    }
}
