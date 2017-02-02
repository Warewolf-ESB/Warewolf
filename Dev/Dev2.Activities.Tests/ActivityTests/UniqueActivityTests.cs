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
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable AccessToModifiedClosure
namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DataSplitActivityTest
    /// </summary>
    [TestClass]
    public class UniqueActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }


        [TestMethod]
        public void EmptyInFieldsStringExpectedNoUnique()
        {
            const string DataList = "<ADL><recset1>\r\n\t\t<field1/>\r\n\t</recset1>\r\n\t<recset2>\r\n\t\t<field2/>\r\n\t</recset2>\r\n\t<OutVar1/>\r\n\t<OutVar2/>\r\n\t<OutVar3/>\r\n\t<OutVar4/>\r\n\t<OutVar5/>\r\n</ADL>";
            SetupArguments("<root>" + DataList + "</root>", DataList, "", "[[recset1().field1]]", "[[OutVar1]]");
            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);

            // remove test datalist ;)

            Assert.AreEqual("", actual);
        }

        [TestMethod]
        public void RecordsetWithWithNoRecordsInRecSetExpectedUniqueAndAppendRecords()
        {
            const string DataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string DataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<OutVar1/></ADL>";
            SetupArguments("<root>" + DataListWithData + "</root>"
                , DataList
                , "[[recset1().field2]]"
                , "[[recset1().field1]]", "[[recset2().id]]");
            List<string> expected = new List<string> { "1", "2", "5" };

            IDSFDataObject result = ExecuteProcess();

            IList<string> actual;
            string error;

            GetRecordSetFieldValueFromDataList(result.Environment, "recset2", "id", out actual, out error);

            // remove test datalist ;)

            List<string> actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d));
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actualRet, comparer);
        }

        [TestMethod]
        public void ScalarResultExpectedError()
        {
            const string DataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string DataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<OutVar1/></ADL>";
            SetupArguments("<root>" + DataListWithData + "</root>"
                , DataList
                , "[[recset1().field2]]"
                , "[[recset1().field1]]", "[[OutVar1]]");

            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);

            Assert.AreEqual(1,result.Environment.Errors.Count);
            Assert.AreEqual(Warewolf.Resource.Errors.ErrorResource.UniqueResultCannotBeScalarErrorTest, result.Environment.FetchErrors());
        }


        [TestMethod]
        public void RecordsetWithWithRecordsInRecSetExpectedUniqueAndAppendRecords()
        {
            const string DataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string DataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<recset2>" +
                                            "<id>10</id><value>zz</value>" +
                                            "</recset2>" +
                                            "<OutVar1/></ADL>";
            SetupArguments("<root>" + DataListWithData + "</root>"
                , DataList
                , "[[recset1().field2]]"
                , "[[recset1().field1]]", "[[recset2().id]]");
            List<string> expected = new List<string> { "10", "1", "2", "5" };

            IDSFDataObject result = ExecuteProcess();

            IList<string> actual;
            string error;

            GetRecordSetFieldValueFromDataList(result.Environment, "recset2", "id", out actual, out error);

            // remove test datalist ;)

            List<string> actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d));
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actualRet, comparer);
        }

        [TestMethod]
        public void RecordsetWithWithMulitpleRecordsInRecSetExpectedUniqueAndAppendRecords()
        {
            const string DataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string DataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<OutVar1/></ADL>";
            SetupArguments("<root>" + DataListWithData + "</root>"
                , DataList
                , "[[recset1().field2]]"
                , "[[recset1().field1]],[[recset1().field3]]", "[[recset2().id]],[[recset2().value]]");
            List<string> expectedId = new List<string> { "1", "2", "5" };
            List<string> expectedValue = new List<string> { "Test1", "Test2", "Test5" };

            IDSFDataObject result = ExecuteProcess();

            IList<string> actual;
            string error;

            GetRecordSetFieldValueFromDataList(result.Environment, "recset2", "id", out actual, out error);
            List<string> actualRet = new List<string>();

            actual.ToList().ForEach(d => actualRet.Add(d));
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expectedId, actualRet, comparer);
            GetRecordSetFieldValueFromDataList(result.Environment, "recset2", "value", out actual, out error);

            // remove test datalist ;)

            actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d));
            comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expectedValue, actualRet, comparer);
        }


        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure we can use the star notation in the unique tool!")]
        [TestCategory("UniqueTool,UnitTest")]
        public void CanUniqueToolUseStarNotationInResultsFields()
        {
            const string DataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string DataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<OutVar1/></ADL>";
            SetupArguments("<root>" + DataListWithData + "</root>"
                , DataList
                , "[[recset1().field2]]"
                , "[[recset1().field1]],[[recset1().field3]]", "[[recset2(*).id]],[[recset2(*).value]]");
            List<string> expectedId = new List<string> { "1", "2", "5" };
            List<string> expectedValue = new List<string> { "Test1", "Test2", "Test5" };

            IDSFDataObject result = ExecuteProcess();

            IList<string> actual;
            string error;

            GetRecordSetFieldValueFromDataList(result.Environment, "recset2", "id", out actual, out error);
            List<string> actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d));
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expectedId, actualRet, comparer);
            GetRecordSetFieldValueFromDataList(result.Environment, "recset2", "value", out actual, out error);

            // remove test datalist ;)

            actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d));
            comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expectedValue, actualRet, comparer);
        }


        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure we can use mixed star and append notation in the unique tool!")]
        [TestCategory("UniqueTool,UnitTest")]
        public void CanUniqueToolUseStarAndAppendNotationInResultsFields()
        {
            const string DataList = "<ADL><recset1><field1/><field2/><field3/></recset1><recset2><id/><value/></recset2><OutVar1/></ADL>";
            const string DataListWithData = "<ADL>" +
                                            "<recset1>" +
                                            "<field1>1</field1><field2>a</field2><field3>Test1</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>2</field1><field2>b</field2><field3>Test2</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>3</field1><field2>a</field2><field3>Test3</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>4</field1><field2>a</field2><field3>Test4</field3>" +
                                            "</recset1>" +
                                            "<recset1>" +
                                            "<field1>5</field1><field2>c</field2><field3>Test5</field3>" +
                                            "</recset1>" +
                                            "<recset2><id>99</id><value></value></recset2>" +
                                            "<OutVar1/></ADL>";
            SetupArguments("<root>" + DataListWithData + "</root>"
                , DataList
                , "[[recset1().field2]]"
                , "[[recset1().field1]],[[recset1().field3]]", "[[recset2().id]],[[recset2(*).value]]");
            List<string> expectedId = new List<string> { "99", "1", "2", "5" };
            List<string> expectedValue = new List<string> { "Test1", "Test2", "Test5", "" };

            IDSFDataObject result = ExecuteProcess();

            IList<string> actual;
            string error;

            GetRecordSetFieldValueFromDataList(result.Environment, "recset2", "id", out actual, out error);
            List<string> actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d));
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expectedId, actualRet, comparer);
            GetRecordSetFieldValueFromDataList(result.Environment, "recset2", "value", out actual, out error);

            // remove test datalist ;)

            actualRet = new List<string>();
            actual.ToList().ForEach(d =>
            {
                try
                {
                    actualRet.Add(d);
                }
                catch(Exception)
                {
                    actualRet.Add("");
                }
            });
            comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expectedValue, actualRet, comparer);
        }

        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_UpdateForEachInputs")]
        public void DsfUniqueActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(InFields, act.InFields);
            Assert.AreEqual(ResultFields, act.ResultFields);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_UpdateForEachInputs")]
        public void DsfUniqueActivity_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };

            var tuple1 = new Tuple<string, string>(ResultFields, "Test");
            var tuple2 = new Tuple<string, string>(InFields, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.InFields);
            Assert.AreEqual("Test", act.ResultFields);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_UpdateForEachOutputs")]
        public void DsfUniqueActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };

            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(Result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_UpdateForEachOutputs")]
        public void DsfUniqueActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(Result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_UpdateForEachOutputs")]
        public void DsfUniqueActivity_UpdateForEachOutputs_1Updates_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };

            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_GetForEachInputs")]
        public void DsfUniqueActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, dsfForEachItems.Count);
            Assert.AreEqual(InFields, dsfForEachItems[0].Name);
            Assert.AreEqual(InFields, dsfForEachItems[0].Value);
            Assert.AreEqual(ResultFields, dsfForEachItems[1].Name);
            Assert.AreEqual(ResultFields, dsfForEachItems[1].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfUniqueActivity_GetForEachOutputs")]
        public void DsfUniqueActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(Result, dsfForEachItems[0].Name);
            Assert.AreEqual(Result, dsfForEachItems[0].Value);
        }

       
        
        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string inFields, string resultFields, string result)
        {
            var unique = new DsfUniqueActivity { InFields = inFields, ResultFields = resultFields, Result = result };
            TestStartNode = new FlowStep
            {
                Action = unique
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
