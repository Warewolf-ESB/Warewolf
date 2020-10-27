using Dev2;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class RequestNameServiceViewModelTests
    {
        [TestMethod]
        [Timeout(250)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeploySourceExplorerViewModel_Ctor_valid")]
        public void RequestNameServiceViewModel_Dispose()
        {
            var serverRepo = new Mock<IServerRepository>();
            var connectionObject = new Mock<IEnvironmentConnection>();
            serverRepo.Setup(repository => repository.ActiveServer.Connection).Returns(connectionObject.Object);
            CustomContainer.Register(serverRepo.Object);
            var vm = new RequestServiceNameViewModel();
            var x = new Mock<IExplorerViewModel>();
            var p = new PrivateObject(vm);
            var env = new Mock<IEnvironmentViewModel>();
            p.SetField("_environmentViewModel", env.Object);
            vm.SingleEnvironmentExplorerViewModel = x.Object;
            vm.Dispose();
            x.Verify(a=>a.Dispose());
            env.Verify(a => a.Dispose());
        }
    }
}
