
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes


        #endregion

        #region MultiAssign Functionality Tests

        [TestMethod]
        public void AssignRecordSetWithEvaluatedSingleExpression()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec().opt]]", fieldCollection.Count));
            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , fieldCollection);
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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[recset]]", "gRec", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[field]]", "opt", fieldCollection.Count));

            fieldCollection.Add(new ActivityDTO("[[cRec(1).opt]]", "[[[[recset]]().[[field]]]]", fieldCollection.Count));
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
</DataList>"
                          , fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec().opt]]", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[cRec().display]]", "[[gRec().display]]", fieldCollection.Count));

            const string data = "<ADL><gRec><opt>Value1</opt><display>display1</display></gRec></ADL>";

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , "<root>" + data + "</root>", fieldCollection);
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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec().opt]]", fieldCollection.Count));

            const string data = "<root><gRec><opt>Value1</opt><display>display1</display></gRec></root>";

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , data, fieldCollection);
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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec().opt]]", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[cRec().display]]", "[[gRec().display]]", fieldCollection.Count));

            const string data = "<ADL><gRec><opt>Value1</opt><display>display1</display></gRec></ADL>";

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , "<root>" + data + "</root>", fieldCollection);
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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[rsElement]]", "gRec", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[rsFieldElement]]", "opt", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[gRec(1).opt]]", "Value1", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[gRec(1).display]]", "display1", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[gRec(2).opt]]", "Value2", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[gRec(2).display]]", "display2", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[[[rsElement]]().[[rsFieldElement]]]]", fieldCollection.Count));

            const string data = "<root><rsElement></rsElement><rsFieldElement></rsFieldElement><gRec><opt></opt><display></display></gRec></root>";

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_mult_eval_fields_rs_adl
                          , data, fieldCollection);
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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec(100).opt]]", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                         , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl, fieldCollection);
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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec(100).opt]]", "[[gRec().opt]]", fieldCollection.Count));

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
                          , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl, fieldCollection);
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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec(100).opt]]", "[[gRec(100).opt]]", fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl, fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec(100).opt]]", "[[gRec().opt]]", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[cRec(100).display]]", "[[gRec().display]]", fieldCollection.Count));

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
                          , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl, fieldCollection);
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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec(3).opt]]", "[[gRec(3).opt]]", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[cRec(3).display]]", "[[gRec(3).display]]", fieldCollection.Count));

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
</ADL>"
                          , fieldCollection);
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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec(100).opt]]", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[cRec().display]]", "[[gRec(100).display]]", fieldCollection.Count));
            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , fieldCollection);
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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec(1).opt]]", "[[gRec().opt]]", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[cRec(1).display]]", "[[gRec().display]]", fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , fieldCollection);
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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[testName1]]","bob",fieldCollection.Count));
            SetupArguments(
                            ActivityStrings.NewScalarShape
                          , ActivityStrings.NewScalarShape
                          , fieldCollection);
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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec(1).opt]]", "[[gRec().opt]]", fieldCollection.Count));
            fieldCollection[0].FieldName = "";

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[testValue1]]", "bob", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[testName1]]", "jim", fieldCollection.Count));
            fieldCollection[0].FieldValue = "";
            fieldCollection[1].FieldValue = "";
            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[testValue1]]","somevalue",fieldCollection.Count));
            fieldCollection[0].FieldValue = "testValue@#";

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[testRecSet1().testRec1]]", "testRecValue1", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("", "", fieldCollection.Count));
            SetupArguments(
                            ActivityStrings.recsetDataListShape
                          , "<root></root>"
                          , fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[testRecSet1(10).testRec1]]", "testRecValue1", fieldCollection.Count));

            SetupArguments(
                                        ActivityStrings.recsetDataListShape
                                      , "<root>" + ActivityStrings.recsetDataListShape + "</root>"
                                      , fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[testRecSet1(1).testRec1]]", "testRecValue1", fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.recsetDataListShape
                          , ActivityStrings.recsetDataListShape
                          , fieldCollection);

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

            var fieldCollection = new ObservableCollection<ActivityDTO>();

            fieldCollection.Add(new ActivityDTO("[[a]]", "1", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[b]]", "2", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[c]]", "abctest", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[recset().a]]", "abc123", fieldCollection.Count));

            SetupArguments(
                ActivityStrings.scalar_in_recordset_adl
              , "<root></root>"
              , fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            const string expected = "abc123";
            string error;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "a", out error).First();

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void AssignRecordSetWithNoIndexAndJustOneExistingRecordExpectedRecordAppended()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "New Value", fieldCollection.Count));

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
</root>"
                          , fieldCollection);
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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "New Value", fieldCollection.Count));

            SetupArguments(
                            @"<root>
  <cRec>
    <opt />
    <display />
  </cRec>
</root>"
                          , @"<root></root>"
                          , fieldCollection);
            IDSFDataObject result = ExecuteProcess();
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual("New Value", actual[0]);
        }

        [TestMethod]
        public void MutiAssignWithAddingTenRecSetsExpectedRecordSetAppended()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec(10).opt]]", "testRecValue1", fieldCollection.Count));

            SetupArguments(
                                        @"<root>
  <cRec>
    <opt />
  </cRec>
</root>"
                                      , "<root></root>"
                                      , fieldCollection);

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

        [TestMethod]
        public void MutiAssignWithCalculationOnBlankRecordSetExpectedCalculationReplacesBlankWithZero()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[scalar]]", GlobalConstants.CalculateTextConvertPrefix + "sum([[cRec().opt]])+1" + GlobalConstants.CalculateTextConvertSuffix, fieldCollection.Count));

            SetupArguments(
                                        @"<root>
  <scalar />
  <cRec>
    <opt />
  </cRec>
</root>"
                                      , @"<root></root>"
                                      , fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            const string expected = "";
            string error;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "scalar", out actual, out error);

            // remove test datalist
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MultiAssignWithAppendCalculationToSameBlankRecordSetAndBlankIndexExpectedValueInFirst()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", GlobalConstants.CalculateTextConvertPrefix + "sum([[cRec().opt]])+1" + GlobalConstants.CalculateTextConvertSuffix, fieldCollection.Count));

            SetupArguments(
                                        @"<root>
  <cRec>
    <opt />
  </cRec>
</root>"
                                      , @"<root><cRec><opt>0</opt></cRec></root>"
                                      , fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "0", "1" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void MutiAssignWithAppendCalculationToSameBlankRecordSetAndStaredIndexExpectedValueInFirst()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", GlobalConstants.CalculateTextConvertPrefix + "sum([[cRec(*).opt]])+1" + GlobalConstants.CalculateTextConvertSuffix, fieldCollection.Count));

            SetupArguments(
                                        @"<root>
  <cRec>
    <opt />
    <display />
  </cRec>
</root>"
                                      , @"<root></root>"
                                      , fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
        }

        [TestMethod]
        public void MutiAssignWithImpliedConcatenationExpectedCorrectSetsScalarValue()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[var]]", "var", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[var]]", "[[var]]iable", fieldCollection.Count));

            SetupArguments(
                                        @"<root>
  <var />
</root>"
                                      , @"<root></root>"
                                      , fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            const string expected = "variable";
            string error;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "var", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MutiAssignWithExplicitConcatenationExpectedCorrectSetsScalarValue()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[var]]", GlobalConstants.CalculateTextConvertPrefix + "concatenate(\"variable\", \"variable\")" + GlobalConstants.CalculateTextConvertSuffix, fieldCollection.Count));

            SetupArguments(
                                        @"<root>
  <var />
</root>"
                                      , @"<root></root>"
                                      , fieldCollection);

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

            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[gRec(*).opt]]", "[[cRec(*).opt]]", fieldCollection.Count));

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
              , ActivityStrings.MutiAssignStarDataList
              , fieldCollection);

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

            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[gRec().opt]]", "[[cRec(*).opt]]", fieldCollection.Count));

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
                          , ActivityStrings.MutiAssignStarDataList
                          , fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[gRec(*).opt]]", "[[cRec().opt]]", fieldCollection.Count));

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
              , "<root>" + ActivityStrings.MutiAssignStarDataList + "</root>"
              , fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[gRec(*).opt]]", "[[cRec(2).opt]]", fieldCollection.Count));


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
                          , "<root>" + ActivityStrings.MutiAssignStarDataList + "</root>"
                          , fieldCollection);
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

            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[gRec(2).opt]]", "[[cRec(*).opt]]", fieldCollection.Count));

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
                          , ActivityStrings.MutiAssignStarDataList
                          , fieldCollection);

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

            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[testScalar]]", "[[cRec(*).opt]]", fieldCollection.Count));
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
                          , "<root>" + ActivityStrings.MultiAssignStarDataListWithScalar + "</root>"
                          , fieldCollection);
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

            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[testScalar]]", "testData", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[gRec(*).opt]]", "[[testScalar]]", fieldCollection.Count));

            TestStartNode = new FlowStep
            {
                Action = new DsfMultiAssignActivity { OutputMapping = null, FieldsCollection = fieldCollection }
            };

            SetupArguments(
                ActivityStrings.MultiAssignStarDataListWithScalar
              , ActivityStrings.MultiAssignStarDataListWithScalar
              , fieldCollection);
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

            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[gRec(1).opt]]", "\"testData\"", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[gRec(2).opt]]", "some value [[gRec(1).opt]] another", fieldCollection.Count));

            TestStartNode = new FlowStep
            {
                Action = new DsfMultiAssignActivity { OutputMapping = null, FieldsCollection = fieldCollection }
            };

            SetupArguments(
                ActivityStrings.MultiAssignStarDataListWithScalar
              , ActivityStrings.MultiAssignStarDataListWithScalar
              , fieldCollection);
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

            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[testScalar]]", "\"testData\"", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[testScalar]]", "some value [[testScalar]] another", fieldCollection.Count));

            TestStartNode = new FlowStep
            {
                Action = new DsfMultiAssignActivity { OutputMapping = null, FieldsCollection = fieldCollection }
            };

            SetupArguments(
                ActivityStrings.MultiAssignStarDataListWithScalar
              , ActivityStrings.MultiAssignStarDataListWithScalar
              , fieldCollection);
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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec(xx).opt]]", "testData", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("", "", fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.MultiAssignStarDataListWithScalar
                          , ActivityStrings.MultiAssignStarDataListWithScalar
                          , fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            const string expected = "<InnerError>Recordset index (xx) contains invalid character(s)</InnerError>";

            string error;
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual, "Assigning to an invalid recordset index did not return an error");
        }

        [TestMethod]
        public void ValueToInvalidRecordsetIndexAndSquareBracesAroundIndexExpectedError()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec([xx]).opt]]", "testData", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("", "", fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.MultiAssignStarDataListWithScalar
                          , ActivityStrings.MultiAssignStarDataListWithScalar
                          , fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec([[xx]).opt]]", "testData", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("", "", fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.MultiAssignStarDataListWithScalar
                          , ActivityStrings.MultiAssignStarDataListWithScalar
                          , fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[cRec([[xx]][aa]]).opt]]", "testData", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("", "", fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.MultiAssignStarDataListWithScalar
                          , ActivityStrings.MultiAssignStarDataListWithScalar
                          , fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[Variable]]", DsfMultiAssignActivity.CalculateTextConvertPrefix + "sum(5,10)", fieldCollection.Count));

            SetupArguments(
                            "<ADL><Variable></Variable></ADL>"
                          , "<ADL><Variable></Variable></ADL>"
                          , fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[Variable]]", "sum(5)" + DsfMultiAssignActivity.CalculateTextConvertSuffix, fieldCollection.Count));

            SetupArguments(
                            "<ADL><Variable></Variable></ADL>"
                          , "<ADL><Variable></Variable></ADL>"
                          , fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[Variable]]", String.Format(DsfMultiAssignActivity.CalculateTextConvertFormat, "sum(5)"), fieldCollection.Count));

            SetupArguments(
                            "<ADL><Variable></Variable></ADL>"
                          , "<root><Variable></Variable></root>"
                          , fieldCollection);

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
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[//().rec]]", "testData", fieldCollection.Count));

            SetupArguments(
                            "<ADL><Variable></Variable></ADL>"
                          , "<ADL><Variable></Variable></ADL>"
                          , fieldCollection);

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

        private void SetupArguments(string currentDL, string testData,ObservableCollection<ActivityDTO> fieldCollection, string outputMapping = null)
        {
            if(outputMapping == null)
            {
                TestStartNode = new FlowStep
                {
                    Action = new DsfMultiAssignActivity { OutputMapping = null, FieldsCollection = fieldCollection }
                };
            }
            else
            {
                TestStartNode = new FlowStep
                {
                    Action = new DsfMultiAssignActivity { OutputMapping = outputMapping, FieldsCollection = fieldCollection }
                };
            }
            TestData = testData;
            CurrentDl = currentDL;
        }

        #endregion
    }
}
