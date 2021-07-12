// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Render.EffectElement
// Assembly: UIX.RenderApi, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: D47658B8-A8EA-43D6-8837-ECE823BFFFC1
// Assembly location: C:\Program Files\Zune\UIX.RenderApi.dll

using Microsoft.Iris.Render.Common;
using Microsoft.Iris.Render.Graphics;
using System;
using System.Globalization;

namespace Microsoft.Iris.Render
{
    public abstract class EffectElement
    {
        protected const byte ClassKeySize = 3;
        protected const byte FloatKeySize = 8;
        protected const byte Vector2KeySize = 12;
        protected const byte Vector3KeySize = 16;
        protected const byte Vector4KeySize = 20;
        protected const byte IntKeySize = 8;
        protected const byte ByteKeySize = 5;
        private const byte k_classCacheKeySize = 3;
        private const byte k_propHeaderCacheKeySize = 4;
        private const byte k_floatCacheKeySize = 8;
        private const byte k_vec2CacheKeySize = 12;
        private const byte k_vec3CacheKeySize = 16;
        private const byte k_vec4CacheKeySize = 20;
        private const byte k_byteCacheKeySize = 5;
        private const byte k_intCacheKeySize = 8;
        protected string m_stName;
        protected Vector<string> m_dynamicProps;

        public string Name
        {
            get => this.m_stName;
            set => this.m_stName = value;
        }

        internal abstract void AddCacheKey(ByteBuilder cacheKey);

        internal void GenerateClassCacheKey(byte nClass, byte nOperation, ByteBuilder cacheKey)
        {
            cacheKey.AppendByte((byte)3);
            cacheKey.AppendByte(nClass);
            cacheKey.AppendByte(nOperation);
        }

        internal void GeneratePropertyCacheKey(
          string stProp,
          byte nPropertyId,
          byte nValue,
          ByteBuilder cacheKey)
        {
            if (this.IsDynamicProperty(stProp))
                return;
            cacheKey.AppendByte((byte)5);
            cacheKey.AppendByte((byte)3);
            cacheKey.AppendByte((byte)9);
            cacheKey.AppendByte(nPropertyId);
            cacheKey.AppendByte(nValue);
        }

        internal void GeneratePropertyCacheKey(
          string stPropName,
          byte nPropertyId,
          int nValue,
          ByteBuilder cacheKey)
        {
            if (this.IsDynamicProperty(stPropName))
                return;
            cacheKey.AppendByte((byte)8);
            cacheKey.AppendByte((byte)3);
            cacheKey.AppendByte((byte)0);
            cacheKey.AppendByte(nPropertyId);
            cacheKey.AppendInt(nValue);
        }

        internal void GeneratePropertyCacheKey(
          string stPropName,
          byte nPropertyId,
          float fValue,
          ByteBuilder cacheKey)
        {
            if (this.IsDynamicProperty(stPropName))
                return;
            cacheKey.AppendByte((byte)8);
            cacheKey.AppendByte((byte)3);
            cacheKey.AppendByte((byte)1);
            cacheKey.AppendByte(nPropertyId);
            cacheKey.AppendFloat(fValue);
        }

        internal void GeneratePropertyCacheKey(
          string stPropName,
          byte nPropertyId,
          Vector2 vValue,
          ByteBuilder cacheKey)
        {
            if (this.IsDynamicProperty(stPropName))
                return;
            cacheKey.AppendByte((byte)12);
            cacheKey.AppendByte((byte)3);
            cacheKey.AppendByte((byte)2);
            cacheKey.AppendByte(nPropertyId);
            cacheKey.AppendVector(vValue);
        }

        internal void GeneratePropertyCacheKey(
          string stPropName,
          byte nPropertyId,
          Vector3 vValue,
          ByteBuilder cacheKey)
        {
            if (this.IsDynamicProperty(stPropName))
                return;
            cacheKey.AppendByte((byte)16);
            cacheKey.AppendByte((byte)3);
            cacheKey.AppendByte((byte)3);
            cacheKey.AppendByte(nPropertyId);
            cacheKey.AppendVector(vValue);
        }

        internal void GeneratePropertyCacheKey(
          string stPropName,
          byte nPropertyId,
          Vector4 vValue,
          ByteBuilder cacheKey)
        {
            if (this.IsDynamicProperty(stPropName))
                return;
            cacheKey.AppendByte((byte)16);
            cacheKey.AppendByte((byte)3);
            cacheKey.AppendByte((byte)4);
            cacheKey.AppendByte(nPropertyId);
            cacheKey.AppendVector(vValue);
        }

        protected string GeneratePropertyPath(string stPropertyName) => string.Format((IFormatProvider)CultureInfo.InvariantCulture, "{0}.{1}", (object)this.m_stName, (object)stPropertyName);

        internal bool GenerateProperty(
          string stName,
          EffectPropertyType eptType,
          object oValue,
          byte nID,
          Map<string, EffectProperty> dictProperties)
        {
            string propertyPath = this.GeneratePropertyPath(stName);
            if (dictProperties.ContainsKey(propertyPath))
            {
                if (dictProperties[propertyPath] != null)
                    return false;
                EffectProperty effectProperty = new EffectProperty(stName, eptType, true, oValue, nID);
                dictProperties[propertyPath] = effectProperty;
                effectProperty.IsDynamic = true;
            }
            else
            {
                EffectProperty effectProperty = new EffectProperty(stName, eptType, false, oValue, nID);
                dictProperties.Add(propertyPath, effectProperty);
            }
            return true;
        }

        protected void MarkDynamicProperty(string stProp)
        {
            if (this.m_dynamicProps == null)
                this.m_dynamicProps = new Vector<string>(5);
            if (this.m_dynamicProps.Contains(stProp))
                return;
            this.m_dynamicProps.Add(stProp);
        }

        internal void AddEffectProperty(Map<string, EffectProperty> dictionary, string stProp)
        {
            string propertyPath = this.GeneratePropertyPath(stProp);
            if (!dictionary.ContainsKey(propertyPath))
                dictionary.Add(propertyPath, (EffectProperty)null);
            this.MarkDynamicProperty(stProp);
        }

        internal bool IsDynamicProperty(string stProp) => this.m_dynamicProps != null && this.m_dynamicProps.Contains(stProp);

        internal bool VerifyDynamicProperty(Map<string, EffectProperty> dictionary, string stProperty)
        {
            string propertyPath = this.GeneratePropertyPath(stProperty);
            if (!dictionary.ContainsKey(propertyPath))
                return false;
            this.MarkDynamicProperty(stProperty);
            return true;
        }

        internal int PreProcessProperty(
          Map<string, EffectProperty> dictionary,
          string stProperty,
          byte nPropSize,
          ref byte nPropertyId,
          ref byte nNextUniqueID)
        {
            int num = 0;
            if (this.VerifyDynamicProperty(dictionary, stProperty))
                nPropertyId = nNextUniqueID++;
            else
                num = (int)nPropSize;
            return num;
        }

        internal virtual bool Process(Map<string, EffectProperty> dictProperties) => true;

        internal virtual int PreProcessProperties(
          Map<string, EffectProperty> dictionary,
          ref byte nNextUniqueID)
        {
            return 3;
        }
    }
}
