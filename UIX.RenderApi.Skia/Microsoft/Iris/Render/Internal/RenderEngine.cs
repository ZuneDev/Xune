using Microsoft.Iris.Library;
using Microsoft.Iris.Render.Common;
using Microsoft.Iris.Render.Protocol;
using Microsoft.Iris.Render.Sound;
using System;
using System.Collections;

namespace Microsoft.Iris.Render.Internal
{
    internal class RenderEngine : RenderObject, IRenderEngine, IRenderObject, IDisposable
    {
        private static BitArray s_contexts;
        private static uint s_contextIdSeed = 1;
        private static uint s_maxContextId = 254;
        private RenderToken m_primaryToken;
        private MessagingSession m_messagingSession;
        private RenderSession m_renderSession;
        private RenderCaps m_renderCaps;
        private GraphicsDeviceType m_typeGraphics;
        private SoundDeviceType m_typeSound;
        private RenderWindowBase m_renderWindow;
        private InputSystem m_inputSystem;
        private SoundDevice m_soundDevice;
        private ContextID m_localContextId;
        private ContextID m_engineContextId;

        static RenderEngine() => s_contexts = new BitArray((int)s_maxContextId + 1);

        internal RenderEngine(IrisEngineInfo engineInfo, IRenderHost renderHost) : base(engineInfo)
        {
            Debug2.Validate(engineInfo != null, typeof(ArgumentNullException), nameof(engineInfo));
            Debug2.Validate(renderHost != null, typeof(ArgumentNullException), nameof(renderHost));
            m_engineContextId = AllocateContextId();
            m_localContextId = AllocateContextId();
            m_primaryToken = new RenderToken(engineInfo, m_localContextId, m_engineContextId, RENDERGROUP.NULL);
            m_messagingSession = new MessagingSession(renderHost, this.m_primaryToken);
            this.m_renderSession = new RenderSession(this);
            this.m_renderCaps = new RenderCaps(engineInfo.Surface);
            this.m_renderCaps.RequestCaps();
        }

        protected override void Dispose(bool fInDispose)
        {
            try
            {
                if (fInDispose)
                {
                    //this.m_renderSession.ReleaseSharedResources();
                    //if (this.m_renderWindow != null)
                    //    this.m_renderWindow.WindowCreatedEvent -= new EventHandler(OnRenderWindowCreated);
                    if (m_soundDevice != null)
                        m_soundDevice.Dispose();
                    //if (this.m_renderWindow != null)
                    //    this.m_renderWindow.Dispose();
                    if (m_inputSystem != null)
                        m_inputSystem.Dispose();
                    //if (this.m_displayManager != null)
                    //    this.m_displayManager.Dispose();
                    //if (this.m_renderSession != null)
                    //    this.m_renderSession.Dispose();
                    if (this.m_messagingSession != null)
                    {
                        if (this.m_messagingSession.IsConnected)
                            this.m_messagingSession.Disconnect();
                        this.m_messagingSession.Dispose();
                    }
                    FreeContextId(m_localContextId);
                    FreeContextId(m_engineContextId);
                }
                m_soundDevice = null;
                //this.m_displayManager = null;
                //this.m_renderWindow = null;
                //this.m_renderSession = null;
                this.m_messagingSession = null;
            }
            finally
            {
                base.Dispose(fInDispose);
            }
        }

        void IRenderEngine.Initialize(
            GraphicsDeviceType typeGraphics,
            GraphicsRenderingQuality renderingQuality,
            SoundDeviceType typeSound)
        {
            Debug2.Validate(IsGraphicsDeviceAvailable(typeGraphics, false), typeof(InvalidOperationException), "Graphics device type not available");
            Debug2.Validate(IsSoundDeviceAvailable(typeSound), typeof(InvalidOperationException), "Sound device type not available");
            m_typeGraphics = typeGraphics;
            m_typeSound = typeSound;
            m_renderSession.Initialize();
            // TODO: Set up input system
            //m_inputSystem = new InputSystem(this.m_renderSession, this.m_renderWindow);
        }

        IRenderSession IRenderEngine.Session => m_renderSession;//throw new NotSupportedException("Skia does not use render sessions"); m_renderSession;

        internal RenderSession Session => m_renderSession;

