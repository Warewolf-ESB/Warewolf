/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    [TestClass]
    public class NamespaceAndServiceMethodListTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("NamespaceList")]
        public void NamespaceList_ToString_SerializesItems()
        {
            var list = new NamespaceList
            {
                new NamespaceItem { AssemblyName = "Asm", FullName = "Some.Type", MethodName = "Do" }
            };

            var json = list.ToString();

            StringAssert.Contains(json, "Some.Type");
            StringAssert.Contains(json, "Do");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("NamespaceList")]
        public void NamespaceItem_ToString_SerializesProperties()
        {
            var item = new NamespaceItem
            {
                AssemblyLocation = "c:\\asm.dll",
                AssemblyName = "Asm",
                FullName = "Some.Type",
                MethodName = "Do",
                JsonObject = "{}"
            };

            var json = item.ToString();

            StringAssert.Contains(json, "c:");
            StringAssert.Contains(json, "Asm");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ServiceMethodList")]
        public void ServiceMethodList_ToString_SerializesEmptyListAsJsonArray()
        {
            var list = new ServiceMethodList();

            Assert.AreEqual("[]", list.ToString());
        }
    }
}
