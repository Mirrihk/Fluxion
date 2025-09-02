// File: Fluxion.Math/Calculus/Solvers/RootFinding.cs
using System;
using Fluxion.Math.Calculus.Concepts;

namespace Fluxion.Math.Calculus.Solvers
{
    public static class RootFinding
    {
        public static double Bisection(IFunction f, double a, double b, double tol = 1e-8, int maxIter = 100)
        {
            double fa = f.Evaluate(a), fb = f.Evaluate(b);
            if (fa * fb > 0) throw new ArgumentException("Bisection requires opposite signs at endpoints.");
            for (int i = 0; i < maxIter; i++)
            {
                double c = 0.5 * (a + b), fc = f.Evaluate(c);
                if (System.Math.Abs(fc) < tol || System.Math.Abs(b - a) < tol) return c;
                if (fa * fc < 0) { b = c; fb = fc; } else { a = c; fa = fc; }
            }
            return 0.5 * (a + b);
        }

        public static double Newton(IFunction f, IFunction fPrime, double x0, double tol = 1e-8, int maxIter = 50)
        {
            double x = x0;
            for (int i = 0; i < maxIter; i++)
            {
                double fx = f.Evaluate(x), d = fPrime.Evaluate(x);
                if (CalculusUtils.NearlyZero(d)) break;
                double x1 = x - fx / d;
                if (System.Math.Abs(x1 - x) < tol) return x1;
                x = x1;
            }
            return x;
        }
    }
}
