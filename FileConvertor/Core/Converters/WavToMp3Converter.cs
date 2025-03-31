using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Lame;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting WAV audio files to MP3 using NAudio and LAME
    /// </summary>
    public class WavToMp3Converter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "wav";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "mp3";

        /// <summary>
        /// Converts a WAV file to MP3
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

            // Create a temporary copy of the stream that we can seek
            using var memoryStream = new MemoryStream();
            await sourceStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // Convert WAV to MP3
            using (var reader = new WaveFileReader(memoryStream))
            {
                // Create an MP3 writer with high quality settings
                using (var writer = new LameMP3FileWriter(targetStream, reader.WaveFormat, 192))
                {
                    // Create a buffer for reading
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    
                    // Read and convert in chunks
                    while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
    }
}
