#if UWP

using Microsoft.Iris.Input;
using Microsoft.Iris.Library;
using Microsoft.Iris.Render.Graphics;
using Microsoft.Iris.Render.Internal;
using System;
using System.Globalization;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Microsoft.Iris.Render
{
    public sealed class XamlRenderWindow : RenderWindowBase
    {
        private bool isDragging = false;
        private GraphicsDevice graphicsDevice;
        private Window XamlWindow { get; set; }
        private ApplicationView CurrentView => ApplicationView.GetForCurrentView();
        private Rect VisibleBounds => CurrentView.VisibleBounds;

        public XamlRenderWindow(Window window)
        {
            XamlWindow = window;

            XamlWindow.SizeChanged += XamlWindow_SizeChanged;
            XamlWindow.Activated += XamlWindow_Activated;
            XamlWindow.Content.GotFocus += XamlWindowContent_GotFocus;
            XamlWindow.Content.LostFocus += XamlWindowContent_LostFocus;
            XamlWindow.Closed += XamlWindow_Closed;
            XamlWindow.Content.DragStarting += XamlWindowContent_DragStarting;
            XamlWindow.Content.DropCompleted += XamlWindowContent_DropCompleted;
            if (XamlWindow.Content is FrameworkElement elem)
            {
                elem.Loaded += XamlWindowContent_Loaded;
            }
        }

        internal override GraphicsDevice CreateGraphicsDevice(
          RenderSession session,
          GraphicsDeviceType graphicsDeviceType,
          GraphicsRenderingQuality renderingQuality)
        {
            if (graphicsDeviceType.FulfillsRequirement(GraphicsDeviceType.Skia))
                graphicsDevice = new SkiaGraphicsDevice(session);
            return GraphicsDevice;
        }

        public override int Left => (int)VisibleBounds.Left;

        public override int Top => (int)VisibleBounds.Top;

        public override int Right => (int)VisibleBounds.Right;

        public override int Bottom => (int)VisibleBounds.Bottom;

        public override int Width => (int)VisibleBounds.Width;

        public override int Height => (int)VisibleBounds.Height;

        public override HWND WindowHandle => throw new PlatformNotSupportedException("UWP cannot use HWNDs directly");

        public override Size ClientSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override Size InitialClientSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override FormPlacement InitialPlacement { set => throw new NotImplementedException(); }

        public override FormPlacement FinalPlacement => throw new NotImplementedException();

        public override int MinResizeWidth
        {
            get => (int)ApplicationView.PreferredLaunchViewSize.Width;
            set => CurrentView.SetPreferredMinSize(new Windows.Foundation.Size(value, 320));
        }
        public override int MaxResizeWidth { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override Point Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string Text
        {
            get => CurrentView.Title;
            set => CurrentView.Title = value;
        }
        public override Cursor Cursor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override Cursor IdleCursor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool Visible
        {
            get => XamlWindow.Visible;
            set => XamlWindow.Content.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        public override bool IsLoaded => XamlWindow != null;

        public override ColorF BackgroundColor
        {
            get
            {
                Windows.UI.Color color = Windows.UI.Colors.Transparent;
                if (XamlWindow.Content is Control ctl && ctl.Background is SolidColorBrush brush)
                    color = brush.Color;

                return new ColorF(color.A / 255, color.R / 255, color.G / 255, color.B / 255);
            }
            set
            {
                if (XamlWindow.Content is Control ctl)
                {
                    ctl.Background = new SolidColorBrush(new Windows.UI.Color
                    {
                        A = (byte)(value.A * 255),
                        R = (byte)(value.R * 255),
                        G = (byte)(value.G * 255),
                        B = (byte)(value.B * 255),
                    });
                }
            }
        }
        public override bool EnableExternalDragDrop
        {
            get => XamlWindow.Content.AllowDrop;
            set => XamlWindow.Content.AllowDrop = true;
        }
        public override bool IsDragInProgress { get => isDragging; set => throw new NotImplementedException(); }
        public override IDisplay CurrentDisplay { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool FullScreenExclusive
        {
            get => CurrentView.IsFullScreenMode;
            set
            {
                if (value)
                    CurrentView.TryEnterFullScreenMode();
                else
                    CurrentView.ExitFullScreenMode();
            }
        }

        public override bool ActivationState => XamlWindow.CoreWindow.ActivationMode == CoreWindowActivationMode.ActivatedInForeground;

        public override WindowState WindowState
        {
            get
            {
                if (CurrentView.ViewMode == ApplicationViewMode.CompactOverlay)
                    return WindowState.Normal;
                else if (XamlWindow.CoreWindow.ActivationMode == CoreWindowActivationMode.ActivatedNotForeground
                    || XamlWindow.CoreWindow.ActivationMode == CoreWindowActivationMode.Deactivated
                    || !XamlWindow.Visible)
                    return WindowState.Minimized;
                else if (CurrentView.AdjacentToLeftDisplayEdge && CurrentView.AdjacentToRightDisplayEdge)
                    return WindowState.Maximized;

                return WindowState.Normal;
            }
            set
            {
                switch (value)
                {
                    case WindowState.Normal:
                        XamlWindow.Content.Visibility = Visibility.Visible;
                        CurrentView.TryEnterViewModeAsync(ApplicationViewMode.Default);
                        break;
                    case WindowState.Minimized:
                        XamlWindow.Content.Visibility = Visibility.Collapsed;
                        break;
                    case WindowState.Maximized:
                        XamlWindow.Content.Visibility = Visibility.Visible;
                        break;
                }
            }
        }
        public override FormStyleInfo Styles { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override HWND AppNotifyWindow { set => throw new NotImplementedException(); }

        public override IVisualContainer VisualRoot => throw new NotImplementedException();

        TreeNode Root => throw new NotImplementedException();

        internal override bool IsClosing => throw new NotImplementedException();

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

        private void XamlWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e) => OnSizeChanged();
        private void XamlWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            OnShow(true, e.WindowActivationState == CoreWindowActivationState.CodeActivated);
            OnActivationChange();
        }
        private void XamlWindowContent_Loaded(object sender, RoutedEventArgs e) => FireLoadEvent();
        private void XamlWindowContent_GotFocus(object sender, RoutedEventArgs e) => OnSetFocus(true);
        private void XamlWindowContent_LostFocus(object sender, RoutedEventArgs e) => OnSetFocus(false);
        private void XamlWindow_Closed(object sender, CoreWindowEventArgs e) => OnClose();
        private void XamlWindowContent_DropCompleted(object sender, DropCompletedEventArgs e) => isDragging = false;
        private void XamlWindowContent_DragStarting(UIElement sender, DragStartingEventArgs args) => isDragging = true;

        public override void BringToTop() => XamlWindow.Activate();

        public override void ClientToScreen(ref Point point)
        {
            throw new NotImplementedException();
        }

        public override void Close(FormCloseReason fcrCloseReason) => XamlWindow.Close();

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
            throw new NotImplementedException();
        }

        public override void ScreenToClient(ref Point point)
        {
            throw new NotImplementedException();
        }

        public override void SetCapture(IRawInputSite captureSite, bool state)
        {
            if (state)
                XamlWindow.CoreWindow.SetPointerCapture();
            else
                XamlWindow.CoreWindow.ReleasePointerCapture();
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
            throw new NotImplementedException();
        }

        public override void SetMouseIdleOptions(Size sizeMouseIdleTolerance, uint nMouseIdleDelay)
        {
            throw new NotImplementedException();
        }

        public override void SetWindowOptions(WindowOptions options, bool enable)
        {

        }

        public override void TakeFocus() => XamlWindow.Activate();

        public override void TakeForeground(bool fForce)
        {
            if (fForce)
                XamlWindow.Activate();
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
