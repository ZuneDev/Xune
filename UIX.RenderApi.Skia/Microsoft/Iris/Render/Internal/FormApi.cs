﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.Internal.FormApi
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using System.Runtime.InteropServices;

namespace Microsoft.Iris.Render.Internal
{
    internal static class FormApi
    {
        private const string s_stEhRenderDll = "UIXRender.dll";

        [DllImport("UIXRender.dll")]
        public static extern HRESULT SpGdiplusInit();

        [DllImport("UIXRender.dll")]
        public static extern HRESULT SpGdiplusUninit();
    }
}
