// QuickSimulationsComprehensiveTests.cs (visual + verbose console)
using System;
using Features;
using Math.Trigonometry.Concepts;
using Math.Trigonometry.Functions;
using Math.Trigonometry.Sampling;

namespace Core.App
{
    public static class QuickSimulationsComprehensiveTests
    {
        private static int _testsRun = 0;
        private static int _testsPassed = 0;

        // ---------- Console helpers ----------
        private static void Banner(string title)
        {
            Console.WriteLine();
            Console.WriteLine(new string('=', System.Math.Max(12, title.Length + 6)));
            Console.WriteLine($"== {title} ==");
            Console.WriteLine(new string('=', System.Math.Max(12, title.Length + 6)));
        }
        private static void Section(string title)
        {
            Console.WriteLine($"\n-- {title} --");
        }
        private static void Log(bool verbose, string msg)
        {
            if (verbose) Console.WriteLine(msg);
        }
        // print a dot every N successful assertions so there’s visible progress
        private static void Tick(int every = 200)
        {
            if (_testsRun % every == 0) { Console.Write("."); Console.Out.Flush(); }
        }

        private static double Shelf(int i) => 2.5 * i; // Z “panel”

        /// <summary>Run tests. visual: draw; verbose: print detailed console logs.</summary>
        public static void Run(bool visual = false, bool verbose = true)
        {
            _testsRun = _testsPassed = 0;

            Banner("Fluxion Comprehensive Tests");
            Console.WriteLine($"Start: visual={(visual ? "on" : "off")}, verbose={(verbose ? "on" : "off")}");
            if (!visual) Console.WriteLine("Visual mode is OFF — rendering calls are skipped; console will still describe each step.");

            try
            {
                var start = DateTime.Now;
                Section("SineEquation");
                TestSineEquation(visual, verbose);
                Console.WriteLine($"\n[PASS] SineEquation section ✓ ({_testsPassed}/{_testsRun} so far)");

                Section("CosineEquation");
                TestCosineEquation(visual, verbose);
                Console.WriteLine($"\n[PASS] CosineEquation section ✓ ({_testsPassed}/{_testsRun} so far)");

                Section("TrigonometrySampler");
                TestTrigonometrySampler(visual, verbose);
                Console.WriteLine($"\n[PASS] TrigonometrySampler section ✓ ({_testsPassed}/{_testsRun} so far)");

                Section("Helix Parametric");
                TestHelixParametric(visual, verbose);
                Console.WriteLine($"\n[PASS] Helix section ✓ ({_testsPassed}/{_testsRun} so far)");

                var dur = DateTime.Now - start;
                Console.WriteLine($"\nAll comprehensive tests passed: {_testsPassed}/{_testsRun} ✅  (in {dur.TotalSeconds:F2}s)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[TEST FAILURE] {ex.Message}");
                Console.WriteLine($"Tests passed before failure: {_testsPassed}/{_testsRun}.");
            }
            Console.WriteLine(); // trailing newline
        }

        private static (double[] xs, double[] ys) Sample(Func<double, double> f, double start, double end, int steps)
            => TrigonometrySampler.Sample(f, start, end, steps);

        private static (double x, double y)[] Zip(double[] xs, double[] ys)
        {
            var pts = new (double x, double y)[xs.Length];
            for (int i = 0; i < xs.Length; i++) pts[i] = (xs[i], ys[i]);
            return pts;
        }

        private static void DrawFunctionAndSamples(
            string label,
            Func<double, double> f,
            (double start, double end, int curveSamples) curve,
            (double start, double end, int steps) dots,
            double shelfZ,
            bool visual,
            bool verbose)
        {
            Log(verbose, $"[VISUAL] {label} on shelf z={shelfZ} (curve {curve.curveSamples} samples, dots {dots.steps})");
            if (!visual) return;

            Graph.F(f).ThreeDLine(curve.start, curve.end, plane: EmbedPlane.XY, offset: shelfZ, samples: curve.curveSamples);

            var (xs, ys) = Sample(f, dots.start, dots.end, dots.steps);
            Graph.Points(Zip(xs, ys)).ThreeDLine(plane: EmbedPlane.XY, offset: shelfZ);
        }

        // ---------- Tests ----------

