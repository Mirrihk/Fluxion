namespace Fluxion.Desktop;

public sealed record GraphRequest(
    string Expression,
    double MinimumX,
    double MaximumX,
    int Resolution,
    bool Wireframe,
    bool ShowAxes,
    bool ShowGrid);