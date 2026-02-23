// File: Fluxion.Math/Calculus/Operations/Integrate.cs
using System;
using Fluxion.Numerics.Calculus.Concepts;

namespace Fluxion.Numerics.Calculus.Operations
{
    public static class Integrate
    {
        /// <summary>Composite Simpson's rule for numeric ∫_a^b f(x) dx (n even, default 200).</summary>
        public static double Simpson(IFunction f, double a, double b, int n = 200)
        {
            if (n % 2 == 1) n++;
            double h = (b - a) / n; double sum = f.Evaluate(a) + f.Evaluate(b);
            for (int i = 1; i < n; i++)
            {
                double x = a + i * h;
                sum += (i % 2 == 0 ? 2 : 4) * f.Evaluate(x);
            }
            return sum * h / 3.0;
        }
    }
}
