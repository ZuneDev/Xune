#if WPF

using Microsoft.Iris.Input;
using Microsoft.Iris.Render.Graphics;
using Microsoft.Iris.Render.Internal;
using Microsoft.Iris.Library;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using WpfCursor = System.Windows.Input.Cursor;
using WpfCursors = System.Windows.Input.Cursors;

namespace Microsoft.Iris.Render
{
    public sealed class WpfRenderWindow : RenderWindowBase
    {
        private bool isDragging;
        private bool isFullscreen;
        private GraphicsDevice graphicsDevice;
        private HWND appNotifyWindow;
        private IVisualContainer visualRoot;

        private Window WpfWindow { get; set; }
        private WindowInteropHelper InteropHelper { get; set; }

        public WpfRenderWindow(Window window)
        {
            WpfWindow = window;
            InteropHelper = new WindowInteropHelper(window);

            WpfWindow.SizeChanged += WpfWindow_SizeChanged;
            WpfWindow.Activated += WpfWindow_Activated;
            WpfWindow.GotFocus += WpfWindow_GotFocus;
            WpfWindow.LostFocus += WpfWindow_LostFocus;
            WpfWindow.Closed += WpfWindow_Closed;
            WpfWindow.Closing += WpfWindow_Closing;
            WpfWindow.DragEnter += WpfWindow_DragStarting;
            //WpfWindow.DropCompleted += XamlWindowContent_DropCompleted;
            WpfWindow.Loaded += WpfWindow_Loaded;
        }

        internal override GraphicsDevice CreateGraphicsDevice(
          RenderSession session,
          GraphicsDeviceType graphicsDeviceType,
          GraphicsRenderingQuality renderingQuality)
        {
            if (graphicsDeviceType.FulfillsRequirement(GraphicsDeviceType.Skia))
                graphicsDevice = new SkiaGraphicsDevice(session);

            visualRoot = new VisualContainer(true, session, this, new Object(), out var visual);

            return GraphicsDevice;
        }

        public override int Left => (int)WpfWindow.Left;

        public override int Top => (int)WpfWindow.Top;

        public override int Right => (int)(WpfWindow.Left + WpfWindow.Width);

        public override int Bottom => (int)(WpfWindow.Left + WpfWindow.Height);

        public override int Width => (int)WpfWindow.Width;

        public override int Height => (int)WpfWindow.Height;

        public override HWND WindowHandle => new HWND(InteropHelper.Handle);

        public override Size ClientSize
        {
            get
            {
                return new Size((int)WpfWindow.ActualWidth, (int)WpfWindow.ActualHeight);
            }
            set => throw new NotImplementedException();
        }
        public override Size InitialClientSize { get => ClientSize; set => ClientSize = value; }
        public override FormPlacement InitialPlacement
        {
            set
            {
                WpfWindow.WindowStartupLocation = WindowStartupLocation.Manual;

                WpfWindow.Left = value.NormalPosition.Left;
                WpfWindow.Top = value.NormalPosition.Top;
                WpfWindow.Width = value.NormalPosition.Width;
                WpfWindow.Height = value.NormalPosition.Height;
            }
        }

        public override FormPlacement FinalPlacement => throw new NotImplementedException();

