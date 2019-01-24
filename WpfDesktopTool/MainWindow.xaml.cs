using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace WpfDesktopTool {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {

        readonly static string[] MUSIC_EXTENTION = new string[] { ".mp3", ".flac", ".ncm" };
        static List<string> OpenedFilePath = new List<string>();

        public MainWindow() {
            InitializeComponent();
        }

        private void LstMusic_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (TxtOutputFolder.Text == string.Empty && LstMusic.Items[0] != null) {
                TxtOutputFolder.Text = ((MusicItem)LstMusic.Items[0]).MusicInfo.Directory.FullName;
            }
        }

        private void GroupBox_Drop(object sender, DragEventArgs e) {
            var data = e.Data.GetData(DataFormats.FileDrop);
            if (data is string[] files) {
                AddFile(files);
            }
        }

        private void AddFile(string[] files) {
            foreach (string filename in files) {
                if (IsRepeated(filename)) return;
                var info = new FileInfo(filename);

                if (TxtOutputFolder.Text == "")
                    TxtOutputFolder.Text = info.Directory.FullName;

                if (ValidateMusic(info)) {
                    var item = new MusicItem(info);
                    LstMusic.Items.Add(item);
                    OpenedFilePath.Add(filename);
                }
            }
        }
        private bool IsRepeated(string filename) {
            foreach(string file in OpenedFilePath) {
                if (file == filename) return true;
            }
            return false;
        }

        private bool ValidateMusic(FileInfo fileInfo) {
            var ext = fileInfo.Extension.ToLower();
            var r = from avaExt in MUSIC_EXTENTION where avaExt == ext select avaExt;
            if (r != null && r.Count() != 0)
                return true;
            return false;
        }

        private void DeleteSeletedItems() {
            var items = LstMusic.SelectedItems;
            if (items == null || items.Count == 0) return;
            while (items.Count != 0) {
                OpenedFilePath.Remove(((MusicItem)items[0]).MusicInfo.FullName);
                LstMusic.Items.Remove(items[0]);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSeletedItems();
        }

        private void LstMusic_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var b = LstMusic.SelectedItem == null;
            BtnDelete.IsEnabled = !b;
        }

        private void LstMusic_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Delete)
                DeleteSeletedItems();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "Music File|*.ncm;*.mp3;*.flac";
            if (dialog.ShowDialog() == true) {
                AddFile(dialog.FileNames);
            }
        }

        private void BtnOutputFolder_Click(object sender, RoutedEventArgs e) {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true) {
                TxtOutputFolder.Text = dialog.SelectedPath;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {

        }
    }
}
