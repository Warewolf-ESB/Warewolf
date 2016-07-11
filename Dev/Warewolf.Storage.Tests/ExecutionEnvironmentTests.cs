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
        //public void ExecutionEnvironmentGetCount_Should()
        //{
        //    Assert.IsNotNull(_mockEnv);
        //    Assert.IsNotNull(_env);
        //    _env.GetCount("");
        //    Assert.Fail("This test is not Correct");
        //}

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void ExecutionEnvironmentEvalAssignFromNestedLast_Should()
        //{
        //    Assert.IsNotNull(_mockEnv);
        //    Assert.IsNotNull(_env);
        //    _env.EvalAssignFromNestedLast(PersonNameExpression, null, 0);
        //}

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void ExecutionEnvironmentAssignFromNestedStar_Should()
        //{
        //    Assert.IsNotNull(_mockEnv);
        //    Assert.IsNotNull(_env);
        //    _env.EvalAssignFromNestedStar(PersonNameExpression, null, 0);
        //}

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenJsonObject_ExecutionEnvironmentEvalAsListOfStrings_ShouldReturnValue()
        {
            Assert.IsNotNull(_mockEnv);
            Assert.IsNotNull(_env);
            _env.AssignJson(new AssignValue(PersonNameExpression, "Sanele"), 0);
            var evalAsListOfStrings = _env.EvalAsListOfStrings(PersonNameExpression, 0);
            Assert.IsTrue(evalAsListOfStrings.Contains("Sanele"));
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenListResults_ExecutionEnvironmentEvalAsListOfStrings_ShouldReturnValuesAsList()
        {
            Assert.IsNotNull(_mockEnv);
            Assert.IsNotNull(_env);
            _env.AssignJson(new AssignValue(PersonNameExpression, "Sanele"), 0);
            var evalAsListOfStrings = _env.EvalAsListOfStrings(PersonNameExpression, 0);
            Assert.IsTrue(evalAsListOfStrings.Contains("Sanele"));
        }

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //[ExpectedException(typeof(Exception))]
        //public void GivenEmptyExpression_ExecutionEnvironmentEvalAsListOfStrings_ShouldReturnErrorMessage()
        //{
        //    Assert.IsNotNull(_mockEnv);
        //    Assert.IsNotNull(_env);
        //    var expression = "";
        //    var message = string.Format(ErrorResource.CouldNotRetrieveStringsFromExpression, expression);
        //    var evalAsListOfStrings = _env.EvalAsListOfStrings(expression, 0);
        //}

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void ExecutionEnvironmentAssignWithFrameAndList_Should()
        //{
        //    Assert.IsNotNull(_mockEnv);
        //    Assert.IsNotNull(_env);
        //    _env.AssignWithFrameAndList(PersonNameExpression, null, false, 0);
        //}


        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void ExecutionEnvironmentHasRecordSet_ShouldReturnTrue()
        //{
        //    Assert.IsNotNull(_mockEnv);
        //    Assert.IsNotNull(_env);
        //    var hasRecordSet = _env.HasRecordSet("[[rec().a]]");
        //    Assert.IsTrue(hasRecordSet);
        //}

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenIncorrectString_ExecutionEnvironmentHasRecordSet_ShouldReturnFalse()
        {
            Assert.IsNotNull(_mockEnv);
            Assert.IsNotNull(_env);
            var hasRecordSet = _env.HasRecordSet("RandomString");
            Assert.IsFalse(hasRecordSet);
        }

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void ExecutionEnvironmentAssignFromNestedNumeric_Should()
        //{
        //    Assert.IsNotNull(_mockEnv);
        //    Assert.IsNotNull(_env);
        //    _env.EvalAssignFromNestedNumeric(PersonNameExpression, null, 0);
        //}

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void ExecutionEnvironmentWarewolfEvalResultToString_Should()
        //{
        //    Assert.IsNotNull(_mockEnv);
        //    Assert.IsNotNull(_env);
        //    ExecutionEnvironment.WarewolfEvalResultToString(null);
        //}

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void GivenRecSet_ExecutionEnvironmentIsRecordSetName_ShouldReturnTrue()
        //{
        //    Assert.IsNotNull(_mockEnv);
        //    Assert.IsNotNull(_env);
        //    _env.Assign("[[rec(1).a]]", "SomeValue", 0);
        //    var isRecordSetName = ExecutionEnvironment.IsRecordSetName("[[rec(1).a]]");
        //    Assert.IsTrue(isRecordSetName);
        //}

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenInvalidRecSet_ExecutionEnvironmentIsRecordSetName_ShouldReturnFalse()
        {
            Assert.IsNotNull(_mockEnv);
            Assert.IsNotNull(_env);
            var isRecordSetName = ExecutionEnvironment.IsRecordSetName(InvalidScalar);
            Assert.IsFalse(isRecordSetName);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenValidExpression_ExecutionEnvironmentIsValidVariableExpression_ShouldReturnTrue()
        {
            Assert.IsNotNull(_mockEnv);
            Assert.IsNotNull(_env);
            string message;
            var isValidVariableExpression = ExecutionEnvironment.IsValidVariableExpression("[[a]]", out message, 0);
            Assert.IsTrue(isValidVariableExpression);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenInValidExpressionOrEmptyString_ExecutionEnvironmentIsValidVariableExpression_ShouldReturnFalse()
        {
            Assert.IsNotNull(_mockEnv);
            Assert.IsNotNull(_env);
            string message;
            //Given Invalid Scalar
            var isValidVariableExpression = ExecutionEnvironment.IsValidVariableExpression(InvalidScalar, out message, 0);
            Assert.IsFalse(isValidVariableExpression);
            //Given Empty Strign
            isValidVariableExpression = ExecutionEnvironment.IsValidVariableExpression(string.Empty, out message, 0);
            Assert.IsFalse(isValidVariableExpression);
        }

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void ExecutionEnvironmentGetLength_Should()
        //{
        //    Assert.IsNotNull(_mockEnv);            
        //    Assert.IsNotNull(_env);
        //    _env.GetLength("");
        //    Assert.Fail("This test is not Correct");
        //}

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentEvalToExpression_Should()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            _env.Assign("[[a]]", "SomeValue", 0);
            _env.EvalToExpression("[[a]]", 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentGetPositionColumnExpression_Should()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            var positionColumnExpression = ExecutionEnvironment.GetPositionColumnExpression("[[rec().a]]");
            Assert.IsNotNull(positionColumnExpression);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentConvertToIndex_Should()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            var convertToIndex = ExecutionEnvironment.ConvertToIndex("[[a]]", 0);
            Assert.IsNotNull(convertToIndex);
            convertToIndex = ExecutionEnvironment.ConvertToIndex("[[rec(1).a]]", 0);
            Assert.IsNotNull(convertToIndex);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenVariable_ExecutionEnvironmentIsScalar_ShouldBeTrue()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            var isScalar = ExecutionEnvironment.IsScalar("[[a]]");
            Assert.IsTrue(isScalar);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenInvalidScarOrSomeString_ExecutionEnvironmentIsScalar_ShouldBeFalse()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            var isScalar = ExecutionEnvironment.IsScalar("SomeString");
            Assert.IsFalse(isScalar);
            isScalar = ExecutionEnvironment.IsScalar("[[a]");
            Assert.IsFalse(isScalar);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentEvalAsList_Should()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            var evalAsList = _env.EvalAsList(PersonNameExpression, 0);
            Assert.IsNotNull(evalAsList);
            evalAsList = _env.EvalAsList(string.Empty, 0);
            Assert.IsNotNull(evalAsList);

        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentSortRecordSet_Should()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            var evalAsList = _env.EvalAsList(PersonNameExpression, 0);
            Assert.IsNotNull(evalAsList);
            evalAsList = _env.EvalAsList(string.Empty, 0);
            Assert.IsNotNull(evalAsList);

        }

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void ExecutionEnvironmentApplyUpdate_Should()
        //{
        //    Assert.IsNotNull(_mockEnv);            
        //    Assert.IsNotNull(_env);
        //    _env.Assign("[[a]]", "SomeValue", 0);
        //    _env.ApplyUpdate("[[a]]", null, 0);
        //}

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void ExecutionEnvironmentEvalWhere_Should()
        //{
        //    Assert.IsNotNull(_mockEnv);            
        //    Assert.IsNotNull(_env);
        //    _env.Assign("[[a]]", "SomeValue", 0);
        //    _env.EvalWhere("[[a]]", null, 0);
        //}

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenJSonExpression_ExecutionEnvironmentGetIndexes_ShouldReturn1Index()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            _env.AssignJson(new AssignValue(PersonNameExpression, "Sanele"), 0);
            var indexes = _env.GetIndexes(PersonNameExpression);
            Assert.AreEqual(1, indexes.Count);
            Assert.IsTrue(indexes.Contains(PersonNameExpression));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenRecSet_ExecutionEnvironmentGetIndexes_ShouldReturn1Index()
        {
            Assert.IsNotNull(_mockEnv);            
            Assert.IsNotNull(_env);
            var recA = "[[rec(*).a]]";
            _env.Assign(recA, "Something", 0);            
            var indexes = _env.GetIndexes(recA);
            Assert.AreEqual(1, indexes.Count);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenNullContainer_ExecutionEnvironmentBuildIndexMap_Should()
        {
            Assert.IsNotNull(_mockEnv);
            Assert.IsNotNull(_env);
            //var privateObj = new PrivateObject(_env);
            //DataStorage.WarewolfEnvironment warewolfEnvironment = PublicFunctions.CreateEnv(@"");
            //warewolfEnvironment.JsonObjects["Person"]
            //object[] args = { null, "", new List<string>(), null };
            //var arr = obj as JArray;
            //privateObj.Invoke("BuildIndexMap", args);
            //_env.BuildIndexMap(null, "", new List<string>(), null);
            //Assert.AreEqual(1, indexes.Count);
        }

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void ExecutionEnvironmentEvalDelete_Should()
        //{
        //    Assert.IsNotNull(_env);
        //    _env.EvalDelete("[[rec().a]]", 0);
        //    Assert.Fail("This test is not Correct");
        //}

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void GivenEmptyStringAndName_ExecutionEnvironmentAssignWithFrame_ShouldReturn()
        //{
        //    Assert.IsNotNull(_env);
        //    _env.AssignWithFrame(new AssignValue(string.Empty, "Value"), 0);
        //    _env.AssignWithFrame(new AssignValue(PersonNameExpression, "Value"), 0);         
        //}

        //[TestMethod]
        //[Owner("Sanele Mthembu")]
        //public void GivenEmptyStringAndName_ExecutionEnvironmentIsValidRecordSetIndex_ShouldReturn()
        //{
        //    Assert.IsNotNull(_env);
        //    Assert.IsTrue(ExecutionEnvironment.IsValidRecordSetIndex("[[rec().a]]"));
        //    Assert.Fail("This test is not Correct");
        //}

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
        //    var values = new List<string> { "John", "Mary", "Joe", "Moe" };
        //    Assert.IsNotNull(_env);
        //    _env.AssignUnique(list, values, null, 0);
        //    Assert.Fail("This test is not Correct");
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