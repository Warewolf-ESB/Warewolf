using Dev2;
using Dev2.Common;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTest
{
    /// <summary>
    /// Summary description for AssignActivity
    /// </summary>
    [TestClass]
    public class MultiAssignActivity : BaseActivityUnitTest
    {
        IList<string> _fieldName;
        IList<string> _fieldValue;
        ObservableCollection<ActivityDTO> _fieldCollection = new ObservableCollection<ActivityDTO>();
        public MultiAssignActivity()
            : base()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
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


            for (int i = 0; i < _fieldName.Count; i++)
            {
                _fieldCollection.Add(new ActivityDTO(_fieldName[i], _fieldValue[i], _fieldCollection.Count));
            }


        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region MultiAssign Functionality Tests

        [TestMethod]
        public void AssignRecordSetWithEvaluatedSingleExpression()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec().opt]]", _fieldCollection.Count));
            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , _fieldCollection
                          );
            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = "Value1";
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error).FirstOrDefault();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RecursiveEvaluation()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[recset]]", "gRec", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[field]]", "opt", _fieldCollection.Count));

            _fieldCollection.Add(new ActivityDTO("[[cRec(1).opt]]", "[[[[recset]]().[[field]]]]", _fieldCollection.Count));
            string adl = GetSimpleADL();
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
  <recset></recset>
  <field></field>
