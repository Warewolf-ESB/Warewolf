
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common.Interfaces.Studio;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Services;
using Dev2.Studio;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.AppResources.WindowManagers;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Services;
using Dev2.Studio.Core.Services.System;
using Dev2.Studio.Factory;
using Dev2.Studio.Feedback;
using Dev2.Studio.StartupResources;
using Dev2.Studio.ViewModels;

namespace Dev2
{
    public class Bootstrapper : Bootstrapper<IMainViewModel>
    {

        protected override void PrepareApplication()
        {
            base.PrepareApplication();
            PreloadReferences();
            CheckPath();
            FileHelper.MigrateTempData(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            var assemblies = base.SelectAssemblies().ToList();
            assemblies.AddRange(new[]
                {
                    Assembly.GetAssembly(typeof (Bootstrapper)),
                    Assembly.GetAssembly(typeof (DebugWriter))
                });
            return assemblies.Distinct();
        }

        #region Fields

        bool _serverServiceStartedFromStudio;

        #endregion

        #region Overrides

        protected override void Configure()
        {
            CustomContainer.Register<IWindowManager>(new WindowManager());
            CustomContainer.Register<IDockAwareWindowManager>(new XamDockManagerDockAwareWindowManager());
            CustomContainer.Register<ISystemInfoService>(new SystemInfoService());
            CustomContainer.Register<IFeedBackRecorder>(new FeedbackRecorder());
            CustomContainer.Register<IPopupController>(new PopupController());
            CustomContainer.Register<IFeedbackInvoker>(new FeedbackInvoker());
            CustomContainer.Register<IMainViewModel>(new MainViewModel());
            CustomContainer.Register<IWindowsServiceManager>(new WindowsServiceManager());
            CustomContainer.Register<IDialogViewModelFactory>(new DialogViewModelFactory());
            ClassRoutedEventHandlers.RegisterEvents();
        }

        #region Overrides of BootstrapperBase


        protected override void OnExit(object sender, EventArgs e)
        {
            if(_serverServiceStartedFromStudio)
            {
                var app = Application.Current as IApp;
                if(app != null)
                {
                    app.ShouldRestart = true;
                }
            }
        }

        #endregion



        protected override void OnStartup(object sender, StartupEventArgs e)
        {

            // ReSharper disable JoinDeclarationAndInitializer
            // ReSharper disable RedundantAssignment
            // ReSharper disable ConvertToConstant.Local
            bool start = true;
            // ReSharper restore ConvertToConstant.Local
            // ReSharper restore RedundantAssignment
            // ReSharper restore JoinDeclarationAndInitializer
#if !DEBUG
            start = CheckWindowsService();
#endif

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if(start)
            {
                base.OnStartup(sender, e);
            }
            else
            // ReSharper disable HeuristicUnreachableCode
            {
                Application.Shutdown();
            }
            // ReSharper restore HeuristicUnreachableCode
        }

        protected override void StartRuntime()
        {
            Dev2SplashScreen.Show();
            base.StartRuntime();
        }

        #region Overrides of BootstrapperBase

        protected override object GetInstance(Type service, string key)
        {
            return CustomContainer.Get(service);
        }

        #endregion

        #endregion Public Methods

        #region Private Methods

        /*
         * DELETE THIS METHOD AND LOOSE A VERY IMPORTANT PART OF YOU ;)
         * 
         * IT IS REQUIRED FOR UPDATES IN RELEASE MODE ;)
         * REMOVING IT MEANS IT IS NOT POSSIBLE TO BUILD AN INSTALLER ;)
         */
        // ReSharper disable once UnusedMember.Local
        private bool CheckWindowsService()
        {
            IWindowsServiceManager windowsServiceManager = CustomContainer.Get<IWindowsServiceManager>();
            IPopupController popup = CustomContainer.Get<IPopupController>();
            ServerServiceConfiguration ssc = new ServerServiceConfiguration(windowsServiceManager, popup);

            if(ssc.DoesServiceExist())
            {
                if(ssc.IsServiceRunning())
                {
                    return true;
                }

                if(ssc.PromptUserToStartService())
                {
                    if(ssc.StartService())
                    {
                        _serverServiceStartedFromStudio = true;
                        return true;
                    }
                }
            }

            return false;
        }

        private void PreloadReferences()
        {
            var currentAsm = typeof(App).Assembly;
            var inspected = new HashSet<string> { currentAsm.GetName().ToString() };
            LoadReferences(currentAsm, inspected);
        }

        private void LoadReferences(Assembly asm, HashSet<string> inspected)
        {
            var allReferences = asm.GetReferencedAssemblies();

            foreach(AssemblyName toLoad in allReferences)
            {
                if(inspected.Add(toLoad.ToString()))
                {
                    try
                    {
                        Assembly loaded = AppDomain.CurrentDomain.Load(toLoad);
                        LoadReferences(loaded, inspected);
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch
                    // ReSharper restore EmptyGeneralCatchClause
                    {
                        // Pissing me off ;) - Some strange dependency :: 'Microsoft.Scripting.Metadata'
                    }
                }
            }
        }

        private void CheckPath()
        {
            var sysUri = new Uri(AppDomain.CurrentDomain.BaseDirectory);

            if(IsLocal(sysUri)) return;

            var popup = new PopupController
                {
                    Header = "Load Error",
                    Description = String.Format(@"The Design Studio could not be launched from a network location.
                                                    {0}Please install the application on your local machine",
                                                Environment.NewLine),
                    Buttons = MessageBoxButton.OK
                };

            popup.Show();

            Application.Current.Shutdown();
        }

        private bool IsLocal(Uri sysUri)
        {
            if(IsUnc(sysUri))
            {
                return false;
            }

            if(!IsUnc(sysUri))
            {
                var currentLocation = new DriveInfo(sysUri.AbsolutePath);
                DriveInfo[] drives = DriveInfo.GetDrives();
                IEnumerable<DriveInfo> info = drives.Where(c => c.DriveType == DriveType.Network);
                if(info.Any(c => c.RootDirectory.Name == currentLocation.RootDirectory.Name))
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

        private static bool IsUnc(Uri sysUri)
        {
            return sysUri.IsUnc;
        }

        #endregion Private Methods
    }
}
