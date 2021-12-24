// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.ViewItems.Text
// Assembly: UIX, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: A56C6C9D-B7F6-46A9-8BDE-B3D9B8D60B11
// Assembly location: C:\Program Files\Zune\UIX.dll

using Microsoft.Iris.Drawing;
using Microsoft.Iris.InputHandlers;
using Microsoft.Iris.Layout;
using Microsoft.Iris.Layouts;
using Microsoft.Iris.Markup;
using Microsoft.Iris.OS;
using Microsoft.Iris.Render;
using Microsoft.Iris.Render.Extensions;
using Microsoft.Iris.RenderAPI.Drawing;
using Microsoft.Iris.Session;
using Microsoft.Iris.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Topten.RichTextKit;

namespace Microsoft.Iris.ViewItems
{
    internal class Text : ViewItem, ILayout
    {
        private uint _bits;
        public const float MaxWidthConstraint = 4095f;
        public const float MaxHeightConstraint = 8191f;
        private const float c_scaleEpsilon = 0.01f;
        private const float c_lineSpacingDefault = 0.0f;
        private const float c_characterSpacingDefault = 0.0f;
        private Size _slotSize;
        private Size _textSize;
        private string _content;
        private RichString _string;
        private Font _font;
        private char _passwordChar;
        private float _scale;
        private float _fadeSize;
        private int _lastVisibleRun;
        private Color _backHighlightColor;
        private Color _textHighlightColor;
        private TextBounds _boundsType;
        private LineAlignment _lineAlignment;
        private bool _disableIme;
        private bool _enableKerning;
        private TextStyle _textStyle;
        private IDictionary _namedStyles;
        private RichText _richTextRasterizer;
        private TextEditingHandler _externalEditingHandler;
        private string _parsedContent;
        private List<MarkedRange> _parsedContentMarkedRanges;
        private TextRange _selection;
        private static Font s_defaultFont = new Font("Arial", 24f);
        private static char[] s_whitespaceChars = new char[3]
        {
            ' ',
            '\r',
            '\n'
        };

        public Text()
        {
            Layout = this;
            _richTextRasterizer = new RichText(true);
            _textStyle = new TextStyle();
            _font = s_defaultFont;
            Color = Color.Black;
            _textHighlightColor = Color.White;
            _backHighlightColor = Color.Black;
            _scale = 1f;
            _fadeSize = 32f;
            _passwordChar = '•';
            _lineAlignment = LineAlignment.Near;
            ContributesToWidth = true;
            TextFitsWidth = true;
            TextFitsHeight = true;
            SetClipped(false);
            MarkScaleDirty();
            ClearBit(Bits.FastMeasurePossible);
            SetBit(Bits.FastMeasureValid);
        }

        public Text(UIClass ownerUI): this()
        {
            
        }

        protected override void OnDispose()
        {
            UnregisterFragmentUsage();
            base.OnDispose();
        }

        public static void Initialize()
        {
        }

        public static void Uninitialize()
        {

        }

        public string Content
        {
            get => _content;
            set
            {
                if (!(_content != value))
                    return;
                if (value != null && value.Length > 3)
                {
                    _content = value.TrimEnd(s_whitespaceChars);
                    char ch1 = value[value.Length - 1];
                    char ch2 = value[value.Length - 2];
                    if (ch1 == ' ' && ch2 != ' ')
                        _content += ch1;
                }
                else
                    _content = value;
                _parsedContent = null;
                OnDisplayedContentChange();
                FireNotification(NotificationID.Content);
            }
        }

        public void MarkScaleDirty() => SetBit(Bits.ScaleDirty);

        private bool InMeasure
        {
            get => GetBit(Bits.InMeasure);
            set => SetBit(Bits.InMeasure, value);
        }

        public void OnDisplayedContentChange()
        {
            if (InMeasure)
                return;
            MarkTextLayoutInvalid();
            MarkPaintInvalid();
            ForceContentChange();
        }

        public Font Font
        {
            get => _font;
            set
            {
                if (_font == value)
                    return;
                _font = value;
                MarkPaintInvalid();
                MarkTextLayoutInvalid();
                ForceContentChange();
                FireNotification(NotificationID.Font);
            }
        }

