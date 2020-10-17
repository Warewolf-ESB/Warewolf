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
        [Timeout(100)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeploySourceExplorerViewModel_Ctor_valid")]
        public void TestDispose()
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


    [TestClass]
    public class ManageDatabaseSourceModellTests
    {

      /*  [TestMethod]
        [Timeout(100)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeploySourceExplorerViewModel_Ctor_valid")]
        public void TestDispose()
        {
            var vm = new ManageDatabaseSourceViewModel(new Mock<IAsyncWorker>().Object );
            var ns = new Mock<IRequestServiceNameViewModel>();
            Task<IRequestServiceNameViewModel> t = new Task<IRequestServiceNameViewModel>(()=> ns.Object );
            t.Start();
            vm.RequestServiceNameViewModel = t;

            vm.Dispose();
            ns.Verify(a=>a.Dispose());
        }*/

    }
}
