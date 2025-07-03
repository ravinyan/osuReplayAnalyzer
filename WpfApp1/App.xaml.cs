using System.Configuration;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using Unosquare.FFME;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Change the default location of the ffmpeg binaries (same directory as application)
            // You can get the 64-bit binaries here: https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full-shared.7z
            Library.FFmpegDirectory = @"c:\ffmpeg";

            // Multi-threaded video enables the creation of independent
            // dispatcher threads to render video frames. This is an experimental feature
            // and might become deprecated in the future as no real performance enhancements
            // have been detected.
            Library.EnableWpfMultiThreadedVideo = false; // !System.Diagnostics.Debugger.IsAttached; // test with true and false

            // this is black magic for me but its from here https://stackoverflow.com/questions/13858665/disable-dpi-awareness-for-wpf-application
            var setDpiHwnd = typeof(HwndTarget).GetField("_setDpi", BindingFlags.Static | BindingFlags.NonPublic);
            setDpiHwnd?.SetValue(null, false);

            var processDpiAwareness = typeof(HwndTarget).GetProperty("ProcessDpiAwareness", BindingFlags.Static | BindingFlags.NonPublic);

            if (processDpiAwareness != null)
            {
                var underlyingType = Nullable.GetUnderlyingType(processDpiAwareness.PropertyType);

                if (underlyingType != null)
                {
                    var val = Enum.Parse(underlyingType, "PROCESS_SYSTEM_DPI_AWARE");
                    processDpiAwareness.SetValue(null, val, null);
                }
            }

            var setDpi = typeof(UIElement).GetField("_setDpi", BindingFlags.Static | BindingFlags.NonPublic);

            setDpi?.SetValue(null, false);

            var setDpiXValues = (List<double>)typeof(UIElement).GetField("DpiScaleXValues", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);

            setDpiXValues?.Insert(0, 1);

            var setDpiYValues = (List<double>)typeof(UIElement).GetField("DpiScaleYValues", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);

            setDpiYValues?.Insert(0, 1);
        }
    }

}
