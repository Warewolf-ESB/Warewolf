using Dev2.Studio.Factory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DialogFactoryTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DialogViewModelFactory_CreateDialogAbout")]
        // ReSharper disable InconsistentNaming
        public void DialogViewModelFactory_CreateDialogAbout_Create_ReturnsDialog()
    
        {
            //------------Setup for test--------------------------
            var dialogViewModelFactory = new DialogViewModelFactory();
            bool called = false;
            dialogViewModelFactory.SetupDialogAction = (model, s, arg3) => { called = true; };
            dialogViewModelFactory.CreateAboutDialog();
            Assert.IsTrue(called);
            //------------Assert Results-------------------------
        }

            [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DialogViewModelFactory_CreateDialogAbout")]
        public void DialogViewModelFactory_CreateServerDialogAbout_Create_ReturnsDialog()
        {
            //------------Setup for test--------------------------
            var dialogViewModelFactory = new DialogViewModelFactory();
            bool called = false;
            dialogViewModelFactory.SetupServerDialogAction = (model, s, arg3,arg4) => { called = true; Assert.AreEqual("1,2,3,4,5",arg4); };
            dialogViewModelFactory.CreateServerAboutDialog("1,2,3,4,5");
            Assert.IsTrue(called);
            //------------Assert Results-------------------------
        }
    }
}
// ReSharper restore InconsistentNaming
