// Fluxion.Math/Algebra/Equations/Polynomial.cs
using System;
using System.Linq;
using Fluxion.Math.Algebra.Concepts;

namespace Fluxion.Math.Algebra.Equations
{
    /// <summary>
    /// Represents a 1-variable polynomial with real coefficients (ascending order):
    /// coeffs[0] = constant, coeffs[1] = x, coeffs[2] = x², ...
    /// Implements IEquation/IDisplay so it can be used by generic algebra ops/solvers.
    /// </summary>
    public sealed class Polynomial : IEquation, IDisplay
    {
        /// <summary>Raw coefficient array (lowest degree first).</summary>
        public double[] Coefficients { get; }

        /// <summary>Backed model for consistency with the Concepts layer.</summary>
        public PolynomialModel Model { get; }

        /// <summary>Create a polynomial with given coefficients (ascending order).</summary>
        public Polynomial(params double[] coeffs)
        {
            if (coeffs is null || coeffs.Length == 0)
                throw new ArgumentException("Polynomial must have at least one coefficient.", nameof(coeffs));

            Coefficients = coeffs;
            Model = new PolynomialModel(coeffs);
        }

        /// <summary>Polynomial degree (highest index).</summary>
        public int Degree => Coefficients.Length - 1;

        /// <summary>Evaluate P(x) using Horner’s method.</summary>
        public double Evaluate(double x)
        {
            double acc = 0;
            for (int i = Coefficients.Length - 1; i >= 0; --i)
                acc = acc * x + Coefficients[i];
            return acc;
        }

        /// <summary>Basic well-formed check (has at least one coefficient).</summary>
        public bool IsWellFormed() => Coefficients.Length > 0;

        /// <summary>Pretty print P(x).</summary>
        public string AsString(string variable = "x")
        {
            // e.g., "P(x) = c0 + c1x + c2x^2"
            var terms = Coefficients.Select((c, i) => i switch
            {
                0 => $"{c}",
                1 => $"{c}{variable}",
                _ => $"{c}{variable}^{i}"
            });
            return $"P({variable}) = " + string.Join(" + ", terms);
        }

        public override string ToString() => AsString("x");
    }
}
