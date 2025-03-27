using System;

namespace FileConvertor.Core.Interfaces
{
    /// <summary>
    /// Interface for global hotkey operations
    /// </summary>
    public interface IHotkeyService
    {
        /// <summary>
        /// Registers a global hotkey
        /// </summary>
        /// <param name="modifiers">Key modifiers (Alt, Ctrl, Shift, Win)</param>
        /// <param name="key">The key to register</param>
        /// <param name="callback">Callback to invoke when the hotkey is pressed</param>
        /// <returns>True if registration was successful, false otherwise</returns>
        bool RegisterHotkey(int modifiers, int key, Action callback);

        /// <summary>
        /// Unregisters a previously registered global hotkey
        /// </summary>
        /// <param name="modifiers">Key modifiers (Alt, Ctrl, Shift, Win)</param>
        /// <param name="key">The key to unregister</param>
        /// <returns>True if unregistration was successful, false otherwise</returns>
        bool UnregisterHotkey(int modifiers, int key);

        /// <summary>
        /// Starts listening for registered hotkeys
        /// </summary>
        void StartListening();

        /// <summary>
        /// Stops listening for registered hotkeys
        /// </summary>
        void StopListening();

        /// <summary>
        /// Disposes resources used by the hotkey service
        /// </summary>
        void Dispose();
    }
}