        IRenderWindow IRenderEngine.Window => m_renderWindow;

        internal RenderWindowBase Window => m_renderWindow;

        IDisplayManager IRenderEngine.DisplayManager => null;

        public InputSystem InputSystem => throw new NotImplementedException("Skia does not support input yet");// m_inputSystem;

        internal SoundDevice SoundDevice => m_soundDevice;

        internal Vector<GraphicsCaps> GraphicsCaps => this.m_renderCaps.Graphics;

        internal Vector<SoundCaps> SoundCaps => this.m_renderCaps.Sound;

        private void DoEvents(uint nTimeoutInMsecs)
        {
            MessagingSession.DoEvents(nTimeoutInMsecs);
        }

        void IRenderEngine.WaitForWork(uint nTimeoutInMsecs)
        {
            EngineApi.SpWaitMessage(nTimeoutInMsecs, IntPtr.Zero);
        }

        void IRenderEngine.FlushBatch()
        {
            
        }

        bool IRenderEngine.IsGraphicsDeviceAvailable(
          GraphicsDeviceType type,
          bool fFilterRecommended)
        {
            return IsGraphicsDeviceAvailable(type, fFilterRecommended);
        }

        private bool IsGraphicsDeviceAvailable(GraphicsDeviceType type, bool fFilterRecommended)
        {
            foreach (GraphicsCaps graphic in this.m_renderCaps.Graphics)
            {
                if (graphic.DeviceType.FulfillsRequirement(type))
                    return !fFilterRecommended || graphic.DriverWarning == 0;
            }
            return false;
        }

        bool IRenderEngine.IsSoundDeviceAvailable(SoundDeviceType type) => IsSoundDeviceAvailable(type);

        private bool IsSoundDeviceAvailable(SoundDeviceType type)
        {
            if (type == SoundDeviceType.None)
                return true;
            foreach (SoundCaps soundCaps in this.m_renderCaps.Sound)
            {
                if ((SoundDeviceType)soundCaps.DeviceType == type)
                    return true;
            }
            return false;
        }

        private void OnRenderWindowCreated(object sender, EventArgs args)
        {
            SoundDevice soundDevice = null;
            switch (m_typeSound)
            {
                case SoundDeviceType.None:
                    m_soundDevice = soundDevice;
                    break;
                case SoundDeviceType.DirectSound8:
                    //soundDevice = new Ds8SoundDevice(this.m_renderSession, m_renderWindow);
                    //goto case SoundDeviceType.None;
                case SoundDeviceType.WaveAudio:
                case SoundDeviceType.XAudio:
                case SoundDeviceType.XAudio2:
                    Debug2.Validate(false, typeof(NotImplementedException), "{0} is not yet supported");
                    break;
                default:
                    Debug2.Validate(false, typeof(ArgumentException), "typeSound");
                    break;
            }
        }

        internal static ContextID AllocateContextId()
        {
            uint num1 = 0;
            lock (s_contexts)
            {
                for (int index = 0; index < s_maxContextId; ++index)
                {
                    uint num2 = s_contextIdSeed++;
                    if (s_contextIdSeed > s_maxContextId)
                        s_contextIdSeed = 1U;
                    if (!s_contexts[(int)num2])
                    {
                        num1 = num2;
                        s_contexts[(int)num1] = true;
                        break;
                    }
                }
            }
            Debug2.Validate(num1 != 0U, typeof(InvalidOperationException), "Failed to allocate a context ID");
            return ContextID.FromUInt32(num1);
        }

        private static void FreeContextId(ContextID contextId)
        {
            uint uint32 = ContextID.ToUInt32(contextId);
            Debug2.Validate(s_contexts[(int)uint32], typeof(InvalidOperationException), "ContextID is not in use");
            lock (s_contexts)
                s_contexts[(int)uint32] = false;
        }

        protected override void Invariant()
        {
            Debug2.Validate(m_localContextId != ContextID.NULL, typeof(InvalidOperationException), "local ContextId should not be NULL while running");
            Debug2.Validate(m_engineContextId != ContextID.NULL, typeof(InvalidOperationException), "engine ContextId should not be NULL while running");
        }

        bool IRenderEngine.ProcessNativeEvents() => true;

        void IRenderEngine.InterThreadWake()
        {
            
        }
    }
}
