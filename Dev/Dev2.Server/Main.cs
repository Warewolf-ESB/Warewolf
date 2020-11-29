#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.ServerLifeCycleWorkers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.ServiceProcess;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Dev2
{
    static class EntryPoint
    {
        static async Task<int> Main(string[] arguments)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Dev2Logger.Fatal("Server has crashed!!!", args.ExceptionObject as Exception, "Warewolf Fatal");
            };

            try
            {
                using (new MemoryFailPoint(2048))
                {
                    return await RunMain(arguments);
                }
            }
            catch (InsufficientMemoryException)
            {
                return await RunMain(arguments);
            }
        }

        internal static async Task<int> RunMain(string[] arguments)
        {
            SetWorkingDirectory();

            const int Result = 0;

#if DEBUG
            if (Environment.GetEnvironmentVariable("WAREWOLF_SERVER_DEBUG") == "1")
            {
                Dev2Logger.Info("** Starting In Debugging Mode **", GlobalConstants.WarewolfInfo);
                while (!Debugger.IsAttached)
                {
                    Thread.Sleep(3000);
                    Console.WriteLine("Still waiting for remote debugging...");
                }
                Console.WriteLine("Ready for remote debugging.");
            }
#endif

            if (Environment.UserInteractive || (arguments.Any() && arguments[0] == "--interactive"))
            {
                Dev2Logger.Info("** Starting In Interactive Mode **", GlobalConstants.WarewolfInfo);
                var manager = new ServerLifecycleManager(new ServerEnvironmentPreparer());
                var runTask = manager.Run(new LifeCycleInitializationList());
                runTask.Wait();

                WaitForUserExit(manager);
            }
            else
            {
                Dev2Logger.Info("** Starting In Service Mode **", GlobalConstants.WarewolfInfo);
                using (var service = new ServerLifecycleManagerService())
                {
                    ServiceBase.Run(service);
                    if (!service.RunSuccessful)
                    {
                        Dev2Logger.Warn("** Service Mode Failed to Start **", GlobalConstants.WarewolfWarn);
                        return -1;
                    }
                }
            }
            return Result;
        }

        static void WaitForUserExit(ServerLifecycleManager manager)
        {
            Console.WriteLine();
            if (EnvironmentVariables.IsServerOnline)
            {
                Console.WriteLine("Press <ENTER> to terminate service and/or web server if started");
                Pause();
            }
            else
            {
                Console.WriteLine("Failed to start Server");
            }
            manager.Stop(false, 0, false);
        }

        private static void Pause() => Console.ReadLine();
        static void SetWorkingDirectory()
        {
            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to set working directory.");
                Console.WriteLine(e);
            }
        }
    }
}
