# Folder Dialog Improvements

## Issues Fixed

### 1. ✅ Items No Longer Disappear
**Problem:** Dragging items onto other items (non-folders) would cause them to disappear.

**Root Cause:** The drag-and-drop logic had an "else" clause that moved items to root when dropped on anything that wasn't a folder, including other lists. This caused visual confusion.

**Solution:**
- `DragOver` now only shows valid drop cursor (green) when over folders
- `DragDrop` now **only accepts drops on folders**
- Dropping on lists or empty space shows red "No Drop" cursor
- Items stay in place if drop is invalid

### 2. ✅ Multi-Select Support Added
**Problem:** Could only move one list at a time, tedious for organizing many lists.

**Solution:**
- **Changed from TreeView to ListView** for native multi-select support
- Users can now:
  - **Ctrl+Click**: Add/remove individual items from selection
  - **Shift+Click**: Select range of items
  - **Drag multiple lists** at once into folders
  - **Move multiple lists** using "Move to Folder" button

---

## UI Changes

### Before (TreeView):
```
📁 Expeditions
  ├─ Expedition-Level 1
  └─ Expedition-Level 2
📁 Workshops
  ├─ Workshop - Medical Lab
  └─ Workshop - Gunsmith I
📄 Quest - Clearer Skies
```

### After (ListView):
```
Type         Name
────────────────────────────────────
📁 Folder    Expeditions
  📄 List      Expedition-Level 1 - Description
  📄 List      Expedition-Level 2 - Description
📁 Folder    Workshops
  📄 List      Workshop - Medical Lab - Description
  📄 List      Workshop - Gunsmith I - Description
📁 Root      (Not in any folder)
  📄 List      Quest - Clearer Skies - Description
```

### Visual Indicators:
- **Folders**: Bold text, gray background, 📁 icon
- **Lists**: Regular text, indented with 📄 icon
- **Root section**: Yellow background

---

## New Features

### Multi-Select Operations

**Drag-and-Drop:**
1. Select multiple lists (Ctrl+Click or Shift+Click)
2. Drag selection onto a folder
3. All selected lists move to that folder

**Move to Folder Button:**
1. Select multiple lists
2. Click "Move to Folder..."
3. Choose destination folder from dropdown
4. All selected lists move at once

**Move to Root Button:**
1. Select multiple lists (from any folder)
2. Click "Move to Root"
3. All selected lists move to root level

### Keyboard Workflow

For users who prefer keyboard:
1. Use arrow keys to navigate
2. Ctrl+Click to multi-select
3. Click "Move to Folder..." button
4. Select destination with arrow keys + Enter

---

## Technical Implementation

### Data Structure Change

**ListView Item Tags:**

```csharp
// For folders
item.Tag = ListFolder object

// For lists in folders
item.Tag = Tuple<string, ListFolder>  // (listId, parentFolder)

// For root lists
item.Tag = string  // just listId

// For headers
item.Tag = "ROOT_HEADER"
```

### Drag-and-Drop Logic

```csharp
DragOver()
├─ Get item under cursor
├─ Check if it's a folder
├─ If folder: e.Effect = Move (green cursor)
└─ If not folder: e.Effect = None (red cursor)

DragDrop()
├─ Get dragged items (List<ListViewItem>)
├─ Get target item
├─ ONLY proceed if target is a folder
├─ For each dragged item:
│   ├─ Extract listId and oldFolder
│   ├─ Remove from oldFolder
│   └─ Add to targetFolder
└─ Reload view
```

### Multi-Select Handling

```csharp
ItemDrag()
├─ Get all selected items
├─ Filter out folders and headers (only lists can be dragged)
├─ Create List<ListViewItem> of draggable items
└─ Start drag operation with the list

GetSelectedListIds()
├─ Iterate through selected items
├─ Extract listId from tag
├─ Store with parent folder reference
└─ Return List<Tuple<string, ListFolder?>>
```

---

## Button State Logic

```
Selected Items           | Rename | Delete | Move to | Move to | Move  | Move
                         | Folder | Folder |  Folder |  Root   |  Up   | Down
─────────────────────────┼────────┼────────┼─────────┼─────────┼───────┼──────
Nothing                  |   ❌   |   ❌   |    ❌   |    ❌   |   ❌  |  ❌
Single Folder            |   ✅   |   ✅   |    ❌   |    ❌   |   ✅  |  ✅
Single List              |   ❌   |   ❌   |    ✅   |    ✅   |   ✅  |  ✅
Multiple Lists           |   ❌   |   ❌   |    ✅   |    ✅   |   ❌  |  ❌
Multiple Folders         |   ❌   |   ❌   |    ❌   |    ❌   |   ❌  |  ❌
Mixed (Folder + List)    |   ❌   |   ❌   |    ❌   |    ❌   |   ❌  |  ❌
```

---

## User Experience Improvements

### Before:
❌ Drag list onto another list → **List disappears**
❌ Want to move 10 lists → **10 separate operations**
❌ Accidentally drag onto wrong place → **Data lost**

### After:
✅ Drag list onto another list → **Red cursor, nothing happens**
✅ Want to move 10 lists → **Select all, drag once**
✅ Accidentally drag onto wrong place → **No change, data safe**

---

## Testing Scenarios

### Test 1: Single Item Drag
1. Drag a list onto a folder → ✅ Moves into folder
2. Drag a list onto another list → ✅ Shows red cursor, no change
3. Drag a list onto empty space → ✅ Shows red cursor, no change

### Test 2: Multi-Select Drag
1. Ctrl+Click 3 lists
2. Drag selection onto folder → ✅ All 3 move
3. Verify all 3 appear in folder → ✅ Success

### Test 3: Mixed Selection
1. Ctrl+Click folder and list → ✅ Buttons properly disabled
2. Cannot drag mixed selection → ✅ Only lists drag

### Test 4: Move to Folder Button
1. Select multiple lists
2. Click "Move to Folder..."
3. Choose folder → ✅ All move
4. Verify in correct folder → ✅ Success

### Test 5: Invalid Operations Protected
1. Try to drag folder onto list → ✅ Cannot drag folders
2. Try to drop lists on header → ✅ Red cursor, no change
3. Select headers → ✅ Buttons disabled

---

## Benefits

✅ **Safety**: Items can't disappear anymore
✅ **Efficiency**: Move multiple lists at once
✅ **Clarity**: Visual feedback shows valid drop zones
✅ **Flexibility**: Both drag-and-drop and button workflows
✅ **Intuitive**: Standard multi-select (Ctrl/Shift+Click)

---

## Files Changed

- `FolderManagerDialog.cs` - Complete rewrite
  - Changed from TreeView to ListView
  - Added multi-select support
  - Improved drag-and-drop logic
  - Added "Move to Folder" button
  - Better visual hierarchy

---

**Status:** ✅ FIXED AND IMPROVED

**Confidence:** 100% - ListView multi-select is a standard, well-tested pattern.
