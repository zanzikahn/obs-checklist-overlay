using System;
using System.Windows.Forms;
using System.Drawing;

namespace OBSChecklistEditor
{
    public class ThemeDialog : Form
    {
        private Theme _theme = null!;

        private TextBox _backgroundColorTextBox = null!;
        private TextBox _textColorTextBox = null!;
        private TextBox _progressBarColorTextBox = null!;
        private TextBox _progressBarBackgroundTextBox = null!;
        private TextBox _checkboxColorTextBox = null!;
        private TextBox _fontFamilyTextBox = null!;
        private TextBox _fontSizeTextBox = null!;
        private TextBox _borderRadiusTextBox = null!;

        private Button _okButton = null!;
        private Button _cancelButton = null!;
        private Button _resetButton = null!;

        public ThemeDialog(Theme currentTheme)
        {
            _theme = currentTheme;
            InitializeComponents();
            LoadThemeToUI();
        }

        private void InitializeComponents()
        {
            this.Text = "Theme Settings";
            this.Width = 500;
            this.Height = 450;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = 20;
            int labelWidth = 180;
            int textBoxX = 200;
            int textBoxWidth = 250;

            // Color Settings
            GroupBox colorGroup = new GroupBox
            {
                Text = "Colors (CSS format: #RRGGBB or rgba(r,g,b,a))",
                Location = new Point(20, yPos),
                Width = 450,
                Height = 240
            };

            AddLabelAndTextBox(colorGroup, "Background Color:", ref _backgroundColorTextBox, 
                20, textBoxX - 20, textBoxWidth);
            AddLabelAndTextBox(colorGroup, "Text Color:", ref _textColorTextBox, 
                55, textBoxX - 20, textBoxWidth);
            AddLabelAndTextBox(colorGroup, "Progress Bar Color:", ref _progressBarColorTextBox, 
                90, textBoxX - 20, textBoxWidth);
            AddLabelAndTextBox(colorGroup, "Progress Bar Background:", ref _progressBarBackgroundTextBox, 
                125, textBoxX - 20, textBoxWidth);
            AddLabelAndTextBox(colorGroup, "Checkbox Color:", ref _checkboxColorTextBox, 
                160, textBoxX - 20, textBoxWidth);

            Label exampleLabel = new Label
            {
                Text = "Examples: #4CAF50, rgba(255, 0, 0, 0.5), rgb(100, 150, 200)",
                Location = new Point(20, 200),
                Width = 400,
                ForeColor = Color.Gray
            };
            colorGroup.Controls.Add(exampleLabel);

            yPos += 250;

            // Font Settings
            GroupBox fontGroup = new GroupBox
            {
                Text = "Font Settings",
                Location = new Point(20, yPos),
                Width = 450,
                Height = 90
            };

            AddLabelAndTextBox(fontGroup, "Font Family:", ref _fontFamilyTextBox, 
                25, textBoxX - 20, textBoxWidth);
            AddLabelAndTextBox(fontGroup, "Font Size:", ref _fontSizeTextBox, 
                55, textBoxX - 20, textBoxWidth);

            yPos += 100;

            // Other Settings
            Label borderLabel = new Label
            {
                Text = "Border Radius:",
                Location = new Point(20, yPos),
                Width = labelWidth
            };

            _borderRadiusTextBox = new TextBox
            {
                Location = new Point(textBoxX, yPos - 3),
                Width = textBoxWidth
            };

            yPos += 40;

            // Buttons
            _resetButton = new Button
            {
                Text = "Reset to Default",
                Location = new Point(20, yPos),
                Width = 120
            };
            _resetButton.Click += ResetButton_Click;

            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(300, yPos),
                Width = 80,
                DialogResult = DialogResult.OK
            };

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(390, yPos),
                Width = 80,
                DialogResult = DialogResult.Cancel
            };

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;

            this.Controls.AddRange(new Control[] {
                colorGroup, fontGroup, borderLabel, _borderRadiusTextBox,
                _resetButton, _okButton, _cancelButton
            });
        }

        private void AddLabelAndTextBox(Control parent, string labelText, ref TextBox textBox, 
            int yPos, int textBoxX, int textBoxWidth)
        {
            Label label = new Label
            {
                Text = labelText,
                Location = new Point(20, yPos + 3),
                Width = 170
            };

            textBox = new TextBox
            {
                Location = new Point(textBoxX, yPos),
                Width = textBoxWidth
            };

            parent.Controls.AddRange(new Control[] { label, textBox });
        }

        private void LoadThemeToUI()
        {
            _backgroundColorTextBox.Text = _theme.backgroundColor;
            _textColorTextBox.Text = _theme.textColor;
            _progressBarColorTextBox.Text = _theme.progressBarColor;
            _progressBarBackgroundTextBox.Text = _theme.progressBarBackground;
            _checkboxColorTextBox.Text = _theme.checkboxColor;
            _fontFamilyTextBox.Text = _theme.fontFamily;
            _fontSizeTextBox.Text = _theme.fontSize;
            _borderRadiusTextBox.Text = _theme.borderRadius;
        }

        private void ResetButton_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show("Reset theme to default values?", "Confirm Reset", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _theme = new Theme
                {
                    backgroundColor = "rgba(0, 0, 0, 0.7)",
                    textColor = "#ffffff",
                    progressBarColor = "#4CAF50",
                    progressBarBackground = "rgba(255, 255, 255, 0.2)",
                    checkboxColor = "#4CAF50",
                    fontFamily = "Arial, sans-serif",
                    fontSize = "18px",
                    borderRadius = "5px"
                };
                LoadThemeToUI();
            }
        }

        public Theme GetTheme()
        {
            return new Theme
            {
                backgroundColor = _backgroundColorTextBox.Text.Trim(),
                textColor = _textColorTextBox.Text.Trim(),
                progressBarColor = _progressBarColorTextBox.Text.Trim(),
                progressBarBackground = _progressBarBackgroundTextBox.Text.Trim(),
                checkboxColor = _checkboxColorTextBox.Text.Trim(),
                fontFamily = _fontFamilyTextBox.Text.Trim(),
                fontSize = _fontSizeTextBox.Text.Trim(),
                borderRadius = _borderRadiusTextBox.Text.Trim()
            };
        }
    }
}
