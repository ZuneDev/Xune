using Microsoft.Iris;
using Microsoft.Iris.Render;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using IrisApp = Microsoft.Iris.Application;

namespace Xune.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
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
            if (irisInitTask != null || IrisApp.IsInitialized || IrisApp.IsInitializing)
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
#if DEBUG
            IrisApp.DebugSettings.TraceSettings.DebugTraceFile
                = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UIX.Skia.log");
#endif
            IrisApp.Initialize(Surface, IrisWindow);

            string pageUixPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Pages", "Home.uix");
            string uiName = "Default";
            //Microsoft.Iris.Application.LoadMarkup("file://" + pageUixPath);
            IrisApp.Window.RequestLoad("file://" + pageUixPath);

            IrisApp.Run(new DeferredInvokeHandler(InitialLoadComplete));
        }

        private void InitialLoadComplete(object args) => ThreadPool.QueueUserWorkItem(new WaitCallback(InitialLoadCompleteWorker));

        private void InitialLoadCompleteWorker(object state)
        {
            System.Diagnostics.Debug.WriteLine("Init done: {0}", state);

            IrisApp.DeferredInvoke(new DeferredInvokeHandler(InitialLoadCompleteUIState), new object[]
            {
                state
            });
        }

        private void InitialLoadCompleteUIState(object args)
        {
            System.Diagnostics.Debug.WriteLine("UI init done: {0}", args);
        }
    }
}
