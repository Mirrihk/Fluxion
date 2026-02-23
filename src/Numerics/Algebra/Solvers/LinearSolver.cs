// Fluxion.Math/Algebra/Solvers/LinearSolver.cs
using System;
using System.Collections.Generic;
using Fluxion.src.Numerics.Algebra.Equations;
using Fluxion.src.Numerics.Algebra.Concepts;

namespace Math.Algebra.Solvers
{
    /// <summary>
    /// Public API surface for the linear equation solver.
    /// Implementation lives in LinearSolver.Core.cs via partial class.
    /// </summary>
    public static partial class LinearSolver
    {
        private const double Eps = 1e-12;

        /// <summary>
        /// Solve a normalized linear expression A*x + B = 0.
        /// Returns NaN if |A| &lt; eps (no unique solution).
        /// </summary>
        public static double Solve(LinearEquation eq, double eps = Eps)
        {
            if (global::System.Math.Abs(eq.A) < eps) return double.NaN;
            return -eq.B / eq.A;
        }

        public enum SolutionKind { Unique, Infinite, None }

        public readonly struct LinearSolveResult
        {
            public SolutionKind Kind { get; }
            public double? Value { get; }                // null if None/Infinite
            public string? Exact { get; }                // e.g., "3/4"
            public IReadOnlyList<string> Steps { get; }  // narration

            public LinearSolveResult(
                SolutionKind kind,
                double? value,
                string? exact,
                IReadOnlyList<string> steps)
            {
                Kind = kind;
                Value = value;
                Exact = exact;
                Steps = steps ?? Array.Empty<string>();
            }

            // Convenience factories
            public static LinearSolveResult Unique(double value, string? exact, IReadOnlyList<string> steps)
                => new(SolutionKind.Unique, value, exact, steps);

            public static LinearSolveResult Infinite(IReadOnlyList<string> steps)
                => new(SolutionKind.Infinite, null, null, steps);

            public static LinearSolveResult None(IReadOnlyList<string> steps)
                => new(SolutionKind.None, null, null, steps);
        }

        /// <summary>
        /// Solve a*x + b = c*x + d using default formatting options.
        /// </summary>
        public static LinearSolveResult Solve(double a, double b, double c, double d) =>
            Solve(a, b, c, d, new SolveFormatOptions());

        /// <summary>
        /// Try-pattern: returns true only for a unique solution.
        /// </summary>
        public static bool TrySolve(double a, double b, double c, double d, out LinearSolveResult result)
        {
            result = Solve(a, b, c, d);
            return result.Kind == SolutionKind.Unique;
        }

        /// <summary>
        /// Formatting options for step text and numeric rounding.
        /// </summary>
        public sealed class SolveFormatOptions
        {
            public int DecimalPlaces { get; init; } = 4;
            public bool UseFractionsInSteps { get; init; } = true;
        }

        // NOTE: The actual implementation of this overload is in LinearSolver.Core.cs.
        public static partial LinearSolveResult Solve(double a, double b, double c, double d, SolveFormatOptions fmt);

        // Keep Explain/SolveMany in Core to avoid signature duplication here.
    }
}
