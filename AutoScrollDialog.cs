using System;
using System.Windows.Forms;
using System.Drawing;

namespace OBSChecklistEditor
{
    public class AutoScrollDialog : Form
    {
        private readonly ChecklistConfig _config;
        
        // Controls
        private CheckBox _enabledCheckBox = null!;
        private NumericUpDown _viewportHeightUpDown = null!;
        private NumericUpDown _speedUpDown = null!;
        private NumericUpDown _pauseBottomUpDown = null!;
        private CheckBox _reverseCheckBox = null!;
        private CheckBox _alternateCheckBox = null!;
        
        private Button _okButton = null!;
        private Button _cancelButton = null!;
        private Button _resetButton = null!;

        public AutoScrollDialog(ChecklistConfig config)
        {
            _config = config;
            InitializeComponents();
            LoadSettingsToUI();
        }

        private void InitializeComponents()
        {
            // Form settings
            Text = "Auto-Scroll Settings";
            Size = new Size(450, 420);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            int yPos = 20;
            int labelX = 20;
            int controlX = 200;
            int controlWidth = 210;
            int lineHeight = 35;

            // Enable checkbox
            _enabledCheckBox = new CheckBox
            {
                Location = new Point(labelX, yPos),
                Size = new Size(controlWidth + controlX - labelX, 20),
                Text = "Enable Auto-Scroll",
                Font = new Font(Font, FontStyle.Bold)
            };
            _enabledCheckBox.CheckedChanged += EnabledCheckBox_CheckedChanged;
            Controls.Add(_enabledCheckBox);
            yPos += lineHeight;

            // Separator
            var separator1 = new Label
            {
                Location = new Point(labelX, yPos),
                Size = new Size(controlWidth + controlX - labelX, 2),
                BorderStyle = BorderStyle.Fixed3D
            };
            Controls.Add(separator1);
            yPos += 15;

            // Viewport height
            var viewportHeightLabel = new Label
            {
                Location = new Point(labelX, yPos + 3),
                Size = new Size(170, 20),
                Text = "Viewport Height (px):",
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(viewportHeightLabel);

            _viewportHeightUpDown = new NumericUpDown
            {
                Location = new Point(controlX, yPos),
                Size = new Size(controlWidth, 25),
                Minimum = 200,
                Maximum = 10000,
                Value = 600,
                Increment = 100
            };
            Controls.Add(_viewportHeightUpDown);

            var viewportHeightHelp = new Label
            {
                Location = new Point(labelX + 20, yPos + 25),
                Size = new Size(controlWidth + 180, 15),
                Text = "Height of scrolling viewport - can exceed OBS source height (default: 600)",
                ForeColor = Color.Gray,
                Font = new Font(Font.FontFamily, 7.5f)
            };
            Controls.Add(viewportHeightHelp);
            yPos += lineHeight + 20;

            // Scroll speed
            var speedLabel = new Label
            {
                Location = new Point(labelX, yPos + 3),
                Size = new Size(170, 20),
                Text = "Scroll Speed:",
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(speedLabel);

            _speedUpDown = new NumericUpDown
            {
                Location = new Point(controlX, yPos),
                Size = new Size(controlWidth, 25),
                Minimum = 10,
                Maximum = 300,
                Value = 30
            };
            Controls.Add(_speedUpDown);

            var speedHelp = new Label
            {
                Location = new Point(labelX + 20, yPos + 25),
                Size = new Size(controlWidth + 180, 15),
                Text = "Higher = faster scroll (default: 30, max: 300)",
                ForeColor = Color.Gray,
                Font = new Font(Font.FontFamily, 7.5f)
            };
            Controls.Add(speedHelp);
            yPos += lineHeight + 20;

            // Pause between lists (for alternate lists mode)
            var pauseBottomLabel = new Label
            {
                Location = new Point(labelX, yPos + 3),
                Size = new Size(170, 20),
                Text = "Pause Between Lists (ms):",
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(pauseBottomLabel);

            _pauseBottomUpDown = new NumericUpDown
            {
                Location = new Point(controlX, yPos),
                Size = new Size(controlWidth, 25),
                Minimum = 0,
                Maximum = 10000,
                Value = 3000,
                Increment = 500
            };
            Controls.Add(_pauseBottomUpDown);

            var pauseBottomHelp = new Label
            {
                Location = new Point(labelX + 20, yPos + 25),
                Size = new Size(controlWidth + 180, 15),
                Text = "Pause when switching between lists in Alternate Lists mode (default: 3000ms = 3s)",
                ForeColor = Color.Gray,
                Font = new Font(Font.FontFamily, 7.5f)
            };
            Controls.Add(pauseBottomHelp);
            yPos += lineHeight + 20;

            // Separator
            var separator2 = new Label
            {
                Location = new Point(labelX, yPos),
                Size = new Size(controlWidth + controlX - labelX, 2),
                BorderStyle = BorderStyle.Fixed3D
            };
            Controls.Add(separator2);
            yPos += 15;

            // Reverse scroll
            _reverseCheckBox = new CheckBox
            {
                Location = new Point(labelX, yPos),
                Size = new Size(controlWidth + controlX - labelX, 20),
                Text = "Reverse Scroll (scroll up instead of down)"
            };
            Controls.Add(_reverseCheckBox);
            yPos += lineHeight;

            // Alternate lists
            _alternateCheckBox = new CheckBox
            {
                Location = new Point(labelX, yPos),
                Size = new Size(controlWidth + controlX - labelX, 20),
                Text = "Alternate Lists (cycle through lists one at a time)"
            };
            Controls.Add(_alternateCheckBox);
            yPos += lineHeight + 10;

            // Buttons
            _resetButton = new Button
            {
                Location = new Point(20, yPos),
                Size = new Size(100, 30),
                Text = "Reset Defaults"
            };
            _resetButton.Click += ResetButton_Click;
            Controls.Add(_resetButton);

            _okButton = new Button
            {
                Location = new Point(240, yPos),
                Size = new Size(90, 30),
                Text = "OK",
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;
            Controls.Add(_okButton);

            _cancelButton = new Button
            {
                Location = new Point(340, yPos),
                Size = new Size(90, 30),
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(_cancelButton);

            AcceptButton = _okButton;
            CancelButton = _cancelButton;
        }

        private void LoadSettingsToUI()
        {
            _enabledCheckBox.Checked = _config.settings.autoScrollEnabled;
            _viewportHeightUpDown.Value = _config.settings.scrollViewportHeight;
            _speedUpDown.Value = _config.settings.autoScrollSpeed;
            _pauseBottomUpDown.Value = _config.settings.pauseTimeBottom;
            _reverseCheckBox.Checked = _config.settings.reverseScroll;
            _alternateCheckBox.Checked = _config.settings.alternateLists;

            UpdateControlStates();
        }

        private void EnabledCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateControlStates();
        }

        private void UpdateControlStates()
        {
            bool enabled = _enabledCheckBox.Checked;
            _viewportHeightUpDown.Enabled = enabled;
            _speedUpDown.Enabled = enabled;
            _pauseBottomUpDown.Enabled = enabled;
            _reverseCheckBox.Enabled = enabled;
            _alternateCheckBox.Enabled = enabled;
        }

        private void ResetButton_Click(object? sender, EventArgs e)
        {
            _enabledCheckBox.Checked = false;
            _viewportHeightUpDown.Value = 600;
            _speedUpDown.Value = 30;
            _pauseBottomUpDown.Value = 3000;
            _reverseCheckBox.Checked = false;
            _alternateCheckBox.Checked = false;
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            // Save settings to config
            _config.settings.autoScrollEnabled = _enabledCheckBox.Checked;
            _config.settings.scrollViewportHeight = (int)_viewportHeightUpDown.Value;
            _config.settings.autoScrollSpeed = (int)_speedUpDown.Value;
            _config.settings.pauseTimeBottom = (int)_pauseBottomUpDown.Value;
            _config.settings.reverseScroll = _reverseCheckBox.Checked;
            _config.settings.alternateLists = _alternateCheckBox.Checked;
        }
    }
}
