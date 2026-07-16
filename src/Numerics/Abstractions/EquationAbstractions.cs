// Math/Abstractions/EquationAbstractions.cs
namespace Fluxion.src.Numerics.Abstractions
{
    /// Contract for single-variable real functions f(x).
    public interface IEquation
    {
        //Evaluate f(x) for a given x.
        double Evaluate(double x);

        // Returns true if parameters are valid (e.g., domain/NaN checks).
        bool IsWellFormed();
    }

    //Contract for a human-friendly display string.
    public interface IDisplay
    {
        //Pretty representation, e.g., "A·x + B".
        string AsString(string variable = "x");
    }
}
