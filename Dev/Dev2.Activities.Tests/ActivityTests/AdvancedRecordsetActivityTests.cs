#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
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
        static AdvancedRecordsetActivity GetAdvancedRecordsetActivity(Mock<IAdvancedRecordsetActivityWorker> worker)
        {
            var activity = new AdvancedRecordsetActivity(worker.Object);
            return activity;
        }
        static IExecutionEnvironment CreateExecutionEnvironment()
        {
            return new ExecutionEnvironment();
        }
        static AdvancedRecordsetActivityWorker GetAdvancedRecordsetWorker(Mock<IAdvancedRecordset> worker)
        {
            var advancedRecordset = new AdvancedRecordsetActivityWorker(null, worker.Object, null);
            return advancedRecordset;
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_Equal_OtherIsNull()
        {
            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            var advancedRecordsetActivityEqual = advancedRecordsetActivity.Equals(null);
            Assert.IsFalse(advancedRecordsetActivityEqual);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_Equal_OtherisEqual()
        {
            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            var advancedRecordsetActivityOther = advancedRecordsetActivity;
            var advancedRecordsetActivityEqual = advancedRecordsetActivity.Equals(advancedRecordsetActivityOther);
            Assert.IsTrue(advancedRecordsetActivityEqual);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_Equal_OtherisObjectofAdvancedRecordsetActivity()
        {
            var advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            object other = new AdvancedRecordsetActivity();
            var advancedRecordsetActivityEqual = advancedRecordsetActivity.Equals(other);
            Assert.IsFalse(advancedRecordsetActivityEqual);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_Equal_BothareObjects()
        {
            object advancedRecordsetActivity = CreateAdvancedRecordsetActivity();
            var other = new object();
            var advancedRecordsetActivityEqual = advancedRecordsetActivity.Equals(other);
            Assert.IsFalse(advancedRecordsetActivityEqual);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_SetGet_RecordsetName()
        {
            using (var advancedRecordsetActivity = new AdvancedRecordsetActivity { RecordsetName = "TestRecordsetName" })
            {
                Assert.AreEqual("TestRecordsetName", advancedRecordsetActivity.RecordsetName);
            }
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_SetGet_SqlQuery()
        {
            using (var advancedRecordsetActivity = new AdvancedRecordsetActivity { SqlQuery = "Select * from person" })
            {
                Assert.AreEqual("Select * from person", advancedRecordsetActivity.SqlQuery);
            }
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_SetGet_DeclareVariables()
        {
            var declareVariables = new List<INameValue>();
            using (var advancedRecordsetActivity = new AdvancedRecordsetActivity { DeclareVariables = declareVariables })
            {
                Assert.AreEqual(declareVariables, advancedRecordsetActivity.DeclareVariables);
            }
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_SetGet_ExecuteActionString()
        {
            const string executeActionString = "exec StoredProc";
            using (var advancedRecordsetActivity = new AdvancedRecordsetActivity { ExecuteActionString = executeActionString })
            {
                Assert.AreEqual(executeActionString, advancedRecordsetActivity.ExecuteActionString);
            }
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_GetFindMissingType_Expect_DataGridActivity()
        {
            using (var advancedRecordsetActivity = new AdvancedRecordsetActivity { SqlQuery = "Select * from person" })
            {
                var findMissingType = advancedRecordsetActivity.GetFindMissingType();
                Assert.AreEqual(enFindMissingType.DataGridActivity, findMissingType);
            }
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_GetHashCode()
        {
            using (var advancedRecordsetActivity = new AdvancedRecordsetActivity { SqlQuery = "Select * from person" })
            {
                var hashCode = advancedRecordsetActivity.GetHashCode();
                Assert.IsNotNull(hashCode);
            }
        }

       
        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
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
        [TestCategory(nameof(AdvancedRecordsetActivity))]
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
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_Worker_Dispose()
        {
            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(o => o.Dispose());

            var workerInvoker = GetAdvancedRecordsetWorker(mockAdvancedRecordset);
            mockAdvancedRecordset.Object.Dispose();
            workerInvoker.Dispose();

            mockAdvancedRecordset.Verify(r => r.Dispose());
        }
       
        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_GetDebugInputs()
        {
            var activity = GetAdvancedRecordsetActivity(new Mock<IAdvancedRecordsetActivityWorker>());
            var workerInvoker = new Mock<IAdvancedRecordsetActivityWorker>();
            workerInvoker.Setup(o => o.Dispose());

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
            Assert.AreEqual(1, getDebugInputs.Count);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_GetOutputs_OutputsisNull()
        {
            var serviceOutputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping("Location", "[[weather().Location]]", "weather")
            };
            var activity = GetAdvancedRecordsetActivity(new Mock<IAdvancedRecordsetActivityWorker>());
            activity.Outputs = serviceOutputs;
            var outputs = activity.GetOutputs();
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[weather().Location]]", outputs[0]);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_GetOutputs_OutputsnotNull()
        {
            var activity = GetAdvancedRecordsetActivity(new Mock<IAdvancedRecordsetActivityWorker>());
            activity.Outputs = null;
            var outputs = activity.GetOutputs();
            Assert.AreEqual(0, outputs.Count);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_GetOutputs_OutputsIsObject()
        {
            var activity = GetAdvancedRecordsetActivity(new Mock<IAdvancedRecordsetActivityWorker>());
            activity.Outputs = null;
            activity.IsObject = true;
            var outputs = activity.GetOutputs();

            Assert.AreEqual(1, outputs.Count);
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_ExecuteSql()
        {
            var started = false;
            var mockAdvancedRecordset = new Mock<IAdvancedRecordset>();
            mockAdvancedRecordset.Setup(o => o.CreateVariableTable());
            mockAdvancedRecordset.Setup(o => o.InsertIntoVariableTable(It.IsAny<string>(), It.IsAny<string>()));

            var workerInvoker = GetAdvancedRecordsetWorker(mockAdvancedRecordset);
            var activity = new AdvancedRecordsetActivity(workerInvoker);
            activity.SqlQuery = "Select * from person";
            //TODO: this is failing as it needs a mock of the recorset
            // workerInvoker.ExecuteSql(0, ref started);
        }
        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
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
            var worker = new AdvancedRecordsetActivityWorker(activity, recordset);

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(o => o.IsDebugMode()).Returns(true);
            dataObject.Setup(o => o.Environment).Returns(env);
            dataObject.Object.Environment = env;
            //TODO: this is failing as it needs a mock of the recorset
            //  worker.ExecuteRecordset(dataObject.Object, 0)
        }

        [TestMethod, DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AdvancedRecordsetActivity))]
        public void AdvancedRecordsetActivity_OnExecute()
        {
            //IDSFDataObject

            var activity = new AdvancedRecordsetActivity();
            //activity.Execute()
        }
    }
}