//using System;
//using System.Text;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Unlimited.Applications.BusinessDesignStudio.Activities;
//using System.Collections.ObjectModel;
//using System.Activities.Statements;
//using Unlimited.Framework;
//using Dev2.DataList.Contract.Binary_Objects;
//using Dev2.DataList.Contract;
//using ActivityUnitTests.Utils;

//namespace ActivityUnitTests.ActivityTest
//{
//    /// <summary>
//    /// Summary description for DataSplitActivityTest
//    /// </summary>
//    [TestClass]
//    public class DataSplitActivityTest : BaseActivityUnitTest
//    {
//        IList<DataSplitDTO> _resultsCollection = new List<DataSplitDTO>();
//        string Source = ActivityStrings.DataSplit_SourceString;
//        public DataSplitActivityTest()
//        {
//            //
//            // TODO: Add constructor logic here
//            //
//        }

//        private TestContext testContextInstance;

//        /// <summary>
//        ///Gets or sets the test context which provides
//        ///information about and functionality for the current test run.
//        ///</summary>
//        public TestContext TestContext
//        {
//            get
//            {
//                return testContextInstance;
//            }
//            set
//            {
//                testContextInstance = value;
//            }
//        }

//        #region Additional test attributes
//        //
//        // You can use the following additional attributes as you write your tests:
//        //
//        // Use ClassInitialize to run code before running the first test in the class
//        // [ClassInitialize()]
//        // public static void MyClassInitialize(TestContext testContext) { }
//        //
//        // Use ClassCleanup to run code after all tests in a class have run
//        // [ClassCleanup()]
//        // public static void MyClassCleanup() { }
//        //
//        // Use TestInitialize to run code before running each test 
//        [TestInitialize()]
//        public void MyTestInitialize()
//        {
//            if (_resultsCollection == null)
//            {
//                _resultsCollection = new List<DataSplitDTO>();
//            }
//            _resultsCollection.Clear();
//        }
//        //
//        // Use TestCleanup to run code after each test has run
//        // [TestCleanup()]
//        // public void MyTestCleanup() { }
//        //
//        #endregion

//        [TestMethod]
//        public void EmptySourceString_Expected_No_Splits()
//        {
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, "", _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();

//            string actual = string.Empty;
//            string error = string.Empty;
//            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

//            Assert.AreEqual(string.Empty, actual);
//        }

//        [TestMethod]
//        public void RecordsetWithNoIndex_Expected_Split_1_Time()
//        {
//            // FIX ME - Amend to properly follow data language
//            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Index", "15", 1));
//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();

//            List<string> expected = new List<string> { "Title|Fname|LNa" 
//                                                     };

//            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
//            string error = string.Empty;
//            List<string> actualRet = new List<string>();
//            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actual, out error);
//            actual.Where(c => c.FieldName == "field1").ToList().ForEach(d => actualRet.Add(d.TheValue));

//            if (string.IsNullOrEmpty(error))
//            {
//                CollectionAssert.AreEqual(expected, actualRet);
//            }
//            else
//            {
//                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
//            }
//        }

//        [TestMethod]
//        public void RecordsetWithAnIndexWithNoRecordsInRecSet_Expected_Split_And_Append_Records()
//        {

//            // FIX ME - Amend to properly follow data language
//            _resultsCollection.Add(new DataSplitDTO("[[recset1(3).field1]]", "Index", "15", 1));

//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>"
//                         , ActivityStrings.DataSplit_preDataList
//                         , Source
//                         , _resultsCollection);
//            List<string> expected = new List<string> { "Title|Fname|LNa"
//                , @"me|TelNo|
//1.Mr"
//                , "|Frank|Williams" };

//            IDSFDataObject result = ExecuteProcess();

//            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
//            string error = string.Empty;

//            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actual, out error);
//            List<string> actualRet = new List<string>();
//            actual.Where(c => c.ItemCollectionIndex >= 3).ToList().ForEach(d => actualRet.Add(d.TheValue));
//            var comparer = new ActivityUnitTests.Utils.StringComparer();
//            CollectionAssert.AreEqual(expected, actualRet, comparer);
//        }

//        [TestMethod]
//        public void RecordsetWithAnIndexWithSomeRecordsInRecSet_Expected_Split_And_Append_Records_After_PreExixsting_Records()
//        {

