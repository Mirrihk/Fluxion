// File: Fluxion.Math/Calculus/Series/ConvergenceTests.cs
using System;

namespace Fluxion.Numerics.Calculus.Series
{
    public static class ConvergenceTests
    {
        /// <summary>Geometric series test: sum r^n converges iff |r|<1.</summary>
        public static bool Geometric(double r) => System.Math.Abs(r) < 1.0;

        /// <summary>Ratio test using limit of |a_{n+1}/a_n| as n→∞ (approx via tail sample).</summary>
        public static bool Ratio(double[] tail)
        {
            if (tail.Length < 2) return false;
            double sum = 0; int k = 0;
            for (int i = 0; i < tail.Length - 1; i++)
            {
                if (tail[i] == 0) return false;
                sum += System.Math.Abs(tail[i + 1] / tail[i]); k++;
            }
            double rho = sum / k;
            return rho < 1.0;
        }
    }
}
