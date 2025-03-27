using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.Tables;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting Markdown files to HTML using Markdig
    /// </summary>
    public class MarkdownToHtmlConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "md";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "html";

        /// <summary>
        /// Converts a Markdown file to HTML
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

            // Read the markdown from the source stream
            string markdown;
            using (var reader = new StreamReader(sourceStream, leaveOpen: true))
            {
                markdown = await reader.ReadToEndAsync();
            }
            
            // Reset the stream position in case it's needed later
            if (sourceStream.CanSeek)
            {
                sourceStream.Position = 0;
            }

            // Configure the Markdig pipeline with common extensions
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()  // Enables most advanced extensions
                .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)  // GitHub-style header IDs
                .UseEmojiAndSmiley()  // Convert emoji shortcodes to Unicode
                .UseGridTables()  // Support for grid tables
                .UsePipeTables()  // Support for pipe tables
                .UseListExtras()  // Extra list features
                .UseTaskLists()  // GitHub-style task lists
                .UseAutoLinks()  // Auto-links
                .UseGenericAttributes()  // Generic attributes
                .Build();

            // Convert markdown to HTML
            string htmlContent = Markdown.ToHtml(markdown, pipeline);

            // Create a complete HTML document
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("  <meta charset=\"UTF-8\">");
            html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine("  <title>Converted Markdown</title>");
            html.AppendLine("  <style>");
            html.AppendLine("    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; line-height: 1.6; padding: 20px; max-width: 800px; margin: 0 auto; color: #24292e; }");
            html.AppendLine("    h1, h2, h3, h4, h5, h6 { margin-top: 24px; margin-bottom: 16px; font-weight: 600; line-height: 1.25; }");
            html.AppendLine("    h1 { font-size: 2em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }");
            html.AppendLine("    h2 { font-size: 1.5em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }");
            html.AppendLine("    a { color: #0366d6; text-decoration: none; }");
            html.AppendLine("    a:hover { text-decoration: underline; }");
            html.AppendLine("    code { font-family: SFMono-Regular, Consolas, 'Liberation Mono', Menlo, monospace; background-color: rgba(27, 31, 35, 0.05); padding: 0.2em 0.4em; border-radius: 3px; }");
            html.AppendLine("    pre { background-color: #f6f8fa; border-radius: 3px; padding: 16px; overflow: auto; }");
            html.AppendLine("    pre code { background-color: transparent; padding: 0; }");
            html.AppendLine("    blockquote { padding: 0 1em; color: #6a737d; border-left: 0.25em solid #dfe2e5; margin: 0; }");
            html.AppendLine("    table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
            html.AppendLine("    table th, table td { border: 1px solid #dfe2e5; padding: 6px 13px; }");
            html.AppendLine("    table tr { background-color: #fff; border-top: 1px solid #c6cbd1; }");
            html.AppendLine("    table tr:nth-child(2n) { background-color: #f6f8fa; }");
            html.AppendLine("    img { max-width: 100%; }");
            html.AppendLine("    .task-list-item { list-style-type: none; }");
            html.AppendLine("    .task-list-item-checkbox { margin-right: 8px; }");
            html.AppendLine("  </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine(htmlContent);
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            // Write the HTML to the target stream
            using (var writer = new StreamWriter(targetStream, Encoding.UTF8, 1024, true))
            {
                await writer.WriteAsync(html.ToString());
            }
        }
    }
}
