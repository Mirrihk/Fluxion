// File: Fluxion.Math/Calculus/CalculusUtils.cs
using System;

namespace Fluxion.Math.Calculus
{
    /// <summary>Helpers for numeric guards and tolerances.</summary>
    public static class CalculusUtils
    {
        public const double DefaultTol = 1e-8;
        public static bool NearlyZero(double v, double tol = DefaultTol) => System.Math.Abs(v) <= tol;
        public static bool NearlyEqual(double a, double b, double tol = DefaultTol) => System.Math.Abs(a - b) <= tol * System.Math.Max(1.0, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));
    }
}
