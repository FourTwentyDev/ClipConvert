using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting XML files to JSON format
    /// </summary>
    public class XmlToJsonConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "xml";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "json";

        /// <summary>
        /// Converts an XML file to JSON format
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

            // Load the XML document
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(sourceStream);
            
            // Configure JSON serializer settings
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };
            
            // Convert XML to JSON
            string jsonText = JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.Indented, true);
            
            // Write the JSON to the target stream
            using (var writer = new StreamWriter(targetStream, System.Text.Encoding.UTF8, 1024, true))
            {
                await writer.WriteAsync(jsonText);
            }
        }
    }
}
