using Silk.NET.OpenGL;
using Silk.NET.GLFW;
using SkiaSharp;
using System;
using System.Drawing;
using Silk.NET.Windowing;
using Silk.NET.Input;
using System.Diagnostics;
using System.Numerics;

namespace XuneSandbox.Win32;

public class Program
{
    static float hue = 0.0f;

    public static unsafe void Main(string[] args)
    {
        Console.WriteLine("Howdy!");

        Glfw glfw = Glfw.GetApi();
        int initWidth = 800;
        int initHeight = 600;

        WindowOptions options = WindowOptions.Default;
        options.Title = "Xune";
        options.Size = new(initWidth, initHeight);

        WindowWrapper window = new(options);

        window.PaintSurface += Window_PaintSurface;
        window.Loaded += Window_Loaded;

        window.Run();
    }

    private static void Window_Loaded(object? sender, EventArgs e)
    {
        if (sender is not WindowWrapper window) return;

        if (window.InputContext.Mice.Count > 0)
        {
            var mouse = window.InputContext.Mice[0];
            mouse.Click += Mouse_Click;
        }
    }

    private static void Mouse_Click(IMouse mouse, Silk.NET.Input.MouseButton button, Vector2 pos)
    {
        lastClick = new(pos.X, pos.Y);
    }

    static SKPoint lastClick = SKPoint.Empty;
    private static void Window_PaintSurface(object? sender, RenderArgs e)
    {
        hue += 1.0f;
        hue %= 360;
        e.Canvas.Clear(SKColor.FromHsl(hue, 100f, 50f));

        e.Canvas.DrawCircle(lastClick, 10, new SKPaint { Color = SKColors.Black });
    }
}