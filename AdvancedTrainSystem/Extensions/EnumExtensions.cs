using System;

namespace AdvancedTrainSystem.Extensions
{
    /// <summary>
    /// Various enum extensions.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets next value of enum.
        /// </summary>
        /// <typeparam name="T">Enum type.</typeparam>
        /// <param name="src">Enum.</param>
        /// <returns>Next value of enum.</returns>
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(string.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }
}