</root>"
                          , @"<root>
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
</root>"
                          , _fieldCollection
                          );

            IDSFDataObject result = ExecuteProcess();

            string expected = "Value1";
            string error = string.Empty;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error).First();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedDoubleExpressionSameRecordSet()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec().opt]]", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[cRec().display]]", "[[gRec().display]]", _fieldCollection.Count));

            var data = "<ADL><gRec><opt>Value1</opt><display>display1</display></gRec></ADL>";

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , "<root>" + data +"</root>"
                          , _fieldCollection
                          );
            IDSFDataObject result = ExecuteProcess();

            string expected = "display1";
            string error = string.Empty;
            List<string> vals = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "display", out error);
            string actual = vals[0];

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedSingleExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec().opt]]", _fieldCollection.Count));

            var data = "<root><gRec><opt>Value1</opt><display>display1</display></gRec></root>";

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , data
                          , _fieldCollection
                          );
            IDSFDataObject result = ExecuteProcess();
            string expected = "Value1";
            string error = string.Empty;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error).First();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedDoubleExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec().opt]]", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[cRec().display]]", "[[gRec().display]]", _fieldCollection.Count));

            var data = "<ADL><gRec><opt>Value1</opt><display>display1</display></gRec></ADL>";

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , "<root>" + data + "</root>"
                          , _fieldCollection
                          );
            IDSFDataObject result = ExecuteProcess();
            string expected = "display1";

            string error = string.Empty;
            //IBinaryDataList = _compiler.FetchBinaryDataList(result.DataListID, out error);

            IList<string> vals = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "display", out error);
            string actual = vals[0];

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

            var data = "<root><rsElement></rsElement><rsFieldElement></rsFieldElement><gRec><opt></opt><display></display></gRec></root>";

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_mult_eval_fields_rs_adl
                          , data
                          , _fieldCollection
                          );
            IDSFDataObject result = ExecuteProcess();
            string expected = "Value2";
            string error = string.Empty;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error).First();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedSingleOutOfBoundRightIndexExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec(100).opt]]", _fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                         , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                         , _fieldCollection
                         );
            IDSFDataObject result = ExecuteProcess();
            string error = string.Empty;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error).First();

            Assert.IsTrue(actual == string.Empty);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedSingleOutOfBoundLeftIndexExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec(100).opt]]", "[[gRec().opt]]", _fieldCollection.Count));
            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , _fieldCollection
                          );
            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            List<string> resultCollection = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            // first and row 100
            Assert.IsTrue(resultCollection.Count == 2 && resultCollection[1] == "Value2");
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedSingleOutOfBoundBothIndexExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec(100).opt]]", "[[gRec(100).opt]]", _fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , _fieldCollection
                          );

            IDSFDataObject result = ExecuteProcess();
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            // two cols at row 100
            Assert.IsTrue(actual.Count == 2 && actual.First() == string.Empty);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedMultOutOfBoundLeftIndexExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec(100).opt]]", "[[gRec().opt]]", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[cRec(100).display]]", "[[gRec().display]]", _fieldCollection.Count));
            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , _fieldCollection
                          );
            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            // first and row 100
            Assert.IsTrue(actual.Count == 2 && actual.Last() == "Value2");
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedMultOutOfBoundBothIndexExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec(100).opt]]", "[[gRec(100).opt]]", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[cRec(100).display]]", "[[gRec(100).display]]", _fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , _fieldCollection
                          );
            IDSFDataObject result = ExecuteProcess();
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);
            // first and row 100
            Assert.IsTrue(actual.Count == 2 && actual.First() == string.Empty);
        }

        [TestMethod]
        public void AssignRecordSetWithEvaluatedMultOutOfBoundRightIndexExpressionMultRecords()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[cRec().opt]]", "[[gRec(100).opt]]", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[cRec().display]]", "[[gRec(100).display]]", _fieldCollection.Count));
            SetupArguments(
                            ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , ActivityStrings.mult_assign_expression_both_sides_mult_rs_adl
                          , _fieldCollection
                          );
            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error).First();

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
                          , ActivityStrings.mult_assign_expression_both_sides_single_rs_adl
                          , _fieldCollection
                          );
            IDSFDataObject result = ExecuteProcess();
            string expected = "display1";
            string error = string.Empty;

            List<string> vals = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "display", out error);
            string actual = vals[0];

            Assert.AreEqual(expected, actual);
        }

        // -- End New

        [TestMethod]
        public void MultiAssignTenAtOnce_Expected_MultiAssignCorrectlySetsAllScalarValues()
        {
            SetupArguments(
                            ActivityStrings.NewScalarShape
                          , ActivityStrings.NewScalarShape
                          , _fieldCollection
                          );
            IDSFDataObject result = ExecuteProcess();
            string expected = "bob";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "testName1", out actual, out error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MultiAssignWithAnEmptyField_Expected_FieldInDataListNotAssignedValue()
        {
            _fieldCollection[0].FieldName = "";

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "testValue1", out actual, out error);

            Assert.IsTrue(actual == string.Empty);
        }

        [TestMethod]
        public void MultiAssignWithAnEmptyValue_Expected_FieldInDataListNotAssignedValue()
        {
            _fieldCollection[0].FieldValue = "";
            _fieldCollection[1].FieldValue = "";
            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "testValue1", out actual, out error);

            Assert.IsTrue(actual == string.Empty);
        }


        // Sashen: 14-11-2012 - Not sure if this case is required as the datalist does not contain this field
        //                      It seems covered by a field that does not exist in the datalist
        //        [TestMethod]
        //        public void MultiAssignWithSpecialCharsInFeild() {
        //            _fieldCollection[0].FieldName = "testName@#";
        //            TestStartNode = new FlowStep {
        //                Action = new DsfMultiAssignActivity { OutputMapping = null, FieldsCollection = _fieldCollection }
        //            };

        //            TestData = ActivityStrings.scalarShape;

        //            UnlimitedObject result = ExecuteProcess();

        //            string expected = @"<ADL>
        //  <fname></fname>
        //  <lname></lname>
        //  <testValue1></testValue1>
        //  <testName1></testName1>
        //  <testName2>testValue2</testName2>
        //  <testName3>testValue3</testName3>
        //  <testName4>testValue4</testName4>
        //  <testName5>testValue5</testName5>
        //  <testName6>testValue6</testName6>
        //  <testName7>testValue7</testName7>
        //  <testName8>testValue8</testName8>
        //  <testName9>testValue9</testName9>
        //  <testName10>testValue10</testName10>
        //</ADL>";

        //            Assert.AreEqual(expected, result.XmlString);
        //        }

        [TestMethod]
        public void MultiAssignWithSpecialCharsInValue()
        {
            _fieldCollection[0].FieldValue = "testValue@#";

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();
            string expected = "testValue@#";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "testValue1", out actual, out error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MutiAssignWithAddingOneRecSets()
        {
            _fieldCollection.Add(new ActivityDTO("[[testRecSet1().testRec1]]", "testRecValue1", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("", "", _fieldCollection.Count));
            SetupArguments(
                            ActivityStrings.recsetDataListShape
                          , "<root></root>"
                         , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            string expected = "testRecValue1";
            string error = string.Empty;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "testRecSet1", "testRec1", out error).First();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MutiAssignWithAddingTenRecSets_Expected_RecordSetPopulatedToIndex()
        {
            _fieldCollection.Add(new ActivityDTO("[[testRecSet1(10).testRec1]]", "testRecValue1", _fieldCollection.Count));

            SetupArguments(
                                        ActivityStrings.recsetDataListShape
                                      , "<root>" + ActivityStrings.recsetDataListShape + "</root>"
                                     , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { ""
                                                     , "testRecValue1" };
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "testRecSet1", "testRec1", out error);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void MutiAssignWithEditingExistingRecSets_Expected_RecordSetDataOverwritten()
        {
            _fieldCollection.Add(new ActivityDTO("[[testRecSet1(1).testRec1]]", "testRecValue1", _fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.recsetDataListShape
                          , ActivityStrings.recsetDataListShape
                         , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            string expected = "testRecValue1";
            string error = string.Empty;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "testRecSet1", "testRec1", out error).First();

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
              , "<root></root>"
             , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            string expected = "abc123";
            string error = string.Empty;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "a", out error).First();

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
</root>"
                          , _fieldCollection
                         );
            IDSFDataObject result = ExecuteProcess();
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

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
                          , @"<root></root>"
                          , _fieldCollection
                         );
            IDSFDataObject result = ExecuteProcess();
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            Assert.AreEqual("New Value", actual[0]);
        }

        //2013.02.07: Ashley Lewis - Bug 8725:Task 8790
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
                                      , "<root></root>"
                                     , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            IList<IBinaryDataListItem> actual;
            GetRecordSetFieldValueFromDataList(result.DataListID, "cRec", "opt", out actual, out error);

            Assert.AreEqual("testRecValue1", actual.FirstOrDefault<IBinaryDataListItem>(c => c.DisplayValue == "cRec(10).opt").TheValue);

            /* to expose server datalist shape error uncomment these */
            //List<string> expected = new List<string> { "testRecValue1" };
            //List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);
            //CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
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
                                      , @"<root></root>"
                                     , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            string expected = "1";
            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "scalar", out actual, out error);

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
                                      , @"<root></root>"
                                     , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "1" };
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

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
                                      , @"<root></root>"
                                     , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "1" };
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "cRec", "opt", out error);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
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
                                      , @"<root></root>"
                                     , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            string expected = "variable";
            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "var", out actual, out error);

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
                                      , @"<root></root>"
                                     , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            string expected = "variablevariable";
            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "var", out actual, out error);

            Assert.AreEqual(expected, actual);
        }

        #endregion MultiAssign Functionality Tests

        #region Language Tests

        [TestMethod]
        public void StarToStar_Expected_AllValuesOverwrittenWithRecordSetFrom()
        {

            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[gRec(*).opt]]", "[[cRec(*).opt]]", _fieldCollection.Count));


            SetupArguments(
                ActivityStrings.MutiAssignStarDataList
              , ActivityStrings.MutiAssignStarDataList
             , _fieldCollection);

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
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Star_To_NoIndex_Expected_ValuesAppendedToRecordSet()
        {

            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[gRec().opt]]", "[[cRec(*).opt]]", _fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.MutiAssignStarDataList
                          , ActivityStrings.MutiAssignStarDataList
                          , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            IList<string> expected = new List<string> { "",
                                                        ""
            
                                                      };
            string error = string.Empty;
            //string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error).Last();

            IList<string> data = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error);

            Assert.AreEqual(17, data.Count);
            Assert.AreEqual("Value1", data[7]);
            Assert.AreEqual("Value10", data[16]);

        }

        [TestMethod]
        public void NoIndex_To_Star_LastValueOverwritesAllCurrentDataListValues()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[gRec(*).opt]]", "[[cRec().opt]]", _fieldCollection.Count));

            string dl = @"<ADL>
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
             , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string>{ "Value10"
                                                    , "Value10"
                                                    , "Value10"
                                                    , "Value10"
                                                    , "Value10"
                                                    , "Value10"
                                                    , "Value10"
                                                    , "Value10"
                                                    , "Value10"
                                                    , "Value10" };
            string error = string.Empty;
            IList<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error);

            Assert.AreEqual(7, actual.Count);
            Assert.AreEqual("Value10", actual[6]);
            Assert.AreEqual("Value10", actual[0]);
            //Assert.AreEqual(expected, actual); // This keeps throwing error about wrong format ?!?!
        }

        [TestMethod]
        public void Index_To_Star_Expected_AllValuesOverwrittenByIndexValue()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[gRec(*).opt]]", "[[cRec(2).opt]]", _fieldCollection.Count));


            string dl = @"<ADL>
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
                          , _fieldCollection);
            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string>{ "Value2"
                                                    , "Value2"
                                                    , "Value2"
                                                    , "Value2"
                                                    , "Value2"
                                                    , "Value2"
                                                    , "Value2" };
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void Star_To_Index_Expected_IndexSetToLastValueOfAssignedRecordSet()
        {

            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[gRec(2).opt]]", "[[cRec(*).opt]]", _fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.MutiAssignStarDataList
                          , ActivityStrings.MutiAssignStarDataList
                          , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            string expected = "Value10";
            string error = string.Empty;
            string actual = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error)[1];
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
                          , "<root>" + ActivityStrings.MultiAssignStarDataListWithScalar + "</root>"
                          , _fieldCollection);
            IDSFDataObject result = ExecuteProcess();

            string expected = "Value10";
            string error = string.Empty;
            string actual = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "testScalar", out actual, out error);

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
              , ActivityStrings.MultiAssignStarDataListWithScalar
              , _fieldCollection);
            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string>{ "testData"
                                                    , "testData"
                                                    , "testData"
                                                    , "testData"
                                                    , "testData"
                                                    , "testData"
                                                    , "testData" };
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "gRec", "opt", out error);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void RemoveItem_Expected_BlankItemsRemoved()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[testScalar]]", "testData", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("[[gRec(*).opt]]", "[[testScalar]]", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("", "", _fieldCollection.Count));
            _fieldCollection.Add(new ActivityDTO("", "", _fieldCollection.Count));

            SetupArguments(
                            ActivityStrings.MultiAssignStarDataListWithScalar
                          , ActivityStrings.MultiAssignStarDataListWithScalar
                          , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            DsfMultiAssignActivity act = TestStartNode.Action as DsfMultiAssignActivity;
            act.RemoveItem();

            Assert.AreEqual(3, act.FieldsCollection.Count);
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
                          , "<ADL><Variable></Variable></ADL>"
                          , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();
            string expected = DsfMultiAssignActivity.CalculateTextConvertPrefix + "sum(5,10)";
            string actual = string.Empty;
            string error = string.Empty;

            GetScalarValueFromDataList(result.DataListID, "Variable", out actual, out error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MutiAssign_CalculateMode_SuffixEncasing_Test()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[Variable]]", "sum(5)" + DsfMultiAssignActivity.CalculateTextConvertSuffix, _fieldCollection.Count));

            SetupArguments(
                            "<ADL><Variable></Variable></ADL>"
                          , "<ADL><Variable></Variable></ADL>"
                          , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();
            string expected = "sum(5)" + DsfMultiAssignActivity.CalculateTextConvertSuffix;
            string actual = string.Empty;
            string error = string.Empty;

            GetScalarValueFromDataList(result.DataListID, "Variable", out actual, out error);
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
                          , "<root><Variable></Variable></root>"
                          , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();
            string expected = "5";
            string actual = string.Empty;
            string error = string.Empty;

            GetScalarValueFromDataList(result.DataListID, "Variable", out actual, out error);
            Assert.AreEqual(expected, actual);
            Assert.IsFalse(Compiler.HasErrors(result.DataListID));
        }

        [TestMethod]
        public void MutiAssign_ErrorHandeling_Expected_ErrorTag()
        {
            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[//().rec]]", "testData", _fieldCollection.Count));

            SetupArguments(
                            "<ADL><Variable></Variable></ADL>"
                          , "<ADL><Variable></Variable></ADL>"
                          , _fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
        }

        #endregion Calculate Mode Tests

        #region GetWizardData Tests

        [TestMethod]
        public void GetWizardData_Expected_Correct_IBinaryDataList()
        {
            bool passTest = true;

            _fieldCollection.Clear();
            _fieldCollection.Add(new ActivityDTO("[[testScalar]]", "testData", 1));
            _fieldCollection.Add(new ActivityDTO("[[gRec(1).opt]]", "[[testScalar]]", 2));
            _fieldCollection.Add(new ActivityDTO("[[gRec(2).opt]]", "TestRecsetData", 3));
            _fieldCollection.Add(new ActivityDTO("", "", _fieldCollection.Count));


            DsfMultiAssignActivity testAct = new DsfMultiAssignActivity { FieldsCollection = _fieldCollection };

            IBinaryDataList binaryDL = testAct.GetWizardData();
            var recsets = binaryDL.FetchRecordsetEntries();
            if (recsets.Count != 1)
            {
                passTest = false;
            }
            else
            {
                if (recsets[0].Columns.Count != 2)
                {
                    passTest = false;
                }
            }
            Assert.IsTrue(passTest);
        }

        #endregion GetWizardData Tests

        #region GetDebugInputs/Outputs

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Assign_Get_Debug_Input_Output_With_All_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<ActivityDTO> fieldsCollection = new List<ActivityDTO>();
            fieldsCollection.Add(new ActivityDTO("[[CompanyName]]", "The Unlimited", 1));
            fieldsCollection.Add(new ActivityDTO("[[Customers(1).FirstName]]", "TestName", 2));
            fieldsCollection.Add(new ActivityDTO("[[Numeric(*).num]]", "123456789", 3));
            DsfMultiAssignActivity act = new DsfMultiAssignActivity { FieldsCollection = fieldsCollection };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(0, inRes.Count);

            Assert.AreEqual(3, outRes.Count);
            Assert.AreEqual(4, outRes[0].Count);
            Assert.AreEqual(4, outRes[1].Count);
            Assert.AreEqual(4, outRes[2].Count);
        }

        #endregion

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, IList<ActivityDTO> assignFields, string outputMapping = null)
        {
            if (outputMapping == null)
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

        private string GetSimpleADL()
        {
            return @"<ADL>
  <cRec>
    <opt></opt>
    <display />
  </cRec>
  <gRec>
    <opt>Value1</opt>
    <display>display1</display>
  </gRec>
  <recset></recset>
  <field></field>
</ADL>";
        }

        #endregion
    }
}
