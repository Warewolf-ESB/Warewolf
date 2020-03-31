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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Network;
using Dev2.Studio;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Services;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels;
using Dev2.Threading;
using Warewolf.Studio.Models;
using Warewolf.Studio.ViewModels;

namespace Dev2
{
    public class Bootstrapper : Bootstrapper<IShellViewModel>, IDisposable
    {
        protected override void PrepareApplication()
        {
            base.PrepareApplication();
            CustomContainer.LoadedTypes = new List<Type>();
            AddRegionTypes();
            CheckPath();
            FileHelper.MigrateTempData(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        }

        void AddRegionTypes()
        {
            CustomContainer.AddToLoadedTypes(typeof(ManagePluginServiceModel));
            CustomContainer.AddToLoadedTypes(typeof(ManageComPluginServiceModel));
            CustomContainer.AddToLoadedTypes(typeof(ManageDbServiceModel));
            CustomContainer.AddToLoadedTypes(typeof(ManageWebServiceModel));
            CustomContainer.AddToLoadedTypes(typeof(ManageWcfServiceModel));
            CustomContainer.AddToLoadedTypes(typeof(ExchangeServiceModel));
            CustomContainer.AddToLoadedTypes(typeof(ManageRabbitMQSourceModel));
        }

        #region Fields

        bool _serverServiceStartedFromStudio;

        #endregion

        #region Overrides
        ShellViewModel _mainViewModel;
        protected override void Configure()
        {
            CustomContainer.Register<IWindowManager>(new WindowManager());
            CustomContainer.Register<IPopupController>(new PopupController());
            _mainViewModel = new ShellViewModel();
            CustomContainer.Register<IShellViewModel>(_mainViewModel);
            CustomContainer.Register<IShellViewModel>(_mainViewModel);
            CustomContainer.Register<IWindowsServiceManager>(new WindowsServiceManager());
            var conn = ServerRepository.Instance.Get(Guid.Empty).Connection;
            conn.Connect(Guid.NewGuid());
            CustomContainer.Register<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>(new Microsoft.Practices.Prism.PubSubEvents.EventAggregator());

            ClassRoutedEventHandlers.RegisterEvents();
        }

        #region Overrides of BootstrapperBase


        protected override void OnExit(object sender, EventArgs e)
        {
            if (_serverServiceStartedFromStudio)
            {
                var app = Application.Current as IApp;
                if (app != null)
                {
                    app.ShouldRestart = true;
                }
            }
        }

        #endregion
        
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            if(CheckWindowsService())
            {
                base.OnStartup(sender, e);
            }
            else            
            {
                Application.Shutdown();
            }

        }

        #region Overrides of BootstrapperBase

        protected override object GetInstance(Type service, string key) => CustomContainer.Get(service);

        #endregion

        #endregion Public Methods

        #region Private Methods

#pragma warning disable CC0091 // Use static method
#pragma warning disable CC0038 // You should use expression bodied members whenever possible.
        bool CheckWindowsService()
#pragma warning restore CC0091 // Use static method
        {
#if DEBUG
            return true;
#else
            IWindowsServiceManager windowsServiceManager = CustomContainer.Get<IWindowsServiceManager>();
            IPopupController popup = CustomContainer.Get<IPopupController>();
            ServerServiceConfiguration ssc = new ServerServiceConfiguration(windowsServiceManager, popup);

            if (ssc.DoesServiceExist())
            {
                if (ssc.IsServiceRunning())
                {
                    return true;
                }

                if (ssc.PromptUserToStartService())
                {
                    if (ssc.StartService())
                    {
                        _serverServiceStartedFromStudio = true;
                        return true;
                    }
                }
            }

            return false;
#endif
        }
#pragma warning restore CC0038 // You should use expression bodied members whenever possible.

        void CheckPath()
        {
            var sysUri = new Uri(AppDomain.CurrentDomain.BaseDirectory);

            if (IsLocal(sysUri))
            {
                return;
            }

            var popup = new PopupController
            {
                Header = "Load Error",
                Description =
                        $@"The Design Studio could not be launched from a network location.
                        {Environment.NewLine}Please install the application on your local machine",
                Buttons = MessageBoxButton.OK
            };

            popup.Show();

            Application.Current.Shutdown();
        }

#pragma warning disable CC0091 // Use static method
        bool IsLocal(Uri sysUri)
#pragma warning restore CC0091 // Use static method
        {
            if (IsUnc(sysUri))
            {
                return false;
            }

            if (!IsUnc(sysUri))
            {
                var currentLocation = new DriveInfo(sysUri.AbsolutePath);
                var drives = DriveInfo.GetDrives();
                var info = drives.Where(c => c.DriveType == DriveType.Network);
                if (info.Any(c => c.RootDirectory.Name == currentLocation.RootDirectory.Name))
                {
                    return false;
                }
            }
            else
            {
                return true;
            }

            return true;
        }

        static bool IsUnc(Uri sysUri) => sysUri.IsUnc;

        public void Dispose()
        {
            ((IDisposable)_mainViewModel).Dispose();
        }

        #endregion Private Methods
    }
}
