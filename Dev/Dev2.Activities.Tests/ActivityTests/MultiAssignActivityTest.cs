using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ActivityUnitTests;
using Dev2.Common;
using Dev2.Common.Interfaces.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for AssignActivity
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class MultiAssignActivityTest : BaseActivityUnitTest
    {
        IList<string> _fieldName;
        IList<string> _fieldValue;
        readonly ObservableCollection<ActivityDTO> _fieldCollection = new ObservableCollection<ActivityDTO>();

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {

            _fieldName = new List<string>();
            _fieldValue = new List<string>();
            _fieldName.Add("[[testValue1]]");
            _fieldName.Add("[[testName1]]");
            _fieldName.Add("[[testName2]]");
            _fieldName.Add("[[testName3]]");
            _fieldName.Add("[[testName4]]");
            _fieldName.Add("[[testName5]]");
            _fieldName.Add("[[testName6]]");
            _fieldName.Add("[[testName7]]");
            _fieldName.Add("[[testName8]]");
            _fieldName.Add("[[testName9]]");
            _fieldName.Add("[[testName10]]");

            _fieldValue.Add("bob");
            _fieldValue.Add("[[testValue1]]");
            _fieldValue.Add("testValue2");
            _fieldValue.Add("testValue3");
            _fieldValue.Add("testValue4");
            _fieldValue.Add("testValue5");
            _fieldValue.Add("testValue6");
            _fieldValue.Add("testValue7");
            _fieldValue.Add("testValue8");
            _fieldValue.Add("testValue9");
            _fieldValue.Add("testValue10");


            for(int i = 0; i < _fieldName.Count; i++)
            {
                _fieldCollection.Add(new ActivityDTO(_fieldName[i], _fieldValue[i], _fieldCollection.Count));
            }


        }

        #endregion

        #region MultiAssign Functionality Tests

        [TestMethod]
        public void AssignRecordSetWithEvaluatedSingleExpression()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec().opt]]", _fieldCollection.Count));
            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , ActivityStrings.mult_assign_expression_both_sides_single_rs_adl);
            IDSFDataObject result = ExecuteProcess();

            string error;
            const string expected = "Value1";
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error).FirstOrDefault();

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void RecursiveEvaluation()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[recset]]", "gRec", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[field]]", "opt", _fieldCollection.Count));

            _fieldCollection.Add(new ActivityDTO("[[cRec(1).opt]]", "[[[[recset]]().[[field]]]]", _fieldCollection.Count));
            SetupArguments(
                            @"<DataList>
  <cRec>
    <opt></opt>
    <display />
  </cRec>
  <gRec>
    <opt></opt>
    <display></display>
  </gRec>
  <recset></recset>
  <field></field>
</DataList>"
                          , @"<DataList>
  <cRec>
    <opt></opt>
    <display />
  </cRec>
  <gRec>
    <opt>Value1</opt>
    <display>display1</display>
  </gRec>
  <recset>gRec</recset>
  <field>opt</field>
</DataList>");

            IDSFDataObject result = ExecuteProcess();

            const string expected = "Value1";
            string error;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error).First();

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedDoubleExpressionSameRecordSet()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec().opt]]", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[cRec().display]]", "[[gRec().display]]", _fieldCollection.Count));

            const string data = "<ADL><gRec><opt>Value1</opt><display>display1</display></gRec></ADL>";

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , "<root>" + data + "</root>");
            IDSFDataObject result = ExecuteProcess();

            const string expected = "display1";
            string error;
            List<string> vals = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "display", out error);
            string actual = vals[0];

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedSingleExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec().opt]]", _fieldCollection.Count));

            const string data = "<root><gRec><opt>Value1</opt><display>display1</display></gRec></root>";

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , data);
            IDSFDataObject result = ExecuteProcess();
            const string expected = "Value1";
            string error;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error).First();

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedDoubleExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec().opt]]", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[cRec().display]]", "[[gRec().display]]", _fieldCollection.Count));

            const string data = "<ADL><gRec><opt>Value1</opt><display>display1</display></gRec></ADL>";

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , "<root>" + data + "</root>");
            IDSFDataObject result = ExecuteProcess();
            const string expected = "display1";

            string error;

            IList<string> vals = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "display", out error);
            string actual = vals[0];

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedRecursiveRightSingleAssignMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[rsElement]]", "gRec", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[rsFieldElement]]", "opt", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[gRec(1).opt]]", "Value1", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[gRec(1).display]]", "display1", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[gRec(2).opt]]", "Value2", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[gRec(2).display]]", "display2", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[[[rsElement]]().[[rsFieldElement]]]]", _fieldCollection.Count));

            const string data = "<root><rsElement></rsElement><rsFieldElement></rsFieldElement><gRec><opt></opt><display></display></gRec></root>";

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_mult_eval_fields_rs_adl
                          , data);
            IDSFDataObject result = ExecuteProcess();
            const string expected = "Value2";
            string error;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error).First();

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedSingleOutOfBoundRightIndexExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec(100).opt]]", _fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                         , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl);
            IDSFDataObject result = ExecuteProcess();
            string error;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error).First();

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.IsTrue(actual == string.Empty);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedSingleOutOfBoundLeftIndexExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec(100).opt]]", "[[gRec().opt]]", _fieldCollection.Count));

            const string shape = @"<ADL>