        public Color Color
        {
            get => _textStyle.Color;
            set
            {
                if (_textStyle.Color != value)
                    _textStyle.Color = value;
                if (KeepFlowAlive)
                {
                    MarkPaintInvalid();
                }
                else
                {
                    MarkLayoutInvalid();
                    if (HasEverPainted)
                        KeepFlowAlive = true;
                }
                FireNotification(NotificationID.Color);
            }
        }

        public Color HighlightColor
        {
            get => _backHighlightColor;
            set
            {
                if (!(_backHighlightColor != value))
                    return;
                _backHighlightColor = value;
                MarkPaintInvalid();
                FireNotification(NotificationID.HighlightColor);
            }
        }

        public Color TextHighlightColor
        {
            get => _textHighlightColor;
            set
            {
                if (!(TextHighlightColor != value))
                    return;
                _textHighlightColor = value;
                MarkPaintInvalid();
                FireNotification(NotificationID.TextHighlightColor);
            }
        }

        public TextSharpness TextSharpness
        {
            get => !_richTextRasterizer.Oversample ? TextSharpness.Sharp : TextSharpness.Soft;
            set
            {
                if (TextSharpness == value)
                    return;
                bool flag = false;
                switch (value)
                {
                    case TextSharpness.Sharp:
                        flag = false;
                        break;
                    case TextSharpness.Soft:
                        flag = true;
                        break;
                }
                _richTextRasterizer.Oversample = flag;
                MarkTextLayoutInvalid();
                ForceContentChange();
                FireNotification(NotificationID.TextSharpness);
            }
        }

        public bool WordWrap
        {
            get => GetBit(Bits.WordWrap);
            set
            {
                if (WordWrap == value)
                    return;
                SetBit(Bits.WordWrap, value);
                if (value)
                    KeepFlowAlive = true;
                MarkPaintInvalid();
                MarkTextLayoutInvalid();
                FireNotification(NotificationID.WordWrap);
            }
        }

        public bool UsePasswordMask
        {
            get => GetBit(Bits.PasswordMasked);
            set
            {
                if (UsePasswordMask == value)
                    return;
                SetBit(Bits.PasswordMasked, value);
                MarkPaintInvalid();
                MarkTextLayoutInvalid();
                FireNotification(NotificationID.UsePasswordMask);
            }
        }

        public char PasswordMask
        {
            get => _passwordChar;
            set
            {
                if (_passwordChar == value)
                    return;
                _passwordChar = value;
                if (UsePasswordMask)
                {
                    MarkPaintInvalid();
                    MarkTextLayoutInvalid();
                }
                FireNotification(NotificationID.PasswordMask);
            }
        }

        public int MaximumLines
        {
            get => (int)_string.MaxLines;
            set
            {
                if (_string.MaxLines == value)
                    return;
                _string.MaxLines = value;
                MarkPaintInvalid();
                MarkTextLayoutInvalid();
                FireNotification(NotificationID.MaximumLines);
            }
        }

        public LineAlignment LineAlignment
        {
            get => _lineAlignment;
            set
            {
                if (_lineAlignment == value)
                    return;
                _lineAlignment = value;
                MarkPaintInvalid();
                MarkTextLayoutInvalid();
                FireNotification(NotificationID.LineAlignment);
            }
        }

        public float LineSpacing
        {
            get => !GetBit(Bits.LineSpacingSet) ? 0.0f : _textStyle.LineSpacing;
            set
            {
                if (LineSpacing == (double)value)
                    return;
                _textStyle.LineSpacing = value;
                SetBit(Bits.LineSpacingSet);
                MarkPaintInvalid();
                MarkTextLayoutInvalid();
                FireNotification(NotificationID.LineSpacing);
            }
        }

        public float CharacterSpacing
        {
            get => !GetBit(Bits.CharacterSpacingSet) ? 0.0f : _textStyle.CharacterSpacing;
            set
            {
                if (CharacterSpacing == (double)value)
                    return;
                _textStyle.CharacterSpacing = value;
                SetBit(Bits.CharacterSpacingSet);
                MarkPaintInvalid();
                MarkTextLayoutInvalid();
                FireNotification(NotificationID.CharacterSpacing);
            }
        }

        public bool EnableKerning
        {
            get => GetBit(Bits.EnableKerningSet) && _enableKerning;
            set
            {
                if (EnableKerning == value)
                    return;
                _enableKerning = value;
                SetBit(Bits.EnableKerningSet);
                MarkPaintInvalid();
                MarkTextLayoutInvalid();
                FireNotification(NotificationID.EnableKerning);
            }
        }

