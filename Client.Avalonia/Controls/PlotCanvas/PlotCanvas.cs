using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Client.Avalonia.Extensions;
using Client.Core.Data;
using SkiaSharp;

namespace Client.Avalonia.Controls.PlotCanvas;

public class PlotCanvas : Control
{
    public List<Stratum> Rectangles { get; } = new();

    public PlotCanvas()
    {
        // Добавляем примеры прямоугольников
        Rectangles.Add(new() { Id = Guid.NewGuid() }); // Активный (Opacity = 1)

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var size = new SKSize((float)Bounds.Width, (float)Bounds.Height);
        using var bitmap = new SKBitmap((int)size.Width, (int)size.Height);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.White);

        // 🎯 Рисуем сетку
        using var gridPaint = new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1 };

        for (int x = 50; x <= 300; x += 50)
            canvas.DrawLine(x, 50, x, 250, gridPaint);

        for (int y = 50; y <= 250; y += 50)
            canvas.DrawLine(50, y, 300, y, gridPaint);

        // 🎯 Рисуем оси координат
        using var axisPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2 };

        canvas.DrawLine(20, 200, 300, 200, axisPaint); // Ось X
        canvas.DrawLine(50, 50, 50, 250, axisPaint);   // Ось Y

        // 🎯 Подписи осей
        using var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 12, IsAntialias = true };

        for (int x = 50; x <= 300; x += 50)
            canvas.DrawText(x.ToString(), x - 10, 220, textPaint);

        for (int y = 50; y <= 250; y += 50)
            canvas.DrawText(y.ToString(), 25, y + 5, textPaint);

        // 🎯 Рисуем прямоугольники
        foreach (var rectModel in Rectangles)
        {
            using var rectPaint = new SKPaint
            {
                Color = SKColors.Blue.WithAlpha(
                    rectModel.IsActive
                        ? (byte)255
                        : (byte)64
                ), // Opacity 1 или 0.25
                Style = SKPaintStyle.Fill
            };

            canvas.DrawRect(rectModel.ToSkRect(), rectPaint);
        }

        // 🎯 Подписываем координаты углов прямоугольников
        using var cornerTextPaint = new SKPaint() { Color = SKColors.Red, TextSize = 10, IsAntialias = true };

        foreach (var rectModel in Rectangles)
        {
            var rect = rectModel.ToSkRect();
            canvas.DrawText($"({rect.Left}, {rect.Top})", rect.Left + 5, rect.Top - 5, cornerTextPaint);
            canvas.DrawText($"({rect.Right}, {rect.Bottom})", rect.Right - 50, rect.Bottom + 15, cornerTextPaint);
        }

        // 🎯 Переносим картинку в Avalonia
        using var skiaImage = SKImage.FromBitmap(bitmap);
        using var skiaData = skiaImage.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = skiaData.AsStream();
        var image = new global::Avalonia.Media.Imaging.Bitmap(stream);

        context.DrawImage(image, new Rect(0, 0, Bounds.Width, Bounds.Height));
    }

    public void AddRectangle(bool isActive)
    {
        // int count = Rectangles.Count;
        // var newRect = new Stratum(100 + count * 20, 100 + count * 20, 50, 40, isActive);
        //
        // Rectangles.AddAsync(newRect);
        InvalidateVisual();
    }
}