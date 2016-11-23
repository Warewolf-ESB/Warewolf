using System;
using System.Collections.Generic;
using System.Reflection;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Builders;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Warewolf.Storage;
using WarewolfParserInterop;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.ESB.Control
{
    [TestClass]
    public class EnvironmentOutputMappingManagerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings_GivenValidArgs_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            var manager = new EnvironmentOutputMappingManager();
            //---------------Assert Precondition----------------
            //(IDSFDataObject dataObject, string outputDefs, int update, bool handleErrors, ErrorResultTO errors)
            var dsfObject = new Mock<IDSFDataObject>();
            var env = new Mock<IExecutionEnvironment>();
            dsfObject.Setup(o => o.PopEnvironment());
            dsfObject.Setup(o => o.Environment).Returns(env.Object);

            //---------------Execute Test ----------------------
            ErrorResultTO errors = new ErrorResultTO();
            var executionEnvironment = manager.UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(dsfObject.Object, "", 0, true, errors);
            //---------------Test Result -----------------------
            dsfObject.Verify(o => o.PopEnvironment(), Times.Once);
            Assert.IsNotNull(executionEnvironment);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
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
            ErrorResultTO errors = new ErrorResultTO();
            var executionEnvironment = manager.UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(dsfObject.Object, "", 0, false, errors);
            //---------------Test Result -----------------------
            dsfObject.Verify(o => o.PopEnvironment(), Times.Once);
            env.VerifyGet(o => o.AllErrors);
            env.VerifyGet(o => o.Errors);
            Assert.IsNotNull(executionEnvironment);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void EvalAssignRecordSets_GivenValidArgs_ShouldEvaluateCorrectlyAndAssignCorreclty()
        {
            //---------------Set up test pack-------------------
            //EvalAssignRecordSets(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, int update, IRecordSetCollection outputRecSets, IList<IDev2Definition> outputs)
            var innerEnvironment = new Mock<IExecutionEnvironment>();
            innerEnvironment.Setup(executionEnvironment => executionEnvironment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(),false))
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

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(EnvironmentOutputMappingManager).GetMethod("EvalAssignRecordSets", BindingFlags.NonPublic | BindingFlags.Static);
            try
            {
                //---------------Test Result -----------------------
                methodInfo.Invoke(null, new object[] { innerEnvironment.Object, environment.Object, 1, outputRecSets, outputs });
                environment.Verify(executionEnvironment => executionEnvironment.EvalAssignFromNestedStar(It.IsAny<string>(), It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), It.IsAny<int>()));
                environment.Verify(executionEnvironment => executionEnvironment.EvalAssignFromNestedLast(It.IsAny<string>(), It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), It.IsAny<int>()));
                environment.Verify(executionEnvironment => executionEnvironment.EvalAssignFromNestedNumeric(It.IsAny<string>(), It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), It.IsAny<int>()));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);

            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void EvalAssignScalars_GivenValidArgs_ShouldEvaluateCorrectlyAndAssignCorreclty()
        {
            //---------------Set up test pack-------------------
            //EvalAssignScalars(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, int update, IEnumerable<IDev2Definition> outputScalarList)
            var innerEnvironment = new Mock<IExecutionEnvironment>();
            innerEnvironment.Setup(executionEnvironment => executionEnvironment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
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


            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(EnvironmentOutputMappingManager).GetMethod("EvalAssignScalars", BindingFlags.NonPublic | BindingFlags.Static);
            try
            {
                //---------------Test Result -----------------------
                methodInfo.Invoke(null, new object[] { innerEnvironment.Object, environment.Object, 1, scalars });
                environment.Verify(executionEnvironment => executionEnvironment.Assign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(3));
                innerEnvironment.Verify(executionEnvironment => executionEnvironment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);

            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void EvalAssignComplexObjects_GivenValidArgs_ShouldEvaluateCorrectlyAndAssignCorreclty()
        {
            //---------------Set up test pack-------------------
            //EvalAssignScalars(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, int update, IEnumerable<IDev2Definition> outputScalarList)
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


            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var methodInfo = typeof(EnvironmentOutputMappingManager).GetMethod("EvalAssignComplexObjects", BindingFlags.NonPublic | BindingFlags.Static);
            try
            {
                //---------------Test Result -----------------------
                methodInfo.Invoke(null, new object[] { innerEnvironment.Object, environment.Object, scalars });
                environment.Verify(executionEnvironment => executionEnvironment.AddToJsonObjects(It.IsAny<string>(), It.IsAny<JContainer>()), Times.Exactly(3));
                innerEnvironment.Verify(executionEnvironment => executionEnvironment.EvalJContainer(It.IsAny<string>()), Times.Exactly(3));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);

            }
        }


    }
}
