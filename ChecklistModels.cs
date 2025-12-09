using System;
using System.Collections.Generic;

namespace OBSChecklistEditor
{
    public class ChecklistConfig
    {
        public Settings settings { get; set; } = null!;
        public Theme theme { get; set; } = null!;
        public Dictionary<string, ChecklistData> lists { get; set; } = null!;
    }

    public class Settings
    {
        public bool sequentialMode { get; set; }
        public int autoScrollSpeed { get; set; }
        public int itemHeight { get; set; }
        public string activeListId { get; set; } = null!; // Legacy support
        public List<string> activeListIds { get; set; } = new List<string>(); // Multiple active lists
        public double overlayOpacity { get; set; } = 1.0; // 0.0 to 1.0
        
        // Auto-scroll settings
        public bool autoScrollEnabled { get; set; } = false;
        public int scrollViewportHeight { get; set; } = 600; // Height of scroll viewport in pixels
        public int pauseTimeBottom { get; set; } = 3000; // Pause between lists in milliseconds (for alternate lists mode)
        public bool reverseScroll { get; set; } = false; // Scroll up instead of down
        public bool alternateLists { get; set; } = false; // Cycle through lists one at a time
    }

    public class Theme
    {
        public string backgroundColor { get; set; } = null!;
        public string textColor { get; set; } = null!;
        public string progressBarColor { get; set; } = null!;
        public string progressBarBackground { get; set; } = null!;
        public string checkboxColor { get; set; } = null!;
        public string fontFamily { get; set; } = null!;
        public string fontSize { get; set; } = null!;
        public string borderRadius { get; set; } = null!;
        public string subHeaderColor { get; set; } = "#FFD700"; // Gold/yellow for section headers
        public string subHeaderBackground { get; set; } = "rgba(255, 215, 0, 0.1)";
    }

    public class ChecklistData
    {
        public string name { get; set; } = null!;
        public List<TaskItem> items { get; set; } = null!;
    }

    public class TaskItem
    {
        public string id { get; set; } = null!;
        public string name { get; set; } = null!;
        public bool completed { get; set; }
        public bool showCheckbox { get; set; }
        public bool showProgressBar { get; set; }
        public bool showCounter { get; set; }
        public int current { get; set; }
        public int total { get; set; }
        
        // New property for sub-headers
        public bool isSubHeader { get; set; } = false;
    }
}
