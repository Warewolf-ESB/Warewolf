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
                File.AppendAllText(LogFile(), DateTime.Now + " :: Kill Process { " + args[0] + " }");
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
                        File.AppendAllText(LogFile(), "No processes");
                    }
                    else
                    {
                        File.AppendAllText(LogFile(),"Found processes");
                        foreach (ManagementObject process in processes)
                        {
                            //print process properties
                            
                            process.Get();
                            PropertyDataCollection processProperties = process.Properties;

                            var pid = processProperties["ProcessID"].Value.ToString();

                            File.AppendAllText(LogFile(), "Killed Process { " + pid + " }");

                            var proc = Process.GetProcessById(Int32.Parse(pid));

                            proc.Kill();
                        }
                    }
                }
            }


            //if (args.Length == 1)
            //{
            //    File.AppendAllText(LogFile(), DateTime.Now + " :: Kill Process { " + args[0] + " }");

            //    try
            //    {
            //        Process[] procs = Process.GetProcessesByName(args[0]);
            //        foreach (var p in procs)
            //        {
            //            File.AppendAllText(LogFile(), DateTime.Now + " :: Killed PID { " + p.Id + " }");
            //            p.Kill();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        File.AppendAllText(LogFile(), DateTime.Now + " :: Error { " + ex.Message + " }");
            //    }
            //}
        }
    }
}
