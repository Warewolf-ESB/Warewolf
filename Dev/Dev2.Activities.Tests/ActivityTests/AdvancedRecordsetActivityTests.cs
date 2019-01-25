/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data;
using Warewolf.Core;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;
using static Dev2.Activities.AdvancedRecordsetActivity;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class AdvancedRecordsetActivityTests : BaseActivityTests
    {

        static AdvancedRecordsetActivity CreateAdvancedRecordsetActivity()
        {
            return new AdvancedRecordsetActivity();
        }
        static AdvancedRecordsetActivity GetAdvancedRecordsetActivity(Mock<IWorker> worker)
        {
            var activity = new AdvancedRecordsetActivity(worker.Object);
            return activity;
        }
        static IExecutionEnvironment CreateExecutionEnvironment()
        {
            return new ExecutionEnvironment();
        }
        static Worker GetAdvancedRecordsetWorker(Mock<IAdvancedRecordset> worker)
        {
            var advancedRecordset = new Worker(worker.Object);
            return advancedRecordset;
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_Equal_OtherIsNull()
        {
            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            var advancedRecordsetActivityEqual = advancedRecordsetActivity.Equals(null);
            Assert.IsFalse(advancedRecordsetActivityEqual);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_Equal_OtherisEqual()
        {
            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            var advancedRecordsetActivityOther = advancedRecordsetActivity;
            var advancedRecordsetActivityEqual = advancedRecordsetActivity.Equals(advancedRecordsetActivityOther);
            Assert.IsTrue(advancedRecordsetActivityEqual);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_Equal_OtherisObjectofAdvancedRecordsetActivity()
        {
            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            object other = new AdvancedRecordsetActivity { };
            var advancedRecordsetActivityEqual = advancedRecordsetActivity.Equals(other);
            Assert.IsFalse(advancedRecordsetActivityEqual);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_Equal_BothareObjects()
        {
            object advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            object other = new object { };
            var advancedRecordsetActivityEqual = advancedRecordsetActivity.Equals(other);
            Assert.IsFalse(advancedRecordsetActivityEqual);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_SetGet_RecordsetName()
        {
            var advancedRecordsetActivity = new AdvancedRecordsetActivity { RecordsetName = "TestRecordsetName" };
            Assert.AreEqual("TestRecordsetName", advancedRecordsetActivity.RecordsetName);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_SetGet_SqlQuery()
        {
            var advancedRecordsetActivity = new AdvancedRecordsetActivity { SqlQuery = "Select * from person" };
            Assert.AreEqual("Select * from person", advancedRecordsetActivity.SqlQuery);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_SetGet_DeclareVariables()
        {
            var declareVariables = new List<INameValue>();
            var advancedRecordsetActivity = new AdvancedRecordsetActivity { DeclareVariables = declareVariables };
            Assert.AreEqual(declareVariables, advancedRecordsetActivity.DeclareVariables);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_SetGet_ExecuteActionString()
        {
            var executeActionString = "exec StoredProc";
            var advancedRecordsetActivity = new AdvancedRecordsetActivity { ExecuteActionString = executeActionString };
            Assert.AreEqual(executeActionString, advancedRecordsetActivity.ExecuteActionString);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_GetFindMissingType_Expect_DataGridActivity()
        {
            var advancedRecordsetActivity = new AdvancedRecordsetActivity { SqlQuery = "Select * from person" };
            var findMissingType = advancedRecordsetActivity.GetFindMissingType();
            Assert.AreEqual(enFindMissingType.DataGridActivity, findMissingType);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_GetHashCode()
        {
            var advancedRecordsetActivity = new AdvancedRecordsetActivity { SqlQuery = "Select * from person" };
            var hashCode = advancedRecordsetActivity.GetHashCode();
            Assert.IsNotNull(hashCode);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_WorkerInvoker_Expect_WorkertobeReturned()
        {
            var activity = GetAdvancedRecordsetActivity(new Mock<IWorker>());
            var workerInvoker = new Mock<IWorker>().Object;
            activity.WorkerInvoker = workerInvoker;
            var actual = activity.WorkerInvoker;
            Assert.AreEqual(workerInvoker, actual);
            Assert.IsNotInstanceOfType(actual, typeof(WebRequestInvoker));
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_Worker_LoadRecordset()
        {
            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(o => o.LoadRecordsetAsTable(It.IsAny<string>()));
            var workerInvoker = GetAdvancedRecordsetWorker(mockAdvancedRecordset);
            workerInvoker.LoadRecordset("person");
            mockAdvancedRecordset.Verify(o => o.LoadRecordsetAsTable(It.IsAny<string>()));
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_Worker_AddDeclarations()
        {
            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(o => o.CreateVariableTable());
            mockAdvancedRecordset.Setup(o => o.InsertIntoVariableTable(It.IsAny<string>(), It.IsAny<string>()));

            var workerInvoker = GetAdvancedRecordsetWorker(mockAdvancedRecordset);
            workerInvoker.AddDeclarations("person", "testUser");
            mockAdvancedRecordset.Verify(o => o.CreateVariableTable());
            mockAdvancedRecordset.Verify(o => o.InsertIntoVariableTable(It.IsAny<string>(), It.IsAny<string>()));
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_Worker_Dispose()
        {
            var activity = GetAdvancedRecordsetActivity(new Mock<IWorker>());
            var workerInvoker = new Mock<IWorker>();
            workerInvoker.Setup(o => o.Dispose());
            activity.WorkerInvoker = workerInvoker.Object;
            activity.WorkerInvoker.Dispose();
            workerInvoker.Verify(o => o.Dispose());
            activity.Dispose();

            Assert.IsNull(activity.WorkerInvoker.AdvancedRecordset);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_GetDebugInputs()
        {
            var activity = GetAdvancedRecordsetActivity(new Mock<IWorker>());
            var workerInvoker = new Mock<IWorker>();
            workerInvoker.Setup(o => o.Dispose());
            activity.WorkerInvoker = workerInvoker.Object;

            var executionEnvironment = new ExecutionEnvironment();
            executionEnvironment.Assign("[[Switch]]", "1", 1);

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(o => o.IsDebugMode()).Returns(true);
            dataObject.Setup(o => o.Environment).Returns(executionEnvironment);

            activity.Execute(dataObject.Object, 0);
            var outputResults = activity.GetDebugOutputs(CreateExecutionEnvironment(), 1);
            Assert.AreEqual(0, outputResults.Count);

            outputResults = activity.GetDebugOutputs(new Mock<IExecutionEnvironment>().Object, 1);
            Assert.AreEqual(0, outputResults.Count);

            var getDebugInputs = activity.GetDebugInputs(new Mock<IExecutionEnvironment>().Object, 1);
            Assert.AreEqual(0, getDebugInputs.Count);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_GetOutputs_OutputsisNull()
        {
            var serviceOutputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping("Location", "[[weather().Location]]", "weather")
            };
            var activity = GetAdvancedRecordsetActivity(new Mock<IWorker>());
            activity.Outputs = serviceOutputs;
            var outputs = activity.GetOutputs();
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[weather().Location]]", outputs[0]);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_GetOutputs_OutputsnotNull()
        {
            var activity = GetAdvancedRecordsetActivity(new Mock<IWorker>());
            activity.Outputs = null;
            var outputs = activity.GetOutputs();
            Assert.AreEqual(0, outputs.Count);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_GetOutputs_OutputsIsObject()
        {
            var activity = GetAdvancedRecordsetActivity(new Mock<IWorker>());
            activity.Outputs = null;
            activity.IsObject = true;
            var outputs = activity.GetOutputs();
           
            Assert.AreEqual(1, outputs.Count);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_ExecuteSql()
        {
            var started = false;
            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(o => o.CreateVariableTable());
            mockAdvancedRecordset.Setup(o => o.InsertIntoVariableTable(It.IsAny<string>(), It.IsAny<string>()));

            var workerInvoker = GetAdvancedRecordsetWorker(mockAdvancedRecordset);
            var activity = new AdvancedRecordsetActivity(workerInvoker);
            activity.SqlQuery = "Select * from person";
            activity.WorkerInvoker = workerInvoker;
           // workerInvoker.ExecuteSql(0, ref started);
        }
        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordsetActivity")]
        public void AdvancedRecordsetActivity_ExecuteRecordset()
        {
            var started = false;
            var personRecordsetName = "person";
            var l = new List<AssignValue>();
            l.Add(new AssignValue("[[person().name]]", "bob"));
            l.Add(new AssignValue("[[person().age]]", "21"));
            l.Add(new AssignValue("[[person().address_id]]", "1"));

            var env = CreateExecutionEnvironment();
            env.AssignWithFrame(l, 0);
            env.CommitAssign();

            var recordset = new AdvancedRecordset(env);
            recordset.LoadRecordsetAsTable(personRecordsetName);
            recordset.ApplyResultToEnvironment(personRecordsetName, new List<IServiceOutputMapping>(), new List<DataRow>(), false, 0, ref started);
            recordset.CreateVariableTable();
            recordset.InsertIntoVariableTable("TestVariable", "testdata");

            var activity = new AdvancedRecordsetActivity()
            {
                SqlQuery = "Select * from person",
            };
            var worker = new Worker(activity, recordset);

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(o => o.IsDebugMode()).Returns(true);
            dataObject.Setup(o => o.Environment).Returns(env);
            dataObject.Object.Environment = env;
            //TODO: this is failing as it needs a mock of the recorset
          //  worker.ExecuteRecordset(dataObject.Object, 0)
        }
    }
}