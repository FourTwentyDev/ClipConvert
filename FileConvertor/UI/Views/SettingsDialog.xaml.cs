using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using FileConvertor.UI.ViewModels;

namespace FileConvertor.UI.Views
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private readonly SettingsViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the SettingsDialog class
        /// </summary>
        /// <param name="viewModel">ViewModel for the dialog</param>
        public SettingsDialog(SettingsViewModel viewModel)
        {
            try
            {
                // Initialize the component to load XAML content
                InitializeComponent();
                
                // Set the DataContext
                _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
                DataContext = _viewModel;
                
                // Subscribe to the CloseRequested event
                _viewModel.CloseRequested += (s, e) => Hide();

                // Allow dragging the window
                MouseDown += (s, e) =>
                {
                    if (e.ChangedButton == MouseButton.Left)
                        DragMove();
                };
                
                // Handle closing to minimize to tray instead
                Closing += SettingsDialog_Closing;
            }
            catch (Exception ex)
            {
                // Show error message if initialization fails
                System.Windows.MessageBox.Show($"Error initializing dialog: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Handles the window closing event
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void SettingsDialog_Closing(object sender, CancelEventArgs e)
        {
            // Just hide the window instead of closing it
            e.Cancel = true;
            Hide();
        }
        
        /// <summary>
        /// Handles the key down event to capture hotkeys
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Only handle key presses if we're recording a hotkey
            if (_viewModel.IsRecordingHotkey)
            {
                // Pass the key and modifiers to the view model
                _viewModel.HandleKeyPress(e.Key, Keyboard.Modifiers);
                
                // Mark the event as handled
                e.Handled = true;
            }
        }
    }
}
