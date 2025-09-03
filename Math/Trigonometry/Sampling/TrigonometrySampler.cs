//Fluxion.Math/Trigonometry/Sampling/TrigonometrySampler.cs
using System;

namespace Math.Trigonometry.Sampling
{
    public static class TrigonometrySampler
    {
        /// <summary>
        /// Uniformly samples a function f(x) between start and end using the given number of steps.
        /// Returns a tuple of (x values, y values).
        /// </summary>
        public static (double[] XValues, double[] YValues) Sample(Func<double, double> function, double start, double end, int numberOfSteps)
        {
            if (numberOfSteps < 2) throw new ArgumentOutOfRangeException(nameof(numberOfSteps), "Number of steps must be >= 2");

            var xValues = new double[numberOfSteps];
            var yValues = new double[numberOfSteps];

            double deltaX = (end - start) / (numberOfSteps - 1);

            for (int i = 0; i < numberOfSteps; i++)
            {
                var xi = start + i * deltaX;
                xValues[i] = xi;
                yValues[i] = function(xi);
            }

            return (xValues, yValues);
        }
    }
}
