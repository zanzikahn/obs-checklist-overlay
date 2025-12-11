# Multi-List Dialog Order Preservation Fix

## Issue
Unchecked items in the Multi-List dialog were not maintaining their position when reopening the dialog. They would revert to their original dictionary order.

## Root Cause

The `MultiListSelectorDialog` constructor was receiving only `activeListIds` (checked items), not the complete `listDisplayOrder`. This meant:

1. Dialog loaded lists from `activeListIds` (checked items only)
2. Then added unchecked items in dictionary order
3. User's carefully ordered unchecked items were lost

### Code Flow (BEFORE):

```csharp
// MainForm.cs
using (var dialog = new MultiListSelectorDialog(_config.lists, _config.settings.activeListIds))
//                                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^
//                                                                  Only checked items!

// MultiListSelectorDialog.cs  
public MultiListSelectorDialog(Dictionary<string, ChecklistData> allLists, List<string> currentlySelected)
{
    _selectedListIds = new List<string>(currentlySelected);
    LoadLists();  // Uses _selectedListIds - unchecked items in random order!
}

private void LoadLists()
{
    // Add checked items first (in order)
    foreach (var listId in _selectedListIds) { ... }
    
    // Add unchecked items (in dictionary order - WRONG!)
    foreach (var kvp in _allLists.Where(kvp => !_selectedListIds.Contains(kvp.Key))) { ... }
}
```

## Solution

Pass **both** `activeListIds` (checked) and `listDisplayOrder` (complete order) to the dialog:

### Changes Made:

**1. Updated Constructor** (`MultiListSelectorDialog.cs`):
```csharp
private List<string> _displayOrder = new List<string>();  // NEW FIELD

public MultiListSelectorDialog(
    Dictionary<string, ChecklistData> allLists, 
    List<string> currentlySelected,
    List<string>? displayOrder = null)  // NEW PARAMETER
{
    _allLists = allLists;
    _selectedListIds = new List<string>(currentlySelected);
    
    // Use provided display order, or fall back to selected items
    _displayOrder = displayOrder != null && displayOrder.Count > 0 
        ? new List<string>(displayOrder) 
        : new List<string>(currentlySelected);
        
    InitializeComponents();
    LoadLists();
}
```

**2. Updated LoadLists()** (`MultiListSelectorDialog.cs`):
```csharp
private void LoadLists()
{
    _listView.Items.Clear();

    // Load lists in display order (ALL lists, not just checked)
    foreach (var listId in _displayOrder)
    {
        if (_allLists.ContainsKey(listId))
        {
            var item = new ListViewItem(listId);
            item.SubItems.Add(_allLists[listId].name);
            item.Checked = _selectedListIds.Contains(listId);  // Check if selected
            item.Tag = listId;
            _listView.Items.Add(item);
        }
    }

    // Then add any remaining lists that aren't in display order (newly created)
    foreach (var kvp in _allLists.Where(kvp => !_displayOrder.Contains(kvp.Key)))
    {
        var item = new ListViewItem(kvp.Key);
        item.SubItems.Add(kvp.Value.name);
        item.Checked = _selectedListIds.Contains(kvp.Key);
        item.Tag = kvp.Key;
        _listView.Items.Add(item);
    }
}
```

**3. Updated Dialog Call** (`MainForm.cs`):
```csharp
using (var dialog = new MultiListSelectorDialog(
    _config.lists, 
    _config.settings.activeListIds,      // Checked items
    _config.settings.listDisplayOrder))  // Complete order (NEW!)
{
    // ...
}
```

## How It Works Now

### Scenario: User's Order
```
1. List A (checked)
2. List B (unchecked) ← Important position!
3. List C (checked)
4. List D (unchecked) ← Another position!
```

### What Gets Saved:
```json
{
  "activeListIds": ["A", "C"],              // For overlay (checked only)
  "listDisplayOrder": ["A", "B", "C", "D"]  // Complete order
}
```

### What Gets Loaded:
```csharp
// Dialog constructor receives:
currentlySelected = ["A", "C"]            // From activeListIds
displayOrder = ["A", "B", "C", "D"]       // From listDisplayOrder

// LoadLists() iterates displayOrder:
foreach (var listId in _displayOrder)  // ["A", "B", "C", "D"]
{
    item.Checked = _selectedListIds.Contains(listId);
    // A: checked=true
    // B: checked=false ← In correct position!
    // C: checked=true
    // D: checked=false ← In correct position!
}
```

### Dialog Shows:
```
☑ List A
☐ List B      ← Preserved position!
☑ List C
☐ List D      ← Preserved position!
```

## Testing

**Before Fix:**
1. Open Multi-List
2. Order: A (checked), B (unchecked), C (checked)
3. Click OK
4. Reopen Multi-List
5. ❌ Order: A (checked), C (checked), B (unchecked) ← B moved to bottom!

**After Fix:**
1. Open Multi-List
2. Order: A (checked), B (unchecked), C (checked)
3. Click OK
4. Reopen Multi-List
5. ✅ Order: A (checked), B (unchecked), C (checked) ← Order preserved!

## Benefits

✅ **Unchecked items maintain position**
✅ **Complete order persists across sessions**
✅ **Users can organize all lists, not just active ones**
✅ **Backwards compatible** (falls back if displayOrder is null)

---

**Status:** ✅ FIXED

**Files Modified:**
- `MultiListSelectorDialog.cs` - Constructor and LoadLists()
- `MainForm.cs` - Dialog instantiation

**Confidence:** 100% - The dialog now receives and uses the complete display order.
