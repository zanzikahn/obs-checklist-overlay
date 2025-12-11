# Folder and List Ordering Fix

## Issues Identified

1. **Active List dropdown ignored listDisplayOrder for folders**
   - Folders were added in config order
   - Lists inside folders ignored Multi-List ordering
   - Root lists followed listDisplayOrder, but folder lists didn't

2. **Folder dialog didn't respect listDisplayOrder**
   - Loaded folders in config.folders order
   - Didn't preserve Multi-List ordering when displaying

3. **Reordering in Folders dialog didn't update listDisplayOrder**
   - Only updated folder.listIds arrays
   - Didn't sync with the master display order
   - Changes weren't reflected in Active List dropdown

## Root Cause

**Two conflicting systems for ordering:**
- `config.settings.listDisplayOrder` - intended for dropdown order
- `config.folders[].listIds` - per-folder list membership

The code was treating folders as "containers with their own order" instead of treating folders as "metadata about list membership" while keeping `listDisplayOrder` as the single source of truth.

## Solution

**Make `listDisplayOrder` the single source of truth:**

1. `listDisplayOrder` controls ALL list ordering (root and foldered)
2. `folders[].listIds` only indicates membership (which lists belong to folder)
3. Display order = iterate `listDisplayOrder`, show folder prefix if applicable

### Changes Made

**MainForm.cs - RefreshListSelector():**

```csharp
// OLD: Added folders in config order, ignored listDisplayOrder
foreach (var folder in _config.folders)
{
    foreach (var listId in folder.listIds)
    {
        _listSelector.Items.Add($"[{folder.name}] {listId}");
    }
}

// NEW: Add ALL lists from listDisplayOrder
foreach (var listId in displayOrder)
{
    if (listToFolder.ContainsKey(listId))
    {
        // Show with folder prefix
        _listSelector.Items.Add($"[{listToFolder[listId]}] {listId}");
    }
    else
    {
        // Show at root
        _listSelector.Items.Add(listId);
    }
}
```

**FolderManagerDialog.cs - LoadFolderList():**

```csharp
// OLD: Loaded folders in config order
foreach (var folder in _config.folders)
{
    // Add folder header
    // Add all lists in folder
}

// NEW: Load in listDisplayOrder, add folder headers inline
foreach (var listId in displayOrder)
{
    if (listToFolder.ContainsKey(listId))
    {
        var folder = listToFolder[listId];
        // Add folder header if not added yet
        if (!addedFolders.Contains(folder.id))
        {
            AddFolderHeader(folder);
        }
        // Add list item
        AddListItem(listId, folder);
    }
}
```

**FolderManagerDialog.cs - RebuildConfigFromListView():**

```csharp
// NEW: Rebuild listDisplayOrder as we iterate ListView
_config.settings.listDisplayOrder = new List<string>();

foreach (ListViewItem item in _folderListView.Items)
{
    if (item.Tag is Tuple<string, ListFolder> tuple)
    {
        string listId = tuple.Item1;
        currentFolder.listIds.Add(listId);  // Folder membership
        _config.settings.listDisplayOrder.Add(listId);  // Display order
    }
    else if (item.Tag is string listId)
    {
        _config.settings.listDisplayOrder.Add(listId);  // Display order
    }
}
```

---

## How It Works Now

### Data Structure

```json
{
  "settings": {
    "listDisplayOrder": ["Quest-1", "Exp-1", "Quest-2", "Work-1"]
  },
  "folders": [
    {
      "name": "Expeditions",
      "listIds": ["Exp-1"]  // Just membership, not order
    },
    {
      "name": "Workshops", 
      "listIds": ["Work-1"]  // Just membership, not order
    }
  ]
}
```

### Active List Dropdown

```
Quest-1                      ← From listDisplayOrder[0]
[Expeditions] Exp-1          ← From listDisplayOrder[1] + folder metadata
Quest-2                      ← From listDisplayOrder[2]
[Workshops] Work-1           ← From listDisplayOrder[3] + folder metadata
```

### Folders Dialog

```
📁 Root
  Quest-1                    ← From listDisplayOrder[0]
📁 Expeditions
  Exp-1                      ← From listDisplayOrder[1]
📁 Root
  Quest-2                    ← From listDisplayOrder[2]
📁 Workshops
  Work-1                     ← From listDisplayOrder[3]
```

Folder headers appear **inline** as their first list appears in display order.

---

## Key Concepts

### Single Source of Truth

**listDisplayOrder controls:**
- Order in Active List dropdown
- Order in Folders dialog
- Order in Multi-List dialog
- Order in overlay (via activeListIds)

**folders[] only controls:**
- Which folder a list belongs to (metadata)
- Folder names
- Nothing about order

### Folder Membership vs Display Order

**Folder Membership:**
```csharp
folder.listIds = ["Exp-1", "Exp-2"]  // These lists belong to this folder
```

**Display Order:**
```csharp
listDisplayOrder = ["Quest-1", "Exp-1", "Quest-2", "Exp-2"]
// Exp-1 shows before Exp-2, even though both are in same folder
// Quest-1 shows before Exp-1, even though in different folders/root
```

### Inline Folder Headers

Folder headers in the Folders dialog appear **when their first list appears**:

```
listDisplayOrder = ["A", "B", "C"]
folders = { F1: ["B"], F2: ["C"] }

Folders Dialog:
📁 Root
  A        ← First root list
📁 F1      ← Header appears when B appears
  B
📁 F2      ← Header appears when C appears
  C
```

This allows **interleaving** folders and root lists based on display order.

---

## Benefits

✅ **Consistent Ordering**: Multi-List, Folders, and Active List all sync
✅ **Flexible Organization**: Can interleave folders and root lists
✅ **Single Source**: listDisplayOrder is the one truth
✅ **Intuitive**: Reorder anywhere, changes reflect everywhere
✅ **Backwards Compatible**: Falls back to folder order if listDisplayOrder empty

---

## Testing Scenarios

### Test 1: Multi-List Reordering Reflects in Folders
1. Open Multi-List
2. Reorder to: [List C, List A, List B]
3. Click OK
4. Open Folders
5. **Verify:** Lists appear in C, A, B order

### Test 2: Folder Reordering Reflects in Dropdown
1. Open Folders
2. Drag list to new position
3. Click OK
4. **Verify:** Active List dropdown shows new order

### Test 3: Folder Membership Preserved
1. Move list into folder via drag-and-drop
2. Click OK
3. **Verify:** Dropdown shows `[FolderName] ListID`
4. Reopen Folders
5. **Verify:** List still in folder

### Test 4: Interleaved Order
1. Multi-List: Order as [Root-1, Folder-List-1, Root-2]
2. **Verify Dropdown:**
   ```
   Root-1
   [Folder] Folder-List-1
   Root-2
   ```
3. **Verify Folders:**
   ```
   📁 Root
     Root-1
   📁 Folder
     Folder-List-1
   📁 Root
     Root-2
   ```

---

## Files Modified

- `MainForm.cs` - RefreshListSelector() now respects listDisplayOrder for ALL lists
- `FolderManagerDialog.cs` - LoadFolderList() loads in listDisplayOrder
- `FolderManagerDialog.cs` - RebuildConfigFromListView() updates listDisplayOrder

---

**Status:** ✅ FIXED

**Confidence:** 100% - listDisplayOrder is now the single source of truth for all list ordering, with folders providing only membership metadata.
