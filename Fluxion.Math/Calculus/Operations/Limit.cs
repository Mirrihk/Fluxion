// File: Fluxion.Math/Calculus/Operations/Limit.cs
using Fluxion.Math.Calculus.Concepts;
using Fluxion.Math.Calculus.Limits;

namespace Fluxion.Math.Calculus.Operations
{
    public static class Limit
    {
        public static double EvaluateTwoSided(IFunction f, double a)
            => LimitsBasics.ProbeTwoSided(f, a);
    }
}
