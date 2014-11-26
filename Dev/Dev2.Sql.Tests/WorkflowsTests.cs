
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
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.SqlServer.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Sql;

namespace Dev2.Sql.Tests
{
    // PBI 8600

    /// <author>Trevor.Williams-Ros</author>
    /// <date>2013/02/21</date>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WorkflowsTests
    {
        const string TestServerUri = "http://localhost:1234/services/Test";

        static XElement _workflowResultXml;

        #region ClassInitialize

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _workflowResultXml = XElement.Parse(Resources.GetFromResources("WorkflowResult.xml"));
        }

        #endregion

        #region RunWorkflowForXmlImpl

        [TestMethod]
        public void RunWorkflowForXmlImplWithNullServerUriExpectedReturnsEmptyXml()
        {
            var workflows = new Workflows();
            var result = workflows.RunWorkflowForXmlImpl(null);
            Assert.IsNotNull(result);
            Assert.IsTrue(string.IsNullOrEmpty(result.Value));
        }

        [TestMethod]
        public void RunWorkflowForXmlImplWithNullRootNameExpectedDoesNotChangeRootName()
        {
            var expectedXml = new XElement("MyRoot");
            var workflows = new WorkflowsMock { ReturnXml = expectedXml.ToString() };
            var result = workflows.RunWorkflowForXmlImpl(TestServerUri);
            Assert.AreEqual(expectedXml.Name, result.Name);
        }

        [TestMethod]
        public void RunWorkflowForXmlImplWithEmptyRootNameExpectedDoesNotChangeRootName()
        {
            var expectedXml = new XElement("MyRoot");
            var workflows = new WorkflowsMock { ReturnXml = expectedXml.ToString() };
            var result = workflows.RunWorkflowForXmlImpl(TestServerUri, string.Empty);
            Assert.AreEqual(expectedXml.Name, result.Name);
        }


        [TestMethod]
        public void RunWorkflowForXmlImplWithRootNameExpectedDoesChangeRootName()
        {
            const string ExpectedRootName = "NewRoot";
            var expectedXml = new XElement("MyRoot");
            var workflows = new WorkflowsMock { ReturnXml = expectedXml.ToString() };
            var result = workflows.RunWorkflowForXmlImpl(TestServerUri, ExpectedRootName);
            Assert.AreEqual(ExpectedRootName, result.Name);
        }

        [TestMethod]
        public void RunWorkflowForXmlImplWithValidParametersExpectedReturnsXml()
        {
            var expectedXml = _workflowResultXml.ToString(SaveOptions.DisableFormatting);
            var workflows = new WorkflowsMock { ReturnXml = expectedXml };
            var result = workflows.RunWorkflowForXmlImpl(TestServerUri);
            var actualXml = result.ToString(SaveOptions.DisableFormatting);
            Assert.AreEqual(expectedXml, actualXml);
        }

        #endregion

        #region RunWorkflowForSqlImpl

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RunWorkflowForSqlImplWithNullSqlCtxThrowsArgumentNullException()
        {
            var workflows = new Workflows();
            workflows.RunWorkflowForSqlImpl(null, null, null);
        }

        [TestMethod]
        public void RunWorkflowForSqlImplWithNullServerUriExpectedReturnsEmptyDataTable()
        {
            var workflows = new Workflows();
            var result = workflows.RunWorkflowForSqlImpl(CreateSqlCtxMock(), null, null);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Rows.Count);
        }

        [TestMethod]
        public void RunWorkflowForSqlImplWithServerUriAndRecordsetNameExpectedReturnsRecordsetAsDataTable()
        {
            var employees = _workflowResultXml.Elements("Employees").ToList();

            var workflows = new WorkflowsMock { ReturnXml = _workflowResultXml.ToString() };
            var result = workflows.RunWorkflowForSqlImpl(CreateSqlCtxMock(), TestServerUri, "Employees");
            Assert.IsNotNull(result);
            Assert.AreEqual(employees.Count, result.Rows.Count);

            // Verify that the DataTable was generated correctly
            foreach(var employee in employees)
            {
                var filter = new StringBuilder();
                foreach(var field in employee.Elements())
                {
                    filter.AppendFormat(" AND {0}='{1}'", field.Name.LocalName, field.Value);
                }
                filter.Remove(0, 5);
                var rows = result.Select(filter.ToString());
                Assert.AreEqual(1, rows.Length);
            }
        }

        [TestMethod]
        public void RunWorkflowForSqlImplWithServerUriAndNullRecordsetNameExpectedReturnsScalarsAsSingleRowInDataTable()
        {
            var workflows = new WorkflowsMock { ReturnXml = _workflowResultXml.ToString() };
            var result = workflows.RunWorkflowForSqlImpl(CreateSqlCtxMock(), TestServerUri, null);
            Assert.IsNotNull(result);

            var filter = new StringBuilder();
            foreach(var field in _workflowResultXml.Elements().Where(e => !e.HasElements))
            {
                filter.AppendFormat(" AND {0}='{1}'", field.Name.LocalName, field.Value);
            }
            filter.Remove(0, 5);
            var rows = result.Select(filter.ToString());
            Assert.AreEqual(1, rows.Length);
        }

        [TestMethod]
        public void RunWorkflowForSqlImplWithServerUriAndNullRecordsetNameExpectedReturnsRecordsetsAsRowsInDataTable()
        {
            var workflows = new WorkflowsMock { ReturnXml = _workflowResultXml.ToString() };
            var result = workflows.RunWorkflowForSqlImpl(CreateSqlCtxMock(), TestServerUri, null);
            Assert.IsNotNull(result);

            foreach(var node in _workflowResultXml.Elements().Where(e => e.HasElements))
            {
                var filter = new StringBuilder();
                foreach(var field in node.Elements())
                {
                    filter.AppendFormat(" AND {0}{1}='{2}'", node.Name.LocalName, field.Name.LocalName, field.Value);
                }
                filter.Remove(0, 5);
                var rows = result.Select(filter.ToString());
                Assert.AreEqual(1, rows.Length);
            }
        }

        [TestMethod]
        public void RunWorkflowForSqlImplWithValidArgsExpectedInvokesSqlCtx()
        {
            var employees = _workflowResultXml.Elements("Employees").ToList();
            var sendRowCount = 0;

            var ctx = new Mock<ISqlCtx>();
            ctx.Setup(c => c.SendStart(It.IsAny<DataTable>())).Verifiable();
            ctx.Setup(c => c.SendEnd()).Verifiable();
            ctx.Setup(c => c.SendRow(It.IsAny<SqlDataRecord>(), It.IsAny<object[]>())).Callback(() => sendRowCount++);

            var workflows = new WorkflowsMock { ReturnXml = _workflowResultXml.ToString() };
            workflows.RunWorkflowForSqlImpl(ctx.Object, TestServerUri, "Employees");

            ctx.VerifyAll();
            Assert.AreEqual(employees.Count, sendRowCount);
        }

        #endregion

        #region CreateSqlCtxMock

        static ISqlCtx CreateSqlCtxMock()
        {
            var ctx = new Mock<ISqlCtx>();
            return ctx.Object;
        }

        #endregion

        #region RunWorkflow

        [TestMethod]
        public void RunWorkflowWithNullArgumentsExpectedReturnsEmptyXml()
        {
            var workflows = new Workflows();
            var result = workflows.RunWorkflow(null);
            Assert.IsNotNull(result);
            Assert.IsTrue(string.IsNullOrEmpty(result.Value));
        }

        #endregion

    }
}
