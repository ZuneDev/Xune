// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Markup.MarkupDataQueryPropertySchema
// Assembly: UIX, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: A56C6C9D-B7F6-46A9-8BDE-B3D9B8D60B11
// Assembly location: C:\Program Files\Zune\UIX.dll

namespace Microsoft.Iris.Markup
{
    internal class MarkupDataQueryPropertySchema : ClassPropertySchema
    {
        private object _defaultValue;
        private bool _invalidatesQuery = true;
        private TypeSchema _underlyingCollectionType;

        public MarkupDataQueryPropertySchema(
          MarkupTypeSchema owner,
          string name,
          TypeSchema propertyType)
          : base(owner, name, propertyType)
        {
        }

        public object DefaultValue
        {
            get => _defaultValue;
            set => _defaultValue = value;
        }

        public bool InvalidatesQuery
        {
            get => _invalidatesQuery;
            set => _invalidatesQuery = value;
        }

        public override TypeSchema AlternateType => _underlyingCollectionType;

        public void SetUnderlyingCollectionType(TypeSchema underlyingCollectionType) => _underlyingCollectionType = underlyingCollectionType;
    }
}
