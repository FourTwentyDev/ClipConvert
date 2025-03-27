using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting CSV files to Excel using ClosedXML
    /// </summary>
    public class CsvToExcelConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "csv";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "xlsx";

        /// <summary>
        /// Converts a CSV file to Excel
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

            // Read the CSV content
            string csvContent;
            using (var reader = new StreamReader(sourceStream, leaveOpen: true))
            {
                csvContent = await reader.ReadToEndAsync();
            }
            
            // Reset the stream position in case it's needed later
            if (sourceStream.CanSeek)
            {
                sourceStream.Position = 0;
            }

            // Parse the CSV content
            var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            
            // Create a new workbook
            using var workbook = new XLWorkbook();
            
            // Add a worksheet
            var worksheet = workbook.Worksheets.Add("Sheet1");
            
            // Process each line
            for (int rowIndex = 0; rowIndex < lines.Length; rowIndex++)
            {
                string line = lines[rowIndex].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                
                // Parse the CSV line
                var values = ParseCsvLine(line);
                
                // Add values to the worksheet
                for (int colIndex = 0; colIndex < values.Length; colIndex++)
                {
                    worksheet.Cell(rowIndex + 1, colIndex + 1).Value = values[colIndex];
                }
            }
            
            // Auto-fit columns
            worksheet.Columns().AdjustToContents();
            
            // Save the workbook to the target stream
            workbook.SaveAs(targetStream);
        }
        
        /// <summary>
        /// Parses a CSV line into an array of values
        /// </summary>
        /// <param name="line">CSV line</param>
        /// <returns>Array of values</returns>
        private string[] ParseCsvLine(string line)
        {
            // This is a simplified CSV parser that handles quoted values
            var result = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    // Check if this is an escaped quote (two double quotes together)
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++; // Skip the next quote
                    }
                    else
                    {
                        // Toggle the inQuotes flag
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // End of value
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    // Add character to the current value
                    sb.Append(c);
                }
            }
            
            // Add the last value
            result.Add(sb.ToString());
            
            return result.ToArray();
        }
    }
}
