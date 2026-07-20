using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Fluxion.src.Plotting.Sampling;

/// <summary>
/// Samples a one-variable function and separates it into independent
/// curve segments so the renderer never connects across discontinuities.
/// </summary>
public static class FunctionSegmentSampler
{
    public static FunctionSampleResult Sample(
        Func<double, double> function,
        double xMinimum,
        double xMaximum,
        int sampleCount,
        FunctionSamplingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(function);

        if (!double.IsFinite(xMinimum) ||
            !double.IsFinite(xMaximum))
        {
            throw new ArgumentException(
                "The sampling range must contain finite values.");
        }

        if (xMinimum >= xMaximum)
        {
            throw new ArgumentException(
                "X minimum must be smaller than X maximum.");
        }

        sampleCount = Math.Max(2, sampleCount);
        options ??= new FunctionSamplingOptions();

        var samples = new SamplePoint[sampleCount];

        double step =
            (xMaximum - xMinimum) /
            (sampleCount - 1);

        int invalidSampleCount = 0;

        for (int index = 0; index < sampleCount; index++)
        {
            double x = index == sampleCount - 1
                ? xMaximum
                : xMinimum + (index * step);

            samples[index] =
                EvaluateSafely(
                    function,
                    x,
                    options.MaximumAbsoluteY);

            if (!samples[index].IsValid)
            {
                invalidSampleCount++;
            }
        }

        double robustScale =
            CalculateRobustScale(samples);

        var segments =
            new List<IReadOnlyList<Vector2>>();

        var currentSegment =
            new List<Vector2>();

        int breakCount = 0;

        for (int index = 0; index < samples.Length; index++)
        {
            SamplePoint current = samples[index];

            if (!current.IsValid)
            {
                if (FlushSegment(currentSegment, segments))
                {
                    breakCount++;
                }

                continue;
            }

            if (currentSegment.Count == 0)
            {
                currentSegment.Add(ToVector(current));
                continue;
            }

            SamplePoint previous = samples[index - 1];

            if (!previous.IsValid)
            {
                currentSegment.Add(ToVector(current));
                continue;
            }

            IntervalInspection inspection =
                InspectInterval(
                    function,
                    previous,
                    current,
                    robustScale,
                    options);

            invalidSampleCount += inspection.InvalidProbeCount;

            if (inspection.ShouldBreak)
            {
                FlushSegment(currentSegment, segments);
                breakCount++;
                currentSegment.Add(ToVector(current));
                continue;
            }

            currentSegment.Add(ToVector(current));
        }

        FlushSegment(currentSegment, segments);

        return new FunctionSampleResult(
            segments,
            breakCount,
            invalidSampleCount,
            robustScale);
    }

    private static IntervalInspection InspectInterval(
        Func<double, double> function,
        SamplePoint left,
        SamplePoint right,
        double robustScale,
        FunctionSamplingOptions options)
    {
        double width = right.X - left.X;

        var probes = new[]
        {
            left,
            EvaluateSafely(
                function,
                left.X + (width * 0.25),
                options.MaximumAbsoluteY),
            EvaluateSafely(
                function,
                left.X + (width * 0.50),
                options.MaximumAbsoluteY),
            EvaluateSafely(
                function,
                left.X + (width * 0.75),
                options.MaximumAbsoluteY),
            right
        };

        int invalidProbeCount =
            probes.Count(point => !point.IsValid);

        if (invalidProbeCount > 0)
        {
            return new IntervalInspection(
                true,
                invalidProbeCount);
        }

        double endpointMaximum =
            Math.Max(
                Math.Abs(left.Y),
                Math.Abs(right.Y));

        double intervalMaximum =
            probes.Max(point => Math.Abs(point.Y));

        bool interiorSpike =
            intervalMaximum >
            Math.Max(
                robustScale *
                options.InteriorSpikeScaleMultiplier,
                endpointMaximum *
                options.InteriorSpikeEndpointMultiplier);

        bool largeSignFlip = false;

        for (int index = 1; index < probes.Length; index++)
        {
            double previousValue = probes[index - 1].Y;
            double currentValue = probes[index].Y;

            bool signsDiffer =
                Math.Sign(previousValue) !=
                Math.Sign(currentValue);

            bool bothLarge =
                Math.Min(
                    Math.Abs(previousValue),
                    Math.Abs(currentValue))
                >
                robustScale *
                options.LargeSignFlipMultiplier;

            if (signsDiffer && bothLarge)
            {
                largeSignFlip = true;
                break;
            }
        }

        double totalVariation = 0.0;

        for (int index = 1; index < probes.Length; index++)
        {
            totalVariation +=
                Math.Abs(
                    probes[index].Y -
                    probes[index - 1].Y);
        }

        double endpointChange =
            Math.Abs(right.Y - left.Y);

        bool excessiveVariation =
            totalVariation >
                robustScale *
                options.TotalVariationScaleMultiplier
            &&
            totalVariation >
                Math.Max(1.0, endpointChange) *
                options.TotalVariationEndpointMultiplier;

        return new IntervalInspection(
            interiorSpike ||
            largeSignFlip ||
            excessiveVariation,
            0);
    }

