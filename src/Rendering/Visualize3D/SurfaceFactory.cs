using System;
using System.Collections.Generic;
using System.Linq;

using Fluxion.src.Numerics.Functions3D;

using OpenTK.Mathematics;

namespace Fluxion.src.Rendering.Visualize3D;

public static class SurfaceFactory
{
    private const double MaximumAbsoluteHeight =
        1_000_000.0;

    public static Mesh3D BuildSurface(
        IScalarField scalarField,
        int gridResolution,
        double xMin,
        double xMax,
        double yMin,
        double yMax)
    {
        ArgumentNullException.ThrowIfNull(
            scalarField);

        ValidateRange(
            xMin,
            xMax,
            nameof(xMin),
            nameof(xMax));

        ValidateRange(
            yMin,
            yMax,
            nameof(yMin),
            nameof(yMax));

        gridResolution =
            Math.Clamp(
                gridResolution,
                2,
                400);

        int xSamples = gridResolution;
        int ySamples = gridResolution;

        int vertexCount =
            xSamples * ySamples;

        var positions =
            new Vector3[vertexCount];

        var normals =
            new Vector3[vertexCount];

        var valid =
            new bool[vertexCount];

        var heights =
            new double[vertexCount];

        double stepX =
            (xMax - xMin) /
            (xSamples - 1);

        double stepY =
            (yMax - yMin) /
            (ySamples - 1);

        for (int row = 0;
             row < ySamples;
             row++)
        {
            double y =
                yMin + (row * stepY);

            for (int column = 0;
                 column < xSamples;
                 column++)
            {
                double x =
                    xMin + (column * stepX);

                int index =
                    (row * xSamples) +
                    column;

                bool isValid =
                    TryEvaluate(
                        scalarField,
                        x,
                        y,
                        out double z);

                valid[index] =
                    isValid;

                heights[index] =
                    z;

                /*
                 * OpenTK uses Y as the vertical world axis.
                 *
                 * Mathematical coordinates:
                 * x -> world X
                 * y -> world Z
                 * z -> world Y
                 */
                positions[index] =
                    new Vector3(
                        (float)x,
                        isValid ? (float)z : 0.0f,
                        (float)y);
            }
        }

        BuildNormals(
            positions,
            normals,
            valid,
            xSamples,
            ySamples);

        double robustHeightScale =
            CalculateRobustHeightScale(
                heights,
                valid);

        var indices =
            BuildIndices(
                heights,
                valid,
                xSamples,
                ySamples,
                robustHeightScale);

        return new Mesh3D
        {
            Positions = positions,
            Normals = normals,
            Indices = indices.ToArray()
        };
    }

    public static Mesh3D BuildPolyline(
        Func<double, Vector3d> curveFunction,
        double tMin,
        double tMax,
        int samplesCount)
    {
        ArgumentNullException.ThrowIfNull(
            curveFunction);

        ValidateRange(
            tMin,
            tMax,
            nameof(tMin),
            nameof(tMax));

        samplesCount =
            Math.Max(
                2,
                samplesCount);

        var sampledPoints =
            new Vector3[samplesCount];

        double tStep =
            (tMax - tMin) /
            (samplesCount - 1);

        for (int index = 0;
             index < samplesCount;
             index++)
        {
            double t =
                tMin + (index * tStep);

            Vector3d point =
                curveFunction(t);

            sampledPoints[index] =
                new Vector3(
                    (float)point.X,
                    (float)point.Z,
                    (float)point.Y);
        }

        return new Mesh3D
        {
            Positions = sampledPoints,
            Normals = Array.Empty<Vector3>(),
            Indices = Array.Empty<int>()
        };
    }