        public float FadeSize
        {
            get => _fadeSize;
            set
            {
                if (_fadeSize == (double)value)
                    return;
                _fadeSize = value;
                InvalidateGradients();
                FireNotification(NotificationID.FadeSize);
            }
        }

        public TextStyle Style
        {
            get => _textStyle;
            set
            {
                if (_textStyle == value)
                    return;
                _textStyle = value;
                MarkPaintInvalid();
                MarkTextLayoutInvalid();
                ForceContentChange();
                FireNotification(NotificationID.Style);
            }
        }

        public IDictionary NamedStyles
        {
            get => _namedStyles;
            set
            {
                if (_namedStyles == value)
                    return;
                _namedStyles = value;
                MarkPaintInvalid();
                MarkTextLayoutInvalid();
                ForceContentChange();
                FireNotification(NotificationID.NamedStyles);
            }
        }

        public IList<TextFragment> Fragments { get; private set; }

        public bool DisableIme
        {
            get => _disableIme;
            set
            {
                if (_disableIme == value)
                    return;
                _disableIme = value;
                FireNotification(NotificationID.DisableIme);
            }
        }

        public Rectangle LastLineBounds => new(_textSize);

        public int NumberOfVisibleLines => 1;

        public bool ContributesToWidth
        {
            get => GetBit(Bits.ContributesToWidth);
            set
            {
                if (ContributesToWidth == value)
                    return;
                SetBit(Bits.ContributesToWidth, value);
                FireNotification(NotificationID.ContributesToWidth);
                MarkLayoutInvalid();
            }
        }

        public TextBounds BoundsType
        {
            get => _boundsType;
            set
            {
                if (_boundsType == value)
                    return;
                _boundsType = value;
                MarkLayoutInvalid();
                FireNotification(NotificationID.BoundsType);
            }
        }

        public bool Clipped => GetBit(Bits.Clipped);

        private void SetClipped(bool value)
        {
            if (GetBit(Bits.Clipped) == value)
                return;
            SetBit(Bits.Clipped, value);
            FireNotification(NotificationID.Clipped);
        }

        public RichText ExternalRasterizer
        {
            set
            {
                if (_richTextRasterizer == value)
                    return;
                bool oversample = _richTextRasterizer.Oversample;
                _richTextRasterizer = value;
                if (value != null)
                {
                    _richTextRasterizer.Oversample = oversample;
                    MarkScaleDirty();
                }
                MarkTextLayoutInvalid();
            }
        }

        public TextEditingHandler ExternalEditingHandler
        {
            set
            {
                if (_externalEditingHandler == value)
                    return;
                _externalEditingHandler = value;
            }
        }

        public ItemAlignment DefaultChildAlignment => ItemAlignment.Default;

        public bool IsViewDependent(ViewItem node) => GetBit(Bits.ViewDependent);

        public void GetInitialChildrenRequests(out int more) => more = 0;

        Size ILayout.Measure(ILayoutNode layoutNode, Size constraint)
        {
            // TODO: Text measure
            Size zero = Size.Zero;
            InMeasure = true;
            LineAlignment alignment = RichText.ReverseAlignment(_lineAlignment, UISession.IsRtl);

            string content;
            if (!UsedForEditing)
            {
                content = Content;
                if (_namedStyles != null)
                {
                    if (_parsedContent == null && content != null)
                    {
                        _parsedContentMarkedRanges = new();
                        _parsedContent = ParseMarkedUpText(content, _parsedContentMarkedRanges);
                    }
                    content = _parsedContent;
                }
            }
            else
                content = _richTextRasterizer.SimpleContent;

            _string = new(content);
            _textSize = new((int)_string.MeasuredWidth, (int)_string.MeasuredHeight);

            Size finalSize = Size.Min(_textSize, constraint);
            DefaultLayout.Measure(layoutNode, finalSize);
            FireNotification(NotificationID.LastLineBounds);
            InMeasure = false;
            return finalSize;
        }

