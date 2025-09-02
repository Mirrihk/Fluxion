// Fluxion.Rendering/Draw/Axes2DRenderer.cs
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Fluxion.Rendering.Draw
{
    public sealed class Axes2DOptions
    {
        public float AxisWidth = 1.5f;
        public float MajorGridWidth = 1.0f;
        public float MinorGridWidth = 0.6f;
        public int MinorDivisions = 5;     // minor lines between majors
        public bool ShowMinor = true;
        public bool ShowGrid = true;
        public bool ShowTicks = true;
        public float TickLength = 0.03f;   // in world units
    }

    public static class Axes2DRenderer
    {
        /// <summary>Draws axes + grid aligned to world bounds.</summary>
        public static void Draw(IRenderer r,
                                double xMin, double xMax,
                                double yMin, double yMax,
                                Axes2DOptions? opts = null)
        {
            opts ??= new Axes2DOptions();

            // 1) Nice step sizes
            var stepX = NiceStep(xMin, xMax);
            var stepY = NiceStep(yMin, yMax);

            // 2) Minor step
            var minorX = opts.ShowMinor ? stepX / System.Math.Max(2, opts.MinorDivisions) : 0.0;
            var minorY = opts.ShowMinor ? stepY / System.Math.Max(2, opts.MinorDivisions) : 0.0;

            // --- GRID ---
            if (opts.ShowGrid)
            {
                // Minor verticals
                if (opts.ShowMinor)
                {
                    foreach (var x in RangeWith0Safe(xMin, xMax, minorX))
                    {
                        if (NearlyMultiple(x, stepX)) continue; // skip where a major will be
                        r.DrawLines(stackalloc Vector2[] { new((float)x, (float)yMin), new((float)x, (float)yMax) }, opts.MinorGridWidth);
                    }
                }
                // Minor horizontals
                if (opts.ShowMinor)
                {
                    foreach (var y in RangeWith0Safe(yMin, yMax, minorY))
                    {
                        if (NearlyMultiple(y, stepY)) continue;
                        r.DrawLines(stackalloc Vector2[] { new((float)xMin, (float)y), new((float)xMax, (float)y) }, opts.MinorGridWidth);
                    }
                }
                // Major verticals
                foreach (var x in RangeWith0Safe(xMin, xMax, stepX))
                    r.DrawLines(stackalloc Vector2[] { new((float)x, (float)yMin), new((float)x, (float)yMax) }, opts.MajorGridWidth);

                // Major horizontals
                foreach (var y in RangeWith0Safe(yMin, yMax, stepY))
                    r.DrawLines(stackalloc Vector2[] { new((float)xMin, (float)y), new((float)xMax, (float)y) }, opts.MajorGridWidth);
            }

            // --- AXES (thicker) ---
            if (xMin <= 0 && 0 <= xMax)
                r.DrawLines(stackalloc Vector2[] { new(0, (float)yMin), new(0, (float)yMax) }, opts.AxisWidth); // Y axis

            if (yMin <= 0 && 0 <= yMax)
                r.DrawLines(stackalloc Vector2[] { new((float)xMin, 0), new((float)xMax, 0) }, opts.AxisWidth); // X axis

            // --- TICKS ---
            if (opts.ShowTicks)
            {
                var tX = (float)opts.TickLength;
                var tY = (float)opts.TickLength;

                // ticks on X axis (for major X positions, if X axis visible)
                if (yMin <= 0 && 0 <= yMax)
                {
                    foreach (var x in RangeWith0Safe(xMin, xMax, stepX))
                    {
                        var p1 = new Vector2((float)x, -tY);
                        var p2 = new Vector2((float)x, +tY);
                        r.DrawLines(stackalloc Vector2[] { p1, p2 }, opts.AxisWidth);
                    }
                }
                // ticks on Y axis (for major Y positions, if Y axis visible)
                if (xMin <= 0 && 0 <= xMax)
                {
                    foreach (var y in RangeWith0Safe(yMin, yMax, stepY))
                    {
                        var p1 = new Vector2(-tX, (float)y);
                        var p2 = new Vector2(+tX, (float)y);
                        r.DrawLines(stackalloc Vector2[] { p1, p2 }, opts.AxisWidth);
                    }
                }
            }
        }

        // --- helpers ---

        static double NiceStep(double min, double max, int targetLines = 10)
        {
            var span = System.Math.Max(1e-9, max - min);
            var rough = span / targetLines;
            var mag = System.Math.Pow(10, System.Math.Floor(System.Math.Log10(rough)));
            var norm = rough / mag;           // 1..10
            double nice;
            if (norm < 1.5) nice = 1;
            else if (norm < 3) nice = 2;
            else if (norm < 7) nice = 5;
            else nice = 10;
            return nice * mag;
        }

        static IEnumerable<double> RangeWith0Safe(double min, double max, double step)
        {
            if (step <= 0) yield break;
            // start at first multiple of step >= min
            var start = System.Math.Ceiling(min / step) * step;
            for (double v = start; v <= max + 1e-12; v += step)
                yield return SnapIfNearZero(v);
        }

        static double SnapIfNearZero(double v) => System.Math.Abs(v) < 1e-9 ? 0.0 : v;
        static bool NearlyMultiple(double v, double step)
        {
            var m = v / step;
            var nearest = System.Math.Round(m);
            return System.Math.Abs(m - nearest) < 1e-6;
        }
    }
}
