// Fluxion.Features/Graph.cs
using System;

namespace Fluxion.Features
{
    /// <summary>How to lift a 1D function f(x) into a 3D surface z = g(x, y).</summary>
    public enum Lift
    {
        /// z = f(x)  (extrude along y)
        Extrude,
        /// z = f(sqrt(x^2 + y^2))  (radial lift)
        Radial,
        /// z = f(x) * cos(y)
        ProductCos,
        /// z = f(x) * sin(y)
        ProductSin
    }

    /// <summary>Which plane to embed a 2D line in when drawing as a 3D line.</summary>
    public enum EmbedPlane { XY, XZ, YZ }

    /// <summary>Unified entry points for graphing.</summary>
    public static class Graph
    {
        /// Start from a 1D function f(x).
        public static Graph1D F(Func<double, double> fx) => new(fx);

        /// Start from a 2D scalar field g(x,y).
        public static Graph2D F2(Func<double, double, double> gxy) => new(gxy);

        /// Start a y = m x + b graph (2D or 3D line).
        public static GraphLine Line(double m, double b) => new GraphLine(m, b);

        /// Graph a polyline from sample points (x,y) with no equation.
        public static GraphPoints Points(params (double x, double y)[] pts) => new GraphPoints(pts);
    }

    /// <summary>1D function f(x).</summary>
    public readonly struct Graph1D
    {
        private readonly Func<double, double> f;
        public Graph1D(Func<double, double> f) => this.f = f;

        /// Plot f(x) in 2D.
        public void TwoD(double xMin, double xMax, int samples = 1000)
        {
            var fLocal = f; // avoid capturing 'this' (struct) in lambdas
            Graph2DFeature.Function(fLocal, xMin, xMax, samples);
        }

        /// Plot the same f(x) as a 3D surface via a lift.
        public void ThreeD(
            double xMin, double xMax,
            double yMin, double yMax,
            Lift lift = Lift.ProductCos,
            int resolution = 120, bool wireframe = false)
        {
            var fLocal = f;
            Func<double, double, double> g = lift switch
            {
                Lift.Extrude => (x, y) => fLocal(x),
                Lift.Radial => (x, y) => fLocal(global::System.Math.Sqrt(x * x + y * y)),
                Lift.ProductCos => (x, y) => fLocal(x) * global::System.Math.Cos(y),
                Lift.ProductSin => (x, y) => fLocal(x) * global::System.Math.Sin(y),
                _ => (x, y) => fLocal(x)
            };

            Graph3DFeature.Surface(g, xMin, xMax, yMin, yMax, resolution, wireframe);
        }

        /// Plot y = f(x) as a 3D **line** embedded in a plane (not a surface).
        public void ThreeDLine(
            double xMin, double xMax,
            EmbedPlane plane = EmbedPlane.XY,
            double offset = 0,
            int samples = 1000)
        {
            var fLocal = f;
            switch (plane)
            {
                case EmbedPlane.XY:
                    // XY plane at Z = offset  → GL (x, y=f(x), z=offset)
                    Graph3DFeature.Parametric(
                        t => t,
                        t => fLocal(t),
                        t => offset,
                        xMin, xMax, samples);
                    break;

                case EmbedPlane.XZ:
                    // XZ plane at Y = offset  → GL (x, y=offset, z=f(x))
                    Graph3DFeature.Parametric(
                        t => t,
                        t => offset,
                        t => fLocal(t),
                        xMin, xMax, samples);
                    break;

                case EmbedPlane.YZ:
                    // YZ plane at X = offset  → GL (x=offset, y=t, z=f(t))
                    Graph3DFeature.Parametric(
                        t => offset,
                        t => t,
                        t => fLocal(t),
                        xMin, xMax, samples);
                    break;
            }
        }
    }

    /// <summary>2D scalar field g(x,y).</summary>
    public readonly struct Graph2D
    {
        private readonly Func<double, double, double> g;
        public Graph2D(Func<double, double, double> g) => this.g = g;

        /// Plot g(x,y) as a 3D surface.
        public void ThreeD(
            double xMin, double xMax,
            double yMin, double yMax,
            int resolution = 120, bool wireframe = false)
        {
            var gLocal = g;
            Graph3DFeature.Surface(gLocal, xMin, xMax, yMin, yMax, resolution, wireframe);
        }
    }

