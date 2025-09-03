//Fluxion.Math/Trigonometry/Functions/SineEquation.cs

using Math.Algebra.Concepts;
using Math.Trigonometry.Concepts;
using System;
using Math.Abstractions;
namespace Math.Trigonometry.Functions
{
    public readonly struct SineEquation : IEquation, IDisplay
    {
        public SineModel Model { get; }

        public SineEquation(SineModel model) => Model = model;

        public double Evaluate(double x)
            => Model.Amplitude * System.Math.Sin(Model.Frequency * x + Model.Phase) + Model.VerticalOffset;

        public bool IsWellFormed()
            => !(double.IsNaN(Model.Amplitude) || double.IsNaN(Model.Frequency)
                 || double.IsNaN(Model.Phase) || double.IsNaN(Model.VerticalOffset));

        public string AsString(string variable = "x")
            => $"{Model.Amplitude}·sin({Model.Frequency}·{variable} + {Model.Phase}) + {Model.VerticalOffset}";
    }
}