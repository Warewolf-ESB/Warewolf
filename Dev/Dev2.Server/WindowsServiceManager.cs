using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using Dev2.Common;

namespace Dev2
{
    [RunInstaller(true)]
    public sealed class WindowsServiceManager : Installer
    {
        #region Fields

        private static bool _pendingCommit;

        #endregion

        #region Contructor

        public WindowsServiceManager()
        {
            var processInstaller = new ServiceProcessInstaller { Account = ServiceAccount.LocalSystem };
            var serviceInstaller = new ServiceInstaller { StartType = ServiceStartMode.Automatic, ServiceName = GlobalConstants.ServiceName };

            while(Installers.Count > 0)
            {
                Installers.RemoveAt(0);
            }

            Installers.AddRange(serviceInstaller.Installers);

            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }

        #endregion

        #region Static Methods

        public static bool Install()
        {
            bool result = true;

            InstallContext context = null;
            try
            {
                //ServiceProcessInstaller spl = new ServiceProcessInstaller();
                //spl.Account
                using(AssemblyInstaller inst = new AssemblyInstaller(typeof(ServerLifecycleManager).Assembly, null))
                {
                    context = inst.Context;
                    LogMessage("Installing service " + GlobalConstants.ServiceName, inst.Context);
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;

                    try
                    {
                        inst.Install(state);
                        inst.Commit(state);
                    }
                    catch(Exception err)
                    {
                        try
                        {
                            inst.Rollback(state);
                        }
                        catch(Exception innerErr)
                        {
                            throw new AggregateException(new List<Exception> { err, innerErr });
                        }
                        throw;
                    }
                }
            }
            catch(Exception ex)
            {
                result = false;
                WriteExceptions(ex, context);
            }

            return result;
        }

        public static bool Uninstall()
        {
            bool result = true;

            InstallContext context = null;
            try
            {
                using(AssemblyInstaller inst = new AssemblyInstaller(typeof(ServerLifecycleManager).Assembly, null))
                {
                    context = inst.Context;
                    LogMessage("Uninstalling service " + GlobalConstants.ServiceName, inst.Context);
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;
                    try
                    {
                        inst.Uninstall(state);
                    }
                    catch(Exception err)
                    {
                        try
                        {
                            inst.Rollback(state);
                        }
                        catch(Exception innerErr)
                        {
                            throw new AggregateException(new List<Exception> { err, innerErr });
                        }
                        throw;
                    }
                }
            }
            catch(Exception ex)
            {
                result = false;
                WriteExceptions(ex, context);
            }

            return result;
        }

        public static bool StartService(InstallContext context)
        {
            bool result = true;

            try
            {
                ServiceController controller = new ServiceController(GlobalConstants.ServiceName);

                if(controller.Status != ServiceControllerStatus.Stopped)
                {
                    throw new Exception(string.Format("Can't start the service because it is in the '{0}' state.", controller.Status.ToString()));
                }

                controller.Start();
                LogMessage("Service started successfully.", context);
            }
            catch(Exception ex)
            {
                result = false;
                LogMessage("An error occurred while starting the service, please start it manually or reboot the computer.", context);
                WriteExceptions(ex, context);
            }

            return result;
        }

        public static bool StopService(InstallContext context)
        {
            bool result = true;

            try
            {
                ServiceController controller = new ServiceController(GlobalConstants.ServiceName);
                if(controller.Status != ServiceControllerStatus.Running)
                {
                    throw new Exception(string.Format("Can't stop the service because it is in the '{0}' state.", controller.Status.ToString()));
                }

                controller.Stop();
                LogMessage("Service stopped successfully.", context);
            }
            catch(Exception ex)
            {
                result = false;
                LogMessage("An error occurred while start the service, please start it manually or reboot the computer.", context);
                WriteExceptions(ex, context);
            }

            return result;
        }

        private static void LogMessage(string message, InstallContext context)
        {
            if(context == null)
            {
                Console.WriteLine(message);
            }
            else
            {
                context.LogMessage(message);
            }
        }

        private static void WriteExceptions(Exception e, InstallContext context)
        {
            if(context == null)
            {
                return;
            }

            AggregateException aggregateException = e as AggregateException;
            if(aggregateException != null)
            {
                foreach(var child in aggregateException.InnerExceptions)
                {
                    context.LogMessage(child.Message);
                }
            }
            else
            {
                context.LogMessage(e.Message);
            }
        }

        #endregion

        #region Override Methods

        public override void Install(IDictionary stateSaver)
        {
            _pendingCommit = false;
            bool serviceExists = false;
            bool tryStartService = false;
            try
            {
                ServiceController controller = new ServiceController(GlobalConstants.ServiceName);
                tryStartService = controller.Status != ServiceControllerStatus.Running;
                serviceExists = true;
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
            }

            if(serviceExists)
            {
                LogMessage("Service already installed.", Context);
                if(tryStartService)
                {
                    LogMessage("Attempting to start service.", Context);
                    StartService(Context);
                }
            }
            else
            {
                _pendingCommit = true;
                base.Install(stateSaver);
                StartService(Context);
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            bool serviceExists = false;
            try
            {
                // ReSharper disable UnusedVariable
                var controller = new ServiceController(GlobalConstants.ServiceName);
                // ReSharper restore UnusedVariable
                serviceExists = true;
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
            }

            if(serviceExists)
            {
                base.Uninstall(savedState);
            }
            else
            {
                LogMessage("Service doesn't exist, nothing to uninstall.", Context);
            }
        }

        public override void Commit(IDictionary savedState)
        {
            if(_pendingCommit)
            {
                base.Commit(savedState);
            }
        }
        #endregion

    }
}
