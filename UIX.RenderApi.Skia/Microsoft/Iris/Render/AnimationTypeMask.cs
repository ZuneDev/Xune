// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.AnimationTypeMask
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using Microsoft.Iris.Render.Common;
using System;
using System.Collections;

namespace Microsoft.Iris.Render
{
  public struct AnimationTypeMask
  {
    public static readonly AnimationTypeMask Default = new AnimationTypeMask();
    public static readonly AnimationTypeMask X = new AnimationTypeMask(AnimationTypeChannel.X);
    public static readonly AnimationTypeMask Y = new AnimationTypeMask(AnimationTypeChannel.Y);
    public static readonly AnimationTypeMask Z = new AnimationTypeMask(AnimationTypeChannel.Z);
    public static readonly AnimationTypeMask W = new AnimationTypeMask(AnimationTypeChannel.W);
    public static readonly AnimationTypeMask A = new AnimationTypeMask(AnimationTypeChannel.W);
    public static readonly AnimationTypeMask R = new AnimationTypeMask(AnimationTypeChannel.X);
    public static readonly AnimationTypeMask G = new AnimationTypeMask(AnimationTypeChannel.Y);
    public static readonly AnimationTypeMask B = new AnimationTypeMask(AnimationTypeChannel.Z);
    private byte m_channelCount;
    private ushort m_maskCode;

    public AnimationTypeMask(AnimationTypeChannel channel0)
    {
      this.m_maskCode = (ushort) 0;
      this.m_channelCount = (byte) 1;
      this[0] = channel0;
    }

    public AnimationTypeMask(AnimationTypeChannel channel0, AnimationTypeChannel channel1)
    {
      this.m_maskCode = (ushort) 0;
      this.m_channelCount = (byte) 2;
      this[0] = channel0;
      this[1] = channel1;
    }

    public AnimationTypeMask(
      AnimationTypeChannel channel0,
      AnimationTypeChannel channel1,
      AnimationTypeChannel channel2)
    {
      this.m_maskCode = (ushort) 0;
      this.m_channelCount = (byte) 3;
      this[0] = channel0;
      this[1] = channel1;
      this[2] = channel2;
    }

    public AnimationTypeMask(
      AnimationTypeChannel channel0,
      AnimationTypeChannel channel1,
      AnimationTypeChannel channel2,
      AnimationTypeChannel channel3)
    {
      this.m_maskCode = (ushort) 0;
      this.m_channelCount = (byte) 4;
      this[0] = channel0;
      this[1] = channel1;
      this[2] = channel2;
      this[3] = channel3;
    }

    public AnimationTypeChannel this[int channelIndex]
    {
      get => this.GetChannel(channelIndex);
      set => this.SetChannel(channelIndex, value);
    }

    internal ushort MaskCode => this.m_maskCode;

    public uint ChannelCount => (uint) this.m_channelCount & 7U;

    public static AnimationTypeMask FromString(string maskSpec)
    {
      AnimationTypeMask animationTypeMask;
      if (string.IsNullOrEmpty(maskSpec))
      {
        animationTypeMask = AnimationTypeMask.Default;
      }
      else
      {
        Debug2.Validate(maskSpec.Length > 0 && maskSpec.Length <= 4, typeof (ArgumentException), "Invalid mask spec");
        ArrayList arrayList = new ArrayList();
        for (int index = 0; index < maskSpec.Length; ++index)
        {
          switch (maskSpec[index])
          {
            case '0':
              arrayList.Add((object) AnimationTypeChannel.O);
              break;
            case 'A':
            case 'a':
              arrayList.Add((object) AnimationTypeChannel.W);
              break;
            case 'B':
            case 'b':
              arrayList.Add((object) AnimationTypeChannel.Z);
              break;
            case 'G':
            case 'g':
              arrayList.Add((object) AnimationTypeChannel.Y);
              break;
            case 'R':
            case 'r':
              arrayList.Add((object) AnimationTypeChannel.X);
              break;
            case 'W':
            case 'w':
              arrayList.Add((object) AnimationTypeChannel.W);
              break;
            case 'X':
            case 'x':
              arrayList.Add((object) AnimationTypeChannel.X);
              break;
            case 'Y':
            case 'y':
              arrayList.Add((object) AnimationTypeChannel.Y);
              break;
            case 'Z':
            case 'z':
              arrayList.Add((object) AnimationTypeChannel.Z);
              break;
            default:
              Debug2.Validate(false, typeof (ArgumentException), (object) "Invalid mask spec: {0}", (object) maskSpec[index]);
              break;
          }
        }
        switch (arrayList.Count)
        {
          case 1:
            animationTypeMask = new AnimationTypeMask((AnimationTypeChannel) arrayList[0]);
            break;
          case 2:
            animationTypeMask = new AnimationTypeMask((AnimationTypeChannel) arrayList[0], (AnimationTypeChannel) arrayList[1]);
            break;
          case 3:
            animationTypeMask = new AnimationTypeMask((AnimationTypeChannel) arrayList[0], (AnimationTypeChannel) arrayList[1], (AnimationTypeChannel) arrayList[2]);
            break;
          case 4:
            animationTypeMask = new AnimationTypeMask((AnimationTypeChannel) arrayList[0], (AnimationTypeChannel) arrayList[1], (AnimationTypeChannel) arrayList[2], (AnimationTypeChannel) arrayList[3]);
            break;
          default:
            Debug2.Throw(false, "Too many channels in mask spec!");
            animationTypeMask = AnimationTypeMask.Default;
            break;
        }
      }
      return animationTypeMask;
    }

