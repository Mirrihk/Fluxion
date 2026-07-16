// File: Fluxion.Math/Calculus/Solvers/ODESolver.cs
using System;

namespace Fluxion.src.Numerics.Calculus.Solvers
{
    public static class ODESolver
    {
        /// <summary>Classic RK4 step: y_{n+1} = y_n + Φ(x_n, y_n, h).</summary>
        public static double RK4Step(Func<double, double, double> f /* dy/dx=f(x,y) */, double x, double y, double h)
        {
            double k1 = f(x, y);
            double k2 = f(x + h / 2.0, y + h * k1 / 2.0);
            double k3 = f(x + h / 2.0, y + h * k2 / 2.0);
            double k4 = f(x + h, y + h * k3);
            return y + h * (k1 + 2 * k2 + 2 * k3 + k4) / 6.0;
        }
    }
}
