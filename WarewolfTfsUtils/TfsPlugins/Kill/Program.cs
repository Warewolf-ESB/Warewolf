using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Management;

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
                try
                {
                    File.AppendAllText(LogFile(),
                                       DateTime.Now + " :: Kill Process { " + args[0] + " }" + Environment.NewLine);
                    var processName = args[0];
                    var query = new SelectQuery(@"SELECT * FROM Win32_Process where Name LIKE '%" + processName + "%'");
                    //initialize the searcher with the query it is
                    //supposed to execute
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                    {
                        //execute the query
                        ManagementObjectCollection processes = searcher.Get();
                        if (processes.Count <= 0)
                        {
                            File.AppendAllText(LogFile(), "No processes " + Environment.NewLine);
                        }
                        else
                        {
                            foreach (ManagementObject process in processes)
                            {
                                //print process properties

                                process.Get();
                                PropertyDataCollection processProperties = process.Properties;

                                var pid = processProperties["ProcessID"].Value.ToString();

                                File.AppendAllText(LogFile(), "Killed Process { " + pid + " } " + Environment.NewLine);

                                var proc = Process.GetProcessById(Int32.Parse(pid));

                                proc.Kill();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    File.AppendAllText(LogFile(),
                                       DateTime.Now + " :: Error { " + e.Message + " }" + Environment.NewLine + e.StackTrace);   
                }
            }
        }
    }
}
