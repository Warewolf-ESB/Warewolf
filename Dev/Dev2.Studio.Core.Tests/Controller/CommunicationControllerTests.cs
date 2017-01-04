using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;

// ReSharper disable InconsistentNaming

namespace Dev2.Core.Tests.Controller
{
    [TestClass]
    public class CommunicationControllerTests
    {

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteCommand_GivenReturnAuthorizationError_ShouldShowCorrectPopup()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPopupController>();
            mock.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false, false, false));
            CustomContainer.Register(mock.Object);
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(environmentConnection => environmentConnection.IsConnected).Returns(true);
            var serializer = new Dev2JsonSerializer();
            var message = new ExecuteMessage
            {
                HasError = true,
                Message = ErrorResource.NotAuthorizedToCreateException.ToStringBuilder()
            };
            connection.Setup(environmentConnection => environmentConnection.ExecuteCommand(It.IsAny<StringBuilder>(), GlobalConstants.ServerWorkspaceID))
                .Returns(serializer.SerializeToBuilder(message));
            var controller = new CommunicationController();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            controller.ExecuteCommand<ExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID);
            //---------------Test Result -----------------------
            mock.Verify(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false, false, false), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteCommand_GivenReturnExploreAuthorizationError_ShouldShowCorrectPopup()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPopupController>();
            mock.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false, false, false));
            CustomContainer.Register(mock.Object);
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(environmentConnection => environmentConnection.IsConnected).Returns(true);
            var serializer = new Dev2JsonSerializer();

            var message = new ExecuteMessage
            {
                HasError = true,
                Message = new StringBuilder(ErrorResource.NotAuthorizedToExecuteException)
            };

            var serializeToBuilder = serializer.SerializeToBuilder(message);
            connection.Setup(environmentConnection => environmentConnection.ExecuteCommand(It.IsAny<StringBuilder>(), GlobalConstants.ServerWorkspaceID))
                .Returns(serializeToBuilder);
            var controller = new CommunicationController();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            controller.ExecuteCommand<ExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID);
            //---------------Test Result -----------------------
            mock.Verify(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false, false, false), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteCommandAsync_GivenReturnExploreAuthorizationError_ShouldShowCorrectPopup()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPopupController>();
            mock.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false, false, false));
            CustomContainer.Register(mock.Object);
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(environmentConnection => environmentConnection.IsConnected).Returns(true);
            var serializer = new Dev2JsonSerializer();

            var message = new ExecuteMessage
            {
                HasError = true,
                Message = new StringBuilder(ErrorResource.NotAuthorizedToExecuteException)
            };

            var serializeToBuilder = serializer.SerializeToBuilder(message);
            connection.Setup(environmentConnection => environmentConnection.ExecuteCommandAsync(It.IsAny<StringBuilder>(), GlobalConstants.ServerWorkspaceID))
                .Returns(Task.FromResult(serializeToBuilder));
            var controller = new CommunicationController();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            var explorerRepositoryResult = controller.ExecuteCommandAsync<ExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID).Result;
            //---------------Test Result -----------------------
            mock.Verify(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false, false, false), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteCommandAsync_GivenHasAuthorizationError_ShouldShowCorrectPopup()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPopupController>();
            mock.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false, false, false));
            CustomContainer.Register(mock.Object);
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(environmentConnection => environmentConnection.IsConnected).Returns(true);
            connection.Setup(environmentConnection => environmentConnection.ExecuteCommandAsync(It.IsAny<StringBuilder>(), GlobalConstants.ServerWorkspaceID))
                .Throws(new ServiceNotAuthorizedException(ErrorResource.NotAuthorizedToCreateException));
            var controller = new CommunicationController();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            controller.ExecuteCommandAsync<ExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID).ContinueWith((d) =>
            {
                //---------------Test Result -----------------------
                Assert.IsNotNull(d);
                mock.Verify(c => c.Show(ErrorResource.NotAuthorizedToCreateException, "ServiceNotAuthorizedException", MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false, false, false), Times.Once);
            });
            
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteCommandAsync_GivenHasAuthorizationError_ShouldShowCorrectPopup_Aggregation()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPopupController>();
            mock.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false, false, false));
            CustomContainer.Register(mock.Object);
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(environmentConnection => environmentConnection.IsConnected).Returns(true);
            var aggregateException = new AggregateException();
            
            connection.Setup(environmentConnection => environmentConnection.ExecuteCommandAsync(It.IsAny<StringBuilder>(), GlobalConstants.ServerWorkspaceID))
                .Throws(aggregateException);
            var controller = new CommunicationController();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            controller.ExecuteCommandAsync<ExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID).ContinueWith((d) =>
            {
                //---------------Test Result -----------------------
                Assert.IsNotNull(d);
                mock.Verify(c => c.Show(ErrorResource.NotAuthorizedToCreateException, "ServiceNotAuthorizedException", MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false, false, false), Times.Never);
            });
            
        }
    }
}
