// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.Extensions.ExtensionsApi
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using Microsoft.Iris.Render.Internal;
using SkiaSharp;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Iris.Render.Extensions
{
    [SuppressUnmanagedCodeSecurity]
    public static class ExtensionsApi
    {
        internal const ushort WAVE_FORMAT_PCM = 1;

        internal static HRESULT SpBitmapLoadFile(
          string stFileName,
          [MarshalAs(UnmanagedType.LPStruct), In] ImageRequirements req,
          BitmapOptions nOptions,
          out HSpBitmap hBmp,
          out ImageInformation info)
        {
            throw new NotImplementedException();
        } 

        internal static HRESULT SpBitmapLoadRaw(
          Size sizeActualPxl,
          int nStride,
          SurfaceFormat nFormat,
          byte[] pvData,
          [MarshalAs(UnmanagedType.LPStruct), In] ImageRequirements req,
          BitmapOptions nOptions,
          out HSpBitmap hBmp,
          out ImageInformation info)
        {
            throw new NotImplementedException();
        }

        internal static HRESULT SpBitmapLoadResource(
          Win32Api.HINSTANCE hinst,
          string stName,
          int nType,
          [MarshalAs(UnmanagedType.LPStruct), In] ImageRequirements req,
          BitmapOptions nOptions,
          out HSpBitmap hBmp,
          out ImageInformation info)
        {
            throw new NotImplementedException();
        }

        internal static HRESULT SpBitmapLoadBuffer(
          byte[] pvSrc,
          uint cbSize,
          [MarshalAs(UnmanagedType.LPStruct), In] ImageRequirements req,
          BitmapOptions nOptions,
          out HSpBitmap hBmp,
          out ImageInformation info)
        {
            hBmp = default;
            using var bitmap = SKBitmap.Decode(pvSrc);
            var final = new SKBitmap(bitmap.Height, bitmap.Width);

            using (var surface = new SKCanvas(final))
            {
                if (nOptions.HasFlag(BitmapOptions.Flip))
                    surface.Scale(-1, 1);
                surface.DrawBitmap(bitmap, 0, 0);
            }

            info = new ImageInformation
            {
                Data = new ImageData
                {
                    rgData = pvSrc
                },
                Header = new ImageHeader
                {
                    nFormat = final.Info.ColorType.ToSurfaceFormat(),
                    nStride = final.Info.RowBytes
                }
            };

            return new HRESULT(0);
        }

        internal static SurfaceFormat ToSurfaceFormat(this SKColorType sk)
        {
            switch (sk)
            {
                case SKColorType.Alpha8:
                    return SurfaceFormat.A8;
                case SKColorType.Argb4444:
                    return SurfaceFormat.ARGB16_1555;
                case SKColorType.RgbaF16:
                    return SurfaceFormat.RGB16_555;

                case SKColorType.Unknown:
                default:
                    return SurfaceFormat.External;
            }
        }

        internal static HRESULT SpBitmapDelete(HSpBitmap hBmp)
        {
            throw new NotImplementedException();
        }

        internal static HRESULT SpSoundLoadBuffer(
          byte[] pBuffer,
          int dwSize,
          SoundOptions options,
          out HSpSound hSound,
          out SoundInformation info)
        {
            throw new NotImplementedException();
        }

        internal static HRESULT SpSoundDispose(
          HSpSound hSound,
          SoundInformation info)
        {
            throw new NotImplementedException();
        }

        [Flags]
        internal enum BitmapOptions
        {
            None = 0,
            Decode = 1,
            Flip = 2,
            Valid = Flip | Decode, // 0x00000003
        }

        [Flags]
        internal enum SoundOptions
        {
            None = 0,
            Decode = 1,
            BigEndian = 2,
            Valid = BigEndian | Decode, // 0x00000003
        }

        [ComVisible(false)]
        public struct SoundHeader
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbExtraData;
            public uint cbDataSize;
        }

        [ComVisible(false)]
        public struct SoundData
        {
            public byte[] rgData;
        }

        [ComVisible(false)]
        public struct SoundInformation
        {
            public SoundHeader Header;
            public SoundData Data;
            public static SoundInformation NULL = new SoundInformation();
        }

        [ComVisible(false)]
        public struct HSpSound
        {
            public IntPtr h;
            public static readonly HSpSound NULL = new HSpSound();

            public static bool operator ==(HSpSound hA, HSpSound hB) => hA.h == hB.h;

            public static bool operator !=(HSpSound hA, HSpSound hB) => hA.h != hB.h;

            public override bool Equals(object oCompare) => oCompare is HSpSound hspSound && this.h == hspSound.h;

            public override int GetHashCode() => (int)this.h.ToInt64();
        }
    }
}
