using System;
using System.ServiceProcess;
using System.Threading;
using Dev2.Common;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Services
{
    public class WindowsServiceManager : IWindowsServiceManager
    {
        #region Methods

        public bool Exists()
        {
            bool result = true;

            try
            {
                ServiceController controller = new ServiceController(GlobalConstants.ServiceName);
                if(controller.Status == ServiceControllerStatus.Running)
                {
                }
            }
            catch(InvalidOperationException)
            {
                result = false;
            }

            return result;
        }

        public bool IsRunning()
        {
            bool result;

            try
            {
                ServiceController controller = new ServiceController(GlobalConstants.ServiceName);
                result = controller.Status == ServiceControllerStatus.Running;
            }
            catch(InvalidOperationException)
            {
                result = false;
            }

            return result;
        }

        public bool Start()
        {
            bool result;

            try
            {
                ServiceController controller = new ServiceController(GlobalConstants.ServiceName);
                if(controller.Status != ServiceControllerStatus.Running)
                {
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(120));
                }
                result = controller.Status == ServiceControllerStatus.Running;
            }
            catch(InvalidOperationException)
            {
                result = false;
            }

            return result;
        }

        public bool Stop()
        {
            bool result = true;

            try
            {
                ServiceController controller = new ServiceController(GlobalConstants.ServiceName);
                if(controller.Status == ServiceControllerStatus.Running)
                {
                    controller.Stop();
                    int pollCount = 0;
                    controller.Refresh();
                    while(controller.Status == ServiceControllerStatus.Running || pollCount > 60)
                    {
                        controller.Refresh();
                        pollCount++;
                        Thread.Sleep(500);
                    }
                }
            }
            catch(InvalidOperationException)
            {
                result = false;
            }

            return result;
        }

        #endregion Methods
    }
}
