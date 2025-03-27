using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting JPG images to PNG using ImageSharp
    /// </summary>
    public class JpgToPngConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "jpg";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "png";

        /// <summary>
        /// Converts a JPG image to PNG
        /// </summary>
        /// <param name="sourceStream">Stream containing the source file data</param>
        /// <param name="targetStream">Stream to write the converted data to</param>
        /// <returns>Task representing the asynchronous conversion operation</returns>
        public override async Task ConvertAsync(Stream sourceStream, Stream targetStream)
        {
            if (sourceStream == null)
                throw new ArgumentNullException(nameof(sourceStream));
            
            if (targetStream == null)
                throw new ArgumentNullException(nameof(targetStream));

            // Load the JPG image
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(sourceStream);
            
            // Configure PNG encoder with high quality settings
            var encoder = new PngEncoder
            {
                CompressionLevel = PngCompressionLevel.BestCompression,
                ColorType = PngColorType.RgbWithAlpha,
                BitDepth = PngBitDepth.Bit8
            };
            
            // Save as PNG
            await image.SaveAsPngAsync(targetStream, encoder);
        }
    }
}
