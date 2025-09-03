// Fluxion/Core/App/Program.cs
using System;

namespace Core.App
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            

            // Default: run everything
            QuickSimulations.RunAll();

            //QuickSimulationsComprehensiveTests.Run(visual: false, verbose: true);
            // or:
            QuickSimulationsComprehensiveTests.Run(visual: true, verbose: true);
        }
    }
}
