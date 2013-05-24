using System;
using Dev2.Common;

namespace Dev2
{
    public static class TraceWriter
    {

        public static void WriteTraceIf(Func<bool> predicate, string msg)
        {
            if(predicate.Invoke())
            {
                WriteTrace(msg);
            }
        }

        public static void WriteTraceIf(bool shouldWriteTrace, string msg)
        {
            if(shouldWriteTrace)
            {
                WriteTrace(msg);
            }
        }


        public static void WriteTrace(string msg)
        {
            var fullMsg = string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff"), msg);
            ServerLogger.LogTrace(fullMsg);
        }

        public static void WriteTrace(IFrameworkDuplexDataChannel managementChannel, string msg)
        {
            WriteTrace(managementChannel, msg, null);
        }

        public static void WriteTrace(IFrameworkDuplexDataChannel managementChannel, string msg, string msgType)
        {
            WriteTrace(msg);
            if(managementChannel != null)
            {
                managementChannel.SendMessage("System", FormatMessage(msg, msgType));
            }
        }

        private static string FormatMessage(string message, string messageType = null)
        {
            string msg = string.Empty;


            if(messageType == null)
            {
                messageType = string.Empty;
            }

            msg = string.Format("{0} [{1}] {2}", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff"), messageType, message);

            return msg;

        }

    }

}


