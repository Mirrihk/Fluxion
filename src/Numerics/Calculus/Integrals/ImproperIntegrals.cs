// File: Fluxion.Math/Calculus/Integrals/ImproperIntegrals.cs
using System;

namespace Fluxion.src.Numerics.Calculus.Integrals
{
    public static class ImproperIntegrals
    {
        /// <summary>Simple p-test style convergence checker for ∫_1^∞ 1/x^p dx.</summary>
        public static bool ConvergesP(double p) => p > 1.0;
    }
}