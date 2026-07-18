using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using Fluxion.src.Expressions;
using Fluxion.src.Rendering.Draw;
using Fluxion.src.Rendering.Visualize2D;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Wpf;

using Math = System.Math;
using Matrix4 = OpenTK.Mathematics.Matrix4;
using NumericsVector2 = System.Numerics.Vector2;
using NumericsVector3 = System.Numerics.Vector3;

namespace Fluxion.Desktop;

public partial class MainWindow : Window
{
    private CurveRenderer2D? _curveRenderer;
    private Plot2D? _functionPlot;

    private readonly List<Plot2D> _gridPlots = new();
    private readonly List<Plot2D> _axisPlots = new();

    private double _xMinimum = -10;
    private double _xMaximum = 10;
    private double _yMinimum = -1.25;
    private double _yMaximum = 1.25;

    private bool _isNumberLineMode;

    public MainWindow()
    {
        InitializeComponent();

        var settings = new GLWpfControlSettings
        {
            MajorVersion = 4,
            MinorVersion = 6,
            RenderContinuously = true
        };

        Viewport.Start(settings);

        BuildPlot();
    }

    private void Viewport_Render(TimeSpan delta)
    {
        if (Viewport.ActualWidth <= 0 ||
            Viewport.ActualHeight <= 0)
        {
            return;
        }

        DpiScale dpi =
            VisualTreeHelper.GetDpi(Viewport);

        int width = Math.Max(
            1,
            (int)(Viewport.ActualWidth *
                  dpi.DpiScaleX));

        int height = Math.Max(
            1,
            (int)(Viewport.ActualHeight *
                  dpi.DpiScaleY));

        GL.Viewport(
            0,
            0,
            width,
            height);

        GL.Disable(EnableCap.DepthTest);

        GL.ClearColor(
            0.055f,
            0.060f,
            0.075f,
            1.0f);

        GL.Clear(
            ClearBufferMask.ColorBufferBit |
            ClearBufferMask.DepthBufferBit);

        /*
         * Create the renderer only after the OpenGL context
         * has been started.
         */
        _curveRenderer ??=
            new CurveRenderer2D();

        Matrix4 projection =
            Matrix4.CreateOrthographicOffCenter(
                (float)_xMinimum,
                (float)_xMaximum,
                (float)_yMinimum,
                (float)_yMaximum,
                -1.0f,
                1.0f);

        /*
         * Number-line mode does not use the normal graph grid.
         */
        if (!_isNumberLineMode &&
            ShowGridBox.IsChecked == true)
        {
            foreach (Plot2D gridLine in _gridPlots)
            {
                _curveRenderer.Draw(
                    gridLine,
                    projection);
            }
        }

        /*
         * A number line must always draw its horizontal axis.
         * For functions, the Show Axes checkbox controls it.
         */
        if (_isNumberLineMode ||
            ShowAxesBox.IsChecked == true)
        {
            foreach (Plot2D axisLine in _axisPlots)
            {
                _curveRenderer.Draw(
                    axisLine,
                    projection);
            }
        }

        if (_functionPlot is not null)
        {
            _curveRenderer.Draw(
                _functionPlot,
                projection);
        }
    }

    private void Plot_Click(
        object sender,
        RoutedEventArgs e)
    {
        BuildPlot();
    }

    private void Clear_Click(
        object sender,
        RoutedEventArgs e)
    {
        _functionPlot = null;

        _gridPlots.Clear();
        _axisPlots.Clear();

        _isNumberLineMode = false;

        StatusText.Text = "Graph cleared";

        Viewport.InvalidateVisual();
    }

    private void ResetCamera_Click(
        object sender,
        RoutedEventArgs e)
    {
        XMinBox.Text = "-10";
        XMaxBox.Text = "10";

        BuildPlot();
    }

    private void ResolutionSlider_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (ResolutionText is null)
        {
            return;
        }

