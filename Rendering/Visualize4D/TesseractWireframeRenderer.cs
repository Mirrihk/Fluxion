// Fluxion.Rendering/Visualize4D/TesseractWireframeRenderer.cs
using System;
using System.Collections.Generic;
using System.Numerics;                 // System.Numerics.Vector3
using OpenTK.Mathematics;              // Vector3d
using Fluxion.Rendering.Scene;             // Surface3DScene
using Fluxion.Rendering.Windowing;
using Fluxion.Rendering.Visualize3D;

namespace Fluxion.Rendering.Visualize4D
{
    public sealed class TesseractWireframeRenderer
    {
        public float EdgeThickness { get; set; } = 1f;
        public bool FadeByDepth { get; set; } = true;

        public void Draw(System.Numerics.Vector3[] v3, (int a, int b)[] edges)
        {
            // 1) Flatten edges into a point list (p0, p1, p0', p1', ...)
            var pts = new List<Vector3d>(edges.Length * 2);
            foreach (var (a, b) in edges)
            {
                var p0 = v3[a];
                var p1 = v3[b];
                pts.Add(new Vector3d(p0.X, p0.Y, p0.Z));
                pts.Add(new Vector3d(p1.X, p1.Y, p1.Z));
            }

            // 2) Use your existing BuildPolyline(Func<double, Vector3d>, ...)
            //    This draws a single continuous polyline (smoke test).
            Vector3d SamplePoint(double t)
            {
                int i = (int)System.Math.Round(System.Math.Clamp(t, 0, pts.Count - 1));
                return pts[i];
            }

            var mesh = SurfaceFactory.BuildPolyline(SamplePoint, 0, pts.Count - 1, pts.Count);

            using var window = new FluxWindow("Fluxion 4D Tesseract (smoke test)", 1280, 800,
                               new Surface3DScene(mesh, wireframe: true));
            window.Run();
        }
    }
}

