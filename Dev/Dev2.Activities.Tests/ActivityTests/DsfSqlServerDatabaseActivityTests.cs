using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Core;
using Warewolf.Storage;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class DsfSqlServerDatabaseActivityTests
    {
        private static DsfSqlServerDatabaseActivity CreateDsfSqlServerDatabaseActivity()
        {
            return new DsfSqlServerDatabaseActivity();
        }

        private static IExecutionEnvironment CreateExecutionEnvironment()
        {
            return new ExecutionEnvironment();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void DsfSqlServerDatabaseActivity_GivenNewInstance_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var sqlServerDatabaseActivity = CreateDsfSqlServerDatabaseActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(sqlServerDatabaseActivity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void CreateNewActivity_GivenIsNew_ShouldHaveDisplayName()
        {
            //---------------Set up test pack-------------------
            var sqlServerDatabaseActivity = CreateDsfSqlServerDatabaseActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sqlServerDatabaseActivity);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual("SQL Server Database", sqlServerDatabaseActivity.DisplayName);

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void GetFindMissingType_GivenIsNew_ShouldSetDatagridAcitivity()
        {
            //---------------Set up test pack-------------------
            var sqlServerDatabaseActivity = CreateDsfSqlServerDatabaseActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var enFindMissingType = sqlServerDatabaseActivity.GetFindMissingType();
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.DataGridActivity, enFindMissingType);

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void GetDebugInputs_GivenEnvironmentIsNull_ShouldHaveNoDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var sqlServerDatabaseActivity = CreateDsfSqlServerDatabaseActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugInputs = sqlServerDatabaseActivity.GetDebugInputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugInputs.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void GetDebugInputs_GivenEnvironmentMockEnvironmentAndFromPath_ShouldHaveOneDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var sqlServerDatabaseActivity = CreateDsfSqlServerDatabaseActivity();
            var serviceInputs = new List<IServiceInput>();

            var serviceInput1 = new ServiceInput
            {
                Name = "name1",
                Value = "value1"
            };

            serviceInputs.Add(serviceInput1);

            var serviceInput2 = new ServiceInput
            {
                Name = "name2",
                Value = "value2"
            };
            serviceInputs.Add(serviceInput2);

            sqlServerDatabaseActivity.Inputs = serviceInputs;
            //---------------Assert Precondition----------------


            //---------------Execute Test ----------------------
            var debugInputs = sqlServerDatabaseActivity.GetDebugInputs(CreateExecutionEnvironment(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(2, debugInputs.Count);
            Assert.AreEqual("name1", debugInputs[0].ResultsList[0].Label);
            Assert.AreEqual("value1", debugInputs[0].ResultsList[0].Value);
            Assert.AreEqual("name2", debugInputs[1].ResultsList[0].Label);
            Assert.AreEqual("value2", debugInputs[1].ResultsList[0].Value);
        }
    }
}
