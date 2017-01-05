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
using System.ServiceProcess;
using Dev2.Util;

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
                ServiceController controller = new ServiceController(AppSettings.ServiceName);
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
                ServiceController controller = new ServiceController(AppSettings.ServiceName);
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
                ServiceController controller = new ServiceController(AppSettings.ServiceName);
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

        #endregion Methods
    }
}
