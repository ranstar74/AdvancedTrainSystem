using System;

namespace AdvancedTrainSystem
{
    public class Debug
    {
        public static void Log(object caller, params object[] msgs)
        {
            Console.WriteLine($"{DateTime.Now:hh:mm:ss} {caller.GetType().Name}");
            foreach(var msg in msgs)
            {
                Console.WriteLine($"\t{msg}");
            }
        }
    }
}
