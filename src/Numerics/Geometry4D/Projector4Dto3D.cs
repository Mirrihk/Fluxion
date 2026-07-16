// File: Fluxion.Rendering/Visualize4D/Projector4Dto3D.cs
using System;
using System.Numerics;

namespace Fluxion.src.Numerics.Geometry4D
{
    public enum Projection4DMode { Orthographic, Perspective }

    /// <summary>
    /// 4D→3D projection helpers (then your existing 3D→2D camera handles the rest).
    /// </summary>
    public static class Projector4Dto3D
    {
        /// <summary>
        /// Orthographic: drop W but allow linear mix via wBias for a slight "tilt" effect.
        /// </summary>
        public static Vector3[] Orthographic(Vector4[] v4, float wBias = 0f)
        {
            var v3 = new Vector3[v4.Length];
            for (int i = 0; i < v4.Length; i++)
            {
                var v = v4[i];
                v3[i] = new Vector3(v.X, v.Y, v.Z) + new Vector3(0, 0, wBias * v.W);
            }
            return v3;
        }

        /// <summary>
        /// Perspective in W: camera at +d on W-axis.
        /// scale = d / (d - w). Use d &gt; max|w| to avoid singularities.
        /// </summary>
        public static Vector3[] Perspective(Vector4[] v4, float d = 3f)
        {
            var v3 = new Vector3[v4.Length];
            for (int i = 0; i < v4.Length; i++)
            {
                var v = v4[i];
                float denom = d - v.W;
                float s = denom != 0f ? d / denom : 1e6f; // guard singularity
                v3[i] = new Vector3(v.X * s, v.Y * s, v.Z * s);
            }
            return v3;
        }
    }
}
