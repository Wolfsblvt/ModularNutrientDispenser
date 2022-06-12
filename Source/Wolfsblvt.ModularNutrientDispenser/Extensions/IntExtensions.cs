using System;
using JetBrains.Annotations;

namespace Wolfsblvt.ModularNutrientDispenser.Extensions
{
    /// <summary>
    ///     A class containing helpful extensions for <see cref="int" />.
    /// </summary>
    public static class IntExtensions
    {
        /// <summary>
        ///     Caps a number at a given maximum.
        /// </summary>
        /// <param name="number">The number</param>
        /// <param name="cap">The maximum it can reach</param>
        /// <returns>The resulting number</returns>
        [Pure]
        public static int Cap(this int number, int cap)
        {
            return Math.Min(number, cap);
        }

        /// <summary>
        ///     Adds the given given value to the base. The result will be capped at the maximum defined.
        ///     <para />
        ///     If <paramref name="doNotAddPartial" /> is chosen, the value will not be added if it would exceed the maximum defined.
        /// </summary>
        /// <param name="start">The base value to start off from</param>
        /// <param name="add">The value to subtract</param>
        /// <param name="cap">The maximum it can reach</param>
        /// <param name="doNotAddPartial">If true the value will only be added if it doesn't exceed the cap</param>
        /// <returns>The final result value after the addition</returns>
        [Pure]
        public static int AddCapped(this int start, int add, int cap, bool doNotAddPartial = false)
        {
            // Special check for not adding partial values, only the full 'subtract'
            if (doNotAddPartial && start + add > cap)
                return start;

            return Math.Min(start + add, cap);
        }

        /// <summary>
        ///     Subtracts the given given value from the base. The result will be capped at the minimum defined.
        ///     <para />
        ///     If <paramref name="doNotSubtractPartial" /> is chosen, the value will not be subtracted if it would exceed the minimum defined.
        /// </summary>
        /// <param name="start">The base value to start off from</param>
        /// <param name="subtract">The value to subtract</param>
        /// <param name="cap">The maximum it can reach</param>
        /// <param name="doNotSubtractPartial">If true the value will only be subtracted if it doesn't exceed the cap</param>
        /// <returns>The final result value after the subtraction</returns>
        [Pure]
        public static int SubtractCapped(this int start, int subtract, int cap, bool doNotSubtractPartial = false)
        {
            // Special check for not adding partial values, only the full 'subtract'
            if (doNotSubtractPartial && start + subtract > cap)
                return start;

            return Math.Min(start + subtract, cap);
        }
    }
}