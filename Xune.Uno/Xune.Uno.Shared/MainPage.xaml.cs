using SkiaSharp;
using SkiaSharp.Views.UWP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Xune.Uno
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        readonly Random rand = new Random();
        DispatcherTimer timer = new DispatcherTimer();

        public MainPage()
        {
            this.InitializeComponent();
            Microsoft.Iris.Application.Initialize();

            Canvas.PaintSurface += Canvas_PaintSurface;
            timer.Interval = new TimeSpan(17000);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            Canvas.Invalidate();
        }

        ulong t = 0;
        private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (t == ulong.MaxValue)
                t = 0;
            byte r = (byte)rand.Next(0, byte.MaxValue);
            byte g = (byte)rand.Next(0, byte.MaxValue);
            byte b = (byte)rand.Next(0, byte.MaxValue);
            e.Surface.Canvas.DrawColor(new SKColor(100, 50, (byte)(t % byte.MaxValue)));
            t++;
        }

        //private unsafe void nothing()
        //{
        //    void*** ptr = (void***)GCHandle.Alloc(new void*[1], GCHandleType.Pinned).AddrOfPinnedObject();
        //}
    }
}
