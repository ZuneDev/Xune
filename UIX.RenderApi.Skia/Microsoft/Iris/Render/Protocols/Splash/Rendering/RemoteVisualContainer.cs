// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.Protocols.Splash.Rendering.RemoteVisualContainer
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using Microsoft.Iris.Render.Common;
using Microsoft.Iris.Render.Protocol;
using System.Runtime.InteropServices;

namespace Microsoft.Iris.Render.Protocols.Splash.Rendering
{
    internal class RemoteVisualContainer : RemoteVisual
    {
        private RemoteCamera m_camera;

        protected RemoteVisualContainer()
        {
        }

        public static unsafe RemoteVisualContainer Create(ProtocolSplashRendering _priv_protocolInstance, IRenderHandleOwner _priv_owner) => Create();

        public static unsafe RemoteVisualContainer Create() => new RemoteVisualContainer();

        public unsafe void SendSetCamera(RemoteCamera camera)
        {
            m_camera = camera;
        }

        public override bool Equals(object other) => other is RemoteVisualContainer && this.m_renderHandle == ((RemoteObject)other).m_renderHandle;

        public override int GetHashCode() => base.GetHashCode();
    }
}
