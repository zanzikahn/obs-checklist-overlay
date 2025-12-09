using System;
using System.Windows.Forms;
using System.Drawing;

namespace OBSChecklistEditor
{
    public class TaskEditDialog : Form
    {
        private TaskItem _taskItem = null!;

        private CheckBox _isSubHeaderCheckBox = null!;
        private Panel _normalTaskPanel = null!;
        
        private TextBox _nameTextBox = null!;
        private CheckBox _showCheckboxCheckBox = null!;
        private CheckBox _showProgressBarCheckBox = null!;
        private CheckBox _showCounterCheckBox = null!;
        private NumericUpDown _currentNumeric = null!;
        private NumericUpDown _totalNumeric = null!;
        private CheckBox _completedCheckBox = null!;
        private Button _okButton = null!;
        private Button _cancelButton = null!;

        public TaskEditDialog(TaskItem? existingTask = null)
        {
            _taskItem = existingTask ?? new TaskItem
            {
                name = "New Task",
                completed = false,
                showCheckbox = true,
                showProgressBar = true,
                showCounter = true,
                current = 0,
                total = 1,
                isSubHeader = false
            };

            InitializeComponents();
            LoadTaskToUI();
        }

        private void InitializeComponents()
        {
            this.Text = _taskItem.id == null ? "Add Task / Section Header" : "Edit Task / Section Header";
            this.Width = 500;
            this.Height = 470;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Task Name
            Label nameLabel = new Label
            {
                Text = "Name:",
                Location = new Point(20, 20),
                Width = 100
            };

            _nameTextBox = new TextBox
            {
                Location = new Point(130, 17),
                Width = 330
            };

            // Sub-Header Checkbox
            _isSubHeaderCheckBox = new CheckBox
            {
                Text = "This is a Section Header (not a task)",
                Location = new Point(20, 55),
                Width = 440,
                Font = new Font(this.Font, FontStyle.Bold)
            };
            _isSubHeaderCheckBox.CheckedChanged += IsSubHeaderCheckBox_CheckedChanged;

            var tooltip = new ToolTip();
            tooltip.SetToolTip(_isSubHeaderCheckBox, 
                "Section headers help organize your list into categories.\n" +
                "They appear as bold dividers and don't have progress tracking.");

            // Info label
            Label infoLabel = new Label
            {
                Text = "💡 Use section headers to divide your list into groups (e.g., \"Phase 1\", \"Resources\", etc.)",
                Location = new Point(40, 78),
                Width = 420,
                Height = 30,
                ForeColor = Color.Gray,
                Font = new Font(this.Font.FontFamily, 8)
            };

            // Normal Task Panel (hidden when sub-header is checked)
            _normalTaskPanel = new Panel
            {
                Location = new Point(20, 115),
                Width = 440,
                Height = 240
            };

            // Display Options
            GroupBox displayGroup = new GroupBox
            {
                Text = "Display Options",
                Location = new Point(0, 0),
                Width = 440,
                Height = 110
            };

            _showCheckboxCheckBox = new CheckBox
            {
                Text = "Show Checkbox",
                Location = new Point(15, 25),
                Width = 150,
                Checked = true
            };
            tooltip.SetToolTip(_showCheckboxCheckBox, "Display a checkbox next to the task name");

            _showProgressBarCheckBox = new CheckBox
            {
                Text = "Show Progress Bar",
                Location = new Point(15, 55),
                Width = 150,
                Checked = true
            };
            tooltip.SetToolTip(_showProgressBarCheckBox, "Display a visual progress bar");

            _showCounterCheckBox = new CheckBox
            {
                Text = "Show Counter (X/Y)",
                Location = new Point(200, 25),
                Width = 200,
                Checked = true
            };
            tooltip.SetToolTip(_showCounterCheckBox, "Show current/total numbers next to progress bar");

            displayGroup.Controls.AddRange(new Control[] {
                _showCheckboxCheckBox, _showProgressBarCheckBox, _showCounterCheckBox
            });

            // Progress Values
            GroupBox progressGroup = new GroupBox
            {
                Text = "Progress Values",
                Location = new Point(0, 120),
                Width = 440,
                Height = 80
            };

            Label currentLabel = new Label
            {
                Text = "Current:",
                Location = new Point(15, 30),
                Width = 60
            };

            _currentNumeric = new NumericUpDown
            {
                Location = new Point(80, 27),
                Width = 100,
                Maximum = 999999,
                Minimum = 0
            };
            tooltip.SetToolTip(_currentNumeric, "Current progress value");

            Label totalLabel = new Label
            {
                Text = "Total:",
                Location = new Point(200, 30),
                Width = 60
            };

            _totalNumeric = new NumericUpDown
            {
                Location = new Point(265, 27),
                Width = 100,
                Maximum = 999999,
                Minimum = 1,
                Value = 1
            };
            tooltip.SetToolTip(_totalNumeric, "Total/goal value");

            progressGroup.Controls.AddRange(new Control[] {
                currentLabel, _currentNumeric, totalLabel, _totalNumeric
            });

            _normalTaskPanel.Controls.AddRange(new Control[] { displayGroup, progressGroup });

            // Completed Checkbox (outside panel, always visible)
            _completedCheckBox = new CheckBox
            {
                Text = "Mark as Completed",
                Location = new Point(20, 370),
                Width = 200
            };
            tooltip.SetToolTip(_completedCheckBox, "Check to mark this item as complete");

            // Buttons
            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(290, 370),
                Width = 80,
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(380, 370),
                Width = 80,
                DialogResult = DialogResult.Cancel
            };

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;

