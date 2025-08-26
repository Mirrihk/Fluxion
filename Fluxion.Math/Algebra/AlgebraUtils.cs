// Fluxion.Math/Algebra/AlgebraUtils.cs
using System;

namespace Fluxion.Math.Algebra
{
    /// <summary>
    /// Common helper methods for algebraic operations.
    /// </summary>
    public static class AlgebraUtils
    {
        /// <summary>
        /// Computes the discriminant of a quadratic equation ax² + bx + c.
        /// </summary>
        public static double Discriminant(double a, double b, double c)
            => (b * b) - (4 * a * c);

        /// <summary>
        /// Greatest common divisor (Euclidean algorithm).
        /// </summary>
        public static int Gcd(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return System.Math.Abs(a);
        }

        /// <summary>
        /// Least common multiple of two integers.
        /// </summary>
        public static int Lcm(int a, int b)
            => System.Math.Abs(a * b) / Gcd(a, b);

        /// <summary>
        /// Checks if two doubles are nearly equal within epsilon tolerance.
        /// </summary>
        public static bool NearlyEqual(double a, double b, double eps = 1e-9)
        {
            if (a.Equals(b)) return true;

            double diff = System.Math.Abs(a - b);
            double norm = System.Math.Min((System.Math.Abs(a) + System.Math.Abs(b)), double.MaxValue);

            return diff < System.Math.Max(eps, eps * norm);
        }

        /// <summary>
        /// Checks if a number is near zero.
        /// </summary>
        public static bool NearZero(double v, double eps = 1e-12)
            => System.Math.Abs(v) < eps;

        /// <summary>
        /// Computes the real roots of a quadratic using a numerically stable formula.
        /// Returns nulls if the discriminant is negative.
        /// </summary>
        public static (double? x1, double? x2) QuadraticFormula(double A, double B, double C)
        {
            double disc = B * B - 4 * A * C;
            if (disc < 0) return (null, null);

            double sqrtD = System.Math.Sqrt(disc);
            double q = -0.5 * (B + System.Math.CopySign(sqrtD, B));

            double x1 = q / A;
            double x2 = C / q;

            return (x1, x2);
        }
    }
}
