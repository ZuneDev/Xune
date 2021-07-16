﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.GraphicsDeviceType
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

namespace Microsoft.Iris.Render
{
    public enum GraphicsDeviceType
    {
        Unknown = -1,

        None,
        Gdi,
        Direct3D9,
        XeDirectX9,

        Metal,
        OpenGL,
        Vulkan,
        Dawn,

        /// <summary>
        /// Skia's GPU backend
        /// </summary>
        Ganesh,
        /// <summary>
        /// Skia's GPU backend
        /// </summary>
        SKGanesh = Ganesh,
    }
}
