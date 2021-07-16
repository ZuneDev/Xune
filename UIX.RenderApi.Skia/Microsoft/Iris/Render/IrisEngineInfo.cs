﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.IrisEngineInfo
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using Microsoft.Iris.Render.Protocol;
using SkiaSharp;
using System;

namespace Microsoft.Iris.Render
{
    public sealed class IrisEngineInfo : EngineInfo
    {
        private SKSurface m_skSurface;
        private ConnectionInfo m_connectionInfo;

        public static EngineInfo CreateLocal(SKSurface skSurface) => new IrisEngineInfo(skSurface, true);

        public static EngineInfo CreateRemote(SKSurface skSurface) => new IrisEngineInfo(skSurface, true, TransportProtocol.TCP, "127.0.0.1", false);

        internal IrisEngineInfo(SKSurface skSurface, bool isPrimary) : base(EngineType.Iris)
        {
            if (!isPrimary)
                throw new NotImplementedException("Local connections to an existing engine are not supported yet");
            this.m_connectionInfo = new LocalConnectionInfo();
            this.m_skSurface = skSurface;
        }

        internal IrisEngineInfo(
            SKSurface skSurface,
            bool isPrimary,
            TransportProtocol protocol,
            string sessionName,
            bool swapByteOrder)
            : base(EngineType.Iris)
        {
            if (!isPrimary)
                throw new NotImplementedException("Local connections to an existing engine are not supported yet");
            this.m_connectionInfo = new RemoteConnectionInfo(protocol, sessionName, swapByteOrder);
            this.m_skSurface = skSurface;
        }

        internal ConnectionInfo ConnectionInfo => this.m_connectionInfo;
        internal SKSurface Surface => this.m_skSurface;
    }
}
