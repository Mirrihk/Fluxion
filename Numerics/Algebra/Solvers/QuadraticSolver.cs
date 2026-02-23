// Fluxion.Math/Algebra/Solvers/QuadraticSolver.cs
using System;
using Fluxion.Numerics.Algebra.Concepts;
using Math.Algebra.Solvers;
using static System.Math;

namespace Fluxion.Numerics.Algebra.Solvers
{
    /// <summary>
    /// Provides methods for solving quadratic equations of the form A x² + B x + C = 0.
    /// </summary>
    public static class QuadraticSolver
    {
        /// <summary>
        /// Returns real roots only using a numerically-stable formula.
        /// If A is ~0, defers to the linear solver on B x + C = 0.
        /// If discriminant &lt; 0, returns (null, null).
        /// </summary>
        public static RealSolutions Solve(double A, double B, double C)
        {
            if (Abs(A) < 1e-12) return LinearSolver.Solve(B, C);

            double disc = B * B - 4 * A * C;
            if (disc < 0) return new RealSolutions(null, null);

            // Stable quadratic formula (minimizes catastrophic cancellation)
            double sqrtD = Sqrt(disc);
            double q = -0.5 * (B + CopySign(sqrtD, B));
            double x1 = q / A;
            double x2 = C / q;
            return new RealSolutions(x1, x2);
        }

        /// <summary>
        /// Quadratic-only solver (A must not be 0).
        /// Returns a pair (x1, x2); if no real roots, both are double.NaN.
        /// Uses a numerically-stable formula.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when A == 0.</exception>
        public static (double x1, double x2) SolvePair(double A, double B, double C)
        {
            if (A == 0)
                throw new ArgumentException("Coefficient 'A' must not be zero for a quadratic equation.", nameof(A));

            double disc = B * B - 4 * A * C;
            if (disc < 0) return (double.NaN, double.NaN);

            double sqrtD = Sqrt(disc);
            double q = -0.5 * (B + CopySign(sqrtD, B));
            double x1 = q / A;
            double x2 = C / q;
            return (x1, x2);
        }
    }
}
