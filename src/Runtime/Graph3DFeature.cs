// Fluxion.Features/Graph3DFeature.cs
using System;
using Fluxion.src.Numerics.Functions3D;
using Fluxion.src.Rendering.Scene;       // Surface3DScene
using Fluxion.src.Rendering.Windowing;
using Fluxion.src.Rendering.Visualize3D;

namespace Fluxion.src.Runtime
{
    public static class Graph3DFeature
    {
        public static void Surface
        (
            Func<double, double, double> scalarFieldFunc,
            double xMin, double xMax,
            double yMin, double yMax,
            int resolution = 100,
            bool wireframe = false
        )
        {
            var field = new DelegateScalarField(scalarFieldFunc);

            // ✅ Correct call: resolution is int, then the bounds
            var mesh = SurfaceFactory.BuildSurface(field, resolution, xMin, xMax, yMin, yMax);

            using var window = new FluxWindow("Fluxion 3D", 1280, 800,
                               new Surface3DScene(mesh, wireframe));
            window.Run();
        }

        public static void Parametric(Func<double, double> x, Func<double, double> y, Func<double, double> z,
                                      double tMin, double tMax, int samples = 1000)
        {
            OpenTK.Mathematics.Vector3d r(double t) => new(x(t), y(t), z(t));
            var mesh = SurfaceFactory.BuildPolyline(r, tMin, tMax, samples);

            using var window = new FluxWindow("Fluxion 3D Curve", 1280, 800,
                               new Surface3DScene(mesh, wireframe: true));
            window.Run();
        }

        public static void Segment(float x1, float y1, float z1,
                                   float x2, float y2, float z2,
                                   float thickness = 1f)
        {
            OpenTK.Mathematics.Vector3d[] pts =
            {
                new(x1, y1, z1),
                new(x2, y2, z2)
            };

            var mesh = SurfaceFactory.BuildPolyline(t => pts[(int)t], 0, 1, 2);

            using var window = new FluxWindow("Fluxion 3D Segment", 800, 600,
                               new Surface3DScene(mesh, wireframe: true));
            window.Run();
        }
    }
}
