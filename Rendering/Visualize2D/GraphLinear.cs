using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Fluxion.Rendering.Draw;

namespace Fluxion.Rendering.Visualize
{
    public static class GraphLinear
    {
        /// <summary>
        /// Graph y = a*x + b and y = c*x + d around the solution.
        /// Draws both lines, the intersection dot, and (optional) axes + guides.
        /// </summary>
        public static void GraphEquation(
            IRenderer renderer,
            double a, double b, double c, double d,
            double solution,
            double range = 2.0,
            int samples = 200,
            bool drawAxes = true,
            bool drawGuides = true,
            float lineThickness = 2f,
            float guideThickness = 1f,
            float dotSize = 8f,
            float padFrac = 0.10f) // 10% vertical padding
        {
            if (renderer is null) return;
            if (double.IsNaN(solution) || double.IsInfinity(solution)) return;
            if (samples < 2) samples = 2;
            if (range <= 0) range = 1;

            double xMin = solution - range;
            double xMax = solution + range;
            double step = (xMax - xMin) / samples;

            var left = new List<Vector2>(samples + 1);
            var right = new List<Vector2>(samples + 1);

            // 1) Gather world points & compute y-extents
            float yMin = float.PositiveInfinity, yMax = float.NegativeInfinity;
            for (int i = 0; i <= samples; i++)
            {
                double x = xMin + i * step;
                float yL = (float)(a * x + b);
                float yR = (float)(c * x + d);

                var pL = new Vector2((float)x, yL);
                var pR = new Vector2((float)x, yR);
                left.Add(pL);
                right.Add(pR);

                if (yL < yMin) yMin = yL; if (yL > yMax) yMax = yL;
                if (yR < yMin) yMin = yR; if (yR > yMax) yMax = yR;
            }

            // also include the solution point in bounds
            float solXf = (float)solution;
            float solYf = (float)(a * solution + b);
            if (solYf < yMin) yMin = solYf;
            if (solYf > yMax) yMax = solYf;

            // 2) Expand vertical bounds for padding; guard degenerate span
            float spanY = yMax - yMin;
            if (spanY <= 1e-6f) { yMin -= 1f; yMax += 1f; spanY = yMax - yMin; }
            float padY = System.Math.Max(spanY * padFrac, 1e-3f);
            yMin -= padY; yMax += padY;

            // 3) World→NDC helpers
            static float Lerp(float a0, float a1, float t) => a0 + (a1 - a0) * t;
            static float InvLerp(float a0, float a1, float v)
            {
                float denom = (a1 - a0);
                return System.Math.Abs(denom) < 1e-12f ? 0.5f : (v - a0) / denom;
            }

            float xMinF = (float)xMin, xMaxF = (float)xMax;

            void NormalizeList(List<Vector2> pts)
            {
                for (int i = 0; i < pts.Count; i++)
                {
                    var p = pts[i];
                    float nx = Lerp(-1f, 1f, InvLerp(xMinF, xMaxF, p.X));
                    float ny = Lerp(-1f, 1f, InvLerp(yMin, yMax, p.Y));
                    pts[i] = new Vector2(nx, ny);
                }
            }

            NormalizeList(left);
            NormalizeList(right);

            // 4) Optional axes (only if 0 is within bounds)
            if (drawAxes)
            {
                // x-axis: y=0 inside [yMin,yMax] ?
                if (0f >= yMin && 0f <= yMax)
                {
                    var xAxis = new Vector2[2];
                    xAxis[0] = new Vector2(Lerp(-1f, 1f, 0f), Lerp(-1f, 1f, InvLerp(yMin, yMax, 0f))); // left at y=0
                    xAxis[1] = new Vector2(Lerp(-1f, 1f, 1f), xAxis[0].Y);                             // right at y=0
                    renderer.DrawLines(new ReadOnlySpan<Vector2>(xAxis), guideThickness);
                }

                // y-axis: x=0 inside [xMin,xMax] ?
                if (0f >= xMinF && 0f <= xMaxF)
                {
                    var yAxis = new Vector2[2];
                    yAxis[0] = new Vector2(Lerp(-1f, 1f, InvLerp(xMinF, xMaxF, 0f)), Lerp(-1f, 1f, 0f)); // bottom at x=0
                    yAxis[1] = new Vector2(yAxis[0].X, Lerp(-1f, 1f, 1f));                               // top at x=0
                    renderer.DrawLines(new ReadOnlySpan<Vector2>(yAxis), guideThickness);
                }
            }

            // 5) Draw both lines
            renderer.DrawLines(CollectionsMarshal.AsSpan(left), lineThickness);
            renderer.DrawLines(CollectionsMarshal.AsSpan(right), lineThickness);

            // 6) Solution dot + optional guides at the intersection
            float snx = Lerp(-1f, 1f, InvLerp(xMinF, xMaxF, solXf));
            float sny = Lerp(-1f, 1f, InvLerp(yMin, yMax, solYf));
            var dot = new Vector2(snx, sny);
            renderer.DrawPoints(new ReadOnlySpan<Vector2>(new[] { dot }), dotSize);

            if (drawGuides)
            {
                // vertical guide through solution
                var vGuide = new Vector2[]
                {
                    new Vector2(snx, -1f),
                    new Vector2(snx,  1f)
                };
                // horizontal guide through solution
                var hGuide = new Vector2[]
                {
                    new Vector2(-1f, sny),
                    new Vector2( 1f, sny)
                };
                renderer.DrawLines(new ReadOnlySpan<Vector2>(vGuide), guideThickness);
                renderer.DrawLines(new ReadOnlySpan<Vector2>(hGuide), guideThickness);
            }
        }
    }
}
