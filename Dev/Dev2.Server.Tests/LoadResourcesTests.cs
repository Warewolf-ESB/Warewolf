/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class LoadResourcesTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(LoadResources))]
        public void LoadResources_LoadExamplesViaBuilder_Success()
        {
            //------------------Arrange---------------
            var mockWriter = new Mock<IWriter>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();

            mockDirectory.Setup(o => o.Exists(It.IsAny<string>())).Returns(true);
            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockResourceCatalog.Setup(o => o.LoadExamplesViaBuilder(It.IsAny<string>()))
                .Callback<string>((path) => Assert.IsTrue(path.EndsWith(@"\Resources - ServerTests")))
                .Returns(()=> null).Verifiable();
            //------------------Act-------------------
            var loadResources =  new LoadResources("Resources - ServerTests", mockWriter.Object, mockDirectory.Object,mockResourceCatalogFactory.Object);
            loadResources.CheckExampleResources();
            //------------------Assert----------------
            mockResourceCatalog.Verify();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(LoadResources))]
        public void LoadResources_LoadActivityCache_Success()
        {
            //------------------Arrange---------------
            var mockWriter = new Mock<IWriter>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();

            mockDirectory.Setup(o => o.Exists(It.IsAny<string>())).Returns(true);
            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockResourceCatalog.Setup(o => o.LoadServerActivityCache()).Verifiable();
            //------------------Act-------------------
            var loadResources = new LoadResources("Resources - ServerTests", mockWriter.Object, mockDirectory.Object, mockResourceCatalogFactory.Object);
            loadResources.LoadActivityCache();
            //------------------Assert----------------
            mockResourceCatalog.Verify();
        }
    }
}
