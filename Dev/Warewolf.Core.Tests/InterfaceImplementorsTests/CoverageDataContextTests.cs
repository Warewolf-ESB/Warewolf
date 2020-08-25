/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.InterfaceImplementors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Dev2.Tests.InterfaceImplementorsTests
{
    [TestClass]
    public class CoverageDataContextTests
    {
        private readonly Guid _resourceId = Guid.Parse("3c2df19f-e5ab-4ed7-b7ff-7e6e43d94649");

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CoverageDataContext))]
        public void CoverageDataContext_CTOR_DefaultSetup_With_EmptyGuid()
        {
            var sut = new CoverageDataContext(Guid.Empty, Web.EmitionTypes.Cover, "http://127.0.0.0.1:3142")
            {
                CoverageReportResourceIds = new Guid[] { _resourceId },
            };

            Assert.AreEqual(Guid.Empty, sut.ResourceID);
            Assert.IsTrue(sut.IsMultipleWorkflowReport);
            Assert.AreEqual(Web.EmitionTypes.Cover, sut.ReturnType);
            Assert.AreEqual(_resourceId, sut.CoverageReportResourceIds.First());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CoverageDataContext))]
        public void CoverageDataContext_CTOR_DefaultSetup_With_NonEmptyGuid()
        {
            var sut = new CoverageDataContext(_resourceId, Web.EmitionTypes.Cover, "http://127.0.0.0.1:3142");

            Assert.AreEqual(_resourceId, sut.ResourceID);
            Assert.IsFalse(sut.IsMultipleWorkflowReport);
            Assert.AreEqual(Web.EmitionTypes.Cover, sut.ReturnType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CoverageDataContext))]
        public void CoverageDataContext_GetAllTestsUrl()
        {
            var sut = new CoverageDataContext(Guid.NewGuid(), Web.EmitionTypes.Cover, "http://127.0.0.0.1:3142/secure/Coverage1/.coverage");

            var result = sut.GetAllTestsUrl();

            Assert.AreEqual("/secure/Coverage1/.tests", result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CoverageDataContext))]
        public void CoverageDataContext_GetTestUrl_Failure()
        {
            var sut = new CoverageDataContext(_resourceId, Web.EmitionTypes.Cover, "http://127.0.0.0.1:3142");

            Assert.ThrowsException<Exception>(()=> sut.GetTestUrl(@"/resources/path/" + _resourceId.ToString()), "unable to generate test uri: unexpected uri");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CoverageDataContext))]
        public void CoverageDataContext_GetTestUrl_Success()
        {
            var sut = new CoverageDataContext(_resourceId, Web.EmitionTypes.Cover, "http://127.0.0.0.1:3142/secure/Coverage1/.coverage");

            var result = sut.GetTestUrl(@"/resources/path/"+_resourceId.ToString());

            Assert.AreEqual("http://127.0.0.0.1:3142/secure//resources/path/3c2df19f-e5ab-4ed7-b7ff-7e6e43d94649.tests", result);
        }

    }
}
