//  Math/Algebra/Equations/Linear.cs
using Math.Algebra.Concepts;
using Math.Abstractions;
namespace Math.Algebra.Equations
{
    public sealed class Linear : IEquation, IDisplay
    {
        public LinearModel Model { get; }
        public Linear(double a, double b) => Model = new(a, b);

        public double Evaluate(double x) => Model.A * x + Model.B;
        public bool IsWellFormed() => !(Model.A == 0 && Model.B != 0); // A=0,B!=0 is contradiction

        public string AsString(string variable = "x")
            => $"{Model.A}*{variable} + {Model.B} = 0";
    }
}