//            // FIX ME - Amend to properly follow data language
//            _resultsCollection.Add(new DataSplitDTO("[[recset1(5).field1]]", "Index", "15", 1));

//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>"
//                         , ActivityStrings.DataSplit_DataListShape
//                         , Source
//                         , _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();

//            List<string> expected = new List<string> { "testData1"
//                                                     , "testData2"
//                                                     , "testData3"
//                                                     , @"|0795628443
//2."
//                                                     , "Mr|Enzo|Ferrari" };

//            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
//            string error = string.Empty;
//            GetRecordSetFieldValueFromDataList(result.DataListID
//                                            , "recset1"
//                                            , "field1"
//                                            , out actual
//                                            , out error);

//            List<string> actualRet = new List<string>();

//            actual.Where(c => c.FieldName == "field1" && c.ItemCollectionIndex == 5).ToList().ForEach(d => actualRet.Add(d.TheValue));
//            var comparer = new ActivityUnitTests.Utils.StringComparer();
//            CollectionAssert.AreEqual(expected, actualRet, comparer);
//        }

//        [TestMethod]
//        public void Scalar_Expected_Split_And_Insert_To_Scalar()
//        {

//            // FIX ME - Amend to properly follow data language
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));

//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();
//            string expected = @"Title|Fname|LNa";
//            string actual = string.Empty;
//            string error = string.Empty;
//            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

//            if (string.IsNullOrEmpty(error))
//            {
//                Assert.AreEqual(expected, actual);
//            }
//            else
//            {
//                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
//            }
//        }

//        [TestMethod]
//        public void MultipleScalars_Expected_Split_And_Insert_Mutiple_Scalars()
//        {

//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar2]]", "Index", "10", 2));
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar3]]", "Index", "5", 3));
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar4]]", "Index", "15", 4));

//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);

//            IDSFDataObject result = ExecuteProcess();

//            List<string> expected = new List<string> { "Title|Fname|LNa"
//                                                      , "me|TelNo|"
//                                                      , "1.Mr"
//                                                      , "|Frank|Williams" };
//            List<string> actual = new List<string>();
//            List<string> errors = new List<string>();
//            for (int i = 1; i <= 4; i++)
//            {
//                string returnVal = string.Empty;
//                string error = string.Empty;
//                GetScalarValueFromDataList(result.DataListID, "OutVar" + i, out returnVal, out error);
//                actual.Add(returnVal.Trim());
//            }
//            ActivityUnitTests.Utils.StringComparer comparer = new ActivityUnitTests.Utils.StringComparer();
//            CollectionAssert.AreEqual(expected, actual, comparer);
//        }

//        [TestMethod]
//        public void MixedScalarsAndRecordsetWithIndex_Expected_Split_Insert_Mutiple_Scalar_And_Recordsets()
//        {
//            // FIX ME

//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
//            _resultsCollection.Add(new DataSplitDTO("[[recset1(2).field1]]", "Index", "15", 2));

//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);

//            IDSFDataObject result = ExecuteProcess();
//            List<string> expected = new List<string> { @"me|TelNo|
//1.Mr" 
//                                                     , @"|0795628443
//2."
//                                                     , "|Frank|Williams" };
//            List<string> actual = new List<string>();
//            string actualScalar = string.Empty;
//            string error = string.Empty;
//            IList<IBinaryDataListItem> actualRecordSet = new List<IBinaryDataListItem>();

//            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actualScalar, out error);
//            actual.Add(actualScalar);
//            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1()", "field1", out actualRecordSet, out error);
//            foreach (IBinaryDataListItem entry in actualRecordSet)
//            {
//                actual.Add(entry.TheValue);
//            }
//            ActivityUnitTests.Utils.StringComparer comparer = new ActivityUnitTests.Utils.StringComparer();
//            CollectionAssert.AreEqual(expected, actual, comparer);
//        }

//        [TestMethod]
//        public void MixedScalarsAndRecordsetWithoutIndex_Expected_Split_To_End_Inserting_Mutiple_Scalar_And_Recordsets()
//        {
//            // FIX ME

//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
//            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Index", "15", 2));

//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);

