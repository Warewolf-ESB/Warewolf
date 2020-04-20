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
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.View;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Versioning;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xaml;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Wrappers;
using Dev2.CustomControls.Progress;
using Dev2.Diagnostics.Debug;
using Dev2.Instrumentation;
using Dev2.Studio.ActivityDesigners;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.Views;
using Dev2.Threading;
using Dev2.Utilities;
using Infragistics.Windows.DockManager;
using Microsoft.Practices.Prism.PubSubEvents;
using Warewolf.Core;
using Warewolf.Studio.Models.Help;
using Warewolf.Studio.Models.Toolbox;
using Warewolf.Studio.ViewModels.Help;
using Warewolf.Studio.ViewModels.ToolBox;
using Dev2.Utils;
using log4net.Config;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.ViewModels;
using Dev2.Util;
using Warewolf.MergeParser;

using Dev2.Instrumentation.Factory;
using Dev2.Studio.Utils;
using System.Security.Claims;
using Dev2.Studio.Interfaces;
using Dev2.Activities;
using Microsoft.VisualBasic.ApplicationServices;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core;
using Dev2.Factory;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Dev2.Common.Interfaces.Scheduler.Interfaces;

namespace Dev2.Studio
{
    public partial class App : Application, IApp, IDisposable
    {
        ShellViewModel _shellViewModel;

        private Mutex _processGuard;

        private AppExceptionHandler _appExceptionHandler;
        private bool _hasShutdownStarted;
        public App(IMergeFactory mergeFactory)
        {
            this._mergeFactory = mergeFactory;
        }
        public App() : this(new MergeFactory())
        {
            // PrincipalPolicy must be set to WindowsPrincipal to check roles.
            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            _hasShutdownStarted = false;
            ShouldRestart = false;

            try
            {
                AppUsageStats.LocalHost = ConfigurationManager.AppSettings["LocalHostServer"];
                InitializeComponent();
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e.Message, e, GlobalConstants.WarewolfError);
                AppUsageStats.LocalHost = "http://localhost:3142";
            }
        }

        [PrincipalPermission(SecurityAction.Demand)]  // Principal must be authenticated
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            CustomContainer.Register<IFieldAndPropertyMapper>(new FieldAndPropertyMapper());
            CustomContainer.Register(ApplicationTrackerFactory.GetApplicationTrackerProvider());
            var applicationTracker = CustomContainer.Get<IApplicationTracker>();
            applicationTracker?.EnableApplicationTracker(VersionInfo.FetchVersionInfo(), VersionInfo.FetchInformationalVersion(), @"Warewolf" + $" ({ClaimsPrincipal.Current.Identity.Name})".ToUpperInvariant());

            ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;

            Task.Factory.StartNew(() =>
            {
                var dir = new DirectoryWrapper();
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), GlobalConstants.Warewolf, "Feedback");
                dir.CleanUp(path);
                dir.CleanUp(Path.Combine(GlobalConstants.TempLocation, GlobalConstants.Warewolf, "Debug"));
            });

            var localprocessGuard = new Mutex(true, GlobalConstants.WarewolfStudio, out bool createdNew);

            if (createdNew)
            {
                _processGuard = localprocessGuard;
            }
            else
            {
                Environment.Exit(Environment.ExitCode);
            }

            InitializeShell(e);
#if ! (DEBUG)
            var versionChecker = new VersionChecker();
            if(versionChecker.GetNewerVersion())
            {
                WebLatestVersionDialog dialog = new WebLatestVersionDialog();
                dialog.ShowDialog();
            }
