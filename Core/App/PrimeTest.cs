using Fluxion.Math.Algebra.Operations;
using System;
using System.Collections.Generic;

namespace Fluxion.Core.App
{
    public static class PrimeTest
    {
        public static void Run()
        {
            var primes = PrimeFinder.GetPrimes(100);
            Console.WriteLine($"Found {primes.Count} primes up to 100:");
            Console.WriteLine(string.Join(", ", primes));
        }
    }
}