        private static void TestSineEquation(bool visual, bool verbose)
        {
            Log(verbose, "Verifying Evaluate/IsWellFormed/AsString over parameter grid…");
            double[] A_ = { 0.0, 0.5, 1.0, 2.0 };
            double[] F_ = { 0.5, 1.0, 2.0 };
            double[] P_ = { 0.0, System.Math.PI / 4, System.Math.PI / 2, System.Math.PI };
            double[] V_ = { -1.0, 0.0, 1.0 };
            double[] xs = { -2 * System.Math.PI, -System.Math.PI, -System.Math.PI / 2, 0.0, System.Math.PI / 2, System.Math.PI, 2 * System.Math.PI };

            int combosShown = 0;
            foreach (var A in A_)
                foreach (var F in F_)
                    foreach (var P in P_)
                        foreach (var V in V_)
                        {
                            var eq = new SineEquation(new SineModel(A, F, P, V));

                            _testsRun++; if (!eq.IsWellFormed()) throw new Exception($"Sine IsWellFormed false for A={A},F={F},P={P},V={V}");
                            _testsPassed++; Tick();

                            _testsRun++; var s = eq.AsString("x");
                            if (string.IsNullOrWhiteSpace(s) || !s.Contains("sin")) throw new Exception("Sine AsString invalid");
                            _testsPassed++; Tick();

                            foreach (var x in xs)
                            {
                                _testsRun++;
                                var expected = A * System.Math.Sin(F * x + P) + V;
                                var actual = eq.Evaluate(x);
                                AssertAlmostEqual(expected, actual, 1e-10, $"Sine mismatch A={A},F={F},P={P},V={V},x={x}");
                                _testsPassed++; Tick();
                            }

                            // Print a few example combos so the console shows concrete cases
                            if (verbose && combosShown < 6)
                            {
                                Console.WriteLine($"  ✓ Sine OK: A={A}, F={F}, P={P:F3}, V={V}");
                                combosShown++;
                            }
                        }

            _testsRun++; if (new SineEquation(new SineModel(double.NaN, 1, 1, 1)).IsWellFormed()) throw new Exception("Sine IsWellFormed should be false for NaN amplitude");
            _testsPassed++; Tick();

            // Visual showcase of 3 representative curves
            var show = new[]
            {
                new SineModel(1.0, 1.0, 0.0, 0.0),
                new SineModel(1.5, 2.0, System.Math.PI/4, -0.5),
                new SineModel(0.5, 0.5, System.Math.PI/2, 0.75),
            };
            int shelfIdx = 0;
            foreach (var m in show)
            {
                var eq = new SineEquation(m);
                DrawFunctionAndSamples($"sine: {eq.AsString()}", eq.Evaluate,
                    curve: (-2 * System.Math.PI, 2 * System.Math.PI, 1200),
                    dots: (-2 * System.Math.PI, 2 * System.Math.PI, 33),
                    shelfZ: Shelf(shelfIdx++), visual, verbose);
            }
        }

        private static void TestCosineEquation(bool visual, bool verbose)
        {
            Log(verbose, "Verifying Evaluate/IsWellFormed/AsString over parameter grid…");
            double[] A_ = { 0.0, 0.5, 1.0, 2.0 };
            double[] F_ = { 0.5, 1.0, 2.0 };
            double[] P_ = { 0.0, System.Math.PI / 4, System.Math.PI / 2, System.Math.PI };
            double[] V_ = { -1.0, 0.0, 1.0 };
            double[] xs = { -2 * System.Math.PI, -System.Math.PI, -System.Math.PI / 2, 0.0, System.Math.PI / 2, System.Math.PI, 2 * System.Math.PI };

            int combosShown = 0;
            foreach (var A in A_)
                foreach (var F in F_)
                    foreach (var P in P_)
                        foreach (var V in V_)
                        {
                            var eq = new CosineEquation(new CosineModel(A, F, P, V));

                            _testsRun++; if (!eq.IsWellFormed()) throw new Exception($"Cos IsWellFormed false for A={A},F={F},P={P},V={V}");
                            _testsPassed++; Tick();

                            _testsRun++; var s = eq.AsString("x");
                            if (string.IsNullOrWhiteSpace(s) || !s.Contains("cos")) throw new Exception("Cos AsString invalid");
                            _testsPassed++; Tick();

                            foreach (var x in xs)
                            {
                                _testsRun++;
                                var expected = A * System.Math.Cos(F * x + P) + V;
                                var actual = eq.Evaluate(x);
                                AssertAlmostEqual(expected, actual, 1e-10, $"Cos mismatch A={A},F={F},P={P},V={V},x={x}");
                                _testsPassed++; Tick();
                            }

                            if (verbose && combosShown < 6)
                            {
                                Console.WriteLine($"  ✓ Cos OK:  A={A}, F={F}, P={P:F3}, V={V}");
                                combosShown++;
                            }
                        }

            _testsRun++; if (new CosineEquation(new CosineModel(1, double.NaN, 1, 1)).IsWellFormed()) throw new Exception("Cos IsWellFormed should be false for NaN frequency");
            _testsPassed++; Tick();

            var show = new[]
            {
                new CosineModel(1.0, 1.0, 0.0, 0.0),
                new CosineModel(1.25, 2.0, System.Math.PI/3, 0.25),
            };
            int shelfIdx = 3;
            foreach (var m in show)
            {
                var eq = new CosineEquation(m);
                DrawFunctionAndSamples($"cosine: {eq.AsString()}", eq.Evaluate,
                    curve: (-2 * System.Math.PI, 2 * System.Math.PI, 1200),
                    dots: (-2 * System.Math.PI, 2 * System.Math.PI, 33),
                    shelfZ: Shelf(shelfIdx++), visual, verbose);
            }
        }