#endif
        }

        static ISplashView _splashView;

        ManualResetEvent _resetSplashCreated;
        Thread _splashThread;
        private bool _hasDotNetFramweworkError;
        private readonly IMergeFactory _mergeFactory;
        protected void InitializeShell(System.Windows.StartupEventArgs e)
        {
            _resetSplashCreated = new ManualResetEvent(false);

            _splashThread = new Thread(ShowSplash);
            _splashThread.SetApartmentState(ApartmentState.STA);
            _splashThread.IsBackground = true;
            _splashThread.Name = "Splash Screen";
            _splashThread.Start();

            _resetSplashCreated.WaitOne();
            new Bootstrapper().Start();
            if (_hasDotNetFramweworkError)
            {
                SplashView.CloseSplash(false);
                var popupController = CustomContainer.Get<IPopupController>();
                popupController.ShowInstallationErrorOccurred();
                Shutdown();
            }
            base.OnStartup(e);
            _shellViewModel = MainWindow.DataContext as ShellViewModel;
            if (_shellViewModel != null)
            {
                CreateDummyWorkflowDesignerForCaching();
                SplashView.CloseSplash(false);

                if (e.Args.Length > 0)
                {
                    OpenBasedOnArguments(new WarwolfStartupEventArgs(e));
                }
                else
                {
                    _shellViewModel.ShowStartPageAsync();
                }
                CheckForDuplicateResources();
                _appExceptionHandler = new AppExceptionHandler(this, _shellViewModel);
                CustomContainer.Register<IApplicationAdaptor>(new ApplicationAdaptor(Current));
                CustomContainer.Register<IShellViewModel>(_shellViewModel);
            }
            var toolboxPane = Current.MainWindow.FindName("Toolbox") as ContentPane;
            toolboxPane?.Activate();
#if DEBUG
            SetAsStarted();
        }

        static void SetAsStarted()
        {
            try
            {
                var studioFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var studioStartedFile = Path.Combine(studioFolder, "StudioStarted");
                if (File.Exists(studioStartedFile))
                {
                    File.Delete(studioStartedFile);
                }
                File.WriteAllText(studioStartedFile, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }
#endif
        }

        public void OpenBasedOnArguments(WarwolfStartupEventArgs e)
        {
            if (e.Args.Any(p => p.Contains("-merge")))
            {
                _mergeFactory.OpenMergeWindow(_shellViewModel, e);
            }
            else
            {
                foreach (var item in e.Args)
                {
                    _shellViewModel.LoadWorkflowAsync(item.Replace("\"", ""));
                }
            }
        }

        private static void CreateDummyWorkflowDesignerForCaching()
        {
            var workflowDesigner = new WorkflowDesigner
            {
                PropertyInspectorFontAndColorData = XamlServices.Save(ActivityDesignerHelper.GetDesignerHashTable())
            };
            var designerConfigService = workflowDesigner.Context.Services.GetService<DesignerConfigurationService>();
            if (designerConfigService != null)
            {
                // set the runtime Framework version to 4.5 as new features are in .NET 4.5 and do not exist in .NET 4
                designerConfigService.TargetFrameworkName = new FrameworkName(".NETFramework", new Version(4, 5));
                designerConfigService.AutoConnectEnabled = true;
                designerConfigService.AutoSplitEnabled = true;
                designerConfigService.PanModeEnabled = true;
                designerConfigService.RubberBandSelectionEnabled = true;
                designerConfigService.BackgroundValidationEnabled = true;

                // prevent design-time background validation from blocking UI thread
                // Disabled for now
                designerConfigService.AnnotationEnabled = false;
                designerConfigService.AutoSurroundWithSequenceEnabled = false;
            }
            var meta = new DesignerMetadata();
            meta.Register();

            var builder = new AttributeTableBuilder();
            foreach (var designerAttribute in ActivityDesignerHelper.DesignerAttributes)
            {
                builder.AddCustomAttributes(designerAttribute.Key, new DesignerAttribute(designerAttribute.Value));
            }

            MetadataStore.AddAttributeTable(builder.CreateTable());
            workflowDesigner.Context.Services.Subscribe<DesignerView>(instance =>
            {
                instance.WorkflowShellHeaderItemsVisibility = ShellHeaderItemsVisibility.All;
                instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.None;
                instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.Zoom | ShellBarItemVisibility.PanMode | ShellBarItemVisibility.MiniMap;
            });
            var activityBuilder = new WorkflowHelper().CreateWorkflow("DummyWF");
            workflowDesigner.Load(activityBuilder);
        }

        async void CheckForDuplicateResources()
        {
            var server = ServerRepository.Instance.Source;
            var loadExplorerDuplicates = await server.LoadExplorerDuplicates();
            if (loadExplorerDuplicates?.Count > 0)
            {
                var newLoadExplorerDuplicates = loadExplorerDuplicates.Select(duplicate => duplicate.Remove(duplicate.LastIndexOf(Environment.NewLine, StringComparison.Ordinal))).ToList();
                var controller = CustomContainer.Get<IPopupController>();
                controller.ShowResourcesConflict(newLoadExplorerDuplicates);
            }
        }

        void ShowSplash()
        {
            // Create the window 
            var repository = ServerRepository.Instance;
            var server = repository.Source;
            server.Connect();
            CustomContainer.Register(server);
            CustomContainer.Register(repository);
            var toolBoxViewModel = new ToolboxViewModel(new ToolboxModel(server, server, null), new ToolboxModel(server, server, null));
            CustomContainer.Register<IToolboxViewModel>(toolBoxViewModel);

            var textToDisplay = Warewolf.Studio.Resources.Languages.Core.StandardStyling.Replace("\r\n", "") +
                                Warewolf.Studio.Resources.Languages.HelpText.WarewolfDefaultHelpDescription +
                                Warewolf.Studio.Resources.Languages.Core.StandardBodyParagraphClosing;

            var helpViewModel = new HelpWindowViewModel(new HelpDescriptorViewModel(new HelpDescriptor("", textToDisplay, null)), new HelpModel(new EventAggregator()));
            CustomContainer.Register<IHelpWindowViewModel>(helpViewModel);
            CustomContainer.Register<IEventAggregator>(new EventAggregator());
            CustomContainer.Register<IPopupController>(new PopupController());
            CustomContainer.Register<IAsyncWorker>(new AsyncWorker());
            CustomContainer.Register<IExplorerTooltips>(new ExplorerTooltips());
            CustomContainer.Register<IWarewolfWebClient>(new WarewolfWebClient(new WebClient { Credentials = CredentialCache.DefaultCredentials }));
            CustomContainer.RegisterInstancePerRequestType<IRequestServiceNameView>(() => new RequestServiceNameView());
            CustomContainer.RegisterInstancePerRequestType<IJsonObjectsView>(() => new JsonObjectsView());
            CustomContainer.RegisterInstancePerRequestType<IChooseDLLView>(() => new ChooseDLLView());
            CustomContainer.RegisterInstancePerRequestType<IFileChooserView>(() => new FileChooserView());
            CustomContainer.Register<IActivityParser>(new ActivityParser());
            CustomContainer.Register<IServiceDifferenceParser>(new ServiceDifferenceParser());

            _hasDotNetFramweworkError = ValidateDotNetFramework();

            var splashViewModel = new SplashViewModel(server, new ExternalProcessExecutor());

            var splashPage = new SplashPage { DataContext = splashViewModel };
            SplashView = splashPage;
            // Show it 
            SplashView.Show(false);

            _resetSplashCreated?.Set();
            splashViewModel.ShowServerStudioVersion();
            Dispatcher.Run();
        }

        private static bool ValidateDotNetFramework()
        {
            var serverLogFile = HelperUtils.GetServerLogSettingsConfigFile();
            if (!File.Exists(serverLogFile))
            {
                return false;
            }
            try
            {
                var lines = File.ReadAllLines(serverLogFile).Reverse();

                foreach (string line in lines)
                {
                    if (line.Contains(@"System.DllNotFoundException: C:\Windows\Microsoft.NET\Framework"))
                    {
                        return true;
                    }
                    if (line.Contains(@"[Header]"))
                    {
                        break;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("Error loading server log", ex, GlobalConstants.WarewolfError);
                return false;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var applicationTracker = CustomContainer.Get<IApplicationTracker>();

            //Stop the action tracking
            applicationTracker?.DisableApplicationTracker();

            SplashView.CloseSplash(true);

            // this is already handled ;)
            _shellViewModel?.PersistTabs(true);
            ProgressFileDownloader.PerformCleanup(new DirectoryWrapper(), GlobalConstants.VersionDownloadPath, new FileWrapper());
            HasShutdownStarted = true;
            DebugDispatcher.Instance.Shutdown();
            try
            {
                base.OnExit(e);
            }

            catch

            {
                // Best effort ;)
            }

            ForceShutdown();
        }

        void ForceShutdown()
        {
            if (ShouldRestart)
            {
                Task.Run(() => Process.Start(ResourceAssembly.Location, Guid.NewGuid().ToString()));
            }
            Environment.Exit(0);
        }


        public new void Shutdown()
        {
            try
            {
                SplashView.CloseSplash(true);
                base.Shutdown();
            }
            catch (Exception e)
            {
                Dev2Logger.Warn(e.Message, GlobalConstants.WarewolfWarn);
            }
            ForceShutdown();
        }


        public bool ShouldRestart { get; set; }

        public bool HasShutdownStarted
        {
            get
            {
                return Dispatcher.CurrentDispatcher.HasShutdownStarted || Dispatcher.CurrentDispatcher.HasShutdownFinished || _hasShutdownStarted;
            }
            set
            {
                _hasShutdownStarted = value;
            }
        }

        public static ISplashView SplashView { get => _splashView; set => _splashView = value; }

        void OnApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception.Message != "Desktop applications setting AppContext switch 'Switch.UseLegacyToolTipDisplay' to false are required to opt in to all earlier accessibility improvements. To do this, ensure that the AppContext switch 'Switch.UseLegacyAccessibilityFeatures.3' is set to 'false', then 'Switch.UseLegacyAccessibilityFeatures' and all 'Switch.UseLegacyAccessibilityFeatures.M' switches, when M < 3, evaluate to false as well. Note that, if a switch for a particular set of accessibility improvements is not present, its value is determined by the target framework version. You can remedy this by adding these switches and setting their value to false." &&
                e.Exception.Message != "Desktop applications setting AppContext switch 'Switch.System.Windows.Controls.ItemsControlDoesNotSupportAutomation' to false are required to opt in to all earlier accessibility improvements. To do this, ensure that the AppContext switch 'Switch.UseLegacyAccessibilityFeatures.3' is set to 'false', then 'Switch.UseLegacyAccessibilityFeatures' and all 'Switch.UseLegacyAccessibilityFeatures.M' switches, when M < 3, evaluate to false as well. Note that, if a switch for a particular set of accessibility improvements is not present, its value is determined by the target framework version. You can remedy this by adding these switches and setting their value to false.")
            {
                try
                {
                    Dev2Logger.Error("Unhandled Exception", e.Exception, GlobalConstants.WarewolfError);
                    var applicationTracker = CustomContainer.Get<IApplicationTracker>();
                    applicationTracker?.TrackCustomEvent(Warewolf.Studio.Resources.Languages.TrackEventExceptions.EventCategory, Warewolf.Studio.Resources.Languages.TrackEventExceptions.UnhandledException, "Method: OnApplicationDispatcherUnhandledException Exception: " + e.Exception);
                    if (_appExceptionHandler != null)
                    {
                        e.Handled = HasShutdownStarted || _appExceptionHandler.Handle(e.Exception);
                    }
                    else
                    {
                        MessageBox.Show("Fatal Error : " + e.Exception);
                    }
                }
                catch (Exception e2)
                {
                    System.Console.WriteLine("== Error ==\nerror: " + e2 + "\n  while processing unhandled exception: " + e.Exception + "\n== Error ==");
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        public void Dispose()
        {
            _resetSplashCreated.Dispose();
        }
    }

    public class WarwolfStartupEventArgs
    {

        public WarwolfStartupEventArgs(System.Windows.StartupEventArgs e)
        {
            Args = e.Args;
        }

        public WarwolfStartupEventArgs(string e)
        {
            Args = e.Split(' ');
        }

        public WarwolfStartupEventArgs(StartupNextInstanceEventArgs e)
        {
            Args = e.CommandLine.ToArray();
        }

        public string[] Args { get; }
    }
}
