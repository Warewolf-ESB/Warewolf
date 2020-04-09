/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Warewolf.Data;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;

namespace Warewolf.Storage.Tests
{
    [TestClass]
    public class ScopedEnvironmentTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_Setup_Constructor_ExpectEquals()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();

            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "bob", "builder");


            var p = new PrivateObject(scopedEnvironment);

            Assert.AreEqual(_mockEnv.Object, p.GetField("_inner"));
            Assert.AreEqual("bob", p.GetField("_datasource").ToString());
            Assert.AreEqual("builder", p.GetField("_alias").ToString());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_Eval_Basic_ExpectEvalReplaced()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            scopedEnvironment.Eval("[[a]]", 0);

            _mockEnv.Verify(a => a.Eval("[[Person(*)]]", 0, false, false));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_Eval_ThrowsIfNotExists()
        {
            var scopedEnvironment = new ScopedEnvironment(new ExecutionEnvironment(), "[[Person(*)]]", "[[list]]");

            try
            {
                scopedEnvironment.Eval("[[a]]", 0, true);
                Assert.Fail("expected exception variable not found");
            } catch (Exception e)
            {
                Assert.AreEqual("variable not found", e.Message);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_Eval_ExpectNoReplacement_IfNoAlias()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            scopedEnvironment.Eval("[[b]]", 0);

            _mockEnv.Verify(a => a.Eval("[[b]]", 0, false, false));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalStrict_Basic_ExpectEvalStrictReplaced()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            scopedEnvironment.EvalStrict("[[a]]", 0);

            _mockEnv.Verify(a => a.EvalStrict("[[Person(*)]]", 0));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalStrict_ExpectNoReplacement_IfNoAlias()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            scopedEnvironment.EvalStrict("[[b]]", 0);

            _mockEnv.Verify(a => a.EvalStrict("[[b]]", 0));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_Assign_Basic_ExpectAssignReplaced()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            scopedEnvironment.Assign("[[a]]", "bob", 0);

            _mockEnv.Verify(a => a.Assign("[[Person(*)]]", "bob", 0));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_Assign_HasUpdateValue_ExpectAssignReplaced()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");

            scopedEnvironment.Assign("[[a]]", "bob", 1);

            _mockEnv.Verify(a => a.Assign("[[Person(1)]]", "bob", 0));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_Assign_HasUpdateValue_ExpectAssignReplacedOnRight()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");

            scopedEnvironment.Assign("[[a]]", "[[a]]", 1);

            _mockEnv.Verify(a => a.Assign("[[Person(1)]]", "[[Person(1)]]", 0));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_Assign_ExpectNoReplacement_IfNoAlias()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            scopedEnvironment.Assign("[[b]]", "bob", 0);

            _mockEnv.Verify(a => a.Assign("[[b]]", "bob", 0));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_AssignWithFrame_Basic_ExpectAssignWithFrameReplaced()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var replaced = false;
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.AssignWithFrame(It.IsAny<IAssignValue>(), It.IsAny<int>())).Callback((IAssignValue a, int b) =>
            {
                replaced = a.Name.Contains("[[Person(*)]]");
            });

            scopedEnvironment.AssignWithFrame(new AssignValue("[[a]]", "bob"), 0);

            Assert.IsTrue(replaced);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_AssignWithFrame_ListArgument()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var ok = false;
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.AssignWithFrame(It.IsAny<IAssignValue>(), It.IsAny<int>())).Callback((IAssignValue a, int b) =>
            {
                ok = a.Name.Contains("[[Person(*)]]");
            });

            scopedEnvironment.AssignWithFrame(new List<IAssignValue> { new AssignValue("[[a]]", "bob") }, 0);

            Assert.IsTrue(ok);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_AssignStrict()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            scopedEnvironment.AssignStrict("[[a]]", "bob", 0);

            _mockEnv.Verify(a => a.AssignStrict("[[Person(*)]]", "bob", 0), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_AssignWithFrame_ExpectNoReplacement_IfNoAlias()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var replaced = false;
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.AssignWithFrame(It.IsAny<IAssignValue>(), It.IsAny<int>())).Callback((IAssignValue a, int b) =>
            {
                replaced = a.Name.Contains("[[Person(*)]]");
            });

            scopedEnvironment.AssignWithFrame(new AssignValue("[[b]]", "bob"), 0);

            Assert.IsFalse(replaced);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_GetLength_ExpectEquals()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.GetLength(It.IsAny<string>())).Returns(1);

            var length = scopedEnvironment.GetLength("[[a]]");

            Assert.AreEqual(length, 1);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_GetObjectLength_ExpectEquals()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[@Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.GetObjectLength(It.IsAny<string>())).Returns(1);

            var length = scopedEnvironment.GetObjectLength("[[a]]");

            Assert.AreEqual(length, 1);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_GetCount_ExpectEquals()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.GetCount(It.IsAny<string>())).Returns(1);

            var length = scopedEnvironment.GetCount("[[a]]");

            Assert.AreEqual(length, 1);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalRecordSetIndexes_Basic_ExpectAssignWithFrameReplaced()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var replaced = false;
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.AssignWithFrame(It.IsAny<IAssignValue>(), It.IsAny<int>())).Callback((IAssignValue a, int b) =>
            {
                replaced = a.Name.Contains("[[Person(*)]]");
            });

            scopedEnvironment.AssignWithFrame(new AssignValue("[[a]]", "bob"), 0);

            Assert.IsTrue(replaced);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalRecordSetIndexes()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var replaced = false;
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.AssignWithFrame(It.IsAny<IAssignValue>(), It.IsAny<int>())).Callback((IAssignValue a, int b) =>
            {
                replaced = a.Name.Contains("[[Person(*)]]");
            });

            scopedEnvironment.AssignWithFrame(new AssignValue("[[a]]", "bob"), 0);

            Assert.IsTrue(replaced);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalRecordSetIndexesTest()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalRecordSetIndexes("[[a]]", 1));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalRecordSetIndexesTestNegative()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalRecordSetIndexes("[[b]]", 1));

        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalAsListOfStringsTest()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalAsListOfStrings("[[a]]", 1));


        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalAsListOfStringsTestNegative()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalAsListOfStrings("[[b]]", 1));

        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalAssignFromNestedStarTest()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalAssignFromNestedStar("[[a]]",It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), 1));


        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalAssignFromNestedStarTestNegative()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalAssignFromNestedStar("[[b]]", It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(),1));

        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalAssignFromNestedLastTest()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalAssignFromNestedLast("[[a]]", It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), 1));


        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalAssignFromNestedLastTestNegative()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalAssignFromNestedLast("[[b]]", It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), 1));

        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalAssignFromNestedNumericTest()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalAssignFromNestedNumeric("[[a]]", It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), 1));


        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalAssignFromNestedNumericTestNegative()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalAssignFromNestedNumeric("[[b]]", It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), 1));

        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalDeleteTest()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalDelete("[[a]]", 1));


        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalDeleteTestNegative()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalDelete("[[b]]", 1));

        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_SortRecordSetTest()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.SortRecordSet("[[a]]",It.IsAny<bool>(), 1));


        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_SortRecordSetTestNegative()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.SortRecordSet("[[b]]",It.IsAny<bool>(), 1));

        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalAsListTest()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalAsList("[[a]]",  1));


        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalAsListTestNegative()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalAsList("[[b]]",  1));

        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalWhereTest()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalWhere("[[a]]", ax => true, 1));


        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalWhereTestNegative()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalWhere("[[b]]",ax=>true, 1));

        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_ApplyUpdateTestNegative()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.ApplyUpdate("[[b]]", ax => ax, 1));

        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalToExpressionTest()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalToExpression("[[a]]",  1));


        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalToExpressionTestNegative()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalToExpression("[[b]]", 1));

        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalForDataMergeTest()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalForDataMerge("[[a]]", 1));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalForDataMergeTestNegative()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalForDataMerge("[[b]]", 1));

        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_HasRecordSet_expectPassThrough()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.HasRecordSet("a");
            _mockEnv.Verify(a => a.HasRecordSet("a"));
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_CommitAssign_expectPassThrough()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.CommitAssign();
            _mockEnv.Verify(a => a.CommitAssign());
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_ToStar_expectPassThrough()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.ToStar("[[a]]");
            _mockEnv.Verify(a => a.ToStar("[[Person(*)]]"));
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_Errors_expectPassThrough()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            var x = scopedEnvironment.Errors;

            _mockEnv.Verify(a => a.Errors);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_AllErrors_expectPassThrough()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            var x = scopedEnvironment.AllErrors;
            _mockEnv.Verify(a => a.AllErrors);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_AddError_expectPassThrough()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.AddError("bob");
            _mockEnv.Verify(a => a.AddError("bob"));
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_AssignDataShape_expectPassThrough()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.AssignDataShape("[[a]]");
            _mockEnv.Verify(a => a.AssignDataShape("[[Person(*)]]"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_ApplyUpdate_expectPassThrough()
        {
            var personName = "[[@Person(*).Name]]";
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, personName, "[[a]]");
            var clause = new Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom>(atom => atom);
            _mockEnv.Setup(environment => environment.Eval(personName, 0))
                .Returns(() => CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing));
            scopedEnvironment.ApplyUpdate(personName, clause, 0);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_AssignJson_expectPassThrough()
        {
            var personName = "[[@Person(*).Name]]";
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, personName, "[[a]]");
            scopedEnvironment.AssignJson(new AssignValue(personName, "[[a]]"),0);
            _mockEnv.Verify(environment => environment.AssignJson(new AssignValue(personName, "[[a]]"), 0));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_GivenListOfJsonObjcts_AssignJson_ShouldAssighnAll()
        {
            var personName = "[[@Person(*).Name]]";
            var assignValues = new List<IAssignValue>
            {
                new AssignValue("[[@Person.Name]]", "John"),
                new AssignValue("[[@Person(2).Name]]", "James"),
                new AssignValue("[[@Person(3).Name]]", "Jason")
            };
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, personName, "[[a]]");
            scopedEnvironment.AssignJson(assignValues, 0);
            _mockEnv.Verify(environment => environment.AssignJson(assignValues, 0));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_GivenListOfJsonObjct_AddToJsonObjects_ShouldAddJsonObject()
        {
            var personName = "[[@Person().Name]]";
            var childName = "[[@Person().Name]]";

            var obj = new JArray(personName);
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, personName, "[[a]]");
            scopedEnvironment.AddToJsonObjects(childName,  obj);
            _mockEnv.Verify(environment => environment.AddToJsonObjects(childName, obj));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_GivenListOfJsonObjct_EvalForJson_ShouldAddJsonObject()
        {
            var personName = "[[@Person().Name]]";
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, personName, "[[a]]");
            scopedEnvironment.EvalForJson(personName);
            _mockEnv.Verify(environment => environment.EvalForJson(personName, false));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_GivenListOfJsonObjct_AssignUnique_Should()
        {
            var personName = "[[@Person().Name]]";
            var recs = new List<string>
            {
                "[[Person().Name]]",
                "[[Person(1).Name]]",
                "[[Person(2).Name]]"
            };
            var values = new List<string> {personName};
            var resList = new List<string>();
            Assert.IsNotNull(resList);
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, personName, "[[a]]");
            scopedEnvironment.AssignUnique(recs, values, resList, 0);
            _mockEnv.Verify(environment => environment.AssignUnique(recs, values, resList, 0));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_GetIndexes_ShouldGetIndex()
        {
            var datasource = "[[@Person(*).Name]]";
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, datasource, "[[a]]");
            scopedEnvironment.GetIndexes(datasource);
            _mockEnv.Verify(environment => environment.GetIndexes(datasource));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalJContainer_Should()
        {
            var datasource = "[[@Person(*).Name]]";
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, datasource, "[[a]]");
            scopedEnvironment.EvalJContainer(datasource);
            _mockEnv.Verify(environment => environment.EvalJContainer(datasource));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_SetDataSource_ShouldSetNewDataSource()
        {
            var datasource = "[[Person(*)]]";
            var personName = "[[@Person(*).Name]]";
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, datasource, "[[a]]");
            var privateObj = new PrivateObject(scopedEnvironment);
            var ds = privateObj.GetField("_datasource");
            Assert.IsNotNull(ds);
            Assert.AreEqual(datasource, ds);
            scopedEnvironment.SetDataSource(personName);
            ds = privateObj.GetField("_datasource");
            Assert.AreEqual(personName, ds);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_AssignDataShape_expectPassThrough_NoReplace()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.AssignDataShape("a");
            _mockEnv.Verify(a => a.AssignDataShape("a"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_FetchErrors_expectPassThrough()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.FetchErrors();
            _mockEnv.Verify(a => a.FetchErrors());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_HasErrors_expectPassThrough()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.HasErrors();
            _mockEnv.Verify(a => a.HasErrors());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalAsTable()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var expected = new Mock<IEnumerable<Tuple<string, DataStorage.WarewolfAtom>[]>>().Object;
            _mockEnv.Setup(o => o.EvalAsTable("[[rec()]]", 0)).Returns(expected);
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            var result = scopedEnvironment.EvalAsTable("[[rec()]]", 0);

            _mockEnv.Verify(o => o.EvalAsTable("[[rec()]]", 0), Times.Once);
            Assert.AreSame(expected, result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_EvalAsTable2()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var expected = new Mock<IEnumerable<Tuple<string, DataStorage.WarewolfAtom>[]>>().Object;
            _mockEnv.Setup(o => o.EvalAsTable("[[rec()]]", 0, true)).Returns(expected);
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            Assert.AreSame(expected, scopedEnvironment.EvalAsTable("[[rec()]]", 0, true));

            _mockEnv.Verify(o => o.EvalAsTable("[[rec()]]", 0, true), Times.Once);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ScopedEnvironment))]
        public void ScopedEnvironment_ToJson()
        {
            var _mockEnv = new Mock<IExecutionEnvironment>();
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            Assert.AreEqual("", scopedEnvironment.ToJson());
        }


        void SetupReplacementFunction(ScopedEnvironment env, IEnumerable<string> originals, IEnumerable<string> replacements, Action<ScopedEnvironment> envAction)
        {
            var orzipped = originals.Zip(replacements, (a, b) => new Tuple<string, string>(a, b));
           var p = new PrivateObject(env);
            var fun = p.GetFieldOrProperty("_doReplace") as Func<string, int,string,string>;
           p.SetFieldOrProperty("_doReplace",new Func<string, int,string,string>(
               (s, i,val) =>
               {
                   var replaced =  fun(s,i,val);
                   Assert.IsTrue(orzipped.Any(a=>a.Item1==val&&a.Item2==replaced));

                   return replaced;
               }));
            envAction?.Invoke(env);
        }

        void SetupReplacementFunctionDoesNotOccur(ScopedEnvironment env, Action<ScopedEnvironment> envAction)
        {
            var p = new PrivateObject(env);
            var fun = p.GetFieldOrProperty("_doReplace") as Func<string, int, string, string>;
            p.SetFieldOrProperty("_doReplace", new Func<string, int, string, string>(
                (s, i, val) =>
                {
                    var replaced = fun(s, i, val);

                    Assert.AreEqual(replaced, val);
                    return replaced;
                }));
            envAction?.Invoke(env);

        }
    }
}
