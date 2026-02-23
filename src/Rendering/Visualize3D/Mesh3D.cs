using OpenTK.Mathematics;

namespace Fluxion.src.Rendering.Visualize3D
{
    public sealed class Mesh3D
    {
        public Vector3[] Positions { get; init; } = Array.Empty<Vector3>();
        public Vector3[] Normals { get; init; } = Array.Empty<Vector3>();
        public int[] Indices { get; init; } = Array.Empty<int>();
    }
}