// File: Fluxion.Math/Calculus/Operations/Differentiate.cs

// File: Fluxion.Math/Calculus/Operations/Differentiate.cs

// File: Fluxion.Math/Calculus/Operations/Differentiate.cs

// File: Fluxion.Math/Calculus/Operations/Differentiate.cs
using Fluxion.Numerics.Calculus.Concepts;

namespace Fluxion.Numerics.Calculus.Operations
{
    public static class Differentiate
    {
        /// <summary>Numerical central difference derivative for fallback.</summary>
        public static IFunction Numeric(IFunction f, double h = 1e-5)
            => new LambdaFunction(x => (f.Evaluate(x + h) - f.Evaluate(x - h)) / (2 * h));
    }
}
