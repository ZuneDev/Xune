﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Markup.UIXEventSchema
// Assembly: UIX, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: A56C6C9D-B7F6-46A9-8BDE-B3D9B8D60B11
// Assembly location: C:\Program Files\Zune\UIX.dll

namespace Microsoft.Iris.Markup
{
    internal class UIXEventSchema : EventSchema
    {
        private string _name;

        public UIXEventSchema(short ownerTypeID, string name)
          : base(UIXTypes.MapIDToType(ownerTypeID))
          => _name = name;

        public override string Name => _name;
    }
}
