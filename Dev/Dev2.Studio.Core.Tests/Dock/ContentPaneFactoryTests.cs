
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
using System.Windows;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.ProperMoqs;
using Dev2.Core.Tests.Utils;
using Dev2.CustomControls.Connections;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Dock;
using Dev2.Studio.ViewModels.WorkSurface;
using Infragistics.Windows.DockManager;
using Infragistics.Windows.DockManager.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Dock
{
    [TestClass]
    [Ignore] //TODO: Fix so not dependant on resource file or localize resource file to test project
    public class ContentPaneFactoryTests : MainViewModelBase
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ContentPaneFactory_OnPaneClosing")]
        public void ContentPaneFactory_OnPaneClosing_MessageBoxResultIsYes_ViewModelIsDisposed()
        {
            //------------Setup for test--------------------------
            WorkSurfaceContextViewModel workSurfaceContextViewModel;
            ContentPane userControl;
            var pane = SetupPane(out workSurfaceContextViewModel, out userControl, MessageBoxResult.Yes);
            var isDisposed = workSurfaceContextViewModel.IsDisposed;
            //------------Execute Test---------------------------
            pane.OnPaneClosing(userControl, new PaneClosingEventArgs());
            //------------Assert Results-------------------------
            Assert.IsNotNull(pane);
            Assert.IsFalse(isDisposed);
            Assert.IsTrue(workSurfaceContextViewModel.IsDisposed);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ContentPaneFactory_OnPaneClosing")]
        public void ContentPaneFactory_OnPaneClosing_MessageBoxResultIsCancel_ViewModelIsNotDisposed()
        {
            //------------Setup for test--------------------------
            WorkSurfaceContextViewModel workSurfaceContextViewModel;
            ContentPane userControl;
            var pane = SetupPane(out workSurfaceContextViewModel, out userControl, MessageBoxResult.Cancel);
            var isDisposed = workSurfaceContextViewModel.IsDisposed;
            //------------Execute Test---------------------------
            pane.OnPaneClosing(userControl, new PaneClosingEventArgs());
            //------------Assert Results-------------------------
            Assert.IsNotNull(pane);
            Assert.IsFalse(isDisposed);
            Assert.IsFalse(workSurfaceContextViewModel.IsDisposed);
        }

        static ContentPaneFactory SetupPane(out WorkSurfaceContextViewModel workSurfaceContextViewModel, out ContentPane userControl, MessageBoxResult messageBoxResult)
        {
            var pane = new ContentPaneFactory();
            var eventAggregator = new Mock<IEventAggregator>();
            var workSurfaceViewModel = new Mock<ITestWorkSurfaceViewModel>();
            workSurfaceViewModel.SetupGet(w => w.WorkSurfaceContext).Returns(WorkSurfaceContext.Workflow);
            workSurfaceViewModel.SetupGet(w => w.ResourceModel).Returns(new TestResourceModel { Authorized = true });
            var localhost = new Mock<IEnvironmentModel>();
            localhost.Setup(e => e.ID).Returns(Guid.Empty);
            localhost.Setup(e => e.IsConnected).Returns(true);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(c => c.All()).Returns(new[] { localhost.Object });
            environmentRepository.Setup(c => c.Source).Returns(localhost.Object);
            var eventPublisher = new Mock<IEventAggregator>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var versionChecker = new Mock<IVersionChecker>();
            var browserPopupController = new Mock<IBrowserPopupController>();

            var savePopup = new Mock<IPopupController>();
            savePopup.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>())).Returns(messageBoxResult);
            var mainViewModel = new MainViewModelMock(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object,new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, new Mock<IConnectControlViewModel>().Object, false, browserPopupController.Object) { IsBusyDownloadingInstaller = () => false, PopupProvider = savePopup.Object };

            workSurfaceContextViewModel = new WorkSurfaceContextViewModel(eventAggregator.Object, new WorkSurfaceKey
                {
                    EnvironmentID = Guid.NewGuid(),
                    ResourceID = Guid.NewGuid(),
                    ServerID = Guid.NewGuid(),
                    WorkSurfaceContext = WorkSurfaceContext.Workflow
                }, workSurfaceViewModel.Object, new Mock<IPopupController>().Object, (a, b) => { })
                {
                    Parent = mainViewModel
                };

            pane.ItemsSource = new List<WorkSurfaceContextViewModel>
                {
                    workSurfaceContextViewModel
                };

            userControl = new ContentPane { DataContext = workSurfaceContextViewModel };
            return pane;
        }
    }

    public interface ITestWorkSurfaceViewModel : IWorkSurfaceViewModel, IWorkflowDesignerViewModel { }
}
