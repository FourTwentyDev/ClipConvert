using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting PDF files to text using PdfPig
    /// </summary>
    public class PdfToTextConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "pdf";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "txt";

        /// <summary>
        /// Converts a PDF file to text
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

            // Extract text from the PDF
            var extractedText = new StringBuilder();
            
            // Open the PDF document
            using (var document = PdfDocument.Open(memoryStream))
            {
                // Add document metadata
                if (!string.IsNullOrEmpty(document.Information.Title))
                {
                    extractedText.AppendLine($"Title: {document.Information.Title}");
                }
                
                if (!string.IsNullOrEmpty(document.Information.Author))
                {
                    extractedText.AppendLine($"Author: {document.Information.Author}");
                }
                
                if (!string.IsNullOrEmpty(document.Information.CreationDate))
                {
                    extractedText.AppendLine($"Creation Date: {document.Information.CreationDate}");
                }
                
                extractedText.AppendLine($"Number of Pages: {document.NumberOfPages}");
                extractedText.AppendLine();
                extractedText.AppendLine("--- Document Content ---");
                extractedText.AppendLine();
                
                // Process each page
                for (var i = 1; i <= document.NumberOfPages; i++)
                {
                    var page = document.GetPage(i);
                    
                    // Add page number
                    extractedText.AppendLine($"--- Page {i} ---");
                    extractedText.AppendLine();
                    
                    // Extract text from the page
                    var pageText = ExtractTextFromPage(page);
                    extractedText.AppendLine(pageText);
                    extractedText.AppendLine();
                }
            }
            
            // Write the extracted text to the target stream
            using (var writer = new StreamWriter(targetStream, Encoding.UTF8, 1024, true))
            {
                await writer.WriteAsync(extractedText.ToString());
            }
        }
        
        /// <summary>
        /// Extracts text from a PDF page
        /// </summary>
        /// <param name="page">PDF page</param>
        /// <returns>Extracted text</returns>
        private string ExtractTextFromPage(Page page)
        {
            var text = new StringBuilder();
            
            // Get all words on the page
            foreach (var word in page.GetWords())
            {
                text.Append(word.Text);
                text.Append(' ');
            }
            
            return text.ToString();
        }
    }
}
