using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Dev2.Providers.Logs
{
    public static class Logger
    {
        public static void Warning(string message = null, [CallerMemberName]string module = null)
        {
            WriteEntry(message, "WARNING", module);
        }

        public static void TraceInfo(string message = null, [CallerMemberName]string module = null)
        {
            WriteEntry(message, "INFORMATION", module);
        }

        static void WriteEntry(string message, string type, string module)
        {
            var format = string.Format("{0} :: {1} -> {2}  {3}", DateTime.Now, type, module, message);
            Trace.WriteLine(format);
        }
    }
}
