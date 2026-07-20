using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Fluxion.src.Expressions;
using Fluxion.src.Numerics.Functions3D;
using Fluxion.src.Plotting.Sampling;
using Fluxion.src.Rendering.Camera;
using Fluxion.src.Rendering.Draw;
using Fluxion.src.Rendering.Visualize2D;
using Fluxion.src.Rendering.Visualize3D;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Wpf;

using Math = System.Math;
using NumericsVector2 = System.Numerics.Vector2;
using NumericsVector3 = System.Numerics.Vector3;
using WpfPoint = System.Windows.Point;

namespace Fluxion.Desktop;

public partial class MainWindow : Window
{
    private enum GraphRenderMode
    {
        TwoD,
        ThreeD
    }

    private CurveRenderer2D? _curveRenderer;

    private readonly List<Plot2D> _functionPlots = new();
    private readonly List<Plot2D> _gridPlots = new();
    private readonly List<Plot2D> _axisPlots = new();

    private SurfaceRenderer? _surfaceRenderer;
    private Axis3DRenderer? _axis3DRenderer;
    private Mesh3D? _surfaceMesh;

    private readonly OrbitCamera _orbitCamera = new();

    private bool _surfaceUploadPending;
    private int _requestedAxisExtent = 10;
    private int _activeAxisExtent;

    private WpfPoint _lastMousePosition;
    private bool _isOrbiting;

    private GraphRenderMode _renderMode =
        GraphRenderMode.TwoD;

    private double _xMinimum = -10;
    private double _xMaximum = 10;
    private double _yMinimum = -1.25;
    private double _yMaximum = 1.25;

    private bool _isNumberLineMode;

    public MainWindow()
    {
        InitializeComponent();

        var settings =
            new GLWpfControlSettings
            {
                MajorVersion = 4,
                MinorVersion = 6,
                RenderContinuously = true
            };

        Viewport.Start(settings);

        Viewport.MouseLeftButtonDown +=
            Viewport_MouseLeftButtonDown;

        Viewport.MouseLeftButtonUp +=
            Viewport_MouseLeftButtonUp;

        Viewport.MouseMove +=
            Viewport_MouseMove;

        Viewport.MouseWheel +=
            Viewport_MouseWheel;

        Viewport.LostMouseCapture +=
            Viewport_LostMouseCapture;

        BuildPlot();
    }

    private void Viewport_Render(
        TimeSpan delta)
    {
        if (Viewport.ActualWidth <= 0 ||
            Viewport.ActualHeight <= 0)
        {
            return;
        }

        DpiScale dpi =
            VisualTreeHelper.GetDpi(
                Viewport);

        int width =
            Math.Max(
                1,
                (int)(
                    Viewport.ActualWidth *
                    dpi.DpiScaleX));

        int height =
            Math.Max(
                1,
                (int)(
                    Viewport.ActualHeight *
                    dpi.DpiScaleY));

        GL.Viewport(
            0,
            0,
            width,
            height);

        GL.ClearColor(
            0.055f,
            0.060f,
            0.075f,
            1.0f);

        GL.Clear(
            ClearBufferMask.ColorBufferBit |
            ClearBufferMask.DepthBufferBit);

        if (_renderMode ==
            GraphRenderMode.ThreeD)
        {
            Render3D(
                width,
                height);

            return;
        }

        Render2D();
    }

    private void Render2D()
    {
        GL.Disable(
            EnableCap.DepthTest);

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

        if (!_isNumberLineMode &&
            ShowGridBox.IsChecked == true)
        {
            foreach (Plot2D gridLine
                     in _gridPlots)
            {
                _curveRenderer.Draw(
                    gridLine,
                    projection);
            }
        }

        if (_isNumberLineMode ||
            ShowAxesBox.IsChecked == true)
        {
            foreach (Plot2D axisLine
                     in _axisPlots)
            {
                _curveRenderer.Draw(
                    axisLine,
                    projection);
            }
        }

        foreach (Plot2D functionPlot
                 in _functionPlots)
        {
            _curveRenderer.Draw(
                functionPlot,
                projection);
        }
    }

