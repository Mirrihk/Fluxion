// File: Fluxion.Math/Calculus/Limits/LHospitalsRule.cs
using System;
using Fluxion.Math.Calculus.Concepts;

namespace Fluxion.Math.Calculus.Limits
{
    /// <summary>L'Hôpital's rule evaluator given f,g and their derivatives.</summary>
    public static class LHospitalsRule
    {
        /// <summary>
        /// Applies one step of L'Hôpital when f(a)=g(a)=0 or |f|,|g|->∞ near a. Caller must supply derivatives.
        /// </summary>
        public static double ApplyOnce(IFunction f, IFunction g, IFunction fPrime, IFunction gPrime, double a)
        {
            var denom = gPrime.Evaluate(a);
            if (CalculusUtils.NearlyZero(denom)) return double.NaN;
            return fPrime.Evaluate(a) / denom;
        }
    }
}
