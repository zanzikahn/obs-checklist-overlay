# Quick Start Guide

## Get Up and Running in 5 Minutes

### Step 1: Build the Application (One-Time Setup)

Open Command Prompt in the project folder and run:

```bash
dotnet build
```

### Step 2: Launch the Editor

Run the compiled application:

```bash
cd bin\Debug\net6.0-windows
OBSChecklistEditor.exe
```

Or simply double-click `OBSChecklistEditor.exe` in the bin folder.

### Step 3: Create Your First Checklist

1. The editor opens with a sample checklist
2. Click **"Add Task"** to create tasks
3. Fill in task details:
   - Task Name: What you need to do
   - Enable Checkbox: ✓
   - Enable Progress Bar: ✓ (if tracking progress)
   - Enable Counter: ✓ (if showing X/Y format)
   - Set Current/Total values
4. Click **OK** to save

### Step 4: Add to OBS

1. Open OBS Studio
2. Add Source → Browser
3. Check **"Local file"**
4. Browse to: `C:\Users\Zanzi\Documents\StreamingTools_OBS_Checklist\overlay.html`
5. Set Width: 400, Height: 600
6. Click OK

### Step 5: Update While Gaming

**Option A: Alt-Tab Method**
1. Alt-Tab to the editor
2. Double-click a task to edit
3. Update current value or check "Mark as Completed"
4. Click OK (saves automatically)
5. Return to game

**Option B: Direct Edit (Advanced)**
1. Keep `checklist-data.json` open in Notepad
2. Edit values directly
3. Save file
4. Changes appear instantly

## Common Scenarios

### Tracking Resources (e.g., "Gather 100 Wood")
- Task Name: "Gather Wood"
- Show Checkbox: ✓
- Show Progress Bar: ✓
- Show Counter: ✓
- Current: 0, Total: 100
- Update "Current" as you gather

### Simple Checklist (e.g., "Kill Boss")
- Task Name: "Kill Boss"
- Show Checkbox: ✓
- Show Progress Bar: ✗
- Show Counter: ✗
- Just check "Mark as Completed" when done

### Multi-Step Progress (e.g., "Complete 5 Quests")
- Task Name: "Side Quests"
- Show Checkbox: ✓
- Show Progress Bar: ✓
- Show Counter: ✓
- Current: 0, Total: 5
- Increment "Current" after each quest

## Customization Quick Tips

### Change Colors
1. Click **"Theme Settings"**
2. Use CSS color formats:
   - `#FF0000` for red
   - `rgba(255, 0, 0, 0.5)` for semi-transparent red
3. Click OK to apply

### Multiple Lists
1. Click **"New List"** to create additional checklists
2. Use dropdown to switch active list
3. Perfect for different games or stream segments

### Sequential Mode
- **ON**: Forces viewers to see only up to the next task
- **OFF**: Shows all tasks (recommended for most uses)

## Troubleshooting Quick Fixes

**Not seeing changes in OBS?**
→ Right-click Browser Source → Refresh

**Editor crashes on start?**
→ Install .NET 6.0: https://dotnet.microsoft.com/download

**JSON syntax error?**
→ Use the editor instead of manually editing
→ Or validate JSON at: https://jsonlint.com/

## Next Steps

- Read the full README.md for advanced features
- Experiment with different themes and layouts
- Create multiple lists for different content
- Share your setup with other streamers!

---

**You're all set! Start streaming with your new checklist overlay!** 🎮✅
