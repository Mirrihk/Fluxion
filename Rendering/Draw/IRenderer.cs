// Fluxion.Rendering/Draw/IRenderer.cs
using System;
using System.Numerics;

namespace Fluxion.Rendering.Draw
{
    /// <summary>
    /// Minimal renderer abstraction for 2D primitives.
    /// Different backends (OpenGL, DirectX, etc.) can implement this.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>Clear screen to RGBA color.</summary>
        void Clear(float r, float g, float b, float a = 1f);

        /// <summary>Draw a set of 2D points.</summary>
        void DrawPoints(ReadOnlySpan<Vector2> pts, float size = 2f);

        /// <summary>Draw a set of 2D connected line segments.</summary>
        void DrawLines(ReadOnlySpan<Vector2> pts, float width = 1f);
    }
}
