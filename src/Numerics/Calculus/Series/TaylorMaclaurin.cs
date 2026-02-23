// File: Fluxion.Math/Calculus/Series/TaylorMaclaurin.cs
using System;

namespace Fluxion.src.Numerics.Calculus.Series
{
    public static class TaylorMaclaurin
    {
        /// <summary>Remainder (Lagrange) bound placeholder for smooth f: |R_{n+1}| ≤ M |x-a|^{n+1}/(n+1)!</summary>
        public static double LagrangeRemainderBound(double M, double dx, int n)
        {
            // (n+1)! might overflow quickly; use simple loop
            double fact = 1.0; for (int k = 2; k <= n + 1; k++) fact *= k;
            return M * System.Math.Pow(System.Math.Abs(dx), n + 1) / fact;
        }
    }
}
