using OpenTK.Mathematics;

namespace Fluxion.Rendering.Visualize3D
{
    public sealed class Mesh3D
    {
        public Vector3[] Positions { get; init; } = System.Array.Empty<Vector3>();
        public Vector3[] Normals { get; init; } = System.Array.Empty<Vector3>();
        public int[] Indices { get; init; } = System.Array.Empty<int>();
    }
}