            this.Controls.AddRange(new Control[] {
                nameLabel, _nameTextBox, _isSubHeaderCheckBox, infoLabel,
                _normalTaskPanel, _completedCheckBox, _okButton, _cancelButton
            });
        }

        private void IsSubHeaderCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            // Hide/show normal task options when sub-header is toggled
            _normalTaskPanel.Visible = !_isSubHeaderCheckBox.Checked;
            
            if (_isSubHeaderCheckBox.Checked)
            {
                // Sub-headers don't need completion tracking
                _completedCheckBox.Visible = false;
                _normalTaskPanel.Visible = false;
                // Keep height the same but content is hidden
            }
            else
            {
                _completedCheckBox.Visible = true;
                _normalTaskPanel.Visible = true;
            }
        }

        private void LoadTaskToUI()
        {
            _nameTextBox.Text = _taskItem.name;
            _isSubHeaderCheckBox.Checked = _taskItem.isSubHeader;
            _showCheckboxCheckBox.Checked = _taskItem.showCheckbox;
            _showProgressBarCheckBox.Checked = _taskItem.showProgressBar;
            _showCounterCheckBox.Checked = _taskItem.showCounter;
            _currentNumeric.Value = _taskItem.current;
            _totalNumeric.Value = _taskItem.total;
            _completedCheckBox.Checked = _taskItem.completed;

            // Trigger layout update
            IsSubHeaderCheckBox_CheckedChanged(null, EventArgs.Empty);
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MessageBox.Show(
                    _isSubHeaderCheckBox.Checked ? "Section header name cannot be empty!" : "Task name cannot be empty!", 
                    "Validation Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }
        }

        public TaskItem GetTaskItem()
        {
            return new TaskItem
            {
                id = _taskItem.id,
                name = _nameTextBox.Text.Trim(),
                completed = _isSubHeaderCheckBox.Checked ? false : _completedCheckBox.Checked,
                showCheckbox = _isSubHeaderCheckBox.Checked ? false : _showCheckboxCheckBox.Checked,
                showProgressBar = _isSubHeaderCheckBox.Checked ? false : _showProgressBarCheckBox.Checked,
                showCounter = _isSubHeaderCheckBox.Checked ? false : _showCounterCheckBox.Checked,
                current = _isSubHeaderCheckBox.Checked ? 0 : (int)_currentNumeric.Value,
                total = _isSubHeaderCheckBox.Checked ? 1 : (int)_totalNumeric.Value,
                isSubHeader = _isSubHeaderCheckBox.Checked
            };
        }
    }
}
