using System.Xml.Linq;
using Dev2.Runtime.ResourceUpgrades;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ResourceUpgraders
{
    [TestClass]
    public class BaseUpgradeConvertorTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BaseResourceUpgrader_Upgrade")]
// ReSharper disable InconsistentNaming
        public void BaseResourceUpgrader_Upgrade_HasMatchin_ExpectReplace()

        {
            //------------Setup for test--------------------------
            var baseResourceUpgrader = new BaseResourceUpgrader();
            
            //------------Execute Test---------------------------
            Assert.AreEqual("<a>clr-namespace:Dev2.Common.Interfaces.Infrastructure.Providers.Errors;assembly=Dev2.Common.Interfaces</a>", baseResourceUpgrader.UpgradeFunc(XElement.Parse("<a>clr-namespace:Dev2.Providers.Errors;assembly=Dev2.Infrastructure</a>")).ToString());
            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BaseResourceUpgrader_Upgrade")]
        public void BaseResourceUpgrader_Upgrade_HasMatchin_ExpectReplaceAlt()
        {
            //------------Setup for test--------------------------
            var baseResourceUpgrader = new BaseResourceUpgrader();

            //------------Execute Test---------------------------
            Assert.AreEqual("<a>clr-namespace:Dev2.Common.Interfaces.Core.Convertors.Case;assembly=Dev2.Common.Interfaces</a>", baseResourceUpgrader.UpgradeFunc(XElement.Parse("<a>clr-namespace:Dev2.Interfaces;assembly=Dev2.Core</a>")).ToString());
            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BaseResourceUpgrader_Upgrade")]
        public void BaseResourceUpgrader_Upgrade_NoMatch_NoReplace()
        {
            //------------Setup for test--------------------------
            var baseResourceUpgrader = new BaseResourceUpgrader();

            //------------Execute Test---------------------------
            Assert.AreEqual("<a>bob</a>", baseResourceUpgrader.UpgradeFunc( XElement.Parse("<a>bob</a>")).ToString());
            //------------Assert Results-------------------------
        }
        // ReSharper restore InconsistentNaming
    }
}