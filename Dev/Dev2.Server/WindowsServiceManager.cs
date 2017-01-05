/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;
using Dev2.Common;
using Dev2.Instrumentation;
using Dev2.Util;

namespace Dev2
{
    [RunInstaller(true)]
    public sealed class WindowsServiceManager : Installer
    {
        #region Fields

        private static bool _pendingCommit;

        #endregion

        #region Constructor

        public WindowsServiceManager()
        {
            var processInstaller = new ServiceProcessInstaller {Account = ServiceAccount.LocalSystem};
            var serviceInstaller = new ServiceInstaller
            {
                StartType = ServiceStartMode.Automatic,
                ServiceName = AppSettings.ServiceName
            };

            while (Installers.Count > 0)
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
            Tracker.StartServer();

            bool result = true;

            InstallContext context = null;
            try
            {
                using (var inst = new AssemblyInstaller(typeof (ServerLifecycleManager).Assembly, null))
                {
                    context = inst.Context; 
                    LogMessage("Installing service " + AppSettings.ServiceName, inst.Context);
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;

                    try
                    {
                        inst.Install(state);
                        inst.Commit(state);
                        Tracker.TrackEvent(TrackerEventGroup.Installations, TrackerEventName.Installed);
                    }
                    catch (Exception err)
                    {
                        Tracker.TrackException("WindowsServiceManager", "Install", err);
                        try
                        {
                            inst.Rollback(state);
                        }
                        catch (Exception innerErr)
                        {
                            throw new AggregateException(new List<Exception> {err, innerErr});
                        }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                WriteExceptions(ex, context);
            }
            finally
            {
                Tracker.Stop();
            }

            return result;
        }

        public static bool Uninstall()
        {
            Tracker.StartServer();

            bool result = true;

            InstallContext context = null;
            try
            {
                using (var inst = new AssemblyInstaller(typeof (ServerLifecycleManager).Assembly, null))
                {
                    context = inst.Context;
                    LogMessage("Uninstalling service " + AppSettings.ServiceName, inst.Context);
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;
                    try
                    {
                        inst.Uninstall(state);
                        Tracker.TrackEvent(TrackerEventGroup.Installations, TrackerEventName.Uninstalled);
                    }
                    catch (Exception err)
                    {
                        Tracker.TrackException("WindowsServiceManager", "Uninstall", err);
                        try
                        {
                            inst.Rollback(state);
                        }
                        catch (Exception innerErr)
                        {
                            throw new AggregateException(new List<Exception> {err, innerErr});
                        }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                WriteExceptions(ex, context);
            }
            finally
            {
                Tracker.Stop();
            }
            return result;
        }

        public static bool StartService(InstallContext context)
        {
            bool result = true;

            try
            {
                var controller = new ServiceController(AppSettings.ServiceName);

                if (controller.Status != ServiceControllerStatus.Stopped)
                {
                    throw new Exception(string.Format("Can't start the service because it is in the '{0}' state.",
                        controller.Status));
                }

                controller.Start();
                LogMessage("Service started successfully.", context);
                LogMessage("Setting Recovery Options.", null);
                SetRecoveryOptions();
                LogMessage("Recovery Options successfully set.", null);
            }
            catch (Exception ex)
            {
                result = false;
                LogMessage(
                    "An error occurred while starting the service, please start it manually or reboot the computer.",
                    context);
                WriteExceptions(ex, context);
            }

            return result;
        }

        private static void SetRecoveryOptions()
        {
            int exitCode;
            using (var process = new Process())
            {
                ProcessStartInfo startInfo = process.StartInfo;
                startInfo.FileName = "sc";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                // tell Windows that the service should restart if it fails
                string arguments = string.Format("failure \"{0}\" reset= 0 actions= restart/60000", "Warewolf Server");
                startInfo.Arguments = arguments;

                process.Start();
                process.WaitForExit();

                exitCode = process.ExitCode;

                process.Close();
            }
            if (exitCode != 0)
            {
                throw new InvalidOperationException();
            }
        }

        public static bool StopService(InstallContext context)
        {
            bool result = true;

            try
            {
                var controller = new ServiceController(AppSettings.ServiceName);
                if (controller.Status != ServiceControllerStatus.Running)
                {
                    throw new Exception(string.Format("Can't stop the service because it is in the '{0}' state.",
                        controller.Status));
                }

                controller.Stop();
                LogMessage("Service stopped successfully.", context);
            }
            catch (Exception ex)
            {
                result = false;
                LogMessage(
                    "An error occurred while start the service, please start it manually or reboot the computer.",
                    context);
                WriteExceptions(ex, context);
            }

            return result;
        }

        private static void LogMessage(string message, InstallContext context)
        {
            if (context == null)
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
            if (context == null)
            {
                return;
            }

            var aggregateException = e as AggregateException;
            if (aggregateException != null)
            {
                foreach (Exception child in aggregateException.InnerExceptions)
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
                var controller = new ServiceController(AppSettings.ServiceName);
                tryStartService = controller.Status != ServiceControllerStatus.Running;
                serviceExists = true;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex);
            }

            if (serviceExists)
            {
                LogMessage("Service already installed.", Context);
                if (tryStartService)
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
                var controller = new ServiceController(AppSettings.ServiceName);
                // ReSharper restore UnusedVariable
                serviceExists = true;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex);
            }

            if (serviceExists)
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
            if (_pendingCommit)
            {
                base.Commit(savedState);
            }
        }

        #endregion
    }
}
