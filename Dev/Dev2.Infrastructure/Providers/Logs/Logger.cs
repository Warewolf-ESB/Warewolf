using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Dev2.Providers.Logs
{
    public static class Logger
    {
        public static void Warning(string message = null, [CallerMemberName] string methodName = null)
        {
            // ReSharper disable ExplicitCallerInfoArgument
            Warning(null, message, methodName);
            // ReSharper restore ExplicitCallerInfoArgument
        }

        public static void Warning(this object obj, string message = null, [CallerMemberName] string methodName = null)
        {
            WriteEntry(message, "WARNING", obj, methodName);
        }

        public static void TraceInfo(string message = null, [CallerMemberName] string methodName = null)
        {
            // ReSharper disable ExplicitCallerInfoArgument
            TraceInfo(null, message, methodName);
            // ReSharper restore ExplicitCallerInfoArgument
        }

        public static void TraceInfo(this object obj, string message = null, [CallerMemberName] string methodName = null)
        {
            WriteEntry(message, "INFORMATION", obj, methodName);
        }

        static void WriteEntry(string message, string type, object obj, string methodName)
        {
            var format = string.Format("{0} :: {1} -> {2} {3} : {4}",
                DateTime.Now,
                type,
                obj == null ? string.Empty : obj.GetType().Name,
                methodName,
                message);

            Trace.WriteLine(format);
        }
    }
}