        ResolutionText.Text =
            $"Resolution: {(int)e.NewValue}";
    }

    private void BuildPlot()
    {
        try
        {
            /*
             * Index 0 is currently the supported automatic
             * scalar-or-2D mode.
             */
            if (GraphTypeBox.SelectedIndex != 0)
            {
                throw new NotSupportedException(
                    "3D surfaces are not connected yet. " +
                    "Select the first graph type option.");
            }

            CompiledExpression expression =
                ExpressionEngine.Compile(
                    FunctionBox.Text);

            /*
             * An expression without x is a scalar calculation.
             *
             * Examples:
             * 1
             * 2 + 3 * 4
             * pi / 2
             * sqrt(16)
             */
            if (!expression.ContainsVariable)
            {
                double result =
                    expression.Evaluate();

                if (!double.IsFinite(result))
                {
                    throw new ArithmeticException(
                        "The expression did not produce " +
                        "a finite real number.");
                }

                BuildNumberLine(result);

                StatusText.Text =
                    $"{FunctionBox.Text.Trim()} = " +
                    result.ToString(
                        "G12",
                        CultureInfo.InvariantCulture);

                Viewport.InvalidateVisual();
                return;
            }

            /*
             * An expression containing x becomes a 2D function.
             */
            _isNumberLineMode = false;

            if (!TryParseNumber(
                    XMinBox.Text,
                    out double xMinimum))
            {
                throw new FormatException(
                    "X minimum is not a valid number.");
            }

            if (!TryParseNumber(
                    XMaxBox.Text,
                    out double xMaximum))
            {
                throw new FormatException(
                    "X maximum is not a valid number.");
            }

            if (xMinimum >= xMaximum)
            {
                throw new ArgumentException(
                    "X minimum must be smaller " +
                    "than X maximum.");
            }

            _xMinimum = xMinimum;
            _xMaximum = xMaximum;

            int samples = Math.Max(
                25,
                (int)ResolutionSlider.Value);

            _functionPlot =
                Plot2DFactory.Function(
                    expression.ToFunction(),
                    _xMinimum,
                    _xMaximum,
                    samples,
                    new PlotStyle
                    {
                        Lines = true,
                        Points = false,
                        Width = 2.5f,
                        Rgb = new NumericsVector3(
                            0.20f,
                            0.80f,
                            1.00f)
                    });

            CalculateYBounds();
            BuildGridAndAxes();

            StatusText.Text =
                $"Plotting y = {expression.Source}";

            Viewport.InvalidateVisual();
        }
        catch (Exception exception)
        {
            StatusText.Text =
                exception.Message;

            MessageBox.Show(
                exception.Message,
                "Unable to process expression",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void BuildNumberLine(double value)
    {
        _isNumberLineMode = true;

        /*
         * Keep both zero and the result visible.
         */
        double padding = Math.Max(
            1.0,
            Math.Abs(value) * 0.20);

        if (Math.Abs(value) < 0.000001)
        {
            _xMinimum = -5;
            _xMaximum = 5;
        }
        else
        {
            _xMinimum =
                Math.Min(0.0, value) - padding;

            _xMaximum =
                Math.Max(0.0, value) + padding;
        }

        _yMinimum = -1.0;
        _yMaximum = 1.0;

        _gridPlots.Clear();
        _axisPlots.Clear();

        var axisColor =
            new NumericsVector3(
                0.85f,
                0.85f,
                0.85f);

        /*
         * Main horizontal number line.
         */
        _axisPlots.Add(
            CreateLine(
                (float)_xMinimum,
                0.0f,
                (float)_xMaximum,
                0.0f,
                axisColor,
                2.0f));

        double tickStep =
            CalculateNiceStep(
                _xMinimum,
                _xMaximum);

        double firstTick =
            Math.Ceiling(
                _xMinimum / tickStep) *
            tickStep;

        /*
         * Tick marks along the number line.
         */
        for (double tick = firstTick;
             tick <= _xMaximum + 0.000001;
             tick += tickStep)
        {
            _axisPlots.Add(
                CreateLine(
                    (float)tick,
                    -0.08f,
                    (float)tick,
                    0.08f,
                    axisColor,
                    1.5f));
        }

        /*
         * Make zero slightly larger.
         */
        if (_xMinimum <= 0 &&
            _xMaximum >= 0)
        {
            _axisPlots.Add(
                CreateLine(
                    0.0f,
                    -0.14f,
                    0.0f,
                    0.14f,
                    axisColor,
                    2.0f));
        }

        /*
         * Draw the evaluated result as a point.
         */
        _functionPlot =
            new Plot2D(
                new PlotStyle
                {
                    Lines = false,
                    Points = true,
                    Width = 12.0f,
                    Rgb = new NumericsVector3(
                        0.20f,
                        0.80f,
                        1.00f)
                });

        _functionPlot.Points.Add(
            new NumericsVector2(
                (float)value,
                0.0f));
    }

    private void CalculateYBounds()
    {
        if (_functionPlot is null)
        {
            _yMinimum = -1;
            _yMaximum = 1;
            return;
        }

        double[] finiteValues =
            _functionPlot.Points
                .Select(point =>
                    (double)point.Y)
                .Where(value =>
                    double.IsFinite(value) &&
                    Math.Abs(value) < 1_000_000)
                .ToArray();

        if (finiteValues.Length == 0)
        {
            throw new ArithmeticException(
                "The expression did not produce any " +
                "finite real values in the selected range.");
        }

        double minimum =
            finiteValues.Min();

        double maximum =
            finiteValues.Max();

        double range =
            maximum - minimum;

        if (range < 0.000001)
        {
            double center = minimum;

            _yMinimum = center - 1.0;
            _yMaximum = center + 1.0;
            return;
        }

        double padding =
            range * 0.15;

        _yMinimum =
            minimum - padding;

        _yMaximum =
            maximum + padding;

        /*
         * Keep zero visible when it is reasonably
         * close to the function's range.
         */
        if (_yMinimum > 0 &&
            _yMinimum < range)
        {
            _yMinimum = 0;
        }

        if (_yMaximum < 0 &&
            Math.Abs(_yMaximum) < range)
        {
            _yMaximum = 0;
        }
    }

    private void BuildGridAndAxes()
    {
        _gridPlots.Clear();
        _axisPlots.Clear();

        double xStep =
            CalculateNiceStep(
                _xMinimum,
                _xMaximum);

        double yStep =
            CalculateNiceStep(
                _yMinimum,
                _yMaximum);

        double firstX =
            Math.Ceiling(
                _xMinimum / xStep) *
            xStep;

        for (double x = firstX;
             x <= _xMaximum + 0.000001;
             x += xStep)
        {
            _gridPlots.Add(
                CreateLine(
                    (float)x,
                    (float)_yMinimum,
                    (float)x,
                    (float)_yMaximum,
                    new NumericsVector3(
                        0.17f,
                        0.18f,
                        0.21f),
                    1.0f));
        }

        double firstY =
            Math.Ceiling(
                _yMinimum / yStep) *
            yStep;

        for (double y = firstY;
             y <= _yMaximum + 0.000001;
             y += yStep)
        {
            _gridPlots.Add(
                CreateLine(
                    (float)_xMinimum,
                    (float)y,
                    (float)_xMaximum,
                    (float)y,
                    new NumericsVector3(
                        0.17f,
                        0.18f,
                        0.21f),
                    1.0f));
        }

        /*
         * Y axis.
         */
        if (_xMinimum <= 0 &&
            _xMaximum >= 0)
        {
            _axisPlots.Add(
                CreateLine(
                    0,
                    (float)_yMinimum,
                    0,
                    (float)_yMaximum,
                    new NumericsVector3(
                        0.85f,
                        0.85f,
                        0.85f),
                    2.0f));
        }

        /*
         * X axis.
         */
        if (_yMinimum <= 0 &&
            _yMaximum >= 0)
        {
            _axisPlots.Add(
                CreateLine(
                    (float)_xMinimum,
                    0,
                    (float)_xMaximum,
                    0,
                    new NumericsVector3(
                        0.85f,
                        0.85f,
                        0.85f),
                    2.0f));
        }
    }

    private static Plot2D CreateLine(
        float x1,
        float y1,
        float x2,
        float y2,
        NumericsVector3 color,
        float width)
    {
        var plot =
            new Plot2D(
                new PlotStyle
                {
                    Lines = true,
                    Points = false,
                    Width = width,
                    Rgb = color
                });

        plot.Points.Add(
            new NumericsVector2(
                x1,
                y1));

        plot.Points.Add(
            new NumericsVector2(
                x2,
                y2));

        return plot;
    }

    private static double CalculateNiceStep(
        double minimum,
        double maximum)
    {
        double span = Math.Max(
            0.000001,
            maximum - minimum);

        double roughStep =
            span / 10.0;

        double magnitude =
            Math.Pow(
                10,
                Math.Floor(
                    Math.Log10(
                        roughStep)));

        double normalized =
            roughStep / magnitude;

        double niceValue =
            normalized switch
            {
                < 1.5 => 1,
                < 3.0 => 2,
                < 7.0 => 5,
                _ => 10
            };

        return niceValue * magnitude;
    }

    private static bool TryParseNumber(
        string text,
        out double result)
    {
        return
            double.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out result)
            ||
            double.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.CurrentCulture,
                out result);
    }
}