// Fluxion.Rendering/Draw/Axis3DRenderer.cs
using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Fluxion.Rendering.Draw
{
    /// <summary>
    /// Minimal 3D axes + grid (XY plane) renderer using OpenGL lines.
    /// - Draws X (red), Y (green), Z (blue) axes through the origin.
    /// - Draws a gray XY grid at Z = 0.
    /// </summary>
    public sealed class Axis3DRenderer : IDisposable
    {
        private readonly int _vao;
        private readonly int _vbo;
        private readonly int _program;
        private readonly int _uMvpLoc;
        private readonly int _vertexCount;

        public Axis3DRenderer(int halfExtent = 10, float step = 1f)
        {
            // --- Build geometry (interleaved: pos.xyz, color.rgb) ---
            var data = new List<float>(1024);

            // Colors
            var red   = (1f, 0f, 0f);
            var green = (0f, 1f, 0f);
            var blue  = (0f, 0f, 1f);
            var grid  = (0.25f, 0.25f, 0.30f);

            // Axes lines (-L..+L)
            int L = System.Math.Max(1, halfExtent);

            // X axis
            PushLine(data, new Vector3(-L, 0, 0), new Vector3(+L, 0, 0), red);
            // Y axis
            PushLine(data, new Vector3(0, -L, 0), new Vector3(0, +L, 0), green);
            // Z axis
            PushLine(data, new Vector3(0, 0, -L), new Vector3(0, 0, +L), blue);

            // XY grid (at Z = 0)
            if (step > 0f)
            {
                for (float x = -L; x <= L + 1e-4f; x += step)
                    PushLine(data, new Vector3(x, -L, 0), new Vector3(x, +L, 0), grid);

                for (float y = -L; y <= L + 1e-4f; y += step)
                    PushLine(data, new Vector3(-L, y, 0), new Vector3(+L, y, 0), grid);
            }

            _vertexCount = data.Count / 6;

            // --- GL objects ---
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Count * sizeof(float), data.ToArray(), BufferUsageHint.StaticDraw);

            // layout(location=0) vec3 aPos;
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            // layout(location=1) vec3 aColor;
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // --- Shader program ---
            var vs = @"
#version 330 core
layout(location=0) in vec3 aPos;
layout(location=1) in vec3 aColor;
uniform mat4 uMVP;
out vec3 vColor;
void main(){
    vColor = aColor;
    gl_Position = uMVP * vec4(aPos, 1.0);
}";
            var fs = @"
#version 330 core
in vec3 vColor;
out vec4 FragColor;
void main(){
    FragColor = vec4(vColor, 1.0);
}";
            _program = CreateProgram(vs, fs);
            _uMvpLoc = GL.GetUniformLocation(_program, "uMVP");
        }

        /// <summary>Draws axes+grid. Accepts the combined MVP matrix.</summary>
        public void Draw(Matrix4 mvp)
        {
            GL.UseProgram(_program);
            GL.UniformMatrix4(_uMvpLoc, false, ref mvp);

            // slightly thicker lines for axes; grid lines are in the same batch,
            // so set a uniform width that looks decent overall
            GL.LineWidth(1.2f);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Lines, 0, _vertexCount);
            GL.BindVertexArray(0);

            GL.UseProgram(0);
        }

        /// <summary>Back-compat with earlier call sites.</summary>
        public void DrawAxes(Matrix4 mvp) => Draw(mvp);

        public void Dispose()
        {
            if (_vbo != 0) GL.DeleteBuffer(_vbo);
            if (_vao != 0) GL.DeleteVertexArray(_vao);
            if (_program != 0) GL.DeleteProgram(_program);
        }

        // ---- helpers ----
        private static void PushLine(List<float> dst, Vector3 a, Vector3 b, (float r, float g, float bcol) c)
        {
            // A
            dst.Add(a.X); dst.Add(a.Y); dst.Add(a.Z);
            dst.Add(c.r); dst.Add(c.g); dst.Add(c.bcol);
            // B
            dst.Add(b.X); dst.Add(b.Y); dst.Add(b.Z);
            dst.Add(c.r); dst.Add(c.g); dst.Add(c.bcol);
        }

        private static int CreateProgram(string vsSrc, string fsSrc)
        {
            int vs = CompileShader(ShaderType.VertexShader, vsSrc);
            int fs = CompileShader(ShaderType.FragmentShader, fsSrc);
            int prog = GL.CreateProgram();
            GL.AttachShader(prog, vs);
            GL.AttachShader(prog, fs);
            GL.LinkProgram(prog);
            GL.GetProgram(prog, GetProgramParameterName.LinkStatus, out var ok);
            if (ok == 0)
            {
                var log = GL.GetProgramInfoLog(prog);
                GL.DeleteShader(vs); GL.DeleteShader(fs); GL.DeleteProgram(prog);
                throw new Exception("Axis3DRenderer link error: " + log);
            }
            GL.DetachShader(prog, vs); GL.DetachShader(prog, fs);
            GL.DeleteShader(vs); GL.DeleteShader(fs);
            return prog;
        }

        private static int CompileShader(ShaderType type, string src)
        {
            int sh = GL.CreateShader(type);
            GL.ShaderSource(sh, src);
            GL.CompileShader(sh);
            GL.GetShader(sh, ShaderParameter.CompileStatus, out var ok);
            if (ok == 0)
            {
                var log = GL.GetShaderInfoLog(sh);
                GL.DeleteShader(sh);
                throw new Exception($"Axis3DRenderer {type} compile error: {log}");
            }
            return sh;
        }
    }
}
