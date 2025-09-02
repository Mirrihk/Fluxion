// Fluxion.Features/Graph2DFeature.cs
using System;
using Fluxion.Rendering.SceneImpl;
using Fluxion.Rendering.Visualize;    // Plot2DFactory
using Fluxion.Rendering.Windowing;    // FluxWindow

namespace Fluxion.Features
{
    public static class Graph2DFeature
    {
        public static void Function(Func<double, double> f, double xMin, double xMax, int samples = 1000)
        {
            var plot = Plot2DFactory.Function(f, xMin, xMax, samples);
            // Estimate Y range (same helper you already have)
            var (yMin, yMax) = EstimateYRange(f, xMin, xMax, samples);

            using var window = new FluxWindow("Fluxion 2D", 1200, 800,
                               new Plot2DScene(plot, xMin, xMax, yMin, yMax));
            window.Run();
        }

        private static (double yMin, double yMax) EstimateYRange(Func<double, double> f, double xMin, double xMax, int samples)
        {
            if (samples < 2) samples = 2;
            double step = (xMax - xMin) / (samples - 1);
            double min = double.PositiveInfinity, max = double.NegativeInfinity;
            for (int i = 0; i < samples; i++)
            {
                double x = xMin + i * step, y = f(x);
                if (double.IsNaN(y) || double.IsInfinity(y)) continue;
                if (y < min) min = y; if (y > max) max = y;
            }
            if (double.IsInfinity(min) || double.IsInfinity(max)) { min = -1; max = 1; }
            double span = System.Math.Abs(max - min);
            double pad = span < 1e-6 ? System.Math.Max(1.0, System.Math.Abs(max) * 0.1 + 1.0) : 0.1 * span;
            return (min - pad, max + pad);
        }
    }
}
