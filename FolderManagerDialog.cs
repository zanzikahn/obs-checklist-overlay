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
        private TreeView _folderTreeView = null!;
        private Button _newFolderButton = null!;
        private Button _renameFolderButton = null!;
        private Button _deleteFolderButton = null!;
        private Button _moveUpButton = null!;
        private Button _moveDownButton = null!;
        private ContextMenuStrip _contextMenu = null!;

        public FolderManagerDialog(ChecklistConfig config)
        {
            _config = config;
            InitializeComponents();
            LoadFolderTree();
        }

        private void InitializeComponents()
        {
            this.Text = "Manage Folders";
            this.Width = 600;
            this.Height = 500;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var instructionLabel = new Label
            {
                Text = "Organize your lists into folders. Drag lists into folders or use the context menu.",
                Location = new Point(20, 15),
                Width = 550,
                Height = 30
            };

            _folderTreeView = new TreeView
            {
                Location = new Point(20, 50),
                Width = 420,
                Height = 340,
                AllowDrop = true,
                HideSelection = false
            };
            _folderTreeView.AfterSelect += FolderTreeView_AfterSelect;
            _folderTreeView.ItemDrag += FolderTreeView_ItemDrag;
            _folderTreeView.DragEnter += FolderTreeView_DragEnter;
            _folderTreeView.DragOver += FolderTreeView_DragOver;
            _folderTreeView.DragDrop += FolderTreeView_DragDrop;
            _folderTreeView.NodeMouseClick += FolderTreeView_NodeMouseClick;

            // Context menu for right-click options
            _contextMenu = new ContextMenuStrip();
            var moveToRootMenuItem = new ToolStripMenuItem("Move to Root");
            moveToRootMenuItem.Click += MoveToRootMenuItem_Click;
            _contextMenu.Items.Add(moveToRootMenuItem);

            // Button panel
            Panel buttonPanel = new Panel
            {
                Location = new Point(450, 50),
                Width = 120,
                Height = 340
            };

            _newFolderButton = new Button
            {
                Text = "New Folder",
                Location = new Point(0, 0),
                Width = 110
            };
            _newFolderButton.Click += NewFolderButton_Click;

            _renameFolderButton = new Button
            {
                Text = "Rename",
                Location = new Point(0, 35),
                Width = 110,
                Enabled = false
            };
            _renameFolderButton.Click += RenameFolderButton_Click;

            _deleteFolderButton = new Button
            {
                Text = "Delete Folder",
                Location = new Point(0, 70),
                Width = 110,
                Enabled = false
            };
            _deleteFolderButton.Click += DeleteFolderButton_Click;

            _moveUpButton = new Button
            {
                Text = "Move Up ▲",
                Location = new Point(0, 120),
                Width = 110,
                Enabled = false
            };
            _moveUpButton.Click += MoveUpButton_Click;

            _moveDownButton = new Button
            {
                Text = "Move Down ▼",
                Location = new Point(0, 155),
                Width = 110,
                Enabled = false
            };
            _moveDownButton.Click += MoveDownButton_Click;

            buttonPanel.Controls.AddRange(new Control[] {
                _newFolderButton, _renameFolderButton, _deleteFolderButton,
                _moveUpButton, _moveDownButton
            });

            var okButton = new Button
            {
                Text = "OK",
                Location = new Point(380, 400),
                Width = 85,
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;

            var cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(475, 400),
                Width = 85,
                DialogResult = DialogResult.Cancel
            };

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            this.Controls.AddRange(new Control[] {
                instructionLabel, _folderTreeView, buttonPanel, okButton, cancelButton
            });
        }

        private void LoadFolderTree()
        {
            _folderTreeView.Nodes.Clear();

            // Create a set of lists that are in folders
            var listsInFolders = new HashSet<string>();
            foreach (var folder in _config.folders)
            {
                foreach (var listId in folder.listIds)
                {
                    listsInFolders.Add(listId);
                }
            }

            // Add folders
            foreach (var folder in _config.folders)
            {
                var folderNode = new TreeNode(folder.name)
                {
                    Tag = folder,
                    ImageIndex = 0,
                    SelectedImageIndex = 0
                };

                // Add lists in this folder
                foreach (var listId in folder.listIds)
                {
                    if (_config.lists.ContainsKey(listId))
                    {
                        var listNode = new TreeNode($"{listId} - {_config.lists[listId].name}")
                        {
                            Tag = listId,
                            ImageIndex = 1,
                            SelectedImageIndex = 1
                        };
                        folderNode.Nodes.Add(listNode);
                    }
                }

                if (folder.isExpanded)
                {
                    folderNode.Expand();
                }

                _folderTreeView.Nodes.Add(folderNode);
            }

            // Add root-level lists (not in any folder)
            foreach (var kvp in _config.lists)
            {
                if (!listsInFolders.Contains(kvp.Key))
                {
                    var listNode = new TreeNode($"{kvp.Key} - {kvp.Value.name}")
                    {
                        Tag = kvp.Key,
                        ImageIndex = 1,
                        SelectedImageIndex = 1
                    };
                    _folderTreeView.Nodes.Add(listNode);
                }
            }
        }

        private void FolderTreeView_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;

            bool isFolder = e.Node.Tag is ListFolder;
            bool isList = e.Node.Tag is string;

            _renameFolderButton.Enabled = isFolder;
            _deleteFolderButton.Enabled = isFolder;

            // Enable move buttons if there are siblings
            var parent = e.Node.Parent;
            var siblings = parent == null ? _folderTreeView.Nodes : parent.Nodes;
            int index = siblings.IndexOf(e.Node);

            _moveUpButton.Enabled = index > 0;
            _moveDownButton.Enabled = index < siblings.Count - 1;
        }

        private void FolderTreeView_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.Node.Tag is string)
            {
                _folderTreeView.SelectedNode = e.Node;
                _contextMenu.Show(_folderTreeView, e.Location);
            }
        }

        // Drag and Drop
        private void FolderTreeView_ItemDrag(object? sender, ItemDragEventArgs e)
        {
            if (e.Item is TreeNode node)
            {
                _folderTreeView.DoDragDrop(node, DragDropEffects.Move);
            }
        }

        private void FolderTreeView_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void FolderTreeView_DragOver(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            Point targetPoint = _folderTreeView.PointToClient(new Point(e.X, e.Y));
            TreeNode? targetNode = _folderTreeView.GetNodeAt(targetPoint);

            if (targetNode != null)
            {
                _folderTreeView.SelectedNode = targetNode;
            }
        }

        private void FolderTreeView_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            var draggedNode = e.Data.GetData(typeof(TreeNode)) as TreeNode;
            if (draggedNode == null) return;

            Point targetPoint = _folderTreeView.PointToClient(new Point(e.X, e.Y));
            TreeNode? targetNode = _folderTreeView.GetNodeAt(targetPoint);

            // Can't drop on itself
            if (targetNode == draggedNode) return;

            // If dragging a list
            if (draggedNode.Tag is string listId)
            {
                // Drop on folder - move list into folder
                if (targetNode != null && targetNode.Tag is ListFolder targetFolder)
                {
                    // Remove from old location
                    if (draggedNode.Parent != null && draggedNode.Parent.Tag is ListFolder oldFolder)
                    {
                        oldFolder.listIds.Remove(listId);
                    }

                    // Add to new folder
                    if (!targetFolder.listIds.Contains(listId))
                    {
                        targetFolder.listIds.Add(listId);
                    }

                    draggedNode.Remove();
                    targetNode.Nodes.Add(draggedNode);
                    targetNode.Expand();
                }
                // Drop on root or another list - move to root
                else
                {
                    // Remove from old folder
                    if (draggedNode.Parent != null && draggedNode.Parent.Tag is ListFolder oldFolder)
                    {
                        oldFolder.listIds.Remove(listId);
                    }

                    draggedNode.Remove();
                    _folderTreeView.Nodes.Add(draggedNode);
                }
            }
            // If dragging a folder, allow reordering
            else if (draggedNode.Tag is ListFolder && targetNode != null && targetNode.Tag is ListFolder)
            {
                // Reorder folders
                int draggedIndex = _folderTreeView.Nodes.IndexOf(draggedNode);
                int targetIndex = _folderTreeView.Nodes.IndexOf(targetNode);

                if (draggedIndex != -1 && targetIndex != -1 && draggedIndex != targetIndex)
                {
                    draggedNode.Remove();
                    _folderTreeView.Nodes.Insert(targetIndex, draggedNode);
                    _folderTreeView.SelectedNode = draggedNode;
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
            LoadFolderTree();
        }

        private void RenameFolderButton_Click(object? sender, EventArgs e)
        {
            if (_folderTreeView.SelectedNode == null) return;
            if (!(_folderTreeView.SelectedNode.Tag is ListFolder folder)) return;

            string newName = PromptForInput("Rename Folder", "Enter new folder name:", folder.name);
            if (string.IsNullOrWhiteSpace(newName)) return;

            folder.name = newName;
            _folderTreeView.SelectedNode.Text = newName;
        }

        private void DeleteFolderButton_Click(object? sender, EventArgs e)
        {
            if (_folderTreeView.SelectedNode == null) return;
            if (!(_folderTreeView.SelectedNode.Tag is ListFolder folder)) return;

            var result = MessageBox.Show(
                $"Delete folder '{folder.name}'?\n\nLists in this folder will be moved to the root level.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _config.folders.Remove(folder);
                LoadFolderTree();
            }
        }

        private void MoveUpButton_Click(object? sender, EventArgs e)
        {
            if (_folderTreeView.SelectedNode == null) return;

            var node = _folderTreeView.SelectedNode;
            var parent = node.Parent;
            var siblings = parent == null ? _folderTreeView.Nodes : parent.Nodes;
            int index = siblings.IndexOf(node);

            if (index <= 0) return;

            siblings.RemoveAt(index);
            siblings.Insert(index - 1, node);
            _folderTreeView.SelectedNode = node;

            // Update folder order in config if it's a folder
            if (node.Tag is ListFolder && parent == null)
            {
                var folder = (ListFolder)node.Tag;
                _config.folders.Remove(folder);
                _config.folders.Insert(index - 1, folder);
            }
        }

        private void MoveDownButton_Click(object? sender, EventArgs e)
        {
            if (_folderTreeView.SelectedNode == null) return;

            var node = _folderTreeView.SelectedNode;
            var parent = node.Parent;
            var siblings = parent == null ? _folderTreeView.Nodes : parent.Nodes;
            int index = siblings.IndexOf(node);

            if (index >= siblings.Count - 1) return;

            siblings.RemoveAt(index);
            siblings.Insert(index + 1, node);
            _folderTreeView.SelectedNode = node;

            // Update folder order in config if it's a folder
            if (node.Tag is ListFolder && parent == null)
            {
                var folder = (ListFolder)node.Tag;
                _config.folders.Remove(folder);
                _config.folders.Insert(index + 1, folder);
            }
        }

        private void MoveToRootMenuItem_Click(object? sender, EventArgs e)
        {
            if (_folderTreeView.SelectedNode == null) return;
            if (!(_folderTreeView.SelectedNode.Tag is string listId)) return;

            var node = _folderTreeView.SelectedNode;

            // Remove from folder
            if (node.Parent != null && node.Parent.Tag is ListFolder folder)
            {
                folder.listIds.Remove(listId);
            }

            node.Remove();
            _folderTreeView.Nodes.Add(node);
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            // Save expanded states
            foreach (TreeNode node in _folderTreeView.Nodes)
            {
                if (node.Tag is ListFolder folder)
                {
                    folder.isExpanded = node.IsExpanded;
                }
            }

            // Rebuild folder structure from tree
            _config.folders.Clear();
            foreach (TreeNode node in _folderTreeView.Nodes)
            {
                if (node.Tag is ListFolder folder)
                {
                    // Rebuild list IDs from child nodes
                    folder.listIds.Clear();
                    foreach (TreeNode childNode in node.Nodes)
                    {
                        if (childNode.Tag is string listId)
                        {
                            folder.listIds.Add(listId);
                        }
                    }
                    _config.folders.Add(folder);
                }
            }
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
