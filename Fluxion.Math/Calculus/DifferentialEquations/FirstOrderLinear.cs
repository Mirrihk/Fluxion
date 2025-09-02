// File: Fluxion.Math/Calculus/DifferentialEquations/FirstOrderLinear.cs
using System;

namespace Fluxion.Math.Calculus.DifferentialEquations
{
    public static class FirstOrderLinear
    {
        /// <summary>Integrating factor μ(x)=exp(∫P dx). Caller supplies μ function directly.</summary>
        public static Func<double, double, double, double> Euler(Func<double, double> P, Func<double, double> Q)
        {
            return (x, y, h) => y + h * (-P(x) * y + Q(x));
        }
    }
}
