using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace OBSChecklistEditor
{
    public class MultiListSelectorDialog : Form
    {
        private ListView _listView = null!;
        private Dictionary<string, ChecklistData> _allLists = null!;
        private List<string> _selectedListIds = new List<string>();
        private Button _moveUpButton = null!;
        private Button _moveDownButton = null!;

        public List<string> SelectedListIds => _selectedListIds;

        public MultiListSelectorDialog(Dictionary<string, ChecklistData> allLists, List<string> currentlySelected)
        {
            _allLists = allLists;
            _selectedListIds = new List<string>(currentlySelected);
            InitializeComponents();
            LoadLists();
        }

        private void InitializeComponents()
        {
            this.Text = "Select and Order Lists for Overlay";
            this.Width = 500;
            this.Height = 500;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var instructionLabel = new Label
            {
                Text = "Check the lists to display in overlay. Drag to reorder, or use buttons:\n" +
                       "(Lists will appear in the order shown below)",
                Location = new Point(20, 15),
                Width = 450,
                Height = 40
            };

            _listView = new ListView
            {
                Location = new Point(20, 60),
                Width = 340,
                Height = 300,
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true,
                AllowDrop = true,
                MultiSelect = false
            };

            _listView.Columns.Add("List ID", 120);
            _listView.Columns.Add("Display Name", 200);
            _listView.ItemChecked += ListView_ItemChecked;
            _listView.ItemDrag += ListView_ItemDrag;
            _listView.DragEnter += ListView_DragEnter;
            _listView.DragDrop += ListView_DragDrop;
            _listView.SelectedIndexChanged += ListView_SelectedIndexChanged;

            // Reorder buttons panel
            Panel buttonPanel = new Panel
            {
                Location = new Point(370, 60),
                Width = 100,
                Height = 300
            };

            _moveUpButton = new Button
            {
                Text = "Move Up ▲",
                Location = new Point(0, 0),
                Width = 100,
                Enabled = false
            };
            _moveUpButton.Click += MoveUpButton_Click;

            _moveDownButton = new Button
            {
                Text = "Move Down ▼",
                Location = new Point(0, 35),
                Width = 100,
                Enabled = false
            };
            _moveDownButton.Click += MoveDownButton_Click;

            buttonPanel.Controls.AddRange(new Control[] { _moveUpButton, _moveDownButton });

            var selectAllButton = new Button
            {
                Text = "Select All",
                Location = new Point(20, 370),
                Width = 100
            };
            selectAllButton.Click += (s, e) =>
            {
                foreach (ListViewItem item in _listView.Items)
                {
                    item.Checked = true;
                }
            };

            var deselectAllButton = new Button
            {
                Text = "Deselect All",
                Location = new Point(130, 370),
                Width = 100
            };
            deselectAllButton.Click += (s, e) =>
            {
                foreach (ListViewItem item in _listView.Items)
                {
                    item.Checked = false;
                }
            };

            var okButton = new Button
            {
                Text = "OK",
                Location = new Point(320, 370),
                Width = 75,
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;

            var cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(405, 370),
                Width = 75,
                DialogResult = DialogResult.Cancel
            };

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            this.Controls.AddRange(new Control[] {
                instructionLabel, _listView, buttonPanel, selectAllButton, deselectAllButton, okButton, cancelButton
            });
        }

        private void LoadLists()
        {
            _listView.Items.Clear();

            // First, add currently selected lists in their current order
            foreach (var listId in _selectedListIds)
            {
                if (_allLists.ContainsKey(listId))
                {
                    var item = new ListViewItem(listId);
                    item.SubItems.Add(_allLists[listId].name);
                    item.Checked = true;
                    item.Tag = listId;
                    _listView.Items.Add(item);
                }
            }

            // Then add remaining lists that aren't selected
            foreach (var kvp in _allLists.Where(kvp => !_selectedListIds.Contains(kvp.Key)))
            {
                var item = new ListViewItem(kvp.Key);
                item.SubItems.Add(kvp.Value.name);
                item.Checked = false;
                item.Tag = kvp.Key;
                _listView.Items.Add(item);
            }
        }

        private void ListView_ItemChecked(object? sender, ItemCheckedEventArgs e)
        {
            // No action needed - we'll collect checked items on OK
        }

        private void ListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool hasSelection = _listView.SelectedItems.Count > 0;
            _moveUpButton.Enabled = hasSelection && _listView.SelectedIndices[0] > 0;
            _moveDownButton.Enabled = hasSelection && _listView.SelectedIndices[0] < _listView.Items.Count - 1;
        }

        private void MoveUpButton_Click(object? sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count == 0) return;

            int selectedIndex = _listView.SelectedIndices[0];
            if (selectedIndex == 0) return;

            var item = _listView.Items[selectedIndex];
            _listView.Items.RemoveAt(selectedIndex);
            _listView.Items.Insert(selectedIndex - 1, item);
            _listView.Items[selectedIndex - 1].Selected = true;
        }

        private void MoveDownButton_Click(object? sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count == 0) return;

            int selectedIndex = _listView.SelectedIndices[0];
            if (selectedIndex >= _listView.Items.Count - 1) return;

            var item = _listView.Items[selectedIndex];
            _listView.Items.RemoveAt(selectedIndex);
            _listView.Items.Insert(selectedIndex + 1, item);
            _listView.Items[selectedIndex + 1].Selected = true;
        }

        // Drag and Drop event handlers
        private void ListView_ItemDrag(object? sender, ItemDragEventArgs e)
        {
            if (e.Item != null)
            {
                _listView.DoDragDrop(e.Item, DragDropEffects.Move);
            }
        }

        private void ListView_DragEnter(object? sender, DragEventArgs e)
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

        private void ListView_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            // Get the dragged item
            var draggedItem = e.Data.GetData(typeof(ListViewItem)) as ListViewItem;
            if (draggedItem == null) return;

            // Find the drop location
            Point cp = _listView.PointToClient(new Point(e.X, e.Y));
            ListViewItem? targetItem = _listView.GetItemAt(cp.X, cp.Y);

            if (targetItem == null) return;

            int draggedIndex = draggedItem.Index;
            int targetIndex = targetItem.Index;

            if (draggedIndex == targetIndex) return;

            // Remove and reinsert
            _listView.Items.RemoveAt(draggedIndex);

            // Adjust target index if needed
            if (draggedIndex < targetIndex)
            {
                targetIndex--;
            }

            _listView.Items.Insert(targetIndex, draggedItem);
            _listView.Items[targetIndex].Selected = true;
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            _selectedListIds.Clear();
            
            // Collect ALL items in their current display order (for dropdown)
            // But only mark checked ones as active for overlay
            List<string> allListsInOrder = new List<string>();
            
            foreach (ListViewItem item in _listView.Items)
            {
                string listId = item.Tag?.ToString() ?? "";
                if (!string.IsNullOrEmpty(listId))
                {
                    allListsInOrder.Add(listId);
                    
                    // Only add checked items to selectedListIds (for overlay)
                    if (item.Checked)
                    {
                        _selectedListIds.Add(listId);
                    }
                }
            }

            if (_selectedListIds.Count == 0)
            {
                MessageBox.Show("Please select at least one list to display!", 
                    "No Lists Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }
            
            // Store the complete order separately (will be accessed via a property)
            this.Tag = allListsInOrder;
        }
    }
}
