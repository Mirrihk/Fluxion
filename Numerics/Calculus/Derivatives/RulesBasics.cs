// File: Fluxion.Math/Calculus/Derivatives/RulesBasics.cs
using System;
using Fluxion.Numerics.Calculus.Concepts;

namespace Fluxion.Numerics.Calculus.Derivatives
{
    /// <summary>Basic differentiation rules for common primitives.</summary>
    public static class RulesBasics
    {
        public static IFunction Constant(double c) => new LambdaFunction(_ => 0.0);
        public static IFunction Power(double n) => new LambdaFunction(x => n * System.Math.Pow(x, n - 1));
        public static IFunction ExpE() => new LambdaFunction(x => System.Math.Exp(x));
        public static IFunction ExpA(double a) => new LambdaFunction(x => System.Math.Pow(a, x) * System.Math.Log(a));
        public static IFunction Ln() => new LambdaFunction(x => 1.0 / x);
        public static IFunction Sin() => new LambdaFunction(x => System.Math.Cos(x));
        public static IFunction Cos() => new LambdaFunction(x => -System.Math.Sin(x));
        public static IFunction Tan() => new LambdaFunction(x => 1.0 / (System.Math.Cos(x) * System.Math.Cos(x)));
    }
}