//            IDSFDataObject result = ExecuteProcess();
//            List<string> expected = new List<string> { @"me|TelNo|
//1.M"
//                                                     , @"|0795628443
//2."
//                                                     , @"|0821169853
//3."
//                                                     , @"|0762458963
//4."
//                                                     , @"via|0724587310"
//                                                     , @"Branson|0812457"
//                                                     , "896" };
//            List<string> actual = new List<string>();
//            string actualScalar = string.Empty;
//            string error = string.Empty;
//            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actualScalar, out error);
//            actual.Add(actualScalar);
//            actual.AddRange(RetrieveAllRecordSetFieldValues(result.DataListID, "recset1()", "field1", out error));

//            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
//        }

//        [TestMethod]
//        public void NoResultVariableInFirst_Expected_Still_Spilt_But_Dont_Insert_For_First()
//        {

//            _resultsCollection.Add(new DataSplitDTO("", "Index", "15", 1));
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 2));
//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);

//            IDSFDataObject result = ExecuteProcess();
//            string expected = @"me|TelNo|
//1.Mr";
//            string actual = string.Empty;
//            string error = string.Empty;
//            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
//            Assert.AreEqual(expected, actual);
//        }

//        [TestMethod]
//        public void NoResultVariableInAnyRow_Expected_Still_Split_But_Dont_Insert_Any()
//        {

//            _resultsCollection.Add(new DataSplitDTO("", "Index", "15", 1));
//            _resultsCollection.Add(new DataSplitDTO("", "Index", "15", 2));
//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);

//            IDSFDataObject result = ExecuteProcess();

//            List<bool> isPopulated = new List<bool>();
//            ErrorResultTO errors = new ErrorResultTO();
//            IBinaryDataList dList = _compiler.FetchBinaryDataList(result.DataListID, out errors);

//            foreach (string data in dList.FetchAllKeys())
//            {
//                IBinaryDataListEntry entry = null;
//                string error = string.Empty;
//                dList.TryGetEntry(data, out entry, out error);
//                if (entry.FetchAppendRecordsetIndex() == 1)
//                {
//                    isPopulated.Add(false);
//                }
//                else
//                {
//                    isPopulated.Add(true);
//                }
//            }

//            CollectionAssert.DoesNotContain(isPopulated, true);
//        }

//        [TestMethod]
//        public void IndexTypeSplit_Expected_Split_At_An_Index()
//        {
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();

//            string expected = "Title|Fname|LNa";
//            string actual = string.Empty;
//            string error = string.Empty;

//            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

//            Assert.AreEqual(expected, actual);
//        }

//        [TestMethod]
//        public void CharsTypeSplitSingle_Expected_Split_Once_At_Chars()
//        {

//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Chars", "|", 1));

//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);

//            IDSFDataObject result = ExecuteProcess();

//            string expected = @"Title";
//            string actual = string.Empty;
//            string error = string.Empty;

//            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

//            Assert.AreEqual(expected, actual);
//        }

//        [TestMethod]
//        public void CharsTypeSplitMultiple_Expected_Split_Mutiple_At_Chars()
//        {
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Chars", "|", 1));
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar2]]", "Chars", "1.", 1));

//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();

//            List<string> expected = new List<string> { "Title", "Fname|LName|TelNo|" };
//            List<string> actual = new List<string>();
//            string tempResult = string.Empty;
//            string error = string.Empty;

//            GetScalarValueFromDataList(result.DataListID, "OutVar1", out tempResult, out error);
//            Assert.AreEqual("Title", tempResult);
//            GetScalarValueFromDataList(result.DataListID, "OutVar2", out tempResult, out error);
//            Assert.AreEqual("Fname|LName|TelNo|", tempResult.Trim());

//        }

//        [TestMethod]
//        public void BlankSpaceTypeSplitSingle_Expected_Split_At_BlankSpace()
//        {
//            _resultsCollection.Clear();
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Space", "", 1));

//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);

//            IDSFDataObject result = ExecuteProcess();

//            string expected = @"Title|Fname|LName|TelNo|
//1.Mr|Frank|Williams|0795628443
//2.Mr|Enzo|Ferrari|0821169853
//3.Mrs|Jenny|Smith|0762458963
//4.Ms|Kerrin|deSilvia|0724587310
//5.Sir|Richard|Branson|0812457896";
//            string actual = string.Empty;
//            string error = string.Empty;

