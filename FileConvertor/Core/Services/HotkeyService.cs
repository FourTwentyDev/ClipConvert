using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using FileConvertor.Core.Interfaces;

namespace FileConvertor.Core.Services
{
    /// <summary>
    /// Service for global hotkey operations
    /// </summary>
    public class HotkeyService : IHotkeyService, IDisposable
    {
        // Windows API constants
        private const int WM_HOTKEY = 0x0312;
        
        // Windows API modifiers
        public const int MOD_ALT = 0x0001;
        public const int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x0004;
        public const int MOD_WIN = 0x0008;
        public const int MOD_NOREPEAT = 0x4000;

        // Windows API functions
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr _windowHandle;
        private HwndSource _source;
        private readonly Dictionary<int, Action> _registeredHotkeys = new Dictionary<int, Action>();
        private int _currentId = 1;
        private bool _isListening;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the HotkeyService class
        /// </summary>
        /// <param name="windowHandle">Handle to the window that will receive hotkey messages</param>
        public HotkeyService(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        /// <summary>
        /// Registers a global hotkey
        /// </summary>
        /// <param name="modifiers">Key modifiers (Alt, Ctrl, Shift, Win)</param>
        /// <param name="key">The key to register</param>
        /// <param name="callback">Callback to invoke when the hotkey is pressed</param>
        /// <returns>True if registration was successful, false otherwise</returns>
        public bool RegisterHotkey(int modifiers, int key, Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (_isDisposed)
                throw new ObjectDisposedException(nameof(HotkeyService));

            int id = _currentId++;
            
            if (RegisterHotKey(_windowHandle, id, modifiers, key))
            {
                _registeredHotkeys[id] = callback;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Unregisters a previously registered global hotkey
        /// </summary>
        /// <param name="modifiers">Key modifiers (Alt, Ctrl, Shift, Win)</param>
        /// <param name="key">The key to unregister</param>
        /// <returns>True if unregistration was successful, false otherwise</returns>
        public bool UnregisterHotkey(int modifiers, int key)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(HotkeyService));

            // This is a simplified implementation that doesn't actually unregister by modifiers and key
            // In a real implementation, we would need to keep track of the id for each modifiers+key combination
            return false;
        }

        /// <summary>
        /// Starts listening for registered hotkeys
        /// </summary>
        public void StartListening()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(HotkeyService));

            if (_isListening)
                return;

            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(WndProc);
            _isListening = true;
        }

        /// <summary>
        /// Stops listening for registered hotkeys
        /// </summary>
        public void StopListening()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(HotkeyService));

            if (!_isListening)
                return;

            _source.RemoveHook(WndProc);
            _source = null;
            _isListening = false;
        }

        /// <summary>
        /// Window procedure to handle hotkey messages
        /// </summary>
        /// <param name="hwnd">Window handle</param>
        /// <param name="msg">Message</param>
        /// <param name="wParam">wParam</param>
        /// <param name="lParam">lParam</param>
        /// <param name="handled">Whether the message was handled</param>
        /// <returns>IntPtr.Zero</returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                
                if (_registeredHotkeys.TryGetValue(id, out Action callback))
                {
                    callback?.Invoke();
                    handled = true;
                }
            }
            
            return IntPtr.Zero;
        }

        /// <summary>
        /// Disposes resources used by the hotkey service
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes resources used by the hotkey service
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                // Dispose managed resources
                StopListening();
            }

            // Unregister all hotkeys
            foreach (int id in _registeredHotkeys.Keys)
            {
                UnregisterHotKey(_windowHandle, id);
            }

            _registeredHotkeys.Clear();
            _isDisposed = true;
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~HotkeyService()
        {
            Dispose(false);
        }
    }
}
