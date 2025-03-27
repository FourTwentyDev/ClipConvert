using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileConvertor.Core.Interfaces;

namespace FileConvertor.Core.Helpers
{
    /// <summary>
    /// Helper class for detecting file types
    /// </summary>
    public class FileTypeDetector : IFileTypeDetector
    {
        private readonly ConverterFactory _converterFactory;
        private readonly Dictionary<string, string> _extensionToFormatMap;

        /// <summary>
        /// Initializes a new instance of the FileTypeDetector class
        /// </summary>
        /// <param name="converterFactory">Converter factory to use for format detection</param>
        public FileTypeDetector(ConverterFactory converterFactory)
        {
            _converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));
            _extensionToFormatMap = InitializeExtensionMap();
        }

        /// <summary>
        /// Initializes the extension to format map
        /// </summary>
        /// <returns>Dictionary mapping file extensions to formats</returns>
        private Dictionary<string, string> InitializeExtensionMap()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Text formats
                { ".txt", "txt" },
                { ".md", "md" },
                { ".html", "html" },
                { ".htm", "html" },
                { ".xml", "xml" },
                { ".json", "json" },
                { ".csv", "csv" },
                
                // Document formats
                { ".pdf", "pdf" },
                { ".docx", "docx" },
                { ".doc", "doc" },
                { ".rtf", "rtf" },
                
                // Spreadsheet formats
                { ".xlsx", "xlsx" },
                { ".xls", "xls" },
                
                // Image formats
                { ".jpg", "jpg" },
                { ".jpeg", "jpg" },
                { ".png", "png" },
                { ".gif", "gif" },
                { ".bmp", "bmp" },
                { ".tiff", "tiff" },
                { ".tif", "tiff" },
                { ".webp", "webp" },
                { ".svg", "svg" },
            };
        }

        /// <summary>
        /// Detects the file type from a file path
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Detected file type or null if detection failed</returns>
        public string DetectFromPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            string extension = Path.GetExtension(filePath);
            
            if (string.IsNullOrEmpty(extension))
                return null;

            return _extensionToFormatMap.TryGetValue(extension, out string format) ? format : null;
        }

        /// <summary>
        /// Detects the file type from a stream
        /// </summary>
        /// <param name="stream">Stream containing the file data</param>
        /// <returns>Detected file type or null if detection failed</returns>
        public string DetectFromStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            // This is a simplified implementation that doesn't actually detect from the stream content
            // In a real implementation, we would check magic bytes or other file signatures
            return null;
        }

        /// <summary>
        /// Gets a list of supported source formats
        /// </summary>
        /// <returns>Array of supported source formats</returns>
        public string[] GetSupportedSourceFormats()
        {
            return _converterFactory.GetSupportedSourceFormats();
        }

        /// <summary>
        /// Gets a list of supported target formats for a given source format
        /// </summary>
        /// <param name="sourceFormat">Source format</param>
        /// <returns>Array of supported target formats</returns>
        public string[] GetSupportedTargetFormats(string sourceFormat)
        {
            if (string.IsNullOrEmpty(sourceFormat))
                throw new ArgumentNullException(nameof(sourceFormat));

            return _converterFactory.GetSupportedTargetFormats(sourceFormat);
        }
    }
}
