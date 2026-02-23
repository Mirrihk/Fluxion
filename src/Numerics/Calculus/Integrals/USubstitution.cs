// File: Fluxion.Math/Calculus/Integrals/USubstitution.cs
using System;
using Fluxion.src.Numerics.Calculus.Concepts;

namespace Fluxion.src.Numerics.Calculus.Integrals
{
    /// <summary>u-substitution template: integrates f(g(x))g'(x) as F(g(x)).</summary>
    public static class USubstitution
    {
        public static IFunction Integrate(IFunction F /* antiderivative of f */, IFunction g)
            => new LambdaFunction(x => F.Evaluate(g.Evaluate(x)));
    }
}
