﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.ColorF
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using Microsoft.Iris.Render.Common;
using Microsoft.Iris.Render.Internal;
using System;

namespace Microsoft.Iris.Render
{
  [Serializable]
  public struct ColorF
  {
    private const int ARGBAlphaShift = 24;
    private const int ARGBRedShift = 16;
    private const int ARGBGreenShift = 8;
    private const int ARGBBlueShift = 0;
    private float m_flA;
    private float m_flR;
    private float m_flG;
    private float m_flB;

    public ColorF(int red, int green, int blue) => this = ColorF.FromArgb((int) byte.MaxValue, red, green, blue);

    public ColorF(float red, float green, float blue)
    {
      this.m_flA = 1f;
      this.m_flR = red;
      this.m_flG = green;
      this.m_flB = blue;
    }

    public ColorF(int alpha, int red, int green, int blue) => this = ColorF.FromArgb(alpha, red, green, blue);

    public ColorF(float alpha, float red, float green, float blue)
    {
      this.m_flA = alpha;
      this.m_flR = red;
      this.m_flG = green;
      this.m_flB = blue;
    }

    public float R
    {
      get => this.m_flR;
      set => this.m_flR = value;
    }

    public float G
    {
      get => this.m_flG;
      set => this.m_flG = value;
    }

    public float B
    {
      get => this.m_flB;
      set => this.m_flB = value;
    }

    public float A
    {
      get => this.m_flA;
      set => this.m_flA = value;
    }

    public uint PackedInt
    {
      get => (uint) ((int) ColorF.ChannelToByte(this.m_flA) << 24 | (int) ColorF.ChannelToByte(this.m_flR) << 16 | (int) ColorF.ChannelToByte(this.m_flG) << 8) | (uint) ColorF.ChannelToByte(this.m_flB);
      set => this = ColorF.FromArgb(value);
    }

    private static float CheckByte(int value, string name)
    {
      if (value < 0 || value > (int) byte.MaxValue)
        throw new ArgumentException(string.Format("Invalid value ({0}) for {1} color channel. Expecting a value between 0 and 255.", (object) value, (object) name), name);
      return (float) value / (float) byte.MaxValue;
    }

    private static byte ChannelToByte(float flChannel) => (byte) Math.Round((double) Math.Max(Math.Min(1f, flChannel), 0.0f) * (double) byte.MaxValue);

    private static uint MakeArgb(byte alpha, byte red, byte green, byte blue) => (uint) ((int) red << 16 | (int) green << 8 | (int) blue | (int) alpha << 24);

    internal static ColorF FromCOLORREF(Win32Api.COLORREF cr) => ColorF.FromArgb(Win32Api.GetRValue(cr), Win32Api.GetGValue(cr), Win32Api.GetBValue(cr));

    public static ColorF FromArgb(uint argb) => new ColorF((int) (argb >> 24) & (int) byte.MaxValue, (int) (argb >> 16) & (int) byte.MaxValue, (int) (argb >> 8) & (int) byte.MaxValue, (int) argb & (int) byte.MaxValue);

    public static ColorF FromArgb(int alpha, int red, int green, int blue) => new ColorF(ColorF.CheckByte(alpha, nameof (alpha)), ColorF.CheckByte(red, nameof (red)), ColorF.CheckByte(green, nameof (green)), ColorF.CheckByte(blue, nameof (blue)));

    public static ColorF FromArgb(int red, int green, int blue) => ColorF.FromArgb((int) byte.MaxValue, red, green, blue);

    internal Win32Api.COLORREF ToCOLORREF() => Win32Api.RGB(ColorF.ChannelToByte(this.m_flR), ColorF.ChannelToByte(this.m_flG), ColorF.ChannelToByte(this.m_flB));

    internal Win32Api.COLORREF ToCOLORREF2() => Win32Api.ARGB(ColorF.ChannelToByte(this.m_flA), ColorF.ChannelToByte(this.m_flR), ColorF.ChannelToByte(this.m_flG), ColorF.ChannelToByte(this.m_flB));

    internal float GetValue()
    {
      float num1 = this.m_flR;
      float num2 = this.m_flR;
      if ((double) this.m_flG > (double) num1)
        num1 = this.m_flG;
      if ((double) this.m_flB > (double) num1)
        num1 = this.m_flB;
      if ((double) this.m_flG < (double) num2)
        num2 = this.m_flG;
      if ((double) this.m_flB < (double) num2)
        num2 = this.m_flB;
      return (float) (((double) num1 + (double) num2) / 2.0);
    }

