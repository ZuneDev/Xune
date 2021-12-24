// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Drawing.TextStyle
// Assembly: UIX, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: A56C6C9D-B7F6-46A9-8BDE-B3D9B8D60B11
// Assembly location: C:\Program Files\Zune\UIX.dll

using System;
using System.Collections.Specialized;
using System.Text;
using Topten.RichTextKit;

namespace Microsoft.Iris.Drawing
{
    internal class TextStyle
    {
        private Style _style;
        private BitVector32 _flags;
        private string _fontFace;
        private float _fontHeightPts;
        private float _altFontHeightPts;
        private float _lineSpacing;
        private float _characterSpacing;
        private Color _textColor;

        public TextStyle()
        {
            _flags = new();
            _style = new();
        }

        public bool IsInitialized() => _flags.Data != 0;

        public string FontFace
        {
            get => _style.FontFamily;
            set
            {
                SetFlag(SetFlags.FontFace, !string.IsNullOrEmpty(value));
                _style.FontFamily = value;
            }
        }

        public float FontSize
        {
            get => _style.FontSize;
            set
            {
                SetFlag(SetFlags.FontSize, true);
                _style.FontSize = value;
            }
        }

        [Obsolete]
        public float AltFontSize
        {
            get => _altFontHeightPts == 0.0 ? _fontHeightPts : _altFontHeightPts;
            set
            {
                SetFlag(SetFlags.AltFontHeight, true);
                _altFontHeightPts = value;
            }
        }

        public bool Bold
        {
            get => _style.FontWeight == 700;
            set
            {
                SetFlag(SetFlags.Bold, true);
                _style.FontWeight = 700;
            }
        }

        public bool Italic
        {
            get => _style.FontItalic;
            set
            {
                SetFlag(SetFlags.Italic, true);
                _style.FontItalic = value;
            }
        }

        public bool Underline
        {
            get => _style.Underline != UnderlineStyle.None;
            set
            {
                SetFlag(SetFlags.Underline, true);
                _style.Underline = value ? UnderlineStyle.Gapped : UnderlineStyle.None;
            }
        }

        public Color Color
        {
            get
            {
                if (_textColor == null)
                    Color.FromSKColor(_style.TextColor);
                return _textColor;
            }
            set => SetColor(value.ToSKColor());
        }

        public float LineSpacing
        {
            get => _style.LineHeight - 1;
            set
            {
                SetFlag(SetFlags.LineSpacing, true);
                _style.LineHeight = 1 + value;
            }
        }

        [Obsolete]
        public bool EnableKerning
        {
            get => GetFlag(SetFlags.EnableKerningValue);
            set
            {
                SetFlag(SetFlags.EnableKerning, true);
                SetFlag(SetFlags.EnableKerningValue, value);
            }
        }

        public float CharacterSpacing
        {
            get => _style.LetterSpacing;
            set
            {
                SetFlag(SetFlags.CharacterSpacing, true);
                _style.LetterSpacing = value;
            }
        }

        public bool Fragment { get; set; }

        public void Add(TextStyle additional)
        {
            Fragment = additional.Fragment;
            if (additional.GetFlag(SetFlags.FontFace))
                FontFace = additional.FontFace;
            if (additional.GetFlag(SetFlags.FontSize))
                FontSize = additional.FontSize;
            if (additional.GetFlag(SetFlags.Bold))
                Bold = additional.Bold;
            if (additional.GetFlag(SetFlags.Italic))
                Italic = additional.Italic;
            if (additional.GetFlag(SetFlags.Underline))
                Underline = additional.Underline;
            if (additional.GetFlag(SetFlags.LineSpacing))
                LineSpacing = additional.LineSpacing;
            if (additional.GetFlag(SetFlags.TextColor))
                Color = additional.Color;
            if (additional.GetFlag(SetFlags.EnableKerning))
                EnableKerning = additional.EnableKerning;
            if (additional.GetFlag(SetFlags.CharacterSpacing))
                CharacterSpacing = additional.CharacterSpacing;
            if (additional.GetFlag(SetFlags.AltFontHeight))
                AltFontSize = additional.AltFontSize;
        }

        public void SetColor(SkiaSharp.SKColor skColor)
        {
            SetFlag(SetFlags.TextColor, true);
            _style.TextColor = skColor;
        }

        public bool HasColor => GetFlag(SetFlags.TextColor);

        public override string ToString()
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append("{" + nameof(TextStyle));
            if (GetFlag(SetFlags.FontFace))
            {
                stringBuilder.Append(" Font = \"");
                stringBuilder.Append(FontFace);
                stringBuilder.Append("\"");
            }
            if (GetFlag(SetFlags.FontSize))
            {
                stringBuilder.Append(" Pt = ");
                stringBuilder.Append(FontSize);
            }
            if (GetFlag(SetFlags.Bold))
            {
                stringBuilder.Append(" Bold = ");
                stringBuilder.Append(Bold);
            }
            if (GetFlag(SetFlags.Italic))
            {
                stringBuilder.Append(" Italic = ");
                stringBuilder.Append(Italic);
            }
            if (GetFlag(SetFlags.Underline))
            {
                stringBuilder.Append(" Underline = ");
                stringBuilder.Append(Underline);
            }
            if (GetFlag(SetFlags.LineSpacing))
            {
                stringBuilder.Append(" LineSpacing = ");
                stringBuilder.Append(LineSpacing);
            }
            if (GetFlag(SetFlags.TextColor))
            {
                stringBuilder.Append(" Color = ");
                stringBuilder.Append(Color);
            }
            stringBuilder.Append(" }");
            return stringBuilder.ToString();
        }

        public bool SetFlag(SetFlags flag, bool value) => _flags[(int)flag] = value;
        public bool GetFlag(SetFlags flag) => _flags[(int)flag];

        [Flags]
        internal enum SetFlags
        {
            None = 0,
            FontFace = 1,
            FontSize = 2,
            Bold = 4,
            Italic = 8,
            Underline = 16, // 0x00000010
            LineSpacing = 32, // 0x00000020
            TextColor = 64, // 0x00000040
            EnableKerning = 128, // 0x00000080
            CharacterSpacing = 256, // 0x00000100
            AltFontHeight = 512, // 0x00000200
            BoldValue = 65536, // 0x00010000
            ItalicValue = 131072, // 0x00020000
            UnderlineValue = 262144, // 0x00040000
            EnableKerningValue = 524288, // 0x00080000
        }
    }
}
