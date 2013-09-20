using System;
using System.Diagnostics;

namespace Dev2.Providers.Logs
{
    public static class Logger
    {
        public static void Warning(string message, string module)
        {
            WriteEntry(message, "WARNING", module);
        }

        public static void TraceInfo(string message, string module)
        {
            WriteEntry(message, "INFORMATION", module);
        }

        static void WriteEntry(string message, string type, string module)
        {
            Trace.WriteLine(
                string.Format("{0} :: {1} -> {2}  {3}",
                    DateTime.Now,
                    type,
                    module,
                    message));
        }
    }
}