<gRec>
<opt></opt>
<display></display>
</gRec>
<cRec>
<opt/>
<display/>
</cRec>
</ADL>";

            SetupArguments(
                            shape
                          , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl);
            IDSFDataObject result = ExecuteProcess();

            string error;
            List<string> resultCollection = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(2, resultCollection.Count);
            Assert.AreEqual("Value2", resultCollection[1]);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedSingleOutOfBoundBothIndexExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec(100).opt]]", "[[gRec(100).opt]]", _fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl);

            IDSFDataObject result = ExecuteProcess();
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            // two cols at row 100
            Assert.IsTrue(actual.Count == 1 && actual.First() == string.Empty);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedMultOutOfBoundLeftIndexExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec(100).opt]]", "[[gRec().opt]]", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[cRec(100).display]]", "[[gRec().display]]", _fieldCollection.Count));

            const string shape = @"<ADL>
<gRec>
<opt></opt>
<display></display>
</gRec>
<cRec>
<opt/>
<display/>
</cRec>
</ADL>";

            SetupArguments(
                            shape
                          , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl);
            IDSFDataObject result = ExecuteProcess();

            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            // first and row 100
            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual("Value2", actual.Last());
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedMultOutOfBoundBothIndexExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec(3).opt]]", "[[gRec(3).opt]]", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[cRec(3).display]]", "[[gRec(3).display]]", _fieldCollection.Count));

            SetupArguments(@"<ADL>
<gRec>
<opt>Value1</opt>
<display>display1</display>
</gRec>
<gRec>
<opt>Value2</opt>
<display>display2</display>
</gRec>
<cRec>
<opt/>
<display/>
</cRec>
</ADL>"
                          , @"<ADL>
<gRec>
<opt>Value1</opt>
<display>display1</display>
</gRec>
<gRec>
<opt>Value2</opt>
<display>display2</display>
</gRec>
<cRec>
<opt/>
<display/>
</cRec>
</ADL>");
            IDSFDataObject result = ExecuteProcess();
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            // first and row 100
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(string.Empty, actual.First());
            Assert.AreEqual(string.Empty, actual.Last());
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedMultOutOfBoundRightIndexExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec(100).opt]]", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[cRec().display]]", "[[gRec(100).display]]", _fieldCollection.Count));
            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl);
            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error).First();

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.IsTrue(actual == string.Empty);
        }


        [TestMethod]
        public void AssignRecordSetWithEvaluatedMultLeftIndexInBoundsExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec(1).opt]]", "[[gRec().opt]]", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[cRec(1).display]]", "[[gRec().display]]", _fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , ActivityStrings.mult_assign_expression_both_sides_single_rs_adl);
            IDSFDataObject result = ExecuteProcess();
            const string expected = "display1";
            string error;

            List<string> vals = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "display", out error);
            string actual = vals[0];

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        // -- End New

        [TestMethod]
        public void MultiAssignTenAtOnce_Expected_MultiAssignCorrectlySetsAllScalarValues()
        {
            SetupArguments(
                            ActivityStrings.NewScalarShape
                          , ActivityStrings.NewScalarShape);
            IDSFDataObject result = ExecuteProcess();
            const string expected = "bob";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testName1", out actual, out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MultiAssignWithAnEmptyField_Expected_FieldInDataListNotAssignedValue()
        {
            _fieldCollection[0].FieldName = "";

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape);

            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testValue1", out actual, out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.IsTrue(actual == string.Empty);
        }

        [TestMethod]
        public void MultiAssignWithAnEmptyValue_Expected_FieldInDataListNotAssignedValue()
        {
            _fieldCollection[0].FieldValue = "";
            _fieldCollection[1].FieldValue = "";
            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape);

            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testValue1", out actual, out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.IsTrue(actual == string.Empty);
        }

        [TestMethod]
        public void MultiAssignWithSpecialCharsInValue()
        {
            _fieldCollection[0].FieldValue = "testValue@#";

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape);

            IDSFDataObject result = ExecuteProcess();
            const string expected = "testValue@#";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testValue1", out actual, out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MutiAssignWithAddingOneRecSets()
        {
            _fieldCollection.Add(new ActivityDTO("[[testRecSet1().testRec1]]", "testRecValue1", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("", "", _fieldCollection.Count));
            SetupArguments(
                            ActivityStrings.recsetDataListShape
                          , "<root></root>");

            IDSFDataObject result = ExecuteProcess();

            const string expected = "testRecValue1";
            string error;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "testRecSet1", "testRec1", out error).First();

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MutiAssignWithAddingTenRecSets_Expected_RecordSetPopulatedToIndex()
        {
            _fieldCollection.Add(new ActivityDTO("[[testRecSet1(10).testRec1]]", "testRecValue1", _fieldCollection.Count));

            SetupArguments(
                                        ActivityStrings.recsetDataListShape
                                      , "<root>" + ActivityStrings.recsetDataListShape + "</root>");

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { ""
                                                     , "testRecValue1" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "testRecSet1", "testRec1", out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void MutiAssignWithEditingExistingRecSets_Expected_RecordSetDataOverwritten()
        {
            _fieldCollection.Add(new ActivityDTO("[[testRecSet1(1).testRec1]]", "testRecValue1", _fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.recsetDataListShape
                          , ActivityStrings.recsetDataListShape);

            IDSFDataObject result = ExecuteProcess();

            const string expected = "testRecValue1";
            string error;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "testRecSet1", "testRec1", out error).First();

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ScalarInRecordset_Expected_MultiAssignCorrectlyIdentifiesField()
        {

            _fieldCollection.Clear();

            _fieldCollection.Add(new ActivityDTO("[[a]]", "1", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[b]]", "2", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[c]]", "[[a]] + [[b]]", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[recset().a]]", "abc123", _fieldCollection.Count));

            SetupArguments(
                ActivityStrings.scalar_in_recordset_adl
              , "<root></root>");

            IDSFDataObject result = ExecuteProcess();

            const string expected = "abc123";
            string error;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "a", out error).First();

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }


        //2013.02.05: Ashley Lewis - Bug 8725:Task 8743
        [TestMethod]
        public void AssignRecordSetWithNoIndexAndJustOneExistingRecordExpectedRecordAppended()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "New Value", _fieldCollection.Count));

            SetupArguments(
                            @"<root>
  <cRec>
    <opt></opt>
    <display />
  </cRec>
</root>"
                          , @"<root>
  <cRec>
    <opt>Existing Value</opt>
    <display />
  </cRec>
</root>");
            IDSFDataObject result = ExecuteProcess();
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(2, actual.Count());
        }

        [TestMethod]
        public void AssignRecordSetWithAppendRecordAndNoExistingRecordExpectedRecordInFirst()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "New Value", _fieldCollection.Count));

            SetupArguments(
                            @"<root>
  <cRec>
    <opt />
    <display />
  </cRec>
</root>"
                          , @"<root></root>");
            IDSFDataObject result = ExecuteProcess();
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual("New Value", actual[0]);
        }

        //2013.02.07: Ashley Lewis - Bug 8725:Task 8790 DONE
        [TestMethod]
        public void MutiAssignWithAddingTenRecSetsExpectedRecordSetAppended()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec(10).opt]]", "testRecValue1", _fieldCollection.Count));

            SetupArguments(
                                        @"<root>
  <cRec>
    <opt />
  </cRec>
</root>"
                                      , "<root></root>");

            IDSFDataObject result = ExecuteProcess();

            string error;
            IList<IBinaryDataListItem> actual;
            GetRecordSetFieldValueFromDataList(result.DataListID, "cRec", "opt", out actual, out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            var binaryDataListItem = actual.FirstOrDefault(c => c.DisplayValue == "cRec(10).opt");
            if(binaryDataListItem != null)
                Assert.AreEqual("testRecValue1", binaryDataListItem.TheValue);
        }

        //2013.02.08: Ashley Lewis - Bug 8725, Task 8797 
        [TestMethod]
        public void MutiAssignWithCalculationOnBlankRecordSetExpectedCalculationReplacesBlankWithZero()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[scalar]]", GlobalConstants.CalculateTextConvertPrefix + "sum([[cRec().opt]])+1" + GlobalConstants.CalculateTextConvertSuffix, _fieldCollection.Count));

            SetupArguments(
                                        @"<root>
  <scalar />
  <cRec>
    <opt />
  </cRec>
</root>"
                                      , @"<root></root>");

            IDSFDataObject result = ExecuteProcess();

            const string expected = "";
            string error;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "scalar", out actual, out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        //2013.02.11: Ashley Lewis - Bug 8725, Task 8794+Task 8835+Task 8830
        [TestMethod]
        public void MultiAssignWithAppendCalculationToSameBlankRecordSetAndBlankIndexExpectedValueInFirst()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", GlobalConstants.CalculateTextConvertPrefix + "sum([[cRec().opt]])+1" + GlobalConstants.CalculateTextConvertSuffix, _fieldCollection.Count));

            SetupArguments(
                                        @"<root>
  <cRec>
    <opt />
  </cRec>
</root>"
                                      , @"<root><cRec><opt>0</opt></cRec></root>");

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "0", "1" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        //2013.02.11: Ashley Lewis - Bug 8725, Task 8794+Task 8835+Task 8830
        [TestMethod]
        public void MutiAssignWithAppendCalculationToSameBlankRecordSetAndStaredIndexExpectedValueInFirst()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", GlobalConstants.CalculateTextConvertPrefix + "sum([[cRec(*).opt]])+1" + GlobalConstants.CalculateTextConvertSuffix, _fieldCollection.Count));

            SetupArguments(
                                        @"<root>
  <cRec>
    <opt />
    <display />
  </cRec>
</root>"
                                      , @"<root></root>");

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
        }

        //2013.02.08: Ashley Lewis - Bug 8725, Task 8833
        [TestMethod]
        public void MutiAssignWithImpliedConcatenationExpectedCorrectSetsScalarValue()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[var]]", "var", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[var]]", "[[var]]iable", _fieldCollection.Count));

            SetupArguments(
                                        @"<root>
  <var />
</root>"
                                      , @"<root></root>");

            IDSFDataObject result = ExecuteProcess();

            const string expected = "variable";
            string error;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "var", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        //2013.02.08: Ashley Lewis - Bug 8725, Task 8834
        [TestMethod]
        public void MutiAssignWithExplicitConcatenationExpectedCorrectSetsScalarValue()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[var]]", GlobalConstants.CalculateTextConvertPrefix + "concatenate(\"variable\", \"variable\")" + GlobalConstants.CalculateTextConvertSuffix, _fieldCollection.Count));

            SetupArguments(
                                        @"<root>
  <var />
</root>"
                                      , @"<root></root>");

            IDSFDataObject result = ExecuteProcess();

            const string expected = "variablevariable";
            string error;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "var", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        #endregion MultiAssign Functionality Tests

        #region Language Tests

        [TestMethod]
        public void StarToStar_Expected_AllValuesOverwrittenWithRecordSetFrom()
        {

            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[gRec(*).opt]]", "[[cRec(*).opt]]", _fieldCollection.Count));

            const string shape = @"<ADL>
