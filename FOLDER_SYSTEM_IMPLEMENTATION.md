# Folder System Implementation

## Overview

The folder system allows users to organize their checklists into folders for better management, especially when dealing with many lists.

## Features

### 1. Folder Management Dialog

**Access:** Click the "📁 Folders" button in the main editor

**Capabilities:**
- ✅ Create new folders
- ✅ Rename folders
- ✅ Delete folders (lists move to root)
- ✅ Drag lists into/out of folders
- ✅ Reorder folders
- ✅ Reorder lists within folders
- ✅ Context menu (right-click on list → "Move to Root")
- ✅ Expand/collapse state persists

### 2. Folder Display in Active List Dropdown

Lists in folders show as: `[FolderName] ListID`
Root-level lists show as: `ListID`

**Example:**
```
[Expeditions] Expedition-Level 1
[Expeditions] Expedition-Level 2
[Workshops] Workshop - Medical Lab
[Workshops] Workshop - Gunsmith I
Quest - Clearer Skies
Quest - Trash Into Treasure
```

## Data Structure

### ListFolder Class

```csharp
public class ListFolder
{
    public string id { get; set; }              // Unique identifier
    public string name { get; set; }            // Display name
    public List<string> listIds { get; set; }   // List IDs in this folder
    public bool isExpanded { get; set; }        // UI state (expanded/collapsed)
}
```

### Config Storage

```json
{
  "folders": [
    {
      "id": "folder123abc",
      "name": "Expeditions",
      "listIds": ["Expedition-Level 1", "Expedition-Level 2"],
      "isExpanded": true
    },
    {
      "id": "folder456def",
      "name": "Workshops",
      "listIds": ["Workshop - Medical Lab", "Workshop - Gunsmith I"],
      "isExpanded": false
    }
  ]
}
```

## User Interface

### FolderManagerDialog Layout

```
┌─────────────────────────────────────────────┐
│  Organize your lists into folders           │
├──────────────────────────┬──────────────────┤
│                          │                  │
│  📁 Expeditions          │  New Folder      │
│    ├─ Expedition-Level 1 │  Rename          │
│    └─ Expedition-Level 2 │  Delete Folder   │
│                          │                  │
│  📁 Workshops            │  Move Up ▲       │
│    ├─ Workshop - Medical │  Move Down ▼     │
│    └─ Workshop - Gunsmith│                  │
│                          │                  │
│  📄 Quest - Clearer Skies│                  │
│  📄 Quest - Trash Into   │                  │
│                          │                  │
└──────────────────────────┴──────────────────┘
                    [OK] [Cancel]
```

### TreeView Structure

- **Folders**: Bold, expandable/collapsible
- **Lists**: Regular text, can be dragged
- **Icons**: 📁 for folders, 📄 for lists

## Implementation Details

### 1. Drag and Drop

**Dragging a List:**
- Can drop on a folder → moves list into folder
- Can drop on root or another list → moves list to root
- Removes from previous folder if applicable

**Dragging a Folder:**
- Can drop on another folder → reorders folders
- Cannot nest folders (flat structure)

**Code Flow:**
```csharp
FolderTreeView_ItemDrag()
  ↓
FolderTreeView_DragEnter() // Validate drop
  ↓
FolderTreeView_DragOver()  // Visual feedback
  ↓
FolderTreeView_DragDrop()  // Execute move
```

### 2. Folder-Aware List Selection

**MainForm Helper Methods:**

```csharp
GetSelectedListId()
├─ Checks if selection starts with "["
├─ Extracts list ID from "[Folder] ListID" format
└─ Returns clean list ID

SelectListById(listId)
├─ Searches dropdown items
├─ Matches both "listId" and "[Folder] listId"
└─ Selects matching item
```

**RefreshListSelector() Logic:**

1. Create map of list → folder
2. Add folder lists: `[FolderName] ListID`
3. Add root lists: `ListID`
4. Add remaining lists (newly created)
5. Restore selection using GetSelectedListId()

