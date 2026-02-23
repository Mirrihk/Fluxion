// Fluxion.Math/Algebra/Operations/Substitution.cs
using Fluxion.src.Numerics.Algebra.Concepts;
using Fluxion.src.Numerics.Abstractions;
namespace Fluxion.src.Numerics.Algebra.Operations
{
    public static class Substitution
    {
        /// Substitute x := value and return f(value).
        public static double Evaluate(IEquation eq, double value) => eq.Evaluate(value);
    }
}
