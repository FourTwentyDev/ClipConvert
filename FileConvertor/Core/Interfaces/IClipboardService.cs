using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileConvertor.Core.Interfaces
{
    /// <summary>
    /// Interface for clipboard operations
    /// </summary>
    public interface IClipboardService
    {
        /// <summary>
        /// Checks if the clipboard contains a file
        /// </summary>
        /// <returns>True if the clipboard contains a file, false otherwise</returns>
        bool ContainsFile();

        /// <summary>
        /// Gets the file path from the clipboard if available
        /// </summary>
        /// <returns>File path or null if no file is in the clipboard</returns>
        string GetFilePath();

        /// <summary>
        /// Gets a list of file paths from the clipboard if available
        /// </summary>
        /// <returns>List of file paths or empty list if no files are in the clipboard</returns>
        List<string> GetFilePaths();

        /// <summary>
        /// Gets the file data from the clipboard if available
        /// </summary>
        /// <returns>Stream containing the file data or null if no file is in the clipboard</returns>
        Stream GetFileData();

        /// <summary>
        /// Sets the file data to the clipboard
        /// </summary>
        /// <param name="fileData">Stream containing the file data</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileType">MIME type of the file</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task SetFileDataAsync(Stream fileData, string fileName, string fileType);
    }
}