//            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
//            if (string.IsNullOrEmpty(error))
//            {
//                Assert.AreEqual(expected, actual);
//            }
//            else
//            {
//                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
//            }

//        }

//        [TestMethod]
//        public void BlankSpaceTypeSplitMultiple_Expected_Split_Mutiple_At_BlankSpace()
//        {

//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Space", "", 1));
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar2]]", "Space", "", 2));
//            string source = "Test source string with spaces";
//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, source, _resultsCollection);

//            IDSFDataObject result = ExecuteProcess();

//            List<string> expected = new List<string> { "Test", "source" };
//            List<string> actual = new List<string>();
//            string tempActual = string.Empty;
//            string error = string.Empty;

//            GetScalarValueFromDataList(result.DataListID, "OutVar1", out tempActual, out error);
//            actual.Add(tempActual);
//            GetScalarValueFromDataList(result.DataListID, "OutVar2", out tempActual, out error);
//            actual.Add(tempActual);


//            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
//        }

//        [TestMethod]
//        public void NewLineTypeSplitWindows_Expected_Split_On_Windows_NewLine()
//        {
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "New Line", "", 1));
//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();

//            string expected = @"Title|Fname|LName|TelNo|";
//            string actual = string.Empty;
//            string error = string.Empty;
//            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

//            if (string.IsNullOrEmpty(error))
//            {
//                Assert.AreEqual(expected, actual);
//            }
//            else
//            {
//                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
//            }
//        }

//        //2012.09.28: massimo.guerrera - Add tab functionality
//        //LANGUAGE FIX PLZ
//        [TestMethod]
//        public void TabTypeSplit_Expected_Split_On_Tab()
//        {
//            _resultsCollection.Add(new DataSplitDTO("[[recset2().field2]]", "Tab", "", 1));
//            string sourceString = "Test	Data	To	Split";
//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, sourceString, _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();

//            List<string> expected = new List<string> { "Test", "Data", "To", "Split" };
//            string error = string.Empty;
//            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset2", "field2", out error);

//            if (string.IsNullOrEmpty(error))
//            {
//                CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
//            }
//            else
//            {
//                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
//            }

//        }

//        [TestMethod]
//        public void NewLineTypeSplitUnix_Expected_Split_On_Unix_NewLine()
//        {
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "New Line", "", 1));
//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);

//            IDSFDataObject result = ExecuteProcess();
//            string expected = @"Title|Fname|LName|TelNo|";
//            string actual = string.Empty;
//            string error = string.Empty;
//            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
//            //Assert.AreEqual(expected, result.XmlString);
//            //Ammend to make provision for UNIX charset
//            Assert.IsTrue(1 == 1, "Need to be fixed need to get some text from unix");
//        }

//        [TestMethod]
//        public void NewLineTypeSplitMac_Expected_Split_On_Mac_NewLine()
//        {
//            _resultsCollection.Clear();
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "New Line", "", 1));

//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);

//            IDSFDataObject result = ExecuteProcess();
//            string expected = @"Title|Fname|LName|TelNo|";
//            string actual = string.Empty;
//            string error = string.Empty;

//            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

//            //Assert.AreEqual(expected, result.XmlString);
//            //Ammend for MAC charset
//            Assert.IsTrue(1 == 1, "Need to be fixed by getting some text from mac");
//        }

//        [TestMethod]
//        public void EndTypeSplit_Expected_Split_On_End_Of_String()
//        {
//            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "End", "", 1));
//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();

//            string expected = @"Title|Fname|LName|TelNo|
//1.Mr|Frank|Williams|0795628443
//2.Mr|Enzo|Ferrari|0821169853
//3.Mrs|Jenny|Smith|0762458963
//4.Ms|Kerrin|deSilvia|0724587310
//5.Sir|Richard|Branson|0812457896";
//            string actual = string.Empty;
//            string error = string.Empty;

//            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
//            if (string.IsNullOrEmpty(error))
//            {
//                Assert.AreEqual(expected, actual);
//            }
//            else
//            {
//                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
//            }

//        }

//        [TestMethod]
//        public void RecordsetsWithVaryingIndexes_Expected_Split_And_Insert_At_Different_Indexes()
//        {
//            // FIX ME

