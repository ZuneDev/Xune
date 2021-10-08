using Microsoft.Iris.Render;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Threading.Tasks;
using System.Windows;

namespace Xune.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Current { get; private set; }

        private WpfRenderWindow IrisWindow { get; set; }
        private SKSurface Surface { get; set; }

        Task irisInitTask;

        public MainWindow()
        {
            InitializeComponent();

            Current = this;
            IrisWindow = new WpfRenderWindow(this);
            Canvas.PaintSurface += Canvas_InitPaintSurface;
        }

        private void Canvas_InitPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            // Initialize UI framework
            if (irisInitTask != null || Microsoft.Iris.Application.IsInitialized || Microsoft.Iris.Application.IsInitializing)
            {
                Canvas.PaintSurface -= Canvas_InitPaintSurface;
                Canvas.PaintSurface += Canvas_PaintSurface;
                Canvas.InvalidateMeasure();
                return;
            }

            Surface = e.Surface;
            irisInitTask = new Task(Init);
            irisInitTask.Start();
        }

        private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Canvas invalidated");
            IrisEngineInfo.UpdateSurface(e.Surface);
            Canvas.InvalidateMeasure();
        }

        private void Init()
        {
            Microsoft.Iris.Application.Initialize(Surface, IrisWindow);

            string pageUixPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
                "Pages", "Home.uix");
            string uiName = "Default";
            //Microsoft.Iris.Application.LoadMarkup("file://" + pageUixPath);
            Microsoft.Iris.Application.Window.RequestLoad("file://" + pageUixPath);

            Microsoft.Iris.Application.Run(new Microsoft.Iris.DeferredInvokeHandler((obj) => System.Diagnostics.Debug.WriteLine("Init done: {0}", obj)));
        }
    }
}
