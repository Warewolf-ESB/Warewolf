using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Builders;
using Dev2.Data.Interfaces;
using Dev2.Data.Parsers;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;

namespace Dev2.Tests.Runtime.ESB.Control
{
    [TestClass]
    public class EnvironmentOutputMappingManagerTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("EnvironmentOutputMappingManager")]        
        public void EnvironmentOutputMappingManager_UpdatePreviousEnvironment_NoOutputs_NoErrors_Success()
        {
            var outputDefinition = "";
            var handleErrors = true;

            var dataObject = new Mock<IDSFDataObject>();
            var mockEnv = new Mock<IExecutionEnvironment>();

            dataObject.SetupGet(o => o.Environment).Returns(mockEnv.Object);
            var copiedErrorsResult = new HashSet<string>();
            mockEnv.SetupGet(o => o.AllErrors).Returns(copiedErrorsResult);

            var mockDataListFactory = new Mock<IDataListFactory>();
            var mockLanguageParser = new Mock<IDev2LanguageParser>();
            var outputs = new List<IDev2Definition>();
            mockLanguageParser.Setup(o => o.Parse(outputDefinition)).Returns(outputs);
            mockDataListFactory.Setup(o => o.CreateOutputParser()).Returns(mockLanguageParser.Object);
            var recordsetOutputs = new List<IRecordSetDefinition>();
            var mockRecordsetOutput = new Mock<IRecordSetCollection>();
            mockRecordsetOutput.Setup(o => o.RecordSets).Returns(recordsetOutputs);
            mockDataListFactory.Setup(o => o.CreateRecordSetCollection(outputs, true)).Returns(mockRecordsetOutput.Object);
            var scalarOutputs = new List<IDev2Definition>();
            var objectOutputs = new List<IDev2Definition>();
            mockDataListFactory.Setup(o => o.CreateScalarList(outputs, true)).Returns(scalarOutputs);
            mockDataListFactory.Setup(o => o.CreateObjectList(outputs)).Returns(objectOutputs);


            var environmentOutputMappingManager = new EnvironmentOutputMappingManager(mockDataListFactory.Object);
            var errorsFound = new ErrorResultTO();
            environmentOutputMappingManager.UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(dataObject.Object, outputDefinition, 0, handleErrors, errorsFound);


            dataObject.Verify(o => o.PopEnvironment(), Times.Once);
            mockEnv.Verify(o => o.HasErrors(), Times.Exactly(2));

            mockDataListFactory.Verify(o => o.CreateOutputParser(), Times.Once);
            mockLanguageParser.Verify(o => o.Parse(outputDefinition), Times.Once);
            mockDataListFactory.Verify(o => o.CreateRecordSetCollection(outputs, true));
            mockRecordsetOutput.Verify(o => o.RecordSets, Times.Once);
            mockDataListFactory.Verify(o => o.CreateScalarList(outputs, true));
            mockDataListFactory.Verify(o => o.CreateObjectList(outputs));

            Assert.AreEqual(0, errorsFound.FetchErrors().Count);
            Assert.AreEqual(0, copiedErrorsResult.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("EnvironmentOutputMappingManager")]
        public void EnvironmentOutputMappingManager_UpdatePreviousEnvironment_Outputs_WithErrorsHandled_Success()
        {
            EnvironmentOutputMappingManager_UpdatePreviousEnvironment_Outputs(true);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("EnvironmentOutputMappingManager")]
        public void EnvironmentOutputMappingManager_UpdatePreviousEnvironment_Outputs_WithErrorsNotHandled_Success()
        {
            EnvironmentOutputMappingManager_UpdatePreviousEnvironment_Outputs(false);
        }

        private void EnvironmentOutputMappingManager_UpdatePreviousEnvironment_Outputs(bool handleErrors)
        {
            var outputDefinition = "<Outputs>" +
               "<Output Name=\"scalar1\" MapsTo=\"[[scalar1]]\" Value=\"[[scalar1]]\" DefaultValue=\"1234\" />" +
               "<Output Name=\"name\" MapsTo=\"[[name]]\" Value=\"[[person(*).name]]\" Recordset=\"person\" DefaultValue=\"bob1\" />" +
               "<Output Name=\"name\" MapsTo=\"[[name]]\" Value=\"[[recB(*).name]]\" Recordset=\"person\" DefaultValue=\"bob2\" />" +
               "<Output Name=\"name\" MapsTo=\"[[name]]\" Value=\"[[recB(*).name]]\" Recordset=\"person\" DefaultValue=\"bob3\" />" +
               "<Output Name=\"@obj.a\" MapsTo=\"[[a]]\" Value=\"[[@obj.a]]\" IsObject=\"True\" DefaultValue=\"1\" />" +
            "</Outputs>";
            var parser = new OutputLanguageParser();

            var dataObject = new DsfDataObject("", Guid.NewGuid());

            var env = new ExecutionEnvironment();
            env.AddError("some error from before");
            env.AllErrors.Add("some all error from before");
            dataObject.Environment = env;
            env = new ExecutionEnvironment();
            env.AddError("some fake error");
            env.AllErrors.Add("some all error");

            env.Assign("[[scalar1]]", "1234", 0);
            env.Assign("[[person().name]]", "bob", 0);
            env.Assign("[[person().name]]", "bob2", 0);
            env.Assign("[[person().name]]", "bob3", 0);
            env.AssignJson(new AssignValue("[[@obj.a]]", "1"), 0);
            env.CommitAssign();

            dataObject.PushEnvironment(env);


            Mock<IDev2LanguageParser> mockLanguageParser;
            Mock<IDataListFactory> mockDataListFactory;
            {
                var dataListFactory = new DataListFactoryImplementation();
                var languageParser = dataListFactory.CreateOutputParser();

                mockDataListFactory = new Mock<IDataListFactory>();
                mockLanguageParser = new Mock<IDev2LanguageParser>();

                mockLanguageParser.Setup(o => o.Parse(outputDefinition)).Returns<string>(defs => languageParser.Parse(defs));


                mockDataListFactory.Setup(o => o.CreateOutputParser()).Returns(mockLanguageParser.Object);

                mockDataListFactory.Setup(o => o.CreateRecordSetCollection(It.IsAny<IList<IDev2Definition>>(), true)).Returns<IList<IDev2Definition>, bool>((list, isOutput) => dataListFactory.CreateRecordSetCollection(list, isOutput));
                mockDataListFactory.Setup(o => o.CreateScalarList(It.IsAny<IList<IDev2Definition>>(), true)).Returns<IList<IDev2Definition>, bool>((list, isOutput) => dataListFactory.CreateScalarList(list, isOutput));
                mockDataListFactory.Setup(o => o.CreateObjectList(It.IsAny<IList<IDev2Definition>>())).Returns<IList<IDev2Definition>>(list => dataListFactory.CreateObjectList(list));
            }



            var environmentOutputMappingManager = new EnvironmentOutputMappingManager(mockDataListFactory.Object);
            var errorsFound = new ErrorResultTO();
            environmentOutputMappingManager.UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(dataObject, outputDefinition, 0, handleErrors, errorsFound);


            mockDataListFactory.Verify(o => o.CreateOutputParser(), Times.Once);
            mockLanguageParser.Verify(o => o.Parse(outputDefinition), Times.Once);
            mockDataListFactory.Verify(o => o.CreateRecordSetCollection(It.IsAny<IList<IDev2Definition>>(), true));
            mockDataListFactory.Verify(o => o.CreateScalarList(It.IsAny<IList<IDev2Definition>>(), true));
            mockDataListFactory.Verify(o => o.CreateObjectList(It.IsAny<IList<IDev2Definition>>()));

            Assert.AreEqual(2, errorsFound.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("EnvironmentOutputMappingManager")]
        public void EnvironmentOutputMappingManager_UpdatePreviousEnvironment()
        {
            //---------------Set up test pack-------------------
            var manager = new EnvironmentOutputMappingManager();
            //---------------Assert Precondition----------------
            var dsfObject = new Mock<IDSFDataObject>();
            var env = new Mock<IExecutionEnvironment>();
            dsfObject.Setup(o => o.PopEnvironment());
            dsfObject.Setup(o => o.Environment).Returns(env.Object);

            //---------------Execute Test ----------------------
            var errors = new ErrorResultTO();
            var executionEnvironment = manager.UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(dsfObject.Object, "", 0, true, errors);
            //---------------Test Result -----------------------
            dsfObject.Verify(o => o.PopEnvironment(), Times.Once);
            Assert.IsNotNull(executionEnvironment);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("EnvironmentOutputMappingManager")]
        public void UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings_GivenEnvHasErrors_ShouldAddErrorsToResultTo()
        {
            //---------------Set up test pack-------------------
            var manager = new EnvironmentOutputMappingManager();
            //---------------Assert Precondition----------------
            var dsfObject = new Mock<IDSFDataObject>();
            var env = new Mock<IExecutionEnvironment>();
            env.Setup(environment => environment.HasErrors()).Returns(true);
            env.SetupGet(environment => environment.AllErrors).Returns(new HashSet<string>() { "Error", "Error1" });
            env.SetupGet(environment => environment.Errors).Returns(new HashSet<string>() { "Error", "Error1" });
            dsfObject.Setup(o => o.PopEnvironment());
            dsfObject.Setup(o => o.Environment).Returns(env.Object);
            dsfObject.Setup(o => o.Environment).Returns(env.Object);
            //---------------Execute Test ----------------------
            var errors = new ErrorResultTO();
            var executionEnvironment = manager.UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(dsfObject.Object, "", 0, false, errors);
            //---------------Test Result -----------------------
            dsfObject.Verify(o => o.PopEnvironment(), Times.Once);
            env.VerifyGet(o => o.AllErrors);
            env.VerifyGet(o => o.Errors);
            Assert.IsNotNull(executionEnvironment);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("EnvironmentOutputMappingManager")]
        public void EvalAssignRecordSets_GivenValidArgs_ShouldEvaluateCorrectlyAndAssignCorrectly()
        {
            //---------------Set up test pack-------------------
            var innerEnvironment = new Mock<IExecutionEnvironment>();
            innerEnvironment.Setup(executionEnvironment => executionEnvironment.Eval(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.NewDataString("Value"))))
                .Verifiable();
            var environment = new Mock<IExecutionEnvironment>();
            environment.Setup(executionEnvironment => executionEnvironment.EvalAssignFromNestedStar(It.IsAny<string>(), It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), It.IsAny<int>())).Verifiable();
            environment.Setup(executionEnvironment => executionEnvironment.EvalAssignFromNestedLast(It.IsAny<string>(), It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), It.IsAny<int>())).Verifiable();
            environment.Setup(executionEnvironment => executionEnvironment.EvalAssignFromNestedNumeric(It.IsAny<string>(), It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), It.IsAny<int>())).Verifiable();
            var builder = new RecordSetCollectionBuilder();
            IList<IDev2Definition> outputs = new IDev2Definition[]
            {
                new Dev2Definition("rec(*).LastName","rec(*).LastName","[[rec(*).LastName]]", "rec", false, "[[rec(*).LastName]]", true, "[[rec(*).LastName]]"),
                new Dev2Definition("rec1().LastName","rec1().LastName","[[rec1().LastName]]", "rec1", false, "[[rec1().LastName]]", true, "[[rec1().LastName]]"),
                new Dev2Definition("rec2(1).LastName","rec2(1).LastName","[[rec2(1).LastName]]", "rec2", false, "[[rec2(1).LastName]]", true, "[[rec2(1).LastName]]"),
            };
            builder.SetParsedOutput(new List<IDev2Definition>(outputs));
            var outputRecSets = builder.Generate();            
            //---------------Execute Test ----------------------
            var methodInfo = typeof(EnvironmentOutputMappingManager).GetMethod("TryEvalAssignRecordSets", BindingFlags.NonPublic | BindingFlags.Static);
            //---------------Test Result -----------------------
            methodInfo.Invoke(null, new object[] { innerEnvironment.Object, environment.Object, 1, outputRecSets, outputs });
            environment.Verify(executionEnvironment => executionEnvironment.EvalAssignFromNestedStar(It.IsAny<string>(), It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), It.IsAny<int>()));
            environment.Verify(executionEnvironment => executionEnvironment.EvalAssignFromNestedLast(It.IsAny<string>(), It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), It.IsAny<int>()));
            environment.Verify(executionEnvironment => executionEnvironment.EvalAssignFromNestedNumeric(It.IsAny<string>(), It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), It.IsAny<int>()));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("EnvironmentOutputMappingManager")]
        public void EvalAssignScalars_GivenRecordsetWithoutItems_ShouldEvaluate()
        {
            //---------------Set up test pack-------------------
            var innerEnvironment = new Mock<IExecutionEnvironment>();
            innerEnvironment.Setup(executionEnvironment => executionEnvironment.Eval(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.NewDataString("Value"))))
                .Verifiable();
            var environment = new Mock<IExecutionEnvironment>();
            //---------------Execute Test ----------------------
            var methodInfo = typeof(EnvironmentOutputMappingManager).GetMethod("EvalAssignScalars", BindingFlags.NonPublic | BindingFlags.Static);
            //---------------Test Result -----------------------
            methodInfo.Invoke(null, new object[] { innerEnvironment.Object, environment.Object, 1, new Dev2Definition("rec(*).LastName", "rec(*).LastName", "[[rec(*).LastName]]", "rec", false, "[[rec(*).LastName]]", true, "[[rec(*).LastName]]") });
            innerEnvironment.Verify(executionEnvironment => executionEnvironment.Eval(It.IsAny<string>(), It.IsAny<int>()));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("EnvironmentOutputMappingManager")]
        public void EvalAssignScalars_GivenValidArgs_ShouldEvaluateCorrectlyAndAssignCorrectly()
        {
            //---------------Set up test pack-------------------
            var innerEnvironment = new Mock<IExecutionEnvironment>();
            innerEnvironment.Setup(executionEnvironment => executionEnvironment.Eval(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("")))
                .Verifiable();
            var environment = new Mock<IExecutionEnvironment>();
            environment.Setup(executionEnvironment => executionEnvironment.Assign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Verifiable();

            IList<IDev2Definition> scalars = new IDev2Definition[]
            {
                new Dev2Definition("LastName","LastName","[[LastName]]", "", false, "[[LastName]]", true, "[[LastName]]"),
                new Dev2Definition("LastName","LastName","[[LastName]]", "", false, "[[LastName]]", true, "[[LastName]]"),
                new Dev2Definition("LastName","LastName","[[LastName]]", "", false, "[[LastName]]", true, "[[LastName]]"),
            };
            
            //---------------Execute Test ----------------------
            var methodInfo = typeof(EnvironmentOutputMappingManager).GetMethod("TryEvalAssignScalars", BindingFlags.NonPublic | BindingFlags.Static);

            //---------------Test Result -----------------------
            methodInfo.Invoke(null, new object[] { innerEnvironment.Object, environment.Object, 1, scalars });
            environment.Verify(executionEnvironment => executionEnvironment.Assign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(3));
            innerEnvironment.Verify(executionEnvironment => executionEnvironment.Eval(It.IsAny<string>(), It.IsAny<int>()));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("EnvironmentOutputMappingManager")]
        public void EvalAssignComplexObjects_GivenValidArgs_ShouldEvaluateCorrectlyAndAssignCorrectly()
        {
            //---------------Set up test pack-------------------
            var innerEnvironment = new Mock<IExecutionEnvironment>();
            var valueFunction = JToken.Parse("{}") as JContainer;
            innerEnvironment.Setup(executionEnvironment => executionEnvironment.EvalJContainer(It.IsAny<string>()))
                .Returns(valueFunction)
                .Verifiable();
            var environment = new Mock<IExecutionEnvironment>();
            environment.Setup(executionEnvironment => executionEnvironment.AddToJsonObjects(It.IsAny<string>(), It.IsAny<JContainer>())).Verifiable();

            IList<IDev2Definition> scalars = new IDev2Definition[]
            {
                new Dev2Definition("@obj.LastName","@obj.LastName","[[@obj.LastName]]", "", false, "[[@obj.LastName]]", true, "[[@obj.LastName]]") {IsObject = true},
                new Dev2Definition("@obj.LastName","@obj.LastName","[[@obj.LastName]]", "", false, "[[@obj.LastName]]", true, "[[@obj.LastName]]"){IsObject = true},
                new Dev2Definition("@obj.LastName","@obj.LastName","[[@obj.LastName]]", "", false, "[[@obj.LastName]]", true, "[[@obj.LastName]]"){IsObject = true},
            };
            
            //---------------Execute Test ----------------------
            var methodInfo = typeof(EnvironmentOutputMappingManager).GetMethod("TryEvalAssignComplexObjects", BindingFlags.NonPublic | BindingFlags.Static);
            
            //---------------Test Result -----------------------
            methodInfo.Invoke(null, new object[] { innerEnvironment.Object, environment.Object, scalars });
            environment.Verify(executionEnvironment => executionEnvironment.AddToJsonObjects(It.IsAny<string>(), It.IsAny<JContainer>()), Times.Exactly(3));
            innerEnvironment.Verify(executionEnvironment => executionEnvironment.EvalJContainer(It.IsAny<string>()), Times.Exactly(3));
        }
    }
}
