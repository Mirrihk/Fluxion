// File: Fluxion.Math/Geometry4D/TesseractModel.cs
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Fluxion.src.Numerics.Geometry4D
{
    /// <summary>
    /// Static builder for a unit tesseract (4D hypercube) centered at origin.
    /// Vertices at (±1,±1,±1,±1). Edges connect vertices differing in exactly one coordinate.
    /// </summary>
    public static class TesseractModel
    {
        public static Vector4[] BuildVertices16(float halfExtent = 1f)
        {
            // TODO: expose scale/offset if needed later
            var v = new Vector4[16];
            int i = 0;
            float h = halfExtent;
            // All 16 sign combinations:
            for (int sx = -1; sx <= 1; sx += 2)
            for (int sy = -1; sy <= 1; sy += 2)
            for (int sz = -1; sz <= 1; sz += 2)
            for (int sw = -1; sw <= 1; sw += 2)
            {
                v[i++] = new Vector4(h * sx, h * sy, h * sz, h * sw);
            }
            return v;
        }

        /// <summary>
        /// Returns the 32 edges as index pairs into the provided vertex array (length 16).
        /// </summary>
        public static (int a, int b)[] BuildEdges32()
        {
            // Vertex indexing uses the same nested-loop order as BuildVertices16.
            // Two vertices are connected if their Hamming distance (sign flips) is exactly 1.
            var edges = new List<(int, int)>(32);
            var v = BuildVertices16(); // only for indexing parity; not used numerically

            for (int i = 0; i < v.Length; i++)
            for (int j = i + 1; j < v.Length; j++)
            {
                int diff = HammingDistanceBySign(v[i], v[j]);
                if (diff == 1) edges.Add((i, j));
            }
            return edges.ToArray();

            static int HammingDistanceBySign(in Vector4 a, in Vector4 b)
          {
                int d = 0;
                if (MathF.Sign(a.X) != MathF.Sign(b.X)) d++;
                if (MathF.Sign(a.Y) != MathF.Sign(b.Y)) d++;
                if (MathF.Sign(a.Z) != MathF.Sign(b.Z)) d++;
                if (MathF.Sign(a.W) != MathF.Sign(b.W)) d++;
                return d;
            }
        }
    }
}
