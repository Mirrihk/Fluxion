using System.Numerics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Fluxion.Rendering.Visualize2D;

namespace Fluxion.Rendering.Draw
{
    /// <summary>
    /// Renders 2D curves (lines/points) from Plot2D using an orthographic projection.
    /// </summary>
    public sealed class CurveRenderer2D : IDisposable
    {
        private readonly int _vao, _vbo, _shader;
        private readonly int _locMvp, _locColor;
        private int _cachedCapacityFloats = 0;

        private const string VS = @"#version 330 core
            layout (location = 0) in vec2 aPos;
            uniform mat4 uMVP;
            void main() { gl_Position = uMVP * vec4(aPos, 0.0, 1.0); }";

        private const string FS = @"#version 330 core
            uniform vec3 uColor;
            out vec4 FragColor;
            void main() { FragColor = vec4(uColor, 1.0); }";

        public CurveRenderer2D()
        {
            _shader = Compile(VS, FS);
            _locMvp = GL.GetUniformLocation(_shader, "uMVP");
            _locColor = GL.GetUniformLocation(_shader, "uColor");

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Draw(Plot2D plot, Matrix4 projection)
        {
            int n = plot.Points.Count;
            if (n == 0) return;

            // Flatten Plot2D points into float[]
            var needed = n * 2;
            var verts = new float[needed];
            for (int i = 0, j = 0; i < n; i++)
            {
                var p = plot.Points[i];
                verts[j++] = p.X;
                verts[j++] = p.Y;
            }

            GL.UseProgram(_shader);
            GL.UniformMatrix4(_locMvp, false, ref projection);

            OpenTK.Mathematics.Vector3 rgb = plot.Style.Rgb.HasValue
                  ? new OpenTK.Mathematics.Vector3(plot.Style.Rgb.Value.X, plot.Style.Rgb.Value.Y, plot.Style.Rgb.Value.Z)
                : new OpenTK.Mathematics.Vector3(1f, 1f, 1f);

            GL.Uniform3(_locColor, rgb.X, rgb.Y, rgb.Z);

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            if (needed > _cachedCapacityFloats)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, needed * sizeof(float), nint.Zero, BufferUsageHint.DynamicDraw);
                _cachedCapacityFloats = needed;
            }
            GL.BufferSubData(BufferTarget.ArrayBuffer, nint.Zero, needed * sizeof(float), verts);

            if (plot.Style.Lines && n >= 2)
            {
                GL.LineWidth(plot.Style.Width <= 0 ? 1f : plot.Style.Width);
                GL.DrawArrays(PrimitiveType.LineStrip, 0, n);
            }
            if (plot.Style.Points)
            {
                GL.PointSize(plot.Style.Width <= 0 ? 1f : plot.Style.Width);
                GL.DrawArrays(PrimitiveType.Points, 0, n);
            }

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        private static int Compile(string vs, string fs)
        {
            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs);
            GL.CompileShader(v);

            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fs);
            GL.CompileShader(f);

            int p = GL.CreateProgram();
            GL.AttachShader(p, v);
            GL.AttachShader(p, f);
            GL.LinkProgram(p);

            GL.DeleteShader(v);
            GL.DeleteShader(f);
            return p;
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteProgram(_shader);
        }
    }
}
