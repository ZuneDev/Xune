// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.Protocols.Splash.Desktop.RemoteInputRouter
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using Microsoft.Iris.Render.Common;
using Microsoft.Iris.Render.Protocol;
using System.Runtime.InteropServices;

namespace Microsoft.Iris.Render.Protocols.Splash.Desktop
{
    internal class RemoteInputRouter : RemoteObject
    {
        protected RemoteInputRouter()
        {
        }

        public static unsafe RemoteInputRouter Create(
          ProtocolSplashDesktop _priv_protocolInstance,
          IRenderHandleOwner _priv_owner,
          LocalInputCallback ic)
        {
            RenderPort port = _priv_protocolInstance.Port;
            RENDERHANDLE routerClassHandle = _priv_protocolInstance.InputRouter_ClassHandle;
            RemoteInputRouter remoteInputRouter = new RemoteInputRouter(port, _priv_owner);
            return remoteInputRouter;
        }

        public unsafe void SendUnRegisterWithInputSource()
        {
            // TODO: Unregister with source
        }

        public unsafe void SendRegisterWithInputSource(uint idGroup, RENDERHANDLE idSource)
        {
            // TODO: Register with source
        }

        public static RemoteInputRouter CreateFromHandle(
          RenderPort port,
          RENDERHANDLE handle)
        {
            return new RemoteInputRouter(port, handle, true);
        }

        public static RemoteInputRouter CreateFromExternalHandle(
          RenderPort port,
          RENDERHANDLE handle,
          IRenderHandleOwner owner)
        {
            port.RegisterKnownHandle(owner, handle);
            return new RemoteInputRouter(port, handle, false);
        }

        protected RemoteInputRouter(RenderPort port, RENDERHANDLE handle, bool fFreeOnDispose)
          : base(port, handle, fFreeOnDispose)
        {
        }

        protected RemoteInputRouter(RenderPort port, IRenderHandleOwner owner)
          : base(port, owner)
        {
        }

        public override bool Equals(object other) => other is RemoteInputRouter && this.m_renderHandle == ((RemoteObject)other).m_renderHandle;

        public override int GetHashCode() => base.GetHashCode();
    }
}
