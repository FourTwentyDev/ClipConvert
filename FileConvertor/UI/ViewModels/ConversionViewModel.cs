using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using FileConvertor.Core;
using FileConvertor.Core.Helpers;
using FileConvertor.Core.Interfaces;
using FileConvertor.Core.Logging;
using FileConvertor.Models;
using FileConvertor.UI.Commands;

namespace FileConvertor.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the conversion dialog
    /// </summary>
    public class ConversionViewModel : ViewModelBase
    {
        private readonly IClipboardService _clipboardService;
        private readonly IFileTypeDetector _fileTypeDetector;
        private readonly ConverterFactory _converterFactory;
        private readonly ConversionContext _conversionContext;
        private readonly DispatcherTimer _clipboardMonitorTimer;
        private readonly DispatcherTimer _successMessageTimer;
        
        // Flag to prevent processing the converted file that was just placed in the clipboard
        private bool _ignoreNextClipboardCheck = false;

        private Models.FileInfo? _selectedFile;
        private string? _selectedTargetFormat;
        private bool _isConverting;
        private bool _isSuccess;
        private bool _isError;
        private string _statusMessage = string.Empty;
        private string _formattedFileSize = string.Empty;
        private string _fileIconKind = "File";

        /// <summary>
        /// Gets or sets the selected file
        /// </summary>
        public Models.FileInfo? SelectedFile
        {
            get => _selectedFile;
            set
            {
                if (SetProperty(ref _selectedFile, value))
                {
                    UpdateAvailableTargetFormats();
                    UpdateFormattedFileSize();
                    UpdateFileIcon();
                    ConvertCommand.RaiseCanExecuteChanged();
                    
                    // Reset status messages when a new file is selected
                    if (value != null)
                    {
                        IsSuccess = false;
                        IsError = false;
                        StatusMessage = "Ready to convert";
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets the formatted file size with appropriate units (B, KB, MB, GB)
        /// </summary>
        public string FormattedFileSize
        {
            get => _formattedFileSize;
            private set => SetProperty(ref _formattedFileSize, value);
        }
        
        /// <summary>
        /// Gets the Material Design icon kind based on file type
        /// </summary>
        public string FileIconKind
        {
            get => _fileIconKind;
            private set => SetProperty(ref _fileIconKind, value);
        }

        /// <summary>
        /// Gets or sets the selected target format
        /// </summary>
        public string? SelectedTargetFormat
        {
            get => _selectedTargetFormat;
            set
            {
                if (SetProperty(ref _selectedTargetFormat, value))
                {
                    ConvertCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether a conversion is in progress
        /// </summary>
        public bool IsConverting
        {
            get => _isConverting;
            set
            {
                if (SetProperty(ref _isConverting, value))
                {
                    OnPropertyChanged(nameof(IsIdle));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the conversion was successful
        /// </summary>
        public bool IsSuccess
        {
            get => _isSuccess;
            set
            {
                if (SetProperty(ref _isSuccess, value))
                {
                    OnPropertyChanged(nameof(IsIdle));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the conversion failed
        /// </summary>
        public bool IsError
        {
            get => _isError;
            set
            {
                if (SetProperty(ref _isError, value))
                {
                    OnPropertyChanged(nameof(IsIdle));
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
        /// Gets whether the converter is in an idle state (not converting, no success, no error)
        /// </summary>
        public bool IsIdle => !IsConverting && !IsSuccess && !IsError;

        /// <summary>
        /// Gets the available target formats
        /// </summary>
        public ObservableCollection<string> AvailableTargetFormats { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets the command to convert the selected file
        /// </summary>
        public RelayCommand ConvertCommand { get; }

        /// <summary>
        /// Gets the command to close the dialog
        /// </summary>
        public RelayCommand CloseCommand { get; }

        /// <summary>
        /// Initializes a new instance of the ConversionViewModel class
        /// </summary>
        /// <param name="clipboardService">Clipboard service</param>
        /// <param name="fileTypeDetector">File type detector</param>
        /// <param name="converterFactory">Converter factory</param>
        public ConversionViewModel(
            IClipboardService clipboardService,
            IFileTypeDetector fileTypeDetector,
            ConverterFactory converterFactory)
        {
            _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
            _fileTypeDetector = fileTypeDetector ?? throw new ArgumentNullException(nameof(fileTypeDetector));
            _converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));
            _conversionContext = new ConversionContext(converterFactory);

            ConvertCommand = new RelayCommand(async _ => await ConvertAsync(), _ => CanConvert());
            // Command to minimize the window
            CloseCommand = new RelayCommand(_ => MinimizeWindow());

            // Set up a timer to periodically check the clipboard for new files
            _clipboardMonitorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _clipboardMonitorTimer.Tick += (s, e) => CheckClipboardForNewFile();
            _clipboardMonitorTimer.Start();
            
            // Set up a timer for the success message display
            _successMessageTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _successMessageTimer.Tick += (s, e) => 
            {
                _successMessageTimer.Stop();
                MinimizeWindow();
            };

            // Try to get a file from the clipboard
            RefreshFromClipboard();
            
            // Set initial status message
            if (SelectedFile == null)
            {
                StatusMessage = "Copy a file to get started";
            }
            else
            {
                StatusMessage = "Ready to convert";
            }
        }

        /// <summary>
        /// Refreshes the selected file from the clipboard
        /// </summary>
        public void RefreshFromClipboard()
        {
            // Clear current selection
            SelectedFile = null;
            
            // Try to get a file from the clipboard
            if (_clipboardService.ContainsFile())
            {
                var filePath = _clipboardService.GetFilePath();
                if (!string.IsNullOrEmpty(filePath))
                {
                    SelectedFile = Models.FileInfo.FromPath(filePath);
                }
            }
        }
        
        /// <summary>
        /// Checks the clipboard for a new file and updates if different from current selection
        /// </summary>
        private void CheckClipboardForNewFile()
        {
            // Skip this check if we're ignoring the next clipboard check
            if (_ignoreNextClipboardCheck)
            {
                _ignoreNextClipboardCheck = false;
                return;
            }
            
            if (!_clipboardService.ContainsFile())
                return;
                
            var filePath = _clipboardService.GetFilePath();
            if (string.IsNullOrEmpty(filePath))
                return;
                
            // Only update if the file is different from the current selection
            if (SelectedFile == null || !string.Equals(SelectedFile.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
            {
                SelectedFile = Models.FileInfo.FromPath(filePath);
            }
        }

        /// <summary>
        /// Updates the formatted file size with appropriate units (B, KB, MB, GB)
        /// </summary>
        private void UpdateFormattedFileSize()
        {
            if (SelectedFile == null)
            {
                FormattedFileSize = string.Empty;
                return;
            }

            long bytes = SelectedFile.FileSize;
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double size = bytes;
            
            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                suffixIndex++;
                size /= 1024;
            }
            
            FormattedFileSize = $"{size:0.##} {suffixes[suffixIndex]}";
        }
        
        /// <summary>
        /// Updates the file icon based on the file type
        /// </summary>
        private void UpdateFileIcon()
        {
            if (SelectedFile == null || string.IsNullOrEmpty(SelectedFile.FileExtension))
            {
                FileIconKind = "File";
                return;
            }

            // Map common file extensions to Material Design icons
            // This can be expanded with more file types as needed
            FileIconKind = SelectedFile.FileExtension.ToLowerInvariant() switch
            {
                "pdf" => "FilePdf",
                "doc" or "docx" => "FileWord",
                "xls" or "xlsx" => "FileExcel",
                "ppt" or "pptx" => "FilePowerpoint",
                "txt" => "FileDocument",
                "jpg" or "jpeg" or "png" or "gif" or "bmp" => "FileImage",
                "mp3" or "wav" or "ogg" => "FileMusic",
                "mp4" or "avi" or "mov" or "wmv" => "FileVideo",
                "zip" or "rar" or "7z" => "FileZip",
                "html" or "htm" => "FileCode",
                "cs" or "js" or "ts" or "py" or "java" => "CodeBraces",
                "md" => "LanguageMarkdown",
                "csv" => "FileCsv",
                "json" => "CodeJson",
                "xml" => "FileXml",
                _ => "File"
            };
        }

        /// <summary>
        /// Updates the available target formats based on the selected file
        /// </summary>
        private void UpdateAvailableTargetFormats()
        {
            AvailableTargetFormats.Clear();

            if (SelectedFile == null || string.IsNullOrEmpty(SelectedFile.FileFormat))
                return;

            var targetFormats = _fileTypeDetector.GetSupportedTargetFormats(SelectedFile.FileFormat);
            foreach (var format in targetFormats)
            {
                AvailableTargetFormats.Add(format);
            }

            if (AvailableTargetFormats.Count > 0)
            {
                SelectedTargetFormat = AvailableTargetFormats[0];
            }
        }

        /// <summary>
        /// Determines if the selected file can be converted
        /// </summary>
        /// <returns>True if the file can be converted, false otherwise</returns>
        private bool CanConvert()
        {
            return SelectedFile != null &&
                   !string.IsNullOrEmpty(SelectedTargetFormat) &&
                   !IsConverting;
        }

        /// <summary>
        /// Converts the selected file
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task ConvertAsync()
        {
            if (SelectedFile == null || string.IsNullOrEmpty(SelectedTargetFormat))
                return;

            Logger.Log(LogLevel.Info, "ConversionViewModel", $"Starting conversion from {SelectedFile.FileFormat} to {SelectedTargetFormat}");
            Logger.Log(LogLevel.Debug, "ConversionViewModel", $"File: {SelectedFile.FilePath}, Size: {SelectedFile.FileSize} bytes");

            IsConverting = true;
            IsSuccess = false;
            IsError = false;
            StatusMessage = "Converting...";

            try
            {
                // Set the appropriate converter using lazy loading
                if (!_conversionContext.SetConverter(SelectedFile.FileFormat, SelectedTargetFormat))
                {
                    var errorMessage = $"No converter found for {SelectedFile.FileFormat} to {SelectedTargetFormat}";
                    Logger.Log(LogLevel.Error, "ConversionViewModel", errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }

                Logger.Log(LogLevel.Debug, "ConversionViewModel", $"Using converter: {_conversionContext.Converter.GetType().Name}");

                // Create a memory stream to hold the converted data
                // Don't use 'using' here as we need the stream to remain open for clipboard operations
                var outputStream = new MemoryStream();
                Logger.Log(LogLevel.Debug, "ConversionViewModel", "Created output memory stream");

                // Convert the file
                if (SelectedFile.FileData != null)
                {
                    // Convert from stream
                    Logger.Log(LogLevel.Debug, "ConversionViewModel", "Converting from file data stream");
                    SelectedFile.FileData.Position = 0;
                    await _conversionContext.ConvertAsync(SelectedFile.FileData, outputStream);
                    Logger.Log(LogLevel.Debug, "ConversionViewModel", "Conversion from stream completed");
                }
                else if (!string.IsNullOrEmpty(SelectedFile.FilePath))
                {
                    Logger.Log(LogLevel.Debug, "ConversionViewModel", $"Converting from file path: {SelectedFile.FilePath}");
                    try
                    {
                        // Convert from file
                        using var inputStream = new FileStream(SelectedFile.FilePath, FileMode.Open, FileAccess.Read);
                        Logger.Log(LogLevel.Debug, "ConversionViewModel", $"File stream opened: CanRead={inputStream.CanRead}, Length={inputStream.Length}");
                        
                        await _conversionContext.ConvertAsync(inputStream, outputStream);
                        Logger.Log(LogLevel.Debug, "ConversionViewModel", "Conversion from file completed");
                    }
                    catch (IOException ex)
                    {
                        // Handle network path access or authentication issues
                        Logger.LogException(LogLevel.Error, "ConversionViewModel", "Error accessing file", ex);
                        throw new InvalidOperationException($"Cannot access file: {ex.Message}", ex);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        // Handle permission issues
                        Logger.LogException(LogLevel.Error, "ConversionViewModel", "Unauthorized access to file", ex);
                        throw new InvalidOperationException($"Permission denied: {ex.Message}", ex);
                    }
                }
                else
                {
                    var errorMessage = "No file data or file path available";
                    Logger.Log(LogLevel.Error, "ConversionViewModel", errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }

                // Reset the stream position
                outputStream.Position = 0;
                Logger.Log(LogLevel.Debug, "ConversionViewModel", $"Output stream size: {outputStream.Length} bytes");

                // Create a new file name with the target extension
                string newFileName = $"{SelectedFile.FileName}.{SelectedTargetFormat}";
                Logger.Log(LogLevel.Debug, "ConversionViewModel", $"New file name: {newFileName}");

                // Copy to clipboard
                Logger.Log(LogLevel.Debug, "ConversionViewModel", "Copying to clipboard");
                try
                {
                    // Set flag to ignore the next clipboard check since we're putting our own file there
                    _ignoreNextClipboardCheck = true;
                    
                    await _clipboardService.SetFileDataAsync(outputStream, newFileName, SelectedTargetFormat);
                    Logger.Log(LogLevel.Debug, "ConversionViewModel", "Copied to clipboard successfully");
                }
                finally
                {
                    // Ensure the stream is disposed after clipboard operation
                    outputStream.Dispose();
                    Logger.Log(LogLevel.Debug, "ConversionViewModel", "Output stream disposed");
                }

                // Ensure success state is set correctly
                StatusMessage = $"Successfully converted to {SelectedTargetFormat.ToUpper()} (available in clipboard)";
                IsSuccess = true;
                
                // Start the timer to show success message for 2 seconds before minimizing
                _successMessageTimer.Start();
                
                Logger.Log(LogLevel.Info, "ConversionViewModel", $"Conversion from {SelectedFile.FileFormat} to {SelectedTargetFormat} completed successfully");
            }
            catch (Exception ex)
            {
                IsError = true;
                StatusMessage = $"Conversion failed: {ex.Message}";
                Logger.LogException(LogLevel.Error, "ConversionViewModel", "Conversion failed", ex);
            }
            finally
            {
                IsConverting = false;
                Logger.Log(LogLevel.Debug, "ConversionViewModel", $"Conversion state: IsSuccess={IsSuccess}, IsError={IsError}, IsConverting={IsConverting}");
            }
        }
        
        /// <summary>
        /// Minimizes the application window
        /// </summary>
        private void MinimizeWindow()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (System.Windows.Application.Current.MainWindow != null)
                {
                    System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
                }
            });
        }
    }
}
