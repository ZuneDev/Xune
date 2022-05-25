/// https://gist.github.com/Bebo-Maker/770179e6e87afa1248dbd67c17bfcd0c

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;
using System;

namespace XuneSandbox.Win32;

public readonly struct RenderArgs
{
    public readonly SKCanvas Canvas;
    public readonly SKSize Size;

    public RenderArgs(SKCanvas canvas, SKSize size)
    {
        Canvas = canvas;
        Size = size;
    }
}

public class WindowWrapper : IDisposable
{
    private readonly IWindow _window;

    private const SKColorType ColorType = SKColorType.Rgba8888;
    private const GRSurfaceOrigin SurfaceOrigin = GRSurfaceOrigin.BottomLeft;

    private IInputContext? _inputContext;

    private GRContext? _grContext;
    private GRGlFramebufferInfo _glInfo;
    private GRBackendRenderTarget? _renderTarget;

    private SKSurface? _surface;
    private SKCanvas? _canvas;

    public event EventHandler<RenderArgs>? PaintSurface;
    public event EventHandler? Loaded;


    public int Width => _window.Size.X;
    public int Height => _window.Size.Y;
    public IInputContext InputContext => _inputContext;

    public WindowWrapper(WindowOptions options)
    {
        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Resize += OnResize;

        // If you want to rerender on every frame uncomment the following line.
        _window.IsEventDriven = true;
    }

    public void Run()
    {
        _window.Run();
    }

    private void OnLoad()
    {
        _inputContext = _window.CreateInput();
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    private void OnResize(Vector2D<int> obj)
    {
        _renderTarget = null;
    }

    private void OnRender(double obj)
    {
        // create the contexts if not done already
        if (_grContext == null)
        {
            _window.MakeCurrent();
            var glInterface = GRGlInterface.Create();
            _grContext = GRContext.CreateGl(glInterface);
        }

        // get the new surface size
        var newSize = new SKSizeI(Width, Height);

        // manage the drawing surface
        if (_renderTarget == null || !_renderTarget.IsValid)
            CreateNewRenderTarget(newSize);

        if (_surface == null)
            CreateSurface();

        using (new SKAutoCanvasRestore(_canvas, true))
            PaintSurface?.Invoke(this, new RenderArgs(_canvas!, newSize));

        // update the control
        _canvas?.Flush();
    }

    private void CreateNewRenderTarget(SKSizeI newSize)
    {
        var gl = GL.GetApi(_window.GLContext?.Source);
        gl.GetInteger(GetPName.DrawFramebufferBinding, out var framebuffer);
        gl.GetInteger(GetPName.StencilTest, out var stencil);
        gl.GetInteger(GetPName.Samples, out var samples);

        var maxSamples = _grContext?.GetMaxSurfaceSampleCount(ColorType);
        if (samples > maxSamples || samples <= 0)
            samples = maxSamples ?? 1;

        _glInfo = new GRGlFramebufferInfo((uint)framebuffer, ColorType.ToGlSizedFormat());

        // destroy the old surface
        _surface?.Dispose();
        _surface = null;
        _canvas = null;

        // re-create the render target
        _renderTarget?.Dispose();
        _renderTarget = new GRBackendRenderTarget(newSize.Width, newSize.Height, samples, stencil, _glInfo);
    }

    public void Dispose()
    {
        _renderTarget?.Dispose();
        _surface?.Dispose();
        _grContext?.Dispose();
        _window?.Dispose();

        GC.SuppressFinalize(this);
    }

    private void CreateSurface()
    {
        _surface = SKSurface.Create(_grContext, _renderTarget, SurfaceOrigin, ColorType);
        _canvas = _surface.Canvas;
    }
}
