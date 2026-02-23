// Fluxion.Math/Algebra/Operations/Simplify.cs
using System;
using Fluxion.src.Numerics.Algebra.Concepts;

namespace Fluxion.src.Numerics.Algebra.Operations
{
    public static class Simplify
    {
        /// Reduce leading/trailing zeros in polynomial coefficients.
        public static PolynomialModel TrimPolynomial(PolynomialModel poly)
        {
            var c = poly.Coefficients;
            if (c.Length == 0) return new(Array.Empty<double>());
            int last = c.Length - 1;
            while (last > 0 && System.Math.Abs(c[last]) < 1e-12) last--;
            var trimmed = new double[last + 1];
            Array.Copy(c, trimmed, trimmed.Length);
            return new(trimmed);
        }
    }
}
