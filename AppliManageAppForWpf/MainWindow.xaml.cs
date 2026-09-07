using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppliManageAppForWpf
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // IconSize as a DependencyProperty to allow binding from XAML templates
        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
            "IconSize", typeof(double), typeof(MainWindow), new PropertyMetadata(48.0));

        public double IconSize
        {
            get { return (double)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }



        // Drag & drop reordering handlers
        private void AppItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _isDragging = false;
            if (FreePlacement)
            {
                // begin canvas drag candidate
                var btn = sender as Button;
                if (btn != null)
                {
                    _canvasDraggingItem = btn.DataContext as AppItem;
                    if (_canvasDraggingItem != null)
                    {
                        _canvasDragStartPoint = e.GetPosition(this.AppList);
                        _canvasDragItemStartX = _canvasDraggingItem.X;
                        _canvasDragItemStartY = _canvasDraggingItem.Y;
                    }
                }
            }
        }

        private void AppItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(null);
                if (!_isDragging && (Math.Abs(pos.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(pos.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    if (FreePlacement)
                    {
                        // start moving on canvas
                        _isCanvasDragging = true;
                    }
                    else
                    {
                        _isDragging = true;
                        var item = btn.DataContext as AppItem;
                        if (item != null)
                        {
                            DragDrop.DoDragDrop(btn, item, DragDropEffects.Move);
                        }
                    }
                }
                // during canvas drag, update position
                if (_isCanvasDragging && _canvasDraggingItem != null)
                {
                    var cur = e.GetPosition(this.AppList);
                    var dx = cur.X - _canvasDragStartPoint.X;
                    var dy = cur.Y - _canvasDragStartPoint.Y;
                    _canvasDraggingItem.X = _canvasDragItemStartX + dx;
                    _canvasDraggingItem.Y = _canvasDragItemStartY + dy;
                    // refresh binding by forcing item update
                    this.AppList.Items.Refresh();
                }
            }
        }

        private void AppItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isCanvasDragging && _canvasDraggingItem != null)
            {
                // finalize position
                _isCanvasDragging = false;
                SaveSettings();
            }
            _canvasDraggingItem = null;
        }

        private void AppItem_Drop(object sender, DragEventArgs e)
        {
            var targetBtn = sender as Button;
            if (targetBtn == null) return;
            var targetItem = targetBtn.DataContext as AppItem;
            var sourceItem = e.Data.GetData(typeof(AppItem)) as AppItem;
            if (sourceItem == null || targetItem == null || ReferenceEquals(sourceItem, targetItem)) return;
            var srcIdx = AppItems.IndexOf(sourceItem);
            var tgtIdx = AppItems.IndexOf(targetItem);
            if (srcIdx < 0 || tgtIdx < 0) return;
            AppItems.Move(srcIdx, tgtIdx);
            SaveSettings();
        }

        // drag state
        private Point _dragStartPoint;
        private bool _isDragging = false;
        private ObservableCollection<AppItem> AppItems = new ObservableCollection<AppItem>();
        private string SettingsFilePath => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppliManageAppForWpf", "settings.xml");
        private List<string> BackgroundLines = new List<string>();
        private string BackgroundTextColor = "#FFFFFFFF";
        private string CurrentShape = "Pentagon";
        private string BackgroundFontName = "Segoe UI";
        private double BackgroundFontSize = 18;
        private string BackgroundTextPosition = "Center";
        private double BackgroundOpacity = 1.0;
        private string BackgroundImagePath = null;
        private double IconSizeValue = 48;
        private bool ListMode = false;
        private string BackgroundImageMode = "Fill";
        private bool BackgroundTextShadow = false;
        private string BackgroundTextShadowColor = "#66000000";
        private double BackgroundLineSpacing = 0;
        private bool BackgroundFontBold = false;
        private bool BackgroundFontItalic = false;
        private string IconPlacement = "Center";
        private bool FreePlacement = false;

        // canvas drag state for free placement
        private bool _isCanvasDragging = false;
        private Point _canvasDragStartPoint;
        private double _canvasDragItemStartX;
        private double _canvasDragItemStartY;
        private AppItem _canvasDraggingItem = null;

        public MainWindow()
        {
            InitializeComponent();
            // ItemsControl にバインド
            this.AppList.ItemsSource = AppItems;
            this.AppListVertical.ItemsSource = AppItems;
            // 読み込み
            LoadSettings();
            this.Closing += MainWindow_Closing;
        }

        private void ApplyShape(string shape)
        {
            if (string.IsNullOrEmpty(shape)) shape = "Pentagon";
            shape = shape.ToLowerInvariant();
            CurrentShape = shape;
            switch (shape)
            {
                case "rectangle":
                    Pentagon.Data = Geometry.Parse("M0,0 L100,0 L100,100 L0,100 Z");
                    break;
                case "ellipse":
                    // Use EllipseGeometry
                    Pentagon.Data = new EllipseGeometry(new System.Windows.Point(50, 50), 50, 50);
                    break;
                case "hexagon":
                    // regular hexagon
                    Pentagon.Data = Geometry.Parse("M50,0 L93.3013,25 L93.3013,75 L50,100 L6.6987,75 L6.6987,25 Z");
                    break;
                case "pentagon":
                default:
                    Pentagon.Data = Geometry.Parse("M50,0 L97.5528,35.3553 L79.3893,90.4508 L20.6107,90.4508 L2.4472,35.3553 Z");
                    break;
            }
        }

        private string GetCurrentShape()
        {
            return string.IsNullOrEmpty(CurrentShape) ? "Pentagon" : CurrentShape;
        }

        // Placeholder: ensure method exists to apply background image and mode.
        // A more complete implementation can be added later.
        private void ApplyBackgroundImageAndMode()
        {
            try
            {
                if (!string.IsNullOrEmpty(BackgroundImagePath) && File.Exists(BackgroundImagePath))
                {
                    var ib = new ImageBrush(new BitmapImage(new Uri(BackgroundImagePath)));
                    ib.Opacity = BackgroundOpacity;
                    switch ((BackgroundImageMode ?? "Fill").ToLowerInvariant())
                    {
                        case "uniform":
                            ib.Stretch = Stretch.Uniform;
                            break;
                        case "uniformtofill":
                            ib.Stretch = Stretch.UniformToFill;
                            break;
                        case "none":
                            ib.Stretch = Stretch.None;
                            break;
                        default:
                            ib.Stretch = Stretch.Fill;
                            break;
                    }
                    Pentagon.Fill = ib;
                    return;
                }
            }
            catch { }

            // Fallback: ensure Pentagon has a SolidColorBrush if image not applied
            try
            {
                if (Pentagon.Fill == null || (Pentagon.Fill is ImageBrush))
                {
                    var colorStr = BackgroundTextColor ?? "#1E88E5";
                    var col = (Color)ColorConverter.ConvertFromString(colorStr);
                    var scb = new SolidColorBrush(col) { Opacity = BackgroundOpacity };
                    Pentagon.Fill = scb;
                }
            }
            catch { }
        }

        private void UpdateBackgroundText()
        {
            BackgroundTextPanel.Children.Clear();
            if (BackgroundLines == null || BackgroundLines.Count == 0) return;
            Brush brush = Brushes.White;
            try { brush = (Brush)new BrushConverter().ConvertFromString(BackgroundTextColor); } catch { }
            foreach (var line in BackgroundLines)
            {
                var tb = new TextBlock { Text = line, Foreground = brush, FontSize = BackgroundFontSize, FontFamily = new FontFamily(BackgroundFontName), HorizontalAlignment = HorizontalAlignment.Center, TextAlignment = TextAlignment.Center };
                tb.FontWeight = BackgroundFontBold ? FontWeights.Bold : FontWeights.Normal;
                tb.FontStyle = BackgroundFontItalic ? FontStyles.Italic : FontStyles.Normal;
                if (BackgroundLineSpacing > 0)
                {
                    tb.LineHeight = BackgroundFontSize + BackgroundLineSpacing;
                    tb.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                }
                if (BackgroundTextShadow)
                {
                    try { var sc = (Color)ColorConverter.ConvertFromString(BackgroundTextShadowColor); tb.Effect = new System.Windows.Media.Effects.DropShadowEffect { Color = sc, BlurRadius = 6, ShadowDepth = 2, Opacity = 1 }; } catch { }
                }
                BackgroundTextPanel.Children.Add(tb);
            }
            // position
            switch ((BackgroundTextPosition ?? "Center").ToLowerInvariant())
            {
                case "top": BackgroundTextPanel.VerticalAlignment = VerticalAlignment.Top; break;
                case "bottom": BackgroundTextPanel.VerticalAlignment = VerticalAlignment.Bottom; break;
                default: BackgroundTextPanel.VerticalAlignment = VerticalAlignment.Center; break;
            }
        }

        private void Pentagon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // ウィンドウをドラッグ移動可能にする
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();
                }
                catch { }
            }
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var f in files)
            {
                try
                {
                    if (File.Exists(f) || Directory.Exists(f))
                    {
                        var item = new AppItem
                        {
                            Name = System.IO.Path.GetFileNameWithoutExtension(f),
                            Path = f,
                            Icon = GetIconImageSource(f)
                        };

                        // 重複がなければ追加 (noop patch)
                        if (!AppItems.Any(a => string.Equals(a.Path, item.Path, StringComparison.OrdinalIgnoreCase)))
                        {
                            // set initial placement based on TextPosition or center
                            double w = this.Width, h = this.Height;
                            switch ((BackgroundTextPosition ?? "Center").ToLowerInvariant())
                            {
                                case "top":
                                    item.X = w / 2 - (IconSizeValue / 2);
                                    item.Y = 40;
                                    break;
                                case "bottom":
                                    item.X = w / 2 - (IconSizeValue / 2);
                                    item.Y = h - 120;
                                    break;
                                case "left":
                                    item.X = 40;
                                    item.Y = h / 2 - (IconSizeValue / 2);
                                    break;
                                case "right":
                                    item.X = w - 120;
                                    item.Y = h / 2 - (IconSizeValue / 2);
                                    break;
                                default:
                                    item.X = w / 2 - (IconSizeValue / 2);
                                    item.Y = h / 2 - (IconSizeValue / 2);
                                    break;
                            }

                            AppItems.Add(item);
                            SaveSettings();
                        }
                    }
                }
                catch { }
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_SMALLICON = 0x000000001;

        private BitmapSource GetIconImageSource(string path)
        {
            try
            {
                SHFILEINFO shfi = new SHFILEINFO();
                IntPtr ret = SHGetFileInfo(path, 0, out shfi, (uint)Marshal.SizeOf(shfi), SHGFI_ICON | SHGFI_SMALLICON);
                if (shfi.hIcon != IntPtr.Zero)
                {
                    var bmp = Imaging.CreateBitmapSourceFromHIcon(shfi.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(48, 48));
                    DestroyIcon(shfi.hIcon);
                    return bmp;
                }
            }
            catch { }
            return null;
        }

        private void AppItem_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            var path = btn.Tag as string;
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                // ショートカット(.lnk)やexeともにShell経由で起動する
                ProcessStartInfo psi = new ProcessStartInfo(path)
                {
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"起動に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditApp_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var item = mi?.CommandParameter as AppItem;
            if (item == null) return;
            var dlg = new EditAppWindow(item.Name, item.Path) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                var idx = AppItems.IndexOf(item);
                if (idx >= 0)
                {
                    var newItem = new AppItem { Name = dlg.ResultName, Path = dlg.ResultPath, Icon = GetIconImageSource(dlg.ResultPath) };
                    AppItems[idx] = newItem;
                    SaveSettings();
                }
            }
        }

        private void DeleteApp_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var item = mi?.CommandParameter as AppItem;
            if (item == null) return;
            if (MessageBox.Show($"'{item.Name}' を削除しますか?", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                AppItems.Remove(item);
                SaveSettings();
            }
        }

        private void MoveLeftApp_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var item = mi?.CommandParameter as AppItem;
            if (item == null) return;
            var idx = AppItems.IndexOf(item);
            if (idx > 0)
            {
                AppItems.Move(idx, idx - 1);
                SaveSettings();
            }
        }

        private void MoveRightApp_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var item = mi?.CommandParameter as AppItem;
            if (item == null) return;
            var idx = AppItems.IndexOf(item);
            if (idx >= 0 && idx < AppItems.Count - 1)
            {
                AppItems.Move(idx, idx + 1);
                SaveSettings();
            }
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            string currentColor = "#1E88E5";
            if (Pentagon.Fill is SolidColorBrush scb)
            {
                currentColor = scb.Color.ToString();
            }
            var dlg = new SettingsWindow(
                this.Width,
                this.Height,
                currentColor,
                /*shape*/GetCurrentShape(),
                /*text*/string.Join("\n", BackgroundLines),
                /*textColor*/BackgroundTextColor,
                /*fontName*/BackgroundFontName,
                /*fontSize*/BackgroundFontSize,
                /*textPosition*/BackgroundTextPosition,
                /*backgroundOpacity*/BackgroundOpacity,
                /*backgroundImagePath*/BackgroundImagePath,
                /*iconSize*/IconSizeValue,
                /*listMode*/ListMode,
                /*imageMode*/BackgroundImageMode,
                /*textShadow*/BackgroundTextShadow,
                /*textShadowColor*/BackgroundTextShadowColor,
                /*lineSpacing*/BackgroundLineSpacing,
                /*fontBold*/BackgroundFontBold,
                /*fontItalic*/BackgroundFontItalic
                ,
                /*iconPlacement*/ (AppItems.Count>0? "Center" : BackgroundTextPosition),
                /*freePlacement*/ false
            ) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                this.Width = dlg.ResultWidth;
                this.Height = dlg.ResultHeight;
                try
                {
                    var col = (Color)ColorConverter.ConvertFromString(dlg.ResultColor);
                    // will set fill below depending on image/opacity

                }
                catch { }
                // apply shape
                ApplyShape(dlg.ResultShape);
                // apply background text
                BackgroundLines = dlg.ResultText?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
                BackgroundTextColor = dlg.ResultTextColor ?? "#FFFFFFFF";
                BackgroundFontName = dlg.ResultFontName ?? BackgroundFontName;
                BackgroundFontSize = dlg.ResultFontSize > 0 ? dlg.ResultFontSize : BackgroundFontSize;
                BackgroundTextPosition = dlg.ResultTextPosition ?? BackgroundTextPosition;
                BackgroundOpacity = dlg.ResultBackgroundOpacity;
                BackgroundImagePath = dlg.ResultBackgroundImagePath;
                IconSizeValue = dlg.ResultIconSize;
                ListMode = dlg.ResultListMode;
                BackgroundImageMode = dlg.ResultImageMode ?? BackgroundImageMode;
                BackgroundTextShadow = dlg.ResultTextShadow;
                BackgroundTextShadowColor = dlg.ResultTextShadowColor ?? BackgroundTextShadowColor;
                BackgroundLineSpacing = dlg.ResultLineSpacing;
                BackgroundFontBold = dlg.ResultFontBold;
                BackgroundFontItalic = dlg.ResultFontItalic;
                // placement settings
                IconPlacement = dlg.ResultIconPlacement ?? IconPlacement;
                FreePlacement = dlg.ResultFreePlacement;

                // apply icon size to the Window dependency property so bindings update
                try { this.IconSize = IconSizeValue; this.Resources["IconSize"] = IconSizeValue; } catch { }

                // apply background fill: image preferred
                ApplyBackgroundImageAndMode();

                UpdateBackgroundText();
                // toggle list/grid
                AppList.Visibility = ListMode ? Visibility.Collapsed : Visibility.Visible;
                // AppListVertical may not exist in older builds; guard
                try { AppListVertical.Visibility = ListMode ? Visibility.Visible : Visibility.Collapsed; } catch { }

                // switch items panel for free placement
                ApplyItemsPanelForPlacement();

                SaveSettings();
            }
        }

        private void ExportSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new SaveFileDialog { Filter = "Settings XML|*.xml|Settings with images (ZIP)|*.zip", FileName = "appli_settings.xml", DefaultExt = "xml" };
                if (dlg.ShowDialog() == true)
                {
                    var data = new SettingsData
                    {
                        WindowWidth = this.Width,
                        WindowHeight = this.Height,
                        BackgroundColor = (Pentagon.Fill is SolidColorBrush scb) ? scb.Color.ToString() : "#1E88E5",
                        Shape = GetCurrentShape(),
                        BackgroundLines = BackgroundLines,
                        BackgroundTextColor = BackgroundTextColor,
                        BackgroundImagePath = BackgroundImagePath,
                        BackgroundOpacity = BackgroundOpacity,
                        IconSize = IconSizeValue,
                        ListMode = ListMode,
                        FontName = BackgroundFontName,
                        FontSize = BackgroundFontSize,
                        TextPosition = BackgroundTextPosition,
                        Apps = AppItems.Select(a => new AppEntry { Name = a.Name, Path = a.Path }).ToList()
                    };

                    // Always export as XML. ZIP export skipped to avoid additional assembly references.
                    var xs = new XmlSerializer(typeof(SettingsData));
                    using (var fs = File.Create(dlg.FileName)) xs.Serialize(fs, data);
                    MessageBox.Show("設定をエクスポートしました。", "エクスポート", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エクスポートに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog { Filter = "Settings XML|*.xml" };
                if (dlg.ShowDialog() == true)
                {
                    if (MessageBox.Show("選択したファイルで現在の設定を置き換えますか?", "インポートの確認", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                    var xs = new XmlSerializer(typeof(SettingsData));
                    using (var fs = File.OpenRead(dlg.FileName))
                    {
                        var data = (SettingsData)xs.Deserialize(fs);
                            if (data != null)
                        {
                            if (data.WindowWidth > 0) this.Width = data.WindowWidth;
                            if (data.WindowHeight > 0) this.Height = data.WindowHeight;
                            try { var col = (Color)ColorConverter.ConvertFromString(data.BackgroundColor); var scb = new SolidColorBrush(col); scb.Opacity = data.BackgroundOpacity; Pentagon.Fill = scb; } catch { }
                            // shape
                            ApplyShape(data.Shape);
                            // background text
                            BackgroundLines = data.BackgroundLines ?? new List<string>();
                            BackgroundTextColor = data.BackgroundTextColor ?? "#FFFFFFFF";
                            BackgroundFontName = data.FontName ?? BackgroundFontName;
                            BackgroundFontSize = data.FontSize > 0 ? data.FontSize : BackgroundFontSize;
                            BackgroundTextPosition = data.TextPosition ?? BackgroundTextPosition;
                            BackgroundOpacity = data.BackgroundOpacity;
                            BackgroundImagePath = data.BackgroundImagePath;
                            IconSizeValue = data.IconSize > 0 ? data.IconSize : IconSizeValue;
                            ListMode = data.ListMode;
                            BackgroundImageMode = string.IsNullOrEmpty(data.ImageMode) ? BackgroundImageMode : data.ImageMode;
                            BackgroundTextShadow = data.TextShadow;
                            BackgroundTextShadowColor = data.TextShadowColor ?? BackgroundTextShadowColor;
                            BackgroundLineSpacing = data.LineSpacing;
                            BackgroundFontBold = data.FontBold;
                            BackgroundFontItalic = data.FontItalic;
                            UpdateBackgroundText();
                            AppItems.Clear();
                            if (data.Apps != null)
                            {
                                foreach (var eItem in data.Apps)
                                {
                                    AppItems.Add(new AppItem { Name = eItem.Name, Path = eItem.Path, Icon = GetIconImageSource(eItem.Path) });
                                }
                            }
                            // apply bg image if provided
                            ApplyBackgroundImageAndMode();

                            // icon size resource (guard AppListVertical assignment for older builds)
                            try { this.Resources["IconSize"] = IconSizeValue; } catch { }
                            AppList.Visibility = ListMode ? Visibility.Collapsed : Visibility.Visible;
                            try { AppListVertical.Visibility = ListMode ? Visibility.Visible : Visibility.Collapsed; } catch { }

                            SaveSettings();
                            MessageBox.Show("設定をインポートしました。", "インポート", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"インポートに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSettings()
        {
            try
            {
                var dir = System.IO.Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var data = new SettingsData
                {
                    WindowWidth = this.Width,
                    WindowHeight = this.Height,
                    BackgroundColor = (Pentagon.Fill is SolidColorBrush scb) ? scb.Color.ToString() : "#1E88E5",
                    Shape = GetCurrentShape(),
                    BackgroundLines = BackgroundLines,
                    BackgroundTextColor = BackgroundTextColor,
                    BackgroundImagePath = BackgroundImagePath,
                    BackgroundOpacity = BackgroundOpacity,
                    ImageMode = BackgroundImageMode,
                    IconSize = IconSizeValue,
                    ListMode = ListMode,
                    FontName = BackgroundFontName,
                    FontSize = BackgroundFontSize,
                    TextPosition = BackgroundTextPosition,
                    TextShadow = BackgroundTextShadow,
                    TextShadowColor = BackgroundTextShadowColor,
                    LineSpacing = BackgroundLineSpacing,
                    FontBold = BackgroundFontBold,
                    FontItalic = BackgroundFontItalic,
                    IconPlacement = IconPlacement,
                    FreePlacement = FreePlacement,
                    Apps = AppItems.Select(a => new AppEntry { Name = a.Name, Path = a.Path }).ToList()
                };
                var xs = new XmlSerializer(typeof(SettingsData));
                using (var fs = File.Create(SettingsFilePath)) xs.Serialize(fs, data);
            }
            catch { }
        }

        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFilePath)) return;
                var xs = new XmlSerializer(typeof(SettingsData));
                using (var fs = File.OpenRead(SettingsFilePath))
                {
                    var data = (SettingsData)xs.Deserialize(fs);
                    if (data != null)
                    {
                        if (data.WindowWidth > 0) this.Width = data.WindowWidth;
                        if (data.WindowHeight > 0) this.Height = data.WindowHeight;
                        try
                        {
                            var col = (Color)ColorConverter.ConvertFromString(data.BackgroundColor);
                            Pentagon.Fill = new SolidColorBrush(col);
                        }
                        catch { }
                        // shape
                        ApplyShape(data.Shape);
                        // background text
                        BackgroundLines = data.BackgroundLines ?? new List<string>();
                        BackgroundTextColor = data.BackgroundTextColor ?? "#FFFFFFFF";
                        UpdateBackgroundText();

                        AppItems.Clear();
                        if (data.Apps != null)
                        {
                            foreach (var e in data.Apps)
                            {
                                AppItems.Add(new AppItem { Name = e.Name, Path = e.Path, Icon = GetIconImageSource(e.Path), X = (e.X), Y = (e.Y) });
                            }
                        }
                        // restore placement settings
                        IconPlacement = data.IconPlacement ?? IconPlacement;
                        FreePlacement = data.FreePlacement;
                        ApplyItemsPanelForPlacement();
                    }
                }
            }
            catch { }
        }

        public class SettingsData
        {
            public double WindowWidth { get; set; }
            public double WindowHeight { get; set; }
            public string BackgroundColor { get; set; }
            public string Shape { get; set; }
            public List<string> BackgroundLines { get; set; }
            public string BackgroundTextColor { get; set; }
            public string BackgroundImagePath { get; set; }
            public double BackgroundOpacity { get; set; }
            public string ImageMode { get; set; }
            public double IconSize { get; set; }
            public bool ListMode { get; set; }
            public string FontName { get; set; }
            public double FontSize { get; set; }
            public string TextPosition { get; set; }
            public bool TextShadow { get; set; }
            public string TextShadowColor { get; set; }
            public double LineSpacing { get; set; }
            public bool FontBold { get; set; }
            public bool FontItalic { get; set; }
            public string IconPlacement { get; set; }
            public bool FreePlacement { get; set; }
            public List<AppEntry> Apps { get; set; }
        }

        public class AppEntry
        {
            public string Name { get; set; }
            public string Path { get; set; }
            // optional placement info
            public double X { get; set; }
            public double Y { get; set; }
        }

        private void ApplyItemsPanelForPlacement()
        {
            try
            {
                if (FreePlacement)
                {
                    // switch to canvas panel
                    var template = this.Resources["CanvasPanel"] as ItemsPanelTemplate;
                    if (template != null) AppList.ItemsPanel = template;
                }
                else
                {
                    // switch back to wrap panel
                    var template = this.Resources["WrapPanelTemplate"] as ItemsPanelTemplate;
                    if (template != null) AppList.ItemsPanel = template;
                }
            }
            catch { }
        }

        private class AppItem
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public BitmapSource Icon { get; set; }
            // position for free placement
            public double X { get; set; }
            public double Y { get; set; }
        }
    }
}
