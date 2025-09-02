using Fluxion.Math.Functions3D;
using OpenTK.Mathematics;
using System;

namespace Fluxion.Rendering.Visualize3D
{
    public static class SurfaceFactory
    {
        /// <summary>Samples z=f(x,y) on a regular grid and builds a triangle mesh.</summary>
        public static Mesh3D BuildSurface(IScalarField f,
            double xMin, double xMax, double yMin, double yMax, int resolution)
        {
            resolution = System.Math.Max(2, resolution);

            int nx = resolution, ny = resolution;
            int vCount = nx * ny;
            var pos = new Vector3[vCount];
            var nrm = new Vector3[vCount];

            double dx = (xMax - xMin) / (nx - 1);
            double dy = (yMax - yMin) / (ny - 1);

            // Positions
            for (int j = 0; j < ny; j++)
            {
                double y = yMin + j * dy;
                for (int i = 0; i < nx; i++)
                {
                    double x = xMin + i * dx;
                    double z = f.Evaluate(x, y);
                    int idx = j * nx + i;
                    pos[idx] = new Vector3((float)x, (float)z, (float)y); // note: Y<-Z, Z<-Y (Y up)
                }
            }

            // Approx normals via central differences
            for (int j = 0; j < ny; j++)
            {
                for (int i = 0; i < nx; i++)
                {
                    int idx = j * nx + i;

                    int il = System.Math.Max(i - 1, 0), ir = System.Math.Min(i + 1, nx - 1);
                    int jb = System.Math.Max(j - 1, 0), jt = System.Math.Min(j + 1, ny - 1);

                    var pl = pos[j * nx + il];
                    var pr = pos[j * nx + ir];
                    var pb = pos[jb * nx + i];
                    var pt = pos[jt * nx + i];

                    var dxv = pr - pl;
                    var dyv = pt - pb;
                    var n = Vector3.Normalize(Vector3.Cross(dyv, dxv));
                    nrm[idx] = n;
                }
            }

            // Indices (two tris per grid quad)
            int triCount = (nx - 1) * (ny - 1) * 2;
            var idxs = new int[triCount * 3];
            int k = 0;
            for (int j = 0; j < ny - 1; j++)
            {
                for (int i = 0; i < nx - 1; i++)
                {
                    int i0 = j * nx + i;
                    int i1 = j * nx + i + 1;
                    int i2 = (j + 1) * nx + i;
                    int i3 = (j + 1) * nx + i + 1;

                    // tri1: i0,i2,i1
                    idxs[k++] = i0; idxs[k++] = i2; idxs[k++] = i1;
                    // tri2: i1,i2,i3
                    idxs[k++] = i1; idxs[k++] = i2; idxs[k++] = i3;
                }
            }

            return new Mesh3D { Positions = pos, Normals = nrm, Indices = idxs };
        }

        /// <summary>Samples a parametric curve r(t) and outputs a thin triangle strip as “tube” substitute.</summary>
        public static Mesh3D BuildPolyline(Func<double, Vector3d> r, double tMin, double tMax, int samples)
        {
            samples = System.Math.Max(2, samples);
            var pts = new Vector3[samples];
            double dt = (tMax - tMin) / (samples - 1);

            for (int i = 0; i < samples; i++)
            {
                double t = tMin + i * dt;
                var v = r(t);
                pts[i] = new Vector3((float)v.X, (float)v.Z, (float)v.Y); // keep Y up
            }

            // Build degenerate triangle list for a simple line strip (rendered as GL_LINES)
            // We’ll let the renderer treat this specially in wireframe mode.
            return new Mesh3D { Positions = pts, Normals = Array.Empty<Vector3>(), Indices = Array.Empty<int>() };
        }
    }
}
