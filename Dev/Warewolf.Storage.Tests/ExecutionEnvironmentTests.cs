using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using WarewolfParserInterop;
// ReSharper disable InconsistentNaming

namespace Warewolf.Storage.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ExecutionEnvironmentTest
    {
        private ExecutionEnvironment _environment;

        private const string OutOfBoundExpression = "[[rec(0).a]]";
        private const string InvalidScalar = "[[rec(0).a]";
        private const string PersonNameExpression = "[[@Person().Name]]";
        private const string ChildNameExpression = "[[@Person.Child().Name]]";
        private const string VariableA = "[[a]]";


        private readonly CommonFunctions.WarewolfEvalResult _warewolfEvalNothingResult =
            CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);

        [TestInitialize]
        public void MockSetup()
        {
            _environment = new ExecutionEnvironment();
            Assert.IsNotNull(_environment);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void GivenInvalidIndex_ExecutionEnvironmentEval_ShouldThrowIndexOutOfRangeException()
        {
            Assert.IsNotNull(_environment);
            _environment.Eval(OutOfBoundExpression, 0, true);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenRecSetName_ExecutionEnvironmentGetCount_ShouldReturn1()
        {         
            Assert.IsNotNull(_environment);            
            _environment.Assign("[[rec().a]]", "sanele", 0);
            var recordSet = _environment.GetCount("rec");
            Assert.AreEqual(1, recordSet);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenRecSet_ExecutionEnvironmentEvalAssignFromNestedLast_Should()
        {
            Assert.IsNotNull(_environment);
            var evalMultiAssign = EvalMultiAssign();
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedLast("[[rec(*).a]]", warewolfAtomListresult, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenNonExistingRec_ExecutionEnvironmentEvalAssignFromNestedLast_ShouldAddStar()
        {
            Assert.IsNotNull(_environment);
            _environment.Assign("[[rec().a]]", "sanele", 0);
            var evalMultiAssign = EvalMultiAssign();
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedLast("[[recs().a]]", warewolfAtomListresult, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentAssignFromNestedStar_Should()
        {
            Assert.IsNotNull(_environment);
            _environment.Assign("[[rec().a]]", "sanele", 0);
            var evalMultiAssign = EvalMultiAssign();
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedStar("[[rec().a]]", warewolfAtomListresult, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentSortRecordSet_ShouldSortRecordSets()
        {
            Assert.IsNotNull(_environment);
            _environment.Assign("[[rec().a]]", "sanele", 0);
            var evalMultiAssign = EvalMultiAssign();
            PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            _environment.SortRecordSet("[[rec().a]]", true, 0);
        }

        private static DataStorage.WarewolfEnvironment EvalMultiAssign()
        {
            var assigns = new List<IAssignValue>
            {
                new AssignValue("[[rec(1).a]]", "27"),
                new AssignValue("[[rec(3).a]]", "33"),
                new AssignValue("[[rec(2).a]]", "25")
            };
            var envEmpty = WarewolfTestData.CreateTestEnvEmpty("");
            var evalMultiAssign = PublicFunctions.EvalMultiAssign(assigns, 0, envEmpty);
            return evalMultiAssign;
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment_EvalAsList_WhenRecSet_ShouldReturnListOfAllValues()
        {
            //------------Setup for test--------------------------
            _environment.Assign("[[rec(1).a]]", "27", 0);
            _environment.Assign("[[rec(1).b]]", "bob", 0);
            _environment.Assign("[[rec(2).a]]", "31", 0);
            _environment.Assign("[[rec(2).b]]", "mary", 0);
            //------------Execute Test---------------------------
            var list = _environment.EvalAsList("[[rec(*)]]", 0).ToList();
            //------------Assert Results-------------------------
            Assert.IsNotNull(list);
            Assert.AreEqual("27",list[0].ToString());
            Assert.AreEqual("31",list[1].ToString());
            Assert.AreEqual("bob",list[2].ToString());
            Assert.AreEqual("mary",list[3].ToString());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment_EvalAsListOfString_WhenRecSet_ShouldReturnListOfAllValues()
        {
            //------------Setup for test--------------------------
            _environment.Assign("[[rec(1).a]]", "27", 0);
            _environment.Assign("[[rec(1).b]]", "bob", 0);
            _environment.Assign("[[rec(2).a]]", "31", 0);
            _environment.Assign("[[rec(2).b]]", "mary", 0);
            //------------Execute Test---------------------------
            var list = _environment.EvalAsListOfStrings("[[rec(*)]]", 0).ToList();
            //------------Assert Results-------------------------
            Assert.IsNotNull(list);
            Assert.AreEqual("27",list[0]);
            Assert.AreEqual("31",list[1]);
            Assert.AreEqual("bob",list[2]);
            Assert.AreEqual("mary",list[3]);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment_EvalAsString_WhenRecSet_ShouldReturnListOfAllValues()
        {
            //------------Setup for test--------------------------
            _environment.Assign("[[rec(1).a]]", "27", 0);
            _environment.Assign("[[rec(1).b]]", "bob", 0);
            _environment.Assign("[[rec(2).a]]", "31", 0);
            _environment.Assign("[[rec(2).b]]", "mary", 0);
            //------------Execute Test---------------------------
            var stringVal = ExecutionEnvironment.WarewolfEvalResultToString(_environment.Eval("[[rec(*)]]", 0));
            //------------Assert Results-------------------------
            Assert.IsNotNull(stringVal);
            Assert.AreEqual("27,31,bob,mary", stringVal);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenJsonObject_ExecutionEnvironmentEvalAsListOfStrings_ShouldReturnValue()
        {
            Assert.IsNotNull(_environment);
            _environment.Assign("[[Person().Name]]", "Sanele", 0);
            _environment.Assign("[[Person().Name]]", "Nathi", 0);
            var evalAsListOfStrings = _environment.EvalAsListOfStrings("[[Person(*).Name]]", 0);
            Assert.IsTrue(evalAsListOfStrings.Contains("Sanele"));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenListResults_ExecutionEnvironmentEvalAsListOfStrings_ShouldReturnValuesAsList()
        {
            Assert.IsNotNull(_environment);
            _environment.AssignJson(new AssignValue(PersonNameExpression, "Sanele"), 0);
            var evalAsListOfStrings = _environment.EvalAsListOfStrings(PersonNameExpression, 0);
            Assert.IsTrue(evalAsListOfStrings.Contains("Sanele"));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentAssignWithFrameAndList_Should()
        {            
            Assert.IsNotNull(_environment);
            _environment.AssignWithFrameAndList(VariableA,
                new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.NewDataString("Test Value")),
                false, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenRecSet_ExecutionEnvironmentHasRecordSet_ShouldReturnTrue()
        {
            var executionEnv = new ExecutionEnvironment();
            executionEnv.Assign("[[rec().a]]","bob",0);
            var hasRecordSet = executionEnv.HasRecordSet("[[rec()]]");
            Assert.IsTrue(hasRecordSet);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenIncorrectString_ExecutionEnvironmentHasRecordSet_ShouldReturnFalse()
        {
            Assert.IsNotNull(_environment);
            var hasRecordSet = _environment.HasRecordSet("RandomString");
            Assert.IsFalse(hasRecordSet);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenJson_ExecutionEnvironmentToStar_ShouldReturnAddStar()
        {
            
            Assert.IsNotNull(_environment);
            var star = _environment.ToStar(PersonNameExpression);
            Assert.IsNotNull(star);
            Assert.AreEqual("[[@Person(*).Name]]", star);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenRecSet_ExecutionEnvironmentToStar_ShouldReturnAddStar()
        {            
            var star = _environment.ToStar("[[rec().a]]");
            Assert.IsNotNull(star);
            Assert.AreEqual("[[rec(*).a]]", star);

            star = _environment.ToStar("[[rec()]]");
            Assert.IsNotNull(star);
            Assert.AreEqual("[[rec(*)]]", star);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenVariable_ExecutionEnvironmentToStar_ShouldReturnTheSame()
        {
            Assert.IsNotNull(_environment);
            var star = _environment.ToStar(VariableA);
            Assert.IsNotNull(star);
            Assert.AreEqual(VariableA, star);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentAssignFromNestedNumeric_Should()
        {
            Assert.IsNotNull(_environment);
            _environment.Assign("[[rec().a]]", "sanele", 0);
            var evalMultiAssign = EvalMultiAssign();
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0,false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedNumeric("[[rec().a]]", warewolfAtomListresult, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentWarewolfEvalResultToString_Should()
        {
            var warewolfEvalResultToString = ExecutionEnvironment.WarewolfEvalResultToString(
                CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(
                    new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.NewDataString("Test string"))));
            Assert.IsNotNull(warewolfEvalResultToString);

            warewolfEvalResultToString = ExecutionEnvironment.WarewolfEvalResultToString(
                CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(
                    DataStorage.WarewolfAtom.NewDataString("Test string")));
            Assert.IsNotNull(warewolfEvalResultToString);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenRecSet_ExecutionEnvironmentIsRecordSetName_ShouldReturnTrue()
        {
            Assert.IsNotNull(_environment);
            _environment.Assign("[[rec(1).a]]", "Test Value", 0);
            var isRecordSetName = ExecutionEnvironment.IsRecordSetName("[[rec()]]");
            Assert.IsTrue(isRecordSetName);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenInvalidRecSet_ExecutionEnvironmentIsRecordSetName_ShouldReturnFalse()
        {
            Assert.IsNotNull(_environment);
            var isRecordSetName = ExecutionEnvironment.IsRecordSetName(InvalidScalar);
            Assert.IsFalse(isRecordSetName);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenValidExpression_ExecutionEnvironmentIsValidVariableExpression_ShouldReturnTrue()
        {
            Assert.IsNotNull(_environment);
            string message;            
            var isValidVariableExpression = ExecutionEnvironment.IsValidVariableExpression(VariableA, out message, 0);
            Assert.IsTrue(isValidVariableExpression);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenInValidExpressionOrEmptyString_ExecutionEnvironmentIsValidVariableExpression_ShouldReturnFalse()
        {
            Assert.IsNotNull(_environment);
            string message;
            //Given Invalid Scalar
            var isValidVariableExpression = ExecutionEnvironment.IsValidVariableExpression(InvalidScalar, out message, 0);
            Assert.IsFalse(isValidVariableExpression);
            //Given Empty Strign
            isValidVariableExpression = ExecutionEnvironment.IsValidVariableExpression(string.Empty, out message, 0);
            Assert.IsFalse(isValidVariableExpression);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentGetLength_Should()
        {
            Assert.IsNotNull(_environment);
            _environment.Assign("[[rec().a]]", "sanele", 0);
            var recordSet = _environment.GetLength("rec");
            Assert.AreEqual(1, recordSet);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentEvalToExpression_Should()
        {            
            Assert.IsNotNull(_environment);
            _environment.Assign(VariableA, "SomeValue", 0);
            var evalToExpression = _environment.EvalToExpression(VariableA, 0);
            Assert.AreEqual("[[a]]", evalToExpression);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenRecSet_ExecutionEnvironmentGetPositionColumnExpression_ShouldReturn()
        {
            Assert.IsNotNull(_environment);
            var positionColumnExpression = ExecutionEnvironment.GetPositionColumnExpression("[[rec()]]");
            Assert.IsNotNull(positionColumnExpression);
            Assert.IsTrue(positionColumnExpression.Contains("rec(*)"));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenVariable_ExecutionEnvironmentGetPositionColumnExpression_ShouldReturnSameVariable()
        {
            Assert.IsNotNull(_environment);
            var positionColumnExpression = ExecutionEnvironment.GetPositionColumnExpression("[[rec]]");
            Assert.IsNotNull(positionColumnExpression);
            Assert.AreEqual("[[rec]]", positionColumnExpression);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentConvertToIndex_Should()
        {            
            Assert.IsNotNull(_environment);
            var convertToIndex = ExecutionEnvironment.ConvertToIndex(VariableA, 0);
            Assert.IsNotNull(convertToIndex);
            convertToIndex = ExecutionEnvironment.ConvertToIndex("[[rec(1).a]]", 0);
            Assert.IsNotNull(convertToIndex);

            convertToIndex = ExecutionEnvironment.ConvertToIndex("[[rec(*).a]]", 0);
            Assert.IsNotNull(convertToIndex);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenVariable_ExecutionEnvironmentIsScalar_ShouldBeTrue()
        {
            
            Assert.IsNotNull(_environment);
            var isScalar = ExecutionEnvironment.IsScalar(VariableA);
            Assert.IsTrue(isScalar);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenInvalidScarOrSomeString_ExecutionEnvironmentIsScalar_ShouldBeFalse()
        {
            
            Assert.IsNotNull(_environment);
            var isScalar = ExecutionEnvironment.IsScalar("SomeString");
            Assert.IsFalse(isScalar);
            isScalar = ExecutionEnvironment.IsScalar("[[a]");
            Assert.IsFalse(isScalar);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentEvalAsList_Should()
        {
            
            Assert.IsNotNull(_environment);
            var evalAsList = _environment.EvalAsList(PersonNameExpression, 0);
            Assert.IsNotNull(evalAsList);
            evalAsList = _environment.EvalAsList(string.Empty, 0);
            Assert.IsNotNull(evalAsList);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentApplyUpdate_Should()
        {
            Assert.IsNotNull(_environment);
            _environment.Assign(VariableA, "SomeValue", 0);
            var clause =
                new Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom>(atom => atom);
            _environment.ApplyUpdate(VariableA, clause, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenIsNothingEval_ExecutionEnvironmentEvalWhere_ShouldReturnNothing()
        {
            Assert.IsNotNull(_environment);
            _environment.Assign(VariableA, "SomeValue", 0);
            var clause = new Func<DataStorage.WarewolfAtom, bool>(atom => atom.IsNothing);
            var evalWhere = _environment.EvalWhere("[[rec()]]",clause, 0);
            Assert.IsNotNull(evalWhere);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenJSonExpression_ExecutionEnvironmentGetIndexes_ShouldReturn1Index()
        {            
            Assert.IsNotNull(_environment);
            _environment.AssignJson(new AssignValue(PersonNameExpression, "Sanele"), 0);
            var indexes = _environment.GetIndexes(PersonNameExpression);
            Assert.AreEqual(1, indexes.Count);
            Assert.IsTrue(indexes.Contains(PersonNameExpression));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenRecSet_ExecutionEnvironmentGetIndexes_ShouldReturn1Index()
        {            
            Assert.IsNotNull(_environment);
            const string recA = "[[rec(*).a]]";
            _environment.Assign(recA, "Something", 0);
            var indexes = _environment.GetIndexes(recA);
            Assert.AreEqual(1, indexes.Count);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenContainer_ExecutionEnvironmentBuildIndexMap_ShouldBuildMapForChildObj()
        {
            Assert.IsNotNull(_environment);
            _environment.AssignJson(new AssignValue(ChildNameExpression, "Sanele"), 0);
            var privateObj = new PrivateObject(_environment);
            var var = EvaluationFunctions.parseLanguageExpressionWithoutUpdate(ChildNameExpression);
            var jsonIdentifierExpression = var as LanguageAST.LanguageExpression.JsonIdentifierExpression;
            var obj = new JArray(ChildNameExpression);
            if (jsonIdentifierExpression == null) return;
            var mapItems = new List<string>();
            object[] args = {jsonIdentifierExpression.Item, "", mapItems, obj };
            privateObj.Invoke("BuildIndexMap", args);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentEvalDelete_Should()
        {
            Assert.IsNotNull(_environment);
            _environment.Assign("[[rec().a]]", "Some Value", 0);
            _environment.EvalDelete("[[rec()]]", 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenEmptyStringAndName_ExecutionEnvironmentAssignWithFrame_ShouldReturn()
        {
            Assert.IsNotNull(_environment);
            _environment.AssignWithFrame(new AssignValue(PersonNameExpression, "Value"), 0);
            try
            {
                _environment.AssignWithFrame(new AssignValue(string.Empty, "Value"), 0);
            }
            catch(Exception e)
            {
                Assert.AreEqual("invalid variable assigned to ", e.Message);                
            }            
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenEmptyStringAndName_ExecutionEnvironmentIsValidRecordSetIndex_ShouldReturn()
        {
            Assert.IsNotNull(_environment);
            Assert.IsTrue(ExecutionEnvironment.IsValidRecordSetIndex("[[rec().a]]"));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentAssignDataShape_ShouldReturn()
        {
            Assert.IsNotNull(_environment);
            _environment.AssignDataShape("[[SomeString]]");
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [ExpectedException(typeof(Exception))]
        public void GivenInvalidScalar_ExecutionEnvironmentAssignWithFrame_ShouldThrowException()
        {
            Assert.IsNotNull(_environment);
            _environment.AssignWithFrame(new AssignValue(InvalidScalar, "Value"), 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentEvalForDataMerge_Should()
        {
            Assert.IsNotNull(_environment);
            _environment.Assign(VariableA, "Sanele", 0);
            _environment.EvalForDataMerge(VariableA, 0);
        }

        [ExpectedException(typeof(NullValueInVariableException))]
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenUnAssignedVar_ExecutionEnvironmentEvalStrict_ShouldThrowNullValueInVariableException()
        {
            Assert.IsNotNull(_environment);
            _environment.EvalStrict(PersonNameExpression, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentEvalStrict_Should()
        {
            Assert.IsNotNull(_environment);
            _environment.Assign(PersonNameExpression, "Sanele", 0);
            _environment.EvalStrict(PersonNameExpression, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenEmptyString_ExecutionEnvironmenAssign_ShouldReturn()
        {
            Assert.IsNotNull(_environment);
            _environment.Assign(string.Empty, string.Empty, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentAssignUnique_Should()
        {
            var recs = new List<string>
            {
                "[[Person().Name]]",
                "[[Person(1).Name]]",
                "[[Person(2).Name]]"
            };
            var values = new List<string> { "[[Person().Name]]" };
            
            Assert.IsNotNull(_environment);
            _environment.Assign("[[Person().Name]]", "sanele", 0);
            var resList = new List<string>();
            _environment.AssignUnique(recs, values, resList, 0);
            Assert.IsNotNull(resList);            
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [ExpectedException(typeof(Exception))]
        public void GivenInvalidExpression_ExecutionEnvironmentEval_ShouldThrowException()
        {
            Assert.IsNotNull(_environment);
            const string expression = "[[rec(0).a]";
            _environment.Eval(expression, 0, true);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenInvalidExpressionAndthrowsifnotexistsIsFalse_ExecutionEnvironmentEval_ShouldReturnNothing()
        {
            Assert.IsNotNull(_environment);
            const string expression = "[[rec(0).a]";
            var warewolfEvalResult = _environment.Eval(expression, 0);
            Assert.AreEqual(_warewolfEvalNothingResult, warewolfEvalResult);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenEmptyString_ExecutionEnvironmentEvalJContainer_ShouldReturn()
        {
            
            Assert.IsNotNull(_environment);
            var evalJContainer = _environment.EvalJContainer(string.Empty);
            Assert.IsNull(evalJContainer);

            const string something = "new {string valu3};";
            evalJContainer = _environment.EvalJContainer(something);
            Assert.IsNull(evalJContainer);

            _environment.AssignJson(new AssignValue(PersonNameExpression, "Sanele"), 0);
            evalJContainer = _environment.EvalJContainer(PersonNameExpression);
            Assert.IsNotNull(evalJContainer);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenEmptyString_ExecutionEnvironmentEvalForJason_ShouldReturnNothing()
        {
            Assert.IsNotNull(_environment);
            var warewolfEvalResult = _environment.EvalForJson(string.Empty);
            Assert.AreEqual(_warewolfEvalNothingResult, warewolfEvalResult);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void GivenInvalidScalar_ExecutionEnvironmentEvalForJason_ShouldReturnNothing()
        {            
            Assert.IsNotNull(_environment);
            var warewolfEvalResult = _environment.EvalForJson(OutOfBoundExpression);
            Assert.AreEqual(_warewolfEvalNothingResult, warewolfEvalResult);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenInvalidScalar_ExecutionEnvironmentEvalForJason_ShouldException()
        {
            
            Assert.IsNotNull(_environment);
            var warewolfEvalResult = _environment.EvalForJson(InvalidScalar);
            Assert.AreEqual(_warewolfEvalNothingResult, warewolfEvalResult);
            warewolfEvalResult = _environment.EvalForJson("[[rec().a]]");
            Assert.IsNotNull(warewolfEvalResult);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenEmptyString_ExecutionEnvironmentAssignJson_ShouldReturn()
        {
            
            Assert.IsNotNull(_environment);
            var values = new AssignValue(string.Empty, "John");
            _environment.AssignJson(values, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenObjectExecutionEnvironmentAssignJson_ShouldAddObject()
        {
            
            Assert.IsNotNull(_environment);
            var values = new List<IAssignValue> { new AssignValue("[[@Person.Name]]", "John") };
            _environment.AssignJson(values, 0);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [ExpectedException(typeof(Exception))]
        public void GivenInvalidObject_ExecutionEnvironmentAssignJson_ShouldThrowParseError()
        {
            
            Assert.IsNotNull(_environment);
            var values = new AssignValue("[[@Person.Name]", "John");
            _environment.AssignJson(values, 0);
            Assert.AreEqual(1, _environment.Errors.Count);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironment_ShouldHave_Ctor()
        {            
            Assert.IsNotNull(_environment);
            var privateObj = new PrivateObject(_environment);
            var field = privateObj.GetField("_env");
            Assert.IsNotNull(field);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExecutionEnvironmentCtor_ShouldErrorsCountAs0()
        {
            Assert.IsNotNull(_environment);
            Assert.AreEqual(0, _environment.AllErrors.Count);
            Assert.AreEqual(0, _environment.Errors.Count);
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
            Assert.IsNotNull(_environment);
            _environment.AddToJsonObjects(PersonNameExpression, null);
            var privateObj = new PrivateObject(_environment);
            var field = privateObj.GetFieldOrProperty("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(field != null && field.JsonObjects.Count > 0);
        }

        [ExpectedException(typeof(NullValueInVariableException))]
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
            Assert.IsNotNull(_environment);
            var countBefore = _environment.Errors.Count;
            Assert.AreEqual(0, _environment.Errors.Count);
            _environment.AddError(It.IsAny<string>());
            Assert.AreEqual(countBefore + 1, _environment.Errors.Count);
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
            _environment.Errors.Add("SomeError");
            return _environment;
        }

        #endregion        
    }
}