// File: Fluxion.Math/Calculus/Series/PowerSeries.cs
using System;
using Fluxion.Math.Calculus.Concepts;

namespace Fluxion.Math.Calculus.Series
{
    public static class PowerSeries
    {
        /// <summary>Evaluates Σ c_n (x-a)^n up to N terms.</summary>
        public static double Evaluate(SeriesModel s, double x, int terms)
        {
            double sum = 0, dx = x - s.Center, p = 1;
            for (int n = 0; n < terms; n++) { if (n > 0) p *= dx; sum += s.Coeff(n) * p; }
            return sum;
        }
    }
}
