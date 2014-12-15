
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Core.Services;

namespace Dev2.Services
{
    public class ServerServiceConfiguration
    {
        public IWindowsServiceManager ServiceManager { get; private set; }
        public IPopupController PopupController { get; private set; }

        public ServerServiceConfiguration(IWindowsServiceManager serviceManager)
        {
            ServiceManager = serviceManager;
        }

        public ServerServiceConfiguration(IWindowsServiceManager serviceManager, IPopupController popupController)
            : this(serviceManager)
        {
            PopupController = popupController;
        }

        public bool IsServiceRunning()
        {
            return ServiceManager.IsRunning();
        }

        public bool PromptUserToStartService()
        {
            if(ServiceManager == null)
            {
                throw new Exception("Null Service Manager");
            }

            if(!IsServiceRunning())
            {
                if(PopupController == null)
                {
                    throw new Exception("Null Popup Controller");
                }

                var startResult = PopupController.Show("The Warewolf service isn't running would you like to start it?", "Service not Running", MessageBoxButton.YesNo, MessageBoxImage.Question, null);

                if(startResult == MessageBoxResult.None || startResult == MessageBoxResult.No || startResult == MessageBoxResult.Cancel)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public bool StartService()
        {
            if(ServiceManager == null)
            {
                throw new Exception("Null Service Manager");
            }

            if(!ServiceManager.IsRunning())
            {
                if(!ServiceManager.Start())
                {
                    PopupController.Show("A time out occurred while trying to start the Warewolf server service. Please try again.", "Timeout", MessageBoxButton.OK, MessageBoxImage.Error, null);
                    return false;
                }
            }

            return true;
        }

        public bool DoesServiceExist()
        {
            if(!ServiceManager.Exists())
            {
                PopupController.Show("The Warewolf service isn't installed. Please re-install the Warewolf server.", "Server Missing", MessageBoxButton.OK, MessageBoxImage.Error, null);
                return false;
            }

            return true;
        }
    }
}
