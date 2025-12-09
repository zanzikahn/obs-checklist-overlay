using System;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace OBSChecklistEditor
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Determine the correct config path
            string configPath = GetConfigPath();
            
            // Initialize logging
            string projectRoot = Path.GetDirectoryName(configPath) ?? AppDomain.CurrentDomain.BaseDirectory;
            Logger.Initialize(projectRoot);
            Logger.Log("Application started");
            Logger.Log($"Config path: {configPath}");
            Logger.Log($"Project root: {projectRoot}");

            Application.Run(new MainForm(configPath));
        }

        private static string GetConfigPath()
        {
            // Try to find the project root by looking for overlay.html
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            
            // Method 1: Check if we're in bin/Debug/net6.0-windows
            // Go up 3 levels to project root
            string projectRoot = Path.GetFullPath(Path.Combine(exeDirectory, "..", "..", ".."));
            string overlayPath = Path.Combine(projectRoot, "overlay.html");
            
            if (File.Exists(overlayPath))
            {
                // Found the project root!
                return Path.Combine(projectRoot, "checklist-data.json");
            }

            // Method 2: Use a fixed known path (fallback)
            string fixedPath = @"C:\Users\Zanzi\Documents\StreamingTools_OBS_Checklist\checklist-data.json";
            if (Directory.Exists(Path.GetDirectoryName(fixedPath)))
            {
                return fixedPath;
            }

            // Method 3: Use exe directory as last resort
            return Path.Combine(exeDirectory, "checklist-data.json");
        }
    }
}
