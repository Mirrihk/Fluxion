// File: Fluxion.Math/Calculus/Limits/AlgebraicLimits.cs
using System;
using Fluxion.Numerics.Calculus.Concepts;

namespace Fluxion.Numerics.Calculus.Limits
{
    /// <summary>Placeholder for symbolic simplifications (factor/cancel, rationalize, squeeze).</summary>
    public static class AlgebraicLimits
    {
        /// <summary>
        /// Attempts to evaluate a removable discontinuity by probing symmetrically and returning the average if stable.
        /// </summary>
        public static double Removable(IFunction f, double a)
        {
            var L = LimitsBasics.ProbeTwoSided(f, a);
            return L;
        }
    }
}
