using System;
using System.Runtime.InteropServices;

namespace AdvancedTrainSystem
{
    /// <summary>
    /// Provides debug log using external console window.
    /// </summary>
    internal class Debug
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        /// <summary>
        /// Writes message in console.
        /// </summary>
        /// <param name="caller">Caller class, for most cases just pass "this".</param>
        /// <param name="msgs">Messages to write.</param>
        internal static void Log(object caller, params object[] msgs)
        {
            Console.WriteLine($"{DateTime.Now:hh:mm:ss} {caller.GetType().Name}");
            foreach(var msg in msgs)
            {
                Console.WriteLine($"\t{msg}");
            }
        }

        /// <summary>
        /// Opens console.
        /// </summary>
        internal static void Start()
        {
            //AllocConsole();
        }

        /// <summary>
        /// Closes console.
        /// </summary>
        internal static void OnAbort()
        {
            FreeConsole();
        }
    }
}
