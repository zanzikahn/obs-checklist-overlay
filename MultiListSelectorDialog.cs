using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace OBSChecklistEditor
{
    public class MultiListSelectorDialog : Form
    {
        private CheckedListBox _listCheckBox = null!;
        private Dictionary<string, ChecklistData> _allLists = null!;
        private List<string> _selectedListIds = new List<string>();

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
            this.Text = "Select Lists to Display in Overlay";
            this.Width = 450;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var instructionLabel = new Label
            {
                Text = "Check the lists you want to display in the overlay:\n" +
                       "(Multiple lists will be stacked vertically)",
                Location = new Point(20, 15),
                Width = 400,
                Height = 40
            };

            _listCheckBox = new CheckedListBox
            {
                Location = new Point(20, 60),
                Width = 390,
                Height = 240,
                CheckOnClick = true
            };

            var selectAllButton = new Button
            {
                Text = "Select All",
                Location = new Point(20, 310),
                Width = 100
            };
            selectAllButton.Click += (s, e) =>
            {
                for (int i = 0; i < _listCheckBox.Items.Count; i++)
                {
                    _listCheckBox.SetItemChecked(i, true);
                }
            };

            var deselectAllButton = new Button
            {
                Text = "Deselect All",
                Location = new Point(130, 310),
                Width = 100
            };
            deselectAllButton.Click += (s, e) =>
            {
                for (int i = 0; i < _listCheckBox.Items.Count; i++)
                {
                    _listCheckBox.SetItemChecked(i, false);
                }
            };

            var okButton = new Button
            {
                Text = "OK",
                Location = new Point(250, 310),
                Width = 75,
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;

            var cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(335, 310),
                Width = 75,
                DialogResult = DialogResult.Cancel
            };

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            this.Controls.AddRange(new Control[] {
                instructionLabel, _listCheckBox, selectAllButton, deselectAllButton, okButton, cancelButton
            });
        }

        private void LoadLists()
        {
            foreach (var kvp in _allLists)
            {
                string displayText = $"{kvp.Key} - {kvp.Value.name}";
                int index = _listCheckBox.Items.Add(displayText);
                
                // Check if this list is currently selected
                if (_selectedListIds.Contains(kvp.Key))
                {
                    _listCheckBox.SetItemChecked(index, true);
                }
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            _selectedListIds.Clear();
            
            for (int i = 0; i < _listCheckBox.Items.Count; i++)
            {
                if (_listCheckBox.GetItemChecked(i))
                {
                    string displayText = _listCheckBox.Items[i].ToString() ?? "";
                    string listId = displayText.Split(new[] { " - " }, StringSplitOptions.None)[0];
                    _selectedListIds.Add(listId);
                }
            }

            if (_selectedListIds.Count == 0)
            {
                MessageBox.Show("Please select at least one list to display!", 
                    "No Lists Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
