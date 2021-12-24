// Decompiled with JetBrains decompiler
// Type: Microsoft.Iris.Drawing.RichText
// Assembly: UIX, Version=4.8.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: A56C6C9D-B7F6-46A9-8BDE-B3D9B8D60B11
// Assembly location: C:\Program Files\Zune\UIX.dll

using Microsoft.Iris.OS;
using Microsoft.Iris.Render;
using Microsoft.Iris.RenderAPI;
using Microsoft.Iris.RenderAPI.Drawing;
using Microsoft.Iris.Session;
using Microsoft.Iris.ViewItems;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using Topten.RichTextKit;
using Topten.RichTextKit.Editor;

namespace Microsoft.Iris.Drawing
{
    internal sealed class RichText : IDisposable
    {
        public const float MaxWidthConstraint = 4095f;
        public const float MaxHeightConstraint = 8191f;
        private RichString _string;
        private TextRange _selection;
        private NativeApi.ReportRunCallback _rrcb;
        private string _currentlyMeasuringText;
        private bool _oversampled;
        private bool _hosted;
        private bool _inImeCompositionMode;
        private ArrayList _timers;
        private EventHandler _timerTickHandler;
        private object _lock;

        public RichText(bool richTextMode)
          : this(richTextMode, null)
        {
        }

        public RichText(bool richTextMode, IRichTextCallbacks callbacks)
        {
            Size sizeMaximumSurface = Size.Zero;
            if (UISession.Default != null)
                sizeMaximumSurface = UIImage.MaximumSurfaceSize(UISession.Default);
            if (callbacks != null)
            {
                _hosted = true;
                _timers = new ArrayList(6);
                _timerTickHandler = new EventHandler(OnTimerTick);
            }
            _string = new();
            _oversampled = false;
            _lock = new object();
        }

        public void Dispose() => Dispose(true);

        private void Dispose(bool inDispose)
        {
            if (!inDispose)
                return;
            GC.SuppressFinalize(this);
            if (_timers == null)
                return;
            for (int index = 0; index < _timers.Count; ++index)
                DisposeTimer((DispatcherTimer)_timers[index]);
            _timers.Clear();
        }

        ~RichText() => Dispose(false);

        public string Content
        {
            set => _string = new(value);
            get => _string.ToString();
        }

        public string SimpleContent
        {
            get => _string.Normal().ToString();
            set => Content = value;
        }

        public bool Oversample
        {
            set => _oversampled = value;
            get => _oversampled;
        }

        public int MaxLength
        {
            set
            {
                // TODO: Set max character length
            }
        }

        public bool DetectUrls
        {
            set
            {
                // TODO: Detect URLs
            }
        }

        public bool HasCallbacks => _hosted;

        public bool ReadOnly
        {
            set
            {
                // TODO: Read only
            }
        }

        public void SetWordWrap(bool wordWrap)
        {
            // TODO: Word wrap
        }

        public Size GetNaturalBounds()
        {
            int cWidth = (int)_string.MeasuredWidth;
            int cHeight = (int)_string.MeasuredHeight;
            return new Size(cWidth, cHeight);
        }

        public void SetSelectionRange(int selectionStart, int selectionEnd)
        {
            _selection = new TextRange(selectionStart, selectionEnd);
        }

        public RichString Measure(string content)
        {
            return new RichString(content);
        }

        public void NotifyOfFocusChange(bool gainingFocus)
        {
            if (gainingFocus)
                return;
            _inImeCompositionMode = false;
        }

        public void ScrollUp(ScrollbarType whichBar)
        {
            // TODO: Scroll up
        }

        public void ScrollDown(ScrollbarType whichBar)
        {
            // TODO: Scroll down
        }

        public void PageUp(ScrollbarType whichBar)
        {
            // TODO: Page up
        }

        public void PageDown(ScrollbarType whichBar)
        {
            // TODO: Page down
        }

        public void ScrollToPosition(ScrollbarType whichBar, int whereTo)
        {
            // TODO: Scroll
        }

        public void SetScrollbars(bool horizontalScrollbar, bool verticalScrollbar)
        {
            // Show/Hide scrollbars
        }

        public bool ForwardKeyStateNotification(uint message, int virtualKey, int scanCode, int repeatCount, uint modifierState, ushort flags)
        {
            bool handled = true;
            // TODO: Implement forward key state
            return handled;
        }

        public bool ForwardKeyCharacterNotification(uint message, int character, int scanCode, int repeatCount, uint modifierState, ushort flags)
        {
            bool handled = true;
            // TODO: Implement forward key character
            return handled;
        }

