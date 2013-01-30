using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using Dev2.Common;
using Unlimited.Applications.DynamicServicesHost;

namespace Dev2
{
    [RunInstaller(true)]
    public sealed class WindowsServiceManager : Installer
    {
        private ServiceProcessInstaller processInstaller;
        private ServiceInstaller serviceInstaller;

        public WindowsServiceManager()
        {
            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = GlobalConstants.ServiceName;

            while (Installers.Count > 0)
            {
                Installers.RemoveAt(0);
            }

            Installers.AddRange(serviceInstaller.Installers);

            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }

        public static bool Install()
        {
            bool result = true;

            try
            {
                Console.WriteLine("Installing service " + GlobalConstants.ServiceName);
                using (AssemblyInstaller inst = new AssemblyInstaller(typeof(ServerLifecycleManager).Assembly, null))
                {
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;
                    try
                    {
                        inst.Install(state);
                        inst.Commit(state);
                    }
                    catch (Exception err)
                    {
                        try
                        {
                            inst.Rollback(state);
                        }
                        catch (Exception innerErr)
                        {
                            throw new AggregateException(new List<Exception> { err, innerErr });
                        }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                WriteExceptions(ex);
            }

            if (result)
            {
                StartService();
            }

            return result;
        }

        public static bool Uninstall()
        {
            bool result = true;

            try
            {
                Console.WriteLine("Uninstalling service " + GlobalConstants.ServiceName);
                using (AssemblyInstaller inst = new AssemblyInstaller(typeof(ServerLifecycleManager).Assembly, null))
                {
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;
                    try
                    {
                        inst.Uninstall(state);
                    }
                    catch (Exception err)
                    {
                        try
                        {
                            inst.Rollback(state);
                        }
                        catch (Exception innerErr)
                        {
                            throw new AggregateException(new List<Exception> { err, innerErr });
                        }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                WriteExceptions(ex);
            }

            return result;
        }

        public static bool StartService()
        {
            bool result = true;

            try
            {
                ServiceController controller = new ServiceController(GlobalConstants.ServiceName);
                if (controller.Status != ServiceControllerStatus.Stopped)
                {
                    throw new Exception(string.Format("Can't start the service because it is in the '{0}' state.", controller.Status.ToString()));
                }

                controller.Start();
                Console.WriteLine("Service started successfully.");
            }
            catch (Exception ex)
            {
                result = false;
                Console.WriteLine("An error occured while start the service, please start it manually or reboot the computer.");
                WriteExceptions(ex);
            }

            return result;
        }

        public static bool StopService()
        {
            bool result = true;

            try
            {
                ServiceController controller = new ServiceController(GlobalConstants.ServiceName);
                if (controller.Status != ServiceControllerStatus.Running)
                {
                    throw new Exception(string.Format("Can't stop the service because it is in the '{0}' state.", controller.Status.ToString()));
                }

                controller.Stop(); 
                Console.WriteLine("Service stopped successfully.");
            }
            catch (Exception ex)
            {
                result = false;
                Console.WriteLine("An error occured while start the service, please start it manually or reboot the computer.");
                WriteExceptions(ex);
            }

            return result;
        }

        private static void WriteExceptions(Exception e)
        {
            AggregateException aggregateException = e as AggregateException;
            if (aggregateException != null)
            {
                foreach (var child in aggregateException.InnerExceptions)
                {
                    Console.Error.WriteLine(child.Message);
                }
            }
            else
            {
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}
