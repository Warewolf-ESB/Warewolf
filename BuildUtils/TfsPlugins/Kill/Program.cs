using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Kill
{
    public class Program
    {
        private static string LogfileName = @"BuildQueueLog.txt";

        private static string LogFile()
        {
            var loc = Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(loc);

            return Path.Combine(dir, LogfileName);
        }

        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                File.AppendAllText(LogFile(), DateTime.Now + " :: Kill Process { " + args[0] + " }");

                try
                {
                    Process[] procs = Process.GetProcessesByName(args[0]);
                    foreach (var p in procs)
                    {
                        File.AppendAllText(LogFile(), DateTime.Now + " :: Killed PID { " + p.Id + " }");
                        p.Kill();
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(LogFile(), DateTime.Now + " :: Error { " + ex.Message + " }");
                }
            }
        }
    }
}
