// Math/Algebra/Equations/Rational.cs
using Fluxion.Numerics.Abstractions;
using Fluxion.Numerics.Algebra.Concepts;

namespace Fluxion.Numerics.Algebra.Equations
{
    public sealed class Rational : IEquation, IDisplay
    {
        public RationalModel Model { get; }
        public Rational(PolynomialModel numer, PolynomialModel denom) => Model = new(numer, denom);

        public double Evaluate(double x)
        {
            var n = new Polynomial(Model.Numer.Coefficients).Evaluate(x);
            var d = new Polynomial(Model.Denom.Coefficients).Evaluate(x);
            return n / d;
        }

        public bool IsWellFormed()
            => Model.Numer.Coefficients.Length > 0 && Model.Denom.Coefficients.Length > 0;

        public string AsString(string variable = "x") => $"N({variable})/D({variable}) = 0";
    }
}
