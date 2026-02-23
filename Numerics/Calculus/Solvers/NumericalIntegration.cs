// File: Fluxion.Math/Calculus/Solvers/NumericalIntegration.cs

// File: Fluxion.Math/Calculus/Solvers/NumericalIntegration.cs

// File: Fluxion.Math/Calculus/Solvers/NumericalIntegration.cs

// File: Fluxion.Math/Calculus/Solvers/NumericalIntegration.cs
using Fluxion.Numerics.Calculus.Concepts;

namespace Fluxion.Numerics.Calculus.Solvers
{
    public static class NumericalIntegration
    {
        public static double Trapezoid(IFunction f, double a, double b, int n = 200)
        {
            double h = (b - a) / n; double sum = 0.5 * (f.Evaluate(a) + f.Evaluate(b));
            for (int i = 1; i < n; i++) sum += f.Evaluate(a + i * h);
            return sum * h;
        }

        public static double Midpoint(IFunction f, double a, double b, int n = 200)
        {
            double h = (b - a) / n; double sum = 0.0;
            for (int i = 0; i < n; i++) sum += f.Evaluate(a + (i + 0.5) * h);
            return sum * h;
        }
    }
}
