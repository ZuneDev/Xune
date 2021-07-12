﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.Extensions.GradientInformation
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Iris.Render.Extensions
{
    public class GradientInformation : IDisposable
    {
        public ImageInformation imageInfo;
        internal GCHandle gcData;

        void IDisposable.Dispose()
        {
            if (!this.gcData.IsAllocated)
                return;
            this.gcData.Free();
        }
    }
}