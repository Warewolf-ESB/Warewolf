
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Xml.Linq;
using Dev2.Runtime.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class UpgradePathTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpgradePath_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
// ReSharper disable InconsistentNaming
        public void UpgradePath_Ctor_NullParamsFrom_ExpectException()

        {
            //------------Setup for test--------------------------
            new UpgradePath(null,new Version(),new ResourceUpgrade(a=>a) );

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpgradePath_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpgradePath_Ctor_NullParamsTo_ExpectException()
        {
            //------------Setup for test--------------------------
            new UpgradePath(null, new Version(), new ResourceUpgrade(a => a));

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpgradePath_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpgradePath_Ctor_NullParamsUpgrade_ExpectException()
        {
            //------------Setup for test--------------------------
            new UpgradePath(new Version(), new Version(), null);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpgradePath_Ctor")]

        public void UpgradePath_CanUpgrade_NoVersion_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var upgrade = new UpgradePath(new Version(0,0), new Version(2,0), new ResourceUpgrade(a=>a));

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(upgrade.CanUpgrade(XElement.Parse("<a></a>")));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpgradePath_Ctor")]

        public void UpgradePath_CanUpgrade_NoVersion_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var upgrade = new UpgradePath(new Version(1, 0), new Version(2, 0), new ResourceUpgrade(a => a));

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsFalse(upgrade.CanUpgrade(XElement.Parse("<a ServerVersion=\"3.4.5.6\"></a>")));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpgradePath_Ctor")]

        public void UpgradePath_CanUpgrade_Boundary_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var upgrade = new UpgradePath(new Version(1, 0), new Version(2, 0), new ResourceUpgrade(a => a));

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsFalse(upgrade.CanUpgrade(XElement.Parse("<a ServerVersion=\"2.0.0.0\"></a>")));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpgradePath_Ctor")]

        public void UpgradePath_CanUpgrade_UpperBoundary_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var upgrade = new UpgradePath(new Version(1, 0), new Version(2, 0), new ResourceUpgrade(a => a));

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsFalse(upgrade.CanUpgrade(XElement.Parse("<a ServerVersion=\"1.0.0.0\"></a>")));
        }
            // ReSharper restore InconsistentNaming
    }
}
