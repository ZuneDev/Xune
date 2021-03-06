// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Animations.SizeYKeyframe
// Assembly: UIX, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: A56C6C9D-B7F6-46A9-8BDE-B3D9B8D60B11
// Assembly location: C:\Program Files\Zune\UIX.dll

using Microsoft.Iris.Render;
using Microsoft.Iris.UI;

namespace Microsoft.Iris.Animations
{
    internal class SizeYKeyframe : BaseFloatKeyframe
    {
        public override AnimationType Type => AnimationType.SizeY;

        public override void Apply(IAnimatableOwner animationTarget, float value) => ((ViewItem)animationTarget).VisualSize = new Vector2(((ViewItem)animationTarget).VisualSize.X, value);

        public override float GetEffectiveValue(
          IAnimatable targetObject,
          float baseValue,
          ref AnimationArgs args)
        {
            return RelativeTo == RelativeTo.Final ? baseValue + args.NewSize.Y : baseValue;
        }
    }
}
