using System;
using System.Collections.Generic;

namespace Fluxion.Numerics.Algebra.Operations
{
    /// <summary>
    /// William-style prime sieve for competitive performance.
    /// </summary>
    public static class PrimeFinder
    {
        /// <summary>
        /// Returns all prime numbers up to n using an optimized sieve.
        /// </summary>
        public static List<int> GetPrimes(int n)
        {
            if (n < 2)
                return new List<int>();

            bool[] isPrime = new bool[n + 1];
            Array.Fill(isPrime, true);
            isPrime[0] = isPrime[1] = false;

            for (int i = 2; i * i <= n; i++)
            {
                if (isPrime[i])
                {
                    for (int j = i * i; j <= n; j += i)
                        isPrime[j] = false;
                }
            }

            var primes = new List<int>();
            for (int i = 2; i <= n; i++)
                if (isPrime[i]) primes.Add(i);

            return primes;
        }

        /// <summary>
        /// Checks if a given number is prime (fast for small numbers).
        /// </summary>
        public static bool IsPrime(int n)
        {
            if (n < 2) return false;
            if (n % 2 == 0) return n == 2;
            for (int i = 3; i * i <= n; i += 2)
                if (n % i == 0) return false;
            return true;
        }
    }
}
