/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ActivityUnitTests;
using Dev2.Common.State;
using Dev2.DynamicServices;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Moq;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class DataSplitActivityTest : BaseActivityUnitTest
    {
        IList<DataSplitDTO> _resultsCollection = new List<DataSplitDTO>();
        readonly string _source = ActivityStrings.DataSplit_SourceString;
        
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            if (_resultsCollection == null)
            {
                _resultsCollection = new List<DataSplitDTO>();
            }
            _resultsCollection.Clear();
        }

        #endregion
        
        #region Funky Language

        [TestMethod]
        [Timeout(60000)]
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
                           _resultsCollection,
                           true);

            //------------Execute Test---------------------------
            var result = ExecuteProcess();

            var col1List = RetrieveAllRecordSetFieldValuesSkipEmpty(result.Environment, "rs", "col1", out string error);
            var col2List = RetrieveAllRecordSetFieldValuesSkipEmpty(result.Environment, "rs", "col2", out error);
            var col3List = RetrieveAllRecordSetFieldValuesSkipEmpty(result.Environment, "rs", "col3", out error);
            var dataList = RetrieveAllRecordSetFieldValuesSkipEmpty(result.Environment, "rs", "data", out error);

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


        [TestMethod]
        [Timeout(60000)]
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
                           _resultsCollection,
                           true);

            //------------Execute Test---------------------------
            var result = ExecuteProcess();

            var col1List = RetrieveAllRecordSetFieldValuesSkipEmpty(result.Environment, "rs", "col1", out string error);
            var col2List = RetrieveAllRecordSetFieldValuesSkipEmpty(result.Environment, "rs", "col2", out error);
            var col3List = RetrieveAllRecordSetFieldValuesSkipEmpty(result.Environment, "rs", "col3", out error);
            var dataList = RetrieveAllRecordSetFieldValuesSkipEmpty(result.Environment, "rs", "data", out error);

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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory("DsfDataMergeActivity_Execute")]
        public void DsfDataSplitActivity_Execute_WhenUsingStarAndMixedSplitType_WithEmptyLine_ExpectCorrectSplit()
        {

            //------------Setup for test--------------------------

            _resultsCollection.Add(new DataSplitDTO("[[rs().col1]]", "Chars", "|", 1));
            _resultsCollection.Add(new DataSplitDTO("[[rs().col2]]", "Chars", "|", 2));
            _resultsCollection.Add(new DataSplitDTO("[[rs().col3]]", "New Line", "", 3));
            _resultsCollection.Add(new DataSplitDTO("[[rs(*).data]]", "New Line", "", 4));
            _resultsCollection.Add(new DataSplitDTO("[[rs(*).data]]", "New Line", "", 5));

            SetupArguments("<root><ADL><testData>RSA ID|FirstName|LastName" + Environment.NewLine +
                           "13456456789|Samantha Some|Jones" + Environment.NewLine +
                           Environment.NewLine +
                           "09123456646|James|Apple</testData></ADL></root>",
                           "<ADL><rs><col1/><col2/><col3/><data/></rs><testData/></ADL>",
                           "[[testData]]",
                           _resultsCollection,
                           true);

            //------------Execute Test---------------------------
            var result = ExecuteProcess();

            var col1List = RetrieveAllRecordSetFieldValuesSkipEmpty(result.Environment, "rs", "col1", out string error);
            var col2List = RetrieveAllRecordSetFieldValuesSkipEmpty(result.Environment, "rs", "col2", out error);
            var col3List = RetrieveAllRecordSetFieldValuesSkipEmpty(result.Environment, "rs", "col3", out error);
            var dataList = RetrieveAllRecordSetFieldValuesSkipEmpty(result.Environment, "rs", "data", out error);

            // remove test datalist ;)

            //------------Assert Results-------------------------

            var col1Expected = new List<string> { "RSA ID", "09123456646" };
            var col2Expected = new List<string> { "FirstName", "James" };
            var col3Expected = new List<string> { "LastName", "Apple" };
            var dataExpected = new List<string> { "13456456789|Samantha Some|Jones" };

            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(col1Expected, col1List, comparer);
            CollectionAssert.AreEqual(col2Expected, col2List, comparer);
            CollectionAssert.AreEqual(col3Expected, col3List, comparer);
            CollectionAssert.AreEqual(dataExpected, dataList, comparer);
        }

        #endregion

        [TestMethod]
        [Timeout(60000)]
        public void EmptySourceString_Expected_No_Splits()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, "", _resultsCollection);
            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out string actual, out string error);
            // remove test datalist ;)

            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        public void Scalar_Expected_Split_And_Insert_To_Scalar()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);
            var result = ExecuteProcess();
            const string expected = @"Title|Fname|LNa";
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual, "Got " + actual + " expected " + expected);

        }

        [TestMethod]
        [Timeout(60000)]
        public void MultipleScalars_Expected_Split_And_Insert_Mutiple_Scalars()
        {

            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
            _resultsCollection.Add(new DataSplitDTO("[[OutVar2]]", "Index", "10", 2));
            _resultsCollection.Add(new DataSplitDTO("[[OutVar3]]", "Index", "5", 3));
            _resultsCollection.Add(new DataSplitDTO("[[OutVar4]]", "Index", "15", 4));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source.Replace("\r", ""), _resultsCollection);

            var result = ExecuteProcess();

            var expected = new List<string> { @"Title|Fname|LNa", "me|TelNo|", "1.Mr|", "Frank|Williams|" };
            var actual = new List<string>();

            for (int i = 1; i <= 4; i++)
            {
                GetScalarValueFromEnvironment(result.Environment, "OutVar" + i, out string returnVal, out string error);
                actual.Add(returnVal.Trim());
            }
            // remove test datalist ;)

            var comparer = new ActivityUnitTests.Utils.StringComparer();
            Assert.AreEqual(4, actual.Count());
            Assert.AreEqual(4, expected.Count());
            CollectionAssert.AreEqual(expected, actual, comparer);
        }

        [TestMethod]
        [Timeout(60000)]
        public void MixedScalarsAndRecordsetWithIndex_Expected_Split_Insert_Mutiple_Scalar_And_Recordsets()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
            _resultsCollection.Add(new DataSplitDTO("[[recset1(2).field1]]", "Index", "15", 2));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);

            var result = ExecuteProcess();
            var expected = new List<string> { @""
                                                     , @"Branson|0812457"
                                                     };

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out string actualScalar, out string error);

            Assert.AreEqual("896", actualScalar);

            GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out IList<string> actualRecordSet, out error);

            // remove test datalist ;)

            var actual = actualRecordSet.Select(entry => entry).ToList();
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actual, comparer);
        }

        [TestMethod]
        [Timeout(60000)]
        public void MixedScalarsAndRecordsetWithoutIndex_Expected_Split_To_End_Inserting_Mutiple_Scalar_And_Recordsets()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Index", "15", 2));

            SetupArguments("<root></root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection, true);

            var result = ExecuteProcess();
            var expected = new List<string> { @"me|TelNo|
1.Mr"
                                                     , @"|0795628443
2."
                                                     , @"|0821169853
3."
                                                     , @"|0762458963
4."
                                                     , @"via|0724587310"
                                                     , @"Branson|0812457"};
            var actual = new List<string>();
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out string actualScalar, out string error);
            Assert.AreEqual("896", actualScalar);

            actual.AddRange(RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error));

            // remove test datalist ;)

            var foo = actual.ToArray();
            actual.Clear();

            actual.AddRange(foo.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        [Timeout(60000)]
        public void NoResultVariableInFirst_Expected_Still_Spilt_But_Dont_Insert_For_First()
        {

            _resultsCollection.Add(new DataSplitDTO("", "Index", "15", 1));
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 2));
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection, true);

            var result = ExecuteProcess();
            const string expected = @"me|TelNo|