        void ILayout.Arrange(ILayoutNode layoutNode, LayoutSlot slot)
        {
            // TODO: Text arrange
            _slotSize = slot.Bounds;
            if (!ContributesToWidth)
                TextFitsWidth = _textSize.Width <= slot.Bounds.Width;
            bool flag1 = false;
            KeepFlowAlive |= flag1;
            SetBit(Bits.ViewDependent, flag1);
            DefaultLayout.Arrange(layoutNode, slot);
        }

        internal TextStyle GetEffectiveTextStyle()
        {
            TextStyle textStyle = new TextStyle();
            Font font = _font ?? s_defaultFont;
            textStyle.FontFace = font.FontName;
            textStyle.FontSize = font.FontSize;
            if (font.AltFontSize != (double)font.FontSize)
                textStyle.AltFontSize = font.AltFontSize;
            if ((font.FontStyle & FontStyles.Bold) != FontStyles.None)
                textStyle.Bold = true;
            if ((font.FontStyle & FontStyles.Italic) != FontStyles.None)
                textStyle.Italic = true;
            if ((font.FontStyle & FontStyles.Underline) != FontStyles.None)
                textStyle.Underline = true;
            textStyle.Color = _textStyle.Color;
            if (GetBit(Bits.LineSpacingSet))
                textStyle.LineSpacing = LineSpacing;
            if (GetBit(Bits.CharacterSpacingSet))
                textStyle.CharacterSpacing = CharacterSpacing;
            if (GetBit(Bits.EnableKerningSet))
                textStyle.EnableKerning = EnableKerning;
            if (_textStyle != null)
                textStyle.Add(_textStyle);
            return textStyle;
        }

        protected override void OnLayoutComplete(ViewItem sender)
        {
            List<TextFragment> arrayList = null;
            if (UpdateFragmentsAfterLayout)
            {
                if (_namedStyles != null)
                    arrayList = AnnotateFragments();
                bool flag = false;
                if (Fragments != null || arrayList != null)
                    flag = TextLayoutInvalid || !AreFragmentListsEquivalent(Fragments, arrayList);
                if (flag)
                {
                    UnregisterFragmentUsage();
                    Fragments = arrayList;
                    RegisterFragmentUsage();
                    FireNotification(NotificationID.Fragments);
                }
                else if (Fragments != null)
                {
                    for (int index = 0; index < Fragments.Count; ++index)
                        Fragments[index].NotifyPaintInvalid();
                }
                ResetMarkedRanges();
                UpdateFragmentsAfterLayout = false;
            }
            SetClipped(!TextFitsWidth || !TextFitsHeight);
            TextLayoutInvalid = false;
            base.OnLayoutComplete(sender);
        }

        private static bool AreFragmentListsEquivalent(IList<TextFragment> lhsFragments, IList<TextFragment> rhsFragments)
        {
            int num1 = lhsFragments != null ? lhsFragments.Count : 0;
            int num2 = rhsFragments != null ? rhsFragments.Count : 0;
            if (num1 != num2)
                return false;
            for (int index = 0; index < num1; ++index)
            {
                if (!lhsFragments[index].IsLayoutEquivalentTo(rhsFragments[index]))
                    return false;
            }
            return true;
        }

        private List<TextFragment> AnnotateFragments()
        {
            List<TextFragment> frags = null;
            /*if (_parsedContentMarkedRanges != null)
            {
                for (int firstVisibleIndex = _flow.FirstVisibleIndex; firstVisibleIndex <= _flow.LastVisibleIndex; ++firstVisibleIndex)
                {
                    TextRun textRun = _flow[firstVisibleIndex];
                    textRun.IsFragment = false;
                    if (textRun.RunColor.A != byte.MaxValue)
                    {
                        MarkedRange markedRange = null;
                        for (int index = 0; index < _parsedContentMarkedRanges.Count; ++index)
                        {
                            MarkedRange contentMarkedRange = (MarkedRange)_parsedContentMarkedRanges[index];
                            if (contentMarkedRange.RangeIDAsColor == textRun.RunColor)
                            {
                                textRun.OverrideColor = contentMarkedRange.GetEffectiveColor(Color.Transparent);
                                markedRange = contentMarkedRange;
                                break;
                            }
                        }
                        if (markedRange != null && markedRange.IsInFragment)
                        {
                            textRun.IsFragment = true;
                            if (markedRange.fragment == null)
                            {
                                markedRange.fragment = new TextFragment(markedRange.tagName, markedRange.attributes, this);
                                if (frags == null)
                                    frags = new();
                                frags.Add(markedRange.fragment);
                            }
                            markedRange.fragment.InternalRuns.Add(new TextRunData(textRun, IsOnLastLine(textRun), this, 0));
                        }
                    }
                }
            }*/
            return frags;
        }

