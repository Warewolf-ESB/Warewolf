using Dev2.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Dev2
{
    static class EntryPoint
    {
        static int Main(string[] arguments)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Dev2Logger.Fatal("Server has crashed!!!", args.ExceptionObject as Exception, "Warewolf Fatal");
            };

            try
            {
                using (new MemoryFailPoint(2048))
                {
                    return RunMain(arguments);
                }
            }
            catch (InsufficientMemoryException)
            {
                return RunMain(arguments);
            }
        }

        internal static int RunMain(string[] arguments)
        {
            const int Result = 0;

            if (Environment.UserInteractive || (arguments.Any() && arguments[0] == "--interactive"))
            {
                Dev2Logger.Info("** Starting In Interactive Mode **", GlobalConstants.WarewolfInfo);
                new ServerLifecycleManager().Run();
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
    }
}