    /// <summary>Convenience: y = m x + b as 2D or 3D line.</summary>
    public sealed class GraphLine
    {
        private readonly double m, b;
        public GraphLine(double m, double b) { this.m = m; this.b = b; }

        public void TwoD(double xMin, double xMax, int samples = 1000)
            => Graph2DFeature.Function(x => m * x + b, xMin, xMax, samples);

        public void ThreeD(double tMin, double tMax, EmbedPlane plane = EmbedPlane.XY, double offset = 0, int samples = 512)
        {
            switch (plane)
            {
                case EmbedPlane.XY:
                    Graph3DFeature.Parametric(
                        t => t,
                        t => m * t + b,
                        t => offset,
                        tMin, tMax, samples);
                    break;
                case EmbedPlane.XZ:
                    Graph3DFeature.Parametric(
                        t => t,
                        t => offset,
                        t => m * t + b,
                        tMin, tMax, samples);
                    break;
                case EmbedPlane.YZ:
                    Graph3DFeature.Parametric(
                        t => offset,
                        t => t,
                        t => m * t + b,
                        tMin, tMax, samples);
                    break;
            }
        }
    }

    /// <summary>Polyline from raw (x,y) samples, drawn as a 3D line on a chosen plane.</summary>
    public sealed class GraphPoints
    {
        private readonly (double x, double y)[] pts;
        public GraphPoints((double x, double y)[] pts) => this.pts = pts;

        // (Optional) Add a 2D points method later if you have a points plotter.
        public void ThreeDLine(EmbedPlane plane = EmbedPlane.XY, double offset = 0)
        {
            // Parametrize by index and linearly interpolate between sample points.
            Graph3DFeature.Parametric(
                t =>
                {
                    int i = (int)global::System.Math.Clamp(global::System.Math.Floor(t), 0, pts.Length - 1);
                    int j = global::System.Math.Min(i + 1, pts.Length - 1);
                    double a = t - i;
                    var (x0, y0) = pts[i];
                    var (x1, y1) = pts[j];
                    double x = x0 + a * (x1 - x0);
                    double y = y0 + a * (y1 - y0);

                    return plane switch
                    {
                        EmbedPlane.XY => x,
                        EmbedPlane.XZ => x,
                        EmbedPlane.YZ => offset, // X fixed
                    };
                },
                t =>
                {
                    int i = (int)global::System.Math.Clamp(global::System.Math.Floor(t), 0, pts.Length - 1);
                    int j = global::System.Math.Min(i + 1, pts.Length - 1);
                    double a = t - i;
                    var (x0, y0) = pts[i];
                    var (x1, y1) = pts[j];
                    double x = x0 + a * (x1 - x0);
                    double y = y0 + a * (y1 - y0);

                    return plane switch
                    {
                        EmbedPlane.XY => y,        // Y = f(x)
                        EmbedPlane.XZ => offset,   // Y fixed
                        EmbedPlane.YZ => x,        // Y varies with x
                    };
                },
                t =>
                {
                    int i = (int)global::System.Math.Clamp(global::System.Math.Floor(t), 0, pts.Length - 1);
                    int j = global::System.Math.Min(i + 1, pts.Length - 1);
                    double a = t - i;
                    var (x0, y0) = pts[i];
                    var (x1, y1) = pts[j];
                    double x = x0 + a * (x1 - x0);
                    double y = y0 + a * (y1 - y0);

                    return plane switch
                    {
                        EmbedPlane.XY => offset,   // Z fixed
                        EmbedPlane.XZ => y,        // Z = f(x)
                        EmbedPlane.YZ => y,        // Z = f(t) with t~x
                    };
                },
                tMin: 0,
                tMax: global::System.Math.Max(1, pts.Length - 1),
                samples: global::System.Math.Max(pts.Length, 512));
        }
    }
}