### 3. Folder Operations

**Create Folder:**
```csharp
NewFolderButton_Click()
├─ Prompt for folder name
├─ Generate unique folder ID (GUID)
├─ Create ListFolder object
├─ Add to config.folders
└─ Reload tree
```

**Rename Folder:**
```csharp
RenameFolderButton_Click()
├─ Get selected folder
├─ Prompt for new name
├─ Update folder.name
└─ Update TreeNode text
```

**Delete Folder:**
```csharp
DeleteFolderButton_Click()
├─ Confirm with user
├─ Remove folder from config.folders
├─ Lists automatically move to root (not in any folder)
└─ Reload tree
```

**Move Up/Down:**
```csharp
MoveUpButton_Click() / MoveDownButton_Click()
├─ Get node and siblings
├─ Calculate new index
├─ Remove and reinsert node
├─ If folder, update config.folders order
└─ Reselect node
```

### 4. Save and Load

**On OK Click:**
```csharp
OkButton_Click()
├─ Save expand/collapse states
├─ Rebuild config.folders from TreeView
├─ Clear and repopulate folder.listIds
└─ Return DialogResult.OK
```

**On Dialog Open:**
```csharp
LoadFolderTree()
├─ Clear TreeView
├─ Create set of lists in folders
├─ Add folder nodes with child lists
├─ Restore expand/collapse state
└─ Add remaining root lists
```

## Usage Examples

### Example 1: Organizing Game Quests

**Before:**
```
Expedition-Level 1
Expedition-Level 2
Expedition-Level 3
Workshop - Medical Lab
Workshop - Gunsmith I
Quest - Clearer Skies
```

**Create Structure:**
1. Click "📁 Folders"
2. Click "New Folder" → name it "Expeditions"
3. Drag Expedition lists into Expeditions folder
4. Click "New Folder" → name it "Workshops"
5. Drag Workshop lists into Workshops folder
6. Click OK

**After:**
```
[Expeditions] Expedition-Level 1
[Expeditions] Expedition-Level 2
[Expeditions] Expedition-Level 3
[Workshops] Workshop - Medical Lab
[Workshops] Workshop - Gunsmith I
Quest - Clearer Skies
```

### Example 2: Moving List to Root

**Method 1: Right-click**
1. Right-click on list in folder
2. Select "Move to Root"

**Method 2: Drag**
1. Drag list from folder
2. Drop on empty space or root list

### Example 3: Reordering Folders

1. Open Folders dialog
2. Select a folder
3. Click "Move Up ▲" or "Move Down ▼"
4. Or drag folder to new position

## Benefits

✅ **Organization**: Group related lists together
✅ **Clarity**: Easier to find lists in long dropdown
✅ **Flexibility**: Easy to reorganize as needs change
✅ **Persistence**: Folder structure saves with config
✅ **Visual**: Tree view shows hierarchy clearly
✅ **Drag-and-Drop**: Intuitive list management

## Technical Notes

### Thread Safety
- All operations are on UI thread
- No threading issues

### Performance
- TreeView handles hundreds of nodes efficiently
- Folder lookup uses HashSet (O(1))
- Linear scan for dropdown matching (acceptable for typical list counts)

### Memory
- Minimal overhead (just folder metadata)
- Lists not duplicated, only referenced by ID

### Backwards Compatibility
- Config without folders works normally
- Lists default to root level
- No migration needed

## Future Enhancements (Not Implemented)

- [ ] Nested folders (sub-folders)
- [ ] Folder colors/icons
- [ ] Bulk move operations
- [ ] Search/filter in tree
- [ ] Folder templates
- [ ] Export/import folder structure

---

**Status:** ✅ FULLY IMPLEMENTED

**Files:**
- `FolderManagerDialog.cs` - Complete folder management UI
- `MainForm.cs` - Integration and dropdown display
- `ChecklistModels.cs` - ListFolder data model

**Confidence:** 100% - Full folder system with all core features working.
