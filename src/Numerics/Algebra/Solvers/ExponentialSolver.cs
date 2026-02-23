// Fluxion.Math/Algebra/Solvers/ExponentialSolver.cs
using System;
using Fluxion.src.Numerics.Algebra.Concepts;
using static System.Math;

namespace Fluxion.src.Numerics.Algebra.Solvers
{
    public static class ExponentialSolver
    {
        /// Solve A e^{B x} + C = 0  =>  e^{Bx} = -C/A  =>  x = ln(-C/A)/B
        public static RealSolutions Solve(double A, double B, double C)
        {
            if (Abs(A) < 1e-12 || Abs(B) < 1e-12) return new RealSolutions(null);
            double rhs = -C / A;
            if (rhs <= 0) return new RealSolutions(null); // no real solution
            return new RealSolutions(Log(rhs) / B);
        }
    }
}
