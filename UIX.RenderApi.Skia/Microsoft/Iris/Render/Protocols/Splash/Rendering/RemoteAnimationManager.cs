// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.Protocols.Splash.Rendering.RemoteAnimationManager
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using Microsoft.Iris.Render.Common;
using Microsoft.Iris.Render.Protocol;
using System.Runtime.InteropServices;

namespace Microsoft.Iris.Render.Protocols.Splash.Rendering
{
    internal class RemoteAnimationManager : RemoteObject
    {
        private int m_pulseMs;
        private int m_framesPerSecond;
        private float m_flFactor;

        protected RemoteAnimationManager()
        {
        }

        public static unsafe RemoteAnimationManager Create(
          ProtocolSplashRendering _priv_protocolInstance,
          object _priv_owner)
        {
            RenderPort port = _priv_protocolInstance.Port;
            RENDERHANDLE managerClassHandle = _priv_protocolInstance.AnimationManager_ClassHandle;
            RemoteAnimationManager animationManager = new RemoteAnimationManager();
            //port.CreateRemoteObject(managerClassHandle, animationManager.m_renderHandle, (Message*)msg3CreatePtr);
            return animationManager;
        }

        public unsafe void SendSetAnimationRate(int nFramesPerSecond)
        {
            Debug2.Throw(this.IsValid, "Non-static method call requires an instance");
            m_framesPerSecond = nFramesPerSecond;
        }

        public unsafe void SendPulseTimeAdvance(int nPulseMs)
        {
            Debug2.Throw(this.IsValid, "Non-static method call requires an instance");
            m_pulseMs = nPulseMs;
        }

        public unsafe void SendSetGlobalSpeedAdjustment(float flFactor)
        {
            Debug2.Throw(this.IsValid, "Non-static method call requires an instance");
            m_flFactor = flFactor;
        }

        public static RemoteAnimationManager CreateFromHandle(
          RenderPort port,
          RENDERHANDLE handle)
        {
            return new RemoteAnimationManager(port, handle, true);
        }

        public static RemoteAnimationManager CreateFromExternalHandle(
          RenderPort port,
          RENDERHANDLE handle,
          IRenderHandleOwner owner)
        {
            port.RegisterKnownHandle(owner, handle);
            return new RemoteAnimationManager(port, handle, false);
        }

        protected RemoteAnimationManager(RenderPort port, RENDERHANDLE handle, bool fFreeOnDispose)
          : base(port, handle, fFreeOnDispose)
        {
        }

        protected RemoteAnimationManager(RenderPort port, IRenderHandleOwner owner)
          : base(port, owner)
        {
        }

        public override bool Equals(object other) => other is RemoteAnimationManager && this.m_renderHandle == ((RemoteObject)other).m_renderHandle;

        public override int GetHashCode() => base.GetHashCode();
    }
}
