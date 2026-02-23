// Fluxion.Math/Algebra/Solvers/LogarithmicSolver.cs
using System;
using Fluxion.Numerics.Algebra.Concepts;
using static System.Math;

namespace Fluxion.Numerics.Algebra.Solvers
{
    public static class LogarithmicSolver
    {
        /// Solve A log_base(x) + C = 0 => log_base(x) = -C/A => x = base^{-C/A}, with x>0, base>0, base≠1
        public static RealSolutions Solve(double A, double @base, double C)
        {
            if (Abs(A) < 1e-12 || @base <= 0 || Abs(@base - 1.0) < 1e-12) return new RealSolutions(null);
            double pow = -C / A;
            double x = Pow(@base, pow);
            if (x <= 0 || double.IsNaN(x) || double.IsInfinity(x)) return new RealSolutions(null);
            return new RealSolutions(x);
        }
    }
}
