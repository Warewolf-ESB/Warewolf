using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
   public class ResourceUpgraderTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [ExpectedException(typeof(ArgumentNullException ))]
        [TestCategory("ResourceUpgrader_Ctor")]
        public void ResourceUpgrader_Ctor_NulParams_ExpectError()
        {
            //------------Setup for test--------------------------
            new ResourceUpgrader(null);
            

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceUpgrader_Properties")]
        public void ResourceUpgrader_Properties()
        {
            //------------Setup for test--------------------------
            var path = new List<IUpgradePath>();
            var a = new ResourceUpgrader(path);
            Assert.AreEqual(path,a.AvailableUpgrades);


        }
         [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceUpgrader_Ctor")]
        public void ResourceUpgrader_Upgrade_NoDictionary()
        {
            //------------Setup for test--------------------------
            var a = new ResourceUpgrader(new List<IUpgradePath>());


            //------------Execute Test---------------------------
           var upgraded = a.UpgradeResource(XElement.Parse("<a></a>"), new Version(1, 2),(x=>{}));
            //------------Assert Results-------------------------
           Assert.AreEqual(upgraded.ToString(), "<a></a>");
        }

               [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceUpgrader_Ctor")]
        public void ResourceUpgrader_Upgrade_EmptyDictionary()
        {
            //------------Setup for test--------------------------
            var a = new ResourceUpgrader(new List<IUpgradePath>());


            //------------Execute Test---------------------------
            var upgraded = a.UpgradeResource(XElement.Parse("<a></a>"), new Version(1, 2), (x => { }));
            //------------Assert Results-------------------------
            Assert.AreEqual(upgraded.ToString(), "<a></a>");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceUpgrader_Ctor")]
        public void ResourceUpgrader_Upgrade_HasDictionaryDictionary()
        {
            //------------Setup for test--------------------------
            var upgradePaths = new List<IUpgradePath>();
            var source =  XElement.Parse("<a></a>");
            var upgrade1 = new Mock<IUpgradePath>();
            upgrade1.Setup(a => a.UpgradesFrom).Returns(new Version());
            upgrade1.Setup(a => a.UpgradesTo).Returns(new Version());
            var mockUpgrade = new Mock<IResourceUpgrade>();
            upgradePaths.Add(upgrade1.Object);

            mockUpgrade.Setup(a => a.UpgradeFunc).Returns(a => XElement.Parse((a.ToString().Replace("a", "b"))));
            upgrade1.Setup(a=>a.CanUpgrade(source)).Returns(true);
            upgrade1.Setup(a => a.Upgrade).Returns(mockUpgrade.Object);
            var upgrader = new ResourceUpgrader(upgradePaths);


            //------------Execute Test---------------------------
            var upgraded = upgrader.UpgradeResource(source, new Version(1, 2), (x => { }));
            //------------Assert Results-------------------------
            Assert.AreEqual(upgraded.ToString(), "<b ServerVersion=\"" +upgrader.GetType().Assembly.GetName().Version + "\"></b>");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceUpgrader_Ctor")]
        public void ResourceUpgrader_Upgrade_HasDictionary_TwoUpgrades()
        {
            //------------Setup for test--------------------------
            var upgradePaths = new List<IUpgradePath>();
            var source = XElement.Parse("<a></a>");
            var upgrade1 = new Mock<IUpgradePath>();
            upgrade1.Setup(a => a.UpgradesFrom).Returns(new Version(1,1));
            upgrade1.Setup(a => a.UpgradesTo).Returns(new Version(2,1));
            var upgrade2 = new Mock<IUpgradePath>();
            upgrade2.Setup(a => a.UpgradesFrom).Returns(new Version(2,1));
            upgrade2.Setup(a => a.UpgradesTo).Returns(new Version(3,1));

            var mockUpgrade = new Mock<IResourceUpgrade>();
            var mockUpgrade2 = new Mock<IResourceUpgrade>();
            upgradePaths.Add(upgrade1.Object);
            upgradePaths.Add(upgrade2.Object);
            mockUpgrade.Setup(a => a.UpgradeFunc).Returns(a => XElement.Parse((a.ToString().Replace("a", "b"))));
            mockUpgrade2.Setup(a => a.UpgradeFunc).Returns(a => XElement.Parse((a.ToString().Replace("b", "c"))));
            upgrade1.Setup(a => a.CanUpgrade(source)).Returns(true);
            upgrade1.Setup(a => a.Upgrade).Returns(mockUpgrade.Object);
            upgrade2.Setup(a => a.CanUpgrade(source)).Returns(true);
            upgrade2.Setup(a => a.Upgrade).Returns(mockUpgrade2.Object);
            var upgrader = new ResourceUpgrader(upgradePaths);


            //------------Execute Test---------------------------
            var upgraded = upgrader.UpgradeResource(source, new Version(1, 2), (x => { }));
            //------------Assert Results-------------------------
            Assert.AreEqual(upgraded.ToString(), "<c ServerVersion=\"" + upgrader.GetType().Assembly.GetName().Version + "\"></c>");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceUpgrader_Ctor")]
        public void ResourceUpgrader_Upgrade_HasDictionary_TwoUpgrades_Only1Matches()
        {
            //------------Setup for test--------------------------
            var upgradePaths = new List<IUpgradePath>();
            var source = XElement.Parse("<a></a>");
            var upgrade1 = new Mock<IUpgradePath>();
            upgrade1.Setup(a => a.UpgradesFrom).Returns(new Version(1, 1));
            upgrade1.Setup(a => a.UpgradesTo).Returns(new Version(2, 1));
            var upgrade2 = new Mock<IUpgradePath>();
            upgrade2.Setup(a => a.UpgradesFrom).Returns(new Version(2, 1));
            upgrade2.Setup(a => a.UpgradesTo).Returns(new Version(3, 1));

            var mockUpgrade = new Mock<IResourceUpgrade>();
            var mockUpgrade2 = new Mock<IResourceUpgrade>();
            upgradePaths.Add(upgrade1.Object);
            upgradePaths.Add(upgrade2.Object);
            mockUpgrade.Setup(a => a.UpgradeFunc).Returns(a => XElement.Parse((a.ToString().Replace("a", "b"))));
            mockUpgrade2.Setup(a => a.UpgradeFunc).Returns(a => XElement.Parse((a.ToString().Replace("b", "c"))));
            upgrade1.Setup(a => a.CanUpgrade(source)).Returns(true);
            upgrade1.Setup(a => a.Upgrade).Returns(mockUpgrade.Object);
            upgrade2.Setup(a => a.CanUpgrade(source)).Returns(false);
            upgrade2.Setup(a => a.Upgrade).Returns(mockUpgrade2.Object);
            var upgrader = new ResourceUpgrader(upgradePaths);


            //------------Execute Test---------------------------
            var upgraded = upgrader.UpgradeResource(source, new Version(1, 2), (x => { }));
            //------------Assert Results-------------------------
            Assert.AreEqual(upgraded.ToString(), "<b ServerVersion=\"" + upgrader.GetType().Assembly.GetName().Version + "\"></b>");
        }

    }
}
