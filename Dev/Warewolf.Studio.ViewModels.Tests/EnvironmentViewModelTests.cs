using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class 
        EnvironmentViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_Constructor")]
        public void EnvironmentViewModel_Constructor_ServerPassedIn_ShouldSetServerProperty()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            var shellViewModelMock = new Mock<IShellViewModel>();
            
            //------------Execute Test---------------------------
            IEnvironmentViewModel environmentViewModel = new EnvironmentViewModel(server.Object, shellViewModelMock.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(environmentViewModel);
            Assert.IsNotNull(environmentViewModel.Server);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnvironmentViewModel_Constructor_NullServer_ArgumentNullException()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            
            //------------Execute Test---------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new EnvironmentViewModel(null,shellViewModelMock.Object);
            //------------Assert Results-------------------------
        }  
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnvironmentViewModel_Constructor_NullShellViewModel_ArgumentNullException()
        {
            //------------Setup for test--------------------------
            var mockServer = new Mock<IServer>();
            
            //------------Execute Test---------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new EnvironmentViewModel(mockServer.Object,null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_Connect")]
        public void EnvironmentViewModel_Connect_ShouldCallConnectOnEnvironment()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            var shellViewModelMock = new Mock<IShellViewModel>();
            server.Setup(server1 => server1.Connect()).Returns(new Task<bool>(() => false));
            var environmentViewModel = new EnvironmentViewModel(server.Object, shellViewModelMock.Object);
            
            //------------Execute Test---------------------------
            environmentViewModel.Connect();
            //------------Assert Results-------------------------
            server.Verify();
            Assert.IsFalse(environmentViewModel.IsConnected);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_Connect")]
        public void EnvironmentViewModel_Connect_WhenServerDoesConnect_ShouldBeConnected()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            var shellViewModelMock = new Mock<IShellViewModel>();
            server.Setup(server1 => server1.Connect()).Returns(Task.FromResult(true)).Verifiable();
            var environmentViewModel = new EnvironmentViewModel(server.Object, shellViewModelMock.Object);
            //------------Execute Test---------------------------
            environmentViewModel.Connect();
            //------------Assert Results-------------------------
            server.Verify();
            Assert.IsTrue(environmentViewModel.IsConnected);
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_Constructor")]
        public void EnvironmentViewModel_Constructor_NewCommandHasResourceTypeParameter()
        {
            //------------Setup for test--------------------------
            ResourceType? resourceTypeParameter = null;
            var shellViewModelMock = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            shellViewModelMock.Setup(model => model.NewResource(It.IsAny<ResourceType?>(), Guid.Empty)).Callback((ResourceType? resourceType,Guid selectedGuid) => resourceTypeParameter = resourceType);
            //------------Execute Test---------------------------
            var environmentViewModel = new EnvironmentViewModel(server.Object,shellViewModelMock.Object);
            //------------Assert Results-------------------------
            environmentViewModel.NewCommand.Execute(ResourceType.DbService);
            shellViewModelMock.Verify(model => model.NewResource(It.IsAny<ResourceType>(), Guid.Empty), Times.Once());
            Assert.IsNotNull(resourceTypeParameter);
            Assert.AreEqual(ResourceType.DbService, resourceTypeParameter);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_Load")]
        public void EnvironmentViewModel_Load_WhenServerNotConnected_ShouldNotLoadResources()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            var shellViewModelMock = new Mock<IShellViewModel>();
            server.Setup(server1 => server1.LoadExplorer()).Returns(new Task<IExplorerItem>(() => null)).Verifiable();
            var environmentModel = new EnvironmentViewModel(server.Object, shellViewModelMock.Object);
            
            //------------Execute Test---------------------------
            environmentModel.Load();
            //------------Assert Results-------------------------
            Assert.IsFalse(environmentModel.IsLoaded);
            server.Verify(server1 => server1.Load(),Times.Never());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_Load")]
        public void EnvironmentViewModel_Load_WhenServerConnected_ShouldLoadResourcesFromServer()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            var shellViewModelMock = new Mock<IShellViewModel>();
            server.Setup(server1 => server1.Connect()).Returns(Task.FromResult(true));
            server.Setup(server1 => server1.LoadExplorer()).Returns(Task.FromResult(new Mock<IExplorerItem>().Object)).Verifiable();
            var environmentModel = new EnvironmentViewModel(server.Object, shellViewModelMock.Object);
            environmentModel.Connect();
            //------------Execute Test---------------------------
            environmentModel.Load();
            //------------Assert Results-------------------------
            server.Verify();
            Assert.IsTrue(environmentModel.IsLoaded);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_Load")]
        public void EnvironmentViewModel_Load_ShouldCreateExplorerItems()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            var resourceWithNoChildren = new Mock<IExplorerItem>();
            resourceWithNoChildren.Setup(resource => resource.Children).Returns(new List<IExplorerItem>());
            var resourceWithOneLevelChildren = new Mock<IExplorerItem>();
            resourceWithOneLevelChildren.Setup(resource => resource.Children).Returns(new List<IExplorerItem> { new Mock<IExplorerItem>().Object });
            var resourceWithMultipleLevelChildren = new Mock<IExplorerItem>();
            var resourceWithChildren = new Mock<IExplorerItem>();
            var childResource = new Mock<IExplorerItem>();
            resourceWithChildren.Setup(resource => resource.Children).Returns(new List<IExplorerItem> { childResource.Object });
            var anotherResourceAsChildren = new Mock<IExplorerItem>();
            resourceWithMultipleLevelChildren.Setup(resource => resource.Children).Returns(new List<IExplorerItem>
            {
                resourceWithChildren.Object,anotherResourceAsChildren.Object
            });
            var server = new Mock<IServer>();
            server.Setup(server1 => server1.Connect()).Returns(Task.FromResult(true));
            var mock = new Mock<IExplorerItem>();
            mock.Setup(item => item.Children).Returns(new List<IExplorerItem> { resourceWithNoChildren.Object, resourceWithOneLevelChildren.Object, resourceWithMultipleLevelChildren.Object });
            server.Setup(server1 => server1.LoadExplorer()).Returns(Task.FromResult(mock.Object)).Verifiable();
            var environmentViewModel = new EnvironmentViewModel(server.Object, shellViewModelMock.Object);
            environmentViewModel.Connect();
            //------------Execute Test---------------------------
            environmentViewModel.Load();
            //------------Assert Results-------------------------
            Assert.IsTrue(environmentViewModel.IsLoaded);
            Assert.AreEqual(3,environmentViewModel.Children.Count);
            Assert.AreEqual(1,environmentViewModel.Children.ToList()[1].Children.Count);
         }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_Filter")]
        public void EnvironmentViewModel_Filter_ShouldSetIsVisible_False_ExplorerItems()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            var resourceWithNoChildren = new Mock<IExplorerItem>();
            resourceWithNoChildren.Setup(item => item.DisplayName).Returns("my re");
            resourceWithNoChildren.Setup(resource => resource.Children).Returns(new List<IExplorerItem>());
            var resourceWithOneLevelChildren = new Mock<IExplorerItem>();
            resourceWithOneLevelChildren.Setup(resource => resource.Children).Returns(new List<IExplorerItem> { new Mock<IExplorerItem>().Object });
            var resourceWithMultipleLevelChildren = new Mock<IExplorerItem>();
            var resourceWithChildren = new Mock<IExplorerItem>();
            var childResource = new Mock<IExplorerItem>();
            resourceWithChildren.Setup(resource => resource.Children).Returns(new List<IExplorerItem> { childResource.Object });
            var anotherResourceAsChildren = new Mock<IExplorerItem>();
            resourceWithMultipleLevelChildren.Setup(resource => resource.Children).Returns(new List<IExplorerItem>
            {
                resourceWithChildren.Object,anotherResourceAsChildren.Object
            });
            var server = new Mock<IServer>();
            server.Setup(server1 => server1.Connect()).Returns(Task.FromResult(true));
            var mock = new Mock<IExplorerItem>();
            mock.Setup(item => item.Children).Returns(new List<IExplorerItem> { resourceWithNoChildren.Object, resourceWithOneLevelChildren.Object, resourceWithMultipleLevelChildren.Object });
            server.Setup(server1 => server1.LoadExplorer()).Returns(Task.FromResult(mock.Object)).Verifiable();
            var environmentViewModel = new EnvironmentViewModel(server.Object, shellViewModelMock.Object);
            environmentViewModel.Connect();
            environmentViewModel.Load();
            //------------Assert Preconditions-------------------
            Assert.IsTrue(environmentViewModel.IsLoaded);
            Assert.AreEqual(3, environmentViewModel.Children.Count);
            Assert.AreEqual(1, environmentViewModel.Children.ToList()[1].Children.Count);
            //------------Execute Test---------------------------
            environmentViewModel.Filter("re");
            //------------Assert Results-------------------------
            var filteredList = environmentViewModel.Children.ToList();
            Assert.IsTrue(filteredList[0].IsVisible);
            
         }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_ShowServerVersionCommand")]
        public void EnvironmentViewModel_ShowServerVersionCommand_ShouldCallShellViewModel()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            IPopupMessage returnedMessage = null;
            var shellViewModelMock = new Mock<IShellViewModel>();
            var studioVersion = Utils.FetchVersionInfo();
            shellViewModelMock.Setup(model => model.ShowPopup(It.IsAny<IPopupMessage>()))
                .Callback<IPopupMessage>(message => returnedMessage = message);
            server.Setup(server1 => server1.Connect()).Returns(Task.FromResult(true));
            server.Setup(server1 => server1.GetServerVersion()).Returns("1.0.0");
            server.Setup(server1 => server1.LoadExplorer()).Returns(Task.FromResult(new Mock<IExplorerItem>().Object)).Verifiable();
            var environmentModel = new EnvironmentViewModel(server.Object, shellViewModelMock.Object);
            //------------Execute Test---------------------------
            environmentModel.ShowServerVersionCommand.Execute(null);
            //------------Assert Results-------------------------
            var correctDescription = "Studio: Version " + studioVersion +
                                     Environment.NewLine +
                                     "Server: Version 1.0.0" +
                                     Environment.NewLine +
                                     Environment.NewLine +
                                     "Sofware development by: www.dev2.co.za" +
                                     Environment.NewLine;
            shellViewModelMock.Verify(model => model.ShowPopup(It.IsAny<IPopupMessage>()),Times.Once());
            server.Verify(server1 => server1.GetServerVersion(),Times.Once());
            Assert.IsNotNull(returnedMessage);
            Assert.AreEqual(correctDescription,returnedMessage.Description);
        }
    }
}
