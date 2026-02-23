using System;
using System.Collections.Generic;

namespace Fluxion.src.Numerics.Algebra.Sequences;

public static class FibonacciSequence
{
    /// <summary>
    /// Returns the nth Fibonacci number (0-indexed).
    /// F(0) = 0
    /// F(1) = 1
    /// </summary>
    public static long Get(int n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n));

        if (n == 0) return 0;
        if (n == 1) return 1;

        long a = 0;
        long b = 1;

        for (int i = 2; i <= n; i++)
        {
            long next = a + b;
            a = b;
            b = next;
        }

        return b;
    }

    /// <summary>
    /// Infinite Fibonacci sequence generator.
    /// </summary>
    public static IEnumerable<long> Infinite()
    {
        long a = 0;
        long b = 1;

        while (true)
        {
            yield return a;

            long next = a + b;
            a = b;
            b = next;
        }
    }

    /// <summary>
    /// Generates first count Fibonacci numbers.
    /// </summary>
    public static IEnumerable<long> Take(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        long a = 0;
        long b = 1;

        for (int i = 0; i < count; i++)
        {
            yield return a;

            long next = a + b;
            a = b;
            b = next;
        }
    }
}
