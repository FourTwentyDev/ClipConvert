using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting PDF files to Word (DOCX) format
    /// </summary>
    public class PdfToWordConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "pdf";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "docx";

        /// <summary>
        /// Converts a PDF file to Word (DOCX) format
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
                // Process each page
                for (var i = 1; i <= document.NumberOfPages; i++)
                {
                    var page = document.GetPage(i);
                    
                    // Extract text from the page
                    foreach (var word in page.GetWords())
                    {
                        extractedText.Append(word.Text);
                        extractedText.Append(' ');
                    }
                    
                    extractedText.AppendLine();
                    extractedText.AppendLine();
                }
            }
            
            // Create a Word document
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(targetStream, WordprocessingDocumentType.Document))
            {
                // Add a main document part
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                
                // Create the document structure
                mainPart.Document = new Document();
                Body body = new Body();
                mainPart.Document.Append(body);
                
                // Create paragraphs from the extracted text
                string[] paragraphs = extractedText.ToString().Split(new[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string paragraph in paragraphs)
                {
                    if (!string.IsNullOrWhiteSpace(paragraph))
                    {
                        Paragraph para = new Paragraph(
                            new Run(
                                new Text(paragraph)
                            )
                        );
                        
                        body.Append(para);
                    }
                }
                
                // Save the document
                mainPart.Document.Save();
            }
        }
    }
}
