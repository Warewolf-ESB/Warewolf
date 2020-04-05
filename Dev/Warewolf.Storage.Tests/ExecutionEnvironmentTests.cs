/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Warewolf.Data;
using Warewolf.Resource.Errors;
using WarewolfParserInterop;

namespace Warewolf.Storage.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ExecutionEnvironmentTest
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ExecutionEnvironment_Eval_GivenInvalidIndex_Throws_IndexOutOfRangeException()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Eval("[[rec(0).a]]", 0, true);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_Constructor_ShouldSet_Id()
        {
            var _environment = new ExecutionEnvironment();
            
            Assert.IsNotNull(_environment.Id);
            Assert.AreNotEqual(Guid.Empty, _environment.Id);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_Constructor_ShouldNotSet_ParentId()
        {
            var _environment = new ExecutionEnvironment();

            Assert.AreEqual(Guid.Empty, _environment.ParentId);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_Assign_RecsetField()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "bob", 0);
            var recordSet = _environment.GetCount("rec");
            Assert.AreEqual(1, recordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_Snapshot()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "bob", 0);

            var originalEnvironment = _environment.Snapshot();

            originalEnvironment.Assign("[[rec().a]]", "ralph", 0);
            Assert.AreEqual("bob", _environment.EvalAsListOfStrings("[[rec().a]]", 0)[0]);

            Assert.AreNotEqual(_environment, originalEnvironment);

            var recordSet = _environment.GetCount("rec");
            var originalEnvironmentResult = originalEnvironment.GetCount("rec");
            Assert.AreEqual(1, recordSet);
            Assert.AreEqual(2, originalEnvironmentResult);

            Assert.AreNotEqual(Guid.Empty, originalEnvironment.Id);
            Assert.AreNotEqual(Guid.Empty, originalEnvironment.ParentId);
            Assert.AreEqual(_environment.Id, originalEnvironment.ParentId);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_Assign_NullExpression_DoesNotThrow()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign(null, "bob", 0);

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
            _environment.AssignStrict("[[rec().a]]", "bob", 0);
            var recordSet = _environment.GetCount("rec");
            Assert.AreEqual(1, recordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignStrict_NullExpression_DoesNotThrow()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignStrict(null, "bob", 0);

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
        public void ExecutionEnvironment_EvalAssignFromNestedLast_GivenRecSet_Should()
        {
            var _environment = new ExecutionEnvironment();
            var evalMultiAssign = EvalMultiAssign();
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;

            Assert.AreEqual(1, _environment.EvalAsListOfStrings("[[rec(*).a]]", 0).Count);

            _environment.EvalAssignFromNestedLast("[[rec(*).a]]", warewolfAtomListresult, 0);

            var result = _environment.EvalAsList("[[rec(*).a]]", 0).ToArray();

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual(27, (result[0] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(25, (result[1] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(33, (result[2] as DataStorage.WarewolfAtom.Int).Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalAssignFromNestedLast_GivenRecSet_TwoColumn_Should()
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
        public void ExecutionEnvironment_EvalAssignFromNestedLast_GivenNonExistingRec_ShouldAddStar()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "Bob", 0);
            var evalMultiAssign = EvalMultiAssign();
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;

            Assert.AreEqual("Bob", _environment.EvalAsListOfStrings("[[rec(*).a]]", 0)[0]);

            _environment.EvalAssignFromNestedLast("[[recs().a]]", warewolfAtomListresult, 0);


            var result = _environment.EvalAsList("[[recs(*).a]]", 0).ToArray();

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual(27, (result[0] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(25, (result[1] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(33, (result[2] as DataStorage.WarewolfAtom.Int).Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetLength_GivenJson_ShouldThrow()
        {
            var _environment = new ExecutionEnvironment();

            try
            {
                _environment.GetLength("@obj.people");

                Assert.Fail("expected exception, not a recordset");
            }
            catch (Exception e)
            {
                Assert.AreEqual("not a recordset", e.Message);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetObjectLength_GivenJson_ShouldThrow()
        {
            var _environment = new ExecutionEnvironment();

            try
            {
                _environment.GetObjectLength("@obj.people");
                Assert.Fail("expected exception, not a json array");
            }
            catch (Exception e)
            {
                Assert.AreEqual("not a json array", e.Message);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignFromNestedStar_Should()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "Bob", 0);
            var evalMultiAssign = EvalMultiAssign();
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedStar("[[rec().a]]", warewolfAtomListresult, 0);

            var result = _environment.EvalAsList("[[rec(*).a]]", 0).ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("Bob", (result[0] as DataStorage.WarewolfAtom.DataString).Item);
            Assert.AreEqual(27, (result[1] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(25, (result[2] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(33, (result[3] as DataStorage.WarewolfAtom.Int).Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_SortRecordSet_ShouldSortRecordSet()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "Bob", 0);
            _environment.Assign("[[rec().a]]", "1Bob", 0);
            _environment.Assign("[[rec().a]]", "bob", 0);
            var evalMultiAssign = EvalMultiAssign();
            PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedStar("[[rec().a]]", warewolfAtomListresult, 0);

            _environment.SortRecordSet("[[rec().a]]", false, 0);

            var result = _environment.EvalAsList("[[rec(*).a]]", 0).ToArray();

            Assert.AreEqual(6, result.Length);
            Assert.AreEqual("1Bob", (result[0] as DataStorage.WarewolfAtom.DataString).Item);
            Assert.AreEqual(25, (result[1] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(27, (result[2] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(33, (result[3] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual("bob", (result[4] as DataStorage.WarewolfAtom.DataString).Item);
            Assert.AreEqual("Bob", (result[5] as DataStorage.WarewolfAtom.DataString).Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_SortRecordSet_Desc_ShouldSortRecordSet_InReverse()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "Bob", 0);
            _environment.Assign("[[rec().a]]", "1Bob", 0);
            _environment.Assign("[[rec().a]]", "bob", 0);
            var evalMultiAssign = EvalMultiAssign();
            PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            _environment.EvalAssignFromNestedStar("[[rec().a]]", warewolfAtomListresult, 0);

            _environment.SortRecordSet("[[rec().a]]", true, 0);

            var result = _environment.EvalAsList("[[rec(*).a]]", 0).ToArray();

            Assert.AreEqual(6, result.Length);
            Assert.AreEqual("Bob", (result[0] as DataStorage.WarewolfAtom.DataString).Item);
            Assert.AreEqual("bob", (result[1] as DataStorage.WarewolfAtom.DataString).Item);
            Assert.AreEqual(33, (result[2] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(27, (result[3] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(25, (result[4] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual("1Bob", (result[5] as DataStorage.WarewolfAtom.DataString).Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment_EvalAsList_WhenRecSet_ListsAllCells_ColumnByColumn()
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
            Assert.AreEqual(27, (list[0] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(31, (list[1] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual("bob", (list[2] as DataStorage.WarewolfAtom.DataString).Item);
            Assert.AreEqual("mary", (list[3] as DataStorage.WarewolfAtom.DataString).Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment_EvalAsList_NonExistentRecordset_Returns1NothingItem()
        {
            var _environment = new ExecutionEnvironment();

            var list = _environment.EvalAsList("[[rec(*)]]", 0).ToArray();

            Assert.AreEqual(1, list.Length);
            Assert.IsTrue(list[0].IsNothing);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment_EvalAsList_Scalar()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[a]]", "1234", 0);

            var list = _environment.EvalAsList("[[a]]", 0).ToArray();

            Assert.AreEqual(1, list.Length);
            Assert.AreEqual(1234, (list[0] as DataStorage.WarewolfAtom.Int).Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment_EvalAsList_Scalar_NotExists_Returns1NothingItem()
        {
            var _environment = new ExecutionEnvironment();

            var list = _environment.EvalAsList("[[a]]", 0).ToArray();

            Assert.AreEqual(1, list.Length);
            Assert.IsTrue(list[0].IsNothing);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment_EvalAsList_JsonObject()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue("[[@Obj]]", "{\"Name\":\"Bob\"}"), 0);

            var list = _environment.EvalAsList("[[@Obj]]", 0).ToArray();

            Assert.AreEqual(1, list.Length);
            if (list[0] is DataStorage.WarewolfAtom.JsonObject firstItem)
            {
                Assert.AreEqual("Bob", firstItem.Item["Name"]);
            }
            else
            {
                Assert.Fail("expected JsonObject");
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment_EvalAsList_JsonObject_NotExists_Returns1NothingItem()
        {
            var _environment = new ExecutionEnvironment();

            var list = _environment.EvalAsList("[[@Obj]]", 0).ToArray();

            Assert.AreEqual(1, list.Length);
            Assert.IsTrue(list[0].IsNothing);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment_EvalAsList_WhenRecSet_PadLeft_IsNotStripped()
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
            Assert.AreEqual("27     ", (list[0] as DataStorage.WarewolfAtom.DataString).Item);
            Assert.AreEqual("31 ", (list[1] as DataStorage.WarewolfAtom.DataString).Item);
            Assert.AreEqual("bob    ", (list[2] as DataStorage.WarewolfAtom.DataString).Item);
            Assert.AreEqual("mary", (list[3] as DataStorage.WarewolfAtom.DataString).Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment_EvalAsList_WhenRecSet_Padding_IsNotStripped()
        {
            var _environment = new ExecutionEnvironment();
            //------------Setup for test--------------------------
            _environment.Assign("[[rec(1).a]]", " 27     ", 0);
            _environment.Assign("[[rec(1).b]]", "  bob    ", 0);
            _environment.Assign("[[rec(2).a]]", "\t31 ", 0);
            _environment.Assign("[[rec(2).b]]", "mary\t", 0);
            //------------Execute Test---------------------------
            var list = _environment.EvalAsList("[[rec(*)]]", 0).ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(" 27     ", (list[0] as DataStorage.WarewolfAtom.DataString).Item);
            Assert.AreEqual("\t31 ", (list[1] as DataStorage.WarewolfAtom.DataString).Item);
            Assert.AreEqual("  bob    ", (list[2] as DataStorage.WarewolfAtom.DataString).Item);
            Assert.AreEqual("mary\t", (list[3] as DataStorage.WarewolfAtom.DataString).Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [TestCategory("ExecutionEnvironment_EvalAsList")]
        public void ExecutionEnvironment_EvalAsListOfString_WhenRecSet_ShouldReturnListOfAllValues()
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
        public void ExecutionEnvironment_EvalAsListOfString_NoData()
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
        public void ExecutionEnvironment_EvalAsString_WhenRecSet_ShouldReturnListOfAllValues()
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
        public void ExecutionEnvironment_EvalAsListOfStrings_SameValues_ShouldReturn()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[Person().Name]]", "Bob", 0);
            _environment.Assign("[[Person().Name]]", "Bob", 0);

            var evalAsListOfStrings = _environment.EvalAsListOfStrings("[[Person(*).Name]]", 0);

            Assert.AreEqual(2, evalAsListOfStrings.Count);
            Assert.AreEqual("Bob", evalAsListOfStrings[0]);
            Assert.AreEqual("Bob", evalAsListOfStrings[1]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalAsListOfStrings_ListOfNothing_DoesNotThrow()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignDataShape("[[Person().Name]]");

            var evalAsListOfStrings = _environment.EvalAsListOfStrings("[[Person(*)]]", 0);

            Assert.AreEqual(0, evalAsListOfStrings.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalAsListOfStrings_GivenListResults_ShouldReturnValuesAsList()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue("[[@Person().Name]]", "Bob"), 0);

            var evalAsListOfStrings = _environment.EvalAsListOfStrings("[[@Person().Name]]", 0);

            Assert.AreEqual(1, evalAsListOfStrings.Count);
            Assert.AreEqual("Bob", evalAsListOfStrings[0]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_HasRecordSet_GivenRecSet_ShouldReturnTrue()
        {
            var executionEnv = new ExecutionEnvironment();
            executionEnv.Assign("[[rec().a]]", "bob", 0);
            var hasRecordSet = executionEnv.HasRecordSet("[[rec()]]");
            Assert.IsTrue(hasRecordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_HasRecordSet_ShouldReturnFalse()
        {
            var _environment = new ExecutionEnvironment();
            var hasRecordSet = _environment.HasRecordSet("[[rec()]]");
            Assert.IsFalse(hasRecordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_HasRecordSet_GivenIncorrectString_ShouldReturnFalse()
        {
            var _environment = new ExecutionEnvironment();
            var hasRecordSet = _environment.HasRecordSet("RandomString");
            Assert.IsFalse(hasRecordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_ToStar_GivenJson_ShouldReturnAddStar()
        {

            var _environment = new ExecutionEnvironment();
            var star = _environment.ToStar("[[@Person().Name]]");

            Assert.AreEqual("[[@Person(*).Name]]", star);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_ToStar_GivenRecSet_ShouldReturnAddStar()
        {
            var _environment = new ExecutionEnvironment();
            var star = _environment.ToStar("[[rec().a]]");

            Assert.AreEqual("[[rec(*).a]]", star);

            star = _environment.ToStar("[[rec()]]");
            Assert.AreEqual("[[rec(*)]]", star);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_ToStar_GivenVariable_ShouldReturnTheSame()
        {
            var _environment = new ExecutionEnvironment();
            var star = _environment.ToStar("[[a]]");

            Assert.AreEqual("[[a]]", star);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignFromNestedNumeric_ShouldAppendLast()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "Bob", 0);
            var evalMultiAssign = EvalMultiAssign();
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;

            _environment.EvalAssignFromNestedNumeric("[[rec().a]]", warewolfAtomListresult, 0);

            var result = _environment.EvalAsList("[[rec(*)]]", 0).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("Bob", (result[0] as DataStorage.WarewolfAtom.DataString).Item);
            Assert.AreEqual(33, (result[1] as DataStorage.WarewolfAtom.Int).Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignFromNestedNumeric_ToStar_Should_ReplaceItemsWithLast()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "Bob", 0);
            _environment.Assign("[[rec().a]]", "Bob2", 0);
            var evalMultiAssign = EvalMultiAssign();
            var items = PublicFunctions.EvalEnvExpression("[[rec(*).a]]", 0, false, evalMultiAssign);
            var warewolfAtomListresult = items as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;

            _environment.EvalAssignFromNestedNumeric("[[rec(*).a]]", warewolfAtomListresult, 0);

            var result = _environment.EvalAsList("[[rec(*)]]", 0).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(33, (result[0] as DataStorage.WarewolfAtom.Int).Item);
            Assert.AreEqual(33, (result[1] as DataStorage.WarewolfAtom.Int).Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsRecordSetName_GivenRecSet_ShouldReturnTrue()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec(1).a]]", "Test Value", 0);
            var isRecordSetName = ExecutionEnvironment.IsRecordSetName("[[rec()]]");
            Assert.IsTrue(isRecordSetName);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsRecordSetName_GivenRecSetExists_UseNameIncorrectly_ShouldReturnFalse()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec(1).a]]", "Test Value", 0);
            var isRecordSetName = ExecutionEnvironment.IsRecordSetName("[[rec]]");
            Assert.IsFalse(isRecordSetName);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsRecordSetName_GivenInvalidRecSet_ShouldReturnFalse()
        {
            var isRecordSetName = ExecutionEnvironment.IsRecordSetName("[[rec(0).a]]");
            Assert.IsFalse(isRecordSetName);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsRecordSetName_GivenScalar_ShouldReturnFalse()
        {
            var isRecordSetName = ExecutionEnvironment.IsRecordSetName("[[a]]");
            Assert.IsFalse(isRecordSetName);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsValidVariableExpression_GivenValidExpression_ShouldReturnTrue()
        {
            var result = ExecutionEnvironment.IsValidVariableExpression("[[a]]", out string message, 0);
            Assert.IsTrue(result);
            Assert.AreEqual("", message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsValidVariableExpression_GivenInValidExpression_ShouldReturnFalse()
        {
            var result = ExecutionEnvironment.IsValidVariableExpression("[[rec(0).a]", out string message, 0);
            Assert.IsFalse(result);
            Assert.AreEqual("parse error", message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsValidVariableExpression_GivenInValidExpression2_ShouldReturnFalse()
        {
            var result = ExecutionEnvironment.IsValidVariableExpression("@", out string message, 0);
            Assert.IsFalse(result);
            // TODO: shouldn't this return parse error?
            Assert.AreEqual("", message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsValidVariableExpression_GivenEmptyString_ShouldReturnFalse()
        {
            var result = ExecutionEnvironment.IsValidVariableExpression(string.Empty, out string message, 0);
            Assert.IsFalse(result);
            // TODO: should this really have no error message?
            Assert.AreEqual("", message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetLength_Should()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "bob", 0);
            _environment.Assign("[[rec().a]]", "bob", 0);
            var recordSet = _environment.GetLength("rec");
            Assert.AreEqual(2, recordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetLength_NotARecordset()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "bob", 0);
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
        public void ExecutionEnvironment_GetObjectLength_GivenChildObject()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue("[[@Obj]]", "{\"Child\":{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeArray\":[\"Bob\",\"Bob2\"]}}"), 0);
            var recordSet = _environment.GetObjectLength("Obj.Child");
            Assert.AreEqual(3, recordSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetObjectLength_GivenChildArray()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue("[[@Obj]]", "{\"Child\":{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeArray\":[\"Bob\",\"Bob2\"]}}"), 0);
            var grandChildCount = _environment.GetObjectLength("Obj.Child.SomeArray");
            Assert.AreEqual(2, grandChildCount);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [Ignore]
        public void ExecutionEnvironment_GetObjectLength_ChildArray2()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue("[[@Obj.Child.SomeArray]]", "[\"Bob\",\"Bob2\"]"), 0);
            var grandChildCount = _environment.GetObjectLength("Obj.Child.SomeArray");
            Assert.AreEqual(2, grandChildCount);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalToExpression_ShouldEvaluateToBaseExpression()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[a]]", "SomeValue", 0);
            _environment.Assign("[[b]]", "[[a]]", 0);

            var evalToExpression = _environment.EvalToExpression("[[[[b]]]]", 0);

            Assert.AreEqual("[[SomeValue]]", evalToExpression);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalToExpression_Should()
        {
            var _environment = new ExecutionEnvironment();

            _environment.Assign("[[a]]", "SomeValue", 0);

            var evalToExpression = _environment.EvalToExpression("[[a]]", 0);
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
        public void ExecutionEnvironment_WarewolfEvalResultToString_EnsureNoNewResultTypes()
        {
            /* This test ensures that if we add a new result type we will make sure that we alter WarewolfEvalResultToString */
            var members = typeof(CommonFunctions.WarewolfEvalResult).FindMembers(System.Reflection.MemberTypes.NestedType, System.Reflection.BindingFlags.Public, (m, s) => true, null).ToArray();

            Assert.AreEqual(4, members.Length);
            Assert.AreEqual("Tags", members[0].Name);
            Assert.AreEqual("WarewolfAtomResult", members[1].Name);
            Assert.AreEqual("WarewolfAtomListresult", members[2].Name);
            Assert.AreEqual("WarewolfRecordSetResult", members[3].Name);
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
        public void ExecutionEnvironment_WarewolfEvalResultToString_GivenWarewolfAtomList_DataStringIsUnaltered()
        {
            var warewolfEvalResultToString = ExecutionEnvironment.WarewolfEvalResultToString(
                CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(
                    new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.NewDataString("Test string"))));
            Assert.IsNotNull(warewolfEvalResultToString);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_WarewolfEvalResultToString_GivenWarewolfAtom_DataStringIsUnaltered()
        {
            var warewolfEvalResultToString = ExecutionEnvironment.WarewolfEvalResultToString(
                CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(
                    DataStorage.WarewolfAtom.NewDataString("Test string")));
            Assert.AreEqual("Test string", warewolfEvalResultToString);
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

            // BUG: this should not throw, instead it should return a count of zero items or a single cell with Nothing in it
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetPositionColumnExpression_GivenRecsetNameExpression()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().name]]", "Bob", 0);

            var positionColumnExpression = ExecutionEnvironment.GetPositionColumnExpression("[[rec()]]");

            Assert.AreEqual("[[rec(*).WarewolfPositionColumn]]", positionColumnExpression);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetPositionColumnExpression_GivenScalar()
        {
            var positionColumnExpression = ExecutionEnvironment.GetPositionColumnExpression("[[a]]");

            Assert.AreEqual("[[a]]", positionColumnExpression);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetPositionColumnExpression_GivenRecSetNameExpression_NotExists()
        {
            var positionColumnExpression = ExecutionEnvironment.GetPositionColumnExpression("[[rec()]]");

            Assert.AreEqual("[[rec(*).WarewolfPositionColumn]]", positionColumnExpression);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetPositionColumnExpression_RecordSetExpression()
        {
            var positionColumnExpression = ExecutionEnvironment.GetPositionColumnExpression("[[rec().N]]");

            Assert.AreEqual("[[rec(*).WarewolfPositionColumn]]", positionColumnExpression);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_ConvertToIndex()
        {
            var result = ExecutionEnvironment.ConvertToIndex("[[a]]", 0);
            Assert.AreEqual("[[a]]", result);

            result = ExecutionEnvironment.ConvertToIndex("[[rec(1).a]]", 0);
            Assert.AreEqual("[[rec(1).a]]", result);

            result = ExecutionEnvironment.ConvertToIndex("[[rec(*).a]]", 0);
            // TODO: this doesn't seem right, we start indexing at 1
            Assert.AreEqual("[[rec(0).a]]", result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_ConvertToIndex_LastNotAltered()
        {
            var result = ExecutionEnvironment.ConvertToIndex("[[rec().a]]", 0);
            Assert.AreEqual("[[rec().a]]", result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsScalar_GivenVariable_ShouldBeTrue()
        {
            var isScalar = ExecutionEnvironment.IsScalar("[[a]]");
            Assert.IsTrue(isScalar);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsScalar_GivenSomeString_ShouldBeFalse()
        {
            var isScalar = ExecutionEnvironment.IsScalar("SomeString");
            Assert.IsFalse(isScalar);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsScalar_GivenInvalid_ShouldNotThrow()
        {
            var isScalar = ExecutionEnvironment.IsScalar("[[a]");
            Assert.IsFalse(isScalar);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsScalar_GivenRecset_ShouldBeFalse()
        {
            var isScalar = ExecutionEnvironment.IsScalar("[[rec().a]]");
            Assert.IsFalse(isScalar);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalAsList_RecordsetNotExists_Returns1NothingCell()
        {
            var _environment = new ExecutionEnvironment();
            var result = _environment.EvalAsList("[[@Person().Name]]", 0).ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.IsTrue(result[0].IsNothing);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalAsList_GivenEmptyString_Returns1DataStringCell()
        {
            var _environment = new ExecutionEnvironment();
            var result = _environment.EvalAsList(string.Empty, 0).ToArray();

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("", (result[0] as DataStorage.WarewolfAtom.DataString).Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalAsList_ScalarNotExists_Returns1NothingCell()
        {
            var _environment = new ExecutionEnvironment();

            var result = _environment.EvalAsList("[[bob]]", 0).ToArray();

            Assert.AreEqual(1, result.Length);
            Assert.IsTrue(result[0].IsNothing);
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
        public void ExecutionEnvironment_ApplyUpdate_ShouldAlterDataInPlace()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[a]]", "SomeValue", 0);
            var clause =
                new Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom>(atom => DataStorage.WarewolfAtom.NewDataString("before" + atom.ToString() + "after"));
            _environment.ApplyUpdate("[[a]]", clause, 0);

            var result = _environment.EvalAsListOfStrings("[[a]]", 0);
            Assert.AreEqual("beforeSomeValueafter", result[0]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_ApplyUpdate_ShouldAlterDataInPlace_Recset()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec(1).a]]", "SomeValue", 0);
            _environment.Assign("[[rec(2).a]]", "1234", 0);
            _environment.Assign("[[rec(3).a]]", "2.4", 0);
            var clause =
                new Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom>(atom => DataStorage.WarewolfAtom.NewDataString("before" + atom.ToString() + "after"));
            _environment.ApplyUpdate("[[rec(*)]]", clause, 0);

            var result = _environment.EvalAsListOfStrings("[[rec(*)]]", 0);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("beforeSomeValueafter", result[0]);
            Assert.AreEqual("before1234after", result[1]);
            Assert.AreEqual("before2.4after", result[2]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ExecutionEnvironment_ApplyUpdate_RecsetNotExists()
        {
            var _environment = new ExecutionEnvironment();

            var clause = new Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom>(
                atom => DataStorage.WarewolfAtom.NewDataString("before" + atom.ToString() + "after")
            );
            _environment.ApplyUpdate("[[rec(*)]]", clause, 0);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_ApplyUpdate_ScalarNotExists()
        {
            var _environment = new ExecutionEnvironment();
            var clause = new Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom>(
                atom => DataStorage.WarewolfAtom.NewDataString("before" + atom.ToString() + "after")
            );
            try
            {
                _environment.ApplyUpdate("[[rec]]", clause, 0);
                Assert.Fail("expected exception Scalar value { rec } is NULL");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Scalar value { rec } is NULL", e.Message);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalWhere_GivenIsNothingEval_ShouldReturnNothing()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[a]]", "SomeValue", 0);
            var clause = new Func<DataStorage.WarewolfAtom, bool>(atom => atom.IsNothing);

            var evalWhere = _environment.EvalWhere("[[rec()]]", clause, 0);

            Assert.AreEqual(0, evalWhere.ToArray().Length);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetIndexes_GivenJSonExpression_ShouldReturn1Index()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue("[[@Person().Name]]", "Bob"), 0);

            var indexes = _environment.GetIndexes("[[@Person().Name]]").ToArray();

            Assert.AreEqual(1, indexes.Length);
            Assert.AreEqual("[[@Person().Name]]", indexes[0]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetIndexes_Scalar_HasNoIndexes()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[a]]", "Bob", 0);
            var indexes = _environment.GetIndexes("[[a]]");
            Assert.AreEqual(0, indexes.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GetIndexes_GivenRecSet_ShouldReturn1Index()
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
        public void ExecutionEnvironment_EvalDelete_Should_Clear_RecordSet()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "Some Value", 0);
            Assert.AreEqual(1, _environment.EvalAsListOfStrings("[[rec(*).a]]", 0).ToArray().Length);

            _environment.EvalDelete("[[rec()]]", 0);

            var result = _environment.Eval("[[rec().a]]", 0);
            Assert.IsTrue(WarewolfDataEvaluationCommon.isNothing(result));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignWithFrame_GivenEmptyString_ThrowsWithMessage()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignWithFrame(new AssignValue("[[@Person().Name]]", "Value"), 0);
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
        public void ExecutionEnvironment_AssignWithFrame_GivenInvalidScalar_ShouldThrowException()
        {
            var _environment = new ExecutionEnvironment();
            try
            {
                _environment.AssignWithFrame(new AssignValue("[[rec(0).a]", "Value"), 0);
                Assert.IsTrue(_environment.HasErrors());
                Assert.AreEqual("parse error", _environment.FetchErrors());

            }
            catch (Exception e)
            {
                Assert.AreEqual("parse error", e.Message);
            }
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

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("n1", result[0]);
            Assert.AreEqual("n2", result[1]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsValidRecordSetIndex_Last()
        {
            Assert.IsTrue(ExecutionEnvironment.IsValidRecordSetIndex("[[rec().a]]"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsValidRecordSetIndex_InvalidString()
        {
            // TODO: shouldn't this just return false to keep similarity with IsScalar?
            try
            {
                Assert.IsFalse(ExecutionEnvironment.IsValidRecordSetIndex("[[a]"));
                Assert.Fail("expected exception, parse error");
            }
            catch (Exception e)
            {
                Assert.AreEqual("parse error", e.Message);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsValidRecordSetIndex_EmptyString()
        {
            // BUG: this should pass?
            //Assert.IsFalse(ExecutionEnvironment.IsValidRecordSetIndex(""));
            Assert.IsTrue(ExecutionEnvironment.IsValidRecordSetIndex(""));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsValidRecordSetIndex_Number()
        {
            Assert.IsTrue(ExecutionEnvironment.IsValidRecordSetIndex("[[rec(1).b]]"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_IsValidRecordSetIndex_Star()
        {
            Assert.IsTrue(ExecutionEnvironment.IsValidRecordSetIndex("[[rec(*).b]]"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignDataShape_Scalar()
        {
            var _environment = new ExecutionEnvironment();

            _environment.AssignDataShape("[[SomeString]]");

            var value = (_environment.Eval("[[SomeString]]", 0) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult).Item;
            Assert.IsTrue(value.IsNothing);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignDataShape_Recset()
        {
            var _environment = new ExecutionEnvironment();

            _environment.AssignDataShape("[[rec().a]]");

            var value = (_environment.Eval("[[rec().a]]", 0) as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult).Item;
            Assert.AreEqual(0, value.Count);

            var values = _environment.EvalAsList("[[rec(*)]]", 0).ToArray();
            Assert.AreEqual(0, values.Length);

            var stringValues = _environment.EvalAsListOfStrings("[[rec(*)]]", 0).ToArray();
            Assert.AreEqual(0, stringValues.Length);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalForDataMerge_ShouldReturnWarewolfEvalResult()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[a]]", "bob", 0);

            var result = _environment.EvalForDataMerge("[[a]]", 0).ToArray();

            Assert.AreEqual(1, result.Length);
            var resultItem = (result[0] as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult);
            Assert.IsNotNull(resultItem);
            Assert.AreEqual("bob", (resultItem.Item as DataStorage.WarewolfAtom.DataString).Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalStrict_GivenUnAssignedVar_ShouldThrowNullValueInVariableException()
        {
            var _environment = new ExecutionEnvironment();

            try
            {
                var result = _environment.EvalStrict("[[@Person().Name]]", 0);
                Assert.Fail("expected no value assigned exception");
            }
            catch (Exception e)
            {
                Assert.AreEqual("The expression [[@Person().Name]] has no value assigned.", e.Message);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalStrict_Should()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[@Person().Name]]", "Bob", 0);
            var result = _environment.EvalStrict("[[@Person().Name]]", 0);

            Assert.IsTrue(result.IsWarewolfAtomResult);
            var resultItem = (result as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult).Item;

            Assert.IsNotNull(resultItem);
            if (resultItem is DataStorage.WarewolfAtom.JsonObject firstItem)
            {
                Assert.AreEqual("Bob", firstItem.Item);
            }
            else
            {
                Assert.Fail("expected JsonObject");
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_Assign_GivenEmptyString_ShouldReturnWithoutAlteringEnvironment()
        {
            var _environment = new ExecutionEnvironment();
            var jsonBefore = _environment.ToJson();

            _environment.Assign(string.Empty, string.Empty, 0);

            var jsonAfter = _environment.ToJson();
            Assert.AreEqual(jsonBefore, jsonAfter);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignUnique_Distinct()
        {
            var cols = new List<string>
            {
                "[[rec().Name]]"
            };
            var distinctCols = new List<string> { "[[rec().Name]]" };

            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().Name]]", "bob", 0);
            _environment.Assign("[[rec().Name]]", "bob", 0);
            _environment.Assign("[[rec().Name]]", "bob1", 0);
            _environment.Assign("[[rec().Name]]", "bob", 0);

            var resultVariable = new List<string> { "[[rec1(*).Name]]" };
            _environment.AssignUnique(cols, distinctCols, resultVariable, 0);

            var result = _environment.EvalAsListOfStrings("[[rec1(*)]]", 0).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("bob", result[0]);
            Assert.AreEqual("bob1", result[1]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignUnique_TwoColumnRecsetOneDistinct()
        {
            var cols = new List<string>
            {
                "[[rec().Name]]"
            };
            var distinctCols = new List<string> { "[[rec().Name]]" };

            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().Name]]", "bob", 0);
            _environment.Assign("[[rec().Surname]]", "sbob", 0);
            _environment.Assign("[[rec().Name]]", "bob", 0);
            _environment.Assign("[[rec().Surname]]", "sbob", 0);
            _environment.Assign("[[rec().Name]]", "bob1", 0);
            _environment.Assign("[[rec().Surname]]", "sbob", 0);
            _environment.Assign("[[rec().Name]]", "bob", 0);
            _environment.Assign("[[rec().Surname]]", "sbob", 0);

            var resultVariable = new List<string> { "[[rec1(*).Name]]" };
            _environment.AssignUnique(cols, distinctCols, resultVariable, 0);

            var result = _environment.EvalAsListOfStrings("[[rec1(*)]]", 0).ToArray();

            // Bug: Is this right?
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("bob", result[0]);
            Assert.AreEqual("bob1", result[1]);
            Assert.AreEqual("", result[2]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [ExpectedException(typeof(Exception))]
        public void ExecutionEnvironment_Eval_GivenInvalidExpression_ShouldThrowException()
        {
            var _environment = new ExecutionEnvironment();
            const string expression = "[[rec(0).a]";
            _environment.Eval(expression, 0, true);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_Eval_GivenInvalidExpressionAndthrowsifnotexistsIsFalse_ShouldReturnNothing()
        {
            var _environment = new ExecutionEnvironment();
            const string expression = "[[rec(0).a]";

            var warewolfEvalResult = _environment.Eval(expression, 0);

            Assert.AreEqual(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing), warewolfEvalResult);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalJContainer_GivenEmptyString_ShouldReturn()
        {

            var _environment = new ExecutionEnvironment();
            var evalJContainer = _environment.EvalJContainer(string.Empty);

            Assert.IsNull(evalJContainer);

            const string something = "new {string valu3};";
            evalJContainer = _environment.EvalJContainer(something);

            Assert.IsNull(evalJContainer);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalJContainer_GivenObjectWithOneField_GetField()
        {
            var _environment = new ExecutionEnvironment();
            _environment.AssignJson(new AssignValue("[[@Person().Name]]", "Bob"), 0);

            var evalJContainer = _environment.EvalJContainer("[[@Person().Name]]");

            Assert.AreEqual(1, evalJContainer.Count);
            Assert.AreEqual("Bob", (evalJContainer[0]["Name"]).Value<string>());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalJContainer_NameExpression_GetObject()
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
        public void ExecutionEnvironment_EvalJContainer_NonJson_DoesNotThrow()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[a]]", "Bob", 0);

            var evalJContainer = _environment.EvalJContainer("[[a]]");

            Assert.IsNull(evalJContainer);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalForJson_GivenEmptyString_ShouldReturnNothing()
        {
            var _environment = new ExecutionEnvironment();
            var warewolfEvalResult = _environment.EvalForJson(string.Empty);

            Assert.AreEqual(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing), warewolfEvalResult);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalForJson_GivenInvalidScalar_ShouldReturnNothing()
        {
            var _environment = new ExecutionEnvironment();
            var warewolfEvalResult = _environment.EvalForJson("[[rec(1).a]]");

            var result = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;

            Assert.AreEqual(1, result.Item.Count);
            Assert.IsTrue(result.Item[0].IsNothing);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ExecutionEnvironment_EvalForJson_GivenInvalidIndex_ShouldThrow()
        {
            var _environment = new ExecutionEnvironment();
            var warewolfEvalResult = _environment.EvalForJson("[[rec(0).a]]");

            var result = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalForJson_GivenValid_ShouldReturn()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Assign("[[rec().a]]", "some value", 0);

            var warewolfEvalResult = _environment.EvalForJson("[[rec(1).a]]");

            var result = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.AreEqual("some value", (result.Item as DataStorage.WarewolfAtom.DataString).Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_EvalForJson_GivenInvalidScalar_ShouldNotThrow()
        {

            var _environment = new ExecutionEnvironment();
            var warewolfEvalResult = _environment.EvalForJson("[[rec(0).a]");

            Assert.AreEqual(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing), warewolfEvalResult);

            warewolfEvalResult = _environment.EvalForJson("[[rec().a]]");

            var resultItem = (warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult).Item;
            Assert.AreEqual(1, resultItem.Count);
            Assert.IsTrue(resultItem[0].IsNothing);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignJson_GivenEmptyString_ShouldNotThrow()
        {

            var _environment = new ExecutionEnvironment();
            var beforeJson = _environment.ToJson();

            var values = new AssignValue(string.Empty, "John");
            _environment.AssignJson(values, 0);

            var afterJson = _environment.ToJson();
            Assert.AreEqual(beforeJson, afterJson);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignJson_GivenObjectExecutionEnvironment_ShouldAddObject()
        {

            var _environment = new ExecutionEnvironment();
            var values = new List<IAssignValue> { new AssignValue("[[@Person.Name]]", "John") };
            _environment.AssignJson(values, 0);

            var result = _environment.EvalAsListOfStrings("[[@Person]]", 0);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("{" + Environment.NewLine + "  \"Name\": \"John\"" + Environment.NewLine + "}", result[0]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AssignJson_GivenInvalidObject_ShouldThrowParseError()
        {

            var _environment = new ExecutionEnvironment();
            var values = new AssignValue("[[@Person.Name]", "John");

            try
            {
                _environment.AssignJson(values, 0);
                Assert.Fail("expected exception parse error");
            }
            catch (Exception e)
            {
                Assert.AreEqual("parse error", e.Message);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_Construct()
        {
            const string expectedJson = "{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[]}";

            var _environment = new ExecutionEnvironment();

            Assert.AreEqual(expectedJson, _environment.ToJson());
            Assert.AreEqual(0, _environment.AllErrors.Count);
            Assert.AreEqual(0, _environment.Errors.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_WarewolfAtomToStringErrorIfNull_ShouldReturnString()
        {
            const string expected = "SomeString";

            var givenSomeString = DataStorage.WarewolfAtom.NewDataString(expected);
            var result = ExecutionEnvironment.WarewolfAtomToStringErrorIfNull(givenSomeString);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_WarewolfAtomToStringErrorIfNull_ShouldReturnEmptyString()
        {
            var result = ExecutionEnvironment.WarewolfAtomToStringErrorIfNull(null);

            Assert.AreEqual("", result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_AddToJsonObjects_ShouldAddJsonObject()
        {
            var expectedJson = "{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{\"Person\":null}},\"Errors\":[],\"AllErrors\":[]}";
            var _environment = new ExecutionEnvironment();

            _environment.AddToJsonObjects("[[@Person().Name]]", null);

            Assert.AreEqual(expectedJson, _environment.ToJson());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_WarewolfAtomToStringErrorIfNull_ShouldThrow()
        {
            var atom = DataStorage.WarewolfAtom.Nothing;
            try
            {
                ExecutionEnvironment.WarewolfAtomToStringErrorIfNull(atom);
                Assert.Fail("expected exception: " + ErrorResource.VariableIsNull);
            }
            catch (Exception e)
            {
                Assert.AreEqual(ErrorResource.VariableIsNull, e.Message);
            }
        }

        [TestMethod]
        public void ExecutionEnvironment_WarewolfAtomToStringNullAsNothing_ShouldReturnNull()
        {
            var givenNoting = DataStorage.WarewolfAtom.Nothing;
            var result = ExecutionEnvironment.WarewolfAtomToStringNullAsNothing(givenNoting);
            Assert.IsNull(result);

            result = ExecutionEnvironment.WarewolfAtomToStringNullAsNothing(null);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ExecutionEnvironment_WarewolfAtomToStringNullAsNothing_ShouldReturnString()
        {
            var givenSomeString = DataStorage.WarewolfAtom.NewDataString("SomeString");
            var result = ExecutionEnvironment.WarewolfAtomToStringNullAsNothing(givenSomeString);
            Assert.AreEqual(givenSomeString, result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_WarewolfAtomToString_GivenNullForWarewolfAtom_ShouldReturnNull()
        {
            var result = ExecutionEnvironment.WarewolfAtomToString(null);
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_WarewolfAtomToString_GivenStringForWarewolfAtom_ShouldReturnString()
        {
            const string somestring = "SomeString";
            var atom = DataStorage.WarewolfAtom.NewDataString(somestring);

            var result = ExecutionEnvironment.WarewolfAtomToString(atom);

            Assert.AreEqual(somestring, result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_WarewolfAtomToString_GivenVarForWarewolfAtom_ShouldReturnVar()
        {
            const string somestring = "[[a]]";
            var atom = DataStorage.WarewolfAtom.NewDataString(somestring);

            var result = ExecutionEnvironment.WarewolfAtomToString(atom);

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

            _environment.AddError("error message #1");

            Assert.AreEqual(countBefore + 1, _environment.Errors.Count);
            Assert.AreEqual("error message #1", _environment.Errors.ToArray()[0]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_HasErrors_ShouldBeTrue()
        {
            var _environment = new ExecutionEnvironment();

            _environment.Errors.Add("SomeError");

            Assert.IsTrue(_environment.HasErrors());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_HasErrors_Cleared_ReturnsFalse()
        {
            var _environment = new ExecutionEnvironment();

            _environment.Errors.Add("SomeError");

            Assert.IsTrue(_environment.HasErrors());

            _environment.Errors.Clear();

            Assert.IsFalse(_environment.HasErrors());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_HasErrors_IgnoresEmptyAndNull()
        {
            var _environment = new ExecutionEnvironment();

            _environment.Errors.Add("");
            _environment.Errors.Add(null);
            _environment.Errors.Add("");

            _environment.AllErrors.Add("");
            _environment.AllErrors.Add("");
            _environment.AllErrors.Add(null);

            Assert.AreEqual(2, _environment.Errors.Count);
            Assert.AreEqual(2, _environment.AllErrors.Count);
            Assert.IsFalse(_environment.HasErrors());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_FetchError_GivenErrorsAndAllErrorsHaveCount_ShouldJoinAllErrors()
        {
            var _environment = new ExecutionEnvironment();
            _environment.Errors.Add("SomeError");
            _environment.AllErrors.Add("AnotherError");

            var expected = $"AnotherError{Environment.NewLine}SomeError";
            Assert.AreEqual(expected, _environment.FetchErrors());

            expected = "{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[\"SomeError\"],\"AllErrors\":[\"AnotherError\"]}";
            Assert.AreEqual(expected, _environment.ToJson());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_FromJson_ShouldSetValidEnvironment()
        {
            // setup
            var _environment = new ExecutionEnvironment();
            var serializedEnv = "{\"Environment\":{\"scalars\":{\"Name\":\"Bob\"},\"record_sets\":{\"R\":{\"FName\":[\"Bob\"],\"WarewolfPositionColumn\":[1]},\"Rec\":{\"Name\":[\"Bob\",,\"Bob\"],\"SurName\":[,\"Bob\",],\"WarewolfPositionColumn\":[1,3,4]},\"RecSet\":{\"Field\":[\"Bob\",\"Jane\"],\"WarewolfPositionColumn\":[1,2]}},\"json_objects\":{\"Person\":{\"Name\":\"B\"}}},\"Errors\":[],\"AllErrors\":[]}";

            // execute
            _environment.FromJson(serializedEnv);

            // verify
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
            Assert.AreEqual("{" + Environment.NewLine + "  \"Name\": \"B\"" + Environment.NewLine + "}", jsonVal);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_FromJson_ShouldSetValidEnvironment1()
        {
            // setup
            var _environment = new ExecutionEnvironment();
            var serializedEnv = "{\"Environment\":{},\"json_objects\":{}}";

            // execute
            _environment.FromJson(serializedEnv);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_FromJson_GivenInvalid_NoThrow()
        {
            var _environment = new ExecutionEnvironment();
            var serializedEnv = "some string";
            _environment.FromJson(serializedEnv);

            var expected = "{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[]}";
            Assert.AreEqual(expected, _environment.ToJson());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_FromJson_GivenJsonSerializedEnv_EmptyString_ShouldNotUpdateEnvironment()
        {
            var _environment = new ExecutionEnvironment();
            var serializedEnv = "";
            _environment.FromJson(serializedEnv);

            var expected = "{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[]}";
            Assert.AreEqual(expected, _environment.ToJson());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ExecutionEnvironment))]
        public void ExecutionEnvironment_GivenJsonSerializedEnv_FromJson_NullString_ShouldNotUpdateEnvironment()
        {
            var _environment = new ExecutionEnvironment();
            string serializedEnv = null;
            _environment.FromJson(serializedEnv);

            var expected = "{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[]}";
            Assert.AreEqual(expected, _environment.ToJson());
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
    }
}