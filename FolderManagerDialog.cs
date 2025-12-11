using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace OBSChecklistEditor
{
    public class FolderManagerDialog : Form
    {
        private ChecklistConfig _config = null!;
        private ListView _folderListView = null!;
        private Button _newFolderButton = null!;
        private Button _renameFolderButton = null!;
        private Button _deleteFolderButton = null!;
        private Button _moveUpButton = null!;
        private Button _moveDownButton = null!;
        private Button _moveToFolderButton = null!;
        private Button _moveToRootButton = null!;
        private int _insertionIndex = -1;  // Track where insertion line should appear

        public FolderManagerDialog(ChecklistConfig config)
        {
            _config = config;
            InitializeComponents();
            LoadFolderList();
        }

        private void InitializeComponents()
        {
            this.Text = "Manage Folders";
            this.Width = 700;
            this.Height = 550;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var instructionLabel = new Label
            {
                Text = "Organize your lists into folders. Use Ctrl+Click or Shift+Click to select multiple lists.\n" +
                       "Drag lists into folders. Lists can only be dropped into folders, not other lists.",
                Location = new Point(20, 15),
                Width = 650,
                Height = 40
            };

            _folderListView = new ListView
            {
                Location = new Point(20, 60),
                Width = 500,
                Height = 380,
                View = View.Details,
                FullRowSelect = true,
                AllowDrop = true,
                HideSelection = false,
                MultiSelect = true  // Enable multi-select
            };
            _folderListView.Columns.Add("Type", 80);
            _folderListView.Columns.Add("Name", 400);
            _folderListView.SelectedIndexChanged += FolderListView_SelectedIndexChanged;
            _folderListView.ItemDrag += FolderListView_ItemDrag;
            _folderListView.DragEnter += FolderListView_DragEnter;
            _folderListView.DragOver += FolderListView_DragOver;
            _folderListView.DragDrop += FolderListView_DragDrop;
            _folderListView.DragLeave += FolderListView_DragLeave;
            _folderListView.Paint += FolderListView_Paint;

            // Button panel
            Panel buttonPanel = new Panel
            {
                Location = new Point(530, 60),
                Width = 140,
                Height = 380
            };

            _newFolderButton = new Button
            {
                Text = "New Folder",
                Location = new Point(0, 0),
                Width = 130
            };
            _newFolderButton.Click += NewFolderButton_Click;

            _renameFolderButton = new Button
            {
                Text = "Rename Folder",
                Location = new Point(0, 35),
                Width = 130,
                Enabled = false
            };
            _renameFolderButton.Click += RenameFolderButton_Click;

            _deleteFolderButton = new Button
            {
                Text = "Delete Folder",
                Location = new Point(0, 70),
                Width = 130,
                Enabled = false
            };
            _deleteFolderButton.Click += DeleteFolderButton_Click;

            _moveToFolderButton = new Button
            {
                Text = "Move to Folder...",
                Location = new Point(0, 120),
                Width = 130,
                Enabled = false
            };
            _moveToFolderButton.Click += MoveToFolderButton_Click;

            _moveToRootButton = new Button
            {
                Text = "Move to Root",
                Location = new Point(0, 155),
                Width = 130,
                Enabled = false
            };
            _moveToRootButton.Click += MoveToRootButton_Click;

            _moveUpButton = new Button
            {
                Text = "Move Up ▲",
                Location = new Point(0, 205),
                Width = 130,
                Enabled = false
            };
            _moveUpButton.Click += MoveUpButton_Click;

            _moveDownButton = new Button
            {
                Text = "Move Down ▼",
                Location = new Point(0, 240),
                Width = 130,
                Enabled = false
            };
            _moveDownButton.Click += MoveDownButton_Click;

            buttonPanel.Controls.AddRange(new Control[] {
                _newFolderButton, _renameFolderButton, _deleteFolderButton,
                _moveToFolderButton, _moveToRootButton, _moveUpButton, _moveDownButton
            });

            var okButton = new Button
            {
                Text = "OK",
                Location = new Point(490, 455),
                Width = 85,
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;

            var cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(585, 455),
                Width = 85,
                DialogResult = DialogResult.Cancel
            };

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            this.Controls.AddRange(new Control[] {
                instructionLabel, _folderListView, buttonPanel, okButton, cancelButton
            });
        }

        private void LoadFolderList()
        {
            _folderListView.Items.Clear();

            // Create a map of which lists are in which folders
            var listToFolder = new Dictionary<string, ListFolder>();
            foreach (var folder in _config.folders)
            {
                foreach (var listId in folder.listIds)
                {
                    listToFolder[listId] = folder;
                }
            }
            
            // Get display order
            List<string> displayOrder = _config.settings.listDisplayOrder;
            if (displayOrder == null || displayOrder.Count == 0)
            {
                // Fallback: build from folders then root
                displayOrder = new List<string>();
                foreach (var folder in _config.folders)
                {
                    displayOrder.AddRange(folder.listIds);
                }
                foreach (var kvp in _config.lists)
                {
                    if (!listToFolder.ContainsKey(kvp.Key))
                    {
                        displayOrder.Add(kvp.Key);
                    }
                }
            }

            // Track which folders we've added headers for
            var addedFolders = new HashSet<string>();
            
            // Add items in displayOrder
            foreach (var listId in displayOrder)
            {
                if (_config.lists.ContainsKey(listId))
                {
                    if (listToFolder.ContainsKey(listId))
                    {
                        // List is in a folder
                        var folder = listToFolder[listId];
                        
                        // Add folder header if not added yet
                        if (!addedFolders.Contains(folder.id))
                        {
                            var folderItem = new ListViewItem("📁 Folder");
                            folderItem.SubItems.Add(folder.name);
                            folderItem.Tag = folder;
                            folderItem.Font = new Font(_folderListView.Font, FontStyle.Bold);
                            folderItem.BackColor = Color.LightGray;
                            _folderListView.Items.Add(folderItem);
                            addedFolders.Add(folder.id);
                        }
                        
                        // Add list item
                        var listItem = new ListViewItem("  📄 List");
                        listItem.SubItems.Add($"  {listId} - {_config.lists[listId].name}");
                        listItem.Tag = new Tuple<string, ListFolder>(listId, folder);
                        _folderListView.Items.Add(listItem);
                    }
                    else
                    {
                        // Root list - add root header if not added yet
                        if (!addedFolders.Contains("ROOT"))
                        {
                            var rootHeader = new ListViewItem("📁 Root");
                            rootHeader.SubItems.Add("(Not in any folder)");
                            rootHeader.Tag = "ROOT_HEADER";
                            rootHeader.Font = new Font(_folderListView.Font, FontStyle.Bold);
                            rootHeader.BackColor = Color.LightYellow;
                            _folderListView.Items.Add(rootHeader);
                            addedFolders.Add("ROOT");
                        }
                        
                        var listItem = new ListViewItem("  📄 List");
                        listItem.SubItems.Add($"  {listId} - {_config.lists[listId].name}");
                        listItem.Tag = listId;
                        _folderListView.Items.Add(listItem);
                    }
                }
            }
            
            // Add any remaining lists not in displayOrder
            foreach (var kvp in _config.lists)
            {
                if (!displayOrder.Contains(kvp.Key))
                {
                    if (listToFolder.ContainsKey(kvp.Key))
                    {
                        // In a folder
                        var folder = listToFolder[kvp.Key];
                        if (!addedFolders.Contains(folder.id))
                        {
                            var folderItem = new ListViewItem("📁 Folder");
                            folderItem.SubItems.Add(folder.name);
                            folderItem.Tag = folder;
                            folderItem.Font = new Font(_folderListView.Font, FontStyle.Bold);
                            folderItem.BackColor = Color.LightGray;
                            _folderListView.Items.Add(folderItem);
                            addedFolders.Add(folder.id);
                        }
                        
                        var listItem = new ListViewItem("  📄 List");
                        listItem.SubItems.Add($"  {kvp.Key} - {kvp.Value.name}");
                        listItem.Tag = new Tuple<string, ListFolder>(kvp.Key, folder);
                        _folderListView.Items.Add(listItem);
                    }
                    else
                    {
                        // At root
                        if (!addedFolders.Contains("ROOT"))
                        {
                            var rootHeader = new ListViewItem("📁 Root");
                            rootHeader.SubItems.Add("(Not in any folder)");
                            rootHeader.Tag = "ROOT_HEADER";
                            rootHeader.Font = new Font(_folderListView.Font, FontStyle.Bold);
                            rootHeader.BackColor = Color.LightYellow;
                            _folderListView.Items.Add(rootHeader);
                            addedFolders.Add("ROOT");
                        }
                        
                        var listItem = new ListViewItem("  📄 List");
                        listItem.SubItems.Add($"  {kvp.Key} - {kvp.Value.name}");
                        listItem.Tag = kvp.Key;
                        _folderListView.Items.Add(listItem);
                    }
                }
            }
        }

        private void FolderListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool hasSelection = _folderListView.SelectedItems.Count > 0;
            
            if (!hasSelection)
            {
                _renameFolderButton.Enabled = false;
                _deleteFolderButton.Enabled = false;
                _moveToFolderButton.Enabled = false;
                _moveToRootButton.Enabled = false;
                _moveUpButton.Enabled = false;
                _moveDownButton.Enabled = false;
                return;
            }

            // Check what's selected
            bool hasFolder = false;
            bool hasList = false;
            bool allLists = true;

            foreach (ListViewItem item in _folderListView.SelectedItems)
            {
                if (item.Tag is ListFolder)
                {
                    hasFolder = true;
                    allLists = false;
                }
                else if (item.Tag is string || item.Tag is Tuple<string, ListFolder>)
                {
                    hasList = true;
                }
                else if (item.Tag is string && (string)item.Tag == "ROOT_HEADER")
                {
                    allLists = false;
                }
            }

            // Enable buttons based on selection
            _renameFolderButton.Enabled = hasFolder && _folderListView.SelectedItems.Count == 1;
            _deleteFolderButton.Enabled = hasFolder && _folderListView.SelectedItems.Count == 1;
            _moveToFolderButton.Enabled = allLists && hasList;
            _moveToRootButton.Enabled = allLists && hasList;
            
            // Move up/down only for single selection
            if (_folderListView.SelectedItems.Count == 1)
            {
                int index = _folderListView.SelectedIndices[0];
                _moveUpButton.Enabled = index > 0;
                _moveDownButton.Enabled = index < _folderListView.Items.Count - 1;
            }
            else
            {
                _moveUpButton.Enabled = false;
                _moveDownButton.Enabled = false;
            }
        }

        // Drag and Drop
        private void FolderListView_ItemDrag(object? sender, ItemDragEventArgs e)
        {
            // Allow dragging lists and folders, but not headers
            var selectedItems = new List<ListViewItem>();
            foreach (ListViewItem item in _folderListView.SelectedItems)
            {
                // Skip headers
                if (item.Tag is string tagStr && tagStr == "ROOT_HEADER")
                {
                    continue;
                }
                
                selectedItems.Add(item);
            }

            if (selectedItems.Count > 0)
            {
                // Use DataObject to properly format the drag data
                DataObject data = new DataObject();
                data.SetData("FolderManagerItems", selectedItems);
                _folderListView.DoDragDrop(data, DragDropEffects.Move);
            }
        }

        private void FolderListView_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent("FolderManagerItems"))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void FolderListView_DragOver(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            Point cp = _folderListView.PointToClient(new Point(e.X, e.Y));
            ListViewItem? targetItem = _folderListView.GetItemAt(cp.X, cp.Y);

            // Default to no drop
            e.Effect = DragDropEffects.None;
            _insertionIndex = -1;

            if (targetItem != null)
            {
                // Check if dropping onto a folder (for moving into folder)
                if (targetItem.Tag is ListFolder)
                {
                    e.Effect = DragDropEffects.Move;
                    _insertionIndex = -1;  // No insertion line for folder merge
                }
                else if (targetItem.Tag is string || targetItem.Tag is Tuple<string, ListFolder>)
                {
                    // Reordering - show insertion line
                    e.Effect = DragDropEffects.Move;
                    
                    // Determine if inserting above or below target
                    Rectangle bounds = targetItem.Bounds;
                    int midPoint = bounds.Top + bounds.Height / 2;
                    
                    if (cp.Y < midPoint)
                    {
                        // Insert above
                        _insertionIndex = targetItem.Index;
                    }
                    else
                    {
                        // Insert below
                        _insertionIndex = targetItem.Index + 1;
                    }
                }
            }
            else
            {
                // Dropping in empty space at bottom - insert at end
                if (_folderListView.Items.Count > 0)
                {
                    e.Effect = DragDropEffects.Move;
                    _insertionIndex = _folderListView.Items.Count;
                }
            }

            // Force repaint to show insertion line
            _folderListView.Invalidate();
        }

        private void FolderListView_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            var draggedItems = e.Data.GetData("FolderManagerItems") as List<ListViewItem>;
            if (draggedItems == null || draggedItems.Count == 0) return;

            Point cp = _folderListView.PointToClient(new Point(e.X, e.Y));
            ListViewItem? targetItem = _folderListView.GetItemAt(cp.X, cp.Y);

            // Clear insertion line
            _insertionIndex = -1;
            _folderListView.Invalidate();

            // Check if dropping onto a folder (merge operation)
            if (targetItem != null && targetItem.Tag is ListFolder targetFolder)
            {
                // Move all dragged lists to the target folder
                foreach (var draggedItem in draggedItems)
                {
                    string listId;
                    ListFolder? oldFolder = null;

                    // Extract list ID and old folder
                    if (draggedItem.Tag is Tuple<string, ListFolder> tuple)
                    {
                        listId = tuple.Item1;
                        oldFolder = tuple.Item2;
                    }
                    else if (draggedItem.Tag is string id)
                    {
                        listId = id;
                    }
                    else
                    {
                        continue;
                    }

                    // Remove from old folder
                    if (oldFolder != null)
                    {
                        oldFolder.listIds.Remove(listId);
                    }

                    // Add to new folder (avoid duplicates)
                    if (!targetFolder.listIds.Contains(listId))
                    {
                        targetFolder.listIds.Add(listId);
                    }
                }

                // Reload the list view
                LoadFolderList();
            }
            else if (_insertionIndex >= 0)
            {
                // Reordering operation
                // Get indices of dragged items
                var draggedIndices = draggedItems.Select(item => item.Index).OrderBy(i => i).ToList();
                
                // Remove dragged items from ListView (in reverse order to maintain indices)
                foreach (var index in draggedIndices.OrderByDescending(i => i))
                {
                    _folderListView.Items.RemoveAt(index);
                }
                
                // Adjust insertion index based on removed items
                int adjustedIndex = _insertionIndex;
                foreach (var removedIndex in draggedIndices)
                {
                    if (removedIndex < _insertionIndex)
                    {
                        adjustedIndex--;
                    }
                }
                
                // Insert items at new location
                for (int i = 0; i < draggedItems.Count; i++)
                {
                    _folderListView.Items.Insert(adjustedIndex + i, draggedItems[i]);
                }
                
                // Rebuild config structure from ListView order
                RebuildConfigFromListView();
                
                // Reselect moved items
                foreach (var item in draggedItems)
                {
                    item.Selected = true;
                }
            }
        }

        private void NewFolderButton_Click(object? sender, EventArgs e)
        {
            string folderName = PromptForInput("New Folder", "Enter folder name:", $"Folder {_config.folders.Count + 1}");
            if (string.IsNullOrWhiteSpace(folderName)) return;

            var newFolder = new ListFolder
            {
                id = $"folder{Guid.NewGuid():N}",
                name = folderName,
                listIds = new List<string>(),
                isExpanded = true
            };

            _config.folders.Add(newFolder);
            LoadFolderList();
        }

        private void RenameFolderButton_Click(object? sender, EventArgs e)
        {
            if (_folderListView.SelectedItems.Count != 1) return;
            if (!(_folderListView.SelectedItems[0].Tag is ListFolder folder)) return;

            string newName = PromptForInput("Rename Folder", "Enter new folder name:", folder.name);
            if (string.IsNullOrWhiteSpace(newName)) return;

            folder.name = newName;
            LoadFolderList();
        }

        private void DeleteFolderButton_Click(object? sender, EventArgs e)
        {
            if (_folderListView.SelectedItems.Count != 1) return;
            if (!(_folderListView.SelectedItems[0].Tag is ListFolder folder)) return;

            var result = MessageBox.Show(
                $"Delete folder '{folder.name}'?\n\nLists in this folder will be moved to the root level.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _config.folders.Remove(folder);
                LoadFolderList();
            }
        }

        private void MoveToFolderButton_Click(object? sender, EventArgs e)
        {
            var selectedLists = GetSelectedListIds();
            if (selectedLists.Count == 0) return;

            // Show folder selection dialog
            var folderNames = _config.folders.Select(f => f.name).ToList();
            if (folderNames.Count == 0)
            {
                MessageBox.Show("No folders available. Create a folder first.", "No Folders",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string selectedFolder = ShowFolderSelectionDialog(folderNames);
            if (string.IsNullOrEmpty(selectedFolder)) return;

            var targetFolder = _config.folders.FirstOrDefault(f => f.name == selectedFolder);
            if (targetFolder == null) return;

            // Move all selected lists to target folder
            MoveListsToFolder(selectedLists, targetFolder);
            LoadFolderList();
        }

        private void MoveToRootButton_Click(object? sender, EventArgs e)
        {
            var selectedLists = GetSelectedListIds();
            if (selectedLists.Count == 0) return;

            MoveListsToFolder(selectedLists, null);
            LoadFolderList();
        }

        private List<Tuple<string, ListFolder?>> GetSelectedListIds()
        {
            var result = new List<Tuple<string, ListFolder?>>();

            foreach (ListViewItem item in _folderListView.SelectedItems)
            {
                if (item.Tag is Tuple<string, ListFolder> tuple)
                {
                    result.Add(new Tuple<string, ListFolder?>(tuple.Item1, tuple.Item2));
                }
                else if (item.Tag is string listId)
                {
                    result.Add(new Tuple<string, ListFolder?>(listId, null));
                }
            }

            return result;
        }

        private void MoveListsToFolder(List<Tuple<string, ListFolder?>> lists, ListFolder? targetFolder)
        {
            foreach (var tuple in lists)
            {
                string listId = tuple.Item1;
                ListFolder? oldFolder = tuple.Item2;

                // Remove from old folder
                if (oldFolder != null)
                {
                    oldFolder.listIds.Remove(listId);
                }

                // Add to new folder (if not root)
                if (targetFolder != null && !targetFolder.listIds.Contains(listId))
                {
                    targetFolder.listIds.Add(listId);
                }
            }
        }

        private void MoveUpButton_Click(object? sender, EventArgs e)
        {
            if (_folderListView.SelectedItems.Count != 1) return;
            int selectedIndex = _folderListView.SelectedIndices[0];
            if (selectedIndex <= 0) return;

            var item = _folderListView.Items[selectedIndex];
            _folderListView.Items.RemoveAt(selectedIndex);
            _folderListView.Items.Insert(selectedIndex - 1, item);
            item.Selected = true;
        }

        private void MoveDownButton_Click(object? sender, EventArgs e)
        {
            if (_folderListView.SelectedItems.Count != 1) return;
            int selectedIndex = _folderListView.SelectedIndices[0];
            if (selectedIndex >= _folderListView.Items.Count - 1) return;

            var item = _folderListView.Items[selectedIndex];
            _folderListView.Items.RemoveAt(selectedIndex);
            _folderListView.Items.Insert(selectedIndex + 1, item);
            item.Selected = true;
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            // Rebuild config from final ListView state
            RebuildConfigFromListView();
            
            // Clean up empty folders if any
            _config.folders.RemoveAll(f => f.listIds.Count == 0);
        }

        private void FolderListView_DragLeave(object? sender, EventArgs e)
        {
            // Clear insertion line when drag leaves
            _insertionIndex = -1;
            _folderListView.Invalidate();
        }

        private void FolderListView_Paint(object? sender, PaintEventArgs e)
        {
            // Draw insertion line if drag is active
            if (_insertionIndex >= 0 && _insertionIndex <= _folderListView.Items.Count)
            {
                int y;
                if (_insertionIndex < _folderListView.Items.Count)
                {
                    // Draw above the item at insertionIndex
                    var item = _folderListView.Items[_insertionIndex];
                    y = item.Bounds.Top;
                }
                else
                {
                    // Draw at bottom
                    var lastItem = _folderListView.Items[_folderListView.Items.Count - 1];
                    y = lastItem.Bounds.Bottom;
                }

                // Draw blue insertion line
                using (Pen pen = new Pen(Color.Blue, 2))
                {
                    e.Graphics.DrawLine(pen, 0, y, _folderListView.Width, y);
                }

                // Draw arrow indicators on both ends
                using (Brush brush = new SolidBrush(Color.Blue))
                {
                    Point[] leftArrow = new Point[]
                    {
                        new Point(5, y),
                        new Point(15, y - 5),
                        new Point(15, y + 5)
                    };
                    Point[] rightArrow = new Point[]
                    {
                        new Point(_folderListView.Width - 5, y),
                        new Point(_folderListView.Width - 15, y - 5),
                        new Point(_folderListView.Width - 15, y + 5)
                    };
                    e.Graphics.FillPolygon(brush, leftArrow);
                    e.Graphics.FillPolygon(brush, rightArrow);
                }
            }
        }

        private void RebuildConfigFromListView()
        {
            // Clear existing folder structure
            _config.folders.Clear();
            
            // Also rebuild listDisplayOrder
            _config.settings.listDisplayOrder = new List<string>();

            ListFolder? currentFolder = null;
            var rootLists = new List<string>();

            // Iterate through ListView items in display order
            foreach (ListViewItem item in _folderListView.Items)
            {
                if (item.Tag is ListFolder folder)
                {
                    // Start a new folder
                    if (currentFolder != null)
                    {
                        _config.folders.Add(currentFolder);
                    }
                    currentFolder = new ListFolder
                    {
                        id = folder.id,
                        name = folder.name,
                        listIds = new List<string>(),
                        isExpanded = folder.isExpanded
                    };
                }
                else if (item.Tag is Tuple<string, ListFolder> tuple)
                {
                    // List in a folder
                    string listId = tuple.Item1;
                    if (currentFolder != null)
                    {
                        currentFolder.listIds.Add(listId);
                    }
                    // Add to display order
                    _config.settings.listDisplayOrder.Add(listId);
                }
                else if (item.Tag is string listId)
                {
                    // Root list
                    rootLists.Add(listId);
                    // Add to display order
                    _config.settings.listDisplayOrder.Add(listId);
                }
                else if (item.Tag is string && (string)item.Tag == "ROOT_HEADER")
                {
                    // End current folder when we hit root section
                    if (currentFolder != null)
                    {
                        _config.folders.Add(currentFolder);
                        currentFolder = null;
                    }
                }
            }

            // Add last folder if exists
            if (currentFolder != null)
            {
                _config.folders.Add(currentFolder);
            }
        }

        private string ShowFolderSelectionDialog(List<string> folderNames)
        {
            Form selectForm = new Form()
            {
                Width = 400,
                Height = 250,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Select Folder",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label label = new Label() { Left = 20, Top = 20, Width = 350, Text = "Select a folder to move the selected list(s) to:" };
            ListBox listBox = new ListBox() { Left = 20, Top = 50, Width = 340, Height = 120 };
            foreach (var name in folderNames)
            {
                listBox.Items.Add(name);
            }

            Button okBtn = new Button() { Text = "OK", Left = 180, Width = 80, Top = 180, DialogResult = DialogResult.OK };
            Button cancelBtn = new Button() { Text = "Cancel", Left = 270, Width = 80, Top = 180, DialogResult = DialogResult.Cancel };

            selectForm.Controls.AddRange(new Control[] { label, listBox, okBtn, cancelBtn });
            selectForm.AcceptButton = okBtn;
            selectForm.CancelButton = cancelBtn;

            return selectForm.ShowDialog() == DialogResult.OK && listBox.SelectedItem != null
                ? listBox.SelectedItem.ToString() ?? ""
                : "";
        }

        private string PromptForInput(string title, string prompt, string defaultValue)
        {
            Form promptForm = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label textLabel = new Label() { Left = 20, Top = 20, Width = 350, Text = prompt };
            TextBox textBox = new TextBox() { Left = 20, Top = 45, Width = 340, Text = defaultValue };
            Button confirmation = new Button() { Text = "OK", Left = 200, Width = 80, Top = 75, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Cancel", Left = 290, Width = 80, Top = 75, DialogResult = DialogResult.Cancel };

            confirmation.Click += (sender, e) => { promptForm.Close(); };
            cancel.Click += (sender, e) => { promptForm.Close(); };

            promptForm.Controls.AddRange(new Control[] { textLabel, textBox, confirmation, cancel });
            promptForm.AcceptButton = confirmation;
            promptForm.CancelButton = cancel;

            return promptForm.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
