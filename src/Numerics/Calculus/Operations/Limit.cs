// File: Fluxion.Math/Calculus/Operations/Limit.cs
using Fluxion.src.Numerics.Calculus.Concepts;
using Fluxion.src.Numerics.Calculus.Limits;

namespace Fluxion.src.Numerics.Calculus.Operations
{
    public static class Limit
    {
        public static double EvaluateTwoSided(IFunction f, double a)
            => LimitsBasics.ProbeTwoSided(f, a);
    }
}
