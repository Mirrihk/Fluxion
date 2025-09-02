// Fluxion.Rendering/Draw/ConsoleRenderer.cs
using System;
using System.Numerics;

namespace Fluxion.Rendering.Draw
{
    public class ConsoleRenderer : IRenderer
    {
        public void Clear(float r, float g, float b, float a = 1f)
            => Console.WriteLine($"Clear({r},{g},{b},{a})");

        public void DrawPoints(ReadOnlySpan<Vector2> pts, float size = 2f)
            => Console.WriteLine($"Draw {pts.Length} point(s), size={size}");

        public void DrawLines(ReadOnlySpan<Vector2> pts, float width = 1f)
            => Console.WriteLine($"Draw polyline with {pts.Length} points, width={width}");
    }
}
