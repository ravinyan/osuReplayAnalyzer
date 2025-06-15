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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

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

            MyButton.Foreground = new SolidColorBrush(Colors.Turquoise);

            // https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.canvas?view=winrt-26100

        }

        async void OnClick(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker()
            {
                ViewMode = PickerViewMode.Thumbnail,
                FileTypeFilter = { ".jpg" },
                SuggestedStartLocation = PickerLocationId.Desktop,
            };
            
            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile file = await picker.PickSingleFileAsync();

            BitmapImage bg = new BitmapImage();
            bg.UriSource = new Uri(file.Path); 
            Background.ImageSource = bg;
        }
    }
}
