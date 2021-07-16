// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.GraphicsDeviceType
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

namespace Microsoft.Iris.Render
{
    public enum GraphicsDeviceType
    {
        Unknown         = -1,

        None            = 0b0000_0000_0000_0000,
        Gdi             = 0b0000_0000_0000_0001,
        Direct3D9       = 0b0000_0000_0000_0010,
        XeDirectX9      = 0b0000_0000_0000_0011,

        Skia            = 0b0000_0000_0001_0000,
        Metal           = 0b0000_0000_0001_0001,
        OpenGL          = 0b0000_0000_0001_0010,
        Vulkan          = 0b0000_0000_0001_0011,
        Dawn            = 0b0000_0000_0001_0100,
    }
}