<gRec>
<opt></opt>
<display></display>
</gRec>
<cRec>
<opt/>
<display/>
</cRec>
</ADL>";

            SetupArguments(
                shape
              , ActivityStrings.MutiAssignStarDataList);

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string>{ "Value1"
                                                    , "Value2"
                                                    , "Value3"
                                                    , "Value4"
                                                    , "Value5"
                                                    , "Value6"
                                                    , "Value7"
                                                    , "Value8"
                                                    , "Value9"
                                                    , "Value10" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Star_To_NoIndex_Expected_ValuesAppendedToRecordSet()
        {

            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[gRec().opt]]", "[[cRec(*).opt]]", _fieldCollection.Count));

            const string shape = @"<ADL>
<gRec>
<opt></opt>
<display></display>
</gRec>
<cRec>
<opt/>
<display/>
</cRec>
</ADL>";

            SetupArguments(
                            shape
                          , ActivityStrings.MutiAssignStarDataList);

            IDSFDataObject result = ExecuteProcess();
            string error;
            IList<string> data = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(17, data.Count);
            Assert.AreEqual("Value1", data[7]);
            Assert.AreEqual("Value10", data[16]);

        }

        [TestMethod]
        public void NoIndex_To_Star_LastValueOverwritesAllCurrentDataListValues()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[gRec(*).opt]]", "[[cRec().opt]]", _fieldCollection.Count));

            const string dl = @"<ADL>
  <cRec>
    <opt></opt>
    <display />
  </cRec>
    <gRec>
    <opt></opt>
    <display></display>
  </gRec>
