//Fluxion/Numericcs/Trigonemtry/Concepts/TrigonometryModels.cs
namespace Fluxion.src.Numerics.Trigonometry.Concepts
{
    /// <summary>
    /// General sinusoid model: f(x) = Amplitude * sin(Frequency * x + Phase) + VerticalOffset
    /// </summary>
    public readonly record struct SineModel(double Amplitude, double Frequency, double Phase, double VerticalOffset);

    /// <summary>
    /// General sinusoid model: f(x) = Amplitude * cos(Frequency * x + Phase) + VerticalOffset
    /// </summary>
    public readonly record struct CosineModel(double Amplitude, double Frequency, double Phase, double VerticalOffset);
}
