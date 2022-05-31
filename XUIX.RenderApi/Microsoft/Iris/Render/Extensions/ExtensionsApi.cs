// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.Extensions.ExtensionsApi
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using Microsoft.Iris.Render.Internal;
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
          IntPtr pvData,
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
          IntPtr pvSrc,
          uint cbSize,
          [MarshalAs(UnmanagedType.LPStruct), In] ImageRequirements req,
          BitmapOptions nOptions,
          out HSpBitmap hBmp,
          out ImageInformation info)
        {
            throw new NotImplementedException();
        }

        internal static HRESULT SpBitmapDelete(HSpBitmap hBmp)
        {
            throw new NotImplementedException();
        }

        internal static HRESULT SpSoundLoadBuffer(
          IntPtr pBuffer,
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
