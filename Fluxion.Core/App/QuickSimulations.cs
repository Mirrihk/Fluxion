// Fluxion.Core/App/QuickSimulations.cs
using System;
using System.Diagnostics;
using Fluxion.Features;

// Trigonometry dependencies
using Fluxion.Math.Trigonometry.Concepts;
using Fluxion.Math.Trigonometry.Functions;
using Fluxion.Math.Trigonometry.Sampling;

namespace Fluxion.Core.App
{
    /// <summary>Reusable demo calls you can invoke from Program.Main or the CLI.</summary>
    public static class QuickSimulations
    {
        // Add inside Fluxion.Core.App.QuickSimulations
        public static void RunAll(bool waitBetween = true)
        {
            var demos = new (string name, Action call)[]
            {
                ("sin2d",        Sin2D),
                ("sinradial3d",  SinRadialSurface3D),
                ("sin3dline",    Sin3DLine),
                ("line2d",       () => Line2D()),
                ("line3d",       () => Line3DOnXZ(offsetY: 1.0)),
                ("polyline3d",   Polyline3D_XY_Z0),
                ("sxcysurface",  () => SinCosSurface3D(wireframe: true)),
                ("helix",        Helix3D),

                // New: Trigonometry-based demos + self-tests
                ("sine2d_trig",  Sine2D_Trigonometry),
                ("cosine2d_trig",Cosine2D_Trigonometry),
                ("trigtests",    TrigonometrySelfTests),
            };

            foreach (var (name, call) in demos)
            {
                Console.WriteLine($"\n=== {name} ===");
                try { call(); }
                catch (Exception ex) { Console.WriteLine($"[ERROR] {name}: {ex.Message}"); }
                if (waitBetween) Pause();
            }
        }

        private static void Pause()
        {
            Console.Write("Press Enter for next demo...");
            Console.ReadLine();
        }

        // ---------------- Existing demos ----------------

        public static void Sin2D()
        {
            Graph.F(System.Math.Sin)
                 .TwoD(-2 * System.Math.PI, 2 * System.Math.PI, samples: 1200);
        }

        public static void SinRadialSurface3D()
        {
            Graph.F(System.Math.Sin)
                 .ThreeD(-8, 8, -8, 8, lift: Lift.Radial, resolution: 180, wireframe: false);
        }

        public static void Sin3DLine()
        {
            Graph.F(System.Math.Sin)
                 .ThreeDLine(-2 * System.Math.PI, 2 * System.Math.PI, plane: EmbedPlane.XY, offset: 0, samples: 1000);
        }

        public static void Line2D(double m = 1.2, double b = -0.5)
        {
            Graph.Line(m, b).TwoD(-10, 10, samples: 600);
        }

        public static void Line3DOnXZ(double m = 1.2, double b = -0.5, double offsetY = 1.0)
        {
            Graph.Line(m, b).ThreeD(-10, 10, plane: EmbedPlane.XZ, offset: offsetY, samples: 600);
        }

        public static void Polyline3D_XY_Z0()
        {
            Graph.Points(
                (-3.0, -1.2), (-2.0, -0.2), (-1.0, 0.8),
                (0.0, 0.0), (1.0, 1.1), (2.0, 1.7), (3.0, 2.2)
            ).ThreeDLine(plane: EmbedPlane.XY, offset: 0);
        }

        public static void SinCosSurface3D(bool wireframe = true)
        {
            Graph3DFeature.Surface(
                (x, y) => System.Math.Sin(x) * System.Math.Cos(y),
                -2 * System.Math.PI, 2 * System.Math.PI,
                -2 * System.Math.PI, 2 * System.Math.PI,
                resolution: 180, wireframe: wireframe);
        }

        public static void Helix3D()
        {
            Graph3DFeature.Parametric(
                t => System.Math.Cos(t),
                t => System.Math.Sin(t),
                t => 0.15 * t,
                tMin: 0, tMax: 12 * System.Math.PI, samples: 1200);
        }

        // ---------------- New: Trigonometry-based demos ----------------

        /// <summary>
        /// Renders f(x) = Amplitude * sin(Frequency * x + Phase) + VerticalOffset
        /// using your Trigonometry models + sampler, then pipes points into the Graph API.
        /// </summary>
        public static void Sine2D_Trigonometry()
        {
            var equation = new SineEquation(new SineModel(
                Amplitude: 1.0, Frequency: 1.0, Phase: 0.0, VerticalOffset: 0.0));

            var (xs, ys) = TrigonometrySampler.Sample(
                equation.Evaluate,
                start: -2 * System.Math.PI,
                end: 2 * System.Math.PI,
                numberOfSteps: 1200);

            // zip arrays -> tuples
            var points = new (double x, double y)[xs.Length];
            for (int i = 0; i < xs.Length; i++)
                points[i] = (xs[i], ys[i]);

            Graph.Points(points).ThreeDLine(plane: EmbedPlane.XY, offset: 0);
        }

