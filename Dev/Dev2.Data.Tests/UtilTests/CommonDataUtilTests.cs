using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Search;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

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
            commonDataUtils.CreateScalarInputs(outerExeEnv, inputScalarList, innerExecEnv, 0);
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
            var scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(tempFile, string.Empty, null, true, ""));
            var dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(zipPathName, string.Empty, null, true, ""));
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
            var scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(tempFile, string.Empty, null, true, ""));
            var dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(zipPathName, string.Empty, null, true, ""));
            Assert.IsNotNull(commonDataUtils);
            dstEndPoint.IOPath.Path = string.Empty;
            commonDataUtils.AddMissingFileDirectoryParts(scrEndPoint, dstEndPoint);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ValidateSourceAndDestinationPaths_GivenIsPathRooted_ShouldReturnFalse()
        {
            const string txt = "C:\\Home\\a.txt";
            var srcPath = new Mock<IActivityIOPath>();
            srcPath.Setup(path => path.Path).Returns(txt);
            var dstPath = new Mock<IActivityIOPath>();
            dstPath.Setup(path => path.Path).Returns("some relative path");
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            src.Setup(p => p.IOPath).Returns(srcPath.Object);
            src.Setup(p => p.PathSeperator()).Returns("\\");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            dst.Setup(p => p.IOPath).Returns(dstPath.Object);
            dst.Setup(p => p.PathSeperator()).Returns("\\");

            var commonDataUtils = new CommonDataUtils();
            commonDataUtils.ValidateSourceAndDestinationPaths(src.Object, dst.Object);

        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void AddMissingFileDirectoryParts_GivenDestinationPathIsDirectory_SourcePathIsNotDirectory()
        {
            const string file = "C:\\Home\\a.txt";
            var srcPath = new Mock<IActivityIOPath>();
            srcPath.Setup(path => path.Path).Returns(file);
            var dstPath = new Mock<IActivityIOPath>();
            dstPath.Setup(path => path.Path).Returns(file);
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            src.Setup(p => p.IOPath).Returns(srcPath.Object);
            src.Setup(p => p.PathSeperator()).Returns("\\");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            dst.Setup(p => p.IOPath).Returns(dstPath.Object);
            dst.Setup(p => p.PathSeperator()).Returns("\\");
            dst.Setup(p => p.PathIs(dstPath.Object)).Returns(enPathType.Directory);

            var commonDataUtils = new CommonDataUtils();
            commonDataUtils.AddMissingFileDirectoryParts(src.Object, dst.Object);
            dstPath.VerifySet(p => p.Path = @"C:\Home\");
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void AddMissingFileDirectoryParts_GivenDestinationPathIsDirectory_SourcePathIsDirectory()
        {
            const string file = "C:\\Parent\\a.txt";
            const string dstfile = "C:\\Parent\\Child\\";
            var srcPath = new Mock<IActivityIOPath>();
            srcPath.Setup(path => path.Path).Returns(file);
            var dstPath = new Mock<IActivityIOPath>();
            dstPath.Setup(path => path.Path).Returns(dstfile);
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            src.Setup(p => p.IOPath).Returns(srcPath.Object);
            src.Setup(p => p.PathSeperator()).Returns("\\");
            src.Setup(p => p.PathIs(srcPath.Object)).Returns(enPathType.Directory);

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            dst.Setup(p => p.IOPath).Returns(dstPath.Object);
            dst.Setup(p => p.PathSeperator()).Returns("\\");
            dst.Setup(p => p.PathIs(dstPath.Object)).Returns(enPathType.Directory);

            var commonDataUtils = new CommonDataUtils();
            commonDataUtils.AddMissingFileDirectoryParts(src.Object, dst.Object);
            dstPath.VerifySet(p => p.Path = @"C:\Parent\Child\a.txt");
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CreateRecordSetsInputs_GivenNullValue()
        {
            var outerEnvironment = new Mock<IExecutionEnvironment>();
            var inputRecSets = new Mock<IRecordSetCollection>();
            var mockedInput = new Mock<IRecordSetDefinition>();
            var nameColumn = new Mock<IDev2Definition>();
            var ageColumn = new Mock<IDev2Definition>();            
            var input1 = new Mock<IDev2Definition>();
            var input2 = new Mock<IDev2Definition>();

            mockedInput.Setup(p => p.SetName).Returns("Person");

            nameColumn.Setup(p => p.Name).Returns("Name");
            nameColumn.Setup(p => p.IsRecordSet).Returns(true);
            nameColumn.Setup(p => p.RecordSetName).Returns("Person");            
            ageColumn.Setup(p => p.Name).Returns("Age");
            ageColumn.Setup(p => p.IsRecordSet).Returns(true);
            ageColumn.Setup(p => p.RecordSetName).Returns("Person");
            
            mockedInput.Setup(p => p.Columns).Returns(new List<IDev2Definition> { nameColumn.Object, ageColumn.Object });
            inputRecSets.Setup(p => p.RecordSets).Returns(new List<IRecordSetDefinition> { mockedInput.Object });

            
            input1.Setup(p => p.IsRecordSet).Returns(true);
            input1.Setup(p => p.MapsTo).Returns("[[Person().Name]]");
            input1.Setup(p => p.RecordSetName).Returns("Person");            
            input2.Setup(p => p.IsRecordSet).Returns(true);
            input2.Setup(p => p.MapsTo).Returns("[[Person().Age]]");
            input2.Setup(p => p.RecordSetName).Returns("Person");

            var inputs = new List<IDev2Definition>
             {
                input1.Object, input2.Object
            };

            var env = new Mock<IExecutionEnvironment>();
            int update = 0;
            var commonDataUtils = new CommonDataUtils();
            commonDataUtils.CreateRecordSetsInputs(outerEnvironment.Object, inputRecSets.Object, inputs, env.Object, update);
            env.Verify(p => p.AssignDataShape("[[Person().Name]]"), Times.AtLeastOnce);
            env.Verify(p => p.AssignDataShape("[[Person().Age]]"), Times.AtLeastOnce);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GenerateDefsFromDataListForDebug_GivenNullValue()
        {            
            const string trueString = "True";
            var datalist = string.Format("<DataList><Person Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"Both\" ><Name Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"Input\" /></Person></DataList>", trueString);
            var commonDataUtils = new CommonDataUtils();
            var results = commonDataUtils.GenerateDefsFromDataListForDebug(datalist, enDev2ColumnArgumentDirection.Both);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Name", results[0].Name);
        }
    }
}
