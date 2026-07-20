using System;

using Fluxion.src.Rendering.Visualize3D;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Fluxion.src.Rendering.Draw;

public sealed class SurfaceRenderer : IDisposable
{
    private readonly int _vao;
    private readonly int _positionBuffer;
    private readonly int _normalBuffer;
    private readonly int _elementBuffer;
    private readonly int _program;

    private readonly int _mvpLocation;
    private readonly int _modelLocation;

    private int _elementCount;
    private bool _isLineStrip;
    private bool _hasMesh;

    private const string VertexShaderSource = """
        #version 330 core

        layout(location = 0) in vec3 inPosition;
        layout(location = 1) in vec3 inNormal;

        uniform mat4 uMVP;
        uniform mat4 uModel;

        out vec3 normal;

        void main()
        {
            gl_Position =
                uMVP *
                vec4(inPosition, 1.0);

            normal =
                mat3(uModel) *
                inNormal;
        }
        """;

    private const string FragmentShaderSource = """
        #version 330 core

        in vec3 normal;

        out vec4 fragmentColor;

        uniform vec3 uLightDirection =
            normalize(vec3(0.5, 1.0, 0.3));

        uniform vec3 uBaseColor =
            vec3(0.2, 0.6, 1.0);

        void main()
        {
            float lightAmount =
                max(
                    dot(
                        normalize(normal),
                        normalize(uLightDirection)),
                    0.0);

            vec3 color =
                uBaseColor *
                (0.35 + (0.65 * lightAmount));

            fragmentColor =
                vec4(color, 1.0);
        }
        """;

    public SurfaceRenderer()
    {
        _program =
            CreateProgram(
                VertexShaderSource,
                FragmentShaderSource);

        _mvpLocation =
            GL.GetUniformLocation(
                _program,
                "uMVP");

        _modelLocation =
            GL.GetUniformLocation(
                _program,
                "uModel");

        _vao =
            GL.GenVertexArray();

        _positionBuffer =
            GL.GenBuffer();

        _normalBuffer =
            GL.GenBuffer();

        _elementBuffer =
            GL.GenBuffer();
    }

    public void Upload(
        Mesh3D mesh,
        bool wireframeAsLines = false)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        _isLineStrip =
            wireframeAsLines &&
            mesh.Indices.Length == 0;

        GL.BindVertexArray(_vao);

        GL.BindBuffer(
            BufferTarget.ArrayBuffer,
            _positionBuffer);

        GL.BufferData(
            BufferTarget.ArrayBuffer,
            mesh.Positions.Length *
            3 * sizeof(float),
            mesh.Positions,
            BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            3 * sizeof(float),
            0);

        if (mesh.Normals.Length ==
            mesh.Positions.Length)
        {
            GL.BindBuffer(
                BufferTarget.ArrayBuffer,
                _normalBuffer);

            GL.BufferData(
                BufferTarget.ArrayBuffer,
                mesh.Normals.Length *
                3 * sizeof(float),
                mesh.Normals,
                BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(
                1,
                3,
                VertexAttribPointerType.Float,
                false,
                3 * sizeof(float),
                0);
        }
        else
        {
            /*
             * A disabled generic vertex attribute uses its
             * current constant value.
             */
            GL.DisableVertexAttribArray(1);
            GL.VertexAttrib3(
                1,
                0.0f,
                1.0f,
                0.0f);
        }

        if (mesh.Indices.Length > 0)
        {
            GL.BindBuffer(
                BufferTarget.ElementArrayBuffer,
                _elementBuffer);

            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                mesh.Indices.Length *
                sizeof(int),
                mesh.Indices,
                BufferUsageHint.StaticDraw);

            _elementCount =
                mesh.Indices.Length;
        }
        else
        {
            _elementCount =
                mesh.Positions.Length;
        }

        GL.BindVertexArray(0);
        GL.BindBuffer(
            BufferTarget.ArrayBuffer,
            0);

        _hasMesh =
            _elementCount > 0;
    }

    public void Draw(
        Matrix4 mvp,
        Matrix4 model,
        bool wireframe)
    {
        if (!_hasMesh)
        {
            return;
        }

        GL.UseProgram(_program);

        GL.UniformMatrix4(
            _mvpLocation,
            false,
            ref mvp);

        GL.UniformMatrix4(
            _modelLocation,
            false,
            ref model);

        GL.BindVertexArray(_vao);

        if (_isLineStrip)
        {
            GL.LineWidth(2.0f);

            GL.DrawArrays(
                PrimitiveType.LineStrip,
                0,
                _elementCount);
        }
        else
        {
            /*
             * The index buffer contains triangles. Wireframe mode
             * must still draw triangles while PolygonMode converts
             * their filled faces into edges.
             */
            if (wireframe)
            {
                GL.PolygonMode(
                    TriangleFace.FrontAndBack,
                    PolygonMode.Line);
            }

            GL.DrawElements(
                PrimitiveType.Triangles,
                _elementCount,
                DrawElementsType.UnsignedInt,
                0);

            if (wireframe)
            {
                GL.PolygonMode(
                    TriangleFace.FrontAndBack,
                    PolygonMode.Fill);
            }
        }

        GL.BindVertexArray(0);
        GL.UseProgram(0);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(
            _elementBuffer);

        GL.DeleteBuffer(
            _normalBuffer);

        GL.DeleteBuffer(
            _positionBuffer);

        GL.DeleteVertexArray(
            _vao);

        GL.DeleteProgram(
            _program);
    }

    private static int CreateProgram(
        string vertexShaderSource,
        string fragmentShaderSource)
    {
        int vertexShader =
            CompileShader(
                ShaderType.VertexShader,
                vertexShaderSource);

        int fragmentShader =
            CompileShader(
                ShaderType.FragmentShader,
                fragmentShaderSource);

        int program =
            GL.CreateProgram();

        GL.AttachShader(
            program,
            vertexShader);

        GL.AttachShader(
            program,
            fragmentShader);

        GL.LinkProgram(program);

        GL.GetProgram(
            program,
            GetProgramParameterName.LinkStatus,
            out int linkSucceeded);

        string linkLog =
            GL.GetProgramInfoLog(program);

        GL.DetachShader(
            program,
            vertexShader);

        GL.DetachShader(
            program,
            fragmentShader);

        GL.DeleteShader(
            vertexShader);

        GL.DeleteShader(
            fragmentShader);

        if (linkSucceeded == 0)
        {
            GL.DeleteProgram(program);

            throw new InvalidOperationException(
                "SurfaceRenderer program link failed: " +
                linkLog);
        }

        return program;
    }

    private static int CompileShader(
        ShaderType shaderType,
        string source)
    {
        int shader =
            GL.CreateShader(shaderType);

        GL.ShaderSource(
            shader,
            source);

        GL.CompileShader(shader);

        GL.GetShader(
            shader,
            ShaderParameter.CompileStatus,
            out int compileSucceeded);

        if (compileSucceeded == 0)
        {
            string log =
                GL.GetShaderInfoLog(shader);

            GL.DeleteShader(shader);

            throw new InvalidOperationException(
                $"SurfaceRenderer {shaderType} compilation failed: {log}");
        }

        return shader;
    }
}
