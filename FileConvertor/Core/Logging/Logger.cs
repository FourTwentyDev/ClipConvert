using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FileConvertor.Core.Logging
{
    /// <summary>
    /// Simple logging utility for the application
    /// </summary>
    public static class Logger
    {
        private static readonly object _lockObject = new object();
        private static string _logFilePath;

        /// <summary>
        /// Initializes the logger with the specified log file path
        /// </summary>
        /// <param name="logFilePath">Path to the log file</param>
        public static void Initialize(string logFilePath)
        {
            _logFilePath = logFilePath;
            
            // Create the directory if it doesn't exist
            var directory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Write a header to the log file
            Log(LogLevel.Info, "Logger", $"Logging initialized at {DateTime.Now}");
        }

        /// <summary>
        /// Logs a message with the specified log level
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="source">Source of the log message</param>
        /// <param name="message">Log message</param>
        public static void Log(LogLevel level, string source, string message)
        {
            if (string.IsNullOrEmpty(_logFilePath))
            {
                // If the logger hasn't been initialized, use a default path
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var logDirectory = Path.Combine(appDataPath, "FileConvertor", "Logs");
                _logFilePath = Path.Combine(logDirectory, $"FileConvertor_{DateTime.Now:yyyyMMdd}.log");
                
                // Create the directory if it doesn't exist
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
            }
            
            try
            {
                var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] [{source}] {message}{Environment.NewLine}";
                
                lock (_lockObject)
                {
                    File.AppendAllText(_logFilePath, logEntry);
                }
                
                // Also write to debug output
                System.Diagnostics.Debug.WriteLine(logEntry);
            }
            catch (Exception ex)
            {
                // If logging fails, write to debug output
                System.Diagnostics.Debug.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs an exception with the specified log level
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="source">Source of the log message</param>
        /// <param name="message">Log message</param>
        /// <param name="exception">Exception to log</param>
        public static void LogException(LogLevel level, string source, string message, Exception exception)
        {
            var sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine($"Exception: {exception.GetType().Name}");
            sb.AppendLine($"Message: {exception.Message}");
            sb.AppendLine($"Stack Trace: {exception.StackTrace}");
            
            // Log inner exception if present
            var innerException = exception.InnerException;
            while (innerException != null)
            {
                sb.AppendLine($"Inner Exception: {innerException.GetType().Name}");
                sb.AppendLine($"Message: {innerException.Message}");
                sb.AppendLine($"Stack Trace: {innerException.StackTrace}");
                innerException = innerException.InnerException;
            }
            
            Log(level, source, sb.ToString());
        }
        
        /// <summary>
        /// Gets the path to the current log file
        /// </summary>
        /// <returns>Path to the log file</returns>
        public static string GetLogFilePath()
        {
            return _logFilePath;
        }
    }
    
    /// <summary>
    /// Log levels for the logger
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }
}
