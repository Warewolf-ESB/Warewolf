using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Core;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class DsfMySqlDatabaseActivityTests
    {
        static DsfMySqlDatabaseActivity CreateMySqlDatabaseActivity()
        {
            return new DsfMySqlDatabaseActivity();
        }

        static IExecutionEnvironment CreateExecutionEnvironment()
        {
            return new ExecutionEnvironment();
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void DsfMySqlDatabaseActivity_GivenNewInstance_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var mySqlDatabaseActivity = CreateMySqlDatabaseActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(mySqlDatabaseActivity);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void CreateNewActivity_GivenIsNew_ShouldHaveDisplayName()
        {
            //---------------Set up test pack-------------------
            var mySqlDatabaseActivity = CreateMySqlDatabaseActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(mySqlDatabaseActivity);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual("MySQL Database", mySqlDatabaseActivity.DisplayName);

        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void GetFindMissingType_GivenIsNew_ShouldSetDatagridAcitivity()
        {
            //---------------Set up test pack-------------------
            var mySqlDatabaseActivity = CreateMySqlDatabaseActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var enFindMissingType = mySqlDatabaseActivity.GetFindMissingType();
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.DataGridActivity, enFindMissingType);

        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void GetDebugInputs_GivenEnvironmentIsNull_ShouldHaveNoDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var mySqlDatabaseActivity = CreateMySqlDatabaseActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugInputs = mySqlDatabaseActivity.GetDebugInputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugInputs.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void GetDebugInputs_GivenEnvironmentMockEnvironmentAndFromPath_ShouldHaveOneDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var mySqlDatabaseActivity = CreateMySqlDatabaseActivity();
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

            mySqlDatabaseActivity.Inputs = serviceInputs;
            //---------------Assert Precondition----------------
            

            //---------------Execute Test ----------------------
            var debugInputs = mySqlDatabaseActivity.GetDebugInputs(CreateExecutionEnvironment(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(2, debugInputs.Count);
            Assert.AreEqual("name1", debugInputs[0].ResultsList[0].Label);
            Assert.AreEqual("value1", debugInputs[0].ResultsList[0].Value);
            Assert.AreEqual("name2", debugInputs[1].ResultsList[0].Label);
            Assert.AreEqual("value2", debugInputs[1].ResultsList[0].Value);
        }
    }
}