    internal float GetHue()
    {
      if (Math2.WithinEpsilon(this.m_flR, this.m_flG) && Math2.WithinEpsilon(this.m_flG, this.m_flB))
        return 0.0f;
      float num1 = 0.0f;
      float num2 = this.m_flR;
      float num3 = this.m_flR;
      if ((double) this.m_flG > (double) num2)
        num2 = this.m_flG;
      if ((double) this.m_flB > (double) num2)
        num2 = this.m_flB;
      if ((double) this.m_flG < (double) num3)
        num3 = this.m_flG;
      if ((double) this.m_flB < (double) num3)
        num3 = this.m_flB;
      float num4 = num2 - num3;
      if ((double) this.m_flR == (double) num2)
        num1 = (this.m_flG - this.m_flB) / num4;
      else if ((double) this.m_flG == (double) num2)
        num1 = (float) (2.0 + ((double) this.m_flB - (double) this.m_flR) / (double) num4);
      else if ((double) this.m_flB == (double) num2)
        num1 = (float) (4.0 + ((double) this.m_flR - (double) this.m_flG) / (double) num4);
      float num5 = num1 * 60f;
      if ((double) num5 < 0.0)
        num5 += 360f;
      return num5;
    }

    internal float GetSaturation()
    {
      float num = 0.0f;
      float flValue1 = this.m_flR;
      float flValue2 = this.m_flR;
      if ((double) this.m_flG > (double) flValue1)
        flValue1 = this.m_flG;
      if ((double) this.m_flB > (double) flValue1)
        flValue1 = this.m_flB;
      if ((double) this.m_flG < (double) flValue2)
        flValue2 = this.m_flG;
      if ((double) this.m_flB < (double) flValue2)
        flValue2 = this.m_flB;
      if (!Math2.WithinEpsilon(flValue1, flValue2))
        num = ((double) flValue1 + (double) flValue2) / 2.0 > 0.5 ? (float) (((double) flValue1 - (double) flValue2) / (2.0 - (double) flValue1 - (double) flValue2)) : (float) (((double) flValue1 - (double) flValue2) / ((double) flValue1 + (double) flValue2));
      return num;
    }

    internal static ColorF FromHSV(
      float flAlpha,
      float flHue,
      float flSaturation,
      float flValue)
    {
      Debug2.Validate((double) flHue >= 0.0 && (double) flHue < 360.0, typeof (ArgumentOutOfRangeException), nameof (flHue));
      Debug2.Validate((double) flSaturation >= 0.0 && (double) flSaturation <= 1.0, typeof (ArgumentOutOfRangeException), nameof (flSaturation));
      Debug2.Validate((double) flValue >= 0.0 && (double) flValue <= 1.0, typeof (ArgumentOutOfRangeException), nameof (flValue));
      if (Math2.WithinEpsilon(flSaturation, 0.0f))
        return new ColorF(flAlpha, flValue, flValue, flValue);
      float num1 = flHue / 60f;
      int num2 = (int) Math.Floor((double) num1) % 6;
      float num3 = num1 - (float) num2;
      float num4 = flValue * (1f - flSaturation);
      float num5 = flValue * (float) (1.0 - (double) num3 * (double) flSaturation);
      float num6 = flValue * (float) (1.0 - (1.0 - (double) num3) * (double) flSaturation);
      switch (num2)
      {
        case 0:
          return new ColorF(flAlpha, flValue, num6, num4);
        case 1:
          return new ColorF(flAlpha, num5, flValue, num4);
        case 2:
          return new ColorF(flAlpha, num4, flValue, num6);
        case 3:
          return new ColorF(flAlpha, num4, num5, flValue);
        case 4:
          return new ColorF(flAlpha, num6, num4, flValue);
        case 5:
          return new ColorF(flAlpha, flValue, num4, num5);
        default:
          return new ColorF(flAlpha, 0.0f, 0.0f, 0.0f);
      }
    }

    public static bool operator ==(ColorF left, ColorF right) => Math2.WithinEpsilon(left.A, right.A) && Math2.WithinEpsilon(left.R, right.R) && Math2.WithinEpsilon(left.G, right.G) && Math2.WithinEpsilon(left.B, right.B);

    public static bool operator !=(ColorF left, ColorF right) => !Math2.WithinEpsilon(left.A, right.A) || !Math2.WithinEpsilon(left.R, right.R) || !Math2.WithinEpsilon(left.G, right.G) || !Math2.WithinEpsilon(left.B, right.B);

    public override bool Equals(object obj) => obj is ColorF colorF && this == colorF;

    public Vector4 ToVector4() => new Vector4(this.m_flR, this.m_flG, this.m_flB, this.m_flA);

    public static ColorF operator *(ColorF color, float value) => new ColorF(color.A * value, color.R * value, color.G * value, color.B * value);

    public static ColorF operator +(ColorF left, ColorF right) => new ColorF(left.A + right.A, left.R + right.R, left.G + right.G, left.B + right.B);

    public void Clamp()
    {
      this.m_flA = Math.Min(Math.Max(0.0f, this.m_flA), 1f);
      this.m_flR = Math.Min(Math.Max(0.0f, this.m_flR), 1f);
      this.m_flG = Math.Min(Math.Max(0.0f, this.m_flG), 1f);
      this.m_flB = Math.Min(Math.Max(0.0f, this.m_flB), 1f);
    }

    public override string ToString() => base.ToString();

    public override int GetHashCode() => this.PackedInt.GetHashCode();
  }
}
