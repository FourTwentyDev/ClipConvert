using System;
using System.IO;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting HTML files to PDF using iText7 HTML2PDF
    /// </summary>
    public class HtmlToPdfConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "html";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "pdf";

        /// <summary>
        /// Converts an HTML file to PDF
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

            // Read the HTML content
            string htmlContent;
            using (var reader = new StreamReader(sourceStream, leaveOpen: true))
            {
                htmlContent = await reader.ReadToEndAsync();
            }
            
            // Reset the stream position in case it's needed later
            if (sourceStream.CanSeek)
            {
                sourceStream.Position = 0;
            }

            // Create a PDF writer
            var writer = new PdfWriter(targetStream);
            
            // Create a PDF document
            var pdf = new PdfDocument(writer);
            
            // Create a converter
            var converterProperties = new ConverterProperties();
            
            // Set base URI for resolving relative resources (images, CSS)
            // In a real implementation, we might need to extract resources from the HTML
            converterProperties.SetBaseUri("");
            
            try
            {
                // Convert HTML to PDF
                HtmlConverter.ConvertToPdf(htmlContent, pdf, converterProperties);
            }
            catch (Exception ex)
            {
                // If conversion fails, try a simpler approach with just the text content
                try
                {
                    // Close the failed document
                    pdf.Close();
                    
                    // Reset the target stream
                    targetStream.SetLength(0);
                    
                    // Create a new writer and document
                    var fallbackWriter = new PdfWriter(targetStream);
                    var fallbackPdf = new PdfDocument(fallbackWriter);
                    var document = new Document(fallbackPdf);
                    
                    // Add a note about the conversion issue
                    document.Add(new iText.Layout.Element.Paragraph("HTML Conversion Note: Full rendering failed. Simplified version shown below."));
                    document.Add(new iText.Layout.Element.Paragraph("Error: " + ex.Message));
                    document.Add(new iText.Layout.Element.Paragraph("\n"));
                    
                    // Extract and add just the text content
                    var textContent = ExtractTextFromHtml(htmlContent);
                    document.Add(new iText.Layout.Element.Paragraph(textContent));
                    
                    // Close the document
                    document.Close();
                }
                catch
                {
                    // If even the fallback fails, rethrow the original exception
                    throw;
                }
            }
        }
        
        /// <summary>
        /// Extracts text content from HTML (very simplified)
        /// </summary>
        /// <param name="html">HTML content</param>
        /// <returns>Extracted text</returns>
        private string ExtractTextFromHtml(string html)
        {
            // This is a very simplified text extraction
            // In a real implementation, we would use a proper HTML parser
            
            // Remove HTML tags
            var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", " ");
            
            // Decode HTML entities
            text = System.Net.WebUtility.HtmlDecode(text);
            
            // Normalize whitespace
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
            
            return text.Trim();
        }
    }
}