        private void RegisterFragmentUsage()
        {
            if (Fragments == null)
                return;
            foreach (TextFragment fragment in Fragments)
            {
                if (fragment.Runs != null)
                {
                    foreach (TextRunData run in fragment.Runs)
                        run.Run.RegisterUsage(this);
                }
            }
        }

        private void UnregisterFragmentUsage()
        {
            if (Fragments == null)
                return;
            foreach (TextFragment fragment in Fragments)
            {
                if (fragment.Runs != null)
                {
                    foreach (TextRunData run in fragment.Runs)
                        run.Run.UnregisterUsage(this);
                }
            }
            Fragments = null;
        }

        private void ResetMarkedRanges()
        {
            if (_parsedContentMarkedRanges == null)
                return;
            for (int index = 0; index < _parsedContentMarkedRanges.Count; ++index)
                _parsedContentMarkedRanges[index].fragment = null;
        }

        private string ParseMarkedUpText(string content, IList<MarkedRange> markedRanges)
        {
            StringBuilder stringBuilder = new();
            List<MarkedRange> ranges = new();
            uint num = 0;
            try
            {
                using (ManagedXmlReader xmlReader = new("<root>" + content + "</root>", true))
                {
                    // Read out root element
                    xmlReader.Read(out var rootType);
                    //Debug.Assert.IsTrue(rootType == XmlNodeType.Document);
                    Debug.Trace.WriteLine(Debug.TraceCategory.Text, xmlReader.Value);

                    MarkedRange markedRange1 = null;
                    while (xmlReader.Read(out XmlNodeType nodeType))
                    {
                        switch (nodeType)
                        {
                            case XmlNodeType.Element:
                                if (!xmlReader.IsEmptyElement)
                                {
                                    string name = xmlReader.Name;
                                    MarkedRange markedRange2 = new MarkedRange();
                                    markedRange2.tagName = name;
                                    markedRange2.firstCharacter = stringBuilder.Length;
                                    markedRange2.lastCharacter = int.MaxValue;
                                    markedRange2.rangeID = ++num;
                                    ranges.Add(markedRange2);
                                    markedRanges.Add(markedRange2);
                                    markedRange2.parentRange = markedRange1;
                                    markedRange1 = markedRange2;
                                    while (xmlReader.ReadAttribute())
                                    {
                                        if (markedRange1.attributes == null)
                                            markedRange1.attributes = new Dictionary<object, object>();
                                        markedRange1.attributes[xmlReader.Name] = xmlReader.Value;
                                    }
                                    continue;
                                }
                                continue;
                            case XmlNodeType.Text:
                            case XmlNodeType.CDATA:
                            case XmlNodeType.Whitespace:
                                string str = xmlReader.Value;
                                if (str.IndexOf("\r\n", StringComparison.Ordinal) >= 0)
                                    str = str.Replace("\r\n", "\r");
                                stringBuilder.Append(str);
                                continue;
                            case XmlNodeType.EndElement:
                                string name1 = xmlReader.Name;
                                for (int index = ranges.Count - 1; index >= 0; --index)
                                {
                                    MarkedRange markedRange2 = ranges[index];
                                    markedRange2.lastCharacter = stringBuilder.Length;
                                    ranges.RemoveAt(index);
                                    if (markedRange2.tagName == name1)
                                        break;
                                }
                                if (markedRange1 != null)
                                {
                                    markedRange1 = markedRange1.parentRange;
                                    continue;
                                }
                                continue;
                            default:
                                continue;
                        }
                    }
                }
            }
            catch (XmlException ex)
            {
                Debug.Trace.WriteLine(Debug.TraceCategory.Text, ex);
                markedRanges.Clear();
                stringBuilder = null;
            }
            if (stringBuilder == null)
                return content;
            return stringBuilder.ToString();
        }

