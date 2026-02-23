// File: Fluxion.Math/Calculus/Derivatives/ProductQuotientRules.cs
using System;
using Fluxion.src.Numerics.Calculus.Concepts;

namespace Fluxion.src.Numerics.Calculus.Derivatives
{
    public static class ProductQuotientRules
    {
        public static IFunction Product(IFunction f, IFunction g, IFunction fPrime, IFunction gPrime)
            => new LambdaFunction(x => fPrime.Evaluate(x) * g.Evaluate(x) + f.Evaluate(x) * gPrime.Evaluate(x));

        public static IFunction Quotient(IFunction f, IFunction g, IFunction fPrime, IFunction gPrime)
            => new LambdaFunction(x =>
            {
                var gx = g.Evaluate(x);
                var num = fPrime.Evaluate(x) * gx - f.Evaluate(x) * gPrime.Evaluate(x);
                return num / (gx * gx);
            });
    }
}