1.Mr";
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out string actual, out string error);
            // remove test datalist ;)
            Assert.AreEqual(expected, actual, "Got " + actual + " expected " + expected);
        }

        [TestMethod]
        [Timeout(60000)]
        public void IndexTypeSplit_Expected_Split_At_An_Index()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Index", "15", 1));
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);
            var result = ExecuteProcess();

            const string expected = "Title|Fname|LNa";

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out string actual, out string error);
            // remove test datalist ;)

            Assert.AreEqual(expected, actual, "Got " + actual + " but expected " + expected);
        }

        [TestMethod]
        [Timeout(60000)]
        public void CharsTypeSplitSingle_Expected_Split_Once_At_Chars()
        {

            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Chars", "|", 1));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);

            var result = ExecuteProcess();

            const string expected = @"Title";

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out string actual, out string error);
            // remove test datalist ;)

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        public void BlankSpaceTypeSplitSingle_Expected_Split_At_BlankSpace()
        {
            _resultsCollection.Clear();
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Space", "", 1));

            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection, true);

            var result = ExecuteProcess();

            const string expected = @"Title|Fname|LName|TelNo|
1.Mr|Frank|Williams|0795628443
2.Mr|Enzo|Ferrari|0821169853
3.Mrs|Jenny|Smith|0762458963
4.Ms|Kerrin|deSilvia|0724587310
5.Sir|Richard|Branson|0812457896";

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out string actual, out string error);
            // remove test datalist ;)

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        [Timeout(60000)]
        public void BlankSpaceTypeSplitMultiple_Expected_Split_Mutiple_At_BlankSpace()
        {

            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "Space", "", 1));
            _resultsCollection.Add(new DataSplitDTO("[[OutVar2]]", "Space", "", 2));
            const string source = "Test source string with spaces";
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, source, _resultsCollection);

            var result = ExecuteProcess();

            var expected = new List<string> { "Test", "source" };
            var actual = new List<string>();

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out string tempActual, out string error);
            actual.Add(tempActual);
            GetScalarValueFromEnvironment(result.Environment, "OutVar2", out tempActual, out error);
            actual.Add(tempActual);
            // remove test datalist ;)

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        [Timeout(60000)]
        public void NewLineTypeSplitWindows_Expected_Split_On_Windows_NewLine()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "New Line", "", 1));
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection);
            var result = ExecuteProcess();

            const string expected = @"Title|Fname|LName|TelNo|";
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual, "Got " + actual + " expected " + expected);

        }
        
        [TestMethod]
        [Timeout(60000)]
        public void TabTypeSplit_Expected_Split_On_Tab()
        {
            _resultsCollection.Add(new DataSplitDTO("[[recset2().field2]]", "Tab", "", 1));
            const string sourceString = "Test	Data	To	Split";
            SetupArguments("<root></root>", ActivityStrings.DataSplit_preDataList, sourceString, _resultsCollection);
            var result = ExecuteProcess();

            var expected = new List<string> { "Test", "Data", "To", "Split" };
            var actual = RetrieveAllRecordSetFieldValues(result.Environment, "recset2", "field2", out string error);

            // remove test datalist ;)

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        [Timeout(60000)]
        public void EndTypeSplit_Expected_Split_On_End_Of_String()
        {
            _resultsCollection.Add(new DataSplitDTO("[[OutVar1]]", "End", "", 1));
            SetupArguments("<root>" + ActivityStrings.DataSplit_preDataList + "</root>", ActivityStrings.DataSplit_preDataList, _source, _resultsCollection, true);
            var result = ExecuteProcess();

            const string expected = @"Title|Fname|LName|TelNo|
1.Mr|Frank|Williams|0795628443
2.Mr|Enzo|Ferrari|0821169853
3.Mrs|Jenny|Smith|0762458963
4.Ms|Kerrin|deSilvia|0724587310
5.Sir|Richard|Branson|0812457896";

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out string actual, out string error);
            // remove test datalist ;)

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
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

            var result = ExecuteProcess();

            var actual1 = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out string error);
            var actual2 = RetrieveAllRecordSetFieldValues(result.Environment, "recset2", "field2", out error);

            // remove test datalist ;)

            Assert.AreEqual("Branson", actual1[0]);
            Assert.AreEqual("0812457896", actual2[0]);
        }

        [TestMethod]
        [Timeout(60000)]
        public void MutiRecsetsWithNoIndex_Expected_Split_Append_To_The_Recordsets()
        {
            _resultsCollection.Add(new DataSplitDTO("[[recset1().field1]]", "Index", "15", 1));
            _resultsCollection.Add(new DataSplitDTO("[[recset1().rec1]]", "Index", "15", 2));

            SetupArguments("<root></root>", ActivityStrings.DataList_NewPreEx, _source, _resultsCollection, true);
            var result = ExecuteProcess();
            var expected = new List<string> { @"me|TelNo|
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
            var actual = RetrieveAllRecordSetFieldValuesSkipEmpty(result.Environment, "recset1", "rec1", out string error);
            actual.AddRange(RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error));

            // remove test datalist ;)

            var foo = actual.ToArray();
            actual.Clear();

            actual.AddRange(foo.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }


        [TestMethod]
        [Timeout(60000)]
        public void RecsetWithStar_Expected_Split_Overwrite_Records_From_Index_1()
        {

            _resultsCollection.Add(new DataSplitDTO("[[recset1(*).field1]]", "Index", "15", 1));

            SetupArguments(@"<root></root>", ActivityStrings.DataSplit_DataListShape, _source, _resultsCollection);

            var result = ExecuteProcess();

            var expected = new List<string> { "Title|Fname|LNa"
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
            var actual = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out string error);

            // remove test datalist ;)

            var foo = actual.ToArray();
            actual.Clear();
            actual.AddRange(foo.Select(f => f.Trim()));

            var exp = expected.ToString();
            var act = actual.ToString();

            FixBreaks(ref exp, ref act);
            Assert.AreEqual(exp, act);
        }
        void FixBreaks(ref string expected, ref string actual)
        {
            expected = new StringBuilder(expected).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
            actual = new StringBuilder(actual).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
        }

        [TestMethod]
        [Timeout(60000)]
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
            var result = ExecuteProcess();
            var expectedRecSet1 = new List<string> { "This is test data to split"
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
            var actual = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out string error);

            // remove test datalist ;)

            CollectionAssert.AreEqual(expectedRecSet1, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataSplitActivity_UpdateForEachInputs")]
        public void DsfDataSplitActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            IList<DataSplitDTO> resultsCollection = new List<DataSplitDTO> { new DataSplitDTO("[[CompanyName]]", "Index", "2", 1) };
            var act = new DsfDataSplitActivity { SourceString = "[[CompanyName]]", ResultsCollection = resultsCollection };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("[[CompanyName]]", act.ResultsCollection[0].OutputVariable);
        }

        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDataSplitActivity_GetState")]
        public void DsfDataSplitActivity_GetState_ReturnsStateVariable()
        {
            IList<DataSplitDTO> resultsCollection = new List<DataSplitDTO> { new DataSplitDTO("[[CompanyName]]", "Index", "2", 1) };
            var act = new DsfDataSplitActivity { SourceString = "[[CompanyName]]", ReverseOrder = true, SkipBlankRows = true, ResultsCollection = resultsCollection };

            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(4, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "SourceString",
                    Type = StateVariable.StateType.Input,
                    Value = "[[CompanyName]]"
                },
                new StateVariable
                {
                    Name = "ReverseOrder",
                    Type = StateVariable.StateType.Input,
                    Value = "True"
                },
                new StateVariable
                {
                    Name = "SkipBlankRows",
                    Type = StateVariable.StateType.Input,
                    Value = "True"
                },
                new StateVariable
                {
                    Name="ResultsCollection",
                    Type = StateVariable.StateType.Output,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(resultsCollection)
                }
            };

            var iter = act.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
                );

            //------------Assert Results-------------------------
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory("DsfDataSplitActivity_GetState")]
        public void DsfDataSplitActivity_EmptyLines_ShouldExist()
        {
            var sourceStringLines = new string[] {
                "",
                "two",
                "three",
                "",
                "five",
                "",
                "",
                "eight",
                "next blank is index 10",
                ""
            };

            _resultsCollection.Add(new DataSplitDTO("[[rs().lines]]", "New Line", "", 1));
            var sourceString = string.Join(Environment.NewLine, sourceStringLines);


            SetupArguments("<root><ADL><testData>" + sourceString + "</testData></ADL></root>",
                           "<ADL><rs><col1/><col2/><col3/><data/></rs><testData/></ADL>",
                           "[[testData]]",
                           _resultsCollection);

            //------------Execute Test---------------------------
            var result = ExecuteProcess();

            var col1List = RetrieveAllRecordSetFieldValues(result.Environment, "rs", "lines", out string error);

            Assert.AreEqual(sourceStringLines.Length, col1List.Count);

            var len = sourceStringLines.Length;
            for (int i = 0; i < len; i++)
            {
                Assert.AreEqual(sourceStringLines[i], col1List[i]);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory("DsfDataSplitActivity_GetState")]
        public void DsfDataSplitActivity_EmptyLinesUnix_ShouldExist()
        {
            var sourceStringLines = new string[] {
                "",
                "two",
                "three",
                "",
                "five",
                "",
                "",
                "eight",
                "next blank is index 10",
                ""
            };

            _resultsCollection.Add(new DataSplitDTO("[[rs().lines]]", "New Line", "", 1));
            var sourceString = string.Join("\n", sourceStringLines);


            SetupArguments("<root><ADL><testData>" + sourceString + "</testData></ADL></root>",
                           "<ADL><rs><col1/><col2/><col3/><data/></rs><testData/></ADL>",
                           "[[testData]]",
                           _resultsCollection);

            //------------Execute Test---------------------------
            var result = ExecuteProcess();

            var col1List = RetrieveAllRecordSetFieldValues(result.Environment, "rs", "lines", out string error);

            Assert.AreEqual(sourceStringLines.Length, col1List.Count);

            var len = sourceStringLines.Length;
            for (int i = 0; i < len; i++)
            {
                Assert.AreEqual(sourceStringLines[i], col1List[i]);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("DsfDataSplitActivity_EmptyColumn")]
        public void DsfDataSplitActivity_EmptyColumn()
        {
            _resultsCollection.Clear();
            _resultsCollection.Add(new DataSplitDTO("[[rs().Title]]", "Chars", "|", 1));
            _resultsCollection.Add(new DataSplitDTO("[[rs().Fname]]", "Chars", "|", 2));
            _resultsCollection.Add(new DataSplitDTO("[[rs().LName]]", "Chars", "|", 3));
            _resultsCollection.Add(new DataSplitDTO("[[rs().TelNo]]", "New Line", "", 4));

            SetupArguments("<root><ADL><testData>Title|Fname|LName|TelNo" + Environment.NewLine +
                           "Mr|Mike|Jones|11111" + Environment.NewLine +
                           "Mrs|Samantha||" + Environment.NewLine +
                           "Mr|Steve||" + Environment.NewLine +
                           "Mr|James|Apple|33333</testData></ADL></root>",
                           "<ADL><rs><Title/><Fname/><LName/><TelNo/><data/></rs><testData/></ADL>",
                           "[[testData]]",
                           _resultsCollection,
                           true);

            //------------Execute Test---------------------------
            var result = ExecuteProcess();

            var col1List = RetrieveAllRecordSetFieldValues(result.Environment, "rs", "Title", out string error);
            var col2List = RetrieveAllRecordSetFieldValues(result.Environment, "rs", "Fname", out error);
            var col3List = RetrieveAllRecordSetFieldValues(result.Environment, "rs", "LName", out error);
            var col4List = RetrieveAllRecordSetFieldValues(result.Environment, "rs", "TelNo", out error);
            
            //------------Assert Results-------------------------

            var col1Expected = new List<string> { "Title", "Mr" , "Mrs", "Mr", "Mr" };
            var col2Expected = new List<string> { "Fname","Mike","Samantha","Steve","James" };
            var col3Expected = new List<string> { "LName" ,"Jones", "","", "Apple"};
            var col4Expected = new List<string> { "TelNo","11111","","","33333" };
            
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(col1Expected, col1List, comparer);
            CollectionAssert.AreEqual(col2Expected, col2List, comparer);
            CollectionAssert.AreEqual(col3Expected, col3List, comparer);
            CollectionAssert.AreEqual(col4Expected, col4List, comparer);

            var len = col1Expected.Count;
            for (int i = 0; i < len; i++)
            {
                Assert.AreEqual(col1Expected[i], col1List[i]);
            }
            len = col2Expected.Count;
            for (int i = 0; i < len; i++)
            {
                Assert.AreEqual(col2Expected[i], col2List[i]);
            }
            len = col3Expected.Count;
            for (int i = 0; i < len; i++)
            {
                Assert.AreEqual(col3Expected[i], col3List[i]);
            }
            len = col4Expected.Count;
            for (int i = 0; i < len; i++)
            {
                Assert.AreEqual(col4Expected[i], col4List[i]);
            }
        }

        #region Private Test Methods

        void SetupArguments(string currentDL, string testData, string sourceString, IList<DataSplitDTO> resultCollection, bool skipBlankRows = false)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDataSplitActivity { SourceString = sourceString, ResultsCollection = resultCollection, SkipBlankRows = skipBlankRows }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
