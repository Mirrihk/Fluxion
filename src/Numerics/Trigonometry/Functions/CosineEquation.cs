using Fluxion.src.Numerics.Abstractions;
using Fluxion.src.Numerics.Trigonometry.Concepts;
using System;

namespace Fluxion.src.Numerics.Trigonometry.Functions
{
    /// <summary>f(x) = Amplitude * cos(Frequency * x + Phase) + VerticalOffset</summary>
    public readonly struct CosineEquation : IEquation, IDisplay
    {
        public CosineModel Model { get; }

        public CosineEquation(CosineModel model) => Model = model;

        public double Evaluate(double x)
            => Model.Amplitude * System.Math.Cos(Model.Frequency * x + Model.Phase) + Model.VerticalOffset;

        public bool IsWellFormed()
            => !(double.IsNaN(Model.Amplitude) || double.IsNaN(Model.Frequency) ||
                 double.IsNaN(Model.Phase) || double.IsNaN(Model.VerticalOffset));

        public string AsString(string variable = "x")
            => $"{Model.Amplitude}·cos({Model.Frequency}·{variable} + {Model.Phase}) + {Model.VerticalOffset}";
    }
}
