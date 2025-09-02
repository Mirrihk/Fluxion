// Fluxion.Math/Algebra/Concepts/AlgebraSeeds.cs
using Fluxion.Math.Algebra;

namespace Fluxion.Math.Algebra.Concepts
{
    public static class AlgebraSeeds
    {
        public static IReadOnlyList<AlgebraTopic> Basics => new[]
        {
            new AlgebraTopic("Linear Equations", new[]
            {
                new FormulaItem("Slope-Intercept", "y = m x + b", "m = slope, b = y-intercept"),
                new FormulaItem("Point-Slope", "y - y₁ = m(x - x₁)")
            }),
            new AlgebraTopic("Quadratic", new[]
            {
                new FormulaItem("Standard Form", "y = ax² + bx + c"),
                new FormulaItem("Vertex", "x_v = -b/(2a),  y_v = c - b²/(4a)"),
                new FormulaItem("Roots (Quadratic Formula)", "x = (-b ± √(b² - 4ac)) / (2a)")
            }),
            new AlgebraTopic("Exponent Rules", new[]
            {
                new FormulaItem("Product", "a^m * a^n = a^(m+n)"),
                new FormulaItem("Quotient", "a^m / a^n = a^(m-n)"),
                new FormulaItem("Power of Power", "(a^m)^n = a^(mn)")
            }),
        };
    }
}
