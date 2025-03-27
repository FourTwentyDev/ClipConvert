using System;
using System.IO;

namespace FileConvertor.Models
{
    /// <summary>
    /// Model class for file information
    /// </summary>
    public class FileInfo
    {
        /// <summary>
        /// Gets or sets the file path
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file name
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file extension
        /// </summary>
        public string FileExtension { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file size in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the file format
        /// </summary>
        public string FileFormat { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file data
        /// </summary>
        public Stream? FileData { get; set; }

        /// <summary>
        /// Creates a new instance of the FileInfo class from a file path
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>FileInfo instance</returns>
        public static FileInfo FromPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            try
            {
                var fileInfo = new System.IO.FileInfo(filePath);
                
                return new FileInfo
                {
                    FilePath = filePath,
                    FileName = Path.GetFileNameWithoutExtension(filePath),
                    FileExtension = Path.GetExtension(filePath).TrimStart('.'),
                    FileSize = fileInfo.Length,
                    FileFormat = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant()
                };
            }
            catch (IOException ex)
            {
                // Handle network path access or authentication issues gracefully
                // Log the exception if logging is available
                System.Diagnostics.Debug.WriteLine($"Warning: Could not access file: {ex.Message}");
                
                // Return a FileInfo object with the information we can extract from the path
                return new FileInfo
                {
                    FilePath = filePath,
                    FileName = Path.GetFileNameWithoutExtension(filePath),
                    FileExtension = Path.GetExtension(filePath).TrimStart('.'),
                    FileSize = -1, // Indicate that file size is unknown
                    FileFormat = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant()
                };
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle permission issues
                System.Diagnostics.Debug.WriteLine($"Warning: Unauthorized access to file: {ex.Message}");
                
                return new FileInfo
                {
                    FilePath = filePath,
                    FileName = Path.GetFileNameWithoutExtension(filePath),
                    FileExtension = Path.GetExtension(filePath).TrimStart('.'),
                    FileSize = -1,
                    FileFormat = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant()
                };
            }
            catch (Exception ex)
            {
                // Handle any other exceptions that might occur
                System.Diagnostics.Debug.WriteLine($"Warning: Error accessing file: {ex.Message}");
                
                return new FileInfo
                {
                    FilePath = filePath,
                    FileName = Path.GetFileNameWithoutExtension(filePath),
                    FileExtension = Path.GetExtension(filePath).TrimStart('.'),
                    FileSize = -1,
                    FileFormat = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant()
                };
            }
        }

        /// <summary>
        /// Creates a new instance of the FileInfo class from a stream
        /// </summary>
        /// <param name="stream">Stream containing the file data</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileFormat">Format of the file</param>
        /// <returns>FileInfo instance</returns>
        public static FileInfo FromStream(Stream stream, string fileName, string fileFormat)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            
            if (string.IsNullOrEmpty(fileFormat))
                throw new ArgumentNullException(nameof(fileFormat));

            return new FileInfo
            {
                FileName = Path.GetFileNameWithoutExtension(fileName),
                FileExtension = fileFormat,
                FileSize = stream.Length,
                FileFormat = fileFormat,
                FileData = stream
            };
        }
    }
}
