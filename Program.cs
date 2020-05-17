using System;
using System.IO;
using SkiaSharp;

namespace SkiaFiddle
{
    class Program
    {
        static SKColor StupidBlend(SKColor bg, SKColor fg)
        {
            using (var bmp = new SKBitmap(1, 1))
            {
                bmp.SetPixel(0, 0, bg);
                using (var canvas = new SKCanvas(bmp))
                using (var paint = new SKPaint
                {
                    Color = fg
                })
                    canvas.DrawRect(-1, -1, 3, 3, paint);
                return bmp.GetPixel(0, 0);
            }
        }

        static SKColorFilter CreateAlphaColorFilter(double opacity)
        {
            if (opacity > 1)
                opacity = 1;
            var c = new byte[256];
            var a = new byte[256];
            for (var i = 0; i < 256; i++)
            {
                c[i] = 255;
                a[i] = (byte)(i * opacity);
            }

            return SKColorFilter.CreateTable(a, c, c, c);
        }

        static void Main(string[] args)
        {
            var bmp = SKBitmap.Decode("tree.jpg");
            //var bmp = new SKBitmap(1280, 800);
            var canvas = new SKCanvas(bmp);
            var rect = new SKRectI(400, 200, 800, 600);

            {
                // Blur behind will be done by the compositor
                var copy = new SKBitmap();
                bmp.ExtractSubset(copy, rect);
                
                var blurPaint = new SKPaint();
                blurPaint.ImageFilter =
                    SKImageFilter.CreateBlur(10, 10);
                canvas.Save();
                canvas.ClipRect(rect);
                canvas.DrawBitmap(copy, rect, blurPaint);
                canvas.Restore();
            }
            
            var paint = new SKPaint();

            var tintOpacity = 0.6;
            var noiseOpcity = 0.1;
            
            var excl = new SKColor(255, 255, 255, 25);
            var tint = new SKColor(255, 255, 255, (byte) (255 * tintOpacity));

            tint =  StupidBlend(excl, tint);

            var tintShader = SKShader.CreateColor(tint);
            var noiseShader = 
                //SKShader.CreatePerlinNoiseImprovedNoise(0.5f, 0.5f, 4, 0)
                SKShader.CreatePerlinNoiseTurbulence(0.4f, 0.4f, 4, 0)
                .WithColorFilter(CreateAlphaColorFilter(noiseOpcity));

            var compose = SKShader.CreateCompose(tintShader, noiseShader);
            paint.Shader = compose;
            //canvas.Clear(SKColors.Black);
            canvas.DrawRect(400, 200, 400, 400, paint);
            using (var output = File.Create("out.png"))
                bmp.Encode(output, SKEncodedImageFormat.Png, 1);
        }
    }
}