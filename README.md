# OBS Streaming Checklist Overlay

A complete **OBS overlay system** for streamers to display interactive checklists during live streams. Built 100% with Claude (Sonnet 4.5).

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![Built with Claude](https://img.shields.io/badge/Built%20with-Claude-purple.svg)

## Features

- ✅ **Visual Editor** - Windows Forms editor with drag-and-drop task management
- ✅ **Multiple Lists** - Create separate checklists for different purposes
- ✅ **Progress Tracking** - Checkboxes, progress bars, and counters
- ✅ **Auto-Scroll** - Seamless infinite scrolling or alternate between lists
- ✅ **Live Theme Editor** - Customize colors, fonts, borders, opacity in real-time
- ✅ **Persistent State** - Saves to local JSON file automatically
- ✅ **OBS Integration** - Simple browser source setup

## Quick Start

### Option 1: Use Pre-built Editor (Easiest)

1. Download the latest release from [Releases](../../releases)
2. Extract the files
3. Run `OBSChecklistEditor.exe`
4. Create your checklists in the editor
5. In OBS: **Add → Browser Source → Local File** → select `overlay.html`

### Option 2: Build from Source

**Requirements:**
- .NET Framework 4.8 or higher
- Visual Studio 2019+ (or any C# compiler)

**Build:**
```bash
# Clone the repository
git clone https://github.com/YOUR_USERNAME/obs-checklist-overlay.git
cd obs-checklist-overlay

# Build using the included batch file
BUILD_AND_RUN.bat

# Or build manually
csc /target:winexe /out:bin/OBSChecklistEditor.exe *.cs /r:System.Windows.Forms.dll /r:System.Drawing.dll
```

### Option 3: Overlay Only (No Editor)

If you just want to manually edit the JSON file:

1. Download `overlay.html` and `checklist-data-examples.json`
2. Rename `checklist-data-examples.json` to `checklist-data.json`
3. Edit the JSON file with your tasks
4. Add `overlay.html` as Browser Source in OBS

## Usage

### Editor Window

1. **Create Lists** - Click "Add List" to create new checklists
2. **Add Tasks** - Click "Add Task" and configure:
   - Task name
   - Checkboxes (on/off)
   - Progress bars with current/total values
   - Progress counters
   - Sub-headers for organization
3. **Reorder** - Drag and drop tasks to reorder
4. **Theme** - Click "Theme Editor" to customize appearance
5. **Auto-Scroll** - Configure scrolling behavior in "Auto-Scroll Settings"

### Auto-Scroll Modes

**Seamless Infinite Scroll:**
- Continuously loops through all selected lists
- No pauses or resets
- Best for: Long checklists that need constant display

**Alternate Lists:**
- Shows one list at a time
- Scrolls through each list individually
- Pauses between lists
- Best for: Multiple distinct categories

### OBS Setup

1. **Add Browser Source:**
   - Source → Browser
   - Check "Local file"
   - Browse to `overlay.html`
   
2. **Recommended Settings:**
   - Width: 400px (or match your overlay width setting)
   - Height: 600px (or match viewport height setting)
   - FPS: 30
   - Check "Shutdown source when not visible"

3. **Position & Style:**
   - Drag to desired position on stream
   - Use OBS filters for additional effects if needed

## Configuration

All settings are stored in `checklist-data.json`:

```json
{
  "lists": {
    "list1": {
      "name": "Pre-Stream Setup",
      "items": [
        {
          "name": "Check Audio Levels",
          "completed": false,
          "showCheckbox": true,
          "showProgressBar": false
        }
      ]
    }
  },
  "settings": {
    "activeListIds": ["list1"],
    "autoScrollEnabled": true,
    "autoScrollSpeed": 50,
    "scrollViewportHeight": 600
  },
  "theme": {
    "backgroundColor": "rgba(0, 0, 0, 0.7)",
    "textColor": "#ffffff",
    "progressBarColor": "#4CAF50"
  }
}
```

## Use Cases

- **Pre-Stream Checklists** - Verify audio, camera, overlays before going live
- **Game Achievement Tracking** - Show progress on challenges/collectibles
- **Donation Goal Displays** - Track fundraising progress with progress bars
- **Multi-Day Event Tasks** - Marathon stream goals and schedules
- **Speedrun Route Trackers** - Display current objective and completion status

## Technical Details

### Architecture

- **Editor:** Windows Forms C# application
- **Overlay:** Pure vanilla HTML/CSS/JavaScript (no frameworks)
- **Data Flow:** Editor writes JSON → Overlay polls every 500ms
- **No Server Required:** Everything runs locally

### File Structure

```
├── OBSChecklistEditor.exe    # Windows editor application
├── overlay.html               # OBS browser source
├── checklist-data.json        # Your checklist data
└── checklist-data-examples.json  # Example configurations
```

### Browser Compatibility

The overlay works in:
- OBS Browser Source (CEF)
- Chrome/Edge (via local server)
- Firefox (via local server)

**Note:** Modern browsers block local file access. The overlay works perfectly in OBS, but for browser testing you'll need to run a local web server.

## Development

Built entirely with **Claude (Sonnet 4)** across 15+ sessions:

- Initial architecture and feature design
- Multi-list support and drag-and-drop
- Auto-scroll with seamless infinite mode
- Theme system with live preview
- Bug fixes for edge cases (race conditions, viewport clipping, scroll math)

### Key Challenges Solved

1. **Seamless Infinite Scroll Math** - Rendering 3x content, calculating cycle heights
2. **Polling + Scroll Timing** - Coordinating config updates with active scrolling
3. **Transform Persistence** - Managing CSS transforms across re-renders
4. **Sequential Mode** - Showing only next incomplete item per list

## Security

**This project accesses NO external data, credentials, or network resources.**

- All data stored locally in JSON files
- No cloud services or APIs
- No telemetry or analytics
- No auto-updates

Safe for use on streaming PCs.

## Contributing

This project was built with AI assistance. Contributions welcome:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

MIT License - Feel free to use, modify, and distribute.

## Credits

Built 100% with **Claude (Sonnet 4)** by Anthropic.

Special thanks to the Claude Developer Discord community for inspiration and support.

---

**Questions?** Open an issue or discussion on GitHub.

**Found a bug?** Please report it with steps to reproduce.

**Want a feature?** Suggest it in the issues section!
