using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Fluxion.src.Rendering.Draw;

/// <summary>
/// Draws mathematical X, Y, and Z axes plus an X/Y ground grid.
///
/// World-axis mapping:
/// mathematical X -> world X
/// mathematical Y -> world Z
/// mathematical Z -> world Y
///
/// This keeps world Y as the vertical direction for the orbit camera.
/// </summary>
public sealed class Axis3DRenderer : IDisposable
{
    private readonly int _vertexArray;
    private readonly int _vertexBuffer;
    private readonly int _program;
    private readonly int _mvpLocation;

    private readonly int _axisVertexCount;
    private readonly int _gridFirstVertex;
    private readonly int _gridVertexCount;

    public Axis3DRenderer(
        int halfExtent = 10,
        float step = 1.0f)
    {
        halfExtent =
            Math.Max(
                1,
                halfExtent);

        var axisData =
            new List<float>(64);

        var gridData =
            new List<float>(1024);

        var xColor =
            (R: 1.0f, G: 0.25f, B: 0.25f);

        var yColor =
            (R: 0.25f, G: 1.0f, B: 0.35f);

        var zColor =
            (R: 0.25f, G: 0.50f, B: 1.0f);

        var gridColor =
            (R: 0.25f, G: 0.25f, B: 0.30f);

        /*
         * Mathematical X axis: world X.
         */
        PushLine(
            axisData,
            new Vector3(
                -halfExtent,
                0,
                0),
            new Vector3(
                halfExtent,
                0,
                0),
            xColor);

        /*
         * Mathematical Y axis: world Z.
         */
        PushLine(
            axisData,
            new Vector3(
                0,
                0,
                -halfExtent),
            new Vector3(
                0,
                0,
                halfExtent),
            yColor);

        /*
         * Mathematical Z axis: world Y.
         */
        PushLine(
            axisData,
            new Vector3(
                0,
                -halfExtent,
                0),
            new Vector3(
                0,
                halfExtent,
                0),
            zColor);

        if (step > 0.0f)
        {
            /*
             * X/Y mathematical grid lies on world Y = 0,
             * which is the world X/Z plane.
             */
            for (float x = -halfExtent;
                 x <= halfExtent + 0.0001f;
                 x += step)
            {
                PushLine(
                    gridData,
                    new Vector3(
                        x,
                        0,
                        -halfExtent),
                    new Vector3(
                        x,
                        0,
                        halfExtent),
                    gridColor);
            }

            for (float y = -halfExtent;
                 y <= halfExtent + 0.0001f;
                 y += step)
            {
                PushLine(
                    gridData,
                    new Vector3(
                        -halfExtent,
                        0,
                        y),
                    new Vector3(
                        halfExtent,
                        0,
                        y),
                    gridColor);
            }
        }

        _axisVertexCount =
            axisData.Count / 6;

        _gridFirstVertex =
            _axisVertexCount;

        _gridVertexCount =
            gridData.Count / 6;

        axisData.AddRange(
            gridData);

        _vertexArray =
            GL.GenVertexArray();

        _vertexBuffer =
            GL.GenBuffer();

        GL.BindVertexArray(
            _vertexArray);

        GL.BindBuffer(
            BufferTarget.ArrayBuffer,
            _vertexBuffer);

        GL.BufferData(
            BufferTarget.ArrayBuffer,
            axisData.Count *
            sizeof(float),
            axisData.ToArray(),
            BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            6 * sizeof(float),
            0);

        GL.EnableVertexAttribArray(1);

        GL.VertexAttribPointer(
            1,
            3,
            VertexAttribPointerType.Float,
            false,
            6 * sizeof(float),
            3 * sizeof(float));

        GL.BindVertexArray(0);
        GL.BindBuffer(
            BufferTarget.ArrayBuffer,
            0);

        const string vertexShaderSource = """
            #version 330 core

            layout(location = 0) in vec3 position;
            layout(location = 1) in vec3 color;

            uniform mat4 uMVP;

            out vec3 vertexColor;

            void main()
            {
                vertexColor = color;

                gl_Position =
                    uMVP *
                    vec4(position, 1.0);
            }
            """;

        const string fragmentShaderSource = """
            #version 330 core

            in vec3 vertexColor;

            out vec4 fragmentColor;

            void main()
            {
                fragmentColor =
                    vec4(vertexColor, 1.0);
            }
            """;

        _program =
            CreateProgram(
                vertexShaderSource,
                fragmentShaderSource);

        _mvpLocation =
            GL.GetUniformLocation(
                _program,
                "uMVP");
    }

    public void Draw(
        Matrix4 mvp)
    {
        Draw(
            mvp,
            showAxes: true,
            showGrid: true);
    }

    public void Draw(
        Matrix4 mvp,
        bool showAxes,
        bool showGrid)
    {
        if (!showAxes &&
            !showGrid)
        {
            return;
        }

        GL.UseProgram(_program);

        GL.UniformMatrix4(
            _mvpLocation,
            false,
            ref mvp);

        GL.BindVertexArray(
            _vertexArray);

        if (showGrid &&
            _gridVertexCount > 0)
        {
            GL.LineWidth(1.0f);

            GL.DrawArrays(
                PrimitiveType.Lines,
                _gridFirstVertex,
                _gridVertexCount);
        }

        if (showAxes &&
            _axisVertexCount > 0)
        {
            GL.LineWidth(2.0f);

            GL.DrawArrays(
                PrimitiveType.Lines,
                0,
                _axisVertexCount);
        }

        GL.BindVertexArray(0);
        GL.UseProgram(0);
    }

    public void DrawAxes(
        Matrix4 mvp)
    {
        Draw(
            mvp,
            showAxes: true,
            showGrid: false);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(
            _vertexBuffer);

        GL.DeleteVertexArray(
            _vertexArray);

        GL.DeleteProgram(
            _program);
    }

    private static void PushLine(
        ICollection<float> destination,
        Vector3 start,
        Vector3 end,
        (float R, float G, float B) color)
    {
        destination.Add(start.X);
        destination.Add(start.Y);
        destination.Add(start.Z);

        destination.Add(color.R);
        destination.Add(color.G);
        destination.Add(color.B);

        destination.Add(end.X);
        destination.Add(end.Y);
        destination.Add(end.Z);

        destination.Add(color.R);
        destination.Add(color.G);
        destination.Add(color.B);
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
                "Axis3DRenderer program link failed: " +
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
                $"Axis3DRenderer {shaderType} compilation failed: {log}");
        }

        return shader;
    }
}
