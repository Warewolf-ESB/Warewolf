
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
