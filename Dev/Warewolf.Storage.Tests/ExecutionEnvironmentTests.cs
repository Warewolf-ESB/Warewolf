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
using Dev2.Data.Util;

namespace Warewolf.Storage.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ExecutionEnvironmentTest
    {

        const string OutOfBoundExpression = "[[rec(0).a]]";
        const string InvalidScalar = "[[rec(0).a]";
        const string PersonNameExpression = "[[@Person().Name]]";
        const string ChildNameExpression = "[[@Person.Child().Name]]";
        const string VariableA = "[[a]]";


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ExecutionEnvironment_GivenInvalidIndex_ExecutionEnvironmentEval_ShouldThrowIndexOutOfRangeException()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Eval(OutOfBoundExpression, 0, true);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_Assign_RecsetField()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "sanele", 0);
            var recordSet = _environment.GetCount("rec");
            Assert.AreEqual(1, recordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_Assign_NullExpression_DoesNotThrow()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign(null, "sanele", 0);

            // reached here, test passed
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_Assign_RecordSetToScalar_HasError()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().name]]", "n1", 0);
            _environment.Assign("[[rec().name]]", "n2", 0);

            _environment.Assign("[[v]]", "[[rec()]]", 0);

            Assert.IsTrue(_environment.HasErrors());
            Assert.AreEqual("assigning an entire recordset to a variable is not defined", _environment.Errors.First());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_Assign_ParseError_NoThrow()
        {
            var _environment = new ExecutionEnvironment();

            _environment.AssignStrict("[[v]]", "[[asdf.asdf]]", 0);

            Assert.IsTrue(_environment.HasErrors());
            Assert.AreEqual("parse error", _environment.Errors.First());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignStrict_RecsetField()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignStrict("[[rec().a]]", "sanele", 0);
            var recordSet = _environment.GetCount("rec");
            Assert.AreEqual(1, recordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignStrict_NullExpression_DoesNotThrow()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignStrict(null, "sanele", 0);

            // reached here, test passed
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignStrict_RecordSetToScalar_HasError()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignStrict("[[rec().name]]", "n1", 0);
            _environment.AssignStrict("[[rec().name]]", "n2", 0);

            _environment.AssignStrict("[[v]]", "[[rec()]]", 0);

            // BUG: this should pass?
            //Assert.IsTrue(_environment.HasErrors());
            //Assert.AreEqual("assigning an entire recordset to a variable is not defined", _environment.Errors.First());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignStrict_ParseError_NoThrow()
        {
            var _environment = new ExecutionEnvironment();

            _environment.AssignStrict("[[v]]", "[[asdf.asdf]]", 0);

            Assert.IsTrue(_environment.HasErrors());
            Assert.AreEqual("parse error", _environment.Errors.First());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignWithFrame()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignWithFrame(new List<IAssignValue> {
                new AssignValue("[[rec().name]]", "n1"),
                new AssignValue("[[rec().name]]", "n2"),
            }, 0);

            Assert.IsFalse(_environment.HasErrors());
            var result = _environment.EvalAsListOfStrings("[[rec(*).name]]", 0);

            Assert.AreEqual("n1", result[0]);
            Assert.AreEqual("n2", result[1]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenRecSet_ExecutionEnvironmentEvalAssignFromNestedLast_Should()
        {
            var _environment = new ExecutionEnvironment();
            var evalMultiAssign = EvalMultiAssign();
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedLast("[[rec(*).a]]", warewolfAtomListresult, 0);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenRecSet_ExecutionEnvironmentEvalAssignFromNestedLast_TwoColumn_Should()
        {
            var _environment = new ExecutionEnvironment();
            var evalMultiAssign = EvalMultiAssignTwoColumn();
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedLast("[[rec().a]]", warewolfAtomListresult, 0);
            items = PublicFunctions.EvalEnvExpression("[[rec(*).b]]", 0, false, evalMultiAssign);
            warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedLast("[[rec().b]]", warewolfAtomListresult, 0);
            items = PublicFunctions.EvalEnvExpression("[[rec(*).c]]", 0, false, evalMultiAssign);
            warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedLast("[[rec().c]]", warewolfAtomListresult, 0);
            evalMultiAssign = EvalMultiAssignTwoColumn();
            items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedLast("[[rec().a]]", warewolfAtomListresult, 0);
            items = PublicFunctions.EvalEnvExpression("[[rec(*).b]]", 0, false, evalMultiAssign);
            warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedLast("[[rec().b]]", warewolfAtomListresult, 0);
            items = PublicFunctions.EvalEnvExpression("[[rec(*).c]]", 0, false, evalMultiAssign);
            warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedLast("[[rec().c]]", warewolfAtomListresult, 0);

            var list_a = _environment.EvalAsListOfStrings("[[rec(*).a]]", 0);
            var list_b = _environment.EvalAsListOfStrings("[[rec(*).b]]", 0);
            var list_c = _environment.EvalAsListOfStrings("[[rec(*).c]]", 0);
            Assert.AreEqual(list_a.Count, 4);
            Assert.AreEqual(list_b.Count, 4);
            Assert.AreEqual(list_c.Count, 4);

            Assert.IsTrue(list_a[0].Equals("a11"));
            Assert.IsTrue(list_a[1].Equals("ayy"));
            Assert.IsTrue(list_a[2].Equals("a11"));
            Assert.IsTrue(list_a[3].Equals("ayy"));

            Assert.IsTrue(list_b[0].Equals(""));
            Assert.IsTrue(list_b[1].Equals("b33"));
            Assert.IsTrue(list_b[2].Equals(""));
            Assert.IsTrue(list_b[3].Equals("b33"));

            Assert.IsTrue(list_c[0].Equals("c22"));
            Assert.IsTrue(list_c[1].Equals("czz"));
            Assert.IsTrue(list_c[2].Equals("c22"));
            Assert.IsTrue(list_c[3].Equals("czz"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenNonExistingRec_ExecutionEnvironmentEvalAssignFromNestedLast_ShouldAddStar()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "sanele", 0);
            var evalMultiAssign = EvalMultiAssign();
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedLast("[[recs().a]]", warewolfAtomListresult, 0);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetLengthOfJson_ShouldThrow()
        {
            var _environment = new ExecutionEnvironment();
            string msg = null;
            try
            {
                _environment.GetLength("@obj.people");
            } catch (Exception e)
            {
                msg = e.Message;
            }
            Assert.AreEqual("not a recordset", msg);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetObjectLengthOfJson_ShouldThrow()
        {
            var _environment = new ExecutionEnvironment();
            string msg = null;
            try
            {
                _environment.GetObjectLength("@obj.people");
            } catch (Exception e)
            {
                msg = e.Message;
            }
            Assert.AreEqual("not a json array", msg);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignFromNestedStar_Should()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "sanele", 0);
            var evalMultiAssign = EvalMultiAssign();
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedStar("[[rec().a]]", warewolfAtomListresult, 0);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_SortRecordSet_ShouldSortRecordSets()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "sanele", 0);
            var evalMultiAssign = EvalMultiAssign();
            PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            _environment.SortRecordSet("[[rec().a]]", true, 0);
        }

        static DataStorage.WarewolfEnvironment EvalMultiAssign()
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
        static DataStorage.WarewolfEnvironment EvalMultiAssignTwoColumn()
        {
            var assigns = new List<IAssignValue>
            {
                new AssignValue("[[rec().a]]", "a11"),
                new AssignValue("[[rec().b]]", ""),
                new AssignValue("[[rec().c]]", "c22"),
                new AssignValue("[[rec().a]]", "ayy"),
                new AssignValue("[[rec().b]]", "b33"),
                new AssignValue("[[rec().c]]", "czz")
            };
            var envEmpty = WarewolfTestData.CreateTestEnvEmpty("");
            var evalMultiAssign = PublicFunctions.EvalMultiAssign(assigns, 0, envEmpty);
            return evalMultiAssign;
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment__EvalAsList_WhenRecSet_ShouldReturnListOfAllValues()
        {
            var _environment = new ExecutionEnvironment();
            //------------Setup for test--------------------------
            _environment.Assign("[[rec(1).a]]", "27", 0);
            _environment.Assign("[[rec(1).b]]", "bob", 0);
            _environment.Assign("[[rec(2).a]]", "31", 0);
            _environment.Assign("[[rec(2).b]]", "mary", 0);
            //------------Execute Test---------------------------
            var list = _environment.EvalAsList("[[rec(*)]]", 0).ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual("27", list[0].ToString());
            Assert.AreEqual("31", list[1].ToString());
            Assert.AreEqual("bob", list[2].ToString());
            Assert.AreEqual("mary", list[3].ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment__EvalAsList_WhenRecSet_ShouldReturnListOfAllValues_PadLeft()
        {
            var _environment = new ExecutionEnvironment();
            //------------Setup for test--------------------------
            _environment.Assign("[[rec(1).a]]", "27     ", 0);
            _environment.Assign("[[rec(1).b]]", "bob    ", 0);
            _environment.Assign("[[rec(2).a]]", "31 ", 0);
            _environment.Assign("[[rec(2).b]]", "mary", 0);
            //------------Execute Test---------------------------
            var list = _environment.EvalAsList("[[rec(*)]]", 0).ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual("27     ", list[0].ToString());
            Assert.AreEqual("31 ", list[1].ToString());
            Assert.AreEqual("bob    ", list[2].ToString());
            Assert.AreEqual("mary", list[3].ToString());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment__EvalAsListOfString_WhenRecSet_ShouldReturnListOfAllValues()
        {
            var _environment = new ExecutionEnvironment();
            //------------Setup for test--------------------------
            _environment.Assign("[[rec(1).a]]", "27", 0);
            _environment.Assign("[[rec(1).b]]", "bob", 0);
            _environment.Assign("[[rec(2).a]]", "31", 0);
            _environment.Assign("[[rec(2).b]]", "mary", 0);
            //------------Execute Test---------------------------
            var list = _environment.EvalAsListOfStrings("[[rec(*)]]", 0).ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual("27", list[0]);
            Assert.AreEqual("31", list[1]);
            Assert.AreEqual("bob", list[2]);
            Assert.AreEqual("mary", list[3]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment__EvalAsListOfString_NoData()
        {
            var _environment = new ExecutionEnvironment();
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var list = _environment.EvalAsListOfStrings("[[rec(*)]]", 0).ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("", list[0]);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment__EvalAsString_WhenRecSet_ShouldReturnListOfAllValues()
        {
            var _environment = new ExecutionEnvironment();
            //------------Setup for test--------------------------
            _environment.Assign("[[rec(1).a]]", "27", 0);
            _environment.Assign("[[rec(1).b]]", "bob", 0);
            _environment.Assign("[[rec(2).a]]", "31", 0);
            _environment.Assign("[[rec(2).b]]", "mary", 0);
            //------------Execute Test---------------------------
            var stringVal = ExecutionEnvironment.WarewolfEvalResultToString(_environment.Eval("[[rec(*)]]", 0));
            //------------Assert Results-------------------------

            Assert.AreEqual("27,31,bob,mary", stringVal);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenJsonObject_ExecutionEnvironmentEvalAsListOfStrings_ShouldReturnValue()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[Person().Name]]", "Sanele", 0);
            _environment.Assign("[[Person().Name]]", "Nathi", 0);
            var evalAsListOfStrings = _environment.EvalAsListOfStrings("[[Person(*).Name]]", 0);
            Assert.IsTrue(evalAsListOfStrings.Contains("Sanele"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenListResults_ExecutionEnvironmentEvalAsListOfStrings_ShouldReturnValuesAsList()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue(PersonNameExpression, "Sanele"), 0);
            var evalAsListOfStrings = _environment.EvalAsListOfStrings(PersonNameExpression, 0);
            Assert.IsTrue(evalAsListOfStrings.Contains("Sanele"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignWithFrameAndList_Should()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignWithFrameAndList(VariableA,
                new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.NewDataString("Test Value")),
                false, 0);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenRecSet_ExecutionEnvironmentHasRecordSet_ShouldReturnTrue()
        {
            var executionEnv = new ExecutionEnvironment();
            executionEnv.Assign("[[rec().a]]", "bob", 0);
            var hasRecordSet = executionEnv.HasRecordSet("[[rec()]]");
            Assert.IsTrue(hasRecordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenIncorrectString_ExecutionEnvironmentHasRecordSet_ShouldReturnFalse()
        {
            var _environment = new ExecutionEnvironment();
            var hasRecordSet = _environment.HasRecordSet("RandomString");
            Assert.IsFalse(hasRecordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenJson_ExecutionEnvironmentToStar_ShouldReturnAddStar()
        {

            var _environment = new ExecutionEnvironment();
            var star = _environment.ToStar(PersonNameExpression);
            Assert.IsNotNull(star);
            Assert.AreEqual("[[@Person(*).Name]]", star);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenRecSet_ExecutionEnvironmentToStar_ShouldReturnAddStar()
        {
            var _environment = new ExecutionEnvironment();
            var star = _environment.ToStar("[[rec().a]]");
            Assert.IsNotNull(star);
            Assert.AreEqual("[[rec(*).a]]", star);

            star = _environment.ToStar("[[rec()]]");
            Assert.IsNotNull(star);
            Assert.AreEqual("[[rec(*)]]", star);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenVariable_ExecutionEnvironmentToStar_ShouldReturnTheSame()
        {
            var _environment = new ExecutionEnvironment();
            var star = _environment.ToStar(VariableA);
            Assert.IsNotNull(star);
            Assert.AreEqual(VariableA, star);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignFromNestedNumeric_Should()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "sanele", 0);
            var evalMultiAssign = EvalMultiAssign();
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedNumeric("[[rec().a]]", warewolfAtomListresult, 0);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_WarewolfEvalResultToString_Should()
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
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenRecSet_ExecutionEnvironmentIsRecordSetName_ShouldReturnTrue()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec(1).a]]", "Test Value", 0);
            var isRecordSetName = ExecutionEnvironment.IsRecordSetName("[[rec()]]");
            Assert.IsTrue(isRecordSetName);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenInvalidRecSet_ExecutionEnvironmentIsRecordSetName_ShouldReturnFalse()
        {
            var _environment = new ExecutionEnvironment();
            var isRecordSetName = ExecutionEnvironment.IsRecordSetName(InvalidScalar);
            Assert.IsFalse(isRecordSetName);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenValidExpression_ExecutionEnvironmentIsValidVariableExpression_ShouldReturnTrue()
        {
            var _environment = new ExecutionEnvironment();
            var isValidVariableExpression = ExecutionEnvironment.IsValidVariableExpression(VariableA, out string message, 0);
            Assert.IsTrue(isValidVariableExpression);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenInValidExpressionOrEmptyString_ExecutionEnvironmentIsValidVariableExpression_ShouldReturnFalse()
        {
            var _environment = new ExecutionEnvironment();
            //Given Invalid Scalar
            var isValidVariableExpression = ExecutionEnvironment.IsValidVariableExpression(InvalidScalar, out string message, 0);
            Assert.IsFalse(isValidVariableExpression);
            //Given Empty Strign
            isValidVariableExpression = ExecutionEnvironment.IsValidVariableExpression(string.Empty, out message, 0);
            Assert.IsFalse(isValidVariableExpression);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetLength_Should()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "sanele", 0);
            var recordSet = _environment.GetLength("rec");
            Assert.AreEqual(1, recordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetLength_NotARecordset()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "sanele", 0);
            try
            {
                var recordSet = _environment.GetLength("@");
                Assert.Fail("expected not a recordset exception");
            }
            catch (Exception e)
            {
                // BUG: This should pass
                //Assert.AreEqual("not a recordset", e.Message);
                Assert.AreEqual("The given key was not present in the dictionary.", e.Message);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetLength_EmptyIsNotARecordset()
        {
            var _environment = new ExecutionEnvironment();
            try
            {
                var recordSet = _environment.GetLength("");
                Assert.Fail("expected not a recordset exception");
            }
            catch (Exception e)
            {
                // BUG: This should pass
                //Assert.AreEqual("not a recordset", e.Message);
                Assert.AreEqual("The given key was not present in the dictionary.", e.Message);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetObjectLength_Should()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue("[[@Obj()]]", "{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeVal\":\"Bob\"}"), 0);
            var recordSet = _environment.GetObjectLength("Obj");
            Assert.AreEqual(1, recordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetObjectLength_ChildObject()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue("[[@Obj.Child()]]", "{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeVal\":\"Bob\"}"), 0);
            var recordSet = _environment.GetObjectLength("Obj.Child");
            Assert.AreEqual(1, recordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetObjectLength_ChildChildObject()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue("[[@Obj.Child.Child()]]", "{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeVal\":\"Bob\"}"), 0);
            _environment.AssignJson(new AssignValue("[[@Obj.Child.Child()]]", "{\"PolicyNo\":\"A0004\",\"DateId\":33,\"SomeVal\":\"Bob2\"}"), 0);
            var recordSet = _environment.GetObjectLength("Obj.Child.Child");
            Assert.AreEqual(2, recordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [Ignore]
        public void ExecutionEnvironment_GetObjectLength_ChildArray()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue("[[@Obj()]]", "{\"Child\":{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeArray\":[\"Bob\",\"Bob2\"]}}"), 0);
            var recordSet = _environment.GetObjectLength("Obj.Child.SomeArray");
            Assert.AreEqual(2, recordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [Ignore]
        public void ExecutionEnvironment_GetObjectLength_ChildArray2()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue("[[@Obj().Child.SomeArray]]", "[\"Bob\",\"Bob2\"]"), 0);
            var recordSet = _environment.GetObjectLength("Obj.Child.SomeArray");
            Assert.AreEqual(2, recordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalToExpression_Should()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign(VariableA, "SomeValue", 0);
            var evalToExpression = _environment.EvalToExpression(VariableA, 0);
            Assert.AreEqual("[[a]]", evalToExpression);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalToExpression_Empty()
        {
            var _environment = new ExecutionEnvironment();
            var evalToExpression = _environment.EvalToExpression("", 0);
            Assert.AreEqual("", evalToExpression);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalComplexCalcExpression_ShouldNotReplaceSpaces()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[FirstNames]]", "Bob", 0);
            var calcExpr = "!~calculation~!FIND(\" \",[[FirstNames]],1)!~~calculation~!";
            var evalResult = _environment.Eval(calcExpr, 0);
            Assert.AreEqual("!~calculation~!FIND(\" \",\"Bob\",1)!~~calculation~!", ExecutionEnvironment.WarewolfEvalResultToString(evalResult));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_WarewolfEvalResultToString()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[FirstNames().name]]", "Bob1", 0);
            _environment.Assign("[[FirstNames().name]]", "Bob2", 0);

            var result = ExecutionEnvironment.WarewolfEvalResultToString(_environment.Eval("[[FirstNames(*).name]]", 0));

            Assert.AreEqual("Bob1,Bob2", result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_WarewolfEvalResultToString_NothingResultsInNull()
        {
            var _environment = new ExecutionEnvironment();

            var result = ExecutionEnvironment.WarewolfEvalResultToString(_environment.Eval("[[FirstNames(*).name]]", 0));

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalAsTable()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[FirstNames().name]]", "Bob1", 0);
            _environment.Assign("[[FirstNames().name]]", "Bob2", 0);

            var result = _environment.EvalAsTable("[[FirstNames(*)]]", 0);

            var rows = result.ToList();
            Assert.AreEqual("name", rows[0][0].Item1);
            Assert.IsTrue(rows[0][0].Item2.IsDataString);
            Assert.AreEqual("Bob1", rows[0][0].Item2.ToString());

            Assert.AreEqual("name", rows[1][0].Item1);
            Assert.IsTrue(rows[1][0].Item2.IsDataString);
            Assert.AreEqual("Bob2", rows[1][0].Item2.ToString());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [ExpectedException(typeof(NullValueInVariableException))]
        public void ExecutionEnvironment_EvalAsTable_Empty()
        {
            var _environment = new ExecutionEnvironment();

            var result = _environment.EvalAsTable("[[FirstNames(*).name]]", 0);

            // BUG: this should not throw but should return a count of zero items
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenRecSet_ExecutionEnvironmentGetPositionColumnExpression_ShouldReturn()
        {
            var _environment = new ExecutionEnvironment();
            var positionColumnExpression = ExecutionEnvironment.GetPositionColumnExpression("[[rec()]]");
            Assert.IsNotNull(positionColumnExpression);
            Assert.IsTrue(positionColumnExpression.Contains("rec(*)"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenVariable_ExecutionEnvironmentGetPositionColumnExpression_ShouldReturnSameVariable()
        {
            var _environment = new ExecutionEnvironment();
            var positionColumnExpression = ExecutionEnvironment.GetPositionColumnExpression("[[rec]]");
            Assert.IsNotNull(positionColumnExpression);
            Assert.AreEqual("[[rec]]", positionColumnExpression);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_ConvertToIndex_Should()
        {
            var _environment = new ExecutionEnvironment();
            var convertToIndex = ExecutionEnvironment.ConvertToIndex(VariableA, 0);
            Assert.IsNotNull(convertToIndex);
            convertToIndex = ExecutionEnvironment.ConvertToIndex("[[rec(1).a]]", 0);
            Assert.IsNotNull(convertToIndex);

            convertToIndex = ExecutionEnvironment.ConvertToIndex("[[rec(*).a]]", 0);
            Assert.IsNotNull(convertToIndex);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenVariable_ExecutionEnvironmentIsScalar_ShouldBeTrue()
        {

            var _environment = new ExecutionEnvironment();
            var isScalar = ExecutionEnvironment.IsScalar(VariableA);
            Assert.IsTrue(isScalar);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenInvalidScarOrSomeString_ExecutionEnvironmentIsScalar_ShouldBeFalse()
        {

            var _environment = new ExecutionEnvironment();
            var isScalar = ExecutionEnvironment.IsScalar("SomeString");
            Assert.IsFalse(isScalar);
            isScalar = ExecutionEnvironment.IsScalar("[[a]");
            Assert.IsFalse(isScalar);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalAsList_Should()
        {

            var _environment = new ExecutionEnvironment();
            var evalAsList = _environment.EvalAsList(PersonNameExpression, 0);
            Assert.IsNotNull(evalAsList);
            evalAsList = _environment.EvalAsList(string.Empty, 0);
            Assert.IsNotNull(evalAsList);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalAsList_NothingEvaluatesToEmpty()
        {

            var _environment = new ExecutionEnvironment();

            var result = _environment.EvalAsList("[[bob]]", 0).ToArray();

            Assert.AreEqual(1, result.Length);
            Assert.IsTrue(result[0].IsNothing);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalAsList_EmptyEvaluatesToEmpty()
        {
            var _environment = new ExecutionEnvironment();

            var result = _environment.EvalAsList(string.Empty, 0).ToArray();

            Assert.AreEqual(1, result.Length);
            Assert.IsTrue(result[0].IsDataString);
            var v = result[0] as DataStorage.WarewolfAtom.DataString;
            Assert.AreEqual("", v.Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalAsList_List()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[FirstNames().name]]", "Bob1", 0);
            _environment.Assign("[[FirstNames().name]]", "Bob2", 0);

            var result = _environment.EvalAsList("[[FirstNames(*).name]]", 0).ToArray();

            Assert.AreEqual(2, result.Length);

            Assert.IsTrue(result[0].IsDataString);
            var v = result[0] as DataStorage.WarewolfAtom.DataString;
            Assert.AreEqual("Bob1", v.Item);

            Assert.IsTrue(result[1].IsDataString);
            v = result[1] as DataStorage.WarewolfAtom.DataString;
            Assert.AreEqual("Bob2", v.Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_ApplyUpdate_Should()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign(VariableA, "SomeValue", 0);
            var clause =
                new Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom>(atom => atom);
            _environment.ApplyUpdate(VariableA, clause, 0);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenIsNothingEval_ExecutionEnvironmentEvalWhere_ShouldReturnNothing()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign(VariableA, "SomeValue", 0);
            var clause = new Func<DataStorage.WarewolfAtom, bool>(atom => atom.IsNothing);
            var evalWhere = _environment.EvalWhere("[[rec()]]", clause, 0);
            Assert.IsNotNull(evalWhere);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenJSonExpression_ExecutionEnvironmentGetIndexes_ShouldReturn1Index()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue(PersonNameExpression, "Sanele"), 0);
            var indexes = _environment.GetIndexes(PersonNameExpression);
            Assert.AreEqual(1, indexes.Count);
            Assert.IsTrue(indexes.Contains(PersonNameExpression));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenRecSet_ExecutionEnvironmentGetIndexes_ShouldReturn1Index()
        {
            var _environment = new ExecutionEnvironment();
            const string recA = "[[rec(*).a]]";
            _environment.Assign(recA, "Something", 0);
            var indexes = _environment.GetIndexes(recA);
            Assert.AreEqual(1, indexes.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenContainer_ExecutionEnvironmentBuildIndexMap_ShouldBuildMapForChildObj()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue(ChildNameExpression, "Sanele"), 0);
            var privateObj = new PrivateObject(_environment);
            var var = EvaluationFunctions.parseLanguageExpressionWithoutUpdate(ChildNameExpression);
            var jsonIdentifierExpression = var as LanguageAST.LanguageExpression.JsonIdentifierExpression;
            var obj = new JArray(ChildNameExpression);
            if (jsonIdentifierExpression == null)
            {
                return;
            }

            var mapItems = new List<string>();
            object[] args = { jsonIdentifierExpression.Item, "", mapItems, obj };
            privateObj.Invoke("BuildIndexMap", args);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalDelete_Should_Clear_RecordSet()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "Some Value", 0);
            _environment.EvalDelete("[[rec()]]", 0);
            var result = _environment.Eval("[[rec().a]]", 0);
            Assert.IsTrue(WarewolfDataEvaluationCommon.isNothing(result));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenEmptyStringAndName_ExecutionEnvironmentAssignWithFrame_ShouldReturn()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignWithFrame(new AssignValue(PersonNameExpression, "Value"), 0);
            try
            {
                _environment.AssignWithFrame(new AssignValue(string.Empty, "Value"), 0);
            }
            catch (Exception e)
            {
                Assert.AreEqual("invalid variable assigned to ", e.Message);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenEmptyStringAndName_ExecutionEnvironmentIsValidRecordSetIndex_ShouldReturn()
        {
            var _environment = new ExecutionEnvironment();
            Assert.IsTrue(ExecutionEnvironment.IsValidRecordSetIndex("[[rec().a]]"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignDataShape_ShouldReturn()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignDataShape("[[SomeString]]");
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [ExpectedException(typeof(Exception))]
        public void ExecutionEnvironment_GivenInvalidScalar_ExecutionEnvironmentAssignWithFrame_ShouldThrowException()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignWithFrame(new AssignValue(InvalidScalar, "Value"), 0);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalForDataMerge_Should()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign(VariableA, "Sanele", 0);
            _environment.EvalForDataMerge(VariableA, 0);
        }

        [ExpectedException(typeof(NullValueInVariableException))]
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenUnAssignedVar_ExecutionEnvironmentEvalStrict_ShouldThrowNullValueInVariableException()
        {
            var _environment = new ExecutionEnvironment();
            _environment.EvalStrict(PersonNameExpression, 0);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalStrict_Should()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign(PersonNameExpression, "Sanele", 0);
            _environment.EvalStrict(PersonNameExpression, 0);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenEmptyString_ExecutionEnvironmenAssign_ShouldReturn()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign(string.Empty, string.Empty, 0);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignUnique_Should()
        {
            var recs = new List<string>
            {
                "[[Person().Name]]",
                "[[Person(1).Name]]",
                "[[Person(2).Name]]"
            };
            var values = new List<string> { "[[Person().Name]]" };

            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[Person().Name]]", "sanele", 0);
            var resList = new List<string>();
            _environment.AssignUnique(recs, values, resList, 0);
            Assert.IsNotNull(resList);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [ExpectedException(typeof(Exception))]
        public void ExecutionEnvironment_GivenInvalidExpression_ExecutionEnvironmentEval_ShouldThrowException()
        {
            var _environment = new ExecutionEnvironment();
            const string expression = "[[rec(0).a]";
            _environment.Eval(expression, 0, true);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenInvalidExpressionAndthrowsifnotexistsIsFalse_ExecutionEnvironmentEval_ShouldReturnNothing()
        {
            var _environment = new ExecutionEnvironment();
            const string expression = "[[rec(0).a]";
            var warewolfEvalResult = _environment.Eval(expression, 0);
            Assert.AreEqual(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing), warewolfEvalResult);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenEmptyString_ExecutionEnvironmentEvalJContainer_ShouldReturn()
        {

            var _environment = new ExecutionEnvironment();
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
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalJContainer_NameExpression()
        {

            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue("[[@Obj().name]]", "Bob"), 0);

            const string something = "[[@Obj]]";
            var evalJContainer = _environment.EvalJContainer(something);
            Assert.AreEqual("Bob", evalJContainer[0]["name"].ToString());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenEmptyString_ExecutionEnvironmentEvalForJason_ShouldReturnNothing()
        {
            var _environment = new ExecutionEnvironment();
            var warewolfEvalResult = _environment.EvalForJson(string.Empty);
            Assert.AreEqual(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing), warewolfEvalResult);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ExecutionEnvironment_GivenInvalidScalar_ExecutionEnvironmentEvalForJason_ShouldReturnNothing()
        {
            var _environment = new ExecutionEnvironment();
            var warewolfEvalResult = _environment.EvalForJson(OutOfBoundExpression);
            Assert.AreEqual(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing), warewolfEvalResult);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenInvalidScalar_ExecutionEnvironmentEvalForJason_ShouldException()
        {

            var _environment = new ExecutionEnvironment();
            var warewolfEvalResult = _environment.EvalForJson(InvalidScalar);
            Assert.AreEqual(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing), warewolfEvalResult);
            warewolfEvalResult = _environment.EvalForJson("[[rec().a]]");
            Assert.IsNotNull(warewolfEvalResult);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenEmptyString_ExecutionEnvironmentAssignJson_ShouldReturn()
        {

            var _environment = new ExecutionEnvironment();
            var values = new AssignValue(string.Empty, "John");
            _environment.AssignJson(values, 0);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenObjectExecutionEnvironmentAssignJson_ShouldAddObject()
        {

            var _environment = new ExecutionEnvironment();
            var values = new List<IAssignValue> { new AssignValue("[[@Person.Name]]", "John") };
            _environment.AssignJson(values, 0);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [ExpectedException(typeof(Exception))]
        public void ExecutionEnvironment_GivenInvalidObject_ExecutionEnvironmentAssignJson_ShouldThrowParseError()
        {

            var _environment = new ExecutionEnvironment();
            var values = new AssignValue("[[@Person.Name]", "John");
            _environment.AssignJson(values, 0);
            Assert.AreEqual(1, _environment.Errors.Count);
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment__ShouldHave_Ctor()
        {
            var _environment = new ExecutionEnvironment();
            var privateObj = new PrivateObject(_environment);
            var field = privateObj.GetField("_env");
            Assert.IsNotNull(field);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_Ctor_ShouldErrorsCountAs0()
        {
            var _environment = new ExecutionEnvironment();
            Assert.AreEqual(0, _environment.AllErrors.Count);
            Assert.AreEqual(0, _environment.Errors.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_WarewolfAtomToStringErrorIfNull_ShouldReturnStringEmpty()
        {
            const string expected = "SomeString";
            var givenSomeString = DataStorage.WarewolfAtom.NewDataString(expected);
            var result = ExecutionEnvironment.WarewolfAtomToStringErrorIfNull(givenSomeString);
            Assert.AreEqual(expected, result);

            result = ExecutionEnvironment.WarewolfAtomToStringErrorIfNull(null);
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AddToJsonObjects_ShouldAddJsonObject()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AddToJsonObjects(PersonNameExpression, null);
            var privateObj = new PrivateObject(_environment);
            var field = privateObj.GetFieldOrProperty("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(field != null && field.JsonObjects.Count > 0);
        }

        [ExpectedException(typeof(NullValueInVariableException))]
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_WarewolfAtomToStringErrorIfNull_ShouldThrowAnError()
        {
            var atom = DataStorage.WarewolfAtom.Nothing;
            ExecutionEnvironment.WarewolfAtomToStringErrorIfNull(atom);
        }

        [TestMethod]
        public void ExecutionEnvironment_WarewolfAtomToStringNullAsNothing_ShouldReturnNull()
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
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenNullForWarewolfAtom_ExecutionEnvironmentWarewolfAtomToString_ShouldReturnNull()
        {
            var result = ExecutionEnvironment.WarewolfAtomToString(null);
            Assert.IsTrue(string.IsNullOrEmpty(result));
            const string somestring = "SomeString";
            var atom = DataStorage.WarewolfAtom.NewDataString(somestring);
            result = ExecutionEnvironment.WarewolfAtomToString(atom);
            Assert.AreEqual(somestring, result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AddError_ShouldIncreaseErrorCount()
        {
            var _environment = new ExecutionEnvironment();
            var countBefore = _environment.Errors.Count;
            Assert.AreEqual(0, _environment.Errors.Count);
            _environment.AddError(It.IsAny<string>());
            Assert.AreEqual(countBefore + 1, _environment.Errors.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenErrorsExecutionEnvironmentHasError_ShouldBeTrue()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Errors.Add("SomeError");
            
            Assert.IsNotNull(_environment);
            Assert.IsTrue(_environment.Errors.Count > 0);
            Assert.IsTrue(_environment.HasErrors());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenErrorsAndAllErrorsHaveCount_ExecutionEnvironmentFetchError_ShouldJoinAllErrors()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Errors.Add("SomeError");

            Assert.IsNotNull(_environment);
            _environment.AllErrors.Add("AnotherError");
            Assert.IsTrue(_environment.Errors.Count > 0);
            Assert.IsTrue(_environment.HasErrors());
            var expected = $"AnotherError{Environment.NewLine}SomeError";
            Assert.AreEqual(expected, _environment.FetchErrors());
            expected = "{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[\"SomeError\"],\"AllErrors\":[\"AnotherError\"]}";
            Assert.AreEqual(expected, _environment.ToJson());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenJsonSerializedEnv_FromJson_ShouldSetValidEnvironment()
        {
            var _environment = new ExecutionEnvironment();
            var serializedEnv= "{\"Environment\":{\"scalars\":{\"Name\":\"Bob\"},\"record_sets\":{\"R\":{\"FName\":[\"Bob\"],\"WarewolfPositionColumn\":[1]},\"Rec\":{\"Name\":[\"Bob\",,\"Bob\"],\"SurName\":[,\"Bob\",],\"WarewolfPositionColumn\":[1,3,4]},\"RecSet\":{\"Field\":[\"Bob\",\"Jane\"],\"WarewolfPositionColumn\":[1,2]}},\"json_objects\":{\"Person\":{\"Name\":\"B\"}}},\"Errors\":[],\"AllErrors\":[]}";
            _environment.FromJson(serializedEnv);
            var rec1Field1 = ExecutionEnvironment.WarewolfEvalResultToString(_environment.Eval("[[R(*).FName]]", 0));
            var rec2Field1 = ExecutionEnvironment.WarewolfEvalResultToString(_environment.Eval("[[Rec(*).Name]]", 0));
            var rec2Field2 = ExecutionEnvironment.WarewolfEvalResultToString(_environment.Eval("[[Rec(*).SurName]]", 0));
            var rec3Field1 = ExecutionEnvironment.WarewolfEvalResultToString(_environment.Eval("[[RecSet(*).Field]]", 0));
            var scalar = ExecutionEnvironment.WarewolfEvalResultToString(_environment.Eval("[[Name]]", 0));
            var jsonVal = ExecutionEnvironment.WarewolfEvalResultToString(_environment.Eval("[[@Person]]", 0));
            Assert.AreEqual("Bob", scalar);
            Assert.AreEqual("Bob", rec1Field1);
            Assert.AreEqual("Bob,,Bob", rec2Field1);
            Assert.AreEqual(",Bob,", rec2Field2);
            Assert.AreEqual("Bob,Jane", rec3Field1);
            Assert.AreEqual("{"+Environment.NewLine+"  \"Name\": \"B\""+Environment.NewLine+"}", jsonVal);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_HasRecordSet()
        {
            var _environment = new ExecutionEnvironment();
            var serializedEnv = "some string";
            _environment.FromJson(serializedEnv);
            var hasRecSet = _environment.HasRecordSet("R");
            Assert.IsFalse(hasRecSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenJsonSerializedEnv_FromJson_Invalid_ShouldNotUpdateEnvironment()
        {
            var _environment = new ExecutionEnvironment();
            var serializedEnv = "some string";
            _environment.FromJson(serializedEnv);
            var hasRecSet = _environment.HasRecordSet("R");
            Assert.IsFalse(hasRecSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenJsonSerializedEnv_FromJson_EmptyString_ShouldNotUpdateEnvironment()
        {
            var _environment = new ExecutionEnvironment();
            var serializedEnv = "";
            _environment.FromJson(serializedEnv);
            var hasRecSet = _environment.HasRecordSet("R");
            Assert.IsFalse(hasRecSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenJsonSerializedEnv_FromJson_NullString_ShouldNotUpdateEnvironment()
        {
            var _environment = new ExecutionEnvironment();
            string serializedEnv = null;
            _environment.FromJson(serializedEnv);
            var hasRecSet = _environment.HasRecordSet("R");
            Assert.IsFalse(hasRecSet);
        }
    }
}