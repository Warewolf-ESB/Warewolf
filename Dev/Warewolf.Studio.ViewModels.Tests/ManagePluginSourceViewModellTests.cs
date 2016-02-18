using Dev2.Common.Interfaces.SaveDialog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManagePluginSourceViewModellTests
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeploySourceExplorerViewModel_Ctor_valid")]
        public void TestDispose()
        {
            var vm = new ManagePluginSourceViewModel();
            var ns = new Mock<IRequestServiceNameViewModel>();

            vm.RequestServiceNameViewModel = ns.Object;

            vm.Dispose();
            ns.Verify(a => a.Dispose());
        }

    }
}