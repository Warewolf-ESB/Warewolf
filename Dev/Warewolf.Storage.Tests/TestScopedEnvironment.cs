using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;



namespace Warewolf.Storage.Tests
{
    [TestClass]
    public class TestScopedEnvironment
    {
        private Mock<IExecutionEnvironment> _mockEnv;

        [TestInitialize]
        public void Setup()
        {
            _mockEnv = new Mock<IExecutionEnvironment>();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Setup")]
        public void ScopedEnvironment_Setup_Constructor_ExpectEquals()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "bob", "builder");

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            PrivateObject p = new PrivateObject(scopedEnvironment);
            Assert.AreEqual(_mockEnv.Object, p.GetField("_inner"));
            Assert.AreEqual("bob", p.GetField("_datasource").ToString());
            Assert.AreEqual("builder", p.GetField("_alias").ToString());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Eval")]
        public void ScopedEnvironment_Eval_Basic_ExpectEvalReplaced()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.Eval("[[a]]", 0);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.Eval("[[Person(*)]]", 0, false, false));
        }
       

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Eval")]
        public void ScopedEnvironment_Eval_ExpectNoReplacement_IfNoAlias()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.Eval("[[b]]", 0);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.Eval("[[b]]", 0, false, false));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalStrict")]
        public void ScopedEnvironment_EvalStrict_Basic_ExpectEvalStrictReplaced()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.EvalStrict("[[a]]", 0);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.EvalStrict("[[Person(*)]]", 0));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalStrict")]
        public void ScopedEnvironment_EvalStrict_ExpectNoReplacement_IfNoAlias()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.EvalStrict("[[b]]", 0);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.EvalStrict("[[b]]", 0));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Assign")]
        public void ScopedEnvironment_Assign_Basic_ExpectAssignReplaced()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.Assign("[[a]]", "bob", 0);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.Assign("[[Person(*)]]", "bob", 0));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Assign")]
        public void ScopedEnvironment_Assign_HasUpdateValue_ExpectAssignReplaced()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.Assign("[[a]]", "bob", 1);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.Assign("[[Person(1)]]", "bob", 0));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Assign")]
        public void ScopedEnvironment_Assign_HasUpdateValue_ExpectAssignReplacedOnRight()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.Assign("[[a]]", "[[a]]", 1);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.Assign("[[Person(1)]]", "[[Person(1)]]", 0));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Assign")]
        public void ScopedEnvironment_Assign_ExpectNoReplacement_IfNoAlias()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.Assign("[[b]]", "bob", 0);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.Assign("[[b]]", "bob", 0));
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignWithFrame")]
        public void ScopedEnvironment_AssignWithFrame_Basic_ExpectAssignWithFrameReplaced()
        {
            //------------Setup for test--------------------------
            bool replaced = false;
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.AssignWithFrame(It.IsAny<IAssignValue>(), It.IsAny<int>())).Callback((IAssignValue a,  int b) =>
            {
                replaced = a.Name.Contains("[[Person(*)]]");
            });

            //------------Execute Test---------------------------
            scopedEnvironment.AssignWithFrame(new AssignValue("[[a]]", "bob"), 0);
            //------------Assert Results-------------------------
            Assert.IsTrue(replaced);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignWithFrame")]
        public void ScopedEnvironment_AssignWithFrame_ExpectNoReplacement_IfNoAlias()
        {
            //------------Setup for test--------------------------
            bool replaced = false;
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.AssignWithFrame(It.IsAny<IAssignValue>(), It.IsAny<int>())).Callback((IAssignValue a, int b) =>
            {
                replaced = a.Name.Contains("[[Person(*)]]");
            });

            //------------Execute Test---------------------------
            scopedEnvironment.AssignWithFrame(new AssignValue("[[b]]", "bob"), 0);
            //------------Assert Results-------------------------
            Assert.IsFalse(replaced);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_GetLength")]
        public void ScopedEnvironment_GetLength_ExpectEquals()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.GetLength(It.IsAny<string>())).Returns(1);

            //------------Execute Test---------------------------
            var length = scopedEnvironment.GetLength("[[a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(length, 1);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ScopedEnvironment_GetObjectLength")]
        public void ScopedEnvironment_GetObjectLength_ExpectEquals()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[@Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.GetObjectLength(It.IsAny<string>())).Returns(1);

            //------------Execute Test---------------------------
            var length = scopedEnvironment.GetObjectLength("[[a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(length, 1);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_GetCount")]
        public void ScopedEnvironment_GetCount_ExpectEquals()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.GetCount(It.IsAny<string>())).Returns(1);

            //------------Execute Test---------------------------
            var length = scopedEnvironment.GetCount("[[a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(length, 1);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalRecordSetIndexes")]
        public void ScopedEnvironment_EvalRecordSetIndexes_Basic_ExpectAssignWithFrameReplaced()
        {
            //------------Setup for test--------------------------
            bool replaced = false;
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.AssignWithFrame(It.IsAny<IAssignValue>(), It.IsAny<int>())).Callback((IAssignValue a, int b) =>
            {
                replaced = a.Name.Contains("[[Person(*)]]");
            });

            //------------Execute Test---------------------------
            scopedEnvironment.AssignWithFrame(new AssignValue("[[a]]", "bob"), 0);
            //------------Assert Results-------------------------
            Assert.IsTrue(replaced);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalRecordSetIndexes")]
        public void ScopedEnvironment_EvalRecordSetIndexes()
        {
            //------------Setup for test--------------------------
            bool replaced = false;
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.AssignWithFrame(It.IsAny<IAssignValue>(), It.IsAny<int>())).Callback((IAssignValue a, int b) =>
            {
                replaced = a.Name.Contains("[[Person(*)]]");
            });

            //------------Execute Test---------------------------
            scopedEnvironment.AssignWithFrame(new AssignValue("[[a]]", "bob"), 0);
            //------------Assert Results-------------------------
            Assert.IsTrue(replaced);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalRecordSetIndexes")]
        public void ScopedEnvironment_EvalRecordSetIndexesTest()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalRecordSetIndexes("[[a]]", 1));
   

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalRecordSetIndexes")]
        public void ScopedEnvironment_EvalRecordSetIndexesTestNegative()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalRecordSetIndexes("[[b]]", 1));

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalAsListOfStrings")]
        public void ScopedEnvironment_EvalAsListOfStringsTest()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalAsListOfStrings("[[a]]", 1));


        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalAsListOfStrings")]
        public void ScopedEnvironment_EvalAsListOfStringsTestNegative()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalAsListOfStrings("[[b]]", 1));

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalAssignFromNestedStar")]
        public void ScopedEnvironment_EvalAssignFromNestedStarTest()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalAssignFromNestedStar("[[a]]",It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), 1));


        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalAssignFromNestedStar")]
        public void ScopedEnvironment_EvalAssignFromNestedStarTestNegative()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalAssignFromNestedStar("[[b]]", It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(),1));

        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalAssignFromNestedLast")]
        public void ScopedEnvironment_EvalAssignFromNestedLastTest()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalAssignFromNestedLast("[[a]]", It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), 1));


        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalAssignFromNestedLast")]
        public void ScopedEnvironment_EvalAssignFromNestedLastTestNegative()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalAssignFromNestedLast("[[b]]", It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), 1));

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalAssignFromNestedNumeric")]
        public void ScopedEnvironment_EvalAssignFromNestedNumericTest()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalAssignFromNestedNumeric("[[a]]", It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), 1));


        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalAssignFromNestedNumeric")]
        public void ScopedEnvironment_EvalAssignFromNestedNumericTestNegative()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalAssignFromNestedNumeric("[[b]]", It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), 1));

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalDelete")]
        public void ScopedEnvironment_EvalDeleteTest()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalDelete("[[a]]", 1));


        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalDelete")]
        public void ScopedEnvironment_EvalDeleteTestNegative()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalDelete("[[b]]", 1));

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_SortRecordSet")]
        public void ScopedEnvironment_SortRecordSetTest()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.SortRecordSet("[[a]]",It.IsAny<bool>(), 1));


        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_SortRecordSet")]
        public void ScopedEnvironment_SortRecordSetTestNegative()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.SortRecordSet("[[b]]",It.IsAny<bool>(), 1));

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalAsList")]
        public void ScopedEnvironment_EvalAsListTest()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalAsList("[[a]]",  1));


        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalAsList")]
        public void ScopedEnvironment_EvalAsListTestNegative()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalAsList("[[b]]",  1));

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalWhere")]
        public void ScopedEnvironment_EvalWhereTest()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalWhere("[[a]]", ax => true, 1));


        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalWhere")]
        public void ScopedEnvironment_EvalWhereTestNegative()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalWhere("[[b]]",ax=>true, 1));

        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_ApplyUpdate")]
        public void ScopedEnvironment_ApplyUpdateTestNegative()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.ApplyUpdate("[[b]]", ax => ax, 1));

        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalToExpression")]
        public void ScopedEnvironment_EvalToExpressionTest()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalToExpression("[[a]]",  1));


        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalToExpression")]
        public void ScopedEnvironment_EvalToExpressionTestNegative()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalToExpression("[[b]]", 1));

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalForDataMerge")]
        public void ScopedEnvironment_EvalForDataMergeTest()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(1)]]", "[[a]]");
            SetupReplacementFunction(scopedEnvironment, new List<string>() { "[[a]]" }, new List<string> { "[[Person(1)]]" }, a => a.EvalForDataMerge("[[a]]", 1));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalForDataMerge")]
        public void ScopedEnvironment_EvalForDataMergeTestNegative()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            SetupReplacementFunctionDoesNotOccur(scopedEnvironment, a => a.EvalForDataMerge("[[b]]", 1));

        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_HasRecordSet")]
        public void ScopedEnvironment_HasRecordSet_expectPassThrough()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.HasRecordSet("a");
            _mockEnv.Verify(a => a.HasRecordSet("a"));
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_CommitAssign")]
        public void ScopedEnvironment_CommitAssign_expectPassThrough()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.CommitAssign();
            _mockEnv.Verify(a => a.CommitAssign());
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_ToStar")]
        public void ScopedEnvironment_ToStar_expectPassThrough()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.ToStar("[[a]]");
            _mockEnv.Verify(a => a.ToStar("[[Person(*)]]"));
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Errors")]
        public void ScopedEnvironment_Errors_expectPassThrough()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            var x = scopedEnvironment.Errors;
            _mockEnv.Verify(a => a.Errors);
        
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AllErrors")]
        public void ScopedEnvironment_AllErrors_expectPassThrough()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            var x = scopedEnvironment.AllErrors;
            _mockEnv.Verify(a => a.AllErrors);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AddError")]
        public void ScopedEnvironment_AddError_expectPassThrough()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.AddError("bob");
            _mockEnv.Verify(a => a.AddError("bob"));
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignDataShape")]
        public void ScopedEnvironment_AssignDataShape_expectPassThrough()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.AssignDataShape("[[a]]");
            _mockEnv.Verify(a => a.AssignDataShape("[[Person(*)]]"));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignDataShape")]
        public void ScopedEnvironment_ApplyUpdate_expectPassThrough()
        {
            var personName = "[[@Person(*).Name]]";
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, personName, "[[a]]");
            var clause = new Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom>(atom => atom);
            _mockEnv.Setup(environment => environment.Eval(personName, 0, false, false))
                .Returns(() => CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing));            
            scopedEnvironment.ApplyUpdate(personName, clause, 0);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignDataShape")]
        public void ScopedEnvironment_AssignJson_expectPassThrough()
        {
            var personName = "[[@Person(*).Name]]";
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, personName, "[[a]]");
            scopedEnvironment.AssignJson(new AssignValue(personName, "[[a]]"),0);
            _mockEnv.Verify(environment => environment.AssignJson(new AssignValue(personName, "[[a]]"), 0));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignDataShape")]
        public void GivenListOfJsonObjcts_ScopedEnvironment_AssignJson_ShouldAssighnAll()
        {
            var personName = "[[@Person(*).Name]]";
            var assignValues = new List<IAssignValue>
            {
                new AssignValue("[[@Person.Name]]", "John"),
                new AssignValue("[[@Person(2).Name]]", "James"),
                new AssignValue("[[@Person(3).Name]]", "Jason")
            };
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, personName, "[[a]]");
            scopedEnvironment.AssignJson(assignValues, 0);
            _mockEnv.Verify(environment => environment.AssignJson(assignValues, 0));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignDataShape")]
        public void GivenListOfJsonObjct_ScopedEnvironment_AddToJsonObjects_ShouldAddJsonObject()
        {            
            var personName = "[[@Person().Name]]";
            var childName = "[[@Person().Name]]";

            var obj = new JArray(personName);
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, personName, "[[a]]");
            scopedEnvironment.AddToJsonObjects(childName,  obj);
            _mockEnv.Verify(environment => environment.AddToJsonObjects(childName, obj));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignDataShape")]
        public void GivenListOfJsonObjct_ScopedEnvironment_EvalForJson_ShouldAddJsonObject()
        {
            var personName = "[[@Person().Name]]";
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, personName, "[[a]]");
            scopedEnvironment.EvalForJson(personName);
            _mockEnv.Verify(environment => environment.EvalForJson(personName, false));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignDataShape")]
        public void GivenListOfJsonObjct_ScopedEnvironment_AssignUnique_Should()
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
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, personName, "[[a]]");
            scopedEnvironment.AssignUnique(recs, values, resList, 0);
            _mockEnv.Verify(environment => environment.AssignUnique(recs, values, resList, 0));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignDataShape")]
        public void ScopedEnvironment_GetIndexes_ShouldGetIndex()
        {
            var datasource = "[[@Person(*).Name]]";
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, datasource, "[[a]]");
            scopedEnvironment.GetIndexes(datasource);            
            _mockEnv.Verify(environment => environment.GetIndexes(datasource));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignDataShape")]
        public void ScopedEnvironment_EvalJContainer_Should()
        {
            var datasource = "[[@Person(*).Name]]";
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, datasource, "[[a]]");
            scopedEnvironment.EvalJContainer(datasource);            
            _mockEnv.Verify(environment => environment.EvalJContainer(datasource));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignDataShape")]
        public void ScopedEnvironment_SetDataSource_ShouldSetNewDataSource()
        {
            var datasource = "[[Person(*)]]";
            var personName = "[[@Person(*).Name]]";
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
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignDataShape")]
        public void ScopedEnvironment_AssignDataShape_expectPassThrough_NoReplace()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.AssignDataShape("a");
            _mockEnv.Verify(a => a.AssignDataShape("a"));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_FetchErrors")]
        public void ScopedEnvironment_FetchErrors_expectPassThrough()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.FetchErrors();
            _mockEnv.Verify(a => a.FetchErrors());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_HasErrors")]
        public void ScopedEnvironment_HasErrors_expectPassThrough()
        {
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            scopedEnvironment.HasErrors();
            _mockEnv.Verify(a => a.HasErrors());
        }


         void SetupReplacementFunction(ScopedEnvironment env, IEnumerable<string> originals, IEnumerable<string> replacements, Action<ScopedEnvironment> envAction)
        {
            var orzipped = originals.Zip(replacements, (a, b) => new Tuple<string, string>(a, b));
           PrivateObject p = new PrivateObject(env);
           var fun = p.GetFieldOrProperty("_doReplace") as Func<string, int,string,string>;
           p.SetFieldOrProperty("_doReplace",new Func<string, int,string,string>(
               (s, i,val) =>
               {

                   
                   var replaced =  fun(s,i,val);
                   Assert.IsTrue(orzipped.Any(a=>a.Item1==val&&a.Item2==replaced));
                  
                   return replaced;
               }));
           envAction(env);
        }

        void SetupReplacementFunctionDoesNotOccur(ScopedEnvironment env, Action<ScopedEnvironment> envAction)
        {
            PrivateObject p = new PrivateObject(env);
            var fun = p.GetFieldOrProperty("_doReplace") as Func<string, int, string, string>;
            p.SetFieldOrProperty("_doReplace", new Func<string, int, string, string>(
                (s, i, val) =>
                {
                    
                    var replaced = fun(s, i, val);

                    Assert.AreEqual(replaced, val);
                    return replaced;
                }));
            envAction(env);

        }
    }
}
