using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace OBSChecklistEditor
{
    public class MainForm : Form
    {
        private ConfigManager _configManager = null!;
        private ChecklistConfig _config = null!;
        private bool _isRefreshing = false;  // Flag to prevent event loops during refresh
        
        // UI Controls
        private ComboBox _listSelector = null!;
        private TextBox _listNameTextBox = null!;
        private CheckBox _sequentialModeCheckBox = null!;
        private ListView _taskListView = null!;
        private Button _addTaskButton = null!;
        private Button _editTaskButton = null!;
        private Button _deleteTaskButton = null!;
        private Button _moveUpButton = null!;
        private Button _moveDownButton = null!;
        private Button _addListButton = null!;
        private Button _deleteListButton = null!;
        private Button _themeButton = null!;
        private Label _statusLabel = null!;

        public MainForm(string configPath)
        {
            Logger.Log("MainForm initializing...");
            _configManager = new ConfigManager(configPath);
            _config = _configManager.LoadConfig();
            
            InitializeComponents();
            LoadConfigToUI();
            Logger.Log("MainForm initialized successfully");
        }

        private void InitializeComponents()
        {
            this.Text = "OBS Checklist Editor";
            this.Width = 1000;  // Increased from 900 to 1000
            this.Height = 700;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Top Panel - List Management
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(10)
            };

            Label listLabel = new Label
            {
                Text = "Active List:",
                Location = new Point(10, 15),
                AutoSize = true
            };

            _listSelector = new ComboBox
            {
                Location = new Point(90, 12),
                Width = 250,  // Increased from 150 to 250
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _listSelector.SelectedIndexChanged += ListSelector_SelectedIndexChanged;

            Button renameListButton = new Button
            {
                Text = "✏️",
                Location = new Point(345, 11),  // Moved from 245 to 345
                Width = 35,
                Height = 23
            };
            renameListButton.Click += RenameListButton_Click;
            var renameListTooltip = new ToolTip();
            renameListTooltip.SetToolTip(renameListButton, "Rename the active list ID");

            _addListButton = new Button
            {
                Text = "New List",
                Location = new Point(400, 10),  // Moved from 300 to 400
                Width = 80
            };
            _addListButton.Click += AddListButton_Click;

            _deleteListButton = new Button
            {
                Text = "Delete List",
                Location = new Point(490, 10),  // Moved from 390 to 490
                Width = 90
            };
            _deleteListButton.Click += DeleteListButton_Click;

            Button multiListButton = new Button
            {
                Text = "📋 Multi-List",
                Location = new Point(590, 10),  // Moved from 490 to 590
                Width = 100
            };
            multiListButton.Click += MultiListButton_Click;
            var multiListTooltip = new ToolTip();
            multiListTooltip.SetToolTip(multiListButton, "Select multiple lists to display in overlay");

            Button manageFoldersButton = new Button
            {
                Text = "📁 Folders",
                Location = new Point(700, 10),
                Width = 90
            };
            manageFoldersButton.Click += ManageFoldersButton_Click;
            var foldersTooltip = new ToolTip();
            foldersTooltip.SetToolTip(manageFoldersButton, "Organize lists into folders");

            Label listNameLabel = new Label
            {
                Text = "List Name:",
                Location = new Point(10, 50),
                AutoSize = true
            };

            _listNameTextBox = new TextBox
            {
                Location = new Point(90, 47),
                Width = 200
            };
            _listNameTextBox.TextChanged += ListNameTextBox_TextChanged;

            _sequentialModeCheckBox = new CheckBox
            {
                Text = "Sequential Mode (Only show next incomplete task)",
                Location = new Point(300, 48),
                Width = 400,
                AutoSize = true
            };
            _sequentialModeCheckBox.CheckedChanged += SequentialModeCheckBox_CheckedChanged;

            _themeButton = new Button
            {
                Text = "Theme Settings",
                Location = new Point(800, 10),  // Moved from 700
                Width = 120
            };
            _themeButton.Click += ThemeButton_Click;

            Button autoScrollButton = new Button
            {
                Text = "🔄 Auto-Scroll",
                Location = new Point(800, 45),  // Moved from 700
                Width = 120
            };
            autoScrollButton.Click += AutoScrollButton_Click;
            var autoScrollTooltip = new ToolTip();
            autoScrollTooltip.SetToolTip(autoScrollButton, "Configure auto-scroll settings");

            topPanel.Controls.AddRange(new Control[] { 
                listLabel, _listSelector, renameListButton, _addListButton, _deleteListButton, multiListButton, manageFoldersButton,
                listNameLabel, _listNameTextBox, _sequentialModeCheckBox, _themeButton, autoScrollButton
            });

            // Middle Panel - Task List
            Panel middlePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            Label tasksLabel = new Label
            {
                Text = "Tasks:",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font(this.Font, FontStyle.Bold)
            };

            _taskListView = new ListView
            {
                Location = new Point(10, 35),
                Width = 650,
                Height = 450,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                AllowDrop = true  // Enable drag and drop
            };

            _taskListView.Columns.Add("Task Name", 200);
            _taskListView.Columns.Add("Checkbox", 80);
            _taskListView.Columns.Add("Progress Bar", 100);
            _taskListView.Columns.Add("Counter", 80);
            _taskListView.Columns.Add("Current/Total", 100);
            _taskListView.Columns.Add("Completed", 80);
            _taskListView.SelectedIndexChanged += TaskListView_SelectedIndexChanged;
            _taskListView.DoubleClick += EditTaskButton_Click;
            _taskListView.ItemDrag += TaskListView_ItemDrag;
            _taskListView.DragEnter += TaskListView_DragEnter;
            _taskListView.DragDrop += TaskListView_DragDrop;

            // Button Panel
            Panel buttonPanel = new Panel
            {
                Location = new Point(670, 35),
                Width = 200,
                Height = 450
            };

            _addTaskButton = new Button
            {
                Text = "Add Task",
                Location = new Point(0, 0),
                Width = 180
            };
            _addTaskButton.Click += AddTaskButton_Click;

            _editTaskButton = new Button
            {
                Text = "Edit Task",
                Location = new Point(0, 35),
                Width = 180,
                Enabled = false
            };
            _editTaskButton.Click += EditTaskButton_Click;

            _deleteTaskButton = new Button
            {
                Text = "Delete Task",
                Location = new Point(0, 70),
                Width = 180,
                Enabled = false
            };
            _deleteTaskButton.Click += DeleteTaskButton_Click;

            _moveUpButton = new Button
            {
                Text = "Move Up",
                Location = new Point(0, 120),
                Width = 180,
                Enabled = false
            };
            _moveUpButton.Click += MoveUpButton_Click;

            _moveDownButton = new Button
            {
                Text = "Move Down",
                Location = new Point(0, 155),
                Width = 180,
                Enabled = false
            };
            _moveDownButton.Click += MoveDownButton_Click;

            buttonPanel.Controls.AddRange(new Control[] {
                _addTaskButton, _editTaskButton, _deleteTaskButton,
                _moveUpButton, _moveDownButton
            });

            middlePanel.Controls.AddRange(new Control[] {
                tasksLabel, _taskListView, buttonPanel
            });

            // Bottom Panel - Status
            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                Padding = new Padding(10, 5, 10, 5)
            };

            _statusLabel = new Label
            {
                Text = $"Config: {_configManager.GetConfigPath()}",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            bottomPanel.Controls.Add(_statusLabel);

            // Add panels to form
            this.Controls.Add(middlePanel);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        private void LoadConfigToUI()
        {
            RefreshListSelector();
            
            // Load settings
            _sequentialModeCheckBox.Checked = _config.settings.sequentialMode;

            RefreshTaskList();
        }

        private void RefreshListSelector()
        {
            // Save current selection
            string? currentSelection = _listSelector.SelectedItem?.ToString();
            
            // Reload lists in the order defined by listDisplayOrder
            _listSelector.Items.Clear();
            
            // Use listDisplayOrder if available, otherwise fall back to activeListIds
            List<string> displayOrder = _config.settings.listDisplayOrder;
            if (displayOrder == null || displayOrder.Count == 0)
            {
                // Fallback: use activeListIds for backwards compatibility
                displayOrder = _config.settings.activeListIds ?? new List<string>();
            }
            
            // First, add lists from display order
            if (displayOrder.Count > 0)
            {
                foreach (var listId in displayOrder)
                {
                    if (_config.lists.ContainsKey(listId))
                    {
                        _listSelector.Items.Add(listId);
                    }
                }
            }
            
            // Then add any remaining lists that aren't in displayOrder (newly created lists)
            foreach (var listKey in _config.lists.Keys)
            {
                if (!_listSelector.Items.Contains(listKey))
                {
                    _listSelector.Items.Add(listKey);
                }
            }

            // Restore selection or select active list
            if (currentSelection != null && _listSelector.Items.Contains(currentSelection))
            {
                _listSelector.SelectedItem = currentSelection;
            }
            else if (_listSelector.Items.Contains(_config.settings.activeListId))
            {
                _listSelector.SelectedItem = _config.settings.activeListId;
            }
            else if (_listSelector.Items.Count > 0)
            {
                _listSelector.SelectedIndex = 0;
            }
        }

        private void RefreshTaskList()
        {
            _isRefreshing = true;  // Set flag to prevent TextChanged from saving during refresh
            
            _taskListView.Items.Clear();

            if (_listSelector.SelectedItem == null)
            {
                _isRefreshing = false;
                return;
            }

            var activeListId = _listSelector.SelectedItem.ToString();
            if (activeListId == null || !_config.lists.ContainsKey(activeListId))
            {
                _isRefreshing = false;
                return;
            }

            var activeList = _config.lists[activeListId];
            _listNameTextBox.Text = activeList.name;

            foreach (var task in activeList.items)
            {
                var item = new ListViewItem(task.name);
                
                if (task.isSubHeader)
                {
                    // Display sub-header differently
                    item.SubItems.Add("[HEADER]");
                    item.SubItems.Add("-");
                    item.SubItems.Add("-");
                    item.SubItems.Add("-");
                    item.SubItems.Add("-");
                    item.Font = new Font(item.Font, FontStyle.Bold);
                    item.ForeColor = Color.DarkGoldenrod;
                }
                else
                {
                    // Regular task
                    item.SubItems.Add(task.showCheckbox ? "Yes" : "No");
                    item.SubItems.Add(task.showProgressBar ? "Yes" : "No");
                    item.SubItems.Add(task.showCounter ? "Yes" : "No");
                    item.SubItems.Add($"{task.current}/{task.total}");
                    item.SubItems.Add(task.completed ? "Yes" : "No");
                }
                
                item.Tag = task;
                _taskListView.Items.Add(item);
            }
            
            _isRefreshing = false;  // Clear flag after refresh is complete
        }

        private void SaveConfig()
        {
            if (_configManager.SaveConfig(_config))
            {
                _statusLabel.Text = $"Saved at {DateTime.Now:HH:mm:ss}";
            }
            else
            {
                _statusLabel.Text = "Error saving config!";
            }
        }

        // Event Handlers
        private void ListSelector_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_listSelector.SelectedItem != null)
            {
                var activeListId = _listSelector.SelectedItem.ToString();
                if (activeListId != null)
                {
                    _config.settings.activeListId = activeListId;
                    SaveConfig();
                    RefreshTaskList();
                }
            }
        }

        private void ListNameTextBox_TextChanged(object? sender, EventArgs e)
        {
            // Don't save if we're in the middle of refreshing the UI
            if (_isRefreshing) return;
            
            if (_listSelector.SelectedItem == null) return;

            var activeListId = _listSelector.SelectedItem.ToString();
            if (activeListId == null || !_config.lists.ContainsKey(activeListId)) return;

            // Update the list's display name
            _config.lists[activeListId].name = _listNameTextBox.Text;
            SaveConfig();
        }

        private void RenameListButton_Click(object? sender, EventArgs e)
        {
            if (_listSelector.SelectedItem == null) return;

            var currentListId = _listSelector.SelectedItem.ToString();
            if (currentListId == null || !_config.lists.ContainsKey(currentListId)) return;

            // Prompt for new list ID
            string newListId = PromptForInput("Rename List ID", 
                $"Enter new ID for '{currentListId}':", currentListId);

            if (string.IsNullOrWhiteSpace(newListId))
            {
                return; // User cancelled
            }

            // Validate new ID
            if (newListId == currentListId)
            {
                MessageBox.Show("New ID is the same as the current ID.", "No Change",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_config.lists.ContainsKey(newListId))
            {
                MessageBox.Show($"List ID '{newListId}' already exists!", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Rename the list ID
            var listData = _config.lists[currentListId];
            _config.lists.Remove(currentListId);
            _config.lists[newListId] = listData;

            // Update active list ID if this was the active list
            if (_config.settings.activeListId == currentListId)
            {
                _config.settings.activeListId = newListId;
            }

            SaveConfig();
            RefreshListSelector();
            _listSelector.SelectedItem = newListId;

            MessageBox.Show($"List ID renamed from '{currentListId}' to '{newListId}'", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void SequentialModeCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            _config.settings.sequentialMode = _sequentialModeCheckBox.Checked;
            SaveConfig();
        }

        private void AddListButton_Click(object? sender, EventArgs e)
        {
            string listId = $"list{_config.lists.Count + 1}";
            string listName = $"New List {_config.lists.Count + 1}";

            _config.lists[listId] = new ChecklistData
            {
                name = listName,
                items = new List<TaskItem>()
            };

            _listSelector.Items.Add(listId);
            _listSelector.SelectedItem = listId;
            SaveConfig();
        }

        private void DeleteListButton_Click(object? sender, EventArgs e)
        {
            if (_listSelector.SelectedItem == null) return;
            if (_config.lists.Count <= 1)
            {
                MessageBox.Show("Cannot delete the last list!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var activeListId = _listSelector.SelectedItem.ToString();
            if (activeListId == null || !_config.lists.ContainsKey(activeListId)) return;
            
            var result = MessageBox.Show($"Delete list '{_config.lists[activeListId].name}'?", 
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _config.lists.Remove(activeListId);
                _listSelector.Items.Remove(activeListId);
                if (_listSelector.Items.Count > 0)
                {
                    _listSelector.SelectedIndex = 0;
                }
                SaveConfig();
            }
        }

        private void AddTaskButton_Click(object? sender, EventArgs e)
        {
            if (_listSelector.SelectedItem == null) return;

            var activeListId = _listSelector.SelectedItem.ToString();
            if (activeListId == null) return;
            
            using (var dialog = new TaskEditDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var newTask = dialog.GetTaskItem();
                    newTask.id = $"task{Guid.NewGuid():N}";
                    _config.lists[activeListId].items.Add(newTask);
                    SaveConfig();
                    RefreshTaskList();
                }
            }
        }

        private void EditTaskButton_Click(object? sender, EventArgs e)
        {
            if (_taskListView.SelectedItems.Count == 0) return;
            if (_listSelector.SelectedItem == null) return;

            var activeListId = _listSelector.SelectedItem.ToString();
            if (activeListId == null) return;
            
            var selectedTask = _taskListView.SelectedItems[0].Tag as TaskItem;
            if (selectedTask == null) return;
            
            int index = _config.lists[activeListId].items.IndexOf(selectedTask);

            using (var dialog = new TaskEditDialog(selectedTask))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _config.lists[activeListId].items[index] = dialog.GetTaskItem();
                    SaveConfig();
                    RefreshTaskList();
                }
            }
        }

        private void DeleteTaskButton_Click(object? sender, EventArgs e)
        {
            if (_taskListView.SelectedItems.Count == 0) return;
            if (_listSelector.SelectedItem == null) return;

            var activeListId = _listSelector.SelectedItem.ToString();
            if (activeListId == null) return;
            
            var selectedTask = _taskListView.SelectedItems[0].Tag as TaskItem;
            if (selectedTask == null) return;

            var result = MessageBox.Show($"Delete task '{selectedTask.name}'?", 
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _config.lists[activeListId].items.Remove(selectedTask);
                SaveConfig();
                RefreshTaskList();
            }
        }

        private void MoveUpButton_Click(object? sender, EventArgs e)
        {
            if (_taskListView.SelectedItems.Count == 0) return;
            if (_listSelector.SelectedItem == null) return;

            int selectedIndex = _taskListView.SelectedIndices[0];
            if (selectedIndex == 0) return;

            var activeListId = _listSelector.SelectedItem.ToString();
            if (activeListId == null) return;
            
            var items = _config.lists[activeListId].items;
            var temp = items[selectedIndex];
            items[selectedIndex] = items[selectedIndex - 1];
            items[selectedIndex - 1] = temp;

            SaveConfig();
            RefreshTaskList();
            _taskListView.Items[selectedIndex - 1].Selected = true;
        }

        private void MoveDownButton_Click(object? sender, EventArgs e)
        {
            if (_taskListView.SelectedItems.Count == 0) return;
            if (_listSelector.SelectedItem == null) return;

            int selectedIndex = _taskListView.SelectedIndices[0];
            var activeListId = _listSelector.SelectedItem.ToString();
            if (activeListId == null) return;
            
            var items = _config.lists[activeListId].items;

            if (selectedIndex >= items.Count - 1) return;

            var temp = items[selectedIndex];
            items[selectedIndex] = items[selectedIndex + 1];
            items[selectedIndex + 1] = temp;

            SaveConfig();
            RefreshTaskList();
            _taskListView.Items[selectedIndex + 1].Selected = true;
        }

        private void TaskListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool hasSelection = _taskListView.SelectedItems.Count > 0;
            _editTaskButton.Enabled = hasSelection;
            _deleteTaskButton.Enabled = hasSelection;
            _moveUpButton.Enabled = hasSelection && _taskListView.SelectedIndices[0] > 0;
            _moveDownButton.Enabled = hasSelection && _taskListView.SelectedIndices[0] < _taskListView.Items.Count - 1;
        }

        // Drag and Drop event handlers for tasks
        private void TaskListView_ItemDrag(object? sender, ItemDragEventArgs e)
        {
            if (e.Item != null)
            {
                _taskListView.DoDragDrop(e.Item, DragDropEffects.Move);
            }
        }

        private void TaskListView_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void TaskListView_DragDrop(object? sender, DragEventArgs e)
        {
            if (_listSelector.SelectedItem == null) return;
            if (e.Data == null) return;

            var activeListId = _listSelector.SelectedItem.ToString();
            if (activeListId == null || !_config.lists.ContainsKey(activeListId)) return;

            // Get the dragged item
            var draggedItem = e.Data.GetData(typeof(ListViewItem)) as ListViewItem;
            if (draggedItem == null) return;

            // Find the drop location
            Point cp = _taskListView.PointToClient(new Point(e.X, e.Y));
            ListViewItem? targetItem = _taskListView.GetItemAt(cp.X, cp.Y);

            if (targetItem == null) return;

            int draggedIndex = draggedItem.Index;
            int targetIndex = targetItem.Index;

            if (draggedIndex == targetIndex) return;

            // Reorder in the config
            var items = _config.lists[activeListId].items;
            var task = items[draggedIndex];
            items.RemoveAt(draggedIndex);

            // Adjust target index if needed
            if (draggedIndex < targetIndex)
            {
                targetIndex--;
            }

            items.Insert(targetIndex, task);

            SaveConfig();
            RefreshTaskList();

            // Reselect the moved item
            if (targetIndex < _taskListView.Items.Count)
            {
                _taskListView.Items[targetIndex].Selected = true;
            }
        }

        private void MultiListButton_Click(object? sender, EventArgs e)
        {
            // Ensure activeListIds is initialized
            if (_config.settings.activeListIds == null || _config.settings.activeListIds.Count == 0)
            {
                _config.settings.activeListIds = new List<string> { _config.settings.activeListId };
            }

            using (var dialog = new MultiListSelectorDialog(_config.lists, _config.settings.activeListIds))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the checked lists (for overlay display)
                    _config.settings.activeListIds = dialog.SelectedListIds;
                    
                    // Get the complete display order (all lists, checked and unchecked)
                    var allListsInOrder = dialog.Tag as List<string>;
                    if (allListsInOrder != null)
                    {
                        _config.settings.listDisplayOrder = allListsInOrder;
                    }
                    
                    // Update single activeListId for backwards compatibility
                    if (_config.settings.activeListIds.Count > 0)
                    {
                        _config.settings.activeListId = _config.settings.activeListIds[0];
                    }
                    
                    SaveConfig();
                    RefreshListSelector();  // Refresh dropdown to reflect new order
                    MessageBox.Show($"{_config.settings.activeListIds.Count} list(s) selected for overlay display.\n" +
                        "The Active List dropdown order has been updated.",
                        "Multi-List Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ThemeButton_Click(object? sender, EventArgs e)
        {
            using (var dialog = new EnhancedThemeDialog(_config.theme, _config))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _config.theme = dialog.GetTheme();
                    SaveConfig();
                }
            }
        }

        private void AutoScrollButton_Click(object? sender, EventArgs e)
        {
            using (var dialog = new AutoScrollDialog(_config))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SaveConfig();
                    MessageBox.Show("Auto-scroll settings updated!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ManageFoldersButton_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Folder management feature:\n\n" +
                "• Create folders to organize your lists\n" +
                "• Drag lists into folders\n" +
                "• Folders appear in the Active List dropdown\n\n" +
                "This feature is coming soon!",
                "Folders - Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // TODO: Implement FolderManagerDialog
            // using (var dialog = new FolderManagerDialog(_config))
            // {
            //     if (dialog.ShowDialog() == DialogResult.OK)
            //     {
            //         SaveConfig();
            //         RefreshListSelector();
            //     }
            // }
        }
    }
}
