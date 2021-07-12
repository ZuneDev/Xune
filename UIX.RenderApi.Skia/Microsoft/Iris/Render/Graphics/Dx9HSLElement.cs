// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.Graphics.Dx9HSLElement
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using Microsoft.Iris.Library;

namespace Microsoft.Iris.Render.Graphics
{
  internal class Dx9HSLElement
  {
    internal static void Generate(HSLElement efoHSL, ref Dx9EffectBuilder effectBuilder)
    {
      effectBuilder.EmitIncludesFragment("\"HSL.fx\"");
      VariableInfo variableInfo1 = new VariableInfo()
      {
        ID = (int) efoHSL.HueID,
        Type = Dx9VariableType.Float,
        IsDynamic = efoHSL.IsDynamicProperty("Hue")
      };
      variableInfo1.Name = variableInfo1.IsDynamic ? effectBuilder.GenerateGlobalVariable(variableInfo1.Type, efoHSL.Name + "Hue") : effectBuilder.GenerateGlobalConstant(variableInfo1.Type, efoHSL.Name + "Hue");
      variableInfo1.DefaultValue = (object) efoHSL.Hue;
      effectBuilder.AddPropertyVariable(variableInfo1);
      VariableInfo variableInfo2 = new VariableInfo()
      {
        ID = (int) efoHSL.SaturationID,
        Type = Dx9VariableType.Float,
        IsDynamic = efoHSL.IsDynamicProperty("Saturation")
      };
      variableInfo2.Name = variableInfo2.IsDynamic ? effectBuilder.GenerateGlobalVariable(variableInfo2.Type, efoHSL.Name + "Saturation") : effectBuilder.GenerateGlobalConstant(variableInfo2.Type, efoHSL.Name + "Saturation");
      variableInfo2.DefaultValue = (object) efoHSL.Saturation;
      effectBuilder.AddPropertyVariable(variableInfo2);
      VariableInfo variableInfo3 = new VariableInfo()
      {
        ID = (int) efoHSL.LightnessID,
        Type = Dx9VariableType.Float,
        IsDynamic = efoHSL.IsDynamicProperty("Lightness")
      };
      variableInfo3.Name = variableInfo3.IsDynamic ? effectBuilder.GenerateGlobalVariable(variableInfo3.Type, efoHSL.Name + "Lightness") : effectBuilder.GenerateGlobalConstant(variableInfo3.Type, efoHSL.Name + "Lightness");
      variableInfo3.DefaultValue = (object) efoHSL.Lightness;
      effectBuilder.AddPropertyVariable(variableInfo3);
      effectBuilder.EmitPixelFragment(InvariantString.Format("    {{\r\n        // convert to HSL color space\r\n        float3 fHSL = RGBToHSL({0});\r\n\r\n        // adjust Hue, Sat and Luma\r\n        fHSL *= float3({1}, {2}, {3});\r\n        fHSL = saturate(fHSL);\r\n\r\n        // convert back to RGB space\r\n        float3 fBackToRGB = HSLToRGB(fHSL);\r\n        {0}.rgb = fBackToRGB;\r\n    }}\r\n", (object) effectBuilder.PixelShaderOutput, (object) variableInfo1.Name, (object) variableInfo2.Name, (object) variableInfo3.Name));
    }
  }
}
