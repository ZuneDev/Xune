using Silk.NET.OpenGL;
using Silk.NET.GLFW;
using SkiaSharp;
using System;
using System.Drawing;

namespace XuneSandbox.Win32;

public class Program
{
    public static unsafe void Main(string[] args)
    {
        Console.WriteLine("Howdy!");

        int initWidth = 800;
        int initHeight = 600;

        var GLFW = Glfw.GetApi();
        GLFW.Init();

        var window = GLFW.CreateWindow(initWidth, initHeight, "Xune", (Monitor*)IntPtr.Zero.ToPointer(), (WindowHandle*)IntPtr.Zero.ToPointer());
        GLFW.MakeContextCurrent(window);

        SKImageInfo imageInfo = new(initWidth, initHeight);

        GRContext context = GRContext.CreateGl(GRGlInterface.CreateOpenGl(name => new IntPtr(GLFW.GetProcAddress(name))));

        using SKSurface surface = SKSurface.Create(context, false, imageInfo, 1, GRSurfaceOrigin.TopLeft);
        SKCanvas canvas = surface.Canvas;
        float hue = 0.0f;

        while (!GLFW.WindowShouldClose(window))
        {
            GLFW.GetWindowSize(window, out var width, out var height);
            var suiv = SKTypeface.FromFamilyName("Segoe UI Variable");
            SKPaint textPaint = new(new SKFont(suiv))
            {
                Color = SKColors.White,
            };

            hue += 0.1f;
            var backColor = SKColor.FromHsl(hue, 100.0f, 50f);
            System.Diagnostics.Debug.WriteLine(backColor);
            canvas.Clear(backColor);
            //canvas.DrawRect(0, 0, 100, 50, textPaint);
            canvas.Flush();

            GLFW.SwapBuffers(window);
            GLFW.PollEvents();
        }
    }
}