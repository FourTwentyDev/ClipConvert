using System;
using System.IO;
using System.Threading.Tasks;
using ImageMagick;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting HEIC images to JPG using Magick.NET
    /// </summary>
    public class HeicToJpgConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "heic";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "jpg";

        /// <summary>
        /// Converts a HEIC image to JPG
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

            // Create temporary files for processing
            string tempInputPath = Path.GetTempFileName() + ".heic";
            
            try
            {
                // Save the source stream to a temporary file
                using (var fileStream = new FileStream(tempInputPath, FileMode.Create, FileAccess.Write))
                {
                    sourceStream.Position = 0;
                    await sourceStream.CopyToAsync(fileStream);
                }

                // Use Magick.NET to convert HEIC to JPG
                using (var image = new MagickImage(tempInputPath))
                {
                    // Configure quality settings
                    image.Quality = 90; // High quality
                    
                    // Write directly to the target stream
                    await Task.Run(() => image.Write(targetStream, MagickFormat.Jpg));
                }
            }
            finally
            {
                // Clean up temporary files
                if (File.Exists(tempInputPath))
                    File.Delete(tempInputPath);
            }
        }
    }
}
