// Fluxion.Math/Algebra/Equations/SystemOfEquations2x2.cs
using Math.Algebra.Concepts;
using Math.Abstractions;
namespace Math.Algebra.Equations
{
    public sealed class SystemOfEquations2x2 : IDisplay
    {
        public System2x2Model Model { get; }
        public SystemOfEquations2x2(System2x2Model model) => Model = model;

        /// <summary>
        /// IDisplay implementation. The single parameter is interpreted as the x-variable name.
        /// The y-variable will default to "y" (or "_y" if xVar is "y" to avoid duplicate names).
        /// </summary>
        public string AsString(string variable = "x")
        {
            var xVar = string.IsNullOrWhiteSpace(variable) ? "x" : variable;
            var yVar = xVar == "y" ? "_y" : "y";

            return $"{Model.A}{xVar} + {Model.B}{yVar} = {Model.E};  {Model.C}{xVar} + {Model.D}{yVar} = {Model.F}";
        }

        /// <summary>
        /// Overload to control both variable names explicitly.
        /// </summary>
        public string AsString(string xVar, string yVar)
            => $"{Model.A}{xVar} + {Model.B}{yVar} = {Model.E};  {Model.C}{xVar} + {Model.D}{yVar} = {Model.F}";
    }
}
