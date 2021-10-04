// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.Internal.RenderWindow
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using Microsoft.Iris.Input;
using Microsoft.Iris.Render.Common;
using Microsoft.Iris.Render.Graphics;
using Microsoft.Iris.Render.Protocol;
using Microsoft.Iris.Render.Protocols.Splash.Desktop;
using Microsoft.Iris.Render.Protocols.Splash.Desktop.Nt;
using Microsoft.Iris.Render.Protocols.Splash.Rendering;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Iris.Render.Internal
{
    [SuppressUnmanagedCodeSecurity]
    internal sealed class RenderWindow : RenderWindowBase, IFormWindowCallback, IRenderHandleOwner
    {
        private const uint ACT_INACTIVE = 0;
        private const uint ACT_ACTIVE = 1;
        private const uint ACT_POPUP = 2;
        private const uint ACT_SYSUI = 3;
        private const uint STCH_MODE = 1;
        private const uint STCH_ACTIVATION = 2;
        private const uint STCH_VISIBILITY = 4;
        private const uint STCH_LOCATION = 8;
        private const uint STCH_SIZE = 16;
        private const uint STCH_UNPLANNED = 256;
        private const uint WM_KEYFIRST = 256;
        private const uint WM_KEYLAST = 265;
        private const uint WM_MOUSEFIRST = 512;
        private const uint WM_MOUSELAST = 526;
        private static ushort[] s_ByteOrder_FormStateCallbackMsg;
        private RenderSession m_session;
        private DisplayManager m_displayManager;
        private GraphicsDevice m_device;
        private GraphicsDeviceType m_graphicsDeviceType;
        private VisualContainer m_rootVisual;
        private int m_nX;
        private int m_nY;
        private int m_nWidth;
        private int m_nHeight;
        private int m_nMinResizeWidth;
        private int m_nMaxResizeWidth;
        private Size m_szDefault;
        private string m_stText;
        private Display m_currentDisplay;
        private bool m_fSpanningMonitors;
        private bool m_fOnSecondaryMonitor;
        private bool m_fRightToLeft;
        private bool m_fActivation;
        private bool m_fFocused;
        private bool m_fVisible;
        private bool m_fExclusive;
        private bool m_fShownBefore;
        private bool m_fSessionHasDisplay;
        private bool m_fSessionLocked;
        private bool m_fExplicitlyLocked;
        private bool m_fClosing;
        private bool m_fLoadComplete;
        private bool m_fLoadEventFired;
        private WindowState m_nWindowState;
        private FormStyleInfo m_windowStyles;
        private ColorF m_clrBackground;
        private ColorF m_clrOutlineAllColor;
        private ColorF m_clrOutlineMarkedColor;
        private Cursor m_cursor;
        private Cursor m_cursorIdle;
        private Stack m_stkWaitCursors;
        private HWND m_hwnd;
        private RemoteFormWindow m_remoteWindow;
        private RenderFlags m_renderFlags;
        private bool m_fPreProcessedInput;
        private int m_nMouseLockCount;
        private ArrayList m_partialDropData;
        private bool m_fEnableExternalDragDrop;
        private uint m_nDragDropResult;
        private uint m_nDragOverResult;
        private bool m_fIsDragInProgress;
        private FormPlacement m_finalPlacement;
        private SmartMap<ShutdownHookInfo> m_mapShutdownHooks;
        private ushort m_nextShutdownHookId;

        internal RenderWindow(
          RenderSession session,
          DisplayManager displayManager,
          GraphicsDeviceType graphicsDeviceType,
          GraphicsRenderingQuality renderingQuality)
        {
            Debug2.Validate(session != null, typeof(ArgumentNullException), "must pass a valid session");
            session.AssertOwningThread();
            Debug2.Validate(displayManager != null, typeof(ArgumentNullException), "must pass a valid DisplayManager");
            m_session = session;
            m_displayManager = displayManager;
            CreateGraphicsDevice(session, graphicsDeviceType, renderingQuality);
            m_device.RegisterWindow(this);
            m_clrBackground = new ColorF(0.0f, 0.0f, 0.0f);
            m_remoteWindow = m_session.BuildRemoteFormWindow(this, m_displayManager);
            m_fPreProcessedInput = false;
            m_cursor = Cursor.Default;
            m_cursorIdle = Cursor.Default;
            m_nWindowState = WindowState.Normal;
            m_fRightToLeft = false;
            m_fSessionHasDisplay = true;
            m_fSessionLocked = false;
            m_szDefault = new Size(640, 480);
            m_clrOutlineAllColor = new ColorF(byte.MaxValue, 0, byte.MaxValue, 0);
            m_clrOutlineMarkedColor = new ColorF(byte.MaxValue, byte.MaxValue, 0, 0);
            m_mapShutdownHooks = new SmartMap<RenderWindow.ShutdownHookInfo>();
            m_nextShutdownHookId = 1;
            m_nMouseLockCount = 0;
            SetFullScreenExclusive(false);
        }

        public override void Initialize() => BuildRootContainer();

        internal override GraphicsDevice CreateGraphicsDevice(
          RenderSession session,
          GraphicsDeviceType graphicsDeviceType,
          GraphicsRenderingQuality renderingQuality)
        {
            GraphicsDevice graphicsDevice = null;
            EngineApi.IFC(FormApi.SpGdiplusInit());
            m_graphicsDeviceType = graphicsDeviceType;
            switch (m_graphicsDeviceType)
            {
                case GraphicsDeviceType.Gdi:
                    m_device = new GdiGraphicsDevice(session);
                    break;
                case GraphicsDeviceType.Direct3D9:
                    m_device = new NtGraphicsDevice(session, renderingQuality);
                    break;
            }
            return graphicsDevice;
        }

        internal void Dispose()
        {
            Visible = false;
            m_remoteWindow.SendSetRoot(null);
            m_rootVisual.UnregisterUsage(this);
            StopRendering();
        }

        internal void StopRendering()
        {
            if (m_device != null)
            {
                m_device.Dispose();
                m_device = null;
            }
            if (m_remoteWindow != null)
            {
                m_remoteWindow.Dispose();
                m_remoteWindow = null;
            }
            EngineApi.IFC(FormApi.SpGdiplusUninit());
        }

        RENDERHANDLE IRenderHandleOwner.RenderHandle => m_remoteWindow.RenderHandle;

        void IRenderHandleOwner.OnDisconnect() => m_remoteWindow = null;

        internal RemoteFormWindow RemoteStub => m_remoteWindow;

        public override int Left => m_nX;

        public override int Top => m_nY;

        public override int Right => m_nX + m_nWidth;

        public override int Bottom => m_nY + m_nHeight;

        public override int Width => m_nWidth;

        public override int Height => m_nHeight;

        public override HWND WindowHandle => m_hwnd;

        public override Size ClientSize
        {
            get => new Size(m_nWidth, m_nHeight);
            set
            {
                if (m_nWidth == value.Width && m_nHeight == value.Height || !m_session.IsValid)
                    return;
                m_remoteWindow.SendSetSize(value);
            }
        }

        public override Size InitialClientSize
        {
            get => m_szDefault;
            set => m_szDefault = value;
        }

        public override FormPlacement InitialPlacement
        {
            set
            {
                if (!m_session.IsValid || m_remoteWindow == null || !(m_hwnd == HWND.NULL))
                    return;
                m_remoteWindow.SendSetInitialPlacement(value.ShowState, value.NormalPosition, value.MaximizedLocation);
            }
        }

        public override FormPlacement FinalPlacement => m_finalPlacement;

        public override int MinResizeWidth
        {
            get => m_nMinResizeWidth;
            set
            {
                if (m_nMinResizeWidth == value)
                    return;
                m_nMinResizeWidth = value;
                if (!m_session.IsValid)
                    return;
                m_remoteWindow.SendSetMinResizeWidth(m_nMinResizeWidth);
            }
        }

        public override int MaxResizeWidth
        {
            get => m_nMaxResizeWidth;
            set
            {
                if (m_nMaxResizeWidth == value)
                    return;
                m_nMaxResizeWidth = value;
                if (!m_session.IsValid)
                    return;
                m_remoteWindow.SendSetMaxResizeWidth(m_nMaxResizeWidth);
            }
        }

        public override Point Position
        {
            get => new Point(m_nX, m_nY);
            set
            {
                if (m_nX == value.X && m_nY == value.Y || !m_session.IsValid)
                    return;
                m_remoteWindow.SendSetPosition(value);
            }
        }

        public override string Text
        {
            get => m_stText == null ? "" : m_stText;
            set
            {
                m_stText = value;
                UpdateText(true);
            }
        }

        public override Cursor Cursor
        {
            get => m_cursor;
            set
            {
                if (m_cursor == value)
                    return;
                m_cursor = value;
                UpdateCursors();
            }
        }

        public override Cursor IdleCursor
        {
            get => m_cursorIdle;
            set
            {
                if (m_cursorIdle == value)
                    return;
                m_cursorIdle = value;
                UpdateCursors();
            }
        }

        public override bool Visible
        {
            get => m_fVisible;
            set
            {
                if (m_fVisible == value)
                    return;
                m_fVisible = value;
                if (!m_session.IsValid)
                    return;
                m_remoteWindow.SendSetVisible(m_fVisible);
            }
        }

        public override bool IsLoaded => m_fLoadEventFired;

        public override ColorF BackgroundColor
        {
            get => m_clrBackground;
            set => SetBackgroundColor(value);
        }

        public override IDisplay CurrentDisplay
        {
            get => m_currentDisplay == null ? m_displayManager.PrimaryDisplay : m_currentDisplay;
            set => SetCurrentDisplay(value);
        }

        public override void SetCurrentDisplay(IDisplay inputIDisplay)
        {
            Debug2.Validate(inputIDisplay is Display, null, "CurrentDisplay.set param MUST be a valid Display object");
            Display display = m_displayManager.DisplayFromUniqueId((inputIDisplay as Display).UniqueId);
            if (display == null)
                return;
            m_currentDisplay = display;
        }

        public override bool FullScreenExclusive
        {
            get => m_fExclusive;
            set => SetFullScreenExclusive(value);
        }

        public override void SetFullScreenExclusive(bool fNewValue)
        {
            switch (m_graphicsDeviceType)
            {
                case GraphicsDeviceType.Direct3D9:
                    m_fExclusive = fNewValue;
                    break;
                case GraphicsDeviceType.XeDirectX9:
                    m_fExclusive = true;
                    break;
                default:
                    m_fExclusive = false;
                    break;
            }
        }

        public override bool ActivationState => m_fActivation;

        public override WindowState WindowState
        {
            get => m_nWindowState;
            set
            {
                if (m_nWindowState == value)
                    return;
                m_nWindowState = value;
                if (!m_session.IsValid)
                    return;
                m_remoteWindow.SendSetMode((uint)m_nWindowState);
            }
        }

        public override FormStyleInfo Styles
        {
            get => m_windowStyles;
            set
            {
                m_windowStyles = value;
                if (!m_session.IsValid)
                    return;
                m_remoteWindow.SendSetStyles(value.uStyleRestored, value.uExStyleRestored, value.uStyleMinimized, value.uExStyleMinimized, value.uStyleMaximized, value.uExStyleMaximized, value.uStyleFullscreen, value.uExStyleFullscreen);
            }
        }

        public override HWND AppNotifyWindow
        {
            set => m_remoteWindow.INPROC_SendSetAppNotifyWindow(value);
        }

        public override IVisualContainer VisualRoot => m_rootVisual;

        internal TreeNode Root => m_rootVisual;

        internal override bool IsClosing => m_fClosing;

        internal override bool IsSessionActive => m_fSessionHasDisplay && !m_fSessionLocked;

        internal override bool IsSessionRemote => false;

        internal override bool IsSpanningMonitors => m_fSpanningMonitors;

        internal override bool IsOnSecondaryMonitor => m_fOnSecondaryMonitor;

        internal override ColorF OutlineAllColor
        {
            get => m_clrOutlineAllColor;
            set
            {
                if (!(m_clrOutlineAllColor != value))
                    return;
                m_clrOutlineAllColor = value;
                m_remoteWindow.SendSetOutlineAllColor(value);
            }
        }

        internal override ColorF OutlineMarkedColor
        {
            get => m_clrOutlineMarkedColor;
            set
            {
                if (!(m_clrOutlineMarkedColor != value))
                    return;
                m_clrOutlineMarkedColor = value;
                m_remoteWindow.SendSetOutlineMarkedColor(value);
            }
        }

        internal override GraphicsDevice GraphicsDevice => m_device;

        internal override RenderSession Session => m_session;

        internal override GraphicsDeviceType GraphicsDeviceType => m_graphicsDeviceType;

        internal override bool IsRightToLeft => m_fRightToLeft;

        public event RendererSuspendedHandler RendererSuspendedEvent;

        internal event EventHandler WindowCreatedEvent;

        private void OnCreated()
        {
            if (WindowCreatedEvent == null)
                return;
            WindowCreatedEvent(this, EventArgs.Empty);
        }

        private new void FireLoadEvent()
        {
            if (m_fLoadEventFired || !m_fLoadComplete || m_nWidth == 0 && m_nHeight == 0)
                return;
            base.FireLoadEvent();
            m_fLoadEventFired = true;
        }

        internal void NotifyDisplayReconfigured() => OnSizeChanged();

        private new void OnSizeChanged()
        {
            FireLoadEvent();
            base.OnSizeChanged();
        }

        private void OnDestroyed()
        {
        }

        private void OnTermSessionChange(uint uParam)
        {
            bool flag = false;
            bool isSessionActive = IsSessionActive;
            switch (uParam)
            {
                case 1:
                case 3:
                    m_fSessionHasDisplay = true;
                    break;
                case 2:
                case 4:
                    m_fSessionHasDisplay = false;
                    break;
                case 7:
                    m_fSessionLocked = true;
                    m_fExplicitlyLocked = true;
                    break;
                case 8:
                    m_fSessionLocked = false;
                    flag = flag || !m_fExplicitlyLocked;
                    break;
            }
            if (IsSessionActive != isSessionActive || flag)
                FireSessionActivate(IsSessionActive);
            if (uParam != 2U && uParam != 1U && (uParam != 4U && uParam != 3U))
                return;
            FireSessionConnect(uParam == 1U || uParam == 3U);
        }

        public void FireSessionActivate(bool fIsActive) => OnSessionActivate(fIsActive);

        private new void OnSessionActivate(bool fIsActive)
        {
            if (fIsActive && Visible && m_fExclusive)
            {
                Focus();
                m_remoteWindow.SendSetForeground(false);
            }
            base.OnSessionActivate(fIsActive);
        }

        private void OnNativeScreensave(bool fStart)
        {
        }

        private void OnDroppedFiles(IEnumerable files)
        {
        }

        public void EnableShellShutdownHook(string hookName, EventHandler handler) => GetShutdownHookInfo(hookName, true).Handler += handler;

        private ShutdownHookInfo GetShutdownHookInfo(string hookName, bool fCanAdd)
        {
            RenderWindow.ShutdownHookInfo desired = new RenderWindow.ShutdownHookInfo(hookName);
            if (m_mapShutdownHooks.Lookup(desired, out uint _))
                return desired;
            if (fCanAdd)
            {
                ushort uIdMsg = m_nextShutdownHookId++;
                m_mapShutdownHooks.SetValue(uIdMsg, desired);
                m_remoteWindow.SendEnableShellShutdownHook(hookName, uIdMsg);
            }
            else
                desired = null;
            return desired;
        }

        void IFormWindowCallback.OnTerminalSessionChange(RENDERHANDLE target, IntPtr wParam, IntPtr lParam)
        {
            OnTermSessionChange((uint)wParam.ToInt32());
        }

        void IFormWindowCallback.OnPrivateSysCommand(RENDERHANDLE target, IntPtr wParam, IntPtr lParam)
        {
            OnSysCommand(wParam, lParam);
        }

        void IFormWindowCallback.OnMouseIdle(RENDERHANDLE target, bool fNewIdle) => OnMouseIdle(fNewIdle);

        void IFormWindowCallback.OnCloseRequested(RENDERHANDLE target) => EngineCloseRequest();

        void IFormWindowCallback.OnLoad(RENDERHANDLE target)
        {
            m_fLoadComplete = true;
            if (m_fClosing)
                return;
            FireLoadEvent();
        }

        void IFormWindowCallback.OnWindowDestroyed(
          RENDERHANDLE target,
          uint nFinalShowState,
          Rectangle rcFinalPosition,
          Point ptFinalMaximizedLocation)
        {
            m_finalPlacement.NormalPosition = rcFinalPosition;
            m_finalPlacement.MaximizedLocation = ptFinalMaximizedLocation;
            m_finalPlacement.ShowState = nFinalShowState;
            m_hwnd = HWND.NULL;
            m_fClosing = true;
            base.OnClose();
        }

        void IFormWindowCallback.OnWindowCreated(RENDERHANDLE target, HWND hWnd)
        {
            m_hwnd = hWnd;
            m_device.PostCreate();
            UpdateText(false);
            OnCreated();
        }

        unsafe void IFormWindowCallback.OnStateChange(RENDERHANDLE target, Message* pmsgRaw)
        {
            RenderWindow.FormStateCallbackMsg* stateCallbackMsgPtr = (RenderWindow.FormStateCallbackMsg*)pmsgRaw;
            if (m_session.IsForeignByteOrderOnWindowing)
                MarshalHelper.SwapByteOrder((byte*)stateCallbackMsgPtr, ref s_ByteOrder_FormStateCallbackMsg, typeof(RenderWindow.FormStateCallbackMsg), 0, 0);
            uint num = 0;
            if (m_currentDisplay != null)
                num = m_currentDisplay.UniqueId;
            bool flag1 = stateCallbackMsgPtr->fOnSecondaryMonitor != 0;
            bool flag2 = stateCallbackMsgPtr->cSpanningMonitors > 1U;
            bool flag3 = false;
            if (stateCallbackMsgPtr->idDisplay != uint.MaxValue)
                flag3 = (int)num != (int)stateCallbackMsgPtr->idDisplay || m_fOnSecondaryMonitor != flag1 || m_fSpanningMonitors != flag2;
            bool flag4 = m_nX != stateCallbackMsgPtr->rcWindowGlobal_left || m_nY != stateCallbackMsgPtr->rcWindowGlobal_top;
            bool flag5 = m_nWidth != stateCallbackMsgPtr->szClientDims_cx || m_nHeight != stateCallbackMsgPtr->szClientDims_cy;
            m_nX = stateCallbackMsgPtr->rcWindowGlobal_left;
            m_nY = stateCallbackMsgPtr->rcWindowGlobal_top;
            m_nWidth = stateCallbackMsgPtr->szClientDims_cx;
            m_nHeight = stateCallbackMsgPtr->szClientDims_cy;
            m_nWindowState = (WindowState)stateCallbackMsgPtr->uCurrentMode;
            m_fVisible = stateCallbackMsgPtr->fVisible != 0;
            m_fSpanningMonitors = flag2;
            m_fOnSecondaryMonitor = flag1;
            SetFullScreenExclusive(stateCallbackMsgPtr->fExclusive != 0);
            bool fActivation = m_fActivation;
            m_fActivation = stateCallbackMsgPtr->uActivation == 1U;
            if (flag3)
                m_currentDisplay = m_displayManager.DisplayFromUniqueId(stateCallbackMsgPtr->idDisplay);
            if (flag3)
                OnMonitorChanged();
            if (flag4)
                OnLocationChanged();
            if (flag5)
            {
                m_rootVisual.Size = new Vector2(m_nWidth, m_nHeight);
                OnSizeChanged();
            }
            if (((int)stateCallbackMsgPtr->uRecentlyChanged & 1) != 0)
                OnWindowStateChanged(((int)stateCallbackMsgPtr->uRecentlyChanged & 256) != 0);
            if (((int)stateCallbackMsgPtr->uRecentlyChanged & 4) != 0)
            {
                bool fFirstShow = m_fVisible && !m_fShownBefore;
                if (m_fVisible)
                    m_fShownBefore = true;
                OnShow(m_fVisible, fFirstShow);
            }
            if (((int)stateCallbackMsgPtr->uRecentlyChanged & 2) == 0 || m_fActivation == fActivation)
                return;
            OnActivationChange();
        }

        void IFormWindowCallback.OnPartialDrop(RENDERHANDLE target, string file)
        {
            if (m_partialDropData == null)
                m_partialDropData = new ArrayList();
            m_partialDropData.Add(file);
        }

        void IFormWindowCallback.OnDropComplete(RENDERHANDLE target)
        {
            if (m_partialDropData == null || m_partialDropData.Count <= 0)
                return;
            IEnumerable partialDropData = m_partialDropData;
            m_partialDropData = null;
            OnDroppedFiles(partialDropData);
        }

        void IFormWindowCallback.OnSetFocus(RENDERHANDLE target, bool focused, HWND hwndFocusChange)
        {
            m_fFocused = focused;
            base.OnSetFocus(focused);
        }

        void IFormWindowCallback.OnShellShutdownHook(RENDERHANDLE target, ushort hookId)
        {
            RenderWindow.ShutdownHookInfo shutdownHookInfo;
            if (!m_mapShutdownHooks.TryGetValue(hookId, out shutdownHookInfo))
                return;
            shutdownHookInfo.OnHook(this, EventArgs.Empty);
        }

        void IFormWindowCallback.OnNativeScreensave(RENDERHANDLE target, bool fStartScreensave)
        {
            m_session.DeferredInvoke(new DeferredHandler(DoNativeScreensave), fStartScreensave, DeferredInvokePriority.Idle);
        }

        private void DoNativeScreensave(object objParam) => OnNativeScreensave((bool)objParam);

        private void DoPopAnimations(object objParam)
        {
        }

        void IFormWindowCallback.OnRendererSuspended(RENDERHANDLE target, bool fSuspended)
        {
            FireRendererSuspended(fSuspended);
        }

        private void FireRendererSuspended(bool fSuspended)
        {
            if (RendererSuspendedEvent == null)
                return;
            RendererSuspendedEvent(m_device, new RenderWindow.RendererSuspendedArgs(fSuspended));
        }

        internal RenderFlags GlobalRenderFlags
        {
            get => m_renderFlags;
            set => SetGlobalRenderFlags(value, RenderFlags.All);
        }

        internal bool SetGlobalRenderFlags(RenderFlags flags, RenderFlags mask)
        {
            bool flag = false;
            RenderWindow.RenderFlags renderFlags = m_renderFlags & ~mask | flags & mask;
            if (m_renderFlags != renderFlags)
            {
                m_renderFlags = renderFlags;
                flag = true;
                m_remoteWindow.SendChangeDataBits((uint)flags, (uint)mask);
            }
            return flag;
        }

        public override void SetWindowOptions(WindowOptions optionMask, bool enable)
        {
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendSetOptions((uint)optionMask, enable ? (uint)optionMask : 0U);
        }

        public override void SetMouseIdleOptions(Size mouseIdleTolerance, uint mouseIdleDelay)
        {
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendSetMouseIdleOptions(mouseIdleTolerance, mouseIdleDelay);
        }

        public void ForwardWindowMessage(ref Win32Api.MSG msg)
        {
            if (msg.message < 256U || msg.message > 265U)
                return;
            OnForwardWndMsg(msg.message, msg.wParam, msg.lParam);
        }

        private void BuildRootContainer()
        {
            m_remoteWindow.SendSetSize(m_szDefault);
            m_remoteWindow.SendCreateRootContainer();
            m_remoteWindow.SendSetHitMasks(1U, 2U, 4U, 8U);
            RemoteVisual remoteVisual;
            m_rootVisual = new VisualContainer(true, m_session, this, null, out remoteVisual);
            m_rootVisual.RegisterUsage(this);
            m_remoteWindow.SendSetRoot(remoteVisual);
            m_rootVisual.Size = new Vector2(m_nWidth, m_nHeight);
        }

        public override void ClientToScreen(ref Point point)
        {
            Win32Api.POINT pt;
            pt.x = !m_fRightToLeft ? point.X : ClientSize.Width - point.X;
            pt.y = point.Y;
            Win32Api.ClientToScreen(m_hwnd, ref pt);
            point.X = pt.x;
            point.Y = pt.y;
        }

        public override void ScreenToClient(ref Point point)
        {
            Win32Api.POINT pt;
            pt.x = point.X;
            pt.y = point.Y;
            Win32Api.ScreenToClient(m_hwnd, ref pt);
            point.X = !m_fRightToLeft ? pt.x : ClientSize.Width - pt.x;
            point.Y = pt.y;
        }

        public override void TakeFocus() => Focus();

        private void Focus()
        {
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendTakeFocus();
        }

        public override void TakeForeground(bool fForce)
        {
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendSetForeground(fForce);
        }

        public override void BringToTop()
        {
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendBringToTop();
        }

        public override void RefreshHitTarget()
        {
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendRefreshHitTarget();
        }

        private void EngineCloseRequest()
        {
            if (CatchCloseRequests)
                base.OnCloseRequest();
            else
                Close(FormCloseReason.RendererRequest);
        }

        public override void Close(FormCloseReason nReason)
        {
            if (m_fClosing)
                return;
            ForceCloseWorker(nReason);
        }

        private void ForceClose() => ForceCloseWorker(FormCloseReason.ForcedClose);

        private void ForceCloseWorker(FormCloseReason nReason)
        {
            if (m_fClosing)
                return;
            m_fClosing = true;
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendDestroy();
        }

        public void PushWaitCursor(Cursor cursor)
        {
            if (m_stkWaitCursors == null)
                m_stkWaitCursors = new Stack();
            m_stkWaitCursors.Push(cursor);
            UpdateCursors();
        }

        public void PopWaitCursor()
        {
            m_stkWaitCursors.Pop();
            UpdateCursors();
        }

        public override void ForceMouseIdle(bool fIdle)
        {
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendForceMouseIdle(fIdle);
        }

        public override void SetCapture(IRawInputSite captureSite, bool state)
        {
            if (captureSite == null)
                return;
            m_remoteWindow.SendSetCapture((captureSite as Visual).RemoteStub, state);
        }

        public void SetBackgroundColor(ColorF color)
        {
            if (!(m_clrBackground != color))
                return;
            m_clrBackground = color;
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendSetBackgroundColor(color);
        }

        public override void SetIcon(string sModuleName, uint nResourceID, IconFlags nOptions)
        {
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendSetIcon(sModuleName, nResourceID, (uint)nOptions);
        }

        public override void SetEdgeImages(bool fActiveEdges, ShadowEdgePart[] edges)
        {
            if (!m_session.IsValid)
                return;
            Debug2.Validate(edges != null, null, "Must pass non-null edges set");
            Debug2.Validate(edges.Length == 4, null, "Must pass exactly 4 edges, LTRB");
            for (int index = 0; index < 4; ++index)
            {
                Debug2.Validate(edges[index].ModuleName != null, null, "Must pass non-null ModuleName:" + index.ToString());
                Debug2.Validate(edges[index].ResourceName != null, null, "Must pass non-null ResourceName:" + index.ToString());
            }
            Debug2.Validate(string.Equals(edges[0].ModuleName, edges[1].ModuleName), null, "Module names differ - not supported");
            Debug2.Validate(string.Equals(edges[0].ModuleName, edges[2].ModuleName), null, "Module names differ - not supported");
            Debug2.Validate(string.Equals(edges[0].ModuleName, edges[3].ModuleName), null, "Module names differ - not supported");
            Debug2.Validate(edges[0].SplitPoints == edges[2].SplitPoints, null, "L+R splits differ - not supported");
            Debug2.Validate(edges[1].SplitPoints == edges[3].SplitPoints, null, "T+B splits differ - not supported");
            Inset insetSplits = new Inset(edges[1].SplitPoints.Left, edges[0].SplitPoints.Top, edges[1].SplitPoints.Right, edges[0].SplitPoints.Bottom);
            m_remoteWindow.SendSetEdgeImageParts(fActiveEdges, edges[0].ModuleName, edges[0].ResourceName, edges[1].ResourceName, edges[2].ResourceName, edges[3].ResourceName, insetSplits);
        }

        public override bool EnableExternalDragDrop
        {
            get => m_fEnableExternalDragDrop;
            set
            {
                if (m_fEnableExternalDragDrop == value)
                    return;
                m_fEnableExternalDragDrop = value;
                if (!m_session.IsValid)
                    return;
                m_remoteWindow.SendEnableExternalDragDrop(value);
            }
        }

        public override void SetDragDropResult(uint nDragOverResult, uint nDragDropResult)
        {
            if ((int)m_nDragOverResult == (int)nDragOverResult && (int)m_nDragDropResult == (int)nDragDropResult)
                return;
            m_nDragDropResult = nDragDropResult;
            m_nDragOverResult = nDragOverResult;
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendSetDragDropResult(nDragOverResult, nDragDropResult);
        }

        public override bool IsDragInProgress
        {
            get => m_fIsDragInProgress;
            set
            {
                if (m_fIsDragInProgress == value)
                    return;
                m_fIsDragInProgress = value;
                if (!m_session.IsValid)
                    return;
                if (m_fIsDragInProgress)
                    m_remoteWindow.SendEnterInternalDrag();
                else
                    m_remoteWindow.SendExitInternalDrag();
            }
        }

        public override void Restore()
        {
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendRestore();
        }

        public override void TemporarilyExitExclusiveMode()
        {
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendTemporarilyExitExclusiveMode();
        }

        public override IHwndHostWindow CreateHwndHostWindow() => new HwndHostWindow(this);

        public void UnlockForegroundWindow()
        {
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendUpdateForegroundLockState();
        }

        public override void LockMouseActive(bool fLock)
        {
            if (fLock)
            {
                ++m_nMouseLockCount;
                if (m_nMouseLockCount != 1)
                    return;
                SetWindowOptions(WindowOptions.LockMouseActive, true);
            }
            else
            {
                m_nMouseLockCount = Math.Max(m_nMouseLockCount - 1, 0);
                if (m_nMouseLockCount != 0)
                    return;
                SetWindowOptions(WindowOptions.LockMouseActive, false);
            }
        }

        private void UpdateText(bool fChanged)
        {
            if (!(m_hwnd != HWND.NULL) || m_stText == null && !fChanged || !m_session.IsValid)
                return;
            m_remoteWindow.SendSetText(Text);
        }

        private void UpdateCursors()
        {
            Cursor cursor1 = Cursor.NullCursor;
            Cursor nullCursor = Cursor.NullCursor;
            Cursor cursor2 = null;
            if (m_stkWaitCursors != null && m_stkWaitCursors.Count > 0)
                cursor2 = m_stkWaitCursors.Peek() as Cursor;
            Cursor cursor3;
            if (cursor2 != null)
            {
                cursor1 = cursor3 = cursor2;
            }
            else
            {
                if (m_cursor != null)
                    cursor1 = m_cursor;
                cursor3 = m_cursorIdle == null ? cursor1 : m_cursorIdle;
            }
            if (!m_session.IsValid)
                return;
            m_remoteWindow.SendSetCursors(cursor1.ResourceId, cursor3.ResourceId);
        }

        public bool IsPreProcessedInput => m_fPreProcessedInput;

        public delegate void RendererSuspendedHandler(object sender, RendererSuspendedArgs args);

        internal class ShutdownHookInfo
        {
            private string m_stHookId;

            public ShutdownHookInfo(string stHookId) => m_stHookId = stHookId;

            public event EventHandler Handler;

            public void OnHook(object sender, EventArgs args)
            {
                if (Handler == null)
                    return;
                Handler(sender, args);
            }

            public override bool Equals(object obj) => obj is ShutdownHookInfo shutdownHookInfo && m_stHookId == shutdownHookInfo.m_stHookId;

            public override int GetHashCode() => m_stHookId.GetHashCode();
        }

        internal class RendererSuspendedArgs : EventArgs
        {
            private bool m_fSuspended;

            public RendererSuspendedArgs(bool fSuspended) => m_fSuspended = fSuspended;

            public bool Suspended => m_fSuspended;
        }

        [Flags]
        internal enum RenderFlags
        {
            None = 0,
            DebugPainting = 1,
            OutlineMarked = 2,
            OutlineAll = 4,
            RenderDump = 8,
            Wireframe = 16, // 0x00000010
            InvalidationDebug = 32, // 0x00000020
            NoLighting = 64, // 0x00000040
            NoVertexAlpha = 128, // 0x00000080
            NoTextures = 256, // 0x00000100
            NoAlpha = 512, // 0x00000200
            RemoteDebugMode = 1024, // 0x00000400
            DumpRopsOneTime = 2048, // 0x00000800
            All = DumpRopsOneTime | RemoteDebugMode | NoAlpha | NoTextures | NoVertexAlpha | NoLighting | InvalidationDebug | Wireframe | RenderDump | OutlineAll | OutlineMarked | DebugPainting, // 0x00000FFF
        }

        [ComVisible(false)]
        private struct FormStateCallbackMsg
        {
            public uint cbSize;
            public int nMsg;
            public RENDERHANDLE idObjectSubject;
            public uint uRecentlyChanged;
            public uint uCurrentMode;
            public uint uActivation;
            public int fVisible;
            public int szClientDims_cx;
            public int szClientDims_cy;
            public int rcWindowGlobal_left;
            public int rcWindowGlobal_top;
            public int rcWindowGlobal_right;
            public int rcWindowGlobal_bottom;
            public uint idDisplay;
            public uint cSpanningMonitors;
            public int fExclusive;
            public int fOnSecondaryMonitor;
        }
    }
}
