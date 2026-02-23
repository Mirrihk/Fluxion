// Fluxion.Math/Algebra/Solvers/SystemSolver.cs
using System;
using Fluxion.Numerics.Algebra.Concepts;

namespace Fluxion.Numerics.Algebra.Solvers
{
    /// <summary>
    /// Canonical 2×2 system solver variants.
    /// </summary>
    public static class SystemSolver2x2
    {
        private const double Eps = 1e-12;

        /// <summary>
        /// Solve:
        ///   A x + B y = E
        ///   C x + D y = F
        /// Returns (x, y) when the system has a unique solution; otherwise null.
        /// </summary>
        public static (double x, double y)? Solve(System2x2Model s)
        {
            double det = s.A * s.D - s.B * s.C;
            if (System.Math.Abs(det) < Eps) return null; // singular or ill-conditioned

            double x = (s.E * s.D - s.B * s.F) / det;
            double y = (s.A * s.F - s.E * s.C) / det;
            return (x, y);
        }

        /// <summary>
        /// Try-pattern for solving
        ///   a1 x + b1 y = c1
        ///   a2 x + b2 y = c2
        /// Returns true if unique solution exists; false otherwise.
        /// </summary>
        public static bool TrySolve2x2(
            double a1, double b1, double c1,
            double a2, double b2, double c2,
            out double x, out double y)
        {
            double det = a1 * b2 - a2 * b1;
            if (System.Math.Abs(det) < Eps)
            {
                x = y = double.NaN;
                return false;
            }

            x = (c1 * b2 - c2 * b1) / det;
            y = (a1 * c2 - a2 * c1) / det;
            return true;
        }
    }

    /// <summary>
    /// Throwing convenience wrapper (kept for backward compatibility with your original API).
    /// </summary>
    public static class SystemSolver
    {
        /// <summary>
        /// Solves a 2×2 system:
        ///   a1 x + b1 y = c1
        ///   a2 x + b2 y = c2
        /// Throws if the system has no unique solution.
        /// </summary>
        /// <exception cref="InvalidOperationException">Determinant is zero (no unique solution).</exception>
        public static (double x, double y) Solve2x2(
            double a1, double b1, double c1,
            double a2, double b2, double c2)
        {
            if (!SystemSolver2x2.TrySolve2x2(a1, b1, c1, a2, b2, c2, out var x, out var y))
                throw new InvalidOperationException("System has no unique solution (determinant is zero).");

            return (x, y);
        }
    }
}
