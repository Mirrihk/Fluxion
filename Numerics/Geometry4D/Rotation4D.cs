// ===============================
// File: Fluxion.Math/Geometry4D/Rotation4D.cs
// ===============================
using System;
using System.Numerics;

namespace Fluxion.Numerics.Geometry4D
{
    /// <summary>
    /// 4D plane rotations. Each method returns a Matrix4x4 rotating in the named plane.
    /// Convention: (X,Y,Z,W) basis; right-handed sin/cos like 3D planes.
    /// </summary>
    public static class Rotation4D
    {
        public static Matrix4x4 RotationXY(float theta) => Plane(0, 1, theta);
        public static Matrix4x4 RotationXZ(float theta) => Plane(0, 2, theta);
        public static Matrix4x4 RotationXW(float theta) => Plane(0, 3, theta);
        public static Matrix4x4 RotationYZ(float theta) => Plane(1, 2, theta);
        public static Matrix4x4 RotationYW(float theta) => Plane(1, 3, theta);
        public static Matrix4x4 RotationZW(float theta) => Plane(2, 3, theta);

        /// <summary>
        /// Compose rotations in a fixed order (non-commutative!). Adjust order once and keep it consistent.
        /// </summary>
        public static Matrix4x4 Compose(
            float xy, float xz, float xw, float yz, float yw, float zw)
        {
            // Order: ZW · YW · XW · YZ · XZ · XY  (right-multiply column vectors)
            var r = Matrix4x4.Identity;
            r = Matrix4x4.Multiply(r, RotationXY(xy));
            r = Matrix4x4.Multiply(r, RotationXZ(xz));
            r = Matrix4x4.Multiply(r, RotationYZ(yz));
            r = Matrix4x4.Multiply(r, RotationXW(xw));
            r = Matrix4x4.Multiply(r, RotationYW(yw));
            r = Matrix4x4.Multiply(r, RotationZW(zw));
            return r;
        }

        /// <summary>
        /// Create an identity 4x4 then apply a 2x2 rotation block on axes (i,j).
        /// Indices: 0=X, 1=Y, 2=Z, 3=W
        /// </summary>
        private static Matrix4x4 Plane(int i, int j, float theta)
        {
            float c = MathF.Cos(theta);
            float s = MathF.Sin(theta);

            // Start as identity in row-major (Matrix4x4 is row-major fields)
            var m = Matrix4x4.Identity;

            // Overwrite the 2x2 block (i,j)
            Set(m, i, i, c);  Set(m, i, j, -s);
            Set(m, j, i, s);  Set(m, j, j,  c);
            return m;

            static void Set(Matrix4x4 mat, int r, int cIdx, float val)
            {
                // Matrix4x4 fields map: M11..M44; helper to reduce boilerplate
                switch ((r, cIdx))
                {
                    case (0,0): mat.M11 = val; break; case (0,1): mat.M12 = val; break;
                    case (0,2): mat.M13 = val; break; case (0,3): mat.M14 = val; break;
                    case (1,0): mat.M21 = val; break; case (1,1): mat.M22 = val; break;
                    case (1,2): mat.M23 = val; break; case (1,3): mat.M24 = val; break;
                    case (2,0): mat.M31 = val; break; case (2,1): mat.M32 = val; break;
                    case (2,2): mat.M33 = val; break; case (2,3): mat.M34 = val; break;
                    case (3,0): mat.M41 = val; break; case (3,1): mat.M42 = val; break;
                    case (3,2): mat.M43 = val; break; case (3,3): mat.M44 = val; break;
                }
            }
        }

        /// <summary>Apply a 4x4 matrix to a 4D vector.</summary>
        public static Vector4 Transform(in Matrix4x4 m, in Vector4 v)
        {
            return new Vector4(
                m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z + m.M14 * v.W,
                m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z + m.M24 * v.W,
                m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z + m.M34 * v.W,
                m.M41 * v.X + m.M42 * v.Y + m.M43 * v.Z + m.M44 * v.W
            );
        }
    }
}