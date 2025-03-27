using System;
using System.IO;
using System.Threading.Tasks;
using FileConvertor.Core.Interfaces;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Base class for file format converters
    /// </summary>
    public abstract class BaseConverter : IConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public abstract string SourceFormat { get; }

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public abstract string TargetFormat { get; }

        /// <summary>
        /// Checks if this converter can handle the specified source and target formats
        /// </summary>
        /// <param name="sourceFormat">Source file format</param>
        /// <param name="targetFormat">Target file format</param>
        /// <returns>True if this converter can handle the conversion, false otherwise</returns>
        public virtual bool CanConvert(string sourceFormat, string targetFormat)
        {
            if (string.IsNullOrEmpty(sourceFormat))
                throw new ArgumentNullException(nameof(sourceFormat));
            
            if (string.IsNullOrEmpty(targetFormat))
                throw new ArgumentNullException(nameof(targetFormat));

            return SourceFormat.Equals(sourceFormat, StringComparison.OrdinalIgnoreCase) &&
                   TargetFormat.Equals(targetFormat, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Converts a file from source format to target format
        /// </summary>
        /// <param name="sourceStream">Stream containing the source file data</param>
        /// <param name="targetStream">Stream to write the converted data to</param>
        /// <returns>Task representing the asynchronous conversion operation</returns>
        public abstract Task ConvertAsync(Stream sourceStream, Stream targetStream);

        /// <summary>
        /// Converts a file from source format to target format
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="targetPath">Path to save the converted file</param>
        /// <returns>Task representing the asynchronous conversion operation</returns>
        public virtual async Task ConvertFileAsync(string sourcePath, string targetPath)
        {
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath));
            
            if (string.IsNullOrEmpty(targetPath))
                throw new ArgumentNullException(nameof(targetPath));

            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            using (var targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
            {
                await ConvertAsync(sourceStream, targetStream);
            }
        }
    }
}