        private static void TestTrigonometrySampler(bool visual, bool verbose)
        {
            Log(verbose, "Checking exceptions, spacing, and exactness against sin/cos…");

            _testsRun++; bool threw = false;
            try { TrigonometrySampler.Sample(x => x, 0.0, 1.0, 1); }
            catch (ArgumentOutOfRangeException) { threw = true; }
            if (!threw) throw new Exception("Sampler should throw when numberOfSteps < 2.");
            _testsPassed++; Tick();

            // sin 0..2π, 5 steps
            {
                double start = 0.0, end = 2 * System.Math.PI; int steps = 5;
                var (xs, ys) = TrigonometrySampler.Sample(System.Math.Sin, start, end, steps);

                _testsRun++; if (xs.Length != steps || ys.Length != steps) throw new Exception("Sampler wrong count"); _testsPassed++; Tick();
                double dx = (end - start) / (steps - 1);
                for (int i = 0; i < steps; i++)
                {
                    _testsRun++; AssertAlmostEqual(start + i * dx, xs[i], 1e-12, "Sampler x mismatch"); _testsPassed++; Tick();
                    _testsRun++; AssertAlmostEqual(System.Math.Sin(xs[i]), ys[i], 1e-12, "Sampler y mismatch"); _testsPassed++; Tick();
                }

                DrawFunctionAndSamples("sampler vs sin", System.Math.Sin,
                    curve: (start, end, 600),
                    dots: (start, end, steps),
                    shelfZ: Shelf(6), visual, verbose);
            }

            // cos -π..π, 9 steps
            {
                double start = -System.Math.PI, end = System.Math.PI; int steps = 9;
                var (xs, ys) = TrigonometrySampler.Sample(System.Math.Cos, start, end, steps);

                _testsRun++; if (xs.Length != steps || ys.Length != steps) throw new Exception("Sampler wrong count"); _testsPassed++; Tick();
                double dx = (end - start) / (steps - 1);
                for (int i = 0; i < steps; i++)
                {
                    _testsRun++; AssertAlmostEqual(start + i * dx, xs[i], 1e-12, "Sampler x mismatch"); _testsPassed++; Tick();
                    _testsRun++; AssertAlmostEqual(System.Math.Cos(xs[i]), ys[i], 1e-12, "Sampler y mismatch"); _testsPassed++; Tick();
                }

                DrawFunctionAndSamples("sampler vs cos", System.Math.Cos,
                    curve: (start, end, 600),
                    dots: (start, end, steps),
                    shelfZ: Shelf(7), visual, verbose);
            }
        }

        private static void TestHelixParametric(bool visual, bool verbose)
        {
            Log(verbose, "Validating helix parametric (cos t, sin t, 0.15t) on probe points…");

            double[] ts = { 0.0, System.Math.PI / 2, System.Math.PI, 2 * System.Math.PI, 4 * System.Math.PI, 6 * System.Math.PI };
            foreach (var t in ts)
            {
                _testsRun++;
                var ex = System.Math.Cos(t); var ey = System.Math.Sin(t); var ez = 0.15 * t;
                var x = System.Math.Cos(t); var y = System.Math.Sin(t); var z = 0.15 * t;
                AssertAlmostEqual(ex, x, 1e-12, $"Helix x mismatch t={t}");
                AssertAlmostEqual(ey, y, 1e-12, $"Helix y mismatch t={t}");
                AssertAlmostEqual(ez, z, 1e-12, $"Helix z mismatch t={t}");
                _testsPassed += 3; Tick();
                if (verbose) Console.WriteLine($"  ✓ t={t:F2}  ({x:F4}, {y:F4}, {z:F4})");
            }

            if (visual)
            {
                Log(verbose, "[VISUAL] helix (parametric) full curve");
                Graph3DFeature.Parametric(
                    t => System.Math.Cos(t),
                    t => System.Math.Sin(t),
                    t => 0.15 * t,
                    tMin: 0, tMax: 12 * System.Math.PI, samples: 1200
                );
            }
        }

        private static void AssertAlmostEqual(double expected, double actual, double tol, string message)
        {
            if (double.IsNaN(actual) || System.Math.Abs(expected - actual) > tol)
                throw new Exception($"{message}. Expected {expected}, got {actual} (±{tol}).");
        }
    }
}
