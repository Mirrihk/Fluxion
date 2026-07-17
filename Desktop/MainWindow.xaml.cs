using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Fluxion.src.Rendering.Draw;
using Fluxion.src.Rendering.Visualize2D;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Wpf;

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

    public MainWindow()
    {
        InitializeComponent();

        /*
         * This was the missing operation.
         *
         * GLWpfControl does not automatically create and start its
         * OpenGL rendering context. Start() must be called explicitly.
         */
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
        if (Viewport.ActualWidth <= 0 || Viewport.ActualHeight <= 0)
        {
            return;
        }

        DpiScale dpi = VisualTreeHelper.GetDpi(Viewport);

        int width = Math.Max(
            1,
            (int)(Viewport.ActualWidth * dpi.DpiScaleX));

        int height = Math.Max(
            1,
            (int)(Viewport.ActualHeight * dpi.DpiScaleY));

        GL.Viewport(0, 0, width, height);

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
         * CurveRenderer2D creates shaders, a VAO and a VBO.
         * It must therefore be created only after Start() has created
         * an active OpenGL context.
         */
        _curveRenderer ??= new CurveRenderer2D();

        Matrix4 projection =
            Matrix4.CreateOrthographicOffCenter(
                (float)_xMinimum,
                (float)_xMaximum,
                (float)_yMinimum,
                (float)_yMaximum,
                -1.0f,
                1.0f);

        if (ShowGridBox.IsChecked == true)
        {
            foreach (Plot2D gridLine in _gridPlots)
            {
                _curveRenderer.Draw(gridLine, projection);
            }
        }

        if (ShowAxesBox.IsChecked == true)
        {
            foreach (Plot2D axisLine in _axisPlots)
            {
                _curveRenderer.Draw(axisLine, projection);
            }
        }

        if (_functionPlot is not null)
        {
            _curveRenderer.Draw(_functionPlot, projection);
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
            if (GraphTypeBox.SelectedIndex != 0)
            {
                throw new NotSupportedException(
                    "The WPF viewport is currently connected to the 2D renderer. Select 2D Function.");
            }

            if (!TryParseNumber(XMinBox.Text, out double xMinimum))
            {
                throw new FormatException(
                    "X minimum is not a valid number.");
            }

            if (!TryParseNumber(XMaxBox.Text, out double xMaximum))
            {
                throw new FormatException(
                    "X maximum is not a valid number.");
            }

            if (xMinimum >= xMaximum)
            {
                throw new ArgumentException(
                    "X minimum must be smaller than X maximum.");
            }

            _xMinimum = xMinimum;
            _xMaximum = xMaximum;

            int samples = Math.Max(
                25,
                (int)ResolutionSlider.Value);

            Func<double, double> function =
                ParseFunction(FunctionBox.Text);

            _functionPlot =
                Plot2DFactory.Function(
                    function,
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
                $"Plotting {FunctionBox.Text.Trim()}";

            Viewport.InvalidateVisual();
        }
        catch (Exception exception)
        {
            StatusText.Text = exception.Message;

            MessageBox.Show(
                exception.Message,
                "Unable to plot function",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void CalculateYBounds()
    {
        if (_functionPlot is null)
        {
            _yMinimum = -1;
            _yMaximum = 1;
            return;
        }

        double[] finiteValues = _functionPlot.Points
            .Select(point => (double)point.Y)
            .Where(value =>
                double.IsFinite(value) &&
                Math.Abs(value) < 1_000_000)
            .ToArray();

        if (finiteValues.Length == 0)
        {
            _yMinimum = -1;
            _yMaximum = 1;
            return;
        }

        double minimum = finiteValues.Min();
        double maximum = finiteValues.Max();

        double range = maximum - minimum;

        if (range < 0.000001)
        {
            range = 2;
        }

        double padding = range * 0.15;

        _yMinimum = minimum - padding;
        _yMaximum = maximum + padding;

        /*
         * Keep zero visible when the function is close to zero,
         * which makes functions such as sin(x) easier to read.
         */
        if (_yMinimum > 0 && _yMinimum < range)
        {
            _yMinimum = 0;
        }

        if (_yMaximum < 0 && Math.Abs(_yMaximum) < range)
        {
            _yMaximum = 0;
        }
    }

    private void BuildGridAndAxes()
    {
        _gridPlots.Clear();
        _axisPlots.Clear();

        double xStep =
            CalculateNiceStep(_xMinimum, _xMaximum);

        double yStep =
            CalculateNiceStep(_yMinimum, _yMaximum);

        double firstX =
            Math.Ceiling(_xMinimum / xStep) * xStep;

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
            Math.Ceiling(_yMinimum / yStep) * yStep;

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

        if (_xMinimum <= 0 && _xMaximum >= 0)
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

        if (_yMinimum <= 0 && _yMaximum >= 0)
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
        var plot = new Plot2D(
            new PlotStyle
            {
                Lines = true,
                Points = false,
                Width = width,
                Rgb = color
            });

        plot.Points.Add(
            new NumericsVector2(x1, y1));

        plot.Points.Add(
            new NumericsVector2(x2, y2));

        return plot;
    }

    private static Func<double, double> ParseFunction(
        string expression)
    {
        string normalized = expression
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", string.Empty);

        return normalized switch
        {
            "sin(x)" or "sin" =>
                System.Math.Sin,

            "cos(x)" or "cos" =>
                System.Math.Cos,

            "tan(x)" or "tan" =>
                System.Math.Tan,

            "x" =>
                x => x,

            "x^2" or "x*x" =>
                x => x * x,

            "x^3" or "x*x*x" =>
                x => x * x * x,

            "abs(x)" =>
                System.Math.Abs,

            "sqrt(x)" =>
                System.Math.Sqrt,

            "1/x" =>
                x => 1.0 / x,

            _ => throw new NotSupportedException(
                "Supported functions: sin(x), cos(x), tan(x), x, x^2, x^3, abs(x), sqrt(x), and 1/x.")
        };
    }

    private static double CalculateNiceStep(
        double minimum,
        double maximum)
    {
        double span =
            Math.Max(0.000001, maximum - minimum);

        double roughStep = span / 10.0;

        double magnitude =
            Math.Pow(
                10,
                Math.Floor(
                    Math.Log10(roughStep)));

        double normalized = roughStep / magnitude;

        double niceValue = normalized switch
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