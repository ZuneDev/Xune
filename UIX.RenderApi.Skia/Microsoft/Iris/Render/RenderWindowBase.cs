using Microsoft.Iris.Input;
using Microsoft.Iris.Library;
using Microsoft.Iris.Render.Common;
using Microsoft.Iris.Render.Graphics;
using Microsoft.Iris.Render.Internal;
using System;

namespace Microsoft.Iris.Render
{
    public abstract class RenderWindowBase : IRenderWindow, ITreeOwner
    {
        private RenderSession m_session;
        private GraphicsDevice m_graphicsDevice;
        private IVisualContainer m_visualRoot;

        internal virtual GraphicsDevice CreateGraphicsDevice(
          RenderSession session,
          GraphicsDeviceType graphicsDeviceType,
          GraphicsRenderingQuality renderingQuality)
        {
            m_session = session;
            if (graphicsDeviceType.FulfillsRequirement(GraphicsDeviceType.Skia))
                m_graphicsDevice = new SkiaGraphicsDevice(session);

            m_visualRoot = new VisualContainer(true, session, this, new Object(), out var visual);
            InvokePaint();

            return GraphicsDevice;
        }

        public abstract int Left { get; }
        public abstract int Top { get; }
        public abstract int Right { get; }
        public abstract int Bottom { get; }
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract HWND WindowHandle { get; }
        public abstract Size ClientSize { get; set; }
        public abstract Size InitialClientSize { get; set; }
        public abstract FormPlacement InitialPlacement { set; }
        public abstract FormPlacement FinalPlacement { get; }
        public abstract int MinResizeWidth { get; set; }
        public abstract int MaxResizeWidth { get; set; }
        public abstract Point Position { get; set; }
        public abstract string Text { get; set; }
        public abstract Cursor Cursor { get; set; }
        public abstract Cursor IdleCursor { get; set; }
        public abstract bool Visible { get; set; }
        public abstract bool IsLoaded { get; }
        public abstract ColorF BackgroundColor { get; set; }
        public abstract bool EnableExternalDragDrop { get; set; }
        public abstract bool IsDragInProgress { get; set; }
        public abstract IDisplay CurrentDisplay { get; set; }
        public abstract bool FullScreenExclusive { get; set; }
        public abstract bool ActivationState { get; }
        public abstract WindowState WindowState { get; set; }
        public abstract FormStyleInfo Styles { get; set; }
        public abstract HWND AppNotifyWindow { set; }
        public virtual IVisualContainer VisualRoot => m_visualRoot;
        TreeNode ITreeOwner.Root { get; }
        public bool CatchCloseRequests => CloseRequestEvent != null;

        internal abstract bool IsClosing { get; }

        internal abstract bool IsSessionActive { get; }

        internal abstract bool IsSessionRemote { get; }

        internal abstract bool IsSpanningMonitors { get; }

        internal abstract bool IsOnSecondaryMonitor { get; }

        internal abstract ColorF OutlineAllColor { get; set; }

        internal abstract ColorF OutlineMarkedColor { get; set; }

        internal virtual GraphicsDevice GraphicsDevice => m_graphicsDevice;

        internal virtual RenderSession Session => m_session;

        internal abstract GraphicsDeviceType GraphicsDeviceType { get; }

        internal abstract bool IsRightToLeft { get; }

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

        public abstract void BringToTop();
        public abstract void ClientToScreen(ref Point point);
        public abstract void Close(FormCloseReason fcrCloseReason);
        public abstract IHwndHostWindow CreateHwndHostWindow();
        public abstract void ForceMouseIdle(bool fIdle);
        public abstract void Initialize();
        public abstract void LockMouseActive(bool fActive);
        public abstract void RefreshHitTarget();
        public abstract void Restore();
        public abstract void ScreenToClient(ref Point point);
        public abstract void SetCapture(IRawInputSite captureSite, bool state);
        public abstract void SetCurrentDisplay(IDisplay inputIDisplay);
        public abstract void SetDragDropResult(uint nDragOverResult, uint nDragDropResult);
        public abstract void SetEdgeImages(bool fActiveEdges, ShadowEdgePart[] edges);
        public abstract void SetFullScreenExclusive(bool fNewValue);
        public abstract void SetIcon(string sModuleName, uint nResourceID, IconFlags nOptions);
        public abstract void SetMouseIdleOptions(Size sizeMouseIdleTolerance, uint nMouseIdleDelay);
        public abstract void SetWindowOptions(WindowOptions options, bool enable);
        public abstract void TakeFocus();
        public abstract void TakeForeground(bool fForce);
        public abstract void TemporarilyExitExclusiveMode();

        protected virtual void OnForwardWndMsg(uint msg, IntPtr wParam, IntPtr lParam) => ForwardMessageEvent?.Invoke(msg, wParam, lParam);
        protected virtual void FireLoadEvent() => LoadEvent?.Invoke();
        protected virtual void OnLocationChanged() => LocationChangedEvent?.Invoke(Position);
        protected virtual void OnSizeChanged() => SizeChangedEvent?.Invoke();
        protected virtual void OnMonitorChanged() => MonitorChangedEvent?.Invoke();
        protected virtual void OnWindowStateChanged(bool fUnplanned) => WindowStateChangedEvent?.Invoke(fUnplanned);
        protected virtual void OnSysCommand(IntPtr uParam1, IntPtr uParam2) => SysCommandEvent?.Invoke(uParam1, uParam2);
        protected virtual void OnMouseIdle(bool fIdle) => MouseIdleEvent?.Invoke(fIdle);
        protected virtual void OnShow(bool fShow, bool fFirstShow) => ShowEvent?.Invoke(fShow, fFirstShow);
        protected virtual void OnActivationChange() => ActivationChangeEvent?.Invoke();
        protected virtual void OnSessionActivate(bool fIsActive) => SessionActivateEvent?.Invoke(fIsActive);
        protected virtual void FireSessionConnect(bool fIsConnected) => SessionConnectEvent?.Invoke(fIsConnected);
        protected virtual void OnClose() => CloseEvent?.Invoke();
        protected virtual void OnCloseRequest() => CloseRequestEvent?.Invoke();
        protected virtual void OnSetFocus(bool focused) => SetFocusEvent?.Invoke(focused);
        
        public virtual void InvokePaint()
        {
            Session?.DeferredInvoke(new DeferredHandler((obj) =>
            {
                Debug2.WriteLine(DebugCategory.FormWindow, 0, (string)obj);
            }), "Howdy from dispatcher!", DeferredInvokePriority.VisualUpdate);
            return;

            var surface = Session.EngineInfo.Surface;
            surface.Canvas.DrawText("Howdy from UIX in WPF!", 0, 0, new SkiaSharp.SKPaint(new SkiaSharp.SKFont(SkiaSharp.SKTypeface.Default)));
        }
    }
}
