using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using Microsoft.IO;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting Word documents to PDF using OpenXML and iText7
    /// </summary>
    public class WordToPdfConverter : BaseConverter
    {
        private static readonly RecyclableMemoryStreamManager _memoryStreamManager = new RecyclableMemoryStreamManager();

        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "docx";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "pdf";

        /// <summary>
        /// Converts a Word document to PDF
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

            // Create a complete copy of the source stream to ensure we have all the data
            using var sourceMemoryStream = _memoryStreamManager.GetStream("WordToPdfConverter.SourceCopy");
            await sourceStream.CopyToAsync(sourceMemoryStream);
            sourceMemoryStream.Position = 0;

            // Extract text and structure from the Word document
            var documentContent = ExtractWordContent(sourceMemoryStream);

            // Create a temporary file for the PDF output
            string tempPdfPath = Path.Combine(Path.GetTempPath(), $"WordToPdf_{Guid.NewGuid()}.pdf");
            
            try
            {
                // Create a PDF writer that writes to a file
                using (var fileStream = new FileStream(tempPdfPath, FileMode.Create, FileAccess.Write))
                {
                    var writer = new PdfWriter(fileStream);
                    var pdf = new PdfDocument(writer);
                    var document = new iText.Layout.Document(pdf);
                    
                    // Set default font
                    var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    var italicFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);
                    
                    // Process each paragraph
                    foreach (var para in documentContent.Paragraphs)
                    {
                        var paragraph = new iText.Layout.Element.Paragraph();
                        
                        // Apply paragraph style
                        if (para.IsHeading)
                        {
                            float fontSize = para.HeadingLevel switch
                            {
                                1 => 24,
                                2 => 20,
                                3 => 18,
                                4 => 16,
                                5 => 14,
                                6 => 12,
                                _ => 12
                            };
                            
                            paragraph.SetFont(boldFont)
                                    .SetFontSize(fontSize)
                                    .SetMarginBottom(12);
                        }
                        else
                        {
                            paragraph.SetFont(font)
                                    .SetFontSize(11)
                                    .SetMarginBottom(8);
                        }
                        
                        // Add text with formatting
                        foreach (var run in para.Runs)
                        {
                            var text = new iText.Layout.Element.Text(run.Text);
                            
                            if (run.IsBold)
                                text.SetFont(boldFont);
                            else if (run.IsItalic)
                                text.SetFont(italicFont);
                            else
                                text.SetFont(font);
                                
                            paragraph.Add(text);
                        }
                        
                        document.Add(paragraph);
                    }
                    
                    // Add metadata
                    pdf.GetDocumentInfo().SetTitle("Converted Word Document");
                    pdf.GetDocumentInfo().SetCreator("File Converter Application");
                    pdf.GetDocumentInfo().SetSubject("Word to PDF Conversion");
                    
                    // Close the document to apply all changes
                    document.Close();
                }
                
                // Now read the PDF file and copy it to the target stream
                using (var fileStream = new FileStream(tempPdfPath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(targetStream);
                }
            }
            finally
            {
                // Clean up the temporary file
                if (File.Exists(tempPdfPath))
                {
                    try
                    {
                        File.Delete(tempPdfPath);
                    }
                    catch
                    {
                        // Ignore errors during cleanup
                    }
                }
            }
        }
        
        /// <summary>
        /// Extracts content from a Word document
        /// </summary>
        /// <param name="stream">Stream containing the Word document</param>
        /// <returns>Extracted document content</returns>
        private DocContent ExtractWordContent(Stream stream)
        {
            var content = new DocContent();
            
            // Create a complete copy of the stream to avoid it being closed by WordprocessingDocument
            using var memoryStream = _memoryStreamManager.GetStream("WordToPdfConverter.ExtractContent");
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            
            // Use leaveOpen: false since we're working with our own copy of the stream
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(memoryStream, false))
            {
                var mainPart = wordDocument.MainDocumentPart;
                if (mainPart != null && mainPart.Document != null)
                {
                    var body = mainPart.Document.Body;
                    if (body != null)
                    {
                        // Process paragraphs
                        foreach (var para in body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                        {
                            var paragraph = new DocParagraph();
                            
                            // Check if it's a heading
                            var styleId = para.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                            if (styleId != null && styleId.StartsWith("Heading"))
                            {
                                paragraph.IsHeading = true;
                                if (int.TryParse(styleId.Substring(7), out int level))
                                {
                                    paragraph.HeadingLevel = level;
                                }
                            }
                            
                            // Process runs (text fragments)
                            foreach (var run in para.Elements<Run>())
                            {
                                var textRun = new DocTextRun();
                                
                                // Get text
                                var text = string.Join("", run.Elements<DocumentFormat.OpenXml.Wordprocessing.Text>().Select(t => t.Text));
                                textRun.Text = text;
                                
                                // Check formatting
                                var runProps = run.RunProperties;
                                if (runProps != null)
                                {
                                    textRun.IsBold = runProps.Bold != null;
                                    textRun.IsItalic = runProps.Italic != null;
                                    textRun.IsUnderline = runProps.Underline != null;
                                }
                                
                                paragraph.Runs.Add(textRun);
                            }
                            
                            content.Paragraphs.Add(paragraph);
                        }
                    }
                }
            }
            
            return content;
        }
        
        /// <summary>
        /// Class representing the content of a Word document
        /// </summary>
        private class DocContent
        {
            public List<DocParagraph> Paragraphs { get; } = new List<DocParagraph>();
        }
        
        /// <summary>
        /// Class representing a paragraph in a Word document
        /// </summary>
        private class DocParagraph
        {
            public bool IsHeading { get; set; }
            public int HeadingLevel { get; set; }
            public List<DocTextRun> Runs { get; } = new List<DocTextRun>();
        }
        
        /// <summary>
        /// Class representing a text run in a Word document
        /// </summary>
        private class DocTextRun
        {
            public string Text { get; set; } = string.Empty;
            public bool IsBold { get; set; }
            public bool IsItalic { get; set; }
            public bool IsUnderline { get; set; }
        }
    }
}
