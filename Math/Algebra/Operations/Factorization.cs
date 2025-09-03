// Fluxion.Math/Algebra/Operations/Factorization.cs
using Math.Algebra.Concepts;

namespace Fluxion.Math.Algebra.Operations
{
    public static class Factorization
    {
        /// Factor a quadratic Ax^2 + Bx + C into (ux + v)(wx + z) when possible over reals.
        public static (double u, double v, double w, double z, bool success) FactorQuadratic(QuadraticModel q)
        {
            // Real factorization exists only if discriminant >= 0 and A != 0
            double disc = q.B * q.B - 4 * q.A * q.C;
            if (q.A == 0 || disc < 0) return default;

            // If monic, quick path
            if (System.Math.Abs(q.A - 1.0) < 1e-12)
            {
                double r1 = (-q.B + System.Math.Sqrt(disc)) / (2 * q.A);
                double r2 = (-q.B - System.Math.Sqrt(disc)) / (2 * q.A);
                // (x - r1)(x - r2)
                return (1, -r1, 1, -r2, true);
            }

            // General case: (Ax^2+Bx+C) = A (x - r1)(x - r2)
            double rr1 = (-q.B + System.Math.Sqrt(disc)) / (2 * q.A);
            double rr2 = (-q.B - System.Math.Sqrt(disc)) / (2 * q.A);
            // (sqrt(A)x - sqrt(A)r1)(sqrt(A)x - sqrt(A)r2)
            double sA = System.Math.Sqrt(System.Math.Abs(q.A));
            double u = sA, w = sA, v = -sA * rr1, z = -sA * rr2;
            return (u, v, w, z, true);
        }
    }
}
