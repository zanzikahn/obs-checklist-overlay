using System;
using System.IO;

namespace OBSChecklistEditor
{
    public static class Logger
    {
        private static string _logPath = "";
        private static readonly object _lockObj = new object();

        public static void Initialize(string projectRoot)
        {
            _logPath = Path.Combine(projectRoot, "overlay-log.txt");
            
            // Clear old log on start
            try
            {
                File.WriteAllText(_logPath, $"=== OBS Checklist Overlay Log Started {DateTime.Now} ===\n\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize log: {ex.Message}");
            }
        }

        public static void Log(string message)
        {
            if (string.IsNullOrEmpty(_logPath)) return;

            lock (_lockObj)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    string logLine = $"[{timestamp}] {message}\n";
                    File.AppendAllText(_logPath, logLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write log: {ex.Message}");
                }
            }
        }

        public static void LogError(string message, Exception ex)
        {
            Log($"ERROR: {message} - {ex.Message}");
            if (ex.StackTrace != null)
            {
                Log($"Stack: {ex.StackTrace}");
            }
        }

        public static string GetLogPath()
        {
            return _logPath;
        }
    }
}
