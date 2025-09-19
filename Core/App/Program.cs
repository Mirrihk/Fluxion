// Fluxion/Core/App/Program.cs
using System;

namespace Core.App
{
    public static class Program
    {
        public static void Main(string[] args)
        {


            // Default: run everything
            //QuickSimulations.RunAll();
            QuickSimulations.Tesseract4D();
            System.Console.WriteLine("Done. Press Enter to exit.");
            System.Console.ReadLine();
            // Or run with pauses between: 
            //QuickSimulations.RunAll(waitBetween: true);

            //QuickSimulationsComprehensiveTests.Run(visual: false, verbose: true);
            // or:
            //QuickSimulationsComprehensiveTests.Run(visual: true, verbose: true);
        }
    }
}
