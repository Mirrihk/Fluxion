// Fluxion.Math/Algebra/Equations/Quadratic.cs
using Math.Algebra.Concepts;
using static System.Math;
using Math.Abstractions;
namespace Fluxion.Math.Algebra.Equations
{
    public sealed class Quadratic : IEquation, IDisplay
    {
        public QuadraticModel Model { get; }
        public Quadratic(double a, double b, double c) => Model = new(a, b, c);

        public double Evaluate(double x) => Model.A * x * x + Model.B * x + Model.C;
        public bool IsWellFormed() => Model.A != 0; // avoid degenerating into linear here

        public double Discriminant => Model.B * Model.B - 4.0 * Model.A * Model.C;

        public string AsString(string variable = "x")
            => $"{Model.A}*{variable}^2 + {Model.B}*{variable} + {Model.C} = 0";
    }
}
