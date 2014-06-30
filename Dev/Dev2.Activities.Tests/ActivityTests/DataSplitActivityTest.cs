using ActivityUnitTests;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DataSplitActivityTest
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    [ExcludeFromCodeCoverage]
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

            SetupArguments("<ADL><testData>RSA ID|FirstName|LastName" + Environment.NewLine +
                           "13456456789|Samantha Some|Jones" + Environment.NewLine +
                           "09123456646|James|Apple</testData></ADL>",
                           "<ADL><rs><col1/><col2/><col3/><data/></rs><testData/></ADL>",
                           "[[testData]]",
                           _resultsCollection);

            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            string error;

            var col1List = RetrieveAllRecordSetFieldValues(result.DataListID, "rs", "col1", out error);
            var col2List = RetrieveAllRecordSetFieldValues(result.DataListID, "rs", "col2", out error);
            var col3List = RetrieveAllRecordSetFieldValues(result.DataListID, "rs", "col3", out error);
            var dataList = RetrieveAllRecordSetFieldValues(result.DataListID, "rs", "data", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            //------------Assert Results-------------------------

            var col1Expected = new List<string> { "RSA ID", "" };
            var col2Expected = new List<string> { "FirstName", "" };
            var col3Expected = new List<string> { "LastName", "" };
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

            SetupArguments("<ADL><testData>RSA ID|FirstName|LastName" + Environment.NewLine +
                           "13456456789|Samantha Some|Jones" + Environment.NewLine +
                           "09123456646|James|Apple</testData></ADL>",
                           "<ADL><rs><col1/><col2/><col3/><data/></rs><testData/></ADL>",
                           "[[testData]]",
                           _resultsCollection);

            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            string error;

            var col1List = RetrieveAllRecordSetFieldValues(result.DataListID, "rs", "col1", out error);
            var col2List = RetrieveAllRecordSetFieldValues(result.DataListID, "rs", "col2", out error);
            var col3List = RetrieveAllRecordSetFieldValues(result.DataListID, "rs", "col3", out error);
            var dataList = RetrieveAllRecordSetFieldValues(result.DataListID, "rs", "data", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            //------------Assert Results-------------------------

            var col1Expected = new List<string> { "RSA ID", "" };
            var col2Expected = new List<string> { "FirstName", "" };
            var col3Expected = new List<string> { "LastName", "" };
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
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void RecordsetWithAnIndexWithNoRecordsInRecSet_Expected_Split_And_Append_Records()
        {
            _resultsCollection.Add(new DataSplitDTO("[[recset1(3).field1]]", "Index", "15", 1));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>"
                         , ActivityStrings.DataSplit_preDataList
                         , _source
                         , _resultsCollection);
            List<string> expected = new List<string> { "896" };

            IDSFDataObject result = ExecuteProcess();

            IList<IBinaryDataListItem> actual;
            string error;

            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            List<string> actualRet = new List<string>();
            actual.Where(c => c.ItemCollectionIndex >= 3).ToList().ForEach(d => actualRet.Add(d.TheValue));
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actualRet, comparer);
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
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual, "Got " + actual + " expected " + expected);

        }

        [TestMethod]
        public void MultipleScalars_Expected_Split_And_Insert_Mutiple_Scalars()
        {

            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
            _resultsCollection.Add(new DataSplitDTO("[[OutVar2]]", "Index", "10", 2));
            _resultsCollection.Add(new DataSplitDTO("[[OutVar3]]", "Index", "5", 3));
            _resultsCollection.Add(new DataSplitDTO("[[OutVar4]]", "Index", "15", 4));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { @"Title|Fname|LNa", "me|TelNo|", "1.Mr", "|Frank|Williams" };
            List<string> actual = new List<string>();

            for(int i = 1; i <= 4; i++)
            {
                string returnVal;
                string error;
                GetScalarValueFromDataList(result.DataListID, "OutVar" + i, out returnVal, out error);
                actual.Add(returnVal.Trim());
            }
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            ActivityUnitTests.Utils.StringComparer comparer = new ActivityUnitTests.Utils.StringComparer();
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
            IList<IBinaryDataListItem> actualRecordSet;

            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actualScalar, out error);

            Assert.AreEqual("896", actualScalar);

            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actualRecordSet, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            List<string> actual = actualRecordSet.Select(entry => entry.TheValue).ToList();
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
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actualScalar, out error);
            Assert.AreEqual("896", actualScalar);

            actual.AddRange(RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error));

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

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
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            Assert.AreEqual(expected, actual, "Got " + actual + " expected " + expected);
        }

        // Bug : 8725
        [TestMethod]
        public void NoResultVariableInAnyRow_Expected_Still_Split_But_Dont_Insert_Any()
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            _resultsCollection.Add(new DataSplitDTO("", "Index", "15", 1));
            _resultsCollection.Add(new DataSplitDTO("", "Index", "15", 2));
            SetupArguments("<root></root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);

            IDSFDataObject result = ExecuteProcess();

            List<bool> isPopulated = new List<bool>();
            ErrorResultTO errors;
            IBinaryDataList dList = compiler.FetchBinaryDataList(result.DataListID, out errors);



            foreach(string data in dList.FetchAllUserKeys())
            {
                IBinaryDataListEntry entry;
                string error;
                dList.TryGetEntry(data, out entry, out error);
                if(entry.FetchAppendRecordsetIndex() == 1)
                {
                    isPopulated.Add(false);
                }
                else
                {
                    isPopulated.Add(true);
                }
            }

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.DoesNotContain(isPopulated, true);
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

            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

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

            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CharsTypeSplitMultiple_Expected_Split_Mutiple_At_Chars()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Chars", "|", 1));
            _resultsCollection.Add(new DataSplitDTO("[[OutVar2]]", "Chars", "1.", 1));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();


            string tempResult;
            string error;

            GetScalarValueFromDataList(result.DataListID, "OutVar1", out tempResult, out error);
            Assert.AreEqual("Title", tempResult);
            GetScalarValueFromDataList(result.DataListID, "OutVar2", out tempResult, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(@"Fname|LName|TelNo|", tempResult.Trim());

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

            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

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

            GetScalarValueFromDataList(result.DataListID, "OutVar1", out tempActual, out error);
            actual.Add(tempActual);
            GetScalarValueFromDataList(result.DataListID, "OutVar2", out tempActual, out error);
            actual.Add(tempActual);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

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
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

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
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset2", "field2", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

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

            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);

        }

        //TJ-TODO:-
        [Ignore]
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
            List<string> actual1 = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);
            List<string> actual2 = RetrieveAllRecordSetFieldValues(result.DataListID, "recset2", "field2", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual("Branson", actual1[0]);
            Assert.AreEqual("0812457896", actual2[0]);
        }

        [TestMethod]
        public void CleaningArgs_Expected_Clean_Arguments_Coming_In()
        {
            /*
             *  Expected Tokenization...
             *  
             *  1. Title|Fname|LNa
                2. me|TelNo| 1.Mr
                3. |Frank|Williams
                4. |07956284432.
                4. Mr|Enzo|Ferrari
                5. |0821169853 3.
                6. Mrs|Jenny|Smith
                7. |0762458963 4.
                8. Ms|Kerrin|deSil
                9. via|0724587310
                10. 5.Sir|Richard|
                11. Branson|0812457
                12. 896
             */

            _resultsCollection.Add(new DataSplitDTO("[[recset1(5).field1]]", "Index", "15", 1));
            _resultsCollection.Add(new DataSplitDTO("[[recset2(2).field2]]", "Index", "", 2));
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual("896", actual[1]);
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
                                                       @"",
                                                       @"Title|Fname|LNa",
                                                        @"|Frank|Williams",
                                                        @"Mr|Enzo|Ferrari",
                                                        @"Mrs|Jenny|Smith",
                                                        @"Ms|Kerrin|deSil",
                                                        @"5.Sir|Richard|",
                                                        @"896"
                                                        
                                                        };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "rec1", out error);
            actual.AddRange(RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error));

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            string[] foo = actual.ToArray();
            actual.Clear();

            actual.AddRange(foo.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void RecsetWithExistingIndex_Expected_Split_Insert_at_Index_Specified()
        {
            _resultsCollection.Add(new DataSplitDTO("[[recset1(2).field1]]", "Index", "15", 1));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_DataListShape, _source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual("896", actual[1]);
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
                                                    , "896" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            string[] foo = actual.ToArray();
            actual.Clear();
            actual.AddRange(foo.Select(f => f.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void RecsetWithZeroAsIndex_Expected_No_Splits()
        {
            _resultsCollection.Clear();
            _resultsCollection.Add(new DataSplitDTO("[[recset1(0).field1]]", "Index", "15", 1));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);

        }

        [TestMethod]
        public void CharsSplitWithNoChars_Expected_No_Splits()
        {
            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Chars", "", 1));
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);

            IDSFDataObject result = ExecuteProcess();
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(string.Empty, actual[0]);
        }

        [TestMethod]
        public void IndexSplitWithZeroIndex_Expected_No_Splits()
        {
            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Index", "0", 1));
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);



            Assert.IsTrue(res);
        }

        [TestMethod]
        public void IndexSplitWithNegitiveNumberIndex_Expected_No_Splits()
        {
            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Index", "-4", 1));
            SetupArguments(ActivityStrings.DataSplit_preDataList, ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);

            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
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
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

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
            act.UpdateForEachInputs(null, null);
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
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2, tuple3 }, null);
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
            act.UpdateForEachOutputs(null, null);
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
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
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
