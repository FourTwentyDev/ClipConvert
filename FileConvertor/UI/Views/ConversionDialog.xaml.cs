using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using FileConvertor.UI.ViewModels;

namespace FileConvertor.UI.Views
{
    /// <summary>
    /// Interaction logic for ConversionDialog.xaml
    /// </summary>
    public partial class ConversionDialog : Window
    {
        /// <summary>
        /// Initializes a new instance of the ConversionDialog class
        /// </summary>
        /// <param name="viewModel">ViewModel for the dialog</param>
        public ConversionDialog(ConversionViewModel viewModel)
        {
            try
            {
                // Initialize the component to load XAML content
                InitializeComponent();
                
                // Set the DataContext
                DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

                // Allow dragging the window
                MouseDown += (s, e) =>
                {
                    if (e.ChangedButton == MouseButton.Left)
                        DragMove();
                };
                
                // Handle closing to minimize to tray instead
                Closing += ConversionDialog_Closing;
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
        private void ConversionDialog_Closing(object sender, CancelEventArgs e)
        {
            // Cancel the close and minimize to taskbar instead
            e.Cancel = true;
            WindowState = WindowState.Minimized;
        }
    }
}
