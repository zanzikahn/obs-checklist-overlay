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
                Text = "Organize your lists into folders. Folders appear above Root. Lists not in folders go to Root.\n" +
                       "Use the buttons to manage folders and reorder items.",
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
                AllowDrop = false,
                HideSelection = false,
                MultiSelect = true
            };
            _folderListView.Columns.Add("Type", 80);
            _folderListView.Columns.Add("Name", 400);
            _folderListView.SelectedIndexChanged += FolderListView_SelectedIndexChanged;
            _folderListView.DoubleClick += FolderListView_DoubleClick;

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

            // Track which folders we've added
            var addedFolders = new HashSet<string>();
            
            // STEP 1: Add all folders and their lists (folders appear FIRST)
            foreach (var folder in _config.folders)
            {
                // Add folder header with expand/collapse indicator
                string expandIndicator = folder.isExpanded ? "▼" : "▶";
                var folderItem = new ListViewItem($"{expandIndicator} 📁 Folder");
                folderItem.SubItems.Add(folder.name);
                folderItem.Tag = folder;
                folderItem.Font = new Font(_folderListView.Font, FontStyle.Bold);
                folderItem.BackColor = Color.LightGray;
                _folderListView.Items.Add(folderItem);
                addedFolders.Add(folder.id);
                
                // Only add lists if folder is expanded
                if (folder.isExpanded)
                {
                    // Add lists in this folder (in displayOrder)
                    foreach (var listId in displayOrder)
                    {
                        if (folder.listIds.Contains(listId) && _config.lists.ContainsKey(listId))
                        {
                            var listItem = new ListViewItem("  📄 List");
                            listItem.SubItems.Add($"  {listId} - {_config.lists[listId].name}");
                            listItem.Tag = new Tuple<string, ListFolder>(listId, folder);
                            _folderListView.Items.Add(listItem);
                        }
                    }
                    
                    // Add any lists in folder not in displayOrder
                    foreach (var listId in folder.listIds)
                    {
                        if (!displayOrder.Contains(listId) && _config.lists.ContainsKey(listId))
                        {
                            var listItem = new ListViewItem("  📄 List");
                            listItem.SubItems.Add($"  {listId} - {_config.lists[listId].name}");
                            listItem.Tag = new Tuple<string, ListFolder>(listId, folder);
                            _folderListView.Items.Add(listItem);
                        }
                    }
                }
            }
            
            // STEP 2: Add Root header
            var rootHeader = new ListViewItem("📁 Root");
            rootHeader.SubItems.Add("(Not in any folder)");
            rootHeader.Tag = "ROOT_HEADER";
            rootHeader.Font = new Font(_folderListView.Font, FontStyle.Bold);
            rootHeader.BackColor = Color.LightYellow;
            _folderListView.Items.Add(rootHeader);
            
            // STEP 3: Add root lists (lists not in any folder) in displayOrder
            foreach (var listId in displayOrder)
            {
                if (!listToFolder.ContainsKey(listId) && _config.lists.ContainsKey(listId))
                {
                    var listItem = new ListViewItem("  📄 List");
                    listItem.SubItems.Add($"  {listId} - {_config.lists[listId].name}");
                    listItem.Tag = listId;
                    _folderListView.Items.Add(listItem);
                }
            }
            
            // Add any remaining root lists not in displayOrder
            foreach (var kvp in _config.lists)
            {
                if (!listToFolder.ContainsKey(kvp.Key) && !displayOrder.Contains(kvp.Key))
                {
                    var listItem = new ListViewItem("  📄 List");
                    listItem.SubItems.Add($"  {kvp.Key} - {kvp.Value.name}");
                    listItem.Tag = kvp.Key;
                    _folderListView.Items.Add(listItem);
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

        private void FolderListView_DoubleClick(object? sender, EventArgs e)
        {
            // Toggle folder expansion on double-click
            if (_folderListView.SelectedItems.Count != 1) return;
            
            var selectedItem = _folderListView.SelectedItems[0];
            if (selectedItem.Tag is ListFolder folder)
            {
                // Toggle expansion
                folder.isExpanded = !folder.isExpanded;
                
                // Reload to show/hide children
                LoadFolderList();
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
            
            // Don't allow moving ROOT_HEADER
            if (item.Tag is string tagStr && tagStr == "ROOT_HEADER")
            {
                return;
            }
            
            // If moving a folder header, move entire folder (header + all lists)
            if (item.Tag is ListFolder folder)
            {
                // Find all items in this folder
                var folderItems = new List<ListViewItem>();
                folderItems.Add(item); // Add header
                
                // Add all lists after header until we hit another folder/root
                for (int i = selectedIndex + 1; i < _folderListView.Items.Count; i++)
                {
                    var nextItem = _folderListView.Items[i];
                    // Stop if we hit another folder or root
                    if (nextItem.Tag is ListFolder || (nextItem.Tag is string s && s == "ROOT_HEADER"))
                    {
                        break;
                    }
                    folderItems.Add(nextItem);
                }
                
                // Don't allow moving above first position
                if (selectedIndex == 0) return;
                
                // Calculate target position (above previous folder/list group)
                int targetIndex = selectedIndex - 1;
                
                // If target is a list, find its folder header
                var targetItem = _folderListView.Items[targetIndex];
                if (targetItem.Tag is Tuple<string, ListFolder> || (targetItem.Tag is string && !(targetItem.Tag as string == "ROOT_HEADER")))
                {
                    // Find the folder header for this list
                    for (int i = targetIndex; i >= 0; i--)
                    {
                        if (_folderListView.Items[i].Tag is ListFolder)
                        {
                            targetIndex = i;
                            break;
                        }
                    }
                }
                
                // Remove all folder items
                foreach (var folderItem in folderItems)
                {
                    _folderListView.Items.Remove(folderItem);
                }
                
                // Insert at target position
                for (int i = 0; i < folderItems.Count; i++)
                {
                    _folderListView.Items.Insert(targetIndex + i, folderItems[i]);
                }
                
                folderItems[0].Selected = true;
                folderItems[0].EnsureVisible();
            }
            else
            {
                // Moving a regular list item
                _folderListView.Items.RemoveAt(selectedIndex);
                _folderListView.Items.Insert(selectedIndex - 1, item);
                item.Selected = true;
                item.EnsureVisible();
            }
            
            // Rebuild config to update listDisplayOrder
            RebuildConfigFromListView();
        }

        private void MoveDownButton_Click(object? sender, EventArgs e)
        {
            if (_folderListView.SelectedItems.Count != 1) return;
            int selectedIndex = _folderListView.SelectedIndices[0];
            if (selectedIndex >= _folderListView.Items.Count - 1) return;

            var item = _folderListView.Items[selectedIndex];
            
            // Don't allow moving ROOT_HEADER
            if (item.Tag is string tagStr && tagStr == "ROOT_HEADER")
            {
                return;
            }
            
            // If moving a folder header, move entire folder (header + all lists)
            if (item.Tag is ListFolder folder)
            {
                // Find all items in this folder
                var folderItems = new List<ListViewItem>();
                folderItems.Add(item); // Add header
                
                // Add all lists after header until we hit another folder/root
                for (int i = selectedIndex + 1; i < _folderListView.Items.Count; i++)
                {
                    var nextItem = _folderListView.Items[i];
                    // Stop if we hit another folder or root
                    if (nextItem.Tag is ListFolder || (nextItem.Tag is string rootTag && rootTag == "ROOT_HEADER"))
                    {
                        break;
                    }
                    folderItems.Add(nextItem);
                }
                
                // Calculate target position (below next folder/list group)
                int targetIndex = selectedIndex + folderItems.Count;
                
                // Don't allow moving past Root header
                if (targetIndex >= _folderListView.Items.Count)
                {
                    return;
                }
                
                var targetItem = _folderListView.Items[targetIndex];
                if (targetItem.Tag is string targetTag && targetTag == "ROOT_HEADER")
                {
                    // Can't move folder below Root
                    return;
                }
                
                // If target is another folder, move below all its items
                if (targetItem.Tag is ListFolder)
                {
                    // Count items in target folder
                    for (int i = targetIndex + 1; i < _folderListView.Items.Count; i++)
                    {
                        var nextItem = _folderListView.Items[i];
                        if (nextItem.Tag is ListFolder || (nextItem.Tag is string nextRootTag && nextRootTag == "ROOT_HEADER"))
                        {
                            break;
                        }
                        targetIndex++;
                    }
                    targetIndex++; // Move past last item
                }
                
                // Remove all folder items
                foreach (var folderItem in folderItems)
                {
                    _folderListView.Items.Remove(folderItem);
                }
                
                // Adjust target index after removal
                targetIndex = Math.Min(targetIndex, _folderListView.Items.Count);
                
                // Insert at target position
                for (int i = 0; i < folderItems.Count; i++)
                {
                    _folderListView.Items.Insert(targetIndex + i, folderItems[i]);
                }
                
                folderItems[0].Selected = true;
                folderItems[0].EnsureVisible();
            }
            else
            {
                // Moving a regular list item
                // Don't allow moving into Root from a folder
                var nextItem = _folderListView.Items[selectedIndex + 1];
                if (nextItem.Tag is string nextTag && nextTag == "ROOT_HEADER")
                {
                    return; // Can't jump folders
                }
                
                _folderListView.Items.RemoveAt(selectedIndex);
                _folderListView.Items.Insert(selectedIndex + 1, item);
                item.Selected = true;
                item.EnsureVisible();
            }
            
            // Rebuild config to update listDisplayOrder
            RebuildConfigFromListView();
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            // Rebuild config from final ListView state
            RebuildConfigFromListView();
            
            // Clean up empty folders if any
            _config.folders.RemoveAll(f => f.listIds.Count == 0);
        }

        private void RebuildConfigFromListView()
        {
            // IMPORTANT: Store original folders BEFORE clearing to preserve collapsed folder contents
            var originalFolders = new Dictionary<string, ListFolder>();
            foreach (var folder in _config.folders)
            {
                originalFolders[folder.id] = folder;
            }
            
            // Clear existing folder structure
            _config.folders.Clear();
            
            // Also rebuild listDisplayOrder
            _config.settings.listDisplayOrder = new List<string>();

            ListFolder? currentFolder = null;

            // Iterate through ListView items in display order
            foreach (ListViewItem item in _folderListView.Items)
            {
                if (item.Tag is ListFolder folder)
                {
                    // Save previous folder if exists
                    if (currentFolder != null)
                    {
                        _config.folders.Add(currentFolder);
                    }
                    
                    // Start a new folder - preserve ALL lists from original folder
                    var originalFolder = originalFolders.ContainsKey(folder.id) ? originalFolders[folder.id] : null;
                    
                    currentFolder = new ListFolder
                    {
                        id = folder.id,
                        name = folder.name,
                        listIds = originalFolder?.listIds ?? new List<string>(),  // Preserve all lists including hidden
                        isExpanded = folder.isExpanded
                    };
                }
                else if (item.Tag is Tuple<string, ListFolder> tuple)
                {
                    // List in a folder (visible) - just add to display order
                    string listId = tuple.Item1;
                    
                    // Add to display order
                    _config.settings.listDisplayOrder.Add(listId);
                }
                else if (item.Tag is string tagStr && tagStr == "ROOT_HEADER")
                {
                    // Save current folder before Root section
                    if (currentFolder != null)
                    {
                        _config.folders.Add(currentFolder);
                        currentFolder = null;
                    }
                }
                else if (item.Tag is string listId)
                {
                    // Root list
                    // Add to display order
                    _config.settings.listDisplayOrder.Add(listId);
                }
            }

            // Add last folder if exists (shouldn't happen with new structure)
            if (currentFolder != null)
            {
                _config.folders.Add(currentFolder);
            }
            
            // Add any lists that were in collapsed folders to listDisplayOrder
            foreach (var folder in _config.folders)
            {
                foreach (var listId in folder.listIds)
                {
                    if (!_config.settings.listDisplayOrder.Contains(listId))
                    {
                        _config.settings.listDisplayOrder.Add(listId);
                    }
                }
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
