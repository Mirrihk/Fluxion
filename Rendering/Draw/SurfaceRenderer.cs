//Fuxion/Fluxion.Rendering/Draw/SurfaceRenderer.cs

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
namespace Fluxion.Rendering.Draw
{
   
    using global::Fluxion.Rendering.Visualize3D;

    public sealed class SurfaceRenderer : IDisposable
    {
        private int vao;      // Vertex Array Object
        private int vboPos;   // Vertex Buffer Object - positions
        private int vboNrm;   // Vertex Buffer Object - normals
        private int ebo;      // Element Buffer Object (indices)
        private int program;  // Compiled shader program

        private int indexCount;
        private bool isLineStrip;

        private const string VS = @"
            #version 330 core
            layout(location=0) in vec3 inPos;
            layout(location=1) in vec3 inNrm;
            uniform mat4 uMVP;
            uniform mat4 uModel;
            out vec3 vNrm;
            void main(){
            gl_Position = uMVP * vec4(inPos, 1.0);
            vNrm = mat3(uModel) * inNrm;
            }";
        private const string FS = @"
            #version 330 core
            in vec3 vNrm;
            out vec4 FragColor;
            uniform vec3 uLightDir = normalize(vec3(0.5, 1.0, 0.3));
            uniform vec3 uBase = vec3(0.2, 0.6, 1.0);
                void main(){
            float ndl = max(dot(normalize(vNrm), normalize(uLightDir)), 0.0);
             vec3 col = uBase * (0.35 + 0.65 * ndl);
             FragColor = vec4(col, 1.0);
            }";

        public SurfaceRenderer()
        {
            program = Compile(VS, FS);
            vao = GL.GenVertexArray();
            vboPos = GL.GenBuffer();
            vboNrm = GL.GenBuffer();
            ebo = GL.GenBuffer();
        }

        public void Upload(Mesh3D mesh, bool wireframeAsLines = false)
        {
            isLineStrip = wireframeAsLines && mesh.Indices.Length == 0 && mesh.Normals.Length == 0;

            GL.BindVertexArray(vao);

            // Positions
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboPos);
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.Positions.Length * 3 * sizeof(float), mesh.Positions, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Normals (optional for polylines)
            if (mesh.Normals.Length == mesh.Positions.Length)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboNrm);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.Normals.Length * 3 * sizeof(float), mesh.Normals, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
            }
            else
            {
                GL.DisableVertexAttribArray(1);
            }

            // Indices
            if (mesh.Indices.Length > 0)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.Indices.Length * sizeof(int), mesh.Indices, BufferUsageHint.StaticDraw);
                indexCount = mesh.Indices.Length;
            }
            else
            {
                indexCount = mesh.Positions.Length; // for line strip
            }

            GL.BindVertexArray(0);
        }

        public void Draw(Matrix4 mvp, Matrix4 model, bool wireframe)
        {
            GL.UseProgram(program);
            GL.UniformMatrix4(GL.GetUniformLocation(program, "uMVP"), false, ref mvp);
            GL.UniformMatrix4(GL.GetUniformLocation(program, "uModel"), false, ref model);

            GL.BindVertexArray(vao);

            if (isLineStrip)
            {
                GL.LineWidth(2f);
                GL.DrawArrays(PrimitiveType.LineStrip, 0, indexCount);
            }
            else
            {
                var mode = wireframe ? PrimitiveType.Lines : PrimitiveType.Triangles;
                if (wireframe)
                {
                    // GPU-level wireframe (optional). Alternatively, draw edges—this is simple:
                    GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
                }
                GL.DrawElements(mode, indexCount, DrawElementsType.UnsignedInt, 0);
                if (wireframe)
                    GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            }

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public void Dispose()
        {
            if (ebo != 0) GL.DeleteBuffer(ebo);
            if (vboNrm != 0) GL.DeleteBuffer(vboNrm);
            if (vboPos != 0) GL.DeleteBuffer(vboPos);
            if (vao != 0) GL.DeleteVertexArray(vao);
            if (program != 0) GL.DeleteProgram(program);
        }

        private static int Compile(string vs, string fs)
        {
            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs); GL.CompileShader(v); GL.GetShader(v, ShaderParameter.CompileStatus, out int okV);
            if (okV == 0) throw new InvalidOperationException("VS: " + GL.GetShaderInfoLog(v));

            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fs); GL.CompileShader(f); GL.GetShader(f, ShaderParameter.CompileStatus, out int okF);
            if (okF == 0) throw new InvalidOperationException("FS: " + GL.GetShaderInfoLog(f));

            int p = GL.CreateProgram();
            GL.AttachShader(p, v); GL.AttachShader(p, f); GL.LinkProgram(p);
            GL.GetProgram(p, GetProgramParameterName.LinkStatus, out int okP);
            GL.DeleteShader(v); GL.DeleteShader(f);
            if (okP == 0) throw new InvalidOperationException("Link: " + GL.GetProgramInfoLog(p));
            return p;
        }
    }
}
