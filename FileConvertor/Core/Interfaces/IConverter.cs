using System;
using System.IO;
using System.Threading.Tasks;

namespace FileConvertor.Core.Interfaces
{
    /// <summary>
    /// Interface for file format converters
    /// </summary>
    public interface IConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        string SourceFormat { get; }

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        string TargetFormat { get; }

        /// <summary>
        /// Checks if this converter can handle the specified source and target formats
        /// </summary>
        /// <param name="sourceFormat">Source file format</param>
        /// <param name="targetFormat">Target file format</param>
        /// <returns>True if this converter can handle the conversion, false otherwise</returns>
        bool CanConvert(string sourceFormat, string targetFormat);

        /// <summary>
        /// Converts a file from source format to target format
        /// </summary>
        /// <param name="sourceStream">Stream containing the source file data</param>
        /// <param name="targetStream">Stream to write the converted data to</param>
        /// <returns>Task representing the asynchronous conversion operation</returns>
        Task ConvertAsync(Stream sourceStream, Stream targetStream);
    }
}
