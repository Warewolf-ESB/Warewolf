using System.Threading.Tasks;
using Dev2.Common.Interfaces.SaveDialog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageEmailSourceViewModelTest
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeploySourceExplorerViewModel_Ctor_valid")]
        public void TestDispose()
        {
            var vm = new ManageEmailSourceViewModel();
            var ns = new Mock<IRequestServiceNameViewModel>();
            Task<IRequestServiceNameViewModel> t = new Task<IRequestServiceNameViewModel>(() => ns.Object);
            t.Start();
            vm.RequestServiceNameViewModel = t;

            vm.Dispose();
            ns.Verify(a => a.Dispose());
        }

    }
}