    private void Render3D(
        int width,
        int height)
    {
        GL.Enable(
            EnableCap.DepthTest);

        Ensure3DResources();

        if (_surfaceMesh is null ||
            _surfaceRenderer is null)
        {
            return;
        }

        if (_surfaceUploadPending)
        {
            _surfaceRenderer.Upload(
                _surfaceMesh);

            _surfaceUploadPending =
                false;
        }

        float aspect =
            width /
            (float)Math.Max(
                1,
                height);

        float farPlane =
            Math.Max(
                200.0f,
                (float)(
                    _orbitCamera.Distance *
                    10.0));

        Matrix4 model =
            Matrix4.Identity;

        Matrix4 view =
            _orbitCamera.GetViewMatrix();

        Matrix4 projection =
            _orbitCamera.GetProjectionMatrix(
                aspect,
                50.0f,
                0.05f,
                farPlane);

        Matrix4 mvp =
            model *
            view *
            projection;

        _axis3DRenderer?.Draw(
            mvp,
            showAxes:
                ShowAxesBox.IsChecked == true,
            showGrid:
                ShowGridBox.IsChecked == true);

        _surfaceRenderer.Draw(
            mvp,
            model,
            wireframe:
                WireframeBox.IsChecked == true);
    }

    private void Ensure3DResources()
    {
        _surfaceRenderer ??=
            new SurfaceRenderer();

        if (_axis3DRenderer is not null &&
            _activeAxisExtent ==
            _requestedAxisExtent)
        {
            return;
        }

        _axis3DRenderer?.Dispose();

        float gridStep =
            (float)CalculateNiceStep(
                -_requestedAxisExtent,
                _requestedAxisExtent);

        _axis3DRenderer =
            new Axis3DRenderer(
                _requestedAxisExtent,
                gridStep);

        _activeAxisExtent =
            _requestedAxisExtent;
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
        _functionPlots.Clear();
        _gridPlots.Clear();
        _axisPlots.Clear();

        _surfaceMesh = null;
        _surfaceUploadPending = false;

        _isNumberLineMode = false;

        StatusText.Text =
            "Graph cleared";

        Viewport.InvalidateVisual();
    }

    private void ResetCamera_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_renderMode ==
                GraphRenderMode.ThreeD &&
            _surfaceMesh is not null)
        {
            FitCameraToMesh(
                _surfaceMesh);

            StatusText.Text =
                "3D camera reset";

            Viewport.InvalidateVisual();
            return;
        }

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
            if (GraphTypeBox.SelectedIndex == 1)
            {
                Build3DSurface();
                return;
            }

            Build2DOrScalar();
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

    private void Build2DOrScalar()
    {
        CompiledExpression expression =
            ExpressionEngine.Compile(
                FunctionBox.Text);

        if (expression.ContainsY)
        {
            throw new InvalidOperationException(
                "This expression uses y. " +
                "Select 3D Surface.");
        }

        _renderMode =
            GraphRenderMode.TwoD;

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

            BuildNumberLine(
                result);

            StatusText.Text =
                $"{FunctionBox.Text.Trim()} = " +
                result.ToString(
                    "G12",
                    CultureInfo.InvariantCulture);

            Viewport.InvalidateVisual();
            return;
        }

        _isNumberLineMode = false;

        ReadXBounds(
            out double xMinimum,
            out double xMaximum);

        _xMinimum = xMinimum;
        _xMaximum = xMaximum;

        int samples =
            Math.Max(
                25,
                (int)ResolutionSlider.Value);

        FunctionSampleResult sampling =
            FunctionSegmentSampler.Sample(
                expression.ToFunction(),
                _xMinimum,
                _xMaximum,
                samples);

        if (sampling.Segments.Count == 0)
        {
            throw new ArithmeticException(
                "The expression did not produce a drawable " +
                "real-valued curve in the selected range.");
        }

        _functionPlots.Clear();

        foreach (
            IReadOnlyList<NumericsVector2> segment
            in sampling.Segments)
        {
            var plot =
                new Plot2D(
                    CreateFunctionStyle());

            foreach (
                NumericsVector2 point
                in segment)
            {
                plot.Points.Add(
                    point);
            }

            _functionPlots.Add(
                plot);
        }

        CalculateYBounds();
        BuildGridAndAxes();

        string segmentWord =
            sampling.Segments.Count == 1
                ? "segment"
                : "segments";

        string breakText =
            sampling.BreakCount == 0
                ? "continuous"
                : $"{sampling.BreakCount} detected break(s)";

        StatusText.Text =
            $"Plotting y = {expression.Source} — " +
            $"{sampling.Segments.Count} {segmentWord}, " +
            breakText;

        Viewport.InvalidateVisual();
    }

    private void Build3DSurface()
    {
        ReadXBounds(
            out double minimum,
            out double maximum);

        CompiledExpression expression =
            ExpressionEngine.Compile(
                FunctionBox.Text);

        /*
         * For the first WPF 3D version, the X minimum and maximum
         * are used for both the mathematical x and y domains.
         */
        double xMinimum = minimum;
        double xMaximum = maximum;
        double yMinimum = minimum;
        double yMaximum = maximum;

        int gridResolution =
            Math.Clamp(
                (int)Math.Round(
                    ResolutionSlider.Value /
                    4.0),
                10,
                200);

        var scalarField =
            new DelegateScalarField(
                expression.ToSurface());

        Mesh3D mesh =
            SurfaceFactory.BuildSurface(
                scalarField,
                gridResolution,
                xMinimum,
                xMaximum,
                yMinimum,
                yMaximum);

        if (mesh.Indices.Length == 0)
        {
            throw new ArithmeticException(
                "The expression did not produce a drawable " +
                "3D surface in the selected range.");
        }

        _renderMode =
            GraphRenderMode.ThreeD;

        _isNumberLineMode = false;

        _functionPlots.Clear();
        _gridPlots.Clear();
        _axisPlots.Clear();

        _surfaceMesh =
            mesh;

        _surfaceUploadPending =
            true;

        _requestedAxisExtent =
            Math.Clamp(
                (int)Math.Ceiling(
                    Math.Max(
                        Math.Abs(minimum),
                        Math.Abs(maximum))),
                1,
                10_000);

        FitCameraToMesh(
            mesh);

        StatusText.Text =
            $"Plotting z = {expression.Source} — " +
            $"{gridResolution} × {gridResolution} surface. " +
            "Drag to orbit; use the mouse wheel to zoom.";

        Viewport.InvalidateVisual();
    }

    private static PlotStyle CreateFunctionStyle()
    {
        return new PlotStyle
        {
            Lines = true,
            Points = false,
            Width = 2.5f,
            Rgb = new NumericsVector3(
                0.20f,
                0.80f,
                1.00f)
        };
    }

    private void BuildNumberLine(
        double value)
    {
        _renderMode =
            GraphRenderMode.TwoD;

        _isNumberLineMode =
            true;

        double padding =
            Math.Max(
                1.0,
                Math.Abs(value) *
                0.20);

        if (Math.Abs(value) <
            0.000001)
        {
            _xMinimum = -5;
            _xMaximum = 5;
        }
        else
        {
            _xMinimum =
                Math.Min(
                    0.0,
                    value) -
                padding;

            _xMaximum =
                Math.Max(
                    0.0,
                    value) +
                padding;
        }

        _yMinimum = -1.0;
        _yMaximum = 1.0;

        _gridPlots.Clear();
        _axisPlots.Clear();
        _functionPlots.Clear();

        var axisColor =
            new NumericsVector3(
                0.85f,
                0.85f,
                0.85f);

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
                _xMinimum /
                tickStep) *
            tickStep;

        for (
            double tick = firstTick;
            tick <=
                _xMaximum +
                0.000001;
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

        var resultPoint =
            new Plot2D(
                new PlotStyle
                {
                    Lines = false,
                    Points = true,
                    Width = 12.0f,
                    Rgb =
                        new NumericsVector3(
                            0.20f,
                            0.80f,
                            1.00f)
                });

        resultPoint.Points.Add(
            new NumericsVector2(
                (float)value,
                0.0f));

        _functionPlots.Add(
            resultPoint);
    }

    private void CalculateYBounds()
    {
        double[] finiteValues =
            _functionPlots
                .SelectMany(
                    plot =>
                        plot.Points)
                .Select(
                    point =>
                        (double)point.Y)
                .Where(
                    value =>
                        double.IsFinite(value) &&
                        Math.Abs(value) <
                        1_000_000)
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
            maximum -
            minimum;

        if (range <
            0.000001)
        {
            double center =
                minimum;

            _yMinimum =
                center -
                1.0;

            _yMaximum =
                center +
                1.0;

            return;
        }

        Array.Sort(
            finiteValues);

        int lowerIndex =
            (int)Math.Floor(
                (finiteValues.Length - 1) *
                0.02);

        int upperIndex =
            (int)Math.Ceiling(
                (finiteValues.Length - 1) *
                0.98);

        double visibleMinimum =
            finiteValues[lowerIndex];

        double visibleMaximum =
            finiteValues[upperIndex];

        double visibleRange =
            visibleMaximum -
            visibleMinimum;

        if (visibleRange <
            0.000001)
        {
            visibleMinimum =
                minimum;

            visibleMaximum =
                maximum;

            visibleRange =
                range;
        }

        double padding =
            visibleRange *
            0.15;

        _yMinimum =
            visibleMinimum -
            padding;

        _yMaximum =
            visibleMaximum +
            padding;

        if (_yMinimum > 0 &&
            _yMinimum <
            visibleRange)
        {
            _yMinimum = 0;
        }

        if (_yMaximum < 0 &&
            Math.Abs(
                _yMaximum) <
            visibleRange)
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
                _xMinimum /
                xStep) *
            xStep;

        for (
            double x = firstX;
            x <=
                _xMaximum +
                0.000001;
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
                _yMinimum /
                yStep) *
            yStep;

        for (
            double y = firstY;
            y <=
                _yMaximum +
                0.000001;
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
        double span =
            Math.Max(
                0.000001,
                maximum -
                minimum);

        double roughStep =
            span /
            10.0;

        double magnitude =
            Math.Pow(
                10,
                Math.Floor(
                    Math.Log10(
                        roughStep)));

        double normalized =
            roughStep /
            magnitude;

        double niceValue =
            normalized switch
            {
                < 1.5 => 1,
                < 3.0 => 2,
                < 7.0 => 5,
                _ => 10
            };

        return
            niceValue *
            magnitude;
    }

    private void ReadXBounds(
        out double minimum,
        out double maximum)
    {
        if (!TryParseNumber(
                XMinBox.Text,
                out minimum))
        {
            throw new FormatException(
                "X minimum is not a valid number.");
        }

        if (!TryParseNumber(
                XMaxBox.Text,
                out maximum))
        {
            throw new FormatException(
                "X maximum is not a valid number.");
        }

        if (minimum >= maximum)
        {
            throw new ArgumentException(
                "X minimum must be smaller " +
                "than X maximum.");
        }
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

    private void FitCameraToMesh(
        Mesh3D mesh)
    {
        if (mesh.Positions.Length == 0)
        {
            _orbitCamera.Target =
                Vector3d.Zero;

            _orbitCamera.Distance =
                10.0;

            return;
        }

        Vector3 minimum =
            mesh.Positions[0];

        Vector3 maximum =
            mesh.Positions[0];

        foreach (
            Vector3 position
            in mesh.Positions)
        {
            minimum =
                Vector3.ComponentMin(
                    minimum,
                    position);

            maximum =
                Vector3.ComponentMax(
                    maximum,
                    position);
        }

        Vector3 center =
            (minimum +
             maximum) *
            0.5f;

        Vector3 size =
            maximum -
            minimum;

        double radius =
            Math.Max(
                1.0,
                size.Length *
                0.5);

        _orbitCamera.Target =
            new Vector3d(
                center.X,
                center.Y,
                center.Z);

        _orbitCamera.Distance =
            Math.Clamp(
                radius *
                2.5,
                3.0,
                1_000_000.0);

        _orbitCamera.Yaw =
            0.9;

        _orbitCamera.Pitch =
            0.55;
    }

    private void Viewport_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (_renderMode !=
            GraphRenderMode.ThreeD)
        {
            return;
        }

        _isOrbiting =
            true;

        _lastMousePosition =
            e.GetPosition(
                Viewport);

        Viewport.CaptureMouse();
        Viewport.Focus();

        e.Handled = true;
    }

    private void Viewport_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        StopOrbiting();
        e.Handled = true;
    }

    private void Viewport_LostMouseCapture(
        object sender,
        MouseEventArgs e)
    {
        _isOrbiting = false;
    }

    private void Viewport_MouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (!_isOrbiting ||
            _renderMode !=
            GraphRenderMode.ThreeD)
        {
            return;
        }

        WpfPoint currentPosition =
            e.GetPosition(
                Viewport);

        double deltaX =
            currentPosition.X -
            _lastMousePosition.X;

        double deltaY =
            currentPosition.Y -
            _lastMousePosition.Y;

        _lastMousePosition =
            currentPosition;

        _orbitCamera.Yaw +=
            deltaX *
            0.01;

        _orbitCamera.Pitch -=
            deltaY *
            0.01;

        Viewport.InvalidateVisual();
        e.Handled = true;
    }

    private void Viewport_MouseWheel(
        object sender,
        MouseWheelEventArgs e)
    {
        if (_renderMode !=
            GraphRenderMode.ThreeD)
        {
            return;
        }

        double wheelSteps =
            e.Delta /
            120.0;

        double zoomFactor =
            Math.Pow(
                0.85,
                wheelSteps);

        _orbitCamera.Distance =
            Math.Clamp(
                _orbitCamera.Distance *
                zoomFactor,
                0.25,
                1_000_000.0);

        Viewport.InvalidateVisual();
        e.Handled = true;
    }

    private void StopOrbiting()
    {
        if (!_isOrbiting)
        {
            return;
        }

        _isOrbiting = false;
        Viewport.ReleaseMouseCapture();
    }

    protected override void OnClosed(
        EventArgs e)
    {
        try
        {
            _surfaceRenderer?.Dispose();
            _axis3DRenderer?.Dispose();
        }
        finally
        {
            base.OnClosed(e);
        }
    }
}
