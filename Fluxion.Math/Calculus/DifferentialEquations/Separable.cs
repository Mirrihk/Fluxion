// File: Fluxion.Math/Calculus/DifferentialEquations/Separable.cs
using System;
using Fluxion.Math.Calculus.Concepts;

namespace Fluxion.Math.Calculus.DifferentialEquations
{
    public static class Separable
    {
        /// <summary>
        /// Template: returns a numerical stepper for dy/dx = g(x) h(y) using forward Euler.
        /// </summary>
        public static Func<double, double, double, double> Euler(Func<double, double> g, Func<double, double> h)
        {
            return (x, y, hstep) => y + hstep * g(x) * h(y);
        }
    }
}
