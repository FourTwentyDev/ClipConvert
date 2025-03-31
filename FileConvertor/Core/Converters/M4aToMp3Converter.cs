using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.MediaFoundation;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting M4A audio files to MP3 using NAudio
    /// </summary>
    public class M4aToMp3Converter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "m4a";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "mp3";

        /// <summary>
        /// Converts an M4A file to MP3
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
            string tempInputPath = Path.GetTempFileName() + ".m4a";
            string tempWavPath = Path.GetTempFileName() + ".wav";
            
            try
            {
                // Save the source stream to a temporary file
                using (var fileStream = new FileStream(tempInputPath, FileMode.Create, FileAccess.Write))
                {
                    sourceStream.Position = 0;
                    await sourceStream.CopyToAsync(fileStream);
                }

                // Extract audio from M4A to WAV using MediaFoundation
                using (var reader = new MediaFoundationReader(tempInputPath))
                {
                    // Save as WAV first
                    WaveFileWriter.CreateWaveFile(tempWavPath, reader);
                }

                // Convert WAV to MP3
                using (var reader = new WaveFileReader(tempWavPath))
                {
                    // Use MediaFoundationEncoder to convert to MP3
                    MediaFoundationEncoder.EncodeToMp3(reader, targetStream, 192000);
                }
            }
            finally
            {
                // Clean up temporary files
                if (File.Exists(tempInputPath))
                    File.Delete(tempInputPath);
                
                if (File.Exists(tempWavPath))
                    File.Delete(tempWavPath);
            }
        }
    }
}
