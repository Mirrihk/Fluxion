// Fluxion.Rendering/Draw/OpenGLRenderer.cs
using OpenTK.Graphics.OpenGL4;
using System;
using System.Numerics;

namespace Fluxion.Rendering.Draw
{
    /// <summary>
    /// Minimal OpenGL implementation of IRenderer.
    /// Assumes incoming points are in NDC (-1..1). Use a visualizer to normalize world coords.
    /// </summary>
    public sealed class OpenGLRenderer : IRenderer, IDisposable
    {
        private int _program = 0;
        private int _vao = 0; //Ve
        private int _vbo = 0;
        private int _uColor = -1;

        // lazy init: valid when GL context is current (inside FluxWindow Render/Load)
        private void EnsureInit()
        {
            if (_program != 0) return;

            // Shaders
            const string vs = @"#version 330 core
                layout (location = 0) in vec2 aPos;
                void main() { gl_Position = vec4(aPos, 0.0, 1.0); }";

            const string fs = @"#version 330 core
                uniform vec4 uColor;
                out vec4 FragColor;
                void main() { FragColor = uColor; }";

            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs);
            GL.CompileShader(v);
            GL.GetShader(v, ShaderParameter.CompileStatus, out int vOk);
            if (vOk == 0) throw new Exception(GL.GetShaderInfoLog(v));

            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fs);
            GL.CompileShader(f);
            GL.GetShader(f, ShaderParameter.CompileStatus, out int fOk);
            if (fOk == 0) throw new Exception(GL.GetShaderInfoLog(f));

            _program = GL.CreateProgram();
            GL.AttachShader(_program, v);
            GL.AttachShader(_program, f);
            GL.LinkProgram(_program);
            GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out int pOk);
            if (pOk == 0) throw new Exception(GL.GetProgramInfoLog(_program));

            GL.DeleteShader(v);
            GL.DeleteShader(f);

            _uColor = GL.GetUniformLocation(_program, "uColor");

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, 0, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);

            GL.BindVertexArray(0);
        }

        public void Clear(float r, float g, float b, float a = 1f)
        {
            EnsureInit();
            GL.ClearColor(r, g, b, a);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void DrawLines(ReadOnlySpan<Vector2> pts, float width = 1f)
        {
            if (pts.Length < 2) return;
            EnsureInit();

            GL.UseProgram(_program);
            GL.Uniform4(_uColor, 1f, 1f, 1f, 1f); // white
            GL.LineWidth(MathF.Max(1f, width));

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            // SAFE upload: flatten to float[] (x,y) pairs
            var floats = new float[pts.Length * 2];
            for (int i = 0, j = 0; i < pts.Length; i++)
            {
                floats[j++] = pts[i].X;
                floats[j++] = pts[i].Y;
            }

            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * floats.Length, floats, BufferUsageHint.DynamicDraw);
            GL.DrawArrays(PrimitiveType.LineStrip, 0, pts.Length);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public void DrawPoints(ReadOnlySpan<Vector2> pts, float size = 2f)
        {
            if (pts.Length < 1) return;
            EnsureInit();

            GL.UseProgram(_program);
            GL.Uniform4(_uColor, 1f, 1f, 1f, 1f); // white
            GL.PointSize(MathF.Max(1f, size));

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            var floats = new float[pts.Length * 2];
            for (int i = 0, j = 0; i < pts.Length; i++)
            {
                floats[j++] = pts[i].X;
                floats[j++] = pts[i].Y;
            }

            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * floats.Length, floats, BufferUsageHint.DynamicDraw);
            GL.DrawArrays(PrimitiveType.Points, 0, pts.Length);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public void Dispose()
        {
            if (_vbo != 0) GL.DeleteBuffer(_vbo);
            if (_vao != 0) GL.DeleteVertexArray(_vao);
            if (_program != 0) GL.DeleteProgram(_program);
            _vbo = _vao = _program = 0;
        }
    }
}
