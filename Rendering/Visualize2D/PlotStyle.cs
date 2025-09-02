using System.Numerics;

namespace Fluxion.Rendering.Visualize
{
    public class PlotStyle
    {
        public bool Lines { get; set; } = true;
        public bool Points { get; set; } = false;
        public float Width { get; set; } = 2f;
        public Vector3? Rgb { get; set; } = new Vector3(1f, 1f, 1f);
    }
}
