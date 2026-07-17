// Fluxion.Math/Algebra/Operations/Solve.cs
using Fluxion.src;
using System;
using Fluxion.src.Numerics.Algebra.Concepts;
using Fluxion.src.Numerics.Algebra.Solvers;

namespace Fluxion.src.Numerics.Algebra.Operations
{
    public static class Solve
    {
        public static RealSolutions Linear(LinearModel m) => LinearSolver.Solve(m.A, m.B);
        public static RealSolutions Quadratic(QuadraticModel m) => QuadraticSolver.Solve(m.A, m.B, m.C);
        public static RealSolutions Exponential(ExponentialModel m) => ExponentialSolver.Solve(m.A, m.B, m.C);
        public static RealSolutions Logarithmic(LogarithmicModel m) => LogarithmicSolver.Solve(m.A, m.Base, m.C);
        public static RealSolutions Rational(RationalModel m) => RationalSolver.Solve(m);

        public static (double x, double y)? System2x2(System2x2Model m) => SystemSolver2x2.Solve(m);
    }
}