        public bool ForwardMouseInput(uint message, uint modifierState, int mouseButton, int x, int y, int mouseWheelDelta)
        {
            bool handled = true;
            // TODO: Implement forward mouse input
            return handled;
        }

        public HRESULT ForwardImeMessage(uint message, UIntPtr wParam, UIntPtr lParam)
        {
            lock (_lock)
            {
                switch (message)
                {
                    case 269:
                        _inImeCompositionMode = true;
                        break;
                    case 270:
                        _inImeCompositionMode = false;
                        break;
                }
                //RendererApi.IFC(NativeApi.SpRichTextForwardImeMessage(_rtoHandle, message, wParam, lParam));
            }
            return new HRESULT(0);
        }

        public bool CanUndo
        {
            get => false;
        }

        public void Undo()
        {
            throw new NotImplementedException();
        }

        public void Cut()
        {
            // TODO: Implement selection
            string clipContent = SimpleContent.Substring(_selection.Start, _selection.Length);
            Content = SimpleContent.Remove(_selection.Start, _selection.Length);

            // TODO: Copy to clipboard
            
        }

        public void Copy()
        {
            // TODO: Implement selection
            string clipContent = SimpleContent.Substring(_selection.Start, _selection.Length);
            // TODO: Copy to clipboard
        }

        public void Paste()
        {
            // TODO: Implement selection
            // TODO: Get content from clipboard
            string clipContent = "xune";
            string text = SimpleContent;
            if (_selection.IsRange)
                text = text.Remove(_selection.Start, _selection.Length);
            Content = text.Insert(_selection.Start, clipContent);
        }

        public void Delete()
        {
            // TODO: Implement selection
            string content = SimpleContent;
            if (_selection.IsRange)
            {
                Content = SimpleContent.Remove(_selection.Start, _selection.Length);
            }
            else if (_selection.Maximum == content.Length)
            {
                Content = content.Substring(0, content.Length - 1);
            }
            else
            {
                Content = content.Remove(_selection.Start, 1);
            }
        }

        public static LineAlignment ReverseAlignment(
          LineAlignment alignment,
          bool condition)
        {
            if (condition)
            {
                switch (alignment)
                {
                    case LineAlignment.Near:
                        alignment = LineAlignment.Far;
                        break;
                    case LineAlignment.Far:
                        alignment = LineAlignment.Near;
                        break;
                }
            }
            return alignment;
        }

        internal SkiaSharp.SKBitmap Rasterize(bool outlineMode, Color textColor, bool shadowMode)
        {
            return Rasterize(_string, outlineMode, textColor, shadowMode);
        }

        internal static SkiaSharp.SKBitmap Rasterize(RichString str, bool outlineMode, Color textColor, bool shadowMode)
        {
            // TODO: Re-implement using SkiaSharp and/or ImageSharp
            str = str.TextColor(new SkiaSharp.SKColor(textColor.R, textColor.G, textColor.B, textColor.A));

            // Render to bitmap
            SkiaSharp.SKBitmap bitmap = new((int)str.MeasuredWidth, (int)str.MeasuredHeight);
            using (SkiaSharp.SKCanvas canvas = new(bitmap))
            {
                str.Paint(canvas);/*, new TextPaintOptions
                {
                    Selection = _selection
                });*/
            }

            return bitmap;
        }

        public void SetTimer(uint id, uint timeout)
        {
            DispatcherTimer dispatcherTimer = FindTimer(id);
            if (dispatcherTimer == null)
            {
                dispatcherTimer = new DispatcherTimer();
                _timers.Add(dispatcherTimer);
            }
            dispatcherTimer.UserData = id;
            dispatcherTimer.Interval = (int)timeout;
            dispatcherTimer.Tick += _timerTickHandler;
            dispatcherTimer.Start();
        }

        public void KillTimer(uint id)
        {
            DispatcherTimer timer = FindTimer(id);
            if (timer == null)
                return;
            DisposeTimer(timer);
            _timers.Remove(timer);
        }

        private DispatcherTimer FindTimer(uint id)
        {
            for (int index = 0; index < _timers.Count; ++index)
            {
                DispatcherTimer timer = (DispatcherTimer)_timers[index];
                if ((int)(uint)timer.UserData == (int)id)
                    return timer;
            }
            return null;
        }

        private void DisposeTimer(DispatcherTimer timer)
        {
            timer.Tick -= _timerTickHandler;
            timer.Stop();
        }

        private void OnTimerTick(object sender, EventArgs ea)
        {
            DispatcherTimer dispatcherTimer = (DispatcherTimer)sender;
        }
    }
}
