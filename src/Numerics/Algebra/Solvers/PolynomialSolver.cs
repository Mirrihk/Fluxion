// Fluxion.Math/Algebra/Solvers/PolynomialSolver.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Fluxion.src.Numerics.Algebra.Concepts;
using Fluxion.src.Numerics.Algebra.Equations;

namespace Fluxion.src.Numerics.Algebra.Solvers
{
    /// <summary>Solves real polynomials for real roots (degree ≤ 2 for now).</summary>
    public static class PolynomialSolver
    {
        /// <summary>
        /// Solve for real roots of a polynomial given as a Concepts model.
        /// Supports degrees 0–2. Throws NotSupportedException above 2.
        /// </summary>
        public static IEnumerable<double> SolveRealRoots(PolynomialModel poly)
            => SolveFromCoeffs(poly.Coefficients);

        /// <summary>
        /// Solve for real roots of a polynomial given as an Equations.Polynomial.
        /// Returns a materialized list. Supports degrees 0–2.
        /// </summary>
        public static IReadOnlyList<double> Solve(Polynomial p)
            => SolveFromCoeffs(p.Coefficients).ToArray();

        /// <summary>
        /// Core implementation operating on ascending-order coefficients:
        /// a[0] + a[1]x + a[2]x^2 + ...
        /// </summary>
        private static IEnumerable<double> SolveFromCoeffs(double[] a)
        {
            if (a is null || a.Length == 0) yield break;

            int deg = a.Length - 1;

            switch (deg)
            {
                case 0:
                    // a0 = 0 → infinite solutions (identity), else no solution. We return empty in both cases.
                    yield break;

                case 1:
                    {
                        // a0 + a1 x = 0 → x = -a0/a1 (if a1 != 0)
                        double a1 = a[1], a0 = a[0];
                        if (System.Math.Abs(a1) < 1e-12) yield break; // degenerate/unsolvable here
                        yield return -a0 / a1;
                        yield break;
                    }

                case 2:
                    {
                        // a0 + a1 x + a2 x^2 = 0 → Quadratic with (A=a2, B=a1, C=a0)
                        double a2 = a[2], a1 = a[1], a0 = a[0];

                        // Degenerate to linear if leading term is ~0
                        if (System.Math.Abs(a2) < 1e-12)
                        {
                            if (System.Math.Abs(a1) < 1e-12) yield break;
                            yield return -a0 / a1;
                            yield break;
                        }

                        var r = QuadraticSolver.Solve(a2, a1, a0);
                        if (r.X1.HasValue) yield return r.X1.Value;
                        if (r.X2.HasValue && (!r.X1.HasValue || r.X2.Value != r.X1.Value)) yield return r.X2.Value;
                        yield break;
                    }

                default:
                    throw new NotSupportedException($"Polynomial degree {deg} not supported yet (implement a numeric root finder).");
            }
        }
    }
}
