// File: Fluxion.Math/Calculus/Limits/LimitsBasics.cs
using System;
using Fluxion.Numerics.Calculus;
using Fluxion.Numerics.Calculus.Concepts;

namespace Fluxion.Numerics.Calculus.Limits
{
    /// <summary>Basic tools to numerically probe limits; formal checks live in AlgebraicLimits.</summary>
    public static class LimitsBasics
    {
        /// <summary>
        /// Probes lim_{x->a} f(x) numerically from left/right using shrinking steps. Use for intuition or fallback.
        /// </summary>
        public static double ProbeTwoSided(IFunction f, double a, double h0 = 1e-1, int steps = 8)
        {
            double left = 0, right = 0;
            double h = h0;
            for (int i = 0; i < steps; i++) { left = f.Evaluate(a - h); right = f.Evaluate(a + h); h *= 0.5; }
            if (CalculusUtils.NearlyEqual(left, right)) return (left + right) / 2.0;
            return double.NaN; // indicates mismatch; try algebraic techniques
        }
    }
}
