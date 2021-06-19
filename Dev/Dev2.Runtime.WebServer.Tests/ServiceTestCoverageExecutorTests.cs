﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework.Internal;
using System;

namespace Dev2.Runtime.WebServer.Tests
{
    [TestClass]
    public class ServiceTestCoverageExecutorTests
    {
        [TestMethod]
        [TestCategory("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestCoverageExecutor))]
        public void ServiceTestCoverageExecutor_GetTestCoverageReports_ShouldReturnBlank()
        {
            var sut = ServiceTestCoverageExecutor.GetTestCoverageReports(new Mock<ICoverageDataObject>().Object,Guid.NewGuid(),
                                                                        new Communication.Dev2JsonSerializer(),
                                                                        new Mock<ITestCoverageCatalog>().Object,
                                                                        new Mock<ITestCatalog>().Object,
                                                                        new Mock<IResourceCatalog>().Object,
                                                                        out string executePayload);

            Assert.IsNull(executePayload);
            Assert.AreEqual("HTML", sut.FormatName);
            Assert.AreEqual("text/html; charset=utf-8", sut.ContentType);
            Assert.AreEqual(EmitionTypes.Cover, sut.PublicFormatName);
        }

        [TestMethod]
        [TestCategory("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestCoverageExecutor))]
        public void ServiceTestCoverageExecutor_RunCoverageAndReturnJSON_ShouldReturnBlank()
        {
            var mockCoverageDataObject = new Mock<ICoverageDataObject>();
            mockCoverageDataObject.Setup(o => o.CoverageReportResourceIds).Returns(new[] { Guid.NewGuid() });
            mockCoverageDataObject.Setup(o => o.ReturnType).Returns(EmitionTypes.CoverJson);

            var sut = ServiceTestCoverageExecutor.GetTestCoverageReports(mockCoverageDataObject.Object, Guid.NewGuid(),
                                                                         new Communication.Dev2JsonSerializer(),
                                                                         new Mock<ITestCoverageCatalog>().Object,
                                                                         new Mock<ITestCatalog>().Object,
                                                                         new Mock<IResourceCatalog>().Object,
                                                                         out string executePayload);

            Assert.IsNotNull(executePayload);
            Assert.AreEqual("JSON", sut.FormatName);
            Assert.AreEqual("application/json", sut.ContentType);
            Assert.AreEqual(EmitionTypes.JSON, sut.PublicFormatName);
        }

        [TestMethod]
        [TestCategory("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestCoverageExecutor))]
        public void ServiceTestCoverageExecutor_RunCoverageAndReturnHTML_ShouldReturnBlank()
        {
            var mockCoverageDataObject = new Mock<ICoverageDataObject>();
            mockCoverageDataObject.Setup(o => o.CoverageReportResourceIds).Returns(new[] { Guid.NewGuid() });
            mockCoverageDataObject.Setup(o => o.ReturnType).Returns(EmitionTypes.Cover);

            var sut = ServiceTestCoverageExecutor.GetTestCoverageReports(mockCoverageDataObject.Object, Guid.NewGuid(),
                                                                         new Communication.Dev2JsonSerializer(),
                                                                         new Mock<ITestCoverageCatalog>().Object,
                                                                         new Mock<ITestCatalog>().Object,
                                                                         new Mock<IResourceCatalog>().Object,
                                                                         out string executePayload);

            Assert.IsNotNull(executePayload);
            Assert.AreEqual("HTML", sut.FormatName);
            Assert.AreEqual("text/html; charset=utf-8", sut.ContentType);
            Assert.AreEqual(EmitionTypes.Cover, sut.PublicFormatName);
        }

        [TestMethod]
        [TestCategory("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestCoverageExecutor))]
        public void ServiceTestCoverageExecutor_EmitionTypesCoverJson_With_CoverageReportResourceIds_ShouldReturnBlank()
        {
            var mockCoverageDataObject = new Mock<ICoverageDataObject>();
            mockCoverageDataObject.Setup(o => o.CoverageReportResourceIds).Returns(new[] { Guid.NewGuid() });

            var sut = ServiceTestCoverageExecutor.GetTestCoverageReports(mockCoverageDataObject.Object, Guid.NewGuid(),
                                                                         new Communication.Dev2JsonSerializer(),
                                                                         new Mock<ITestCoverageCatalog>().Object,
                                                                         new Mock<ITestCatalog>().Object,
                                                                         new Mock<IResourceCatalog>().Object,
                                                                         out string executePayload);

            Assert.IsNull(executePayload);
            Assert.AreEqual("HTML", sut.FormatName);
            Assert.AreEqual("text/html; charset=utf-8", sut.ContentType);
            Assert.AreEqual(EmitionTypes.Cover, sut.PublicFormatName);
        }


        [TestMethod]
        [TestCategory("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestCoverageExecutor))]
        public void ServiceTestCoverageExecutor_EmitionTypesCoverJson_Without_CoverageReportResourceIds_ShouldReturnBlank()
        {
            var sut = ServiceTestCoverageExecutor.GetTestCoverageReports(new Mock<ICoverageDataObject>().Object, Guid.NewGuid(),
                                                                         new Communication.Dev2JsonSerializer(),
                                                                         new Mock<ITestCoverageCatalog>().Object,
                                                                         new Mock<ITestCatalog>().Object,
                                                                         new Mock<IResourceCatalog>().Object,
                                                                         out string executePayload);

            Assert.IsNull(executePayload);
            Assert.AreEqual("HTML", sut.FormatName);
            Assert.AreEqual("text/html; charset=utf-8", sut.ContentType);
            Assert.AreEqual(EmitionTypes.Cover, sut.PublicFormatName);
        }
    }
}
