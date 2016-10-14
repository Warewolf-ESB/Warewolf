using System.Text;
using System.Windows;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Hosting;
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
        public void ExecuteCommand_GivenHasAuthorizationError_ShouldShowCorrectPopup()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPopupController>();
            mock.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false));
            CustomContainer.Register(mock.Object);
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(environmentConnection => environmentConnection.IsConnected).Returns(true);
            connection.Setup(environmentConnection => environmentConnection.ExecuteCommand(It.IsAny<StringBuilder>(), GlobalConstants.ServerWorkspaceID))
                .Throws(new ServiceNotAuthorizedException(ErrorResource.NotAuthorizedToCreateException));
            CommunicationController controller = new CommunicationController();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            controller.ExecuteCommand<ExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID);
            //---------------Test Result -----------------------
            mock.Verify(c => c.Show(ErrorResource.NotAuthorizedToCreateException, "ServiceNotAuthorizedException", MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteCommand_GivenReturnAuthorizationError_ShouldShowCorrectPopup()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPopupController>();
            mock.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false));
            CustomContainer.Register(mock.Object);
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(environmentConnection => environmentConnection.IsConnected).Returns(true);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            ExecuteMessage message = new ExecuteMessage
            {
                HasError = true,
                Message = ErrorResource.NotAuthorizedToCreateException.ToStringBuilder()
            };
            connection.Setup(environmentConnection => environmentConnection.ExecuteCommand(It.IsAny<StringBuilder>(), GlobalConstants.ServerWorkspaceID))
                .Returns(serializer.SerializeToBuilder(message));
            CommunicationController controller = new CommunicationController();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            controller.ExecuteCommand<ExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID);
            //---------------Test Result -----------------------
            mock.Verify(c => c.Show(ErrorResource.NotAuthorizedToCreateException, "ServiceNotAuthorizedException", MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteCommand_GivenReturnExploreAuthorizationError_ShouldShowCorrectPopup()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IPopupController>();
            mock.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false));
            CustomContainer.Register(mock.Object);
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(environmentConnection => environmentConnection.IsConnected).Returns(true);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            ExplorerRepositoryResult message = new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.NotAuthorizedToCreateException);

            var serializeToBuilder = serializer.SerializeToBuilder(message);
            connection.Setup(environmentConnection => environmentConnection.ExecuteCommand(It.IsAny<StringBuilder>(), GlobalConstants.ServerWorkspaceID))
                .Returns(serializeToBuilder);
            CommunicationController controller = new CommunicationController();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            controller.ExecuteCommand<ExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID);
            //---------------Test Result -----------------------
            mock.Verify(c => c.Show(ErrorResource.NotAuthorizedToCreateException, "ServiceNotAuthorizedException", MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false), Times.Once);
        }
    }
}
