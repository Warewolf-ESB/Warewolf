using System.Diagnostics;
using System.Runtime.InteropServices;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Network;
using Dev2.Network.Execution;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Services;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.StartupResources;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace Dev2.Studio
{
    public class Bootstrapper : Bootstrapper<IMainViewModel>
    {
        protected override void PrepareApplication()
        {
            base.PrepareApplication();
            CheckPath();
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            var assemblies = base.SelectAssemblies().ToList();
            assemblies.AddRange(new[]
                {
                    Assembly.GetAssembly(typeof (Bootstrapper)),
                    Assembly.GetAssembly(typeof (IMainViewModel)),
                    Assembly.GetAssembly(typeof (INetworkMessageBroker)),
                    Assembly.GetAssembly(typeof (INetworkExecutionChannel))
                });
            return assemblies.Distinct();
        }

        //protected override void Configure()
        //{
        //    base.Configure();
        //    //PreloadReferences();

        //    ////
        //    //// Setup MEF import service
        //    ////
        //    //ImportService.Initialize(new List<ComposablePartCatalog>
        //    //    {
        //    //            new AssemblyCatalog(Assembly.GetAssembly(typeof(Bootstrapper))),
        //    //            new AssemblyCatalog(Assembly.GetAssembly(typeof(IMainViewModel))),
        //    //            new AssemblyCatalog(Assembly.GetAssembly(typeof(INetworkMessageBroker))),
        //    //            new AssemblyCatalog(Assembly.GetAssembly(typeof(INetworkExecutionChannel))),
        //    //        });

        //    ////
        //    //// Create and show main window
        //    ////
        //    //IMainViewModel mainViewModel = new MainViewModel();
        //    //ImportService.AddExportedValueToContainer(mainViewModel);
        //    //ImportService.SatisfyImports(mainViewModel);

        //    ////
        //    ////Create main view
        //    ////
        //    //Application.Current.MainWindow = new MainView();
        //    //Application.Current.MainWindow.DataContext = mainViewModel;
        //    //Application.Current.MainWindow.Show();
        //}

        #region Fields

        private CompositionContainer _container;

        #endregion

        #region Overrides

        protected override void Configure()
        {
            _container = new CompositionContainer(
                new AggregateCatalog(
                    AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()));

            var batch = new CompositionBatch();

            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(_container);

            _container.Compose(batch);
            ImportService.Initialize(_container);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = _container.GetExportedValues<object>(contract);

            if (exports.Count() > 0)
                return exports.First();  

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            _container.SatisfyImportsOnce(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {

            bool start = true;
#if !DEBUG
            start = CheckWindowsService();
#endif

            Dev2SplashScreen.Close(TimeSpan.FromSeconds(1));

            if (start)
            {
                base.OnStartup(sender, e);
            }
            else
            {
                Application.Shutdown();
            }
        }

        protected override void StartRuntime()
        {
            Dev2SplashScreen.Show();
            base.StartRuntime();
        }

        #endregion Public Methods 

        #region Private Methods


        //private void PreloadReferences()
        //{
        //    var currentAsm = typeof(App).Assembly;
        //    var inspected = new HashSet<string> {currentAsm.GetName().ToString()};
        //    LoadReferences(currentAsm, inspected);
        //}

        //private void LoadReferences(Assembly asm, HashSet<string> inspected)
        //{
        //    var allReferences = asm.GetReferencedAssemblies();

        //    foreach (AssemblyName toLoad in allReferences)
        //        if (inspected.Add(toLoad.ToString()))
        //        {
        //            Assembly loaded = AppDomain.CurrentDomain.Load(toLoad);
        //            LoadReferences(loaded, inspected);
        //        }
        //}

        private bool CheckWindowsService()
        {
            IWindowsServiceManager windowsServiceManager = ImportService.GetExportValue<IWindowsServiceManager>();
            IPopUp popup = ImportService.GetExportValue<IPopUp>();

            if (windowsServiceManager == null)
            {
                throw new Exception("Unable to instantiate the windows service manager.");
            }

            if (popup == null)
            {
                throw new Exception("Unable to instantiate the popup manager.");
            }

            if (!windowsServiceManager.Exists())
            {
                popup.Show("The Warewolf service isn't installed. Please re-install the Warewolf server.", "Server Missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!windowsServiceManager.IsRunning())
            {
                MessageBoxResult promptResult = popup.Show("The Warewolf service isn't running would you like to start it?", "Service not Running", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (promptResult == MessageBoxResult.Cancel)
                {
                    return false;
                }
                
                if (promptResult == MessageBoxResult.No)
                {
                    return true;
                }

                if (!windowsServiceManager.Start())
                {
                    popup.Show("A time out occured while trying to start the Warewolf server service. Please try again.", "Timeout", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            return true;
        }

        private void CheckPath()
        {
            var sysUri = new Uri(AppDomain.CurrentDomain.BaseDirectory);

            if (IsLocal(sysUri)) return;

            var popup = new PopUp
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
            if (IsUnc(sysUri))
            {
                return false;
            }

            if (!IsUnc(sysUri))
            {
                var currentLocation = new DriveInfo(sysUri.AbsolutePath);
                DriveInfo[] drives = DriveInfo.GetDrives();
                IEnumerable<DriveInfo> info = drives.Where(c => c.DriveType == DriveType.Network);
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

        private static bool IsUnc(Uri sysUri)
        {
            return sysUri.IsUnc;
        }

        #endregion Private Methods
    }
}