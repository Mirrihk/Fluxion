using Fluxion.Math.Algebra.Operations;
using System;
using System.Collections.Generic;

namespace Fluxion.Core.App
{
    public static class PrimeTest
    {
        public static void Run()
        {
            int target = 10000;
            var primes = PrimeFinder.GetPrimes(target);
            Console.WriteLine($"Found {primes.Count} primes up to {target}:");
            Console.WriteLine(string.Join(", ", primes));
        }
    }
}