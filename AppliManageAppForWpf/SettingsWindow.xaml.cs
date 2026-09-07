using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.IO;

namespace AppliManageAppForWpf
{
    public partial class SettingsWindow : Window
    {
        public double ResultWidth { get; private set; }
        public double ResultHeight { get; private set; }
        public string ResultColor { get; private set; }
        public string ResultShape { get; private set; }
        public string ResultText { get; private set; }
        public string ResultTextColor { get; private set; }
        public string ResultImageMode { get; private set; }
        public bool ResultTextShadow { get; private set; }
        public string ResultTextShadowColor { get; private set; }
        public double ResultLineSpacing { get; private set; }
        public bool ResultFontBold { get; private set; }
        public bool ResultFontItalic { get; private set; }
        public string ResultFontName { get; private set; }
        public double ResultFontSize { get; private set; }
        public string ResultTextPosition { get; private set; }
        public double ResultBackgroundOpacity { get; private set; }
        public string ResultBackgroundImagePath { get; private set; }
        public double ResultIconSize { get; private set; }
        public bool ResultListMode { get; private set; }

        public SettingsWindow(double currentWidth, double currentHeight, string currentColor, string currentShape = "Pentagon", string currentText = "", string currentTextColor = "#FFFFFFFF",
            string currentFontName = null, double currentFontSize = 18, string currentTextPosition = "Center", double currentBackgroundOpacity = 1.0,
            string currentBackgroundImagePath = null, double currentIconSize = 48, bool currentListMode = false, string currentImageMode = "Fill",
            bool currentTextShadow = false, string currentTextShadowColor = "#66000000", double currentLineSpacing = 0, bool currentFontBold = false, bool currentFontItalic = false)
        {
            InitializeComponent();
            WidthBox.Text = ((int)currentWidth).ToString();
            HeightBox.Text = ((int)currentHeight).ToString();
            ColorBox.Text = currentColor ?? "#1E88E5";
            // shape
            if (!string.IsNullOrEmpty(currentShape))
            {
                foreach (ComboBoxItem it in ShapeBox.Items)
                {
                    if (string.Equals(it.Content.ToString(), currentShape, StringComparison.OrdinalIgnoreCase)) { ShapeBox.SelectedItem = it; break; }
                }
            }
            MultiTextBox.Text = currentText ?? string.Empty;
            TextColorBox.Text = currentTextColor ?? "#FFFFFFFF";

            // fonts
            foreach (var f in Fonts.SystemFontFamilies.OrderBy(ff => ff.Source)) FontBox.Items.Add(f.Source);
            if (FontBox.Items.Count > 0) FontBox.Text = currentFontName ?? Fonts.SystemFontFamilies.FirstOrDefault()?.Source;
            FontSizeBox.Text = (currentFontSize > 0) ? currentFontSize.ToString() : "18";
            var opacityBoxInit = this.FindName("OpacityBox") as TextBox;
            if (opacityBoxInit != null) opacityBoxInit.Text = currentBackgroundOpacity.ToString("F2");
            var iconSizeBoxInit = this.FindName("IconSizeBox") as TextBox;
            if (iconSizeBoxInit != null) iconSizeBoxInit.Text = ((int)currentIconSize).ToString();
            var bgImageBoxInit = this.FindName("BgImageBox") as TextBox;
            if (bgImageBoxInit != null) bgImageBoxInit.Text = string.Empty;
            var textPosBoxInit = this.FindName("TextPosBox") as ComboBox;
            if (textPosBoxInit != null)
            {
                // select item matching currentTextPosition
                foreach (ComboBoxItem it in textPosBoxInit.Items)
                {
                    if (string.Equals((it.Content ?? "").ToString(), currentTextPosition, StringComparison.OrdinalIgnoreCase)) { textPosBoxInit.SelectedItem = it; break; }
                }
            }
            var imageModeBoxInit = this.FindName("ImageModeBox") as ComboBox;
            if (imageModeBoxInit != null)
            {
                foreach (ComboBoxItem it in imageModeBoxInit.Items)
                {
                    if (string.Equals((it.Content ?? "").ToString(), currentImageMode, StringComparison.OrdinalIgnoreCase)) { imageModeBoxInit.SelectedItem = it; break; }
                }
            }
            var shadowColorBoxInit = this.FindName("ShadowColorBox") as TextBox;
            if (shadowColorBoxInit != null) shadowColorBoxInit.Text = currentTextShadowColor ?? "#66000000";
            var textShadowBoxInit = this.FindName("TextShadowBox") as CheckBox;
            if (textShadowBoxInit != null) textShadowBoxInit.IsChecked = currentTextShadow;
            var lineSpacingBoxInit = this.FindName("LineSpacingBox") as TextBox;
            if (lineSpacingBoxInit != null) lineSpacingBoxInit.Text = currentLineSpacing.ToString();
            var boldBoxInit = this.FindName("BoldBox") as CheckBox;
            if (boldBoxInit != null) boldBoxInit.IsChecked = currentFontBold;
            var italicBoxInit = this.FindName("ItalicBox") as CheckBox;
            if (italicBoxInit != null) italicBoxInit.IsChecked = currentFontItalic;
            var listModeBoxInit = this.FindName("ListModeBox") as CheckBox;
            if (listModeBoxInit != null) listModeBoxInit.IsChecked = currentListMode;

            // attach change handlers to update preview (use FindName to avoid compile-time field dependencies)
            var widthBox = this.FindName("WidthBox") as TextBox; if (widthBox != null) widthBox.TextChanged += (s, e) => UpdatePreview();
            var heightBox = this.FindName("HeightBox") as TextBox; if (heightBox != null) heightBox.TextChanged += (s, e) => UpdatePreview();
            var colorBox = this.FindName("ColorBox") as TextBox; if (colorBox != null) colorBox.TextChanged += (s, e) => UpdatePreview();
            var shapeBox = this.FindName("ShapeBox") as ComboBox; if (shapeBox != null) shapeBox.SelectionChanged += (s, e) => UpdatePreview();
            var multiTextBox = this.FindName("MultiTextBox") as TextBox; if (multiTextBox != null) multiTextBox.TextChanged += (s, e) => UpdatePreview();
            var textColorBox = this.FindName("TextColorBox") as TextBox; if (textColorBox != null) textColorBox.TextChanged += (s, e) => UpdatePreview();
            var fontBox = this.FindName("FontBox") as ComboBox; if (fontBox != null) { fontBox.SelectionChanged += (s, e) => UpdatePreview(); fontBox.LostFocus += (s, e) => UpdatePreview(); }
            var fontSizeBox = this.FindName("FontSizeBox") as TextBox; if (fontSizeBox != null) fontSizeBox.TextChanged += (s, e) => UpdatePreview();
            var opacityBoxChange = this.FindName("OpacityBox") as TextBox; if (opacityBoxChange != null) opacityBoxChange.TextChanged += (s, e) => UpdatePreview();
            var bgImageBoxChange = this.FindName("BgImageBox") as TextBox; if (bgImageBoxChange != null) bgImageBoxChange.TextChanged += (s, e) => UpdatePreview();
            var textPosBox = this.FindName("TextPosBox") as ComboBox; if (textPosBox != null) textPosBox.SelectionChanged += (s, e) => UpdatePreview();
            var iconSizeBox = this.FindName("IconSizeBox") as TextBox; if (iconSizeBox != null) iconSizeBox.TextChanged += (s, e) => { PreviewIconSizeChanged(); };

            var previewFont = this.FindName("PreviewFontSizeSlider") as System.Windows.Controls.Slider; if (previewFont != null) previewFont.ValueChanged += (s, e) => { var fb = this.FindName("FontSizeBox") as TextBox; if (fb != null) fb.Text = ((int)previewFont.Value).ToString(); UpdatePreview(); };
            var previewOpacity = this.FindName("PreviewOpacitySlider") as System.Windows.Controls.Slider; if (previewOpacity != null) previewOpacity.ValueChanged += (s, e) => { var ob = this.FindName("OpacityBox") as TextBox; if (ob != null) ob.Text = previewOpacity.Value.ToString("F2"); UpdatePreview(); };
            var previewIcon = this.FindName("PreviewIconSizeSlider") as System.Windows.Controls.Slider; if (previewIcon != null) previewIcon.ValueChanged += (s, e) => { var ib = this.FindName("IconSizeBox") as TextBox; if (ib != null) ib.Text = ((int)previewIcon.Value).ToString(); PreviewIconSizeChanged(); };

            UpdatePreview();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(WidthBox.Text, out double w) || w < 200) { MessageBox.Show("幅は200以上の数値を指定してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!double.TryParse(HeightBox.Text, out double h) || h < 200) { MessageBox.Show("高さは200以上の数値を指定してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            var col = ColorBox.Text?.Trim();
            if (string.IsNullOrEmpty(col) || !(col.StartsWith("#") && (col.Length == 7 || col.Length == 9 || col.Length == 9 || col.Length == 11))) { MessageBox.Show("色は#RRGGBBまたは#AARRGGBB形式で指定してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            ResultWidth = w; ResultHeight = h; ResultColor = col;

            var shape = (ShapeBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Pentagon";
            ResultShape = shape;
            ResultText = MultiTextBox.Text ?? string.Empty;
            var tcol = TextColorBox.Text?.Trim();
            if (string.IsNullOrEmpty(tcol)) tcol = "#FFFFFFFF";
            ResultTextColor = tcol;
            ResultImageMode = (ImageModeBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Fill";
            ResultTextShadow = TextShadowBox.IsChecked == true;
            ResultTextShadowColor = ShadowColorBox.Text?.Trim() ?? "#66000000";
            if (!double.TryParse(LineSpacingBox.Text, out double ls)) ls = 0; ResultLineSpacing = ls;
            ResultFontBold = BoldBox.IsChecked == true;
            ResultFontItalic = ItalicBox.IsChecked == true;
            ResultFontName = FontBox.Text?.Trim();
            if (!double.TryParse(FontSizeBox.Text, out double fs)) fs = 18; ResultFontSize = fs;
            ResultTextPosition = (TextPosBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Center";
            if (!double.TryParse(OpacityBox.Text, out double op)) op = 1.0; ResultBackgroundOpacity = Math.Max(0, Math.Min(1, op));
            ResultBackgroundImagePath = string.IsNullOrWhiteSpace(BgImageBox.Text) ? null : BgImageBox.Text.Trim();
            if (!double.TryParse(IconSizeBox.Text, out double isz)) isz = 48; ResultIconSize = Math.Max(8, isz);
            ResultListMode = ListModeBox.IsChecked == true;
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*" };
            if (dlg.ShowDialog() == true)
            {
                BgImageBox.Text = dlg.FileName;
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            // defaults
            WidthBox.Text = "600";
            HeightBox.Text = "450";
            ColorBox.Text = "#FF1E88E5";
            ShapeBox.SelectedIndex = 2; // Pentagon
            TextColorBox.Text = "#FFFFFFFF";
            FontBox.Text = "Segoe UI";
            FontSizeBox.Text = "18";
            TextPosBox.SelectedIndex = 1; // Center
            OpacityBox.Text = "1.0";
            IconSizeBox.Text = "48";
            BgImageBox.Text = string.Empty;
            ListModeBox.IsChecked = false;
            MultiTextBox.Text = string.Empty;
            PreviewFontSizeSlider.Value = 18;
            PreviewOpacitySlider.Value = 1.0;
            PreviewIconSizeSlider.Value = 48;
            UpdatePreview();
        }

        private void PreviewIconSizeChanged()
        {
            // set IconSize resource to allow template updates
            if (double.TryParse(IconSizeBox.Text, out double val))
            {
                try { this.Owner.Resources["IconSize"] = val; } catch { try { this.Resources["IconSize"] = val; } catch { } }
            }
        }

        private void UpdatePreview()
        {
            // shape
            var shape = (ShapeBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Pentagon";
            switch (shape.ToLowerInvariant())
            {
                case "rectangle": PreviewShape.Data = Geometry.Parse("M0,0 L100,0 L100,100 L0,100 Z"); break;
                case "ellipse": PreviewShape.Data = new EllipseGeometry(new System.Windows.Point(50, 50), 50, 50); break;
                case "hexagon": PreviewShape.Data = Geometry.Parse("M50,0 L93.3013,25 L93.3013,75 L50,100 L6.6987,75 L6.6987,25 Z"); break;
                default: PreviewShape.Data = Geometry.Parse("M50,0 L97.5528,35.3553 L79.3893,90.4508 L20.6107,90.4508 L2.4472,35.3553 Z"); break;
            }
            // color/opacity
            try
            {
                var c = (Color)ColorConverter.ConvertFromString(ColorBox.Text);
                double op = 1.0; double.TryParse(OpacityBox.Text, out op);
                var scb = new SolidColorBrush(c);
                scb.Opacity = Math.Max(0, Math.Min(1, op));
                PreviewShape.Fill = scb;
            }
            catch { }

            // background image
            try
            {
                if (!string.IsNullOrEmpty(BgImageBox.Text) && File.Exists(BgImageBox.Text))
                {
                    var ib = new ImageBrush(new BitmapImage(new Uri(BgImageBox.Text)));
                    if (double.TryParse(OpacityBox.Text, out double op2)) ib.Opacity = Math.Max(0, Math.Min(1, op2));
                    PreviewShape.Fill = ib;
                }
            }
            catch { }

            // text
            PreviewTextPanel.Children.Clear();
            Brush brush = Brushes.White;
            var textColorBox = this.FindName("TextColorBox") as TextBox;
            try { brush = (Brush)new BrushConverter().ConvertFromString(textColorBox?.Text); } catch { }
            var fontBox = this.FindName("FontBox") as ComboBox;
            var font = fontBox?.Text ?? "Segoe UI";
            var fontSizeBox = this.FindName("FontSizeBox") as TextBox;
            if (!double.TryParse(fontSizeBox?.Text, out double fsz)) fsz = 18;
            var multiTextBox = this.FindName("MultiTextBox") as TextBox;
            var lines = (multiTextBox?.Text ?? string.Empty).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var boldBox = this.FindName("BoldBox") as CheckBox;
            var italicBox = this.FindName("ItalicBox") as CheckBox;
            var lineSpacingBox = this.FindName("LineSpacingBox") as TextBox;
            var textShadowBox = this.FindName("TextShadowBox") as CheckBox;
            var shadowColorBox = this.FindName("ShadowColorBox") as TextBox;
            foreach (var l in lines)
            {
                var tb = new TextBlock { Text = l, Foreground = brush, FontSize = fsz, FontFamily = new FontFamily(font), HorizontalAlignment = HorizontalAlignment.Center, TextAlignment = TextAlignment.Center };
                // font style
                tb.FontWeight = (boldBox?.IsChecked == true) ? FontWeights.Bold : FontWeights.Normal;
                tb.FontStyle = (italicBox?.IsChecked == true) ? FontStyles.Italic : FontStyles.Normal;
                // line spacing
                if (double.TryParse(lineSpacingBox?.Text, out double ls) && ls > 0)
                {
                    tb.LineHeight = fsz + ls;
                    tb.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                }
                // shadow
                if (textShadowBox?.IsChecked == true)
                {
                    try
                    {
                        var sc = (Color)ColorConverter.ConvertFromString(shadowColorBox?.Text ?? "#66000000");
                        tb.Effect = new System.Windows.Media.Effects.DropShadowEffect { Color = sc, BlurRadius = 6, ShadowDepth = 2, Opacity = 1 };
                    }
                    catch { }
                }
                PreviewTextPanel.Children.Add(tb);
            }
            // position
            var pos = (TextPosBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Center";
            switch (pos.ToLowerInvariant()) { case "top": PreviewTextPanel.VerticalAlignment = VerticalAlignment.Top; break; case "bottom": PreviewTextPanel.VerticalAlignment = VerticalAlignment.Bottom; break; default: PreviewTextPanel.VerticalAlignment = VerticalAlignment.Center; break; }
        }
    }
}
