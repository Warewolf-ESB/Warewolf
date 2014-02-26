using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Utils;
using Dev2.Providers.Events;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Deploy;
using Dev2.Studio.ViewModels.Navigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Deploy
{
    /// <summary>
    /// Summary description for DeployUnitTestIsolation
    /// THIS test is in here because is always fails when included in the DeployViewModelTest
    /// even after using Trevor's work around of ImportService.CurrentContext = _okayContext;
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DeployUnitTestIsolation
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Deployed Items Come From Resource Repository

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DeployViewModel_Deploy")]
        public void DeployViewModel_Deploy_WhenDeployingResource_ResourceRepositoryDeployCalled()
        {
            //ImportService.CurrentContext = _okayContext;
            //MEFF
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>());

            //New Mocks
            var mockedServerRepo = new Mock<IEnvironmentRepository>();
            var server = new Mock<IEnvironmentModel>();
            var secondServer = new Mock<IEnvironmentModel>();
            var provider = new Mock<IEnvironmentModelProvider>();
            var resourceNode = new Mock<IContextualResourceModel>();
            var resRepo = new Mock<IResourceRepository>();
            var resRepo2 = new Mock<IResourceRepository>();
            var id = Guid.NewGuid();

            //Setup Servers
            resRepo.Setup(c => c.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Verifiable();
            resRepo.Setup(c => c.DeployResources(It.IsAny<IEnvironmentModel>(), It.IsAny<IEnvironmentModel>(),
                                       It.IsAny<IDeployDto>(), It.IsAny<IEventAggregator>())).Verifiable();

            resRepo.Setup(c => c.All()).Returns(new List<IResourceModel>());
            resRepo2.Setup(c => c.All()).Returns(new List<IResourceModel>());

            server.Setup(svr => svr.IsConnected).Returns(true);
            server.Setup(svr => svr.Connection).Returns(DebugOutputViewModelTest.CreateMockConnection(new Random(), new string[0]).Object);
            server.Setup(svr => svr.ResourceRepository).Returns(resRepo.Object);

            secondServer.Setup(svr => svr.IsConnected).Returns(true);
            secondServer.Setup(svr => svr.Connection).Returns(DebugOutputViewModelTest.CreateMockConnection(new Random(), new string[0]).Object);
            secondServer.Setup(svr => svr.ResourceRepository).Returns(resRepo2.Object);

            mockedServerRepo.Setup(svr => svr.Fetch(It.IsAny<IEnvironmentModel>())).Returns(server.Object);

            provider.Setup(prov => prov.Load()).Returns(new List<IEnvironmentModel> { server.Object, secondServer.Object });

            const string expectedResourceName = "Test Resource";
            var initialResource = new Mock<IContextualResourceModel>();
            initialResource.Setup(res => res.Environment).Returns(server.Object);
            initialResource.Setup(res => res.ResourceName).Returns(expectedResourceName);

            //Setup Navigation Tree
            var eventAggregator = new Mock<IEventAggregator>().Object;
            var mockedSource = new NavigationViewModel(eventAggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, It.IsAny<Guid>(), mockedServerRepo.Object);

            var treeParent = new CategoryTreeViewModel(eventAggregator, null, "Test Category", ResourceType.WorkflowService)
            {
                IsExpanded = false
            };


            resourceNode.Setup(res => res.ResourceName).Returns(expectedResourceName);
            resourceNode.Setup(res => res.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            resourceNode.Setup(res => res.ID).Returns(id);

            var resourceTreeNode = new ResourceTreeViewModel(eventAggregator, treeParent, resourceNode.Object);

            //Setup Server Resources
            resourceTreeNode.IsChecked = true;
            server.Setup(svr => svr.LoadResources()).Callback(() => mockedSource.Root.Add(resourceTreeNode));

            var deployViewModel = new DeployViewModel(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, provider.Object, mockedServerRepo.Object, new Mock<IEventAggregator>().Object)
            {
                Source = mockedSource
            };

            //------------Execute Test--------------------------- 
            deployViewModel.DeployCommand.Execute(null);

            resRepo.Verify(
                sender =>
                sender.DeployResources(It.IsAny<IEnvironmentModel>(), It.IsAny<IEnvironmentModel>(),
                                       It.IsAny<IDeployDto>(), It.IsAny<IEventAggregator>()));
        }

        #endregion

    }
}
