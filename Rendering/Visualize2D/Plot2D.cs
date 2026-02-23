using System.Collections.Generic;
using System.Numerics;

namespace Fluxion.Rendering.Visualize2D
{
    public class Plot2D
    {
        public List<Vector2> Points { get; } = new();
        public PlotStyle Style { get; }

        public Plot2D(PlotStyle style)
        {
            Style = style;
        }
    }
}
