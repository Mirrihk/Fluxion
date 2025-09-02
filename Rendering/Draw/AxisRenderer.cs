// Fluxion.Rendering/Draw/AxisRenderer.cs
using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Fluxion.Rendering.Draw
{
    /// <summary>2D axes + grid + ticks (OpenGL lines). Uses the shared Axes2DOptions type.</summary>
    public sealed class AxesRenderer : IDisposable
    {
        private readonly int _vao, _vbo, _program;
        private readonly int _locMvp, _locColor;

        private const string VS = @"#version 330 core
layout (location = 0) in vec2 aPos;
uniform mat4 uMVP;
void main(){ gl_Position = uMVP * vec4(aPos, 0.0, 1.0); }";

        private const string FS = @"#version 330 core
uniform vec3 uColor;
out vec4 FragColor;
void main(){ FragColor = vec4(uColor, 1.0); }";

        // default palette
        private static readonly Vector3 MinorColor = new(0.25f, 0.25f, 0.30f);
        private static readonly Vector3 MajorColor = new(0.35f, 0.35f, 0.40f);
        private static readonly Vector3 AxisColor = new(0.85f, 0.85f, 0.85f);
        private static readonly Vector3 TickColor = new(0.85f, 0.85f, 0.85f);

        public AxesRenderer()
        {
            _program = CreateProgram(VS, FS);
            _locMvp = GL.GetUniformLocation(_program, "uMVP");
            _locColor = GL.GetUniformLocation(_program, "uColor");

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        /// <summary>Full grid/axes/ticks based on world bounds.</summary>
        public void DrawAxesGrid(Matrix4 projection,
                                 float xMin, float xMax,
                                 float yMin, float yMax,
                                 Axes2DOptions? opts = null)
        {
            opts ??= new Axes2DOptions();

            var minors = new List<float>(2048);
            var majors = new List<float>(1024);
            var axes = new List<float>(32);
            var ticks = new List<float>(512);

            // Choose “nice” steps (fixed target = 10 major lines per axis)
            const int targetMajors = 10;
            double stepX = NiceStep(xMin, xMax, targetMajors);
            double stepY = NiceStep(yMin, yMax, targetMajors);

            int minorDiv = System.Math.Max(2, opts.MinorDivisions);
            bool showMinor = opts.ShowMinor;
            bool showGrid = opts.ShowGrid;
            bool showTicks = opts.ShowTicks;
            double minorX = showMinor ? stepX / minorDiv : 0.0;
            double minorY = showMinor ? stepY / minorDiv : 0.0;

            // ---- GRID ----
            if (showGrid)
            {
                // verticals
                if (showMinor)
                {
                    foreach (var x in RangeWith0Safe(xMin, xMax, (float)minorX))
                        if (!NearlyMultiple(x, (float)stepX)) AddLine(minors, x, yMin, x, yMax);
                }
                foreach (var x in RangeWith0Safe(xMin, xMax, (float)stepX))
                    AddLine(majors, x, yMin, x, yMax);

                // horizontals
                if (showMinor)
                {
                    foreach (var y in RangeWith0Safe(yMin, yMax, (float)minorY))
                        if (!NearlyMultiple(y, (float)stepY)) AddLine(minors, xMin, y, xMax, y);
                }
                foreach (var y in RangeWith0Safe(yMin, yMax, (float)stepY))
                    AddLine(majors, xMin, y, xMax, y);
            }

            // ---- AXES ----
            if (xMin <= 0 && 0 <= xMax) AddLine(axes, 0, yMin, 0, yMax);   // Y axis
            if (yMin <= 0 && 0 <= yMax) AddLine(axes, xMin, 0, xMax, 0);   // X axis

            // ---- TICKS ----
            if (showTicks)
            {
                float tl = opts.TickLength <= 0 ? (xMax - xMin) * 0.01f : opts.TickLength;

                // ticks along X axis (vertical marks)
                if (yMin <= 0 && 0 <= yMax)
                {
                    foreach (var x in RangeWith0Safe(xMin, xMax, (float)stepX))
                        AddLine(ticks, x, -tl, x, +tl);
                }

                // ticks along Y axis (horizontal marks)
                if (xMin <= 0 && 0 <= xMax)
                {
                    foreach (var y in RangeWith0Safe(yMin, yMax, (float)stepY))
                        AddLine(ticks, -tl, y, +tl, y);
                }
            }

            // ---- render batches ----
            PrepareGL();

            if (minors.Count > 0) DrawBatch(projection, minors, opts.MinorGridWidth, MinorColor);
            if (majors.Count > 0) DrawBatch(projection, majors, opts.MajorGridWidth, MajorColor);
            if (axes.Count > 0) DrawBatch(projection, axes, opts.AxisWidth, AxisColor);
            if (ticks.Count > 0) DrawBatch(projection, ticks, opts.AxisWidth, TickColor);
        }

        /// <summary>Back-compat: simple cross (no grid).</summary>
        public void DrawAxes(Matrix4 projection, float range = 10f)
        {
            DrawAxesGrid(projection, -range, range, -range, range, new Axes2DOptions
            {
                ShowMinor = false,
                ShowGrid = false,
                ShowTicks = false
            });
        }

        // ----- helpers -----
        private void DrawBatch(Matrix4 mvp, List<float> verts, float width, Vector3 color)
        {
            if (verts.Count == 0) return;
            GL.UseProgram(_program);
            GL.UniformMatrix4(_locMvp, false, ref mvp);
            GL.Uniform3(_locColor, color);
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Count * sizeof(float), verts.ToArray(), BufferUsageHint.DynamicDraw);
            GL.LineWidth(System.Math.Max(1f, width));
            GL.DrawArrays(PrimitiveType.Lines, 0, verts.Count / 2);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        private static void AddLine(List<float> dst, float x1, float y1, float x2, float y2)
        { dst.Add(x1); dst.Add(y1); dst.Add(x2); dst.Add(y2); }

        private static void PrepareGL()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.LineSmooth);
        }

        private static double NiceStep(float min, float max, int target = 10)
        {
            var span = System.Math.Max(1e-9, max - min);
            var rough = span / target;
            var mag =  System.Math.Pow(10, System.Math.Floor(System.Math.Log10(rough)));
            var norm = rough / mag;                  // 1..10
            double nice = (norm < 1.5) ? 1 : (norm < 3) ? 2 : (norm < 7) ? 5 : 10;
            return nice * mag;
        }

        private static IEnumerable<float> RangeWith0Safe(float min, float max, float step)
        {
            if (step <= 0) yield break;
            var start = MathF.Ceiling(min / step) * step;
            for (float v = start; v <= max + 1e-6f; v += step)
                yield return MathF.Abs(v) < 1e-6f ? 0f : v;
        }

        private static bool NearlyMultiple(float v, float step)
        {
            var m = v / step;
            var nearest = MathF.Round(m);
            return MathF.Abs(m - nearest) < 1e-5f;
        }

        private static int CreateProgram(string vs, string fs)
        {
            int vsId = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vsId, vs);
            GL.CompileShader(vsId);

            int fsId = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fsId, fs);
            GL.CompileShader(fsId);

            int prog = GL.CreateProgram();
            GL.AttachShader(prog, vsId);
            GL.AttachShader(prog, fsId);
            GL.LinkProgram(prog);

            GL.DeleteShader(vsId);
            GL.DeleteShader(fsId);
            return prog;
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteProgram(_program);
        }
    }
}
