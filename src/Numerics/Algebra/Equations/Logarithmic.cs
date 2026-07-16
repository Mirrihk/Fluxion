// Fluxion.Math/Algebra/Equations/Logarithmic.cs
using static System.Math;
using Fluxion.src.Numerics.Abstractions;
using Fluxion.src.Numerics.Algebra.Concepts;
namespace Fluxion.src.Numerics.Algebra.Equations
{
    public sealed class Logarithmic : IEquation, IDisplay
    {
        public LogarithmicModel Model { get; }
        public Logarithmic(double a, double @base, double c) => Model = new(a, @base, c);

        public double Evaluate(double x) => Model.A * (Log(x) / Log(Model.Base)) + Model.C;
        public bool IsWellFormed() => Model.Base > 0 && Model.Base != 1;

        public string AsString(string variable = "x")
            => $"{Model.A}*log_{Model.Base}({variable}) + {Model.C} = 0";
    }
}
