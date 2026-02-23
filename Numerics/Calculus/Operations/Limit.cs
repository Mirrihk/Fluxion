// File: Fluxion.Math/Calculus/Operations/Limit.cs
using Fluxion.Numerics.Calculus.Concepts;
using Fluxion.Numerics.Calculus.Limits;

namespace Fluxion.Numerics.Calculus.Operations
{
    public static class Limit
    {
        public static double EvaluateTwoSided(IFunction f, double a)
            => LimitsBasics.ProbeTwoSided(f, a);
    }
}
