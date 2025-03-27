using System;
using System.IO;

namespace FileConvertor.Core.Interfaces
{
    /// <summary>
    /// Interface for detecting file types
    /// </summary>
    public interface IFileTypeDetector
    {
        /// <summary>
        /// Detects the file type from a file path
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Detected file type or null if detection failed</returns>
        string DetectFromPath(string filePath);

        /// <summary>
        /// Detects the file type from a stream
        /// </summary>
        /// <param name="stream">Stream containing the file data</param>
        /// <returns>Detected file type or null if detection failed</returns>
        string DetectFromStream(Stream stream);

        /// <summary>
        /// Gets a list of supported source formats
        /// </summary>
        /// <returns>Array of supported source formats</returns>
        string[] GetSupportedSourceFormats();

        /// <summary>
        /// Gets a list of supported target formats for a given source format
        /// </summary>
        /// <param name="sourceFormat">Source format</param>
        /// <returns>Array of supported target formats</returns>
        string[] GetSupportedTargetFormats(string sourceFormat);
    }
}
