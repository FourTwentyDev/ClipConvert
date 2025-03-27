using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting PNG images to JPG
    /// </summary>
    public class PngToJpgConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "png";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "jpg";

        /// <summary>
        /// Converts a PNG image to JPG
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
            var decoder = new PngBitmapDecoder(sourceStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            var frame = decoder.Frames[0];

            // Convert to JPG
            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(frame));
            encoder.QualityLevel = 90; // 0-100, higher is better quality but larger file size

            // Save to the target stream
            encoder.Save(targetStream);

            // Ensure all data is written
            await targetStream.FlushAsync();
        }
    }
}