    private static SamplePoint EvaluateSafely(
        Func<double, double> function,
        double x,
        double maximumAbsoluteY)
    {
        try
        {
            double y = function(x);

            bool valid =
                double.IsFinite(y) &&
                Math.Abs(y) <= maximumAbsoluteY;

            return new SamplePoint(x, y, valid);
        }
        catch
        {
            return new SamplePoint(
                x,
                double.NaN,
                false);
        }
    }

    private static double CalculateRobustScale(
        IReadOnlyList<SamplePoint> samples)
    {
        double[] magnitudes =
            samples
                .Where(point => point.IsValid)
                .Select(point => Math.Abs(point.Y))
                .OrderBy(value => value)
                .ToArray();

        if (magnitudes.Length == 0)
        {
            return 1.0;
        }

        int percentileIndex =
            (int)Math.Floor(
                (magnitudes.Length - 1) * 0.80);

        return Math.Max(
            1.0e-6,
            magnitudes[percentileIndex]);
    }

    private static bool FlushSegment(
        List<Vector2> currentSegment,
        List<IReadOnlyList<Vector2>> segments)
    {
        if (currentSegment.Count >= 2)
        {
            segments.Add(currentSegment.ToArray());
            currentSegment.Clear();
            return true;
        }

        currentSegment.Clear();
        return false;
    }

    private static Vector2 ToVector(SamplePoint point)
    {
        return new Vector2(
            (float)point.X,
            (float)point.Y);
    }

    private readonly record struct SamplePoint(
        double X,
        double Y,
        bool IsValid);

    private readonly record struct IntervalInspection(
        bool ShouldBreak,
        int InvalidProbeCount);
}

public sealed class FunctionSamplingOptions
{
    public double MaximumAbsoluteY { get; init; } =
        1_000_000.0;

    public double InteriorSpikeScaleMultiplier { get; init; } =
        8.0;

    public double InteriorSpikeEndpointMultiplier { get; init; } =
        6.0;

    public double LargeSignFlipMultiplier { get; init; } =
        4.0;

    public double TotalVariationScaleMultiplier { get; init; } =
        20.0;

    public double TotalVariationEndpointMultiplier { get; init; } =
        4.0;
}

public sealed class FunctionSampleResult
{
    public FunctionSampleResult(
        IReadOnlyList<IReadOnlyList<Vector2>> segments,
        int breakCount,
        int invalidSampleCount,
        double robustScale)
    {
        Segments = segments;
        BreakCount = breakCount;
        InvalidSampleCount = invalidSampleCount;
        RobustScale = robustScale;
    }

    public IReadOnlyList<IReadOnlyList<Vector2>> Segments { get; }

    public int BreakCount { get; }

    public int InvalidSampleCount { get; }

    public double RobustScale { get; }
}
