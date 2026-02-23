// ===============================
// File: Fluxion.Math/Calculus/Concepts/CalculusModels.cs
// ===============================
using Fluxion.src.Numerics.Calculus.Limits;
using Fluxion.src.Numerics.Calculus.Series;
using System;

namespace Fluxion.src.Numerics.Calculus.Concepts
{
    /// <summary>Simple function abstraction backed by a delegate.</summary>
    public interface IFunction
    {
        double Evaluate(double x);
    }

    public sealed class LambdaFunction : IFunction
    {
        private readonly Func<double, double> _f;
        public LambdaFunction(Func<double, double> f) => _f = f ?? throw new ArgumentNullException(nameof(f));
        public double Evaluate(double x) => _f(x);
    }

    /// <summary>Encapsulates a limit problem: lim_{x->a^dir} f(x)</summary>
    public sealed class LimitModel
    {
        public IFunction F { get; }
        public double Point { get; }
        public ApproachDirection Direction { get; }
        public LimitModel(IFunction f, double point, ApproachDirection dir = ApproachDirection.TwoSided)
        { F = f; Point = point; Direction = dir; }
    }

    public enum ApproachDirection { Left, Right, TwoSided }

    /// <summary>Describes a power series \sum c_n (x-a)^n.</summary>
    public sealed class SeriesModel
    {
        public double Center { get; }
        public Func<int, double> Coeff { get; }
        public SeriesModel(double center, Func<int, double> coeff)
        { Center = center; Coeff = coeff; }
    }
}

