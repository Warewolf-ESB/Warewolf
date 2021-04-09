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
using Dev2.Common;
using Dev2.Common.State;
using Dev2.Interfaces;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    
    public class ReplaceActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Replace Positive Tests


        //Added for - Bug 9937
        [TestMethod]
        [Timeout(60000)]
        [Owner("Massimo Guerrera")]
        [Description("Replace special chars")]
        [TestCategory("DsfReplaceActivity")]
        public void ReplaceActivity_UnitTest_WithAllSpecialChars_ReplaceSuccessful()
        {
            SetupArguments(ActivityStrings.ReplaceSpecialCharsDataListWithData, ActivityStrings.ReplaceSpecialCharsDataListShape, "[[SpecialChar]]", @"\*+?|{[()^$# ", "It Worked", "[[res]]", false);

            var result = ExecuteProcess();
            const string expected = @"1";
            GetScalarValueFromEnvironment(result.Environment, "res", out string actual, out string error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromEnvironment(result.Environment, "SpecialChar", out string replacedRes, out error);
                // remove test datalist ;)

                Assert.AreEqual("It Worked", replacedRes);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void Replace_In_Two_Recordset_Fields_Expected_Two_Replaces_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(*).field1]],[[Customers(*).FirstName]]", "Barney", "Wallis", "[[res]]", false);

            var result = ExecuteProcess();
            const string expected = @"2";

            GetScalarValueFromEnvironment(result.Environment, "res", out string actual, out string error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out IList<string> dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0]);
                GetRecordSetFieldValueFromDataList(result.Environment, "Customers", "FirstName", out dataListItems, out error);
                // remove test datalist ;)
                Assert.AreEqual("Wallis", dataListItems[0]);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void Replace_In_Two_Recordset_Fields_With_Space_Between_Expected_Two_Replaces_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(*).field1]], [[Customers(*).FirstName]]", "Barney", "Wallis", "[[res]]", false);

            var result = ExecuteProcess();
            const string expected = @"2";

            GetScalarValueFromEnvironment(result.Environment, "res", out string actual, out string error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out IList<string> dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0]);
                GetRecordSetFieldValueFromDataList(result.Environment, "Customers", "FirstName", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0]);

                // remove test datalist ;)
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void Replace_In_Scalar_Field_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[CompanyName]]", "Dev2", "TheUnlimted", "[[res]]", false);

            var result = ExecuteProcess();
            const string expected = @"1";


            GetScalarValueFromEnvironment(result.Environment, "res", out string actual, out string error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromEnvironment(result.Environment, "CompanyName", out actual, out error);
                // remove test datalist ;)

                Assert.AreEqual("TheUnlimted", actual);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void Replace_In_Scalar_Field_With_CaseMatch_On_Expected_Zero_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[CompanyName]]", "dev2", "TheUnlimted", "[[res]]", true);

            var result = ExecuteProcess();
            const string expected = @"0";
            GetScalarValueFromEnvironment(result.Environment, "res", out string actual, out string error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromEnvironment(result.Environment, "CompanyName", out actual, out error);
                // remove test datalist ;)
                Assert.AreEqual("Dev2", actual);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void Replace_Recordset_Fields_With_CaseMatch_On_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(*).field1]],[[Customers(*).FirstName]]", "Barney", "Wallis", "[[res]]", true);

            var result = ExecuteProcess();
            const string expected = @"1";
            GetScalarValueFromEnvironment(result.Environment, "res", out string actual, out string error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out IList<string> dataListItems, out error);
                Assert.AreEqual("barney", dataListItems[0]);
                GetRecordSetFieldValueFromDataList(result.Environment, "Customers", "FirstName", out dataListItems, out error);

                // remove test datalist ;)

                Assert.AreEqual("Wallis", dataListItems[0]);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void Replace_Recordset_Fields_With_Index_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(1).field1]],[[Customers(2).FirstName]]", "Barney", "Wallis", "[[res]]", false);

            var result = ExecuteProcess();
            const string expected = @"1";
            GetScalarValueFromEnvironment(result.Environment, "res", out string actual, out string error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out IList<string> dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0]);
                GetRecordSetFieldValueFromDataList(result.Environment, "Customers", "FirstName", out dataListItems, out error);

                // remove test datalist ;)

                Assert.AreEqual("Barney", dataListItems[0]);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void Replace_Scalar_With_Backlash_Expected_One_Replace()
        {
            SetupArguments(@"<DataList><Thing>testlol\</Thing><Res></Res></DataList>", @"<DataList><Thing></Thing><Res></Res></DataList>", @"[[Thing]]", @"lol\", @"Wallis", "[[Res]]", false);

            var result = ExecuteProcess();
            const string expected = @"1";
            GetScalarValueFromEnvironment(result.Environment, "Res", out string actual, out string error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromEnvironment(result.Environment, "Thing", out actual, out error);

                // remove test datalist ;)

                Assert.AreEqual("testWallis", actual);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }
        
        [TestMethod]
        [Timeout(60000)]
        public void ReplaceInAllRecordsetFieldsExpectedTwoReplacesSuccess()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData.Replace("f2r2", "barney"), ActivityStrings.ReplaceDataListShape, "[[recset1(*)]]", "Barney", "Wallis", "[[res]]", false);

            var result = ExecuteProcess();
            const string expected = @"2";

            GetScalarValueFromEnvironment(result.Environment, "res", out string actual, out string error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out IList<string> dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0]);
                GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field2", out dataListItems, out error);
                // remove test datalist ;)
                Assert.AreEqual("Wallis", dataListItems[1]);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

     
        #endregion Replace Positive Tests

        #region Replace Negative Tests

        [TestMethod]
        [Timeout(60000)]
        public void ReplaceRawStringAsInputExpectedFriendlyErrorMessage()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "rawstringdata", "Barney", "Wallis", "[[res]]", false);

            var result = ExecuteProcess();
            const string expected = @"Please insert only variables into Fields To Search";
            GetScalarValueFromEnvironment(result.Environment, GlobalConstants.ErrorPayload, out string actual, out string error);
            // remove test datalist ;)
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void Replace_Recordset_Field_With_Negative_Index_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(-1).field1]]", "Barney", "Wallis", "[[res]]", false);

            var result = ExecuteProcess();
            const string expected = @"Recordset index [ -1 ] is not greater than zero";
            GetScalarValueFromEnvironment(result.Environment, GlobalConstants.ErrorPayload, out string actual, out string error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void Replace_Recordset_Field_With_Zero_Index_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListShape, ActivityStrings.ReplaceDataListWithData, "[[recset1(0).field1]]", "Barney", "Wallis", "[[res]]", false);

            var result = ExecuteProcess();
            const string expected = @"Recordset index [ 0 ] is not greater than zero";
            GetScalarValueFromEnvironment(result.Environment, GlobalConstants.ErrorPayload, out string actual, out string error);
            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        #endregion Replace Negative Tests

        

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfReplaceActivity_UpdateForEachInputs")]
        public void DsfReplaceActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(FieldsToSearch, act.FieldsToSearch);
            Assert.AreEqual(Find, act.Find);
            Assert.AreEqual(ReplaceWith, act.ReplaceWith);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetOutputs")]
        public void GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[res]]", outputs[0]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfReplaceActivity_UpdateForEachInputs")]
        public void DsfReplaceActivity_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            var tuple1 = new Tuple<string, string>(FieldsToSearch, "Test");
            var tuple2 = new Tuple<string, string>(Find, "Test2");
            var tuple3 = new Tuple<string, string>(ReplaceWith, "Test3");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2, tuple3 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.Find);
            Assert.AreEqual("Test", act.FieldsToSearch);
            Assert.AreEqual("Test3", act.ReplaceWith);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfReplaceActivity_UpdateForEachOutputs")]
        public void DsfReplaceActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(Result, act.Result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfReplaceActivity_UpdateForEachOutputs")]
        public void DsfReplaceActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

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
        [TestCategory("DsfReplaceActivity_UpdateForEachOutputs")]
        public void DsfReplaceActivity_UpdateForEachOutputs_1Updates_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfReplaceActivity_GetForEachInputs")]
        public void DsfReplaceActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(3, dsfForEachItems.Count);
            Assert.AreEqual(FieldsToSearch, dsfForEachItems[0].Name);
            Assert.AreEqual(FieldsToSearch, dsfForEachItems[0].Value);
            Assert.AreEqual(Find, dsfForEachItems[1].Name);
            Assert.AreEqual(Find, dsfForEachItems[1].Value);
            Assert.AreEqual(ReplaceWith, dsfForEachItems[2].Name);
            Assert.AreEqual(ReplaceWith, dsfForEachItems[2].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfReplaceActivity_GetForEachOutputs")]
        public void DsfReplaceActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(Result, dsfForEachItems[0].Name);
            Assert.AreEqual(Result, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("DsfReplaceActivity_Replace_Recordset_Fields")]
        public void DsfReplaceActivity_TestingIsSingleValueRule()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[CompanyName]]", "Dev2", "TheUnlimted", "[[rec]],[[bob]]", false);

            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out string actual, out string error);

            Assert.AreEqual(2, result.Environment.Errors.Count);
            Assert.AreEqual("The result field only allows a single result\r\nScalar value { rec } is NULL", result.Environment.FetchErrors());
        }


        [TestMethod]
        [Timeout(60000)]
        [TestCategory("DsfReplaceActivity_UpdateForEachOutputs")]
        public void DsfReplaceActivity_GetState_Returns_Inputs_And_Outputs()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            //------------Assert Results-------------------------
            Assert.AreEqual(5, stateItems.Count());
            var expectedResults = new[]
            {
                 new StateVariable
                {
                    Name ="FieldsToSearch",
                    Type = StateVariable.StateType.Input,
                    Value = act.FieldsToSearch
                },
                new StateVariable
                {
                    Name ="Find",
                    Type = StateVariable.StateType.Input,
                    Value = act.Find
                },
                new StateVariable
                {
                    Name ="ReplaceWith",
                    Type = StateVariable.StateType.Input,
                    Value = act.ReplaceWith
                },
                new StateVariable
                {
                    Name ="CaseMatch",
                    Type = StateVariable.StateType.Input,
                    Value = act.CaseMatch.ToString()
                },
                new StateVariable
                {
                    Name ="Result",
                    Type = StateVariable.StateType.Output,
                    Value = act.Result
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

        void SetupArguments(string currentDL, string testData, string fieldsToSearch, string find, string replaceWith, string result, bool caseMatch)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfReplaceActivity { FieldsToSearch = fieldsToSearch, Find = find, ReplaceWith = replaceWith, Result = result, CaseMatch = caseMatch }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
