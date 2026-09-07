using Microsoft.Win32;
using System.Windows;

namespace AppliManageAppForWpf
{
    public partial class EditAppWindow : Window
    {
        public string ResultName { get; private set; }
        public string ResultPath { get; private set; }

        public EditAppWindow(string name, string path)
        {
            InitializeComponent();
            NameBox.Text = name;
            PathBox.Text = path;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "実行可能ファイル|*.exe;*.lnk|すべてのファイル|*.*";
            if (dlg.ShowDialog() == true)
            {
                PathBox.Text = dlg.FileName;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text)) { MessageBox.Show("名前を入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (string.IsNullOrWhiteSpace(PathBox.Text)) { MessageBox.Show("パスを入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            ResultName = NameBox.Text.Trim();
            ResultPath = PathBox.Text.Trim();
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
