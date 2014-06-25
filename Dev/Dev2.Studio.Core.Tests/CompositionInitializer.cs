#region

using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Windows;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.ProperMoqs;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Configuration;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Services.System;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Feedback;
using Dev2.Studio.ViewModels;
using Dev2.Studio.Webs;
using Dev2.Webs;
using Moq;

#endregion

namespace Dev2.Core.Tests
{
    public static class CompositionInitializer
    {
        public static ImportServiceContext DefaultInitialize()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
               // new FullTestAggregateCatalog()
            });


            // IMainViewModel mainViewModel = new MainViewModel();
            IMainViewModel mainViewModel = new Mock<IMainViewModel>().Object;
            ImportService.AddExportedValueToContainer(mainViewModel);

            return importServiceContext;
        }

        public static ImportServiceContext EmptyInitialize()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());

            return importServiceContext;
        }

        public static ImportServiceContext InitializeWithEventAggregator()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());

            Mock<IWindowManager> dev2WindowManager = new Mock<IWindowManager>();
            ImportService.AddExportedValueToContainer(dev2WindowManager.Object);

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            ImportService.AddExportedValueToContainer(feedbackInvoker.Object);

            Mock<IMainViewModel> mainViewModel = new Mock<IMainViewModel>();
            ImportService.AddExportedValueToContainer(mainViewModel.Object);

            Mock<IWebCommunication> webCommunication = new Mock<IWebCommunication>();
            ImportService.AddExportedValueToContainer(webCommunication.Object);

            return importServiceContext;
        }

        internal static ImportServiceContext InitializeForMeflessBaseViewModel()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());

            var dev2WindowManager = new Mock<IWindowManager>();
            ImportService.AddExportedValueToContainer(dev2WindowManager.Object);

            var feedbackInvoker = new Mock<IFeedbackInvoker>();
            ImportService.AddExportedValueToContainer(feedbackInvoker.Object);

            var mainViewModel = new Mock<IMainViewModel>();
            ImportService.AddExportedValueToContainer(mainViewModel.Object);

            var webCommunication = new Mock<IWebCommunication>();
            ImportService.AddExportedValueToContainer(webCommunication.Object);



            return importServiceContext;
        }

        internal static ImportServiceContext InitializeForSettingsViewModel(Mock<IRuntimeConfigurationAssemblyRepository> assemblyRepository, Mock<IWindowManager> windowManager, Mock<IPopupController> popup)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());

            ImportService.AddExportedValueToContainer(assemblyRepository.Object);
            ImportService.AddExportedValueToContainer(windowManager.Object);
            ImportService.AddExportedValueToContainer(popup.Object);

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            ImportService.AddExportedValueToContainer(feedbackInvoker.Object);

            Mock<IMainViewModel> mainViewModel = new Mock<IMainViewModel>();
            ImportService.AddExportedValueToContainer(mainViewModel.Object);

            Mock<IWebCommunication> webCommunication = new Mock<IWebCommunication>();
            ImportService.AddExportedValueToContainer(webCommunication.Object);

            // PBI 9598 - 2013.06.10 - TWR : added

            // PBI 9598 - 2013.06.10 - TWR : added

            return importServiceContext;
        }

        public static ImportServiceContext InitializeMockedMainViewModel(
            Mock<IWebController> webController = null,
            Mock<IWindowManager> windowManager = null,
            Mock<IPopupController> popupController = null,
            IFrameworkRepository<IEnvironmentModel> environmentRepo = null,
            Mock<IFeedbackInvoker> feedbackInvoker = null,
            Mock<IFeedBackRecorder> feedbackRecorder = null,
            Mock<IFrameworkRepository<UserInterfaceLayoutModel>> layoutRepo = null,
            IWorkspaceItemRepository workspaceItemRepository = null)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                //new FullTestAggregateCatalog()
            });

            if(popupController == null)
                popupController = new Mock<IPopupController>();

            ImportService.AddExportedValueToContainer(popupController.Object);
            ImportService.AddExportedValueToContainer((windowManager == null) ? new WindowManager() : windowManager.Object);
            ImportService.AddExportedValueToContainer((webController == null) ?
                new WebController() : webController.Object);

            ImportService.AddExportedValueToContainer(environmentRepo);
            ImportService.AddExportedValueToContainer((feedbackInvoker == null) ? new FeedbackInvoker() : feedbackInvoker.Object);
            ImportService.AddExportedValueToContainer((feedbackRecorder == null) ? new FeedbackRecorder() : feedbackRecorder.Object);
            ImportService.AddExportedValueToContainer((layoutRepo == null) ? new UserInterfaceLayoutRepository() : layoutRepo.Object);

            return importServiceContext;
        }

        internal static ImportServiceContext InitializeMockedMainViewModelOnly()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());

            var mainViewModel = new Mock<IMainViewModel>();

            ImportService.AddExportedValueToContainer(mainViewModel.Object);

            return importServiceContext;
        }

        internal static ImportServiceContext InitializeMockedMainViewModelStudioCore()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new StudioCoreTestAggregateCatalog()
            });

            var mainViewModel = new Mock<IMainViewModel>();

            ImportService.AddExportedValueToContainer(mainViewModel.Object);

            return importServiceContext;
        }

        internal static ImportServiceContext InitializeForFrameworkSecurityProviderTests()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());

            ImportService.AddExportedValueToContainer<IPopupController>(new MoqPopup());

            return importServiceContext;
        }

        internal static ImportServiceContext InitializeResourceWizardViewModelTests(IWebCommunication webCommunication)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());

            ImportService.AddExportedValueToContainer<IPopupController>(new MoqPopup());
            ImportService.AddExportedValueToContainer(webCommunication);

            //IMainViewModel mainViewModel = new MainViewModel();
            //ImportService.AddExportedValueToContainer<IMainViewModel>(mainViewModel);
            //ImportService.SatisfyImports(mainViewModel);

            return importServiceContext;
        }

        internal static ImportServiceContext InitializeResourceWizardViewModelTests()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());

            ImportService.AddExportedValueToContainer<IPopupController>(new MoqPopup());

            //IMainViewModel mainViewModel = new MainViewModel();
            //ImportService.AddExportedValueToContainer<IMainViewModel>(mainViewModel);
            //ImportService.SatisfyImports(mainViewModel);

            return importServiceContext;
        }
        internal static ImportServiceContext ExplorerViewModelTest()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });

            // setup env repo
            var repo = new Mock<IEnvironmentRepository>();
            repo.Setup(l => l.Load()).Verifiable();
            repo.Setup(repository => repository.Load()).Callback(() => { });
            var model = new Mock<IEnvironmentModel>();
            repo.Setup(l => l.Save(model.Object)).Verifiable();
            IList<IEnvironmentModel> models = new List<IEnvironmentModel>();
            repo.Setup(l => l.All()).Returns(models);
            // set up window behavior
            var winBehavior = new Mock<IWindowManager>();

            winBehavior.Setup(w => w.ShowDialog(null, null, null));
            ImportService.AddExportedValueToContainer(repo.Object);
            ImportService.AddExportedValueToContainer(winBehavior.Object);

            return importServiceContext;
        }

        internal static ImportServiceContext DeployViewModelOkayTest(Mock<IWindowManager> mockWindowManager = null)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });

            var mainViewModel = new Mock<IMainViewModel>();

            ImportService.AddExportedValueToContainer(mainViewModel.Object);

            // setup env repo
            var repo = new Mock<IEnvironmentRepository>();
            repo.Setup(l => l.Load()).Verifiable();

            var model = new Mock<IEnvironmentModel>();
            repo.Setup(l => l.Save(model.Object)).Verifiable();

            IList<IEnvironmentModel> models = new List<IEnvironmentModel>();
            repo.Setup(l => l.All()).Returns(models);

            // set up window behavior
            if(mockWindowManager == null)
                mockWindowManager = new Mock<IWindowManager>();
            ImportService.AddExportedValueToContainer(mockWindowManager.Object);

            //winBehavior.Setup(w => w.ShowDialog(It.IsAny<SimpleBaseViewModel>(),null ,null)).Callback(v => v.DialogResult = ViewModelDialogResults.Okay);
            ImportService.AddExportedValueToContainer(repo.Object);

            return importServiceContext;
        }

        internal static ImportServiceContext DeployViewModelCancelTest()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });

            var mainViewModel = new Mock<IMainViewModel>();
            ImportService.AddExportedValueToContainer(mainViewModel.Object);

            // setup env repo
            var repo = new Mock<IEnvironmentRepository>();
            repo.Setup(l => l.Load()).Verifiable();

            var model = new Mock<IEnvironmentModel>();
            repo.Setup(l => l.Save(model.Object)).Verifiable();

            IList<IEnvironmentModel> models = new List<IEnvironmentModel>();
            repo.Setup(l => l.All()).Returns(models);

            // set up window behavior
            var winBehavior = new Mock<IWindowManager>();
            ImportService.AddExportedValueToContainer(winBehavior.Object);

            //winBehavior.Setup(w => w.ShowDialog(It.IsAny<SimpleBaseViewModel>(), null, null)).Callback<SimpleBaseViewModel>(v => v.DialogResult = ViewModelDialogResults.Cancel);
            ImportService.AddExportedValueToContainer(repo.Object);

            return importServiceContext;
        }

        public static ImportServiceContext InitializeMockedWindowNavigationBehavior()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());

            // set up window behavior
            var winBehavior = new Mock<IWindowManager>();
            ImportService.AddExportedValueToContainer(winBehavior.Object);

            return importServiceContext;
        }

        internal static ImportServiceContext PopUpProviderForTestsWithMockMainViewModel()
        {
            return PopUpProviderForTestsWithMockMainViewModel(MessageBoxResult.OK);
        }

        internal static ImportServiceContext PopUpProviderForTestsWithMockMainViewModel(MessageBoxResult popupResult)
        {
            return PopUpProviderForTestsWithMockMainViewModel(new MoqPopup(popupResult));
        }

        internal static ImportServiceContext PopUpProviderForTestsWithMockMainViewModel(MoqPopup moqPopup)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullStudioAggregateCatalog()
            });

            var mainViewModel = new Mock<IMainViewModel>();
            ImportService.AddExportedValueToContainer(mainViewModel.Object);
            ImportService.AddExportedValueToContainer<IPopupController>(moqPopup);

            return importServiceContext;
        }

        internal static ImportServiceContext InitializeForFeedbackActionTests(Mock<IPopupController> popup, Mock<IFeedBackRecorder> feedBackRecorder, Mock<IFeedbackInvoker> feedbackInvoker, Mock<IWindowManager> windowManager)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullStudioAggregateCatalog()
            });

            var mainViewModel = new Mock<IMainViewModel>();
            ImportService.AddExportedValueToContainer(mainViewModel.Object);

            ImportService.AddExportedValueToContainer(popup.Object);
            ImportService.AddExportedValueToContainer(windowManager.Object);
            ImportService.AddExportedValueToContainer(feedBackRecorder.Object);
            ImportService.AddExportedValueToContainer(feedbackInvoker.Object);

            return importServiceContext;
        }

        internal static ImportServiceContext InitializeForFeedbackInvokerTests(Mock<IPopupController> popup)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());

            var mainViewModel = new Mock<IMainViewModel>();
            ImportService.AddExportedValueToContainer(mainViewModel.Object);

            ImportService.AddExportedValueToContainer(popup.Object);

            return importServiceContext;
        }



        internal static ImportServiceContext InitializeEmailFeedbackTest(Mock<ISystemInfoService> systemInfoService)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());

            ImportService.AddExportedValueToContainer(systemInfoService.Object);
            ImportService.AddExportedValueToContainer<IPopupController>(new MoqPopup(MessageBoxResult.OK));

            return importServiceContext;
        }

        internal static ImportServiceContext InitializeEmailFeedbackActionTest()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());
            ImportService.AddExportedValueToContainer<IPopupController>(new MoqPopup(MessageBoxResult.OK));

            return importServiceContext;
        }

        public static ImportServiceContext InitializeWithWindowManagerTest(Mock<ISystemInfoService> mockSysInfo, Mock<IWindowManager> mockWindowManager)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());

            ImportService.AddExportedValueToContainer(mockSysInfo.Object);
            ImportService.AddExportedValueToContainer(mockWindowManager.Object);
            ImportService.AddExportedValueToContainer<IPopupController>(new MoqPopup(MessageBoxResult.OK));

            return importServiceContext;
        }

        public static ImportServiceContext InitializeWithMockEventAggregator(Mock<IEventAggregator> mockEventAggregator)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });

            ImportService.AddExportedValueToContainer(mockEventAggregator.Object);
            ImportService.AddExportedValueToContainer<IPopupController>(new MoqPopup(MessageBoxResult.OK));
            return importServiceContext;
        }

        public static ImportServiceContext InitializeEmptyWithMockEventAggregator()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                //new FullTestAggregateCatalog()
            });

            ImportService.AddExportedValueToContainer<IPopupController>(new MoqPopup(MessageBoxResult.OK));
            return importServiceContext;
        }

        internal static ImportServiceContext InitializeTreeViewModelTests()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());


            return importServiceContext;
        }

        internal static ImportServiceContext InitializeNavigationViewModelTests(Mock<IEnvironmentRepository> repo)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            //
            //            ImportService.Initialize(new List<ComposablePartCatalog>()
            //            {
            //                new FullTestAggregateCatalog()
            //            });
            ImportService.Initialize(new List<ComposablePartCatalog>());
            ImportService.AddExportedValueToContainer(repo.Object);

            return importServiceContext;
        }

        public static ImportServiceContext InializeWithEventAggregator()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });

            var mainViewModel = new Mock<IMainViewModel>();
            ImportService.AddExportedValueToContainer(mainViewModel.Object);

            // setup env repo
            var repo = new Mock<IEnvironmentRepository>();
            repo.Setup(l => l.Load()).Verifiable();

            var model = new Mock<IEnvironmentModel>();
            repo.Setup(l => l.Save(model.Object)).Verifiable();

            IList<IEnvironmentModel> models = new List<IEnvironmentModel>();
            repo.Setup(l => l.All()).Returns(models);

            // set up window behavior
            var winBehavior = new Mock<IWindowManager>();

            // winBehavior.Setup(w => w.ShowDialog(It.IsAny<SimpleBaseViewModel>())).Callback<SimpleBaseViewModel>(v => v.DialogResult = ViewModelDialogResults.Okay);
            ImportService.AddExportedValueToContainer(repo.Object);
            ImportService.AddExportedValueToContainer(winBehavior.Object);

            return importServiceContext;
        }

        internal static ImportServiceContext InitializeIFilePersistenceProvider(Mock<IFilePersistenceProvider> filePersistenceProvider)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                //new StudioCoreTestAggregateCatalog()
            });

            var mainViewModel = new Mock<IMainViewModel>();
            ImportService.AddExportedValueToContainer(mainViewModel.Object);
            ImportService.AddExportedValueToContainer(filePersistenceProvider.Object);

            return importServiceContext;
        }
    }
}