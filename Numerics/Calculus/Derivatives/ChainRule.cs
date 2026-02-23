// File: Fluxion.Math/Calculus/Derivatives/ChainRule.cs
using System;
using Fluxion.Numerics.Calculus.Concepts;

namespace Fluxion.Numerics.Calculus.Derivatives
{
    public static class ChainRule
    {
        /// <summary>(f∘g)'(x) = f'(g(x)) * g'(x)</summary>
        public static IFunction Compose(IFunction outerPrime, IFunction inner, IFunction innerPrime)
            => new LambdaFunction(x => outerPrime.Evaluate(inner.Evaluate(x)) * innerPrime.Evaluate(x));
    }
}
