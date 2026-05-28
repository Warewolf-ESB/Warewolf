/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class CatalogFactoryAndRecordsetListWrapperTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ResourceCatalogFactory))]
        public void ResourceCatalogFactory_New_ReturnsSharedInstance()
        {
            var factory = new ResourceCatalogFactory();

            var first = factory.New();
            var second = factory.New();

            Assert.IsNotNull(first);
            Assert.AreSame(first, second);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(TriggersCatalogFactory))]
        public void TriggersCatalogFactory_New_ReturnsSharedInstance()
        {
            var factory = new TriggersCatalogFactory();

            var first = factory.New();
            var second = factory.New();

            Assert.IsNotNull(first);
            Assert.AreSame(first, second);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(RecordsetListWrapper))]
        public void RecordsetListWrapper_Properties_RoundTrip()
        {
            var list = new RecordsetList();
            var description = new Mock<IOutputDescription>().Object;

            var wrapper = new RecordsetListWrapper
            {
                RecordsetList = list,
                Description = description,
                SerializedResult = "json",
            };

            Assert.AreSame(list, wrapper.RecordsetList);
            Assert.AreSame(description, wrapper.Description);
            Assert.AreEqual("json", wrapper.SerializedResult);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(RecordsetList))]
        public void RecordsetList_Description_RoundTrip()
        {
            var description = new Mock<IOutputDescription>().Object;
            var list = new RecordsetList { Description = description };

            Assert.AreSame(description, list.Description);
        }
    }
}
