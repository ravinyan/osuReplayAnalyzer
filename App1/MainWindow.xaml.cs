using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using ReplayParsers.Decoders;
using ReplayParsers.Classes.Beatmap.osu;
using Windows.Media.Core;
using Windows.Media.Playback;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App1
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // 640 x 480
            
            myButton.Foreground = new SolidColorBrush(Colors.Turquoise);
            musicButton.Translation = new System.Numerics.Vector3(200, 200, 200);
            //Beatmap map = BeatmapDecoder.GetOsuLazerBeatmap();
            
            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);

            MediaPlayer player = new MediaPlayer();
            MediaPlayerElement playerElement = new MediaPlayerElement();
            playerElement.TransportControls.RequestedTheme = ElementTheme.Dark;
            playerElement.TransportControls.Show();

            player.Volume = 0.05;

            musicPlayer.SetMediaPlayer(player);
            

            musicPlayer.AreTransportControlsEnabled = true;
            musicPlayer.TransportControls.IsPlaybackRateButtonVisible = true;
            musicPlayer.TransportControls.IsPlaybackRateEnabled = true;
            musicPlayer.TransportControls.IsCompact = true;
            musicPlayer.TransportControls.IsZoomButtonVisible = false;
            musicPlayer.TransportControls.IsZoomEnabled = false;

            musicPlayerUI.VerticalAlignment = VerticalAlignment.Bottom;
            musicPlayerUI.HorizontalAlignment = HorizontalAlignment.Center;

            musicPlayerUI.Height = 125;
            musicPlayerUI.Width = AppWindow.ClientSize.Width;

            // https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.canvas?view=winrt-26100

        }

        void OnWindowResize(object sender, SizeChangedEventArgs e)
        {
            musicPlayerUI.Height = 125;
            musicPlayerUI.Width = AppWindow.Size.Width / 2;
            musicPlayer.Width = AppWindow.Size.Width / 2;
            musicPlayer.MaxWidth = AppWindow.Size.Width;
            musicPlayer.MaxHeight = AppWindow.Size.Height;
            
        }


        private void ShowMusic(object sender, RoutedEventArgs e)
        {
            musicPlayer.Source = MediaSource.CreateFromUri(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}\\osu\\Audio\\audio.mp3"));
        }


        async void OnClick(object sender, RoutedEventArgs e)
        {
            //FileOpenPicker picker = new FileOpenPicker()
            //{
            //    ViewMode = PickerViewMode.Thumbnail,
            //    FileTypeFilter = { "*" },
            //    SuggestedStartLocation = PickerLocationId.Desktop,
            //};
            //
            //var hwnd = WindowNative.GetWindowHandle(this);
            //InitializeWithWindow.Initialize(picker, hwnd);
            //
            //StorageFile file = await picker.PickSingleFileAsync();

            BitmapImage bg = new BitmapImage();

            bg.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\osu\\Background\\bg.jpg"); 
            background.ImageSource = bg;
        }
    }
}
