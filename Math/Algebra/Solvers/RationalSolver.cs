// Fluxion.Math/Algebra/Solvers/RationalSolver.cs
using System.Linq;
using Fluxion.Math.Algebra.Concepts;

namespace Fluxion.Math.Algebra.Solvers
{
    public static class RationalSolver
    {
        /// Solve N(x)/D(x) = 0 -> N(x)=0 with D(x)≠0.
        public static RealSolutions Solve(RationalModel model)
        {
            var roots = PolynomialSolver.SolveRealRoots(model.Numer).ToArray();
            if (roots.Length == 0) return new RealSolutions(null, null);

            // Filter out any roots that zero the denominator.
            var denom = new Equations.Polynomial(model.Denom.Coefficients);
            var valid = roots.Where(r => System.Math.Abs(denom.Evaluate(r)) > 1e-10).ToArray();

            return valid.Length switch
            {
                0 => new RealSolutions(null, null),
                1 => new RealSolutions(valid[0]),
                _ => new RealSolutions(valid[0], valid[1])
            };
        }
    }
}
