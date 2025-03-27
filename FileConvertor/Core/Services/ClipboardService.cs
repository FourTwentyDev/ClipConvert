using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using FileConvertor.Core.Interfaces;
using FileConvertor.Core.Logging;
using Microsoft.IO;

namespace FileConvertor.Core.Services
{
    /// <summary>
    /// Service for clipboard operations with optimized memory usage
    /// </summary>
    public class ClipboardService : IClipboardService, IDisposable
    {
        // RecyclableMemoryStreamManager for efficient memory stream management
        private static readonly RecyclableMemoryStreamManager _memoryStreamManager = new RecyclableMemoryStreamManager();
        
        // Cache for temporary files to ensure cleanup
        private readonly List<string> _tempFiles = new List<string>();
        private bool _isDisposed;

        /// <summary>
        /// Checks if the clipboard contains a file
        /// </summary>
        /// <returns>True if the clipboard contains a file, false otherwise</returns>
        public bool ContainsFile()
        {
            // Must be executed on the UI thread
            return WpfApplication.Current.Dispatcher.Invoke<bool>(() =>
            {
                return WpfClipboard.ContainsFileDropList() || WpfClipboard.ContainsData("FileContents");
            });
        }

        /// <summary>
        /// Gets the file path from the clipboard if available
        /// </summary>
        /// <returns>File path or null if no file is in the clipboard</returns>
        public string GetFilePath()
        {
            // Must be executed on the UI thread
            return WpfApplication.Current.Dispatcher.Invoke<string>(() =>
            {
                if (WpfClipboard.ContainsFileDropList())
                {
                    var fileDropList = WpfClipboard.GetFileDropList();
                    if (fileDropList.Count > 0)
                    {
                        return fileDropList[0];
                    }
                }
                return null;
            });
        }

        /// <summary>
        /// Gets a list of file paths from the clipboard if available
        /// </summary>
        /// <returns>List of file paths or empty list if no files are in the clipboard</returns>
        public List<string> GetFilePaths()
        {
            // Must be executed on the UI thread
            return WpfApplication.Current.Dispatcher.Invoke<List<string>>(() =>
            {
                var result = new List<string>();
                if (WpfClipboard.ContainsFileDropList())
                {
                    var fileDropList = WpfClipboard.GetFileDropList();
                    foreach (var file in fileDropList)
                    {
                        result.Add(file);
                    }
                }
                return result;
            });
        }

        /// <summary>
        /// Gets the file data from the clipboard if available
        /// </summary>
        /// <returns>Stream containing the file data or null if no file is in the clipboard</returns>
        public Stream GetFileData()
        {
            // Must be executed on the UI thread
            return WpfApplication.Current.Dispatcher.Invoke<Stream>(() =>
            {
                try
                {
                    if (WpfClipboard.ContainsData("FileContents"))
                    {
                        var fileContents = WpfClipboard.GetData("FileContents") as MemoryStream;
                        if (fileContents != null)
                        {
                            // Create a recyclable memory stream instead of a regular memory stream
                            var recyclableStream = _memoryStreamManager.GetStream();
                            fileContents.CopyTo(recyclableStream);
                            recyclableStream.Position = 0;
                            return recyclableStream;
                        }
                    }
                    else if (WpfClipboard.ContainsFileDropList())
                    {
                        var fileDropList = WpfClipboard.GetFileDropList();
                        if (fileDropList.Count > 0)
                        {
                            try
                            {
                                // Use a buffer to read the file in chunks instead of keeping the file open
                                var filePath = fileDropList[0];
                                var fileInfo = new System.IO.FileInfo(filePath);
                                
                                if (fileInfo.Exists)
                                {
                                    var recyclableStream = _memoryStreamManager.GetStream();
                                    
                                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
                                    {
                                        fileStream.CopyTo(recyclableStream);
                                    }
                                    
                                    recyclableStream.Position = 0;
                                    return recyclableStream;
                                }
                            }
                            catch (IOException ex)
                            {
                                // Handle network path access or authentication issues
                                Logger.LogException(LogLevel.Warning, "ClipboardService", "Could not access file", ex);
                                return null;
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                // Handle permission issues
                                Logger.LogException(LogLevel.Warning, "ClipboardService", "Unauthorized access to file", ex);
                                return null;
                            }
                            catch (Exception ex)
                            {
                                // Handle any other exceptions that might occur
                                Logger.LogException(LogLevel.Warning, "ClipboardService", "Error accessing file", ex);
                                return null;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(LogLevel.Error, "ClipboardService", "Error getting file data from clipboard", ex);
                }
                
                return null;
            });
        }

        /// <summary>
        /// Sets the file data to the clipboard
        /// </summary>
        /// <param name="fileData">Stream containing the file data</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileType">MIME type of the file</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public Task SetFileDataAsync(Stream fileData, string fileName, string fileType)
        {
            if (fileData == null)
                throw new ArgumentNullException(nameof(fileData));
            
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            // Create a temporary file to store the data
            string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);
            
            // Add to the list of temp files to clean up later
            lock (_tempFiles)
            {
                _tempFiles.Add(tempFilePath);
            }
            
            return Task.Run(() =>
            {
                try
                {
                    Logger.Log(LogLevel.Debug, "ClipboardService", $"Creating temporary file: {tempFilePath}");
                    
                    // Save the data to a temporary file
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
                    {
                        fileData.CopyTo(fileStream);
                    }

                    // Set the file to the clipboard
                    WpfApplication.Current.Dispatcher.Invoke(() =>
                    {
                        var fileCollection = new System.Collections.Specialized.StringCollection();
                        fileCollection.Add(tempFilePath);
                        WpfClipboard.SetFileDropList(fileCollection);
                    });
                    
                    Logger.Log(LogLevel.Debug, "ClipboardService", "File data set to clipboard successfully");
                }
                catch (IOException ex)
                {
                    // Handle file I/O errors
                    Logger.LogException(LogLevel.Error, "ClipboardService", "Error writing to temporary file", ex);
                    throw new InvalidOperationException($"Could not create temporary file: {ex.Message}", ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Handle permission issues
                    Logger.LogException(LogLevel.Error, "ClipboardService", "Permission denied when writing to temporary file", ex);
                    throw new InvalidOperationException($"Permission denied for temporary file: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    // Handle any other exceptions
                    Logger.LogException(LogLevel.Error, "ClipboardService", "Error setting file data to clipboard", ex);
                    throw;
                }
            });
        }
        
        /// <summary>
        /// Cleans up temporary files created by the clipboard service
        /// </summary>
        private void CleanupTempFiles()
        {
            lock (_tempFiles)
            {
                foreach (var tempFile in _tempFiles)
                {
                    try
                    {
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                            Logger.Log(LogLevel.Debug, "ClipboardService", $"Deleted temporary file: {tempFile}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Just log the error but don't throw
                        Logger.LogException(LogLevel.Warning, "ClipboardService", $"Error deleting temporary file: {tempFile}", ex);
                    }
                }
                
                _tempFiles.Clear();
            }
        }
        
        /// <summary>
        /// Disposes resources used by the clipboard service
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Disposes resources used by the clipboard service
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;
                
            if (disposing)
            {
                // Clean up temporary files
                CleanupTempFiles();
            }
            
            _isDisposed = true;
        }
        
        /// <summary>
        /// Finalizer
        /// </summary>
        ~ClipboardService()
        {
            Dispose(false);
        }
    }
}
