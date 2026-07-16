// File: Fluxion.Math/Calculus/Integrals/IntegrationByParts.cs

using Fluxion.src.Numerics.Calculus.Concepts;

namespace Fluxion.src.Numerics.Calculus.Integrals
{
    /// <summary>Symbolic shell for ∫u dv = uv − ∫v du. Concrete use requires u, v already chosen.</summary>
    public static class IntegrationByParts
    {
        public static IFunction Build(IFunction u, IFunction v)
            => new LambdaFunction(x => u.Evaluate(x) * v.Evaluate(x)); // partial; the remaining ∫v du must be handled externally
    }
}
