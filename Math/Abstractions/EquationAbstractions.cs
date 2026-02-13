// Math/Abstractions/EquationAbstractions.cs
namespace Math.Abstractions
{
    /// Contract for single-variable real functions f(x).</summary>
    public interface IEquation
    {
        /// <summary> Evaluate f(x) for a given x.</summary>
        double Evaluate(double x);

        /// <summary> Returns true if parameters are valid (e.g., domain/NaN checks).</summary>
        bool IsWellFormed();
    }

    /// <summary>Contract for a human-friendly display string.</summary>
    public interface IDisplay
    {
        /// <summary>Pretty representation, e.g., "A·x + B".</summary>
        string AsString(string variable = "x");
    }
}