    public override bool Equals(object obj) => obj is AnimationTypeMask animationTypeMask && (int) animationTypeMask.m_channelCount == (int) this.m_channelCount && (int) animationTypeMask.m_maskCode == (int) this.m_maskCode;

    public override int GetHashCode() => this.m_channelCount.GetHashCode() ^ this.m_maskCode.GetHashCode();

    internal bool CanMapFromType(AnimationInputType animationType)
    {
      bool flag = true;
      AnimationTypeChannel animationTypeChannel;
      switch (animationType)
      {
        case AnimationInputType.Float:
          animationTypeChannel = AnimationTypeChannel.X;
          break;
        case AnimationInputType.Vector2:
          animationTypeChannel = AnimationTypeChannel.Y;
          break;
        case AnimationInputType.Vector3:
          animationTypeChannel = AnimationTypeChannel.Z;
          break;
        case AnimationInputType.Vector4:
        case AnimationInputType.Quaternion:
          animationTypeChannel = AnimationTypeChannel.W;
          break;
        default:
          animationTypeChannel = AnimationTypeChannel.X;
          flag = false;
          break;
      }
      if (this.ChannelCount != 0U)
      {
        for (int channelIndex = 0; (long) channelIndex < (long) this.ChannelCount; ++channelIndex)
        {
          if (this[channelIndex] > animationTypeChannel)
            flag = false;
        }
      }
      return flag;
    }

    internal bool CanMapToType(AnimationInputType animationType)
    {
      bool flag;
      switch (this.ChannelCount)
      {
        case 0:
          flag = true;
          break;
        case 1:
          flag = animationType == AnimationInputType.Float;
          break;
        case 2:
          flag = animationType == AnimationInputType.Vector2;
          break;
        case 3:
          flag = animationType == AnimationInputType.Vector3;
          break;
        case 4:
          flag = animationType == AnimationInputType.Vector4;
          break;
        default:
          flag = false;
          break;
      }
      return flag;
    }

    private AnimationTypeChannel GetChannel(int channelIndex)
    {
      Debug2.Validate(channelIndex >= 0 && channelIndex <= 3, typeof (ArgumentOutOfRangeException), "Channel index must be between 0 and 3");
      return (AnimationTypeChannel) ((int) this.m_maskCode >> channelIndex * 4 & 15);
    }

    private void SetChannel(int channelIndex, AnimationTypeChannel channel)
    {
      Debug2.Validate(channelIndex >= 0 && channelIndex <= 3, typeof (ArgumentOutOfRangeException), "Channel index must be between 0 and 3");
      Debug2.Validate(channel >= AnimationTypeChannel.O && channel <= AnimationTypeChannel.W, typeof (ArgumentOutOfRangeException), nameof (channel));
      int num1 = (int) channel << channelIndex * 4;
      int num2 = 15 << channelIndex * 4;
      this.m_maskCode = (ushort) (((int) this.m_maskCode & ~num2 | num1 & num2) & (int) ushort.MaxValue);
    }
  }
}
