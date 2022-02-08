using System;
using System.Collections.Generic;

namespace CommonLibrary.Attacks
{
    public static class Divisors
    {
        /// <summary>
        ///     Finds all the divisors of any positive integer passed as argument.
        ///     Returns an array of int with all the divisors of the argument.
        ///     Returns null if the argument is zero or negative.
        /// </summary>
        public static IEnumerable<int> GetDivisors(this int n)
        {
            if (n <= 0) return null;
            var divisors = new List<int>();
            for (var i = 1; i <= Math.Sqrt(n); i++)
            {
                if (n % i != 0) continue;
                divisors.Add(i);
                if (i != n / i) divisors.Add(n / i);
            }

            divisors.Sort();
            return divisors;
        }
    }
}