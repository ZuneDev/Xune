using Microsoft.Iris.Input;
using Microsoft.Iris.Render.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Microsoft.Iris.Render
{
    public sealed class IrisCoreWindow : RenderWindowBase
    {
        private bool isDragging = false;
        private Window XamlWindow { get; set; }
        private ApplicationView CurrentView => ApplicationView.GetForCurrentView();
        private Rect VisibleBounds => CurrentView.VisibleBounds;

        public IrisCoreWindow(Window window)
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

        public int Left => (int)VisibleBounds.Left;

        public int Top => (int)VisibleBounds.Top;

        public int Right => (int)VisibleBounds.Right;

        public int Bottom => (int)VisibleBounds.Bottom;

        public int Width => (int)VisibleBounds.Width;

        public int Height => (int)VisibleBounds.Height;

        public HWND WindowHandle => throw new PlatformNotSupportedException("UWP cannot use HWNDs directly");

        public Microsoft.Iris.Render.Size ClientSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Microsoft.Iris.Render.Size InitialClientSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public FormPlacement InitialPlacement { set => throw new NotImplementedException(); }

        public FormPlacement FinalPlacement => throw new NotImplementedException();

        public int MinResizeWidth
        {
            get => (int)ApplicationView.PreferredLaunchViewSize.Width;
            set => CurrentView.SetPreferredMinSize(new Windows.Foundation.Size(value, 320));
        }
        public int MaxResizeWidth { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Microsoft.Iris.Render.Point Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Text
        {
            get => CurrentView.Title;
            set => CurrentView.Title = value;
        }
        public Cursor Cursor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Cursor IdleCursor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Visible
        {
            get => XamlWindow.Visible;
            set => XamlWindow.Visible = value;
        }

        public bool IsLoaded => XamlWindow != null;

        public ColorF BackgroundColor
        {
            get
            {
                Windows.UI.Color color = Windows.UI.Colors.Transparent;
                if (XamlWindow.Content is FrameworkElement elem && elem.Background is SolidColorBrush brush)
                    color = brush.Color;

                return new ColorF(color.A / 255, color.R / 255, color.G / 255, color.B / 255);
            }
            set
            {
                if (XamlWindow.Content is FrameworkElement elem)
                {
                    elem.Background = new SolidColorBrush(new Windows.UI.Color
                    {
                        A = (byte)(value.A * 255),
                        R = (byte)(value.R * 255),
                        G = (byte)(value.G * 255),
                        B = (byte)(value.B * 255),
                    });
                }
            }
        }
        public bool EnableExternalDragDrop
        {
            get => XamlWindow.Content.AllowDrop;
            set => XamlWindow.Content.AllowDrop = true;
        }
        public bool IsDragInProgress { get => isDragging; set => throw new NotImplementedException(); }
        public IDisplay CurrentDisplay { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool FullScreenExclusive
        {
            get => CurrentView.IsFullScreenMode;
            set => CurrentView.TryEnterFullScreenMode();
        }

        public bool ActivationState => XamlWindow.CoreWindow.ActivationMode == CoreWindowActivationMode.ActivatedInForeground;

        public WindowState WindowState
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
                        XamlWindow.Visible = true;
                        CurrentView.TryEnterViewModeAsync(ApplicationViewMode.Default);
                        break;
                    case WindowState.Minimized:
                        XamlWindow.Visible = false;
                        break;
                    case WindowState.Maximized:
                        XamlWindow.Visible = true;
                        break;
                }
            }
        }
        public FormStyleInfo Styles { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public HWND AppNotifyWindow { set => throw new NotImplementedException(); }

        public IVisualContainer VisualRoot => throw new NotImplementedException();

        public event LocationChangedHandler LocationChangedEvent;
        public event SizeChangedHandler SizeChangedEvent;
        public event MonitorChangedHandler MonitorChangedEvent;
        public event WindowStateChangedHandler WindowStateChangedEvent;
        public event SysCommandHandler SysCommandEvent;
        public event MouseIdleHandler MouseIdleEvent;
        public event ShowHandler ShowEvent;
        public event ActivationChangeHandler ActivationChangeEvent;
        public event SessionActivateHandler SessionActivateEvent;
        public event SessionConnectHandler SessionConnectEvent;
        public event SetFocusHandler SetFocusEvent;
        public event LoadHandler LoadEvent;
        public event CloseHandler CloseEvent;
        public event CloseRequestHandler CloseRequestEvent;
        public event ForwardMessageHandler ForwardMessageEvent;

        private void XamlWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e) => SizeChangedEvent?.Invoke();
        private void XamlWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            ShowEvent?.Invoke(true, e.WindowActivationState == CoreWindowActivationState.CodeActivated);
            ActivationChangeEvent?.Invoke();
        }
        private void XamlWindowContent_Loaded(object sender, RoutedEventArgs e) => LoadEvent?.Invoke();
        private void XamlWindowContent_GotFocus(object sender, RoutedEventArgs e) => SetFocusEvent?.Invoke(true);
        private void XamlWindowContent_LostFocus(object sender, RoutedEventArgs e) => SetFocusEvent?.Invoke(false);
        private void XamlWindow_Closed(object sender, CoreWindowEventArgs e) => CloseEvent?.Invoke();
        private void XamlWindowContent_DropCompleted(object sender, DropCompletedEventArgs e) => isDragging = false;
        private void XamlWindowContent_DragStarting(UIElement sender, DragStartingEventArgs args) => isDragging = true;

        public void BringToTop() => XamlWindow.Activate();

        public void ClientToScreen(ref Microsoft.Iris.Render.Point point)
        {
            throw new NotImplementedException();
        }

        public void Close(FormCloseReason fcrCloseReason) => XamlWindow.Close();

        public IHwndHostWindow CreateHwndHostWindow()
        {
            throw new NotImplementedException();
        }

        public void ForceMouseIdle(bool fIdle)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {

        }

        public void LockMouseActive(bool fActive)
        {
            throw new NotImplementedException();
        }

        public void RefreshHitTarget()
        {
            throw new NotImplementedException();
        }

        public void Restore()
        {
            throw new NotImplementedException();
        }

        public void ScreenToClient(ref Microsoft.Iris.Render.Point point)
        {
            throw new NotImplementedException();
        }

        public void SetCapture(IRawInputSite captureSite, bool state)
        {
            if (state)
                XamlWindow.CoreWindow.SetPointerCapture();
            else
                XamlWindow.CoreWindow.ReleasePointerCapture();
        }

        public void SetDragDropResult(uint nDragOverResult, uint nDragDropResult)
        {
            throw new NotImplementedException();
        }

        public void SetEdgeImages(bool fActiveEdges, ShadowEdgePart[] edges)
        {
            throw new NotImplementedException();
        }

        public void SetIcon(string sModuleName, uint nResourceID, IconFlags nOptions)
        {
            throw new NotImplementedException();
        }

        public void SetMouseIdleOptions(Microsoft.Iris.Render.Size sizeMouseIdleTolerance, uint nMouseIdleDelay)
        {
            throw new NotImplementedException();
        }

        public void SetWindowOptions(WindowOptions options, bool enable)
        {

        }

        public void TakeFocus() => XamlWindow.Activate();

        public void TakeForeground(bool fForce)
        {
            if (fForce)
                XamlWindow.Activate();
        }

        public void TemporarilyExitExclusiveMode()
        {
            throw new NotImplementedException();
        }
    }
}