        public void CreateFadeGradientsHelper(ref IGradient gradientClipLeftRight, ref IGradient gradientMultiLine)
        {
            bool flag1 = true;
            if (TextFitsWidth && TextFitsHeight)
                flag1 = false;
            float fadeSize = FadeSize;
            if (!flag1 || fadeSize <= 0.0)
                return;
            if (!WordWrap)
            {
                if (TextFitsWidth)
                    return;
                float num = 0.0f;
                float flPosition = 0.0f;
                LineAlignment lineAlignment = LineAlignment;
                if (lineAlignment == LineAlignment.Center)
                {
                    flPosition = num = fadeSize;
                }
                else
                {
                    bool flag2 = lineAlignment == LineAlignment.Near;
                    if (UISession.IsRtl)
                        flag2 = !flag2;
                    if (flag2)
                        num = fadeSize;
                    else
                        flPosition = fadeSize;
                }
                IGradient gradient = UISession.RenderSession.CreateGradient(this);
                gradient.Orientation = Orientation.Horizontal;
                if (flPosition > 0.0)
                {
                    gradient.AddValue(-1f, 0.0f, RelativeSpace.Min);
                    gradient.AddValue(flPosition, 1f, RelativeSpace.Min);
                }
                if (num > 0.0)
                {
                    gradient.AddValue(_slotSize.Width - num, 1f, RelativeSpace.Min);
                    gradient.AddValue(_slotSize.Width + 1, 0.0f, RelativeSpace.Min);
                }
                gradientClipLeftRight = gradient;
            }
            else
            {
                if (TextFitsHeight)
                    return;
                float flPosition1 = 0.0f;
                float flPosition2 = 0.0f;
                if (!UISession.IsRtl)
                {
                    
                }
                else
                {
                    flPosition2 = 0.0f;
                    flPosition1 = flPosition2 + fadeSize;
                }
                IGradient gradient = UISession.RenderSession.CreateGradient(this);
                gradient.ColorMask = new ColorF(byte.MaxValue, 0, 0, 0);
                gradient.Orientation = Orientation.Horizontal;
                gradient.AddValue(flPosition1, 1f, RelativeSpace.Min);
                gradient.AddValue(flPosition2, 0.0f, RelativeSpace.Min);
                gradientMultiLine = gradient;
            }
        }

        private bool UsingSharedRasterizer => !_richTextRasterizer.HasCallbacks;

        private bool UsedForEditing => _externalEditingHandler != null;

        private void ResetCachedScaleState()
        {
            IgnoreEffectiveScaleChanges = false;
        }

        private void CreateVisuals(IVisualContainer topVisual, IRenderSession renderSession)
        {
            VisualOrder nOrder = VisualOrder.First;
            IGradient gradientClipLeftRight = null;
            IGradient gradientMultiLine = null;
            CreateFadeGradientsHelper(ref gradientClipLeftRight, ref gradientMultiLine);
            /*TextRun textRun = UISession.IsRtl ? _flow.FirstFitRunOnFinalLine : _flow.LastFitRun;
            for (int firstVisibleIndex = _flow.FirstVisibleIndex; firstVisibleIndex <= _flow.LastVisibleIndex; ++firstVisibleIndex)
            {
                TextRun run = _flow[firstVisibleIndex];
                if (run.Visible && !run.IsFragment)
                {
                    Color effectiveColor = GetEffectiveColor(run);
                    IImage imageForRun = GetImageForRun(UISession, run, effectiveColor);
                    if (imageForRun != null)
                    {
                        float x = run.RenderBounds.Left + _lineAlignmentOffset;
                        if (run.Highlighted)
                        {
                            RectangleF lineBound = (RectangleF)_flow.LineBounds[run.Line - 1];
                            ISprite sprite = renderSession.CreateSprite(this, this);
                            sprite.Effect = EffectManager.CreateColorFillEffect(this, _backHighlightColor);
                            sprite.Effect.UnregisterUsage(this);
                            sprite.Position = new Vector3(x, lineBound.Top, 0.0f);
                            sprite.Size = new Vector2(run.RenderBounds.Width, lineBound.Height);
                            topVisual.AddChild(sprite, null, nOrder);
                            sprite.UnregisterUsage(this);
                            run.HighlightSprite = sprite;
                        }
                        ISprite sprite1 = renderSession.CreateSprite(this, this);
                        sprite1.Effect = EffectClass.CreateImageRenderEffectWithFallback(Effect, this, imageForRun);
                        sprite1.Effect.UnregisterUsage(this);
                        sprite1.Position = new Vector3(x, run.RenderBounds.Top, 0.0f);
                        sprite1.Size = new Vector2(run.RenderBounds.Width, run.RenderBounds.Height);
                        if (gradientMultiLine != null && run == textRun)
                            sprite1.AddGradient(gradientMultiLine);
                        topVisual.AddChild(sprite1, null, nOrder);
                        sprite1.UnregisterUsage(this);
                        run.TextSprite = sprite1;
                    }
                }
            }*/
            topVisual.RemoveAllGradients();
            if (gradientClipLeftRight != null)
                topVisual.AddGradient(gradientClipLeftRight);
            gradientClipLeftRight?.UnregisterUsage(this);
            gradientMultiLine?.UnregisterUsage(this);
        }

