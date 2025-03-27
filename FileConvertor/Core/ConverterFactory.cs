using System;
using System.Collections.Generic;
using System.Linq;
using FileConvertor.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using FileConvertor.Core.Logging;

namespace FileConvertor.Core
{
    /// <summary>
    /// Factory class for creating converters with lazy loading support
    /// </summary>
    public class ConverterFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Dictionary<string, Type>> _converterTypes = new Dictionary<string, Dictionary<string, Type>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, Lazy<IConverter>>> _lazyConverters = new Dictionary<string, Dictionary<string, Lazy<IConverter>>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the ConverterFactory class
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving converters</param>
        public ConverterFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Registers a converter type with the factory
        /// </summary>
        /// <param name="converterType">Type of converter to register</param>
        public void RegisterConverterType(Type converterType)
        {
            if (converterType == null)
                throw new ArgumentNullException(nameof(converterType));

            if (!typeof(IConverter).IsAssignableFrom(converterType))
                throw new ArgumentException($"Type {converterType.Name} does not implement IConverter", nameof(converterType));

            try
            {
                // Create a temporary instance to get format information
                var tempConverter = (IConverter)Activator.CreateInstance(converterType);
                var sourceFormat = tempConverter.SourceFormat;
                var targetFormat = tempConverter.TargetFormat;

                Logger.Log(LogLevel.Debug, "ConverterFactory", $"Registering converter type: {converterType.Name} for {sourceFormat} to {targetFormat}");

                // Register the converter type
                if (!_converterTypes.TryGetValue(sourceFormat, out var targetFormats))
                {
                    targetFormats = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
                    _converterTypes[sourceFormat] = targetFormats;
                }

                targetFormats[targetFormat] = converterType;

                // Initialize lazy converter dictionary for this source format if needed
                if (!_lazyConverters.TryGetValue(sourceFormat, out var lazyTargetConverters))
                {
                    lazyTargetConverters = new Dictionary<string, Lazy<IConverter>>(StringComparer.OrdinalIgnoreCase);
                    _lazyConverters[sourceFormat] = lazyTargetConverters;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(LogLevel.Error, "ConverterFactory", $"Error registering converter type: {converterType.Name}", ex);
                throw;
            }
        }

        /// <summary>
        /// Creates a converter for the specified source and target formats
        /// </summary>
        /// <param name="sourceFormat">Source file format</param>
        /// <param name="targetFormat">Target file format</param>
        /// <returns>Converter that can handle the conversion, or null if no suitable converter is found</returns>
        public IConverter CreateConverter(string sourceFormat, string targetFormat)
        {
            if (string.IsNullOrEmpty(sourceFormat))
                throw new ArgumentNullException(nameof(sourceFormat));
            
            if (string.IsNullOrEmpty(targetFormat))
                throw new ArgumentNullException(nameof(targetFormat));

            try
            {
                // Check if we have a lazy-loaded instance already
                if (_lazyConverters.TryGetValue(sourceFormat, out var lazyTargetConverters) &&
                    lazyTargetConverters.TryGetValue(targetFormat, out var lazyConverter))
                {
                    Logger.Log(LogLevel.Debug, "ConverterFactory", $"Using existing lazy converter for {sourceFormat} to {targetFormat}");
                    return lazyConverter.Value;
                }

                // Try to find the converter type
                if (_converterTypes.TryGetValue(sourceFormat, out var targetFormats) &&
                    targetFormats.TryGetValue(targetFormat, out var converterType))
                {
                    Logger.Log(LogLevel.Debug, "ConverterFactory", $"Creating new converter for {sourceFormat} to {targetFormat}");
                    
                    // Create a lazy-loaded converter
                    var lazy = new Lazy<IConverter>(() => 
                    {
                        Logger.Log(LogLevel.Debug, "ConverterFactory", $"Lazy-initializing converter: {converterType.Name}");
                        return (IConverter)ActivatorUtilities.CreateInstance(_serviceProvider, converterType);
                    });
                    
                    // Store it for future use
                    if (!_lazyConverters.TryGetValue(sourceFormat, out lazyTargetConverters))
                    {
                        lazyTargetConverters = new Dictionary<string, Lazy<IConverter>>(StringComparer.OrdinalIgnoreCase);
                        _lazyConverters[sourceFormat] = lazyTargetConverters;
                    }
                    
                    lazyTargetConverters[targetFormat] = lazy;
                    
                    return lazy.Value;
                }

                Logger.Log(LogLevel.Warning, "ConverterFactory", $"No converter found for {sourceFormat} to {targetFormat}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(LogLevel.Error, "ConverterFactory", $"Error creating converter for {sourceFormat} to {targetFormat}", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets a list of supported source formats
        /// </summary>
        /// <returns>Array of supported source formats</returns>
        public string[] GetSupportedSourceFormats()
        {
            return _converterTypes.Keys.ToArray();
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

            if (_converterTypes.TryGetValue(sourceFormat, out var targetFormats))
            {
                return targetFormats.Keys.ToArray();
            }

            return Array.Empty<string>();
        }
    }
}
