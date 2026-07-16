// File: Fluxion.Math/Calculus/Operations/SeriesOps.cs
using Fluxion.src.Numerics.Calculus.Concepts;
using Fluxion.src.Numerics.Calculus.Series;

namespace Fluxion.src.Numerics.Calculus.Operations
{
    public static class SeriesOps
    {
        public static double Evaluate(SeriesModel s, double x, int terms) => PowerSeries.Evaluate(s, x, terms);
    }
}
