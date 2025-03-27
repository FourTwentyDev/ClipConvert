using System;
using System.IO;
using System.Threading.Tasks;
using FileConvertor.Core.Logging;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting text files to PDF using iText7
    /// </summary>
    public class TextToPdfConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "txt";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "pdf";

        /// <summary>
        /// Converts a text file to PDF
        /// </summary>
        /// <param name="sourceStream">Stream containing the source file data</param>
        /// <param name="targetStream">Stream to write the converted data to</param>
        /// <returns>Task representing the asynchronous conversion operation</returns>
        public override async Task ConvertAsync(Stream sourceStream, Stream targetStream)
        {
            Logger.Log(LogLevel.Info, "TextToPdfConverter", "Starting conversion from TXT to PDF");
            
            try
            {
                if (sourceStream == null)
                    throw new ArgumentNullException(nameof(sourceStream));
                
                if (targetStream == null)
                    throw new ArgumentNullException(nameof(targetStream));

                Logger.Log(LogLevel.Debug, "TextToPdfConverter", $"Source stream: CanRead={sourceStream.CanRead}, CanSeek={sourceStream.CanSeek}, Position={sourceStream.Position}, Length={sourceStream.Length}");
                Logger.Log(LogLevel.Debug, "TextToPdfConverter", $"Target stream: CanWrite={targetStream.CanWrite}, CanSeek={targetStream.CanSeek}");

                // Read the text from the source stream
                string text;
                Logger.Log(LogLevel.Debug, "TextToPdfConverter", "Reading text from source stream");
                using (var reader = new StreamReader(sourceStream, leaveOpen: true))
                {
                    text = await reader.ReadToEndAsync();
                }
                Logger.Log(LogLevel.Debug, "TextToPdfConverter", $"Read {text.Length} characters from source stream");
                
                // Reset the stream position in case it's needed later
                if (sourceStream.CanSeek)
                {
                    sourceStream.Position = 0;
                    Logger.Log(LogLevel.Debug, "TextToPdfConverter", "Reset source stream position to 0");
                }

                // Create a memory stream to hold the PDF content temporarily
                using (var memoryStream = new MemoryStream())
                {
                    // Create a byte array to hold the PDF content before the memory stream is closed
                    byte[] pdfBytes;
                    
                    // Create the PDF document in a separate scope to ensure it's properly closed
                    {
                        Logger.Log(LogLevel.Debug, "TextToPdfConverter", "Creating PDF writer");
                        // Create a PDF writer
                        var writer = new PdfWriter(memoryStream);
                        
                        Logger.Log(LogLevel.Debug, "TextToPdfConverter", "Creating PDF document");
                        // Create a PDF document
                        var pdf = new PdfDocument(writer);
                        
                        Logger.Log(LogLevel.Debug, "TextToPdfConverter", "Creating document");
                        // Create a document
                        var document = new Document(pdf);

                        try
                        {
                            Logger.Log(LogLevel.Debug, "TextToPdfConverter", "Setting up font");
                            // Set default font
                            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                            
                            Logger.Log(LogLevel.Debug, "TextToPdfConverter", "Splitting text into lines");
                            // Split the text into lines
                            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                            Logger.Log(LogLevel.Debug, "TextToPdfConverter", $"Text contains {lines.Length} lines");
                            
                            Logger.Log(LogLevel.Debug, "TextToPdfConverter", "Adding paragraphs to document");
                            // Add each line as a paragraph
                            foreach (var line in lines)
                            {
                                var paragraph = new Paragraph(line)
                                    .SetFont(font)
                                    .SetFontSize(11)
                                    .SetMarginBottom(5);
                                
                                document.Add(paragraph);
                            }
                            
                            Logger.Log(LogLevel.Debug, "TextToPdfConverter", "Adding metadata");
                            // Add metadata
                            pdf.GetDocumentInfo().SetTitle("Converted Text Document");
                            pdf.GetDocumentInfo().SetCreator("File Converter Application");
                            pdf.GetDocumentInfo().SetSubject("Text to PDF Conversion");
                            
                            Logger.Log(LogLevel.Debug, "TextToPdfConverter", "Closing document");
                        }
                        finally
                        {
                            // Close the document to apply all changes
                            document.Close();
                            Logger.Log(LogLevel.Debug, "TextToPdfConverter", "Document closed");
                        }
                    }
                    
                    // Get the PDF content as a byte array before the memory stream is disposed
                    pdfBytes = memoryStream.ToArray();
                    Logger.Log(LogLevel.Debug, "TextToPdfConverter", $"PDF content size: {pdfBytes.Length} bytes");
                    
                    // Copy the PDF data to the target stream
                    Logger.Log(LogLevel.Debug, "TextToPdfConverter", "Copying PDF data to target stream");
                    await targetStream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                    Logger.Log(LogLevel.Debug, "TextToPdfConverter", $"Copied {pdfBytes.Length} bytes to target stream");
                }
                
                Logger.Log(LogLevel.Info, "TextToPdfConverter", "Conversion from TXT to PDF completed successfully");
            }
            catch (Exception ex)
            {
                Logger.LogException(LogLevel.Error, "TextToPdfConverter", "Error during conversion from TXT to PDF", ex);
                throw; // Rethrow the exception to be handled by the caller
            }
        }
    }
}
