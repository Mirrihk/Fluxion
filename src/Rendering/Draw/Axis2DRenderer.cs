// Fluxion.Rendering/Draw/Axes2DRenderer.cs
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Fluxion.src.Rendering.Draw
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

            // reusable 2-point span for all line drawing
            Span<Vector2> line = stackalloc Vector2[2];

            // --- GRID ---
            if (opts.ShowGrid)
            {
                // Minor verticals
                if (opts.ShowMinor)
                {
                    foreach (var x in RangeWith0Safe(xMin, xMax, minorX))
                    {
                        if (NearlyMultiple(x, stepX)) continue; // skip where a major will be

                        line[0] = new Vector2((float)x, (float)yMin);
                        line[1] = new Vector2((float)x, (float)yMax);
                        r.DrawLines(line, opts.MinorGridWidth);
                    }
                }

                // Minor horizontals
                if (opts.ShowMinor)
                {
                    foreach (var y in RangeWith0Safe(yMin, yMax, minorY))
                    {
                        if (NearlyMultiple(y, stepY)) continue;

                        line[0] = new Vector2((float)xMin, (float)y);
                        line[1] = new Vector2((float)xMax, (float)y);
                        r.DrawLines(line, opts.MinorGridWidth);
                    }
                }

                // Major verticals
                foreach (var x in RangeWith0Safe(xMin, xMax, stepX))
                {
                    line[0] = new Vector2((float)x, (float)yMin);
                    line[1] = new Vector2((float)x, (float)yMax);
                    r.DrawLines(line, opts.MajorGridWidth);
                }

                // Major horizontals
                foreach (var y in RangeWith0Safe(yMin, yMax, stepY))
                {
                    line[0] = new Vector2((float)xMin, (float)y);
                    line[1] = new Vector2((float)xMax, (float)y);
                    r.DrawLines(line, opts.MajorGridWidth);
                }
            }

            // --- AXES (thicker) ---
            if (xMin <= 0 && 0 <= xMax)
            {
                line[0] = new Vector2(0f, (float)yMin);
                line[1] = new Vector2(0f, (float)yMax);
                r.DrawLines(line, opts.AxisWidth); // Y axis
            }

            if (yMin <= 0 && 0 <= yMax)
            {
                line[0] = new Vector2((float)xMin, 0f);
                line[1] = new Vector2((float)xMax, 0f);
                r.DrawLines(line, opts.AxisWidth); // X axis
            }

            // --- TICKS ---
            if (opts.ShowTicks)
            {
                var tX = opts.TickLength;
                var tY = opts.TickLength;

                // ticks on X axis (for major X positions, if X axis visible)
                if (yMin <= 0 && 0 <= yMax)
                {
                    foreach (var x in RangeWith0Safe(xMin, xMax, stepX))
                    {
                        line[0] = new Vector2((float)x, -tY);
                        line[1] = new Vector2((float)x, +tY);
                        r.DrawLines(line, opts.AxisWidth);
                    }
                }

                // ticks on Y axis (for major Y positions, if Y axis visible)
                if (xMin <= 0 && 0 <= xMax)
                {
                    foreach (var y in RangeWith0Safe(yMin, yMax, stepY))
                    {
                        line[0] = new Vector2(-tX, (float)y);
                        line[1] = new Vector2(+tX, (float)y);
                        r.DrawLines(line, opts.AxisWidth);
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
