// Fluxion.Core/App/Program.cs
using System;

namespace Fluxion.Core.App
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (string.Equals(args[0], "all", StringComparison.OrdinalIgnoreCase))
                {
                    QuickSimulations.RunAll();
                    return;
                }
                if (QuickSimulations.TryRun(args[0])) return;
            }

            // Default: run everything
            //uickSimulations.RunAll();
            //QuickSimulationsComprehensiveTests.Run(visual: false, verbose: true);
            // or:
            QuickSimulationsComprehensiveTests.Run(visual: true, verbose: true);
        }
    }
}
