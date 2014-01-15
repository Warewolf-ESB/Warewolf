using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

namespace Dev2.Providers.Logs
{
    public static class Logger
    {
        public static void Error(string message = null, [CallerMemberName] string methodName = null)
        {
            // ReSharper disable ExplicitCallerInfoArgument
            Error(null, message, methodName);
            // ReSharper restore ExplicitCallerInfoArgument
        }

        public static void Error(this object obj, string message = null, [CallerMemberName] string methodName = null)
        {
            WriteEntry(message, "ERROR", obj, methodName);
        }

        public static void Error(Exception exception, [CallerMemberName] string methodName = null)
        {
            var completeExceptionStackTrace = new StringBuilder();
            if(exception != null)
            {
                completeExceptionStackTrace.AppendLine(JsonConvert.SerializeObject(exception));
                while(exception.InnerException != null)
                {
                    exception = exception.InnerException;
                    completeExceptionStackTrace.AppendLine(JsonConvert.SerializeObject(exception));
                }
            }
            var exceptionMessage = completeExceptionStackTrace.ToString();
            WriteEntry(exceptionMessage, "EXCEPTION", null, methodName);
        }

        public static void Error(Exception exception)
        {
            var completeExceptionStackTrace = new StringBuilder();
            AppendException(exception, completeExceptionStackTrace);
            while(exception.InnerException != null)
            {
                exception = exception.InnerException;
                AppendException(exception, completeExceptionStackTrace);
            }
            var exceptionMessage = completeExceptionStackTrace.ToString();
            WriteEntry(exceptionMessage, "EXCEPTION", null, "");
        }

        static void AppendException(Exception exception, StringBuilder completeExceptionStackTrace)
        {
            completeExceptionStackTrace.AppendLine(exception.Message);
            completeExceptionStackTrace.AppendLine(exception.StackTrace);
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
            var format = string.Format("{0} :: {1} -> {2} {3} : {4}",
                DateTime.Now.ToString("g"),
                type,
                obj == null ? string.Empty : obj.GetType().Name,
                methodName,
                message);

            Trace.WriteLine(format);
        }
    }
}