        public override int MinResizeWidth
        {
            get => (int)WpfWindow.MinWidth;
            set => WpfWindow.MinWidth = value;
        }
        public override int MaxResizeWidth { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override Point Position
        {
            get
            {
                return new Point(Left, Top);
            }
            set
            {
                WpfWindow.Left = value.X;
                WpfWindow.Top = value.Y;
            }
        }
        public override string Text
        {
            get => WpfWindow.Title;
            set => WpfWindow.Title = value;
        }
        public override Cursor Cursor
        {
            get
            {
                CursorID id = CursorID.NotSpecified;
                if (WpfWindow.Cursor == WpfCursors.AppStarting)
                    id = CursorID.AppStarting;
                else if (WpfWindow.Cursor == WpfCursors.Arrow || WpfWindow.Cursor == WpfCursors.ArrowCD)
                    id = CursorID.Arrow;
                else if (WpfWindow.Cursor == WpfCursors.Cross)
                    id = CursorID.Crosshair;
                else if (WpfWindow.Cursor == WpfCursors.Hand)
                    id = CursorID.Hand;
                else if (WpfWindow.Cursor == WpfCursors.Help)
                    id = CursorID.Help;
                else if (WpfWindow.Cursor == WpfCursors.IBeam)
                    id = CursorID.IBeam;
                else if (WpfWindow.Cursor == WpfCursors.No)
                    id = CursorID.No;
                else if (WpfWindow.Cursor == WpfCursors.None)
                    id = CursorID.None;
                else if (WpfWindow.Cursor == WpfCursors.SizeAll)
                    id = CursorID.Size;
                else if (WpfWindow.Cursor == WpfCursors.SizeNESW)
                    id = CursorID.SizeNESW;
                else if (WpfWindow.Cursor == WpfCursors.SizeNS)
                    id = CursorID.SizeNS;
                else if (WpfWindow.Cursor == WpfCursors.SizeNWSE)
                    id = CursorID.SizeNWSE;
                else if (WpfWindow.Cursor == WpfCursors.SizeWE)
                    id = CursorID.SizeWE;

                return new(-1, id);
            }
            set
            {
                WpfWindow.Cursor = value.CursorID switch
                {
                    CursorID.AppStarting    => WpfCursors.AppStarting,
                    CursorID.Arrow          => WpfCursors.Arrow,
                    CursorID.Crosshair      => WpfCursors.Cross,
                    CursorID.Hand           => WpfCursors.Hand,
                    CursorID.Help           => WpfCursors.Help,
                    CursorID.IBeam          => WpfCursors.IBeam,
                    CursorID.No             => WpfCursors.No,
                    CursorID.None           => WpfCursors.None,
                    CursorID.Size           => WpfCursors.SizeAll,
                    CursorID.SizeNESW       => WpfCursors.SizeNESW,
                    CursorID.SizeNS         => WpfCursors.SizeNS,
                    CursorID.SizeNWSE       => WpfCursors.SizeNWSE,
                    CursorID.SizeWE         => WpfCursors.SizeWE,

                    _ => WpfCursors.Arrow,
                };
            }
        }
        public override Cursor IdleCursor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool Visible
        {
            get => WpfWindow.IsVisible;
            set
            {
                if (value)
                    WpfWindow.Show();
                else
                    WpfWindow.Hide();
            }
        }

        public override bool IsLoaded => WpfWindow.IsLoaded;

        public override ColorF BackgroundColor
        {
            get
            {
                Color color = Colors.Transparent;
                if (WpfWindow.Content is Control ctl && ctl.Background is SolidColorBrush brush)
                    color = brush.Color;

                return new ColorF(color.A / 255, color.R / 255, color.G / 255, color.B / 255);
            }
            set
            {
                if (WpfWindow.Content is Control ctl)
                {
                    ctl.Background = new SolidColorBrush(Color.FromArgb(
                        (byte)(value.A * 255),
                        (byte)(value.R * 255),
                        (byte)(value.G * 255),
                        (byte)(value.B * 255)
                    ));
                }
            }
        }
        public override bool EnableExternalDragDrop
        {
            get => WpfWindow.AllowDrop;
            set => WpfWindow.AllowDrop = true;
        }
        public override bool IsDragInProgress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IDisplay CurrentDisplay { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool FullScreenExclusive
        {
            get => isFullscreen;
            set
            {
                isFullscreen = value;
                if (isFullscreen)
                {
                    WpfWindow.WindowStyle = WindowStyle.None;
                    WpfWindow.WindowState = System.Windows.WindowState.Maximized;
                }
                else
                {
                    WpfWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                    WpfWindow.WindowState = System.Windows.WindowState.Normal;
                }
            }
        }

        public override bool ActivationState => WpfWindow.IsActive;

        public override WindowState WindowState
        {
            get
            {
                switch (WpfWindow.WindowState)
                {
                    case System.Windows.WindowState.Normal:
                        return WindowState.Normal;

                    case System.Windows.WindowState.Minimized:
                        return WindowState.Minimized;

                    case System.Windows.WindowState.Maximized:
                        return WindowState.Maximized;

                    default:
                        throw new InvalidOperationException("Unknown window state");
                }
            }
            set
            {
                switch (value)
                {
                    case WindowState.Normal:
                        WpfWindow.WindowState = System.Windows.WindowState.Normal;
                        break;
                    case WindowState.Minimized:
                        WpfWindow.WindowState = System.Windows.WindowState.Minimized;
                        break;
                    case WindowState.Maximized:
                        WpfWindow.WindowState = System.Windows.WindowState.Maximized;
                        break;
                }
            }
        }
        public override FormStyleInfo Styles
        {
            get => new();
            set
            {

            }
        }
        public override HWND AppNotifyWindow { set => appNotifyWindow = value; }

        public override IVisualContainer VisualRoot => visualRoot;

        TreeNode Root => throw new NotImplementedException();

        private bool _IsClosing = false;
        internal override bool IsClosing => _IsClosing;

        internal override bool IsSessionActive => throw new NotImplementedException();

        internal override bool IsSessionRemote => false;

        internal override bool IsSpanningMonitors => throw new NotImplementedException();

        internal override bool IsOnSecondaryMonitor => throw new NotImplementedException();

        internal override ColorF OutlineAllColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        internal override ColorF OutlineMarkedColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        internal override GraphicsDevice GraphicsDevice => graphicsDevice;

        internal override RenderSession Session => throw new NotImplementedException();

        internal override GraphicsDeviceType GraphicsDeviceType => GraphicsDeviceType.Skia;

        internal override bool IsRightToLeft => CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;

        private void WpfWindow_SizeChanged(object sender, EventArgs e) => OnSizeChanged();
        private void WpfWindow_Activated(object sender, EventArgs e)
        {
            OnShow(true, WpfWindow.ShowActivated);
            OnActivationChange();
        }
        private void WpfWindow_Loaded(object sender, RoutedEventArgs e) => FireLoadEvent();
        private void WpfWindow_GotFocus(object sender, RoutedEventArgs e) => OnSetFocus(true);
        private void WpfWindow_LostFocus(object sender, RoutedEventArgs e) => OnSetFocus(false);
        private void WpfWindow_Closed(object sender, EventArgs e) => OnClose();
        private void WpfWindow_Closing(object sender, EventArgs e) => _IsClosing = true;
        private void XamlWindowContent_DropCompleted(object sender, EventArgs e) => isDragging = false;
        private void WpfWindow_DragStarting(object sender, DragEventArgs args) => isDragging = true;

        public override void BringToTop() => WpfWindow.Activate();

        public override void ClientToScreen(ref Point point)
        {
            var p = WpfWindow.PointToScreen(new System.Windows.Point(point.X, point.Y));
            point = new Point((int)p.X, (int)p.Y);
        }

        public override void Close(FormCloseReason fcrCloseReason) => WpfWindow.Close();

        public override IHwndHostWindow CreateHwndHostWindow()
        {
            throw new NotImplementedException();
        }

        public override void ForceMouseIdle(bool fIdle)
        {
            throw new NotImplementedException();
        }

        public override void Initialize()
        {

        }

        public override void LockMouseActive(bool fActive)
        {
            throw new NotImplementedException();
        }

        public override void RefreshHitTarget()
        {
            throw new NotImplementedException();
        }

        public override void Restore()
        {
            WpfWindow.WindowState = System.Windows.WindowState.Normal;
        }

        public override void ScreenToClient(ref Point point)
        {
            var p = WpfWindow.PointFromScreen(new System.Windows.Point(point.X, point.Y));
            point = new Point((int)p.X, (int)p.Y);
        }

        public override void SetCapture(IRawInputSite captureSite, bool state)
        {
            if (state)
                WpfWindow.CaptureMouse();
            else
                WpfWindow.ReleaseMouseCapture();
        }

        public override void SetDragDropResult(uint nDragOverResult, uint nDragDropResult)
        {
            throw new NotImplementedException();
        }

        public override void SetEdgeImages(bool fActiveEdges, ShadowEdgePart[] edges)
        {
            throw new NotImplementedException();
        }

        public override void SetIcon(string sModuleName, uint nResourceID, IconFlags nOptions)
        {
            WpfWindow.Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(sModuleName));
        }

        public override void SetMouseIdleOptions(Size sizeMouseIdleTolerance, uint nMouseIdleDelay)
        {
            throw new NotImplementedException();
        }

        public override void SetWindowOptions(WindowOptions options, bool enable)
        {
            if (options.HasFlag(WindowOptions.FreeformResize))
                WpfWindow.ResizeMode = enable ? ResizeMode.CanResize : ResizeMode.NoResize;
            if (options.HasFlag(WindowOptions.EnableCursor))
                WpfWindow.Cursor = enable ? System.Windows.Input.Cursors.Arrow : System.Windows.Input.Cursors.None;
        }

        public override void TakeFocus() => WpfWindow.Focus();

        public override void TakeForeground(bool fForce)
        {
            if (fForce)
                WpfWindow.Activate();
        }

        public override void TemporarilyExitExclusiveMode()
        {
            throw new NotImplementedException();
        }

        public override void SetCurrentDisplay(IDisplay inputIDisplay) => CurrentDisplay = inputIDisplay;

        public override void SetFullScreenExclusive(bool fNewValue) => FullScreenExclusive = fNewValue;
    }
}

#endif