        private Color GetEffectiveColor(TextRun run)
        {
            Color color = _textStyle.Color;
            if (run.Highlighted)
                color = _textHighlightColor;
            else if (run.OverrideColor != Color.Transparent)
                color = run.OverrideColor;
            else if (run.Link && _externalEditingHandler != null && _externalEditingHandler.LinkColor != Color.Transparent)
                color = _externalEditingHandler.LinkColor;
            else if (_textStyle != null && _textStyle.HasColor)
                color = _textStyle.Color;
            return color;
        }

        protected override void DisposeAllContent()
        {
            base.DisposeAllContent();
            DisposeContent(true);
        }

        protected virtual void DisposeContent(bool removeFromTree)
        {
            if (removeFromTree)
                VisualContainer.RemoveAllChildren();
            Effect?.DoneWithRenderEffects(this);
        }

        protected override void OnPaint(bool visible)
        {
            DisposeAllContent();
            base.OnPaint(visible);
            ResetCachedScaleState();
            if (true)//_flow == null)
            {
                if (_content == null && UsingSharedRasterizer)
                    return;
                MarkTextLayoutInvalid();
            }
            else
            {
                CreateVisuals(VisualContainer, UISession.RenderSession);
                HasEverPainted = true;
            }
        }

        private bool IsOnLastLine(TextRun run) => true;// _flow.IsOnLastLine(run);

        private void InvalidateGradients()
        {
            //_renderingHelper.InvalidateGradients();
            MarkPaintInvalid();
        }

        protected override void OnEffectiveScaleChange()
        {
            float y = ComputeEffectiveScale().Y;
            if (!ScaleDifferenceIsGreaterThanThreshold(y, _scale))
                return;
            Vector<float> vector = new();
            if (!IgnoreEffectiveScaleChanges && vector != null)
            {
                foreach (float newScale in vector)
                {
                    if (!ScaleDifferenceIsGreaterThanThreshold(y, newScale))
                    {
                        IgnoreEffectiveScaleChanges = true;
                        break;
                    }
                }
            }
            if (IgnoreEffectiveScaleChanges)
                return;
            if (vector == null)
                vector = new Vector<float>(4);
            vector.Add(y);
            MarkTextLayoutInvalid();
            _scale = y;
            MarkScaleDirty();
        }

        private bool ScaleDifferenceIsGreaterThanThreshold(float oldScale, float newScale) => Math.Abs(oldScale - newScale) > 0.00999999977648258;

        private void MarkTextLayoutInvalid()
        {
            TextLayoutInvalid = true;
            ClearBit(Bits.FastMeasureValid);
            
            MarkLayoutInvalid();
        }

        internal static IImage GetImageForRun(UISession session, TextRun run, Color textColor)
        {
            if (string.IsNullOrEmpty(run.Content))
                return null;
            string str = "aa";
            bool flag = false;
            RichTextInfoKey richTextInfoKey = new RichTextInfoKey(run, str, flag, textColor);
            ImageCache instance = TextImageCache.Instance;
            ImageCacheItem imageCacheItem = instance.Lookup(richTextInfoKey);
            if (imageCacheItem == null)
            {
                imageCacheItem = new TextImageItem(session.RenderSession, run, str, flag, textColor);
                instance.Add(richTextInfoKey, imageCacheItem);
            }
            return imageCacheItem.RenderImage;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.ToString());
            if (_content != null)
            {
                stringBuilder.Append(" (Content=\"");
                stringBuilder.Append(_content);
                stringBuilder.Append("\")");
            }
            return stringBuilder.ToString();
        }

