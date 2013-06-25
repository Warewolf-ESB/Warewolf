using System;
using System.IO;

namespace Dev2.Studio.UI.Tests.Bootstrap
{
    public static class Logger
    {

        public static void InitLogger()
        {
            File.Delete(@"C:\TfsBuildUtils\CodedU_Log.txt");
        }

        public static void LogMessage(string msg)
        {
            File.AppendAllText(@"C:\TfsBuildUtils\CodedU_Log.txt", msg);
        }

        public static void LogError(Exception e)
        {
            File.AppendAllText(@"C:\TfsBuildUtils\CodedU_Log.txt", e.Message);
            File.AppendAllText(@"C:\TfsBuildUtils\CodedU_Log.txt", e.StackTrace);   
        }
    }
}