        /// <summary>
        /// Renders f(x) = Amplitude * cos(Frequency * x + Phase) + VerticalOffset
        /// using your Trigonometry models + sampler, then pipes points into the Graph API.
        /// </summary>
        public static void Cosine2D_Trigonometry()
        {
            var equation = new CosineEquation(new CosineModel(
                Amplitude: 1.0, Frequency: 1.0, Phase: 0.0, VerticalOffset: 0.0));

            var (xs, ys) = TrigonometrySampler.Sample(
                equation.Evaluate,
                start: -2 * System.Math.PI,
                end: 2 * System.Math.PI,
                numberOfSteps: 1200);

            var points = new (double x, double y)[xs.Length];
            for (int i = 0; i < xs.Length; i++)
                points[i] = (xs[i], ys[i]);

            Graph.Points(points).ThreeDLine(plane: EmbedPlane.XY, offset: 0);
        }

        // ---------------- Built-in self-tests (CLI) ----------------

        /// <summary>
        /// Quick verifications without xUnit. Run with: dotnet run trigtests
        /// Throws on failure; prints OKs on success.
        /// </summary>
        public static void TrigonometrySelfTests()
        {
            Console.WriteLine("[SelfTest] SineEquation_Basic");
            {
                var equation = new SineEquation(new SineModel(1, 1, 0, 0));
                AssertAlmostEqual(0.0, equation.Evaluate(0.0), 1e-10, "sin(0) should be 0");
                AssertAlmostEqual(1.0, equation.Evaluate(System.Math.PI / 2), 1e-10, "sin(pi/2) should be 1");
            }

            Console.WriteLine("[SelfTest] CosineEquation_Basic");
            {
                var equation = new CosineEquation(new CosineModel(1, 1, 0, 0));
                AssertAlmostEqual(1.0, equation.Evaluate(0.0), 1e-10, "cos(0) should be 1");
                AssertAlmostEqual(-1.0, equation.Evaluate(System.Math.PI), 1e-10, "cos(pi) should be -1");
            }

            Console.WriteLine("[SelfTest] TrigonometrySampler_Sanity");
            {
                var equation = new CosineEquation(new CosineModel(1, 1, 0, 0));
                var (xs, ys) = TrigonometrySampler.Sample(equation.Evaluate, 0, System.Math.PI, 3);
                if (xs.Length != 3 || ys.Length != 3) throw new Exception("Sampler should return 3 samples.");
                AssertAlmostEqual(1.0, ys[0], 1e-10, "cos(0) should be 1");
                AssertAlmostEqual(0.0, ys[1], 1e-10, "cos(pi/2) should be 0");
                AssertAlmostEqual(-1.0, ys[2], 1e-10, "cos(pi) should be -1");
            }

            Console.WriteLine("All trigonometry self-tests passed ✅");
        }

        private static void AssertAlmostEqual(double expected, double actual, double tol, string messageIfFail)
        {
            if (double.IsNaN(actual) || System.Math.Abs(expected - actual) > tol)
                throw new Exception($"{messageIfFail}. Expected {expected}, got {actual} (±{tol}).");
        }

        // ---------------- Menu helpers ----------------

        public static void PrintMenu()
        {
            Console.WriteLine("QuickSimulations (run with: dotnet run <name>):");
            Console.WriteLine("  sin2d | sinradial3d | sin3dline | line2d | line3d | polyline3d | sxcysurface | helix");
            Console.WriteLine("  sine2d_trig | cosine2d_trig | trigtests");
        }

        public static bool TryRun(string name)
        {
            switch (name.Trim().ToLowerInvariant())
            {
                case "sin2d": Sin2D(); return true;
                case "sinradial3d": SinRadialSurface3D(); return true;
                case "sin3dline": Sin3DLine(); return true;
                case "line2d": Line2D(); return true;
                case "line3d": Line3DOnXZ(offsetY: 1.0); return true;
                case "polyline3d": Polyline3D_XY_Z0(); return true;
                case "sxcysurface": SinCosSurface3D(wireframe: true); return true;
                case "helix": Helix3D(); return true;

                case "sine2d_trig": Sine2D_Trigonometry(); return true;
                case "cosine2d_trig": Cosine2D_Trigonometry(); return true;
                case "trigtests": TrigonometrySelfTests(); return true;

                default: return false;
            }
        }
    }
}
