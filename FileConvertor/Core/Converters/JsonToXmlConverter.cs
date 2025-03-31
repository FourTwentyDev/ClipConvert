using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting JSON files to XML format
    /// </summary>
    public class JsonToXmlConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "json";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "xml";

        /// <summary>
        /// Converts a JSON file to XML format
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

            // Read the JSON content
            string jsonContent;
            using (var reader = new StreamReader(sourceStream))
            {
                jsonContent = await reader.ReadToEndAsync();
            }

            // Parse the JSON
            JToken jsonToken = JToken.Parse(jsonContent);
            
            // Create XML settings
            var xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace
            };
            
            // Create XML writer
            using (var xmlWriter = XmlWriter.Create(targetStream, xmlSettings))
            {
                // Start document
                xmlWriter.WriteStartDocument();
                
                // Create a root element if the JSON is an object or array
                xmlWriter.WriteStartElement("root");
                
                // Convert JSON to XML
                WriteJsonToXml(jsonToken, xmlWriter);
                
                // End root element
                xmlWriter.WriteEndElement();
                
                // End document
                xmlWriter.WriteEndDocument();
            }
        }
        
        /// <summary>
        /// Recursively writes JSON to XML
        /// </summary>
        /// <param name="token">JSON token to write</param>
        /// <param name="writer">XML writer</param>
        private void WriteJsonToXml(JToken token, XmlWriter writer)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (JProperty property in token.Children<JProperty>())
                    {
                        // Use property name as element name, sanitizing if needed
                        string elementName = SanitizeXmlElementName(property.Name);
                        writer.WriteStartElement(elementName);
                        
                        // Process property value
                        WriteJsonToXml(property.Value, writer);
                        
                        writer.WriteEndElement();
                    }
                    break;
                
                case JTokenType.Array:
                    foreach (JToken item in token.Children())
                    {
                        writer.WriteStartElement("item");
                        WriteJsonToXml(item, writer);
                        writer.WriteEndElement();
                    }
                    break;
                
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Date:
                    writer.WriteString(token.ToString());
                    break;
                
                case JTokenType.Null:
                    // For null values, we write an empty element with a nil attribute
                    writer.WriteAttributeString("nil", "true");
                    break;
            }
        }
        
        /// <summary>
        /// Sanitizes a string to be used as an XML element name
        /// </summary>
        /// <param name="name">Name to sanitize</param>
        /// <returns>Sanitized name</returns>
        private string SanitizeXmlElementName(string name)
        {
            // XML element names must start with a letter or underscore
            if (string.IsNullOrEmpty(name) || !char.IsLetter(name[0]) && name[0] != '_')
            {
                return "element_" + name;
            }
            
            // Replace invalid characters with underscores
            return System.Text.RegularExpressions.Regex.Replace(name, @"[^\w\-\.]", "_");
        }
    }
}
