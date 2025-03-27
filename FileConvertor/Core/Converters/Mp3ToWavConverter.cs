using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting MP3 audio files to WAV using NAudio
    /// </summary>
    public class Mp3ToWavConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "mp3";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "wav";

        /// <summary>
        /// Converts an MP3 file to WAV
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

            // Convert MP3 to WAV
            using (var reader = new Mp3FileReader(memoryStream))
            {
                // Get the wave format from the MP3 file
                var outFormat = new WaveFormat(
                    reader.WaveFormat.SampleRate,
                    reader.WaveFormat.BitsPerSample,
                    reader.WaveFormat.Channels);
                
                // Create a wave provider that converts to the desired format
                using (var resampler = new MediaFoundationResampler(reader, outFormat))
                {
                    // Write the WAV header
                    WaveFileWriter.WriteWavFileToStream(targetStream, resampler);
                }
            }
        }
    }
}
