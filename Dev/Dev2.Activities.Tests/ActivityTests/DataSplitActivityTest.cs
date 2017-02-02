/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActivityUnitTests;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DataSplitActivityTest
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DataSplitActivityTest : BaseActivityUnitTest
    {
        IList<DataSplitDTO> _resultsCollection = new List<DataSplitDTO>();
        readonly string _source = ActivityStrings.DataSplit_SourceString;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            if(_resultsCollection == null)
            {
                _resultsCollection = new List<DataSplitDTO>();
            }
            _resultsCollection.Clear();
        }

        #endregion



        #region Funky Language

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfDataMergeActivity_Execute")]
        public void DsfDataSplitActivity_Execute_WhenUsingAppendAndMixedSplitType_ExpectCorrectSplit()
        {

            //------------Setup for test--------------------------

            _resultsCollection.Add(new DataSplitDTO("[[rs().col1]]", "Chars", "|", 1));
            _resultsCollection.Add(new DataSplitDTO("[[rs().col2]]", "Chars", "|", 2));
            _resultsCollection.Add(new DataSplitDTO("[[rs().col3]]", "New Line", "", 3));
            _resultsCollection.Add(new DataSplitDTO("[[rs().data]]", "New Line", "", 4));
            _resultsCollection.Add(new DataSplitDTO("[[rs().data]]", "New Line", "", 5));

            SetupArguments("<root><ADL><testData>RSA ID|FirstName|LastName" + Environment.NewLine +
                           "13456456789|Samantha Some|Jones" + Environment.NewLine +
                           "09123456646|James|Apple</testData></ADL></root>",
                           "<ADL><rs><col1/><col2/><col3/><data/></rs><testData/></ADL>",
                           "[[testData]]",
                           _resultsCollection);

            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            string error;

            var col1List = RetrieveAllRecordSetFieldValues(result.Environment, "rs", "col1", out error);
            var col2List = RetrieveAllRecordSetFieldValues(result.Environment, "rs", "col2", out error);
            var col3List = RetrieveAllRecordSetFieldValues(result.Environment, "rs", "col3", out error);
            var dataList = RetrieveAllRecordSetFieldValues(result.Environment, "rs", "data", out error);

            // remove test datalist ;)

            //------------Assert Results-------------------------

            var col1Expected = new List<string> { "RSA ID" };
            var col2Expected = new List<string> { "FirstName" };
            var col3Expected = new List<string> { "LastName"};
            var dataExpected = new List<string> { "13456456789|Samantha Some|Jones", "09123456646|James|Apple" };

            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(col1Expected, col1List, comparer);
            CollectionAssert.AreEqual(col2Expected, col2List, comparer);
            CollectionAssert.AreEqual(col3Expected, col3List, comparer);
            CollectionAssert.AreEqual(dataExpected, dataList, comparer);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfDataMergeActivity_Execute")]
        public void DsfDataSplitActivity_Execute_WhenUsingStarAndMixedSplitType_ExpectCorrectSplit()
        {

            //------------Setup for test--------------------------

            _resultsCollection.Add(new DataSplitDTO("[[rs().col1]]", "Chars", "|", 1));
            _resultsCollection.Add(new DataSplitDTO("[[rs().col2]]", "Chars", "|", 2));
            _resultsCollection.Add(new DataSplitDTO("[[rs().col3]]", "New Line", "", 3));
            _resultsCollection.Add(new DataSplitDTO("[[rs(*).data]]", "New Line", "", 4));
            _resultsCollection.Add(new DataSplitDTO("[[rs(*).data]]", "New Line", "", 5));

            SetupArguments("<root><ADL><testData>RSA ID|FirstName|LastName" + Environment.NewLine +
                           "13456456789|Samantha Some|Jones" + Environment.NewLine +
                           "09123456646|James|Apple</testData></ADL></root>",
                           "<ADL><rs><col1/><col2/><col3/><data/></rs><testData/></ADL>",
                           "[[testData]]",
                           _resultsCollection);

            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            string error;

            var col1List = RetrieveAllRecordSetFieldValues(result.Environment, "rs", "col1", out error);
            var col2List = RetrieveAllRecordSetFieldValues(result.Environment, "rs", "col2", out error);
            var col3List = RetrieveAllRecordSetFieldValues(result.Environment, "rs", "col3", out error);
            var dataList = RetrieveAllRecordSetFieldValues(result.Environment, "rs", "data", out error);

            // remove test datalist ;)

            //------------Assert Results-------------------------

            var col1Expected = new List<string> { "RSA ID" };
            var col2Expected = new List<string> { "FirstName" };
            var col3Expected = new List<string> { "LastName" };
            var dataExpected = new List<string> { "13456456789|Samantha Some|Jones", "09123456646|James|Apple" };

            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(col1Expected, col1List, comparer);
            CollectionAssert.AreEqual(col2Expected, col2List, comparer);
            CollectionAssert.AreEqual(col3Expected, col3List, comparer);
            CollectionAssert.AreEqual(dataExpected, dataList, comparer);
        }

        #endregion

        [TestMethod] // - OK
        public void EmptySourceString_Expected_No_Splits()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, "", _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);
            // remove test datalist ;)

            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void Scalar_Expected_Split_And_Insert_To_Scalar()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();
            const string expected = @"Title|Fname|LNa";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual, "Got " + actual + " expected " + expected);

        }

        [TestMethod]
        public void MultipleScalars_Expected_Split_And_Insert_Mutiple_Scalars()
        {

            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
            _resultsCollection.Add(new DataSplitDTO("[[OutVar2]]", "Index", "10", 2));
            _resultsCollection.Add(new DataSplitDTO("[[OutVar3]]", "Index", "5", 3));
            _resultsCollection.Add(new DataSplitDTO("[[OutVar4]]", "Index", "15", 4));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source.Replace("\r", ""), _resultsCollection);

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { @"Title|Fname|LNa", "me|TelNo|", "1.Mr|", "Frank|Williams|" };
            List<string> actual = new List<string>();

            for(int i = 1; i <= 4; i++)
            {
                string returnVal;
                string error;
                GetScalarValueFromEnvironment(result.Environment, "OutVar" + i, out returnVal, out error);
                actual.Add(returnVal.Trim());
            }
            // remove test datalist ;)

            ActivityUnitTests.Utils.StringComparer comparer = new ActivityUnitTests.Utils.StringComparer();
            Assert.AreEqual(4, actual.Count());
            Assert.AreEqual(4, expected.Count());
            CollectionAssert.AreEqual(expected, actual, comparer);
        }

        [TestMethod]
        public void MixedScalarsAndRecordsetWithIndex_Expected_Split_Insert_Mutiple_Scalar_And_Recordsets()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
            _resultsCollection.Add(new DataSplitDTO("[[recset1(2).field1]]", "Index", "15", 2));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);

            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { @"" 
                                                     , @"Branson|0812457"
                                                     };
            string actualScalar;
            string error;
            IList<string> actualRecordSet;

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actualScalar, out error);

            Assert.AreEqual("896", actualScalar);

            GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out actualRecordSet, out error);

            // remove test datalist ;)

            List<string> actual = actualRecordSet.Select(entry => entry).ToList();
            ActivityUnitTests.Utils.StringComparer comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actual, comparer);
        }

        [TestMethod]
        public void MixedScalarsAndRecordsetWithoutIndex_Expected_Split_To_End_Inserting_Mutiple_Scalar_And_Recordsets()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Index", "15", 2));

            SetupArguments("<root></root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);

            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { @"me|TelNo|
1.Mr"
                                                     , @"|0795628443
2."
                                                     , @"|0821169853
3."
                                                     , @"|0762458963
4."
                                                     , @"via|0724587310"
                                                     , @"Branson|0812457"};
            List<string> actual = new List<string>();
            string actualScalar;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actualScalar, out error);
            Assert.AreEqual("896", actualScalar);

            actual.AddRange(RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error));

            // remove test datalist ;)

            string[] foo = actual.ToArray();
            actual.Clear();

            actual.AddRange(foo.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void NoResultVariableInFirst_Expected_Still_Spilt_But_Dont_Insert_For_First()
        {

            _resultsCollection.Add(new DataSplitDTO("", "Index", "15", 1));
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 2));
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"me|TelNo|
1.Mr";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);
            // remove test datalist ;)
            Assert.AreEqual(expected, actual, "Got " + actual + " expected " + expected);
        }

        [TestMethod]
        public void IndexTypeSplit_Expected_Split_At_An_Index()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            const string expected = "Title|Fname|LNa";
            string actual;
            string error;

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);
            // remove test datalist ;)

            Assert.AreEqual(expected, actual, "Got " + actual + " but expected " + expected);
        }

        [TestMethod]
        public void CharsTypeSplitSingle_Expected_Split_Once_At_Chars()
        {

            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Chars", "|", 1));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);

            IDSFDataObject result = ExecuteProcess();

            const string expected = @"Title";
            string actual;
            string error;

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);
            // remove test datalist ;)

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BlankSpaceTypeSplitSingle_Expected_Split_At_BlankSpace()
        {
            _resultsCollection.Clear();
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Space", "", 1));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);

            IDSFDataObject result = ExecuteProcess();

            const string expected = @"Title|Fname|LName|TelNo|
1.Mr|Frank|Williams|0795628443
2.Mr|Enzo|Ferrari|0821169853
3.Mrs|Jenny|Smith|0762458963
4.Ms|Kerrin|deSilvia|0724587310
5.Sir|Richard|Branson|0812457896";
            string actual;
            string error;

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);
            // remove test datalist ;)

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void BlankSpaceTypeSplitMultiple_Expected_Split_Mutiple_At_BlankSpace()
        {

            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Space", "", 1));
            _resultsCollection.Add(new DataSplitDTO("[[OutVar2]]", "Space", "", 2));
            const string source = "Test source string with spaces";
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, source, _resultsCollection);

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "Test", "source" };
            List<string> actual = new List<string>();
            string tempActual;
            string error;

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out tempActual, out error);
            actual.Add(tempActual);
            GetScalarValueFromEnvironment(result.Environment, "OutVar2", out tempActual, out error);
            actual.Add(tempActual);
            // remove test datalist ;)

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void NewLineTypeSplitWindows_Expected_Split_On_Windows_NewLine()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "New Line", "", 1));
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            const string expected = @"Title|Fname|LName|TelNo|";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual, "Got " + actual + " expected " + expected);

        }

        //2012.09.28: massimo.guerrera - Add tab functionality
        [TestMethod]
        public void TabTypeSplit_Expected_Split_On_Tab()
        {
            _resultsCollection.Add(new DataSplitDTO("[[recset2().field2]]", "Tab", "", 1));
            const string sourceString = "Test	Data	To	Split";
            SetupArguments("<root></root>", ActivityStrings.DataSplit_preDataList, sourceString, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "Test", "Data", "To", "Split" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.Environment, "recset2", "field2", out error);

            // remove test datalist ;)

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void EndTypeSplit_Expected_Split_On_End_Of_String()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "End", "", 1));
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            const string expected = @"Title|Fname|LName|TelNo|
1.Mr|Frank|Williams|0795628443
2.Mr|Enzo|Ferrari|0821169853
3.Mrs|Jenny|Smith|0762458963
4.Ms|Kerrin|deSilvia|0724587310
5.Sir|Richard|Branson|0812457896";
            string actual;
            string error;

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);
            // remove test datalist ;)

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataSplitActivity_GetOutputs")]
        public void DsfDataSplitActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            _resultsCollection.Clear();
            _resultsCollection.Add(new DataSplitDTO("[[recset1(5).field1]]", "Chars", "|", 1));
            _resultsCollection.Add(new DataSplitDTO("[[recset2(2).field2]]", "Chars", "|", 2));
            var act = new DsfDataSplitActivity { SourceString = _source, ResultsCollection = _resultsCollection };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, outputs.Count);
            Assert.AreEqual("[[recset1(5).field1]]", outputs[0]);
            Assert.AreEqual("[[recset2(2).field2]]", outputs[1]);
        }

        [TestMethod]
        public void RecordsetsWithVaryingIndexesExpectedSplitAndInsertAtDifferentIndexes()
        {

            _resultsCollection.Clear();
            _resultsCollection.Add(new DataSplitDTO("[[recset1(5).field1]]", "Chars", "|", 1));
            _resultsCollection.Add(new DataSplitDTO("[[recset2(2).field2]]", "Chars", "|", 2));

            TestStartNode = new FlowStep
            {
                Action = new DsfDataSplitActivity { SourceString = _source, ResultsCollection = _resultsCollection }
            };

            TestData = ActivityStrings.DataSplit_preDataList;
            SetupArguments("<root></root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);

            IDSFDataObject result = ExecuteProcess();

            string error;
            List<string> actual1 = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);
            List<string> actual2 = RetrieveAllRecordSetFieldValues(result.Environment, "recset2", "field2", out error);

            // remove test datalist ;)

            Assert.AreEqual("Branson", actual1[0]);
            Assert.AreEqual("0812457896", actual2[0]);
        }

        [TestMethod]
        public void MutiRecsetsWithNoIndex_Expected_Split_Append_To_The_Recordsets()
        {
            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Index", "15", 1));
            _resultsCollection.Add(new DataSplitDTO("[[recset1().rec1]]", "Index", "15", 2));

            SetupArguments("<root></root>", ActivityStrings.DataList_NewPreEx, _source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { @"me|TelNo|
1.Mr"
                                                     , @"|0795628443
2."
                                                     , @"|0821169853
3."
                                                     , @"|0762458963
4."
                                                     , @"via|0724587310"
                                                     , @"Branson|0812457",
                                                       @"Title|Fname|LNa",
                                                        @"|Frank|Williams",
                                                        @"Mr|Enzo|Ferrari",
                                                        @"Mrs|Jenny|Smith",
                                                        @"Ms|Kerrin|deSil",
                                                        @"5.Sir|Richard|",
                                                        @"896"
                                                        
                                                        };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "rec1", out error);
            actual.AddRange(RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error));

            // remove test datalist ;)

            string[] foo = actual.ToArray();
            actual.Clear();

            actual.AddRange(foo.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }


        [TestMethod]
        public void RecsetWithStar_Expected_Split_Overwrite_Records_From_Index_1()
        {

            _resultsCollection.Add(new DataSplitDTO("[[recset1(*).field1]]", "Index", "15", 1));

            SetupArguments(@"<root></root>", ActivityStrings.DataSplit_DataListShape, _source, _resultsCollection);

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "Title|Fname|LNa"
                                                    , @"me|TelNo|
1.Mr"
                                                    , "|Frank|Williams"
                                                    , @"|0795628443
2."
                                                    , "Mr|Enzo|Ferrari"
                                                    , @"|0821169853
3."
                                                    , "Mrs|Jenny|Smith"
                                                    , @"|0762458963
4."
                                                    , "Ms|Kerrin|deSil"
                                                    , "via|0724587310"
                                                    , "5.Sir|Richard|"
                                                    , "Branson|0812457"
                                                    };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);

            // remove test datalist ;)

            string[] foo = actual.ToArray();
            actual.Clear();
            actual.AddRange(foo.Select(f => f.Trim()));

            string exp = expected.ToString();
            string act = actual.ToString();

            FixBreaks(ref exp, ref act);
            Assert.AreEqual(exp, act);
        }
        private void FixBreaks(ref string expected, ref string actual)
        {
            expected = new StringBuilder(expected).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
            actual = new StringBuilder(actual).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
        }

        [TestMethod]
        public void RecorsetWithStarAsIndexInSourceString_Expected_Split_For_Last_Value_In_Recordset()
        {

            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Space", "", 1));

            #region Ugle String of Current DataList

            const string currentDL = @"<root>
	<firstName/>
	<lastName/>
	<telNum/>
	<index/>
	<recset1>
		<field1>This is test data to split</field1>
	</recset1>
    <recset1>
		<field1>This is the second test data to split</field1>
	</recset1>
	<recset2>
		<field2/>
	</recset2>
	<OutVar1/>
	<OutVar2/>
	<OutVar3/>
	<OutVar4/>
	<OutVar5/>
</root>";
            #endregion Ugly String of Current DataList

            SetupArguments(currentDL, ActivityStrings.DataSplit_preDataList, "[[recset1(*).field1]]", _resultsCollection);
            IDSFDataObject result = ExecuteProcess();
            List<string> expectedRecSet1 = new List<string> { "This is test data to split"
                                                            , "This is the second test data to split"
                                                            , "This"
                                                            , "is"
                                                            , "test"
                                                            , "data"
                                                            , "to"
                                                            , "split"
                                                            , "This"
                                                            , "is"
                                                            , "the"
                                                            , "second"
                                                            , "test"
                                                            , "data"
                                                            , "to"
                                                            , "split" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);

            // remove test datalist ;)

            CollectionAssert.AreEqual(expectedRecSet1, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataSplitActivity_UpdateForEachInputs")]
        public void DsfDataSplitActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            IList<DataSplitDTO> resultsCollection = new List<DataSplitDTO> { new DataSplitDTO("[[CompanyName]]", "Index", "2", 1) };
            DsfDataSplitActivity act = new DsfDataSplitActivity { SourceString = "[[CompanyName]]", ResultsCollection = resultsCollection };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("[[CompanyName]]", act.ResultsCollection[0].OutputVariable);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataSplitActivity_UpdateForEachInputs")]
        public void DsfDataSplitActivity_UpdateForEachInputs_MoreThan1Updates_UpdatesMergeCollection()
        {
            //------------Setup for test--------------------------
            IList<DataSplitDTO> resultsCollection = new List<DataSplitDTO> { new DataSplitDTO("[[CompanyName]]", "Index", "2", 1), new DataSplitDTO("[[CompanyName]]", "Index", "1", 2) };
            var act = new DsfDataSplitActivity { SourceString = "[[CompanyName]]", ResultsCollection = resultsCollection };

            var tuple1 = new Tuple<string, string>("2", "Test");
            var tuple2 = new Tuple<string, string>("1", "Test2");
            var tuple3 = new Tuple<string, string>("[[CompanyName]]", "Test3");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2, tuple3 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.ResultsCollection[0].At);
            Assert.AreEqual("Test2", act.ResultsCollection[1].At);
            Assert.AreEqual("Test3", act.SourceString);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataSplitActivity_UpdateForEachOutputs")]
        public void DsfDataSplitActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            IList<DataSplitDTO> resultsCollection = new List<DataSplitDTO> { new DataSplitDTO("[[CompanyName]]", "Index", "2", 1) };
            var act = new DsfDataSplitActivity { SourceString = "[[CompanyName]]", ResultsCollection = resultsCollection };

            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("[[CompanyName]]", act.SourceString);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataSplitActivity_UpdateForEachOutputs")]
        public void DsfDataSplitActivity_UpdateForEachOutputs_1Updates_UpdateCountNumber()
        {
            //------------Setup for test--------------------------
            IList<DataSplitDTO> resultsCollection = new List<DataSplitDTO> { new DataSplitDTO("[[CompanyName]]", "Index", "2", 1) };
            var act = new DsfDataSplitActivity { SourceString = "[[CompanyName]]", ResultsCollection = resultsCollection };

            var tuple1 = new Tuple<string, string>("[[CompanyName]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.ResultsCollection[0].OutputVariable);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataSplitActivity_GetForEachInputs")]
        public void DsfDataSplitActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            IList<DataSplitDTO> resultsCollection = new List<DataSplitDTO> { new DataSplitDTO("[[CompanyName]]", "Index", "2", 1) };
            var act = new DsfDataSplitActivity { SourceString = "[[CompanyName]]", ResultsCollection = resultsCollection };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, dsfForEachItems.Count);
            Assert.AreEqual("[[CompanyName]]", dsfForEachItems[0].Name);
            Assert.AreEqual("[[CompanyName]]", dsfForEachItems[0].Value);
            Assert.AreEqual("2", dsfForEachItems[1].Name);
            Assert.AreEqual("2", dsfForEachItems[1].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataSplitActivity_GetForEachOutputs")]
        public void DsfDataSplitActivity_GetForEachOutputs_WhenHasResult_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            IList<DataSplitDTO> resultsCollection = new List<DataSplitDTO> { new DataSplitDTO("[[CompanyName]]", "Index", "2", 1) };
            var act = new DsfDataSplitActivity { SourceString = "[[CompanyName]]", ResultsCollection = resultsCollection };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual("[[CompanyName]]", dsfForEachItems[0].Name);
            Assert.AreEqual("[[CompanyName]]", dsfForEachItems[0].Value);
        }


        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string sourceString, IList<DataSplitDTO> resultCollection)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDataSplitActivity { SourceString = sourceString, ResultsCollection = resultCollection }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
