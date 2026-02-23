// File: Fluxion.Math/Calculus/Operations/SeriesOps.cs
using Fluxion.Numerics.Calculus.Concepts;
using Fluxion.Numerics.Calculus.Series;

namespace Fluxion.Numerics.Calculus.Operations
{
    public static class SeriesOps
    {
        public static double Evaluate(SeriesModel s, double x, int terms) => PowerSeries.Evaluate(s, x, terms);
    }
}
