using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Processing;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting PNG images to BMP using ImageSharp
    /// </summary>
    public class PngToBmpConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "png";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "bmp";

        /// <summary>
        /// Converts a PNG image to BMP
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

            // Load the PNG image
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(sourceStream);
            
            // Configure BMP encoder with standard settings
            var encoder = new BmpEncoder();
            
            // Save as BMP
            await image.SaveAsBmpAsync(targetStream, encoder);
        }
    }
}
