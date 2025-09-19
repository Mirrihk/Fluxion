// Fluxion.Features/Graph3DFeature.cs
using System;
using Math.Functions3D;
using Rendering.SceneImpl;       // Surface3DScene
using Rendering.Visualize3D;     // SurfaceFactory, Mesh3D
using Rendering.Windowing;       // FluxWindow

namespace Features
{
    public static class Graph3DFeature
    {
        public static void Surface(Func<double, double, double> f,
                                   double xMin, double xMax,
                                   double yMin, double yMax,
                                   int resolution = 100, bool wireframe = false)
        {
            var field = new DelegateScalarField(f);
            var mesh = SurfaceFactory.BuildSurface(field, xMin, xMax, yMin, yMax, resolution);

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
            // Build a 2-point polyline
            OpenTK.Mathematics.Vector3d[] pts =
            {
                new(x1, y1, z1),
                new(x2, y2, z2)
            };
        
            // Build mesh (polyline with 2 points = one segment)
            var mesh = SurfaceFactory.BuildPolyline(t => pts[(int)t], 0, 1, 2);
        
            // Open a window to show it
            using var window = new FluxWindow("Fluxion 3D Segment", 800, 600,
                               new Surface3DScene(mesh, wireframe: true));
            window.Run();
        }
    }
}