//            _resultsCollection.Clear();
//            _resultsCollection.Add(new DataSplitDTO("[[recset1(5).field1]]", "Index", "15", 1));
//            _resultsCollection.Add(new DataSplitDTO("[[recset2(2).field2]]", "Index", "15", 2));

//            TestStartNode = new FlowStep
//            {
//                Action = new DsfDataSplitActivity { SourceString = Source, ResultsCollection = _resultsCollection }
//            };

//            TestData = ActivityStrings.DataSplit_preDataList;
//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);

//            UnlimitedObject result = ExecuteProcess();
//            List<string> expected = new List<string> { @"me|TelNo|
//1.Mr"
//                                                     , @"|0795628443
//2."
//                                                     , @"Title|Fname|LNa"
//                                                     , @"|Frank|Williams"
//                                                     , @"Mr|Enzo|Ferrari"
//                                                     , @"Mrs|Jenny|Smith"
//                                                     , @"Ms|Kerrin|deSil" };
//            Assert.AreEqual(expected, result.XmlString);
//        }

//        [TestMethod]
//        public void CleaningArgs_Expected_Clean_Arguments_Coming_In()
//        {
//            // FIX ME

//            _resultsCollection.Add(new DataSplitDTO("[[recset1(5).field1]]", "Index", "15", 1));
//            _resultsCollection.Add(new DataSplitDTO("[[recset2(2).field2]]", "Index", "", 2));
//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();

//            List<string> expected = new List<string> { "Title|Fname|LNa"
//                                                     , @"me|TelNo|
//1.Mr"
//                                                     , "|Frank|Williams"
//                                                     , @"|0795628443
//2."
//                                                     , "Mr|Enzo|Ferrari"  };
//            string error = string.Empty;
//            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);

//            Assert.AreEqual("Mr|Enzo|Ferrari", actual[1]);
//        }

//        [TestMethod]
//        public void MutiRecsetsWithNoIndex_Expected_Split_Append_To_The_Recordsets()
//        {
//            // FIX ME

//            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Index", "15", 1));
//            _resultsCollection.Add(new DataSplitDTO("[[recset1().rec1]]", "Index", "15", 2));

//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataList_NewPreEx, Source, _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();
//            List<string> expected = new List<string> { @"me|TelNo|
//1.Mr"
//                                                     , @"|0795628443
//2."
//                                                     , @"|0821169853
//3."
//                                                     , @"|0762458963
//4."
//                                                     , @"via|0724587310" };
//            string error = string.Empty;
//            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "rec1", out error);
//            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
//        }

//        [TestMethod]
//        public void RecsetWithExistingIndex_Expected_Split_Insert_at_Index_Specified()
//        {
//            // FIX ME

//            _resultsCollection.Add(new DataSplitDTO("[[recset1(2).field1]]", "Index", "15", 1));

//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_DataListShape, Source, _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();

//            List<string> expected = new List<string> { "testData1", @"me|TelNo|
//1.Mr", "testData3" };
//            string error = string.Empty;
//            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);

//            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
//        }


//        [TestMethod]
//        public void RecsetWithStar_Expected_Split_Overwrite_Records_From_Index_1()
//        {

//            // FIX ME

//            _resultsCollection.Add(new DataSplitDTO("[[recset1(*).field1]]", "Index", "15", 1));

//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_DataListShape, Source, _resultsCollection);

//            IDSFDataObject result = ExecuteProcess();

//            List<string> expected = new List<string> { "Title|Fname|LNa"
//                                                    , @"me|TelNo|
//1.Mr"
//                                                    , "|Frank|Williams"
//                                                    , @"|0795628443
//2."
//                                                    , "Mr|Enzo|Ferrari"
//                                                    , @"|0821169853
//3."
//                                                    , "Mrs|Jenny|Smith"
//                                                    , @"|0762458963
//4."
//                                                    , "Ms|Kerrin|deSil"
//                                                    , "via|0724587310"
//                                                    , "5.Sir|Richard|"
//                                                    , "Branson|0812457"
//                                                    , "896" };
//            string error = string.Empty;
//            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);
//            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
//        }

//        [TestMethod]
//        public void RecsetWithZeroAsIndex_Expected_No_Splits()
//        {
//            _resultsCollection.Clear();
//            _resultsCollection.Add(new DataSplitDTO("[[recset1(0).field1]]", "Index", "15", 1));

//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();

//            Assert.IsTrue(_compiler.HasErrors(result.DataListID));

