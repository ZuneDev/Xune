using Microsoft.Iris.Render;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
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

namespace Xune.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Current { get; private set; }
        private static SKSurface Surface { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Current = this;
            Canvas.PaintSurface += Canvas_PaintSurface;
        }

        private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            // Initialize UI framework
            if (Microsoft.Iris.Application.IsInitialized || Microsoft.Iris.Application.IsInitializing)
            {
                Canvas.PaintSurface -= Canvas_PaintSurface;
                return;
            }

            Surface = e.Surface;
            var initTask = new Task(Init);
            initTask.Start();
        }

        private void Init()
        {
            Microsoft.Iris.Application.Initialize(Surface, new WpfRenderWindow(this));
            Microsoft.Iris.Application.LoadMarkup(@"file://D:\Repos\yoshiask\ZuneUIXTools\test\testA.uix");
            Microsoft.Iris.Application.Run();
        }
    }
}
