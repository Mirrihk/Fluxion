// Fluxion.Math/Algebra/Operations/Substitution.cs
using Fluxion.Numerics.Algebra.Concepts;
using Fluxion.Numerics.Abstractions;
namespace Fluxion.Numerics.Algebra.Operations
{
    public static class Substitution
    {
        /// Substitute x := value and return f(value).
        public static double Evaluate(IEquation eq, double value) => eq.Evaluate(value);
    }
}
