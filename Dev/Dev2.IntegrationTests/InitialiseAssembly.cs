using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests
{
    [TestClass]
    public class InitialiseAssemblyClass
    {
        [AssemblyInitialize]
        public static void InitialiseAssembly(TestContext context)
        {
            const string WmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
            using (var searcher = new ManagementObjectSearcher(WmiQueryString))
            using (var results = searcher.Get())
            {
                // ReSharper disable once MaximumChainedReferences
                var query = (from p in Process.GetProcesses()
                            join mo in results.Cast<ManagementObject>()
                            on p.Id equals (int)(uint)mo["ProcessId"]
                            select new
                            {
                                Process = p,
                                Path = (string)mo["ExecutablePath"],
                                CommandLine = (string)mo["CommandLine"],
                            }).ToArray();

                if(query.Any(a =>a.Path!= null && a.Path.Contains("Warewolf Server")))
                {
                    // ReSharper disable once MaximumChainedReferences
                    var proce = new FileInfo(query.First(a => a.Path != null &&  a.Path.Contains("Warewolf Server")).Path).Directory;
                    if(proce != null)
                    {
                        var firstOrDefault = proce.EnumerateFiles("wareWolf-Server.log").FirstOrDefault();
                        if(firstOrDefault != null)
                        {
                            var logFile = firstOrDefault.FullName;
                            if(!File.ReadAllText(logFile).Contains("Web server listening at https://*:3143/"))
                            {
                                throw new Exception("The Server has started, but is lagging all tests should fail.");
                            }
                        }
                        throw new Exception("The Server has started, but there is no log file. tests should fail.");
                    }
                }
                else
                {
                    throw new Exception("The Server has not started. All Tests should fail");
                }
                // ReSharper disable once MaximumChainedReferences
            }
        }
    }
}
