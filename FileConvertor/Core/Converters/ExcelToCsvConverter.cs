using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace FileConvertor.Core.Converters
{
    /// <summary>
    /// Converter for converting Excel files to CSV using ClosedXML
    /// </summary>
    public class ExcelToCsvConverter : BaseConverter
    {
        /// <summary>
        /// Gets the source format this converter can handle
        /// </summary>
        public override string SourceFormat => "xlsx";

        /// <summary>
        /// Gets the target format this converter can produce
        /// </summary>
        public override string TargetFormat => "csv";

        /// <summary>
        /// Converts an Excel file to CSV
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

            // Load the Excel workbook
            using var workbook = new XLWorkbook(memoryStream);
            
            // Get the first worksheet
            var worksheet = workbook.Worksheet(1);
            
            // Create a writer for the target stream
            using var writer = new StreamWriter(targetStream, Encoding.UTF8, 1024, true);
            
            // Get the range with data
            var range = worksheet.RangeUsed();
            if (range != null)
            {
                // Process each row
                foreach (var row in range.Rows())
                {
                    // Build a CSV line from the cells in this row
                    var cells = row.Cells();
                    var csvLine = new StringBuilder();
                    
                    for (int i = 0; i < cells.Count(); i++)
                    {
                        var cell = cells.ElementAt(i);
                        string value = GetCellValueAsString(cell);
                        
                        // Escape quotes and wrap in quotes if needed
                        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                        {
                            value = "\"" + value.Replace("\"", "\"\"") + "\"";
                        }
                        
                        csvLine.Append(value);
                        
                        // Add comma if not the last cell
                        if (i < cells.Count() - 1)
                        {
                            csvLine.Append(",");
                        }
                    }
                    
                    // Write the CSV line
                    await writer.WriteLineAsync(csvLine.ToString());
                }
            }
            
            // Ensure all data is written
            await writer.FlushAsync();
        }
        
        /// <summary>
        /// Gets the cell value as a string, handling different data types
        /// </summary>
        /// <param name="cell">The Excel cell</param>
        /// <returns>String representation of the cell value</returns>
        private string GetCellValueAsString(IXLCell cell)
        {
            if (cell == null)
                return string.Empty;
                
            switch (cell.DataType)
            {
                case XLDataType.DateTime:
                    return cell.GetDateTime().ToString("yyyy-MM-dd HH:mm:ss");
                    
                case XLDataType.Number:
                    // Use invariant culture to ensure consistent decimal separator
                    return cell.GetValue<double>().ToString(System.Globalization.CultureInfo.InvariantCulture);
                    
                case XLDataType.Boolean:
                    return cell.GetValue<bool>().ToString().ToLower();
                    
                default:
                    return cell.GetString();
            }
        }
    }
}
