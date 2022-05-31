// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.GraphicsDeviceType
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

namespace Microsoft.Iris.Render
{
    public enum GraphicsDeviceType
    {
        Unknown     = -1,
        None        = 0,

        Gdi         = 1,
        Direct3D9   = 2,
        XeDirectX9  = 3,

        XUIX        = 1 << 4,
        Skia        = XUIX | 1,
        Metal       = XUIX | 2,
        OpenGL      = XUIX | 3,
        Vulkan      = XUIX | 4,
        Dawn        = XUIX | 5,
    }
}
