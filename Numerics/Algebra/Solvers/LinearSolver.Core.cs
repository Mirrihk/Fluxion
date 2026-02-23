// Fluxion.Math/Algebra/Solvers/LinearSolver.Core.cs
using System;
using System.Collections.Generic;
using Fluxion.Numerics.Algebra.Concepts;

namespace Math.Algebra.Solvers
{
    /// <summary>
    /// Linear equation solvers and helpers.
    /// </summary>
    public static partial class LinearSolver
    {
        /// <summary>
        /// Solve the canonical linear form Ax + B = 0.
        /// Returns:
        ///  - Unique solution in X1,
        ///  - null if no real solution,
        ///  - NaN (in X1) to represent infinite solutions (identity).
        /// </summary>
        public static RealSolutions Solve(double A, double B)
        {
            if (System.Math.Abs(A) < 1e-12)
            {
                // A == 0 => either no solution (B != 0) or infinite solutions (B == 0).
                return B == 0 ? new RealSolutions(double.NaN) : new RealSolutions(null);
            }
            return new RealSolutions(-B / A);
        }

        /// <summary>
        /// Solve a*x + b = c*x + d with narrated steps and formatting options.
        /// Produces a step list and either a unique value, none, or infinite solutions.
        /// </summary>
        public static partial LinearSolveResult Solve(double a, double b, double c, double d, SolveFormatOptions fmt)
        {
            var steps = new List<string>();

            // Step 1: normalize to A*x + B = 0
            double A = a - c;
            double B = b - d;

            steps.Add($"Move all terms to left side: ({a} - {c})x + ({b} - {d}) = 0");
            steps.Add($"Simplify: {A}x + {B} = 0");

            if (System.Math.Abs(A) < 1e-12)
            {
                if (System.Math.Abs(B) < 1e-12)
                {
                    steps.Add("Result: Infinite solutions (identity).");
                    return LinearSolveResult.Infinite(steps);
                }
                else
                {
                    steps.Add("Result: No solution (contradiction).");
                    return LinearSolveResult.None(steps);
                }
            }

            // Step 2: Solve for x
            double value = -B / A;

            // Optional exact display as a simple fraction string (display only; not reduced)
            string? exact = null;
            if (fmt.UseFractionsInSteps)
            {
                exact = $"{-B}/{A}";
            }

            steps.Add($"Divide both sides by {A}: x = {-B}/{A}");
            steps.Add($"x ≈ {System.Math.Round(value, fmt.DecimalPlaces)}");

            return LinearSolveResult.Unique(value, exact, steps);
        }

        /// <summary>
        /// Return just the narrated steps for a*x + b = c*x + d.
        /// </summary>
        public static IReadOnlyList<string> Explain(double a, double b, double c, double d)
        {
            var result = Solve(a, b, c, d, new SolveFormatOptions());
            return result.Steps;
        }

        /// <summary>
        /// Solve many equations of the form a*x + b = c*x + d with default formatting.
        /// </summary>
        public static IEnumerable<LinearSolveResult> SolveMany(IEnumerable<(double a, double b, double c, double d)> items)
        {
            if (items is null) yield break;

            var fmt = new SolveFormatOptions();
            foreach (var (a, b, c, d) in items)
                yield return Solve(a, b, c, d, fmt);
        }
    }
}
