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
using System.Linq;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Common.State;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;

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
        [Timeout(60000)]
        public void EmptyInFieldsStringExpectedNoUnique()
        {
            const string DataList = "<ADL><recset1>\r\n\t\t<field1/>\r\n\t</recset1>\r\n\t<recset2>\r\n\t\t<field2/>\r\n\t</recset2>\r\n\t<OutVar1/>\r\n\t<OutVar2/>\r\n\t<OutVar3/>\r\n\t<OutVar4/>\r\n\t<OutVar5/>\r\n</ADL>";
            SetupArguments("<root>" + DataList + "</root>", DataList, "", "[[recset1().field1]]", "[[OutVar1]]");
            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual("", actual);
        }

        [TestMethod]
        [Timeout(60000)]
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
            var expected = new List<string> { "1", "2", "5" };

            var result = ExecuteProcess();

            GetRecordSetFieldValueFromDataList(result.Environment, "recset2", "id", out IList<string> actual, out string error);

            // remove test datalist ;)

            var actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d));
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actualRet, comparer);
        }

        [TestMethod]
        [Timeout(60000)]
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

            var result = ExecuteProcess();

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out string actual, out string error);

            Assert.AreEqual(1,result.Environment.Errors.Count);
            Assert.AreEqual(Warewolf.Resource.Errors.ErrorResource.UniqueResultCannotBeScalarErrorTest, result.Environment.FetchErrors());
        }


        [TestMethod]
        [Timeout(60000)]
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
            var expected = new List<string> { "10", "1", "2", "5" };

            var result = ExecuteProcess();

            GetRecordSetFieldValueFromDataList(result.Environment, "recset2", "id", out IList<string> actual, out string error);

            // remove test datalist ;)

            var actualRet = new List<string>();
            actual.ToList().ForEach(d => actualRet.Add(d));
            var comparer = new ActivityUnitTests.Utils.StringComparer();
            CollectionAssert.AreEqual(expected, actualRet, comparer);
        }

        [TestMethod]
        [Timeout(60000)]
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
            var expectedId = new List<string> { "1", "2", "5" };
            var expectedValue = new List<string> { "Test1", "Test2", "Test5" };

            var result = ExecuteProcess();

            GetRecordSetFieldValueFromDataList(result.Environment, "recset2", "id", out IList<string> actual, out string error);
            var actualRet = new List<string>();

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
        [Timeout(60000)]
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
            var expectedId = new List<string> { "1", "2", "5" };
            var expectedValue = new List<string> { "Test1", "Test2", "Test5" };

            var result = ExecuteProcess();

            GetRecordSetFieldValueFromDataList(result.Environment, "recset2", "id", out IList<string> actual, out string error);
            var actualRet = new List<string>();
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
        [Timeout(60000)]
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
            var expectedId = new List<string> { "99", "1", "2", "5" };
            var expectedValue = new List<string> { "Test1", "Test2", "Test5", "" };

            var result = ExecuteProcess();

            GetRecordSetFieldValueFromDataList(result.Environment, "recset2", "id", out IList<string> actual, out string error);
            var actualRet = new List<string>();
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

        

        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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


        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("GetOutputs")]
        public void GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            const string InFields = "[[Numeric(1).num]]";
            const string ResultFields = "Up";
            const string Result = "[[res]]";
            var act = new DsfUniqueActivity { InFields = InFields, ResultFields = ResultFields, Result = Result };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[res]]", outputs[0]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("Errors")]
        public void DsfUniqueActivity_ResultIsEmpty()
        {
            const string DataList = "<ADL><recset1>\r\n\t\t<field1/>\r\n\t</recset1>\r\n\t<recset2>\r\n\t\t<field2/>\r\n\t</recset2>\r\n\t<OutVar1/>\r\n\t<OutVar2/>\r\n\t<OutVar3/>\r\n\t<OutVar4/>\r\n\t<OutVar5/>\r\n</ADL>";
            SetupArguments("<root>" + DataList + "</root>", DataList, "", "[[recset1().field1]]",null);
            var result = ExecuteProcess();
            Assert.AreEqual("Invalid In fields", result.Environment.FetchErrors());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        public void GivenEmptyStringAndName_ExecutionEnvironmentIsValidRecordSetIndex_ShouldReturn()
        {
            ExecutionEnvironment _environment;
            _environment = new ExecutionEnvironment();
            Assert.IsNotNull(_environment);
            Assert.IsTrue(ExecutionEnvironment.IsValidRecordSetIndex("[[rec().a]]"));
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("DsfUniqueActivity_GetState")]
        public void DsfUniqueActivity_GetState()
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
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(3, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "InFields",
                    Type = StateVariable.StateType.Input,
                    Value = InFields
                },
                new StateVariable
                {
                    Name = "ResultFields",
                    Type = StateVariable.StateType.Input,
                    Value = ResultFields
                },
                new StateVariable
                {
                    Name = "Result",
                    Type = StateVariable.StateType.Output,
                    Value = Result
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
        #region Private Test Methods

        void SetupArguments(string currentDL, string testData, string inFields, string resultFields, string result)
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