    private static void BuildNormals(
        IReadOnlyList<Vector3> positions,
        IList<Vector3> normals,
        IReadOnlyList<bool> valid,
        int xSamples,
        int ySamples)
    {
        for (int row = 0;
             row < ySamples;
             row++)
        {
            for (int column = 0;
                 column < xSamples;
                 column++)
            {
                int index =
                    (row * xSamples) +
                    column;

                if (!valid[index])
                {
                    normals[index] =
                        Vector3.UnitY;

                    continue;
                }

                int leftColumn =
                    Math.Max(
                        column - 1,
                        0);

                int rightColumn =
                    Math.Min(
                        column + 1,
                        xSamples - 1);

                int bottomRow =
                    Math.Max(
                        row - 1,
                        0);

                int topRow =
                    Math.Min(
                        row + 1,
                        ySamples - 1);

                int leftIndex =
                    (row * xSamples) +
                    leftColumn;

                int rightIndex =
                    (row * xSamples) +
                    rightColumn;

                int bottomIndex =
                    (bottomRow * xSamples) +
                    column;

                int topIndex =
                    (topRow * xSamples) +
                    column;

                Vector3 center =
                    positions[index];

                Vector3 left =
                    valid[leftIndex]
                        ? positions[leftIndex]
                        : center;

                Vector3 right =
                    valid[rightIndex]
                        ? positions[rightIndex]
                        : center;

                Vector3 bottom =
                    valid[bottomIndex]
                        ? positions[bottomIndex]
                        : center;

                Vector3 top =
                    valid[topIndex]
                        ? positions[topIndex]
                        : center;

                Vector3 xDirection =
                    right - left;

                Vector3 yDirection =
                    top - bottom;

                Vector3 normal =
                    Vector3.Cross(
                        yDirection,
                        xDirection);

                normals[index] =
                    normal.LengthSquared >
                    0.0000001f
                        ? Vector3.Normalize(normal)
                        : Vector3.UnitY;
            }
        }
    }

    private static List<int> BuildIndices(
        IReadOnlyList<double> heights,
        IReadOnlyList<bool> valid,
        int xSamples,
        int ySamples,
        double robustHeightScale)
    {
        var indices =
            new List<int>(
                (xSamples - 1) *
                (ySamples - 1) *
                6);

        for (int row = 0;
             row < ySamples - 1;
             row++)
        {
            for (int column = 0;
                 column < xSamples - 1;
                 column++)
            {
                int v00 =
                    (row * xSamples) +
                    column;

                int v10 =
                    v00 + 1;

                int v01 =
                    ((row + 1) * xSamples) +
                    column;

                int v11 =
                    v01 + 1;

                if (!valid[v00] ||
                    !valid[v10] ||
                    !valid[v01] ||
                    !valid[v11])
                {
                    continue;
                }

                double minimum =
                    Math.Min(
                        Math.Min(
                            heights[v00],
                            heights[v10]),
                        Math.Min(
                            heights[v01],
                            heights[v11]));

                double maximum =
                    Math.Max(
                        Math.Max(
                            heights[v00],
                            heights[v10]),
                        Math.Max(
                            heights[v01],
                            heights[v11]));

                /*
                 * Avoid triangles that bridge a likely vertical
                 * discontinuity. This is deliberately conservative.
                 */
                double maximumAllowedJump =
                    Math.Max(
                        10.0,
                        robustHeightScale * 20.0);

                if (maximum - minimum >
                    maximumAllowedJump)
                {
                    continue;
                }

                indices.Add(v00);
                indices.Add(v01);
                indices.Add(v10);

                indices.Add(v10);
                indices.Add(v01);
                indices.Add(v11);
            }
        }

        return indices;
    }

    private static bool TryEvaluate(
        IScalarField scalarField,
        double x,
        double y,
        out double z)
    {
        try
        {
            z =
                scalarField.Evaluate(
                    x,
                    y);

            return
                double.IsFinite(z) &&
                Math.Abs(z) <=
                MaximumAbsoluteHeight;
        }
        catch
        {
            z = double.NaN;
            return false;
        }
    }

    private static double CalculateRobustHeightScale(
        IReadOnlyList<double> heights,
        IReadOnlyList<bool> valid)
    {
        double[] magnitudes =
            heights
                .Where(
                    (_, index) =>
                        valid[index])
                .Select(Math.Abs)
                .OrderBy(value => value)
                .ToArray();

        if (magnitudes.Length == 0)
        {
            return 1.0;
        }

        int percentileIndex =
            (int)Math.Floor(
                (magnitudes.Length - 1) *
                0.80);

        return Math.Max(
            1.0e-6,
            magnitudes[percentileIndex]);
    }

    private static void ValidateRange(
        double minimum,
        double maximum,
        string minimumName,
        string maximumName)
    {
        if (!double.IsFinite(minimum) ||
            !double.IsFinite(maximum))
        {
            throw new ArgumentException(
                "Surface bounds must be finite.");
        }

        if (minimum >= maximum)
        {
            throw new ArgumentException(
                $"{minimumName} must be smaller than {maximumName}.");
        }
    }
}
