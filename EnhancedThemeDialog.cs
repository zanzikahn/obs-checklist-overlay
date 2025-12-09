using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace OBSChecklistEditor
{
    public class EnhancedThemeDialog : Form
    {
        private Theme _theme = null!;
        private Theme _originalTheme = null!;
        private ChecklistConfig _config = null!;

        // Color picker controls
        private Button _bgColorButton = null!;
        private Button _textColorButton = null!;
        private Button _progressColorButton = null!;
        private Button _progressBgColorButton = null!;
        private Button _checkboxColorButton = null!;
        private Button _subHeaderColorButton = null!;
        private Button _subHeaderBgColorButton = null!;

        // Font controls
        private ComboBox _fontFamilyCombo = null!;
        private NumericUpDown _fontSizeNumeric = null!;
        private NumericUpDown _borderRadiusNumeric = null!;
        private TrackBar _opacitySlider = null!;
        private Label _opacityValueLabel = null!;

        // Preset controls
        private ComboBox _presetCombo = null!;
        private Button _applyPresetButton = null!;

        // Preview panel
        private Panel _previewPanel = null!;
        
        // Buttons
        private Button _okButton = null!;
        private Button _cancelButton = null!;
        private Button _resetButton = null!;
        private Button _resetColorsButton = null!;
        private Button _resetFontsButton = null!;

        // Web-safe fonts
        private readonly string[] _webSafeFonts = new[]
        {
            "Arial",
            "Arial, sans-serif",
            "Helvetica, sans-serif",
            "Times New Roman, serif",
            "Georgia, serif",
            "Courier New, monospace",
            "Verdana, sans-serif",
            "Trebuchet MS, sans-serif",
            "Comic Sans MS, cursive",
            "Impact, sans-serif",
            "Lucida Console, monospace",
            "Tahoma, sans-serif",
            "Palatino, serif",
            "Garamond, serif",
            "Bookman, serif",
            "Segoe UI, sans-serif"
        };

        public EnhancedThemeDialog(Theme currentTheme, ChecklistConfig config)
        {
            _theme = CloneTheme(currentTheme);
            _originalTheme = CloneTheme(currentTheme);
            _config = config;
            InitializeComponents();
            LoadThemeToUI();
            UpdatePreview();
        }

        private Theme CloneTheme(Theme theme)
        {
            return new Theme
            {
                backgroundColor = theme.backgroundColor,
                textColor = theme.textColor,
                progressBarColor = theme.progressBarColor,
                progressBarBackground = theme.progressBarBackground,
                checkboxColor = theme.checkboxColor,
                fontFamily = theme.fontFamily,
                fontSize = theme.fontSize,
                borderRadius = theme.borderRadius,
                subHeaderColor = theme.subHeaderColor ?? "#FFD700",
                subHeaderBackground = theme.subHeaderBackground ?? "rgba(255, 215, 0, 0.1)"
            };
        }

        private void InitializeComponents()
        {
            this.Text = "Theme Settings";
            this.Width = 900;
            this.Height = 750;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.MinimumSize = new Size(900, 750);

            // Main layout
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            int yPos = 10;

            // Presets section
            var presetGroup = CreatePresetSection(ref yPos);
            mainPanel.Controls.Add(presetGroup);

            // Colors section
            var colorGroup = CreateColorSection(ref yPos);
            mainPanel.Controls.Add(colorGroup);

            // Fonts section
            var fontGroup = CreateFontSection(ref yPos);
            mainPanel.Controls.Add(fontGroup);

            // Preview section
            var previewGroup = CreatePreviewSection(ref yPos);
            mainPanel.Controls.Add(previewGroup);

            // Buttons
            CreateButtons(mainPanel, ref yPos);

            this.Controls.Add(mainPanel);
        }

        private GroupBox CreatePresetSection(ref int yPos)
        {
            var group = new GroupBox
            {
                Text = "Theme Presets",
                Location = new Point(10, yPos),
                Width = 850,
                Height = 80
            };

            var label = new Label
            {
                Text = "Quick Start:",
                Location = new Point(15, 25),
                Width = 80,
                AutoSize = false
            };

            _presetCombo = new ComboBox
            {
                Location = new Point(100, 23),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _presetCombo.Items.AddRange(new object[]
            {
                "Dark Mode (Default)",
                "Light Mode",
                "High Contrast",
                "Solarized Dark",
                "Nord Theme",
                "Dracula",
                "Monokai",
                "Cyberpunk"
            });
            _presetCombo.SelectedIndex = 0;

            _applyPresetButton = new Button
            {
                Text = "Apply Preset",
                Location = new Point(310, 22),
                Width = 100
            };
            _applyPresetButton.Click += ApplyPresetButton_Click;

            AddTooltip(label, "Select a pre-made theme to quickly customize your overlay");
            AddTooltip(_presetCombo, "Choose from professionally designed color schemes");

            group.Controls.AddRange(new Control[] { label, _presetCombo, _applyPresetButton });

            yPos += group.Height + 10;
            return group;
        }

        private GroupBox CreateColorSection(ref int yPos)
        {
            var group = new GroupBox
            {
                Text = "Colors (Click buttons to select colors)",
                Location = new Point(10, yPos),
                Width = 850,
                Height = 320
            };

            int itemY = 25;

            AddColorPicker(group, "Background:", ref _bgColorButton, ref itemY, 
                "The main background color of the overlay. Supports transparency (RGBA).");
            AddColorPicker(group, "Text:", ref _textColorButton, ref itemY,
                "Color for task names and labels");
            AddColorPicker(group, "Progress Bar:", ref _progressColorButton, ref itemY,
                "Fill color for progress bars");
            AddColorPicker(group, "Progress Bar Background:", ref _progressBgColorButton, ref itemY,
                "Background color behind progress bars");
            AddColorPicker(group, "Checkbox/Complete:", ref _checkboxColorButton, ref itemY,
                "Color for checkboxes and completion indicators");
            AddColorPicker(group, "Section Header Text:", ref _subHeaderColorButton, ref itemY,
                "Text color for section headers (dividers)");
            AddColorPicker(group, "Section Header Background:", ref _subHeaderBgColorButton, ref itemY,
                "Background color for section headers");

            _resetColorsButton = new Button
            {
                Text = "Reset Colors to Default",
                Location = new Point(15, itemY),
                Width = 180
            };
            _resetColorsButton.Click += ResetColorsButton_Click;
            group.Controls.Add(_resetColorsButton);

            yPos += group.Height + 10;
            return group;
        }

        private void AddColorPicker(GroupBox parent, string labelText, ref Button colorButton, ref int yPos, string tooltip)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(15, yPos + 3),
                Width = 180,
                AutoSize = false
            };

            colorButton = new Button
            {
                Location = new Point(200, yPos),
                Width = 120,
                Height = 30,
                Text = "Pick Color",
                BackColor = Color.White
            };
            colorButton.Click += ColorButton_Click;

            var exampleLabel = new Label
            {
                Text = "Preview →",
                Location = new Point(330, yPos + 5),
                Width = 80,
                ForeColor = Color.Gray,
                Font = new Font(this.Font.FontFamily, 8)
            };

            var previewBox = new Panel
            {
                Location = new Point(420, yPos),
                Width = 200,
                Height = 30,
                BorderStyle = BorderStyle.FixedSingle
            };
            colorButton.Tag = previewBox; // Link button to preview

            AddTooltip(label, tooltip);
            AddTooltip(colorButton, $"Click to open color picker. {tooltip}");

            parent.Controls.AddRange(new Control[] { label, colorButton, exampleLabel, previewBox });

            yPos += 35;
        }

        private GroupBox CreateFontSection(ref int yPos)
        {
            var group = new GroupBox
            {
                Text = "Typography & Opacity",
                Location = new Point(10, yPos),
                Width = 850,
                Height = 175
            };

            // Font Family
            var fontLabel = new Label
            {
                Text = "Font Family:",
                Location = new Point(15, 28),
                Width = 180
            };

            _fontFamilyCombo = new ComboBox
            {
                Location = new Point(200, 25),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _fontFamilyCombo.Items.AddRange(_webSafeFonts);
            _fontFamilyCombo.SelectedIndexChanged += (s, e) => UpdatePreview();

            AddTooltip(fontLabel, "Choose from web-safe fonts that work on all systems");
            AddTooltip(_fontFamilyCombo, "Select a font for all text in the overlay. These fonts are guaranteed to work.");

            // Font Size
            var sizeLabel = new Label
            {
                Text = "Font Size:",
                Location = new Point(15, 63),
                Width = 180
            };

            _fontSizeNumeric = new NumericUpDown
            {
                Location = new Point(200, 60),
                Width = 80,
                Minimum = 10,
                Maximum = 48,
                Value = 18
            };
            _fontSizeNumeric.ValueChanged += (s, e) => UpdatePreview();

            var sizeUnitLabel = new Label
            {
                Text = "px",
                Location = new Point(285, 63),
                Width = 30
            };

            AddTooltip(sizeLabel, "Size of text in pixels. Typical range: 14-24px");

            // Border Radius
            var radiusLabel = new Label
            {
                Text = "Border Radius:",
                Location = new Point(15, 98),
                Width = 180
            };

            _borderRadiusNumeric = new NumericUpDown
            {
                Location = new Point(200, 95),
                Width = 80,
                Minimum = 0,
                Maximum = 50,
                Value = 5
            };
            _borderRadiusNumeric.ValueChanged += (s, e) => UpdatePreview();

            var radiusUnitLabel = new Label
            {
                Text = "px",
                Location = new Point(285, 98),
                Width = 30
            };

            AddTooltip(radiusLabel, "Roundness of corners. 0 = square, higher = rounder");

            // Overlay Opacity
            var opacityLabel = new Label
            {
                Text = "Overlay Opacity:",
                Location = new Point(15, 133),
                Width = 180
            };

            _opacitySlider = new TrackBar
            {
                Location = new Point(200, 130),
                Width = 300,
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                TickFrequency = 10
            };
            _opacitySlider.ValueChanged += OpacitySlider_ValueChanged;

            _opacityValueLabel = new Label
            {
                Text = "100%",
                Location = new Point(510, 133),
                Width = 50,
                Font = new Font(this.Font, FontStyle.Bold)
            };

            AddTooltip(opacityLabel, "Overall transparency of the overlay. 0% = invisible, 100% = fully visible");
            AddTooltip(_opacitySlider, "Drag to adjust overlay transparency");

            _resetFontsButton = new Button
            {
                Text = "Reset Fonts to Default",
                Location = new Point(320, 92),
                Width = 180
            };
            _resetFontsButton.Click += ResetFontsButton_Click;

            group.Controls.AddRange(new Control[] {
                fontLabel, _fontFamilyCombo,
                sizeLabel, _fontSizeNumeric, sizeUnitLabel,
                radiusLabel, _borderRadiusNumeric, radiusUnitLabel,
                opacityLabel, _opacitySlider, _opacityValueLabel,
                _resetFontsButton
            });

            yPos += group.Height + 10;
            return group;
        }

        private GroupBox CreatePreviewSection(ref int yPos)
        {
            var group = new GroupBox
            {
                Text = "Live Preview",
                Location = new Point(10, yPos),
                Width = 850,
                Height = 180
            };

            _previewPanel = new Panel
            {
                Location = new Point(15, 25),
                Width = 820,
                Height = 140,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black
            };
            _previewPanel.Paint += PreviewPanel_Paint;

            var infoLabel = new Label
            {
                Text = "↑ Real-time preview of your theme. Changes appear instantly.",
                Location = new Point(250, 150),
                Width = 400,
                ForeColor = Color.Gray,
                Font = new Font(this.Font.FontFamily, 8)
            };

            group.Controls.AddRange(new Control[] { _previewPanel, infoLabel });

            yPos += group.Height + 10;
            return group;
        }

        private void PreviewPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (_previewPanel == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Get current theme colors
            Color bgColor = _bgColorButton?.BackColor ?? Color.Black;
            Color textColor = _textColorButton?.BackColor ?? Color.White;
            Color progressColor = _progressColorButton?.BackColor ?? Color.Green;
            Color checkboxColor = _checkboxColorButton?.BackColor ?? Color.Green;
            Color subHeaderColor = _subHeaderColorButton?.BackColor ?? Color.Gold;

            // Draw background
            using (var bgBrush = new SolidBrush(Color.FromArgb(180, bgColor.R, bgColor.G, bgColor.B)))
            {
                g.FillRectangle(bgBrush, 0, 0, _previewPanel.Width, _previewPanel.Height);
            }

            // Draw header
            using (var headerBrush = new SolidBrush(textColor))
            using (var headerFont = new Font("Arial", 12, FontStyle.Bold))
            {
                g.DrawString("Preview List", headerFont, headerBrush, 10, 10);
            }

            // Draw section header
            using (var subHeaderBrush = new SolidBrush(Color.FromArgb(25, subHeaderColor.R, subHeaderColor.G, subHeaderColor.B)))
            using (var subHeaderPen = new Pen(subHeaderColor, 3))
            using (var subHeaderTextBrush = new SolidBrush(subHeaderColor))
            using (var subHeaderFont = new Font("Arial", 10, FontStyle.Bold))
            {
                g.FillRectangle(subHeaderBrush, 10, 40, 400, 25);
                g.DrawLine(subHeaderPen, 10, 40, 10, 65);
                g.DrawString("SECTION HEADER", subHeaderFont, subHeaderTextBrush, 20, 45);
            }

            // Draw task 1 with checkbox
            using (var checkboxPen = new Pen(checkboxColor, 2))
            using (var textBrush = new SolidBrush(textColor))
            using (var taskFont = new Font("Arial", 9))
            {
                // Checkbox
                g.DrawRectangle(checkboxPen, 15, 75, 18, 18);
                // Task name
                g.DrawString("Example Task", taskFont, textBrush, 40, 75);
            }

            // Draw progress bar
            using (var progressBgBrush = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
            using (var progressBrush = new SolidBrush(progressColor))
            using (var counterBrush = new SolidBrush(textColor))
            using (var counterFont = new Font("Arial", 8))
            {
                // Progress bar background
                g.FillRectangle(progressBgBrush, 40, 100, 200, 15);
                // Progress bar fill (70%)
                g.FillRectangle(progressBrush, 40, 100, 140, 15);
                // Counter
                g.DrawString("7/10", counterFont, counterBrush, 250, 100);
            }

            // Draw completed task
            using (var checkboxBrush = new SolidBrush(checkboxColor))
            using (var checkBrush = new SolidBrush(Color.White))
            using (var textBrush = new SolidBrush(Color.FromArgb(128, textColor.R, textColor.G, textColor.B)))
            using (var taskFont = new Font("Arial", 9, FontStyle.Strikeout))
            using (var checkFont = new Font("Arial", 12, FontStyle.Bold))
            {
                // Filled checkbox
                g.FillRectangle(checkboxBrush, 15, 120, 18, 18);
                g.DrawString("✓", checkFont, checkBrush, 15, 118);
                // Completed task name (grayed and strikethrough)
                g.DrawString("Completed Task", taskFont, textBrush, 40, 120);
            }
        }

        private void CreateButtons(Panel parent, ref int yPos)
        {
            // Add some padding before buttons
            yPos += 10;
            
            _resetButton = new Button
            {
                Text = "Reset All to Default",
                Location = new Point(20, yPos),
                Width = 150,
                Height = 35
            };
            _resetButton.Click += ResetAllButton_Click;

            _okButton = new Button
            {
                Text = "Apply && Save",
                Location = new Point(620, yPos),
                Width = 120,
                Height = 35,
                DialogResult = DialogResult.OK
            };

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(750, yPos),
                Width = 100,
                Height = 35,
                DialogResult = DialogResult.Cancel
            };

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;

            parent.Controls.AddRange(new Control[] { _resetButton, _okButton, _cancelButton });
        }

        private void ColorButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                using (var dialog = new ColorDialog())
                {
                    dialog.FullOpen = true;
                    dialog.Color = button.BackColor;
                    
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        button.BackColor = dialog.Color;
                        
                        // Update preview box if linked
                        if (button.Tag is Panel previewBox)
                        {
                            previewBox.BackColor = dialog.Color;
                        }
                        
                        UpdatePreview();
                        CheckContrast();
                    }
                }
            }
        }

        private void ApplyPresetButton_Click(object? sender, EventArgs e)
        {
            var preset = _presetCombo.SelectedItem?.ToString() ?? "";
            
            switch (preset)
            {
                case "Dark Mode (Default)":
                    ApplyPreset(
                        "rgba(0, 0, 0, 0.7)", "#ffffff", "#4CAF50",
                        "rgba(255, 255, 255, 0.2)", "#4CAF50", "#FFD700",
                        "rgba(255, 215, 0, 0.1)", "Arial, sans-serif", "18px", "5px");
                    break;
                    
                case "Light Mode":
                    ApplyPreset(
                        "rgba(255, 255, 255, 0.9)", "#000000", "#2196F3",
                        "rgba(0, 0, 0, 0.1)", "#2196F3", "#FF6B6B",
                        "rgba(255, 107, 107, 0.1)", "Arial, sans-serif", "18px", "8px");
                    break;
                    
                case "High Contrast":
                    ApplyPreset(
                        "rgba(0, 0, 0, 0.95)", "#FFFF00", "#00FF00",
                        "rgba(255, 255, 255, 0.3)", "#00FF00", "#FFFF00",
                        "rgba(255, 255, 0, 0.2)", "Arial, sans-serif", "20px", "2px");
                    break;
                    
                case "Solarized Dark":
                    ApplyPreset(
                        "rgba(0, 43, 54, 0.9)", "#839496", "#268bd2",
                        "rgba(131, 148, 150, 0.2)", "#268bd2", "#b58900",
                        "rgba(181, 137, 0, 0.1)", "Courier New, monospace", "16px", "4px");
                    break;
                    
                case "Nord Theme":
                    ApplyPreset(
                        "rgba(46, 52, 64, 0.9)", "#eceff4", "#88c0d0",
                        "rgba(236, 239, 244, 0.1)", "#88c0d0", "#ebcb8b",
                        "rgba(235, 203, 139, 0.1)", "Segoe UI, sans-serif", "18px", "6px");
                    break;
                    
                case "Dracula":
                    ApplyPreset(
                        "rgba(40, 42, 54, 0.9)", "#f8f8f2", "#bd93f9",
                        "rgba(248, 248, 242, 0.1)", "#bd93f9", "#ff79c6",
                        "rgba(255, 121, 198, 0.1)", "Consolas, monospace", "18px", "8px");
                    break;
                    
                case "Monokai":
                    ApplyPreset(
                        "rgba(39, 40, 34, 0.9)", "#f8f8f2", "#a6e22e",
                        "rgba(248, 248, 242, 0.1)", "#a6e22e", "#fd971f",
                        "rgba(253, 151, 31, 0.1)", "Courier New, monospace", "17px", "3px");
                    break;
                    
                case "Cyberpunk":
                    ApplyPreset(
                        "rgba(0, 0, 0, 0.85)", "#00ffff", "#ff00ff",
                        "rgba(0, 255, 255, 0.2)", "#ff00ff", "#ffff00",
                        "rgba(255, 255, 0, 0.15)", "Impact, sans-serif", "20px", "10px");
                    break;
            }
            
            UpdatePreview();
        }

        private void ApplyPreset(string bg, string text, string progress, string progressBg,
            string checkbox, string subHeader, string subHeaderBg, string font, string fontSize, string radius)
        {
            _bgColorButton.BackColor = ParseColor(bg);
            _textColorButton.BackColor = ParseColor(text);
            _progressColorButton.BackColor = ParseColor(progress);
            _progressBgColorButton.BackColor = ParseColor(progressBg);
            _checkboxColorButton.BackColor = ParseColor(checkbox);
            _subHeaderColorButton.BackColor = ParseColor(subHeader);
            _subHeaderBgColorButton.BackColor = ParseColor(subHeaderBg);
            
            int fontIndex = Array.FindIndex(_webSafeFonts, f => f.Contains(font.Split(',')[0]));
            if (fontIndex >= 0) _fontFamilyCombo.SelectedIndex = fontIndex;
            
            _fontSizeNumeric.Value = int.Parse(fontSize.Replace("px", ""));
            _borderRadiusNumeric.Value = int.Parse(radius.Replace("px", ""));
        }

        private Color ParseColor(string colorStr)
        {
            if (colorStr.StartsWith("#"))
            {
                return ColorTranslator.FromHtml(colorStr);
            }
            else if (colorStr.StartsWith("rgba"))
            {
                // Parse rgba(r, g, b, a) - just get the RGB part
                var parts = colorStr.Replace("rgba(", "").Replace(")", "").Split(',');
                if (parts.Length >= 3)
                {
                    int r = int.Parse(parts[0].Trim());
                    int g = int.Parse(parts[1].Trim());
                    int b = int.Parse(parts[2].Trim());
                    return Color.FromArgb(r, g, b);
                }
            }
            return Color.White;
        }

        private void CheckContrast()
        {
            // Simple contrast check
            var bgColor = _bgColorButton.BackColor;
            var textColor = _textColorButton.BackColor;
            
            double bgBrightness = (bgColor.R + bgColor.G + bgColor.B) / 3.0;
            double textBrightness = (textColor.R + textColor.G + textColor.B) / 3.0;
            
            if (Math.Abs(bgBrightness - textBrightness) < 50)
            {
                MessageBox.Show(
                    "⚠️ Accessibility Warning\n\n" +
                    "The contrast between background and text colors is low.\n" +
                    "This may make the overlay difficult to read.\n\n" +
                    "Consider choosing colors with more contrast.",
                    "Low Contrast Detected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void UpdatePreview()
        {
            // Refresh the preview panel
            _previewPanel?.Invalidate();
        }

        private void ResetColorsButton_Click(object? sender, EventArgs e)
        {
            _bgColorButton.BackColor = ParseColor("rgba(0, 0, 0, 0.7)");
            _textColorButton.BackColor = Color.White;
            _progressColorButton.BackColor = ParseColor("#4CAF50");
            _progressBgColorButton.BackColor = ParseColor("rgba(255, 255, 255, 0.2)");
            _checkboxColorButton.BackColor = ParseColor("#4CAF50");
            _subHeaderColorButton.BackColor = ParseColor("#FFD700");
            _subHeaderBgColorButton.BackColor = ParseColor("rgba(255, 215, 0, 0.1)");
            UpdatePreview();
        }

        private void ResetFontsButton_Click(object? sender, EventArgs e)
        {
            _fontFamilyCombo.SelectedIndex = 1; // Arial, sans-serif
            _fontSizeNumeric.Value = 18;
            _borderRadiusNumeric.Value = 5;
            _opacitySlider.Value = 100;
            UpdatePreview();
        }

        private void OpacitySlider_ValueChanged(object? sender, EventArgs e)
        {
            _opacityValueLabel.Text = $"{_opacitySlider.Value}%";
            UpdatePreview();
        }

        private void ResetAllButton_Click(object? sender, EventArgs e)
        {
            ResetColorsButton_Click(sender, e);
            ResetFontsButton_Click(sender, e);
        }

        private void LoadThemeToUI()
        {
            _bgColorButton.BackColor = ParseColor(_theme.backgroundColor);
            _textColorButton.BackColor = ParseColor(_theme.textColor);
            _progressColorButton.BackColor = ParseColor(_theme.progressBarColor);
            _progressBgColorButton.BackColor = ParseColor(_theme.progressBarBackground);
            _checkboxColorButton.BackColor = ParseColor(_theme.checkboxColor);
            _subHeaderColorButton.BackColor = ParseColor(_theme.subHeaderColor);
            _subHeaderBgColorButton.BackColor = ParseColor(_theme.subHeaderBackground);
            
            int fontIndex = Array.FindIndex(_webSafeFonts, f => f == _theme.fontFamily);
            _fontFamilyCombo.SelectedIndex = fontIndex >= 0 ? fontIndex : 1;
            
            _fontSizeNumeric.Value = int.Parse(_theme.fontSize.Replace("px", ""));
            _borderRadiusNumeric.Value = int.Parse(_theme.borderRadius.Replace("px", ""));
            
            // Load opacity from config settings
            _opacitySlider.Value = (int)(_config.settings.overlayOpacity * 100);
            _opacityValueLabel.Text = $"{_opacitySlider.Value}%";
        }

        public Theme GetTheme()
        {
            // Update config opacity
            _config.settings.overlayOpacity = _opacitySlider.Value / 100.0;
            
            return new Theme
            {
                backgroundColor = ColorToRgba(_bgColorButton.BackColor),
                textColor = ColorToHex(_textColorButton.BackColor),
                progressBarColor = ColorToHex(_progressColorButton.BackColor),
                progressBarBackground = ColorToRgba(_progressBgColorButton.BackColor),
                checkboxColor = ColorToHex(_checkboxColorButton.BackColor),
                fontFamily = _fontFamilyCombo.SelectedItem?.ToString() ?? "Arial, sans-serif",
                fontSize = $"{_fontSizeNumeric.Value}px",
                borderRadius = $"{_borderRadiusNumeric.Value}px",
                subHeaderColor = ColorToHex(_subHeaderColorButton.BackColor),
                subHeaderBackground = ColorToRgba(_subHeaderBgColorButton.BackColor)
            };
        }

        private string ColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private string ColorToRgba(Color color)
        {
            return $"rgba({color.R}, {color.G}, {color.B}, 0.7)";
        }

        private void AddTooltip(Control control, string text)
        {
            var tooltip = new ToolTip();
            tooltip.SetToolTip(control, text);
        }
    }
}
