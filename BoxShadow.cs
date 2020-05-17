using System;
using SkiaSharp;

namespace SkiaFiddle
{
    public struct BoxShadow
    {
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public float Blur { get; set; }
        public float Spread { get; set; }
        public SKColor Color { get; set; }
        public bool IsInset { get; set; }

        public void Draw(SKCanvas Canvas, SKRoundRect skRoundRect, double opacity)
        {
            if(IsInset)
                DrawInset(Canvas, skRoundRect, opacity);
            else
                throw new NotSupportedException();
        }
        void DrawInset(SKCanvas Canvas, SKRoundRect skRoundRect, double opacity)
        {
            var boxShadow = this;
            using(var shadowPaint = new SKPaint())
            using (var shadow = BoxShadowFilter.Create(shadowPaint, boxShadow, opacity))
            {
                var spread = (float)boxShadow.Spread;
                var offsetX = (float)boxShadow.OffsetX;
                var offsetY = (float)boxShadow.OffsetY;
                var outerRect = AreaCastingShadowInHole(skRoundRect.Rect, (float)boxShadow.Blur, spread, offsetX, offsetY);

                Canvas.Save();
                using var shadowRect = new SKRoundRect(skRoundRect);
                if (spread != 0)
                    shadowRect.Deflate(spread, spread);
                Canvas.ClipRoundRect(skRoundRect, 
                    shadow.ClipOperation, true);
                Canvas.Translate(boxShadow.OffsetX, boxShadow.OffsetY);
                using (var outerRRect = new SKRoundRect(outerRect))
                    Canvas.DrawRoundRectDifference(outerRRect, shadowRect, shadow.Paint);
                Canvas.Restore();
            }
        }
        struct BoxShadowFilter : IDisposable
        {
            public SKPaint Paint;
            private SKImageFilter _filter;
            public SKClipOperation ClipOperation;

            static float SkBlurRadiusToSigma(double radius) {
                if (radius <= 0)
                    return 0.0f;
                return 0.288675f * (float)radius + 0.5f;
            }
            public static BoxShadowFilter Create(SKPaint paint, BoxShadow shadow, double opacity)
            {
                var color = shadow.Color;
                color = color.WithAlpha((byte) (color.Alpha * opacity));

                SKImageFilter filter = null;
                filter = SKImageFilter.CreateBlur(SkBlurRadiusToSigma(shadow.Blur), SkBlurRadiusToSigma(shadow.Blur));
                

                paint.Reset();
                paint.IsAntialias = true;
                paint.Color = color;
                paint.ImageFilter = filter;
                
                return new BoxShadowFilter
                {
                    Paint = paint, _filter = filter,
                    ClipOperation = shadow.IsInset ? SKClipOperation.Intersect : SKClipOperation.Difference
                };
            }

            public void Dispose()
            {
                Paint.Reset();
                Paint = null;
                _filter?.Dispose();
            }

            public void DrawInset(SKCanvas canvas, SKRoundRect rect)
            {
                
            }
        }
        SKRect AreaCastingShadowInHole(
            SKRect hole_rect,
            float shadow_blur,
            float shadow_spread,
            float offsetX, float offsetY)
        {
            // Adapted from Chromium
            var bounds = hole_rect;

            bounds.Inflate(shadow_blur, shadow_blur);

            if (shadow_spread < 0)
                bounds.Inflate(-shadow_spread, -shadow_spread);

            var offset_bounds = bounds;
            offset_bounds.Offset(-offsetX, -offsetY);
            bounds.Union(offset_bounds);
            return bounds;
        }
    }
}