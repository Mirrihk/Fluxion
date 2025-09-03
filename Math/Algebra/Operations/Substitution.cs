// Fluxion.Math/Algebra/Operations/Substitution.cs
using Math.Algebra.Concepts;
using Math.Abstractions;
namespace Math.Algebra.Operations
{
    public static class Substitution
    {
        /// Substitute x := value and return f(value).
        public static double Evaluate(IEquation eq, double value) => eq.Evaluate(value);
    }
}
