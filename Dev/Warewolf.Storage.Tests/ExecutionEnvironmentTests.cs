using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WarewolfParserInterop;

namespace Warewolf.Storage.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ExecutionEnvironmentTest
    {
        private Mock<IExecutionEnvironment> _mockEnv;
        private ExecutionEnvironment _env;

        private const string OutOfBoundExpression = "[[rec(0).a]]";
        private const string InvalidScalar = "[[rec(0).a]";
        private const string PersonNameExpression = "[[@Person().Name]]";
        private const string ScalarA = "[[a]]";


        private readonly CommonFunctions.WarewolfEvalResult _warewolfEvalNothingResult =
            CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);

        [TestInitialize]
        public void MockSetup()
        {
            _mockEnv = new Mock<IExecutionEnvironment>();           
            _env = new ExecutionEnvironment();
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ExecutionEnvironmentEval_ShouldThrowIndexOutOfRangeException()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            _env.Eval(OutOfBoundExpression, 0, true);
        }

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void ExecutionEnvironmentEvalDelete_Should()
        //{
        //    Assert.IsNotNull(_env);
        //    _env.EvalDelete("[[rec().a]]", 0);         
        //}

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenEmptyStringAndName_ExecutionEnvironmentAssignWithFrame_ShouldReturn()
        {
            Assert.IsNotNull(_env);
            _env.AssignWithFrame(new AssignValue(string.Empty, "Value"), 0);
            _env.AssignWithFrame(new AssignValue(PersonNameExpression, "Value"), 0);         
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentAssignDataShape_ShouldReturn()
        {
            Assert.IsNotNull(_env);
            _env.AssignDataShape("[[SomeString]]");
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [ExpectedException(typeof(Exception))]
        public void GivenInvalidScalar_ExecutionEnvironmentAssignWithFrame_ShouldThrowException()
        {
            Assert.IsNotNull(_env);
            _env.AssignWithFrame(new AssignValue(InvalidScalar, "Value"), 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentEvalForDataMerge_Should()
        {            
            Assert.IsNotNull(_env);
            _env.Assign(ScalarA, "Sanele", 0);
            _env.EvalForDataMerge(ScalarA, 0);
        }

        [ExpectedException(typeof(NullValueInVariableException))]
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenUnAssignedVar_ExecutionEnvironmentEvalStrict_ShouldThrowNullValueInVariableException()
        {
            Assert.IsNotNull(_env);
            _env.EvalStrict(PersonNameExpression, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentEvalStrict_Should()
        {            
            Assert.IsNotNull(_env);
            _env.Assign(PersonNameExpression, "Sanele", 0);
            _env.EvalStrict(PersonNameExpression, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenEmptyString_ExecutionEnvironmenAssign_ShouldReturn()
        {            
            Assert.IsNotNull(_env);
            _env.Assign(string.Empty, string.Empty, 0);
        }

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void ExecutionEnvironmentAssignUnique_Should()
        //{
        //    var list = new List<string>
        //    {
        //        "[[@Person.Name]]",
        //        "[[@Person.Children(1).Name]]",
        //        "[[@Person.Children(2).Name]]",
        //        "[[@Person.Children(*).Name]]"
        //    };
        //    var values = new List<string> {"John", "Mary", "Joe", "Moe"};
        //    Assert.IsNotNull(_env);
        //    _env.AssignUnique(list, values, null, 0);
        //}

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [ExpectedException(typeof(Exception))]
        public void GivenInvalidExpression_ExecutionEnvironmentEval_ShouldThrowException()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            const string expression = "[[rec(0).a]";
            _env.Eval(expression, 0, true);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenInvalidExpressionAndthrowsifnotexistsIsFalse_ExecutionEnvironmentEval_ShouldReturnNothing()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);            
            const string expression = "[[rec(0).a]";
            var warewolfEvalResult = _env.Eval(expression, 0);
            Assert.AreEqual(_warewolfEvalNothingResult, warewolfEvalResult);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenEmptyString_ExecutionEnvironmentEvalJContainer_ShouldReturn()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            var evalJContainer = _env.EvalJContainer(string.Empty);
            Assert.IsNull(evalJContainer);

            const string something = "new {string valu3};";
            evalJContainer = _env.EvalJContainer(something);
            Assert.IsNull(evalJContainer);
            
            _env.AssignJson(new AssignValue(PersonNameExpression, "Sanele"),0);
            evalJContainer = _env.EvalJContainer(PersonNameExpression);
            Assert.IsNull(evalJContainer);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenEmptyString_ExecutionEnvironmentEvalForJason_ShouldReturnNothing()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            var warewolfEvalResult = _env.EvalForJson(string.Empty);
            Assert.AreEqual(_warewolfEvalNothingResult, warewolfEvalResult);
        }        

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenInvalidScalar_ExecutionEnvironmentEvalForJason_ShouldException()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);            
            var warewolfEvalResult = _env.EvalForJson(InvalidScalar);
            Assert.AreEqual(_warewolfEvalNothingResult, warewolfEvalResult);
            warewolfEvalResult = _env.EvalForJson("[[rec().a]]");
            Assert.IsNotNull(warewolfEvalResult);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenEmptyString_ExecutionEnvironmentAssignJson_ShouldReturn()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            var values = new AssignValue(string.Empty, "John");
            _env.AssignJson(values, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenObjectExecutionEnvironmentAssignJson_ShouldAddObject()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            var values = new List<IAssignValue> { new AssignValue("[[@Person.Name]]", "John") };
            _env.AssignJson(values, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [ExpectedException(typeof(Exception))]
        public void GivenInvalidObject_ExecutionEnvironmentAssignJson_ShouldThrowParseError()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            var values = new AssignValue("[[@Person.Name]", "John");
            _env.AssignJson(values, 0);
            Assert.AreEqual(1, _env.Errors.Count);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironment_ShouldHave_Ctor()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);

            var privateObj = new PrivateObject(_env);
            var field = privateObj.GetField("_env");
            Assert.IsNotNull(field);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentCtor_ShouldErrorsCountAs0()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            Assert.AreEqual(0, _env.AllErrors.Count);
            Assert.AreEqual(0, _env.Errors.Count);
        }
        
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentWarewolfAtomToStringErrorIfNull_ShouldReturnStringEmpty()
        {
            const string expected = "SomeString";
            var givenSomeString = DataStorage.WarewolfAtom.NewDataString(expected);
            var result = ExecutionEnvironment.WarewolfAtomToStringErrorIfNull(givenSomeString);
            Assert.AreEqual(expected, result);

            result = ExecutionEnvironment.WarewolfAtomToStringErrorIfNull(null);
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentAddToJsonObjects_ShouldAddJsonObject()
        {
            Assert.IsNotNull(_env);
            _env.AddToJsonObjects(PersonNameExpression,null);
        }
                
        [ExpectedException(typeof (NullValueInVariableException))]
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentWarewolfAtomToStringErrorIfNull_ShouldThrowAnError()
        {
            var atom = DataStorage.WarewolfAtom.Nothing;
            ExecutionEnvironment.WarewolfAtomToStringErrorIfNull(atom);
        }

        [TestMethod]
        public void ExecutionEnvironmentWarewolfAtomToStringNullAsNothing_ShouldReturnNull()
        {            
            var givenNoting = DataStorage.WarewolfAtom.Nothing;
            var result = ExecutionEnvironment.WarewolfAtomToStringNullAsNothing(givenNoting);
            Assert.IsNull(result);
      
            var givenSomeString = DataStorage.WarewolfAtom.NewDataString("SomeString");
            result = ExecutionEnvironment.WarewolfAtomToStringNullAsNothing(givenSomeString);
            Assert.AreEqual(givenSomeString, result);

            result = ExecutionEnvironment.WarewolfAtomToStringNullAsNothing(null);
            Assert.IsNull(result);            
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenNullForWarewolfAtom_ExecutionEnvironmentWarewolfAtomToString_ShouldReturnNull()
        {
            var result = ExecutionEnvironment.WarewolfAtomToString(null);
            Assert.IsTrue(string.IsNullOrEmpty(result));
            const string somestring = "SomeString";
            var atom = DataStorage.WarewolfAtom.NewDataString(somestring);
            result = ExecutionEnvironment.WarewolfAtomToString(atom);
            Assert.AreEqual(somestring, result);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentAddError_ShouldIncreaseErrorCount()
        {
            Assert.IsNotNull(_env);
            var countBefore = _env.Errors.Count;                        
            Assert.AreEqual(0, _env.Errors.Count);
            _env.AddError(It.IsAny<string>());
            Assert.AreEqual(countBefore+1, _env.Errors.Count);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenErrorsExecutionEnvironmentHasError_ShouldBeTrue()
        {
            var envWithErrors = CreateEnvironmentWithErrors();
            Assert.IsNotNull(envWithErrors);                        
            Assert.IsTrue(envWithErrors.Errors.Count > 0);            
            Assert.IsTrue(envWithErrors.HasErrors());
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenErrorsAndAllErrorsHaveCount_ExecutionEnvironmentFetchError_ShouldJoinAllErrors()
        {
            var envWithErrors = CreateEnvironmentWithErrors();
            Assert.IsNotNull(envWithErrors);
            envWithErrors.AllErrors.Add("AnotherError");
            Assert.IsTrue(envWithErrors.Errors.Count > 0);            
            Assert.IsTrue(envWithErrors.HasErrors());
            var expected = $"AnotherError{Environment.NewLine}SomeError";
            Assert.AreEqual(expected, envWithErrors.FetchErrors());
        }

        #region Private Methods

        private ExecutionEnvironment CreateEnvironmentWithErrors()
        {
            _env.Errors.Add("SomeError");
            return _env;
        }

        #endregion

    }
}