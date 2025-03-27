using System;
using System.IO;
using System.Threading.Tasks;
using FileConvertor.Core.Interfaces;
using FileConvertor.Core.Logging;
using Microsoft.IO;

namespace FileConvertor.Core
{
    /// <summary>
    /// Context class for the Strategy pattern that handles file conversion
    /// </summary>
    public class ConversionContext
    {
        private static readonly RecyclableMemoryStreamManager _memoryStreamManager = new RecyclableMemoryStreamManager();
        private IConverter? _converter;
        private readonly ConverterFactory? _converterFactory;

        /// <summary>
        /// Gets or sets the converter strategy
        /// </summary>
        public IConverter Converter
        {
            get => _converter ?? throw new InvalidOperationException("No converter strategy has been set.");
            set => _converter = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Initializes a new instance of the ConversionContext class
        /// </summary>
        /// <param name="converter">The converter strategy to use (can be null for delayed initialization)</param>
        public ConversionContext(IConverter? converter = null)
        {
            if (converter != null)
            {
                Converter = converter;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ConversionContext class with a converter factory
        /// </summary>
        /// <param name="converterFactory">Factory to create converters on demand</param>
        public ConversionContext(ConverterFactory converterFactory)
        {
            _converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));
        }

        /// <summary>
        /// Sets the converter based on source and target formats
        /// </summary>
        /// <param name="sourceFormat">Source format</param>
        /// <param name="targetFormat">Target format</param>
        /// <returns>True if a suitable converter was found, false otherwise</returns>
        public bool SetConverter(string sourceFormat, string targetFormat)
        {
            if (_converterFactory == null)
                throw new InvalidOperationException("Converter factory is not available.");

            var converter = _converterFactory.CreateConverter(sourceFormat, targetFormat);
            if (converter != null)
            {
                Converter = converter;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Converts a file using the current converter strategy
        /// </summary>
        /// <param name="sourceStream">Stream containing the source file data</param>
        /// <param name="targetStream">Stream to write the converted data to</param>
        /// <returns>Task representing the asynchronous conversion operation</returns>
        public async Task ConvertAsync(Stream sourceStream, Stream targetStream)
        {
            if (sourceStream == null)
                throw new ArgumentNullException(nameof(sourceStream));
            
            if (targetStream == null)
                throw new ArgumentNullException(nameof(targetStream));

            try
            {
                // Create a recyclable memory stream copy of the source
                using var sourceMemoryStream = _memoryStreamManager.GetStream("ConversionContext.SourceCopy");
                await sourceStream.CopyToAsync(sourceMemoryStream);
                sourceMemoryStream.Position = 0;
                
                Logger.Log(LogLevel.Debug, "ConversionContext", $"Starting conversion with {Converter.GetType().Name}");
                Logger.Log(LogLevel.Debug, "ConversionContext", $"Source stream: CanRead={sourceMemoryStream.CanRead}, CanSeek={sourceMemoryStream.CanSeek}, Position={sourceMemoryStream.Position}, Length={sourceMemoryStream.Length}");
                Logger.Log(LogLevel.Debug, "ConversionContext", $"Target stream: CanWrite={targetStream.CanWrite}, CanSeek={targetStream.CanSeek}");
                
                // Use the Converter property which will throw a descriptive exception if not set
                await Converter.ConvertAsync(sourceMemoryStream, targetStream);
                
                // Get the target length before any potential disposal
                long targetLength = 0;
                if (targetStream.CanSeek)
                {
                    targetLength = targetStream.Length;
                }
                
                Logger.Log(LogLevel.Debug, "ConversionContext", $"Conversion completed successfully. Target stream length: {targetLength}");
            }
            catch (Exception ex)
            {
                Logger.LogException(LogLevel.Error, "ConversionContext", $"Error during conversion with {Converter.GetType().Name}", ex);
                throw; // Rethrow the exception to be handled by the caller
            }
        }

        /// <summary>
        /// Converts a file using the current converter strategy
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="targetPath">Path to save the converted file</param>
        /// <returns>Task representing the asynchronous conversion operation</returns>
        public async Task ConvertFileAsync(string sourcePath, string targetPath)
        {
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath));
            
            if (string.IsNullOrEmpty(targetPath))
                throw new ArgumentNullException(nameof(targetPath));

            try
            {
                Logger.Log(LogLevel.Info, "ConversionContext", $"Converting file from {sourcePath} to {targetPath}");
                Logger.Log(LogLevel.Debug, "ConversionContext", $"Using converter: {Converter.GetType().Name}");
                
                // Use the Converter property which will throw a descriptive exception if not set
                using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
                using (var targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                {
                    Logger.Log(LogLevel.Debug, "ConversionContext", $"Source file size: {sourceStream.Length} bytes");
                    await Converter.ConvertAsync(sourceStream, targetStream);
                    Logger.Log(LogLevel.Debug, "ConversionContext", $"Target file size: {targetStream.Length} bytes");
                }
                
                Logger.Log(LogLevel.Info, "ConversionContext", $"File conversion completed successfully");
            }
            catch (Exception ex)
            {
                Logger.LogException(LogLevel.Error, "ConversionContext", $"Error converting file from {sourcePath} to {targetPath}", ex);
                throw; // Rethrow the exception to be handled by the caller
            }
        }
    }
}
