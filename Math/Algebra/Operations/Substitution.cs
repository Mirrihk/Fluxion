// Fluxion.Math/Algebra/Operations/Substitution.cs
using Fluxion.Math.Algebra.Concepts;
using Fluxion.Math.Abstractions;
namespace Fluxion.Math.Algebra.Operations
{
    public static class Substitution
    {
        /// Substitute x := value and return f(value).
        public static double Evaluate(IEquation eq, double value) => eq.Evaluate(value);
    }
}
