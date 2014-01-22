using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Dev2.Instrumentation;
using Newtonsoft.Json;

namespace Dev2.Providers.Logs
{
    public static class Logger
    {
        public static void LogError(string className, Exception ex, [CallerMemberName] string methodName = null)
        {
            LogError(className, methodName, ex);
        }

        public static void LogError(this object obj, Exception exception, [CallerMemberName] string methodName = null)
        {
            LogError(obj, methodName, exception);
        }

        static void LogError(object obj, string methodName, Exception ex)
        {
            LogError(obj == null ? "UnknownClass" : obj.GetType().Name, methodName, ex);
        }

        static void LogError(string className, string methodName, Exception ex)
        {
            Tracker.TrackException(className, methodName, ex);

            var errors = new StringBuilder();
            while(ex != null)
            {
                errors.AppendLine(JsonConvert.SerializeObject(ex));
                ex = ex.InnerException;
            }
            WriteEntry(errors.ToString(), "ERROR", className, methodName);
        }

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
            WriteEntry(message, type, obj == null ? string.Empty : obj.GetType().Name, methodName);
        }

        static void WriteEntry(string message, string type, string className, string methodName)
        {
            var format = string.Format("{0} :: {1} -> {2} {3} : {4}",
                DateTime.Now.ToString("g"),
                type,
                className,
                methodName,
                message);

            Trace.WriteLine(format);
        }
    }
}
