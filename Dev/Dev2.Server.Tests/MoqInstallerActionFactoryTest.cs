using Dev2.InstallerActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Server.Tests
{
    /// <summary>
    /// Summary description for MoqInstallerActionFactoryTest
    /// </summary>
    [TestClass]
    public class MoqInstallerActionFactoryTest
    {

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("MoqInstallerActionFactory_CreateInstallerActions")]
        public void MoqInstallerActionFactory_CreateInstallerActions_WhenCreatingNew_ExpectNewObject()
        {
            //------------Execute Test---------------------------
            var result = MoqInstallerActionFactory.CreateInstallerActions();

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("MoqInstallerActionFactory_CreateSecurityOperationsObject")]
        public void MoqInstallerActionFactory_CreateSecurityOperationsObject_WhenCreatingNew_ExpectNewObject()
        {
            //------------Execute Test---------------------------
            var result = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
        }
    }
}
