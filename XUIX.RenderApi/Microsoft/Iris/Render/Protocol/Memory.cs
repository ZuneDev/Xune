// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.Protocol.Memory
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using Microsoft.Iris.Render.Common;
using System;

namespace Microsoft.Iris.Render.Protocol
{
    internal static class Memory
    {
        public static unsafe bool IsOverlap(IntPtr pvA, int cbA, IntPtr pvB, int cbB)
        {
            byte* pointer1 = (byte*)pvA.ToPointer();
            byte* pointer2 = (byte*)pvB.ToPointer();
            if (pointer1 >= pointer2 && pointer1 < pointer2 + cbB)
                return true;
            return pointer2 >= pointer1 && pointer2 < pointer1 + cbA;
        }

        public static unsafe void Zero(IntPtr pvDest, int cbZero)
        {
            uint* pointer = (uint*)pvDest.ToPointer();
            int num1 = cbZero / 4;
            int num2 = num1;
            while (num2-- > 0)
                *pointer++ = 0U;
            byte* numPtr = (byte*)pointer;
            int num3 = cbZero - num1 * 4;
            while (num3-- > 0)
                *numPtr++ = 0;
        }

        public static unsafe void Copy(IntPtr pvDest, IntPtr pvSrc, int cbCopy)
        {
            uint* pointer1 = (uint*)pvDest.ToPointer();
            uint* pointer2 = (uint*)pvSrc.ToPointer();
            int num1 = cbCopy / 4;
            int num2 = num1;
            while (num2-- > 0)
                *pointer1++ = *pointer2++;
            byte* numPtr1 = (byte*)pointer1;
            byte* numPtr2 = (byte*)pointer2;
            int num3 = cbCopy - num1 * 4;
            while (num3-- > 0)
                *numPtr1++ = *numPtr2++;
        }

        public static unsafe void ConvertEndian(
          IntPtr pvDest,
          IntPtr pvSrc,
          int cbConvert,
          int cUnitBits)
        {
            switch (cUnitBits)
            {
                case 8:
                    break;
                case 16:
                    uint* pointer1 = (uint*)pvDest.ToPointer();
                    uint* pointer2 = (uint*)pvSrc.ToPointer();
                    int num1 = cbConvert / 4;
                    while (num1-- > 0)
                    {
                        uint num2 = *pointer2++;
                        *pointer1++ = (uint)((int)((num2 & 4278190080U) >> 8) | ((int)num2 & 16711680) << 8 | (int)((num2 & 65280U) >> 8) | ((int)num2 & byte.MaxValue) << 8);
                    }
                    if (cbConvert % 4 == 0)
                        break;
                    ushort* numPtr1 = (ushort*)pointer1;
                    ushort* numPtr2 = (ushort*)pointer1;
                    ushort* numPtr3 = (ushort*)(numPtr2 + 2);
                    ushort num3 = *numPtr2;
                    ushort* numPtr4 = numPtr1;
                    ushort* numPtr5 = (ushort*)(numPtr4 + 2);
                    int num4 = (ushort)((num3 & 65280) >> 8 | (num3 & byte.MaxValue) << 8);
                    *numPtr4 = (ushort)num4;
                    break;
                case 32:
                    uint* pointer3 = (uint*)pvDest.ToPointer();
                    uint* pointer4 = (uint*)pvSrc.ToPointer();
                    int num5 = cbConvert / 4;
                    while (num5-- > 0)
                    {
                        uint num2 = *pointer4++;
                        *pointer3++ = (uint)((int)((num2 & 4278190080U) >> 24) | (int)((num2 & 16711680U) >> 8) | ((int)num2 & 65280) << 8 | ((int)num2 & byte.MaxValue) << 24);
                    }
                    break;
                default:
                    Debug2.Throw(false, "Unsupported data format");
                    break;
            }
        }

        public static unsafe void ConvertEndian(
          uint[] pvDest,
          uint[] pvSrc,
          int cbConvert,
          int cUnitBits)
        {
            switch (cUnitBits)
            {
                case 8:
                    break;
                case 16:
                    int count16 = cbConvert / 4;
                    int i16 = 0;
                    while (count16-- > 0)
                    {
                        i16++;
                        uint valSrc = pvSrc[i16];
                        pvDest[i16] = (uint)((int)((valSrc & 4278190080U) >> 8) | ((int)valSrc & 16711680) << 8 | (int)((valSrc & 65280U) >> 8) | ((int)valSrc & byte.MaxValue) << 8);
                    }
                    if (cbConvert % 4 == 0)
                        break;
                    
                    // Convert uint to ushort array
                    ushort[] destUInt16 = new ushort[pvSrc.Length * (sizeof(uint) / sizeof(ushort))];

                    ushort lastVal = (ushort)pvDest[i16];
                    int lastValConv = (ushort)((lastVal & 65280) >> 8 | (lastVal & byte.MaxValue) << 8);
                    pvDest[i16] = (ushort)lastValConv;
                    break;
                case 32:
                    int count32 = cbConvert / 4;
                    int i32 = 0;
                    while (count32-- > 0)
                    {
                        i32++;
                        pvDest[i32] = ConvertEndian(pvSrc[i32]);
                    }
                    break;
                default:
                    Debug2.Throw(false, "Unsupported data format");
                    break;
            }
        }

        public static uint ConvertEndian(uint src) => (uint)((int)((src & 4278190080U) >> 24) | (int)((src & 16711680U) >> 8) | ((int)src & 65280) << 8 | ((int)src & byte.MaxValue) << 24);
    
        public static uint[] ReinterpretAsUInt16(byte[] src)
        {
            uint[] dest = new uint[src.Length * (sizeof(uint) / sizeof(byte))];
            for (int s = 0; s < src.Length; s++)
            {
                
            }
            return dest;
        }
    }
}
