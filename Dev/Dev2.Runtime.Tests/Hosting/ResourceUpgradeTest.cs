using System;
using System.Xml.Linq;
using Dev2.Runtime.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class ResourceUpgradeTest
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceUpgrade_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceUpgrade_Ctor_Null_Params_Func()
        {
            //------------Setup for test--------------------------
            // ReSharper disable ObjectCreationAsStatement
            new ResourceUpgrade( null);
            // ReSharper restore ObjectCreationAsStatement

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceUpgrade_Ctor")]
        public void ResourceUpgrade_Properties()
        {
            //------------Setup for test--------------------------
            // ReSharper disable ObjectCreationAsStatement
            var x = new Func<XElement,XElement>( async=>async);
            var a = new ResourceUpgrade(x);
            Assert.AreEqual(x,a.UpgradeFunc);
            // ReSharper restore ObjectCreationAsStatement

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
    }
}