        private bool TextFitsWidth
        {
            get => GetBit(Bits.TextFitsWidth);
            set => SetBit(Bits.TextFitsWidth, value);
        }

        private bool TextFitsHeight
        {
            get => GetBit(Bits.TextFitsHeight);
            set => SetBit(Bits.TextFitsHeight, value);
        }

        private bool ClipToHeight
        {
            get => GetBit(Bits.ClipToHeight);
            set => SetBit(Bits.ClipToHeight, value);
        }

        private bool KeepFlowAlive
        {
            get => GetBit(Bits.KeepFlowAlive);
            set => SetBit(Bits.KeepFlowAlive, value);
        }

        private bool HasEverPainted
        {
            get => GetBit(Bits.HasEverPainted);
            set => SetBit(Bits.HasEverPainted, value);
        }

        private bool TextLayoutInvalid
        {
            get => GetBit(Bits.TextLayoutInvalid);
            set => SetBit(Bits.TextLayoutInvalid, value);
        }

        private bool UpdateFragmentsAfterLayout
        {
            get => GetBit(Bits.UpdateFragmentsAfterLayout);
            set => SetBit(Bits.UpdateFragmentsAfterLayout, value);
        }

        private bool IgnoreEffectiveScaleChanges
        {
            get => GetBit(Bits.IgnoreEffectiveScaleChanges);
            set => SetBit(Bits.IgnoreEffectiveScaleChanges, value);
        }

        private bool GetBit(Bits lookupBit) => ((Bits)_bits & lookupBit) != 0;

        private void SetBit(Bits changeBit, bool value) => _bits = value ? (uint)((Bits)_bits | changeBit) : (uint)((Bits)_bits & ~changeBit);

        private void SetBit(Bits changeBit)
        {
            Text text = this;
            text._bits = (uint)((Bits)text._bits | changeBit);
        }

        private void ClearBit(Bits changeBit)
        {
            Text text = this;
            text._bits = (uint)((Bits)text._bits & ~changeBit);
        }

        private class MarkedRange
        {
            public string tagName;
            public int firstCharacter;
            public int lastCharacter;
            public uint rangeID;
            public Dictionary<object, object> attributes;
            public MarkedRange parentRange;
            public TextStyle cachedStyle;
            public TextFragment fragment;
            private static uint s_rangeIDIndicator = 1073741824;

            public Color RangeIDAsColor => new Color(s_rangeIDIndicator | rangeID);

            public Color GetEffectiveColor(Color defaultColor)
            {
                if (cachedStyle != null && cachedStyle.HasColor)
                    return cachedStyle.Color;
                return parentRange != null ? parentRange.GetEffectiveColor(defaultColor) : defaultColor;
            }

            public bool IsInFragment
            {
                get
                {
                    bool flag = false;
                    for (MarkedRange markedRange = this; markedRange != null; markedRange = markedRange.parentRange)
                    {
                        TextStyle cachedStyle = markedRange.cachedStyle;
                        if (cachedStyle != null && cachedStyle.Fragment)
                        {
                            flag = true;
                            break;
                        }
                    }
                    return flag;
                }
            }
        }

        private enum Bits : uint
        {
            TextFitsWidth = 1,
            TextFitsHeight = 2,
            ClipToHeight = 4,
            WordWrap = 8,
            PasswordMasked = 16, // 0x00000010
            KeepFlowAlive = 32, // 0x00000020
            TextLayoutInvalid = 64, // 0x00000040
            UpdateFragmentsAfterLayout = 128, // 0x00000080
            IgnoreEffectiveScaleChanges = 256, // 0x00000100
            Clipped = 512, // 0x00000200
            InMeasure = 1024, // 0x00000400
            FastMeasureValid = 2048, // 0x00000800
            FastMeasurePossible = 4096, // 0x00001000
            ContributesToWidth = 8192, // 0x00002000
            ScaleDirty = 16384, // 0x00004000
            HasEverPainted = 32768, // 0x00008000
            LineSpacingSet = 65536, // 0x00010000
            CharacterSpacingSet = 131072, // 0x00020000
            EnableKerningSet = 262144, // 0x00040000
            ViewDependent = 524288, // 0x00080000
        }
    }
}