//        }

//        [TestMethod]
//        public void CharsSplitWithNoChars_Expected_No_Splits()
//        {
//            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Chars", "", 1));
//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);

//            IDSFDataObject result = ExecuteProcess();
//            string error = string.Empty;
//            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);
//            Assert.IsTrue(actual.Count == 1 && actual[0] == string.Empty);
//        }

//        [TestMethod]
//        public void IndexSplitWithZeroIndex_Expected_No_Splits()
//        {
//            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Index", "0", 1));
//            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();

//            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
//        }

//        [TestMethod]
//        public void IndexSplitWithNegitiveNumberIndex_Expected_No_Splits()
//        {
//            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Index", "-4", 1));
//            SetupArguments(ActivityStrings.DataSplit_preDataList, ActivityStrings.DataSplit_preDataList, Source, _resultsCollection);

//            IDSFDataObject result = ExecuteProcess();

//            /* Expected Error Message
//            string expected = @"<Error><![CDATA[The following errors occured : 
//No tokenize operations!]]></Error>";
//             */
//            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
//        }

//        [TestMethod]
//        public void RecorsetWithStarAsIndexInSourceString_Expected_Split_For_Last_Value_In_Recordset()
//        {
//            // FIX ME

//            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Space", "", 1));

//            #region Ugle String of Current DataList

//            string currentDL = @"<root>
//	<firstName/>
//	<lastName/>
//	<telNum/>
//	<index/>
//	<recset1>
//		<field1>This is test data to split</field1>
//	</recset1>
//    <recset1>
//		<field1>This is the second test data to split</field1>
//	</recset1>
//	<recset2>
//		<field2/>
//	</recset2>
//	<OutVar1/>
//	<OutVar2/>
//	<OutVar3/>
//	<OutVar4/>
//	<OutVar5/>
//</root>";
//            #endregion Ugly String of Current DataList

//            SetupArguments(currentDL, ActivityStrings.DataSplit_preDataList, "[[recset1(*).field1]]", _resultsCollection);
//            IDSFDataObject result = ExecuteProcess();
//            List<string> expectedRecSet1 = new List<string> { "This is test data to split"
//                                                            , "This is the second test data to split"
//                                                            , "This"
//                                                            , "is"
//                                                            , "test"
//                                                            , "data"
//                                                            , "to"
//                                                            , "split"
//                                                            , "This"
//                                                            , "is"
//                                                            , "the"
//                                                            , "second"
//                                                            , "test"
//                                                            , "data"
//                                                            , "to"
//                                                            , "split" };
//            string error = string.Empty;
//            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);

//            CollectionAssert.AreEqual(expectedRecSet1, actual, new ActivityUnitTests.Utils.StringComparer());
//        }

//        #region GetWizardData Tests

//        [TestMethod]
//        public void GetWizardData_Expected_Correct_IBinaryDataList()
//        {
//            bool passTest = true;
//            IList<DataSplitDTO> _splitCollection = new List<DataSplitDTO>() { new DataSplitDTO("[[result]]", "Index", "5", 1), new DataSplitDTO("[[result1]]", "Index", "1", 2) };

//            DsfDataSplitActivity testAct = new DsfDataSplitActivity { ResultsCollection = _splitCollection, SourceString = "sourceData", ReverseOrder = false };

//            IBinaryDataList binaryDL = testAct.GetWizardData();
//            var recsets = binaryDL.FetchRecordsetEntries();
//            var scalars = binaryDL.FetchScalarEntries();
//            if (recsets.Count != 1 && scalars.Count != 2)
//            {
//                passTest = false;
//            }
//            else
//            {
//                if (recsets[0].Columns.Count != 4)
//                {
//                    passTest = false;
//                }
//            }
//            Assert.IsTrue(passTest);
//        }

//        #endregion GetWizardData Tests

//        #region Private Test Methods

//        private void SetupArguments(string currentDL, string testData, string sourceString, IList<DataSplitDTO> resultCollection)
//        {
//            TestStartNode = new FlowStep
//            {
//                Action = new DsfDataSplitActivity { SourceString = sourceString, ResultsCollection = resultCollection }
//            };

//            CurrentDL = testData;
//            TestData = currentDL;
//        }

//        #endregion Private Test Methods
//    }
//}
