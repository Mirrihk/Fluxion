// Fluxion.Math/Algebra/Equations/Exponential.cs
using Math.Abstractions;
using Math.Algebra.Concepts;
using static System.Math;

namespace Math.Algebra.Equations
{
    public sealed class Exponential : IEquation, IDisplay
    {
        public ExponentialModel Model { get; }
        public Exponential(double a, double b, double c) => Model = new(a, b, c);

        public double Evaluate(double x) => Model.A * Exp(Model.B * x) + Model.C;
        public bool IsWellFormed() => true;

        public string AsString(string variable = "x")
            => $"{Model.A}*e^({Model.B}{variable}) + {Model.C} = 0";
    }
}
