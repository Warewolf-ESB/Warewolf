using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;

namespace Dev2.Data.Tests.UtilTests
{
    [TestClass]
    public class CommonDataUtilTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CreateScalarInputs_Should()
        {
            var commonDataUtils = new CommonDataUtils();
            Assert.IsNotNull(commonDataUtils);

            var outerExeEnv = new ExecutionEnvironment();
            var recUsername = "[[rec(1).UserName]]";
            outerExeEnv.Assign(recUsername, "Sanele", 0);
            var inputScalarList = new List<IDev2Definition>
            {                
                new Dev2Definition("Name", "UserName", "Sanele", false, "NoName", false, recUsername)
            };
            Assert.IsNotNull(inputScalarList);
            var innerExecEnv = new ExecutionEnvironment();
            Assert.IsNotNull(innerExecEnv);
            var prObj = new PrivateObject(innerExecEnv);
            var warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(warewolfEnvironment != null && warewolfEnvironment.Scalar.Count == 0);
            commonDataUtils.CreateScalarInputs(outerExeEnv, inputScalarList, innerExecEnv,0);
            warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(warewolfEnvironment != null && warewolfEnvironment.Scalar.Count == 1);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenRecSet_CreateObjectInputs_ShouldCreateObjectInput_OnTheInnerEnvToo()
        {
            var commonDataUtils = new CommonDataUtils();
            Assert.IsNotNull(commonDataUtils);

            var outerExeEnv = new ExecutionEnvironment();
            var recUsername = "[[rec(1).UserName]]";
            outerExeEnv.Assign(recUsername, "Sanele", 0);
            var definitions = new List<IDev2Definition>
            {                
                new Dev2Definition("Name", "UserName", "Sanele", false, "NoName", false, recUsername)
            };
            Assert.IsNotNull(definitions);
            var innerExecEnv = new ExecutionEnvironment();
            var prObj = new PrivateObject(innerExecEnv);
            var warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(warewolfEnvironment != null && warewolfEnvironment.Scalar.Count == 0);
            commonDataUtils.CreateObjectInputs(outerExeEnv, definitions, innerExecEnv, 0);
            warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(warewolfEnvironment != null && warewolfEnvironment.Scalar.Count == 1);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenVariable_CreateObjectInputs_ShouldCreateObjectInput_OnTheInnerEnvToo()
        {
            var commonDataUtils = new CommonDataUtils();
            Assert.IsNotNull(commonDataUtils);

            var outerExeEnv = new ExecutionEnvironment();
            var recUsername = "[[UserName]]";
            outerExeEnv.Assign(recUsername, "Sanele", 0);
            var definitions = new List<IDev2Definition>
            {                
                new Dev2Definition("Name", "UserName", "Sanele", false, "NoName", false, recUsername)
            };
            Assert.IsNotNull(definitions);
            var innerExecEnv = new ExecutionEnvironment();
            var prObj = new PrivateObject(innerExecEnv);
            var warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(warewolfEnvironment != null && warewolfEnvironment.Scalar.Count == 0);
            commonDataUtils.CreateObjectInputs(outerExeEnv, definitions, innerExecEnv, 0);
            warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(warewolfEnvironment != null && warewolfEnvironment.Scalar.Count == 1);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenJSon_CreateObjectInputs_ShouldCreateObjectInput_OnTheInnerEnvToo()
        {
            var commonDataUtils = new CommonDataUtils();
            Assert.IsNotNull(commonDataUtils);

            var outerExeEnv = new ExecutionEnvironment();
            var recUsername = "[[@Person().UserName]]";
            outerExeEnv.Assign(recUsername, "Sanele", 0);
            var definitions = new List<IDev2Definition>
            {                
                new Dev2Definition("Name", "UserName", "Sanele", false, "NoName", false, recUsername)
            };
            Assert.IsNotNull(definitions);
            var innerExecEnv = new ExecutionEnvironment();
            var prObj = new PrivateObject(innerExecEnv);
            var warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(warewolfEnvironment != null && warewolfEnvironment.JsonObjects.Count == 0);
            commonDataUtils.CreateObjectInputs(outerExeEnv, definitions, innerExecEnv, 0);
            warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(warewolfEnvironment != null && warewolfEnvironment.JsonObjects.Count == 1);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [ExpectedException(typeof(Exception))]
        public void GivenNullSource_AddMissingFileDirectoryParts_ShouldRetunError()
        {
            var commonDataUtils = new CommonDataUtils();
            var tempFile = Path.GetTempFileName();
            const string newFileName = "ZippedTempFile";
            var zipPathName = Path.GetTempPath() + newFileName + ".zip";
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(tempFile, string.Empty, null, true, ""));
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(zipPathName, string.Empty, null, true, ""));
            Assert.IsNotNull(commonDataUtils);
            scrEndPoint.IOPath.Path = string.Empty;
            commonDataUtils.AddMissingFileDirectoryParts(scrEndPoint, dstEndPoint);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]        
        public void GivenNullDestination_AddMissingFileDirectoryParts_ShouldRetunError()
        {
            var commonDataUtils = new CommonDataUtils();
            var tempFile = Path.GetTempFileName();
            const string newFileName = "ZippedTempFile";
            var zipPathName = Path.GetTempPath() + newFileName + ".zip";
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(tempFile, string.Empty, null, true, ""));
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(zipPathName, string.Empty, null, true, ""));
            Assert.IsNotNull(commonDataUtils);
            dstEndPoint.IOPath.Path = string.Empty;
            commonDataUtils.AddMissingFileDirectoryParts(scrEndPoint, dstEndPoint);
        }
    }
}
