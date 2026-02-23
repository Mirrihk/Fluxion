//C:/Users/sebas/source/repos/Fluxion/Rendering/Visualize3D/SurfaceFactory.cs
using Fluxion.Numerics.Functions3D;
using OpenTK.Mathematics;
using System;

namespace Fluxion.Rendering.Visualize3D
{
    public static class SurfaceFactory
    {
        public static Mesh3D BuildSurface
        (   IScalarField scalarField,int gridResolution,
            double xMin, double xMax, 
            double yMin, double yMax
        )
        {
            gridResolution = System.Math.Max(2, gridResolution);

            int xSamples = gridResolution, ySamples = gridResolution;
            int vertexCount = xSamples * ySamples;
            var positions = new Vector3[vertexCount];
            var normals = new Vector3[vertexCount];

            double stepX = (xMax - xMin) / (xSamples - 1);
            double stepY = (yMax - yMin) / (ySamples - 1);

            for (int Positions = 0; Positions < ySamples; Positions++)
            {
                double y = yMin + Positions * stepY;
                for (int i = 0; i < xSamples; i++)
                {
                    double x = xMin + i * stepX;
                    double z = scalarField.Evaluate(x, y);
                    int idx = Positions * xSamples + i;
                    positions[idx] = new Vector3((float)x, (float)z, (float)y); // note: Y<-Z, Z<-Y (Y up)
                }
            }

            for (int j = 0; j < ySamples; j++)
            {
                for (int i = 0; i < xSamples; i++)
                {
                    int vertexIndex = j * xSamples + i;

                    int leftIndex = System.Math.Max(i - 1, 0), rightIndex = System.Math.Min(i + 1, xSamples - 1);
                    int bottomIndex = System.Math.Max(j - 1, 0), topIndex = System.Math.Min(j + 1, ySamples - 1);

                    var leftPos = positions[j * xSamples + leftIndex];
                    var rightPos = positions[j * xSamples + rightIndex];
                    var bottomPos = positions[bottomIndex * xSamples + i];
                    var topPos = positions[topIndex * xSamples + i];

                    var xDir = rightPos - leftPos;
                    var yDir = topPos - bottomPos;
                    var normal = Vector3.Normalize(Vector3.Cross(yDir, xDir));
                    normals[vertexIndex] = normal;
                }
            }

            int triangleCount = (xSamples - 1) * (ySamples - 1) * 2;
            var indices = new int[triangleCount * 3];
            int indexCursor = 0;
            for (int j = 0; j < ySamples - 1; j++)
            {
                for (int i = 0; i < xSamples - 1; i++)
                {
                    int v00 = j * xSamples + i;
                    int v10 = j * xSamples + i + 1;
                    int v01 = (j + 1) * xSamples + i;
                    int v11 = (j + 1) * xSamples + i + 1;

                    indices[indexCursor++] = v00; indices[indexCursor++] = v01; 
                    indices[indexCursor++] = v10; indices[indexCursor++] = v10; 
                    indices[indexCursor++] = v01; indices[indexCursor++] = v11;
                }
            }

            return new Mesh3D { Positions = positions, Normals = normals, Indices = indices };
        }

        public static Mesh3D BuildPolyline
        (
            Func<double, Vector3d> curveFunction, double tMin, double tMax, int samplesCount
        )
        {
            samplesCount = System.Math.Max(2, samplesCount);
            var sampledPoints = new Vector3[samplesCount];
            double tStep = (tMax - tMin) / (samplesCount - 1);

            for (int i = 0; i < samplesCount; i++)
            {
                double t = tMin + i * tStep;
                var samplePoint = curveFunction(t);
                sampledPoints[i] = new Vector3((float)samplePoint.X, (float)samplePoint.Z, (float)samplePoint.Y); // keep Y up
            }

            // Build degenerate triangle list for a simple line strip (rendered as GL_LINES)
            return new Mesh3D { Positions = sampledPoints, Normals = Array.Empty<Vector3>(), Indices = Array.Empty<int>() };
        }
    }
}
