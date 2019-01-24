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
using MaterialDesignThemes.Wpf;
using System.IO;
using NcmDecrypter;

namespace WpfDesktopTool {
    /// <summary>
    /// MusicItem.xaml 的交互逻辑
    /// </summary>
    public partial class MusicItem : UserControl {

        readonly static PackIconKind READY_ICON = PackIconKind.CheckCircleOutline;
        readonly static PackIconKind ALERT_ICON = PackIconKind.AlertCircleOutline;
        readonly static PackIconKind IGNORED_ICON = PackIconKind.CheckboxMarkedCircleOutline;
        readonly static PackIconKind UNCONVERTABLE_ALERT_ICON = PackIconKind.AlertCircle;
        readonly static PackIconKind COMPELETED_ICON = PackIconKind.CheckCircle;

        readonly static Brush COMPELETED_ICON_BRUSH = new SolidColorBrush(Color.FromRgb(0, 200, 83));
        readonly static Brush UNCONVERTABLE_ALERT_ICON_BRUSH = new SolidColorBrush(Color.FromRgb(243, 67, 55));
        readonly static Brush IGNORED_TITLE_BRUSH = new SolidColorBrush(Color.FromRgb(135, 135, 135));
        readonly static Brush IGNORED_ICON_BRUSH = new SolidColorBrush(Color.FromRgb(135, 135, 135));
        readonly static Brush IGNORED_ARTIST_BRUSH = new SolidColorBrush(Color.FromRgb(185, 185, 185));
        readonly static double IGNORED_COVER_OPACTY = 0.5d;
        
        readonly static Brush NORMAL_TITLE_BRUSH = new SolidColorBrush(Color.FromRgb(75, 75, 75));
        readonly static Brush NORMAL_ICON_BRUSH = new SolidColorBrush(Color.FromRgb(75, 75, 75));
        readonly static Brush NORMAL_ARTIST_BRUSH = new SolidColorBrush(Color.FromRgb(147, 147, 147));
        readonly static double NORMAL_COVER_OPACTY = 1d;

        readonly static string READY_TOOLTIP = "Ready to be dumped.";
        readonly static string ALERT_TOOLTIP = "This is nither a ncm file nor a music file, it will not be dumped.";
        readonly static string IGNORED_TOOLTIP = "Not a ncm file but a music file, ignorned.";
        readonly static string UNCONVERTABLE_ALERT_TOOLTIP = "This file cannot be dumped.";
        readonly static string COMPELETED_TOOLTIP = "Dumped.";

        readonly static PackIconKind[] packIconKinds = new PackIconKind[5] { READY_ICON, ALERT_ICON, IGNORED_ICON, UNCONVERTABLE_ALERT_ICON, COMPELETED_ICON };
        readonly static string[] tooltips = new string[5] { READY_TOOLTIP, ALERT_TOOLTIP, IGNORED_TOOLTIP, UNCONVERTABLE_ALERT_TOOLTIP, COMPELETED_TOOLTIP };

        public StatesFlag State { get; private set; } = StatesFlag.Ready;
        public FileInfo MusicInfo { get; set; }
        public string MusicTitle { get => TxtTitle.Text; set => TxtTitle.Text = value; }
        public string MusicArtist { get => TxtArtist.Text; set => TxtArtist.Text = value; }
        public ImageSource MusicCover { get => ImgCover.Source; set => ImgCover.Source = value; }

        private NeteaseCrypto NcmFile;
        private TagLib.File MediaFile;

        public MusicItem() {
            InitializeComponent();
        }
        public MusicItem(FileInfo fileInfo) {
            InitializeComponent();
            MusicInfo = fileInfo;
            if (!fileInfo.Attributes.HasFlag(FileAttributes.Directory)) {
                if (fileInfo.Extension.ToLower() == ".ncm") {
                    this.SetStates(StatesFlag.Ready);
                    try {
                        NcmFile = new NeteaseCrypto(fileInfo.Open(FileMode.Open));
                    }
                    catch {
                        throw;
                    }
                    this.MusicTitle = NcmFile.Name;
                    this.MusicArtist = ArtistArray2Plain(NcmFile.Artist);
                    this.MusicCover = NcmFile.Cover;
                }
                else {
                    this.SetStates(StatesFlag.Ignored);
                    MediaFile = TagLib.File.Create(fileInfo.FullName);
                    TagLib.Tag tag = null;

                    if (fileInfo.Extension.ToLower() == ".mp3")
                        tag = MediaFile.GetTag(TagLib.TagTypes.Id3v2);
                    else if (fileInfo.Extension.ToLower() == ".flac")
                        tag = MediaFile.Tag;

                    this.MusicTitle = tag.Title;
                    this.MusicArtist = ArtistArray2Plain(tag.Performers);
                    if(tag.Pictures.Length > 0) {
                        var data = tag.Pictures[0].Data.Data;
                        var cover = new BitmapImage();
                        cover.BeginInit();
                        cover.StreamSource = new MemoryStream(data);
                        cover.EndInit();
                        cover.Freeze();
                        this.MusicCover = cover;
                    }

                    if (MusicTitle.Trim() == "")
                        MusicTitle = fileInfo.Name;
                }
            }

            Dispatcher.ShutdownStarted += (e, s) => {
                if (NcmFile != null) NcmFile.CloseFile();
                if (MediaFile != null) MediaFile.Dispose();
            };
         }

        string ArtistArray2Plain(string[] artist) {
            var r = "";
            foreach (string art in artist)
                r += art + "\\";
            return r.TrimEnd(new char[] { '\\' });
        }

        public void SetStates(StatesFlag flag) {
            this.State = flag;
            var index = (int)flag;
            IcoStates.Kind = packIconKinds[index];
            IcoStates.ToolTip = tooltips[index];
            if (flag == StatesFlag.Unconvertable) {
                IcoStates.Foreground = UNCONVERTABLE_ALERT_ICON_BRUSH;
                return;
            }
            else {
                IcoStates.Foreground = NORMAL_ICON_BRUSH;
            }
            if (flag == StatesFlag.Compeleted) {
                IcoStates.Foreground = COMPELETED_ICON_BRUSH;
                return;
            }
            else {
                IcoStates.Foreground = NORMAL_ICON_BRUSH;
            }
            if (flag == StatesFlag.Ignored) {
                TxtTitle.Foreground = IGNORED_TITLE_BRUSH;
                TxtArtist.Foreground = IGNORED_ARTIST_BRUSH;
                IcoStates.Foreground = IGNORED_ICON_BRUSH;
                ImgCover.Opacity = IGNORED_COVER_OPACTY;
                return;
            }
            else {
                TxtTitle.Foreground = NORMAL_TITLE_BRUSH;
                TxtArtist.Foreground = NORMAL_ARTIST_BRUSH;
                IcoStates.Foreground = NORMAL_ICON_BRUSH;
                ImgCover.Opacity = NORMAL_COVER_OPACTY;
            }
        }
    }

    public enum StatesFlag {
        Ready = 0,
        Alert = 1,
        Ignored = 2,
        Unconvertable = 3,
        Compeleted = 4,
    }
}
