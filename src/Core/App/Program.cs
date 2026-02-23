// Fluxion/Core/App/Program.cs
using Fluxion.tests;
using System;

namespace Fluxion.src.Core.App
{
    public static class Program
    {
        public static void Main(string[] args)
        {

            // Formula for testing prime number functionalities 
            PrimeTest.Run();

            // Default: run everything
            QuickSimulations.RunAll();
            
            QuickSimulations.Tesseract4D();

            // Or run with pauses between: 
            //QuickSimulations.RunAll();

            //QuickSimulationsComprehensiveTests.Run(visual: false, verbose: true);
            // or:
            //QuickSimulationsComprehensiveTests.Run(visual: true, verbose: true);
        }
    }
}