</ADL>";

            SetupArguments(
                dl
              , "<root>" + ActivityStrings.MutiAssignStarDataList + "</root>");

            IDSFDataObject result = ExecuteProcess();

            string error;
            IList<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(7, actual.Count);
            Assert.AreEqual("Value10", actual[6]);
            Assert.AreEqual("Value10", actual[0]);
        }

        [TestMethod]
        public void Index_To_Star_Expected_AllValuesOverwrittenByIndexValue()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[gRec(*).opt]]", "[[cRec(2).opt]]", _fieldCollection.Count));


            const string dl = @"<ADL>
  <cRec>
    <opt></opt>
    <display />
  </cRec>
  <gRec>
    <opt></opt>
    <display></display>
  </gRec>
</ADL>";

            SetupArguments(
                            dl
                          , "<root>" + ActivityStrings.MutiAssignStarDataList + "</root>");
            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string>{ "Value2"
                                                    , "Value2"
                                                    , "Value2"
                                                    , "Value2"
                                                    , "Value2"
                                                    , "Value2"
                                                    , "Value2" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void Star_To_Index_Expected_IndexSetToLastValueOfAssignedRecordSet()
        {

            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[gRec(2).opt]]", "[[cRec(*).opt]]", _fieldCollection.Count));

            const string shape = @"<ADL>
<gRec>
<opt></opt>
<display></display>
</gRec>
<cRec>
<opt/>
<display/>
</cRec>
</ADL>";

            SetupArguments(
                            shape
                          , ActivityStrings.MutiAssignStarDataList);

            IDSFDataObject result = ExecuteProcess();

            const string expected = "Value10";
            string error;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error)[1];

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void StarToScalar_Expected_ScalarSetToLastValueInRecordSet()
        {

            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[testScalar]]", "[[cRec(*).opt]]", _fieldCollection.Count));
            SetupArguments(
                            @"<root>
  <cRec>
    <opt></opt>
    <display />
  </cRec>
    <gRec>
    <opt></opt>
    <display></display>
  </gRec>
  <testScalar/>
</root>"
                          , "<root>" + ActivityStrings.MultiAssignStarDataListWithScalar + "</root>");
            IDSFDataObject result = ExecuteProcess();

            const string expected = "Value10";
            string error;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "testScalar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ScalarToStar_Expected_AllRecordsOverwrittenWithScalarValue()
        {

            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[testScalar]]", "testData", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[gRec(*).opt]]", "[[testScalar]]", _fieldCollection.Count));

            TestStartNode = new FlowStep
            {
                Action = new DsfMultiAssignActivity { OutputMapping = null, FieldsCollection = _fieldCollection }
            };

            SetupArguments(
                ActivityStrings.MultiAssignStarDataListWithScalar
              , ActivityStrings.MultiAssignStarDataListWithScalar);
            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string>{ "testData"
                                                    , "testData"
                                                    , "testData"
                                                    , "testData"
                                                    , "testData"
                                                    , "testData"
                                                    , "testData" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void RecursiveEvaluateRecordset_WhenDataContainsQuotes_ShouldEvaluateWithoutExtraEscaping()
        {

            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[gRec(1).opt]]", "\"testData\"", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[gRec(2).opt]]", "some value [[gRec(1).opt]] another", _fieldCollection.Count));

            TestStartNode = new FlowStep
            {
                Action = new DsfMultiAssignActivity { OutputMapping = null, FieldsCollection = _fieldCollection }
            };

            SetupArguments(
                ActivityStrings.MultiAssignStarDataListWithScalar
              , ActivityStrings.MultiAssignStarDataListWithScalar);
            IDSFDataObject result = ExecuteProcess();
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            Assert.AreEqual("some value \"testData\" another", actual[1]);
        }

        [TestMethod]
        public void RecursiveEvaluateScalar_WhenDataContainsQuotes_ShouldEvaluateWithoutExtraEscaping()
        {

            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[testScalar]]", "\"testData\"", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[testScalar]]", "some value [[testScalar]] another", _fieldCollection.Count));

            TestStartNode = new FlowStep
            {
                Action = new DsfMultiAssignActivity { OutputMapping = null, FieldsCollection = _fieldCollection }
            };

            SetupArguments(
                ActivityStrings.MultiAssignStarDataListWithScalar
              , ActivityStrings.MultiAssignStarDataListWithScalar);
            IDSFDataObject result = ExecuteProcess();
            string error;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "testScalar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            Assert.AreEqual("some value \"testData\" another", actual);
        }

        [TestMethod]
        public void ValueToInvalidRecordsetIndexExpectedError()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec(xx).opt]]", "testData", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("", "", _fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.MultiAssignStarDataListWithScalar
                          , ActivityStrings.MultiAssignStarDataListWithScalar);

            IDSFDataObject result = ExecuteProcess();

            const string expected = "<InnerError>Recordset index (xx) contains invalid character(s)</InnerError>";

            string error;
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual, "Assigning to an invalid recordset index did not return an error");
        }

        //2013.05.31: Ashley Lewis for bug 9379 - display errors for recset indexs
        [TestMethod]
        public void ValueToInvalidRecordsetIndexAndSquareBracesAroundIndexExpectedError()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec([xx]).opt]]", "testData", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("", "", _fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.MultiAssignStarDataListWithScalar
                          , ActivityStrings.MultiAssignStarDataListWithScalar);

            IDSFDataObject result = ExecuteProcess();

            const string expected = "<InnerError>Recordset index ([xx]) contains invalid character(s)</InnerError>";
            string error;
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual, "Assigning to an invalid recordset index did not return an error");
        }
        [TestMethod]
        public void ValueToInvalidRecordsetIndexAndDoubleOpenningSquareBracesAroundIndexExpectedError()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec([[xx]).opt]]", "testData", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("", "", _fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.MultiAssignStarDataListWithScalar
                          , ActivityStrings.MultiAssignStarDataListWithScalar);

            IDSFDataObject result = ExecuteProcess();

            const string expected = "<InnerError>Invalid region detected: An open [[ without a related close ]]</InnerError><InnerError>Invalid Data : Either empty expression or empty token list. Please check that your variable list does not contain errors.</InnerError><InnerError>Invalid Region [[cRec([[xx]).opt]]</InnerError>";
            string error;
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual, "Assigning to an invalid recordset index did not return an error");
        }
        [TestMethod]
        public void ValueToInvalidRecordsetIndexAndTwoBrokenIndexRegionsExpectedError()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec([[xx]][aa]]).opt]]", "testData", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("", "", _fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.MultiAssignStarDataListWithScalar
                          , ActivityStrings.MultiAssignStarDataListWithScalar);

            IDSFDataObject result = ExecuteProcess();

            const string expected = "<InnerError>Invalid region detected: A close ]] without a related open [[</InnerError><InnerError>Invalid Data : Either empty expression or empty token list. Please check that your variable list does not contain errors.</InnerError><InnerError>Invalid Region [[cRec([[xx]][aa]]).opt]]</InnerError>";
            string error;
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual, "Assigning to an invalid recordset index did not return an error");
        }

        #endregion Language Tests

        #region Calculate Mode Tests

        [TestMethod]
        public void MutiAssign_CalculateMode_PrefixEncasing_Test()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[Variable]]", DsfMultiAssignActivity.CalculateTextConvertPrefix + "sum(5,10)", _fieldCollection.Count));

            SetupArguments(
                            "<ADL><Variable></Variable></ADL>"
                          , "<ADL><Variable></Variable></ADL>");

            IDSFDataObject result = ExecuteProcess();
            const string expected = DsfMultiAssignActivity.CalculateTextConvertPrefix + "sum(5,10)";
            string actual;
            string error;

            GetScalarValueFromDataList(result.DataListID, "Variable", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MutiAssign_CalculateMode_SuffixEncasing_Test()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[Variable]]", "sum(5)" + DsfMultiAssignActivity.CalculateTextConvertSuffix, _fieldCollection.Count));

            SetupArguments(
                            "<ADL><Variable></Variable></ADL>"
                          , "<ADL><Variable></Variable></ADL>");

            IDSFDataObject result = ExecuteProcess();
            const string expected = "sum(5)" + DsfMultiAssignActivity.CalculateTextConvertSuffix;
            string actual;
            string error;

            GetScalarValueFromDataList(result.DataListID, "Variable", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        // changed this test to faciliate the existing evaluate approach, hence the expected is now "" not 5
        [TestMethod]
        public void MutiAssign_CalculateMode_ValidEncasing_Test()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[Variable]]", String.Format(DsfMultiAssignActivity.CalculateTextConvertFormat, "sum(5)"), _fieldCollection.Count));

            SetupArguments(
                            "<ADL><Variable></Variable></ADL>"
                          , "<root><Variable></Variable></root>");

            IDSFDataObject result = ExecuteProcess();
            const string expected = "5";
            string actual;
            string error;

            GetScalarValueFromDataList(result.DataListID, "Variable", out actual, out error);

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
            Assert.IsFalse(res);
        }

        [TestMethod]
        public void MutiAssign_ErrorHandeling_Expected_ErrorTag()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[//().rec]]", "testData", _fieldCollection.Count));

            SetupArguments(
                            "<ADL><Variable></Variable></ADL>"
                          , "<ADL><Variable></Variable></ADL>");

            IDSFDataObject result = ExecuteProcess();

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
        }

        #endregion Calculate Mode Tests

        #region ForEach Update/Get Inputs/Outputs

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfMultiAssignActivity_UpdateForEachInputs")]
        public void DsfMultiAssignActivity_UpdateForEachInputs_WhenContainsMatchingStarAndOtherData_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<ActivityDTO> fieldsCollection = new List<ActivityDTO>
            {
                new ActivityDTO("[[result]]", "[[rs(*).val]] [[result]]", 1),
            };

            DsfMultiAssignActivity act = new DsfMultiAssignActivity { FieldsCollection = fieldsCollection };

            //------------Execute Test---------------------------

            act.UpdateForEachInputs(new List<Tuple<string, string>>
            {
                new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
            }, null);

            //------------Assert Results-------------------------

            var collection = act.FieldsCollection;

            Assert.AreEqual("[[rs(1).val]] [[result]]", collection[0].FieldValue);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfMultiAssignActivity_UpdateForEachInputs")]
        public void DsfMultiAssignActivity_UpdateForEachInputs_WhenContainsMatchingStar_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<ActivityDTO> fieldsCollection = new List<ActivityDTO>
            {
                new ActivityDTO("[[result]]", "[[rs(*).val]]", 1),
            };

            DsfMultiAssignActivity act = new DsfMultiAssignActivity { FieldsCollection = fieldsCollection };

            //------------Execute Test---------------------------

            act.UpdateForEachInputs(new List<Tuple<string, string>>
            {
                new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
            }, null);

            //------------Assert Results-------------------------

            var collection = act.FieldsCollection;

            Assert.AreEqual("[[rs(1).val]]", collection[0].FieldValue);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfMultiAssignActivity_UpdateForEachOutputs")]
        public void DsfMultiAssignActivity_UpdateForEachOutputs_WhenContainsMatchingStar_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<ActivityDTO> fieldsCollection = new List<ActivityDTO>
            {
                new ActivityDTO("[[rs(*).val]]", "abc", 1),
            };

            DsfMultiAssignActivity act = new DsfMultiAssignActivity { FieldsCollection = fieldsCollection };

            //------------Execute Test---------------------------

            act.UpdateForEachOutputs(new List<Tuple<string, string>>
            {
                new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
            }, null);

            //------------Assert Results-------------------------

            var collection = act.FieldsCollection;

            Assert.AreEqual("[[rs(1).val]]", collection[0].FieldName);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfMultiAssignActivity_GetForEachInputs")]
        public void DsfMultiAssignActivity_GetForEachInputs_Normal_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<ActivityDTO> fieldsCollection = new List<ActivityDTO>
            {
                new ActivityDTO("[[rs(*).val]]", "[[result]]", 1),
            };

            DsfMultiAssignActivity act = new DsfMultiAssignActivity { FieldsCollection = fieldsCollection };

            //------------Execute Test---------------------------

            var inputs = act.GetForEachInputs();

            //------------Assert Results-------------------------

            Assert.AreEqual("[[rs(*).val]]", inputs[0].Name);
            Assert.AreEqual("[[result]]", inputs[0].Value);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfMultiAssignActivity_GetForEachOutputs")]
        public void DsfMultiAssignActivity_GetForEachOutputs_Normal_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<ActivityDTO> fieldsCollection = new List<ActivityDTO>
            {
                new ActivityDTO("[[rs(*).val]]", "[[result]]", 1),
            };

            DsfMultiAssignActivity act = new DsfMultiAssignActivity { FieldsCollection = fieldsCollection };

            //------------Execute Test---------------------------

            var inputs = act.GetForEachOutputs();

            //------------Assert Results-------------------------

            Assert.AreEqual("[[rs(*).val]]", inputs[0].Value);
            Assert.AreEqual("[[result]]", inputs[0].Name);
        }

        #endregion

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string outputMapping = null)
        {
            if(outputMapping == null)
            {
                TestStartNode = new FlowStep
                {
                    Action = new DsfMultiAssignActivity { OutputMapping = null, FieldsCollection = _fieldCollection }
                };
            }
            else
            {
                TestStartNode = new FlowStep
                {
                    Action = new DsfMultiAssignActivity { OutputMapping = outputMapping, FieldsCollection = _fieldCollection }
                };
            }
            TestData = testData;
            CurrentDl = currentDL;
        }

        #endregion
    }
}
