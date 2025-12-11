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

            // Add folders and their lists
            foreach (var folder in _config.folders)
            {
                // Add folder header
                var folderItem = new ListViewItem("📁 Folder");
                folderItem.SubItems.Add(folder.name);
                folderItem.Tag = folder;
                folderItem.Font = new Font(folderItem.Font, FontStyle.Bold);
                folderItem.BackColor = Color.LightGray;
                _folderListView.Items.Add(folderItem);

                // Add lists in this folder (indented visually)
                foreach (var listId in folder.listIds)
                {
                    if (_config.lists.ContainsKey(listId))
                    {
                        var listItem = new ListViewItem("  📄 List");
                        listItem.SubItems.Add($"  {listId} - {_config.lists[listId].name}");
                        listItem.Tag = new Tuple<string, ListFolder>(listId, folder);  // Store both list ID and parent folder
                        _folderListView.Items.Add(listItem);
                    }
                }
            }

            // Add root-level lists
            bool hasRootLists = false;
            foreach (var kvp in _config.lists)
            {
                if (!listToFolder.ContainsKey(kvp.Key))
                {
                    if (!hasRootLists)
                    {
                        // Add "Root" header
                        var rootHeader = new ListViewItem("📁 Root");
                        rootHeader.SubItems.Add("(Not in any folder)");
                        rootHeader.Tag = "ROOT_HEADER";
                        rootHeader.Font = new Font(_folderListView.Font, FontStyle.Bold);
                        rootHeader.BackColor = Color.LightYellow;
                        _folderListView.Items.Add(rootHeader);
                        hasRootLists = true;
                    }

                    var listItem = new ListViewItem("  📄 List");
                    listItem.SubItems.Add($"  {kvp.Key} - {kvp.Value.name}");
                    listItem.Tag = kvp.Key;  // Just the list ID for root lists
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

        // Drag and Drop
        private void FolderListView_ItemDrag(object? sender, ItemDragEventArgs e)
        {
            // Only allow dragging lists, not folders or headers
            var selectedItems = new List<ListViewItem>();
            foreach (ListViewItem item in _folderListView.SelectedItems)
            {
                if (item.Tag is string || item.Tag is Tuple<string, ListFolder>)
                {
                    selectedItems.Add(item);
                }
            }

            if (selectedItems.Count > 0)
            {
                _folderListView.DoDragDrop(selectedItems, DragDropEffects.Move);
            }
        }

        private void FolderListView_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(typeof(List<ListViewItem>)))
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

            // Only show valid drop if over a folder
            if (targetItem != null && targetItem.Tag is ListFolder)
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void FolderListView_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            var draggedItems = e.Data.GetData(typeof(List<ListViewItem>)) as List<ListViewItem>;
            if (draggedItems == null || draggedItems.Count == 0) return;

            Point cp = _folderListView.PointToClient(new Point(e.X, e.Y));
            ListViewItem? targetItem = _folderListView.GetItemAt(cp.X, cp.Y);

            // Only allow drop on folders
            if (targetItem == null || !(targetItem.Tag is ListFolder targetFolder)) return;

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
            // Config is already updated through direct modifications
            // Just need to clean up empty folders if any
            _config.folders.RemoveAll(f => f.listIds.Count == 0);
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
