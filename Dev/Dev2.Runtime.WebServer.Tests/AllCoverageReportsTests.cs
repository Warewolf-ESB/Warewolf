/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces;
using Dev2.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using Warewolf.Data;

namespace Dev2.Runtime.WebServer.Tests
{
    [TestClass]
    public class AllCoverageReportsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AllCoverageReports))]
        public void AllCoverageReports_CTOR_Empty_Defaults()
        {
            var sut = new AllCoverageReports();

            Assert.IsNull(sut.StartTime);
            Assert.IsNull(sut.EndTime);
            Assert.AreEqual(0, sut.TotalReportsCoverage);
            Assert.AreEqual(0, sut.WithTestReports.ToList().Count, "should be initialized");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AllCoverageReports))]
        public void AllCoverageReports_Given_WithTestReports_IsNull_Default_ShouldNotReturnNaN()
        {
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();

            var sut = new AllCoverageReports
            {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now
            };

            sut.Add(new WorkflowCoverageReports(mockWarewolfWorkflow.Object)
            {
                //should something unfavorable ever happen, zero make more sense then NaN in this context
            });

            Assert.IsNotNull(sut.StartTime);
            Assert.IsNotNull(sut.EndTime);
            Assert.AreEqual(0, sut.TotalReportsCoverage, "code for safety; this should return zero not NaN");
            Assert.AreEqual(0, sut.WithTestReports.ToList().Count, "should be initialized");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(AllCoverageReports))]
        public void AllCoverageReports_Given_WithTestReports_IsNotNull_ShouldReturn()
        {
            var mockWarewolfWorkflow = new Mock<IWarewolfWorkflow>();
            var mockServiceTestCoverageModelTo_1 = new Mock<IServiceTestCoverageModelTo>();
            var mockServiceTestCoverageModelTo_2 = new Mock<IServiceTestCoverageModelTo>();

            mockServiceTestCoverageModelTo_1.Setup(o => o.TotalCoverage)
                .Returns(.25);
            mockServiceTestCoverageModelTo_2.Setup(o => o.TotalCoverage)
                .Returns(0.5);

            var sut = new AllCoverageReports
            {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now
            };

            var coverageReports = new WorkflowCoverageReports(mockWarewolfWorkflow.Object); 
            coverageReports.Add(mockServiceTestCoverageModelTo_1.Object);
            coverageReports.Add(mockServiceTestCoverageModelTo_2.Object);
            sut.Add(coverageReports);

            Assert.IsNotNull(sut.StartTime);
            Assert.IsNotNull(sut.EndTime);
            Assert.AreEqual(75, sut.TotalReportsCoverage, "code for safety; this should return zero not NaN");
            Assert.AreEqual(1, sut.WithTestReports.ToList().Count, "should be initialized");
        }
    }
}
