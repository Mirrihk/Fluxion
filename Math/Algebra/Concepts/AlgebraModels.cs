// FMath/Algebra/Concepts/AlgebraModels.cs
using System;
using System.Collections.Generic;

namespace Math.Algebra.Concepts
{
    /// <summary>
    /// Represents a linear equation of the form A*x + B = 0.
    /// </summary>
    public class LinearEquation
    {
        public double A { get; }
        public double B { get; }

        public LinearEquation(double a, double b)
        {
            A = a;
            B = b;
        }
    }

    /// <summary>Base contract for 1-variable equations.</summary>

    // -------- Canonical models (parameters only) --------

    /// <summary>Ax + B = 0</summary>
    public readonly record struct LinearModel(double A, double B);

    /// <summary>Ax² + Bx + C = 0</summary>
    public readonly record struct QuadraticModel(double A, double B, double C);

    /// <summary>c0 + c1 x + ... + cn x^n</summary>
    public readonly record struct PolynomialModel(double[] Coefficients);

    /// <summary>N(x) / D(x)</summary>
    public readonly record struct RationalModel(PolynomialModel Numer, PolynomialModel Denom);

    /// <summary>A e^{B x} + C = 0</summary>
    public readonly record struct ExponentialModel(double A, double B, double C);

    /// <summary>A log_base(x) + C = 0 (base > 0, base ≠ 1)</summary>
    public readonly record struct LogarithmicModel(double A, double Base, double C);

    /// <summary>2×2 system: A x + B y = E; C x + D y = F.</summary>
    public readonly record struct System2x2Model(double A, double B, double E, double C, double D, double F);

    /// <summary>Solutions container for real roots.</summary>
    public readonly record struct RealSolutions(double? X1, double? X2 = null)
    {
        public bool HasAny => X1.HasValue || X2.HasValue;
    }

    // -------- Minimal stubs for formulas/panels (now integrated) --------

    /// <summary>Named algebra formula with an optional note.</summary>
    public sealed record FormulaItem(string Name, string Formula, string Notes = "");

    /// <summary>Topic containing a set of reference formulas.</summary>
    public sealed record AlgebraTopic(string Title, IReadOnlyList<FormulaItem> Items);

    // -------- Compatibility shim (to phase out old "Linear") --------
    /// <summary>
    /// Temporary alias to maintain compatibility with old code using "Linear".
    /// Prefer <see cref="LinearEquation"/> instead.
    /// </summary>
    [Obsolete("Use LinearEquation instead.")]
    public class Linear : LinearEquation
    {
        public Linear(double a, double b) : base(a, b) { }
    }
}
