using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting WebP images to JPG using ImageSharp
    /// </summary>
    public class WebpToJpgConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "webp";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "jpg";

        /// <summary>
        /// Converts a WebP image to JPG
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

            // Load the WebP image
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(sourceStream);
            
            // Configure JPG encoder with high quality settings
            var encoder = new JpegEncoder
            {
                Quality = 90 // High quality
            };
            
            // Save as JPG
            await image.SaveAsJpegAsync(targetStream, encoder);
        }
    }
}
