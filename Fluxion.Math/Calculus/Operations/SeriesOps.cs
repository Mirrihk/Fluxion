// File: Fluxion.Math/Calculus/Operations/SeriesOps.cs
using Fluxion.Math.Calculus.Concepts;
using Fluxion.Math.Calculus.Series;

namespace Fluxion.Math.Calculus.Operations
{
    public static class SeriesOps
    {
        public static double Evaluate(SeriesModel s, double x, int terms) => PowerSeries.Evaluate(s, x, terms);
    }
}
