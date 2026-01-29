// Fluxion.Rendering/Visualize/PlotRenderer.cs
using System.Numerics;
using System.Runtime.InteropServices;
using Rendering.Draw;

namespace Rendering.Visualize
{
    public static class PlotRenderer
    {
        public static void Render(IRenderer renderer, Plot2D plot)
        {
            if (plot.Style.Lines && plot.Points.Count > 1)
            {
                renderer.DrawLines(CollectionsMarshal.AsSpan(plot.Points), plot.Style.Width);
            }

            if (plot.Style.Points && plot.Points.Count > 0)
            {
                renderer.DrawPoints(CollectionsMarshal.AsSpan(plot.Points), plot.Style.Width);
            }
        }

        // Convenience: render two plots (e.g., left & right sides of an equation)
        public static void RenderPair(IRenderer renderer, Plot2D a, Plot2D b)
        {
            Render(renderer, a);
            Render(renderer, b);
        }

        // Convenience: draw a single highlight point (e.g., solution/vertex)
        public static void RenderPoint(IRenderer renderer, Vector2 p, float size = 6f)
        {
            // Wrap single point into a tiny span
            var one = new[] { p };
            renderer.DrawPoints(one.AsSpan(), size);
        }
    }
}
