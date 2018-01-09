/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests
{
    
    /// <summary>
    /// Summary description for FindRecordsActivityTest
    /// </summary>
    [TestClass]
    public class FindRecordsMultipleCriteriaActivityTest : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        #endregion

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFindRecordsMultipleCriteriaActivity_GetOutputs")]
        public void DsfFindRecordsMultipleCriteriaActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            var act = new DsfFindRecordsMultipleCriteriaActivity
            {
                FieldsToSearch = "[[Recset().Field1]],[[Recset().Field2]],[[Recset().Field3]]",
                ResultsCollection = new List<FindRecordsTO> { new FindRecordsTO("jimmy", ">", 1) },
                StartIndex = "",
                Result = "[[Result().res]]"
            };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[Result().res]]", outputs[0]);
        }


        [TestMethod]
        public void FindRecordsMulitpleCriteriaActivity_WithTextInMatchField_Expected_NoResults()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfFindRecordsMultipleCriteriaActivity
                {
                    FieldsToSearch = "[[Recset().Field1]],[[Recset().Field2]],[[Recset().Field3]]",
                    ResultsCollection = new List<FindRecordsTO> { new FindRecordsTO("jimmy", ">", 1) },
                    StartIndex = "",
                    Result = "[[Result().res]]"
                }
            };

            CurrentDl = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            const string data = @"<ADL>
  <Recset>
	<Field1>Mr A</Field1>
	<Field2>25</Field2>
	<Field3>a@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr B</Field1>
	<Field2>651</Field2>
	<Field3>b@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr C</Field1>
	<Field2>48</Field2>
	<Field3>c@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr D</Field1>
	<Field2>1</Field2>
	<Field3>d@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr E</Field1>
	<Field2>22</Field2>
	<Field3>e@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr F</Field1>
	<Field2>321</Field2>
	<Field3>f@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr G</Field1>
	<Field2>51</Field2>
	<Field3>g@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr H</Field1>
	<Field2>2120</Field2>
	<Field3>h@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr I</Field1>
	<Field2>46</Field2>
	<Field3>i@abc.co.za</Field3>
  </Recset>
</ADL>";
            TestData = "<root>" + data + "</root>";
            var result = ExecuteProcess();
            GetRecordSetFieldValueFromDataList(result.Environment, "Result", "res", out IList<string> actual, out string error);

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("-1", actual[0]);
        }

        [TestMethod]
        public void FindRecordsMulitpleCriteriaActivity_FindWithMultipleCriteriaExpectAllTrue_Expected_NoResults()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfFindRecordsMultipleCriteriaActivity
                {
                    FieldsToSearch = "[[Recset().Field1]],[[Recset().Field2]],[[Recset().Field3]]",
                    ResultsCollection = new List<FindRecordsTO> { new FindRecordsTO("32", ">", 1), new FindRecordsTO("Mr A", "Equal", 2) },
                    StartIndex = "",
                    RequireAllTrue = true,
                    Result = "[[Result().res]]"
                }
            };

            const string data = @"<ADL>
  <Recset>
	<Field1>Mr A</Field1>
	<Field2>25</Field2>
	<Field3>a@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr B</Field1>
	<Field2>651</Field2>
	<Field3>b@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr C</Field1>
	<Field2>48</Field2>
	<Field3>c@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr D</Field1>
	<Field2>1</Field2>
	<Field3>d@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr E</Field1>
	<Field2>22</Field2>
	<Field3>e@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr F</Field1>
	<Field2>321</Field2>
	<Field3>f@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr G</Field1>
	<Field2>51</Field2>
	<Field3>g@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr H</Field1>
	<Field2>2120</Field2>
	<Field3>h@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr I</Field1>
	<Field2>46</Field2>
	<Field3>i@abc.co.za</Field3>
  </Recset>
</ADL>";

            CurrentDl = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + data + "</root>";
            var result = ExecuteProcess();
            GetRecordSetFieldValueFromDataList(result.Environment, "Result", "res", out IList<string> actual, out string error);

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("-1", actual[0]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFindRecordsMultipleCriteriaActivity_UpdateForEachInputs")]
        public void DsfFindRecordsMultipleCriteriaActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var act = new DsfFindRecordsMultipleCriteriaActivity
            {
                FieldsToSearch = "[[Customers(*).DOB]]",
                ResultsCollection = new List<FindRecordsTO> { new FindRecordsTO("/", "Contains", 1) },
                Result = "[[res]]"
            };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("/", act.ResultsCollection[0].SearchCriteria);
            Assert.AreEqual("[[Customers(*).DOB]]", act.FieldsToSearch);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFindRecordsMultipleCriteriaActivity_UpdateForEachInputs")]
        public void DsfFindRecordsMultipleCriteriaActivity_UpdateForEachInputs_MoreThan1Updates_UpdatesMergeCollection()
        {
            //------------Setup for test--------------------------
            var act = new DsfFindRecordsMultipleCriteriaActivity
            {
                FieldsToSearch = "[[Customers(*).DOB]]",
                ResultsCollection = new List<FindRecordsTO> { new FindRecordsTO("/", "Contains", 1) },
                Result = "[[res]]"
            };

            var tuple1 = new Tuple<string, string>("/", "Test");
            var tuple2 = new Tuple<string, string>("[[Customers(*).DOB]]", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.ResultsCollection[0].SearchCriteria);
            Assert.AreEqual("Test2", act.FieldsToSearch);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFindRecordsMultipleCriteriaActivity_UpdateForEachOutputs")]
        public void DsfFindRecordsMultipleCriteriaActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var act = new DsfFindRecordsMultipleCriteriaActivity
            {
                FieldsToSearch = "[[Customers(*).DOB]]",
                ResultsCollection = new List<FindRecordsTO> { new FindRecordsTO("/", "Contains", 1) },
                Result = "[[res]]"
            };

            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("[[res]]", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFindRecordsMultipleCriteriaActivity_UpdateForEachOutputs")]
        public void DDsfFindRecordsMultipleCriteriaActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var act = new DsfFindRecordsMultipleCriteriaActivity
            {
                FieldsToSearch = "[[Customers(*).DOB]]",
                ResultsCollection = new List<FindRecordsTO> { new FindRecordsTO("/", "Contains", 1) },
                Result = "[[res]]"
            };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual("[[res]]", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFindRecordsMultipleCriteriaActivity_UpdateForEachOutputs")]
        public void DsfFindRecordsMultipleCriteriaActivity_UpdateForEachOutputs_1Updates_UpdateResult()
        {
            //------------Setup for test--------------------------
            var act = new DsfFindRecordsMultipleCriteriaActivity
            {
                FieldsToSearch = "[[Customers(*).DOB]]",
                ResultsCollection = new List<FindRecordsTO> { new FindRecordsTO("/", "Contains", 1) },
                Result = "[[res]]"
            };

            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFindRecordsMultipleCriteriaActivity_GetForEachInputs")]
        public void DsfFindRecordsMultipleCriteriaActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var act = new DsfFindRecordsMultipleCriteriaActivity
            {
                FieldsToSearch = "[[Customers(*).DOB]]",
                ResultsCollection = new List<FindRecordsTO> { new FindRecordsTO("/", "Contains", 1) },
                Result = "[[res]]"
            };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, dsfForEachItems.Count);
            Assert.AreEqual("[[Customers(*).DOB]]", dsfForEachItems[0].Name);
            Assert.AreEqual("[[Customers(*).DOB]]", dsfForEachItems[0].Value);
            Assert.AreEqual("/", dsfForEachItems[1].Name);
            Assert.AreEqual("/", dsfForEachItems[1].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFindRecordsMultipleCriteriaActivity_GetForEachOutputs")]
        public void DsfFindRecordsMultipleCriteriaActivity_GetForEachOutputs_WhenHasResult_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var act = new DsfFindRecordsMultipleCriteriaActivity
            {
                FieldsToSearch = "[[Customers(*).DOB]]",
                ResultsCollection = new List<FindRecordsTO> { new FindRecordsTO("/", "Contains", 1) },
                Result = "[[res]]"
            };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual("[[res]]", dsfForEachItems[0].Name);
            Assert.AreEqual("[[res]]", dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void AddResultDebugInputs_Sets_Operand_To_EmptyString()
        {
            //------------Setup for test-------------------------
            var activity = new DsfFindRecordsMultipleCriteriaActivity();
            var privateObject = new PrivateObject(activity);
            IEnumerable<FindRecordsTO> resultsCollection = new List<FindRecordsTO>
            {
                new FindRecordsTO {SearchCriteria = "1", IndexNumber = 1, SearchType = "="}
            };
            var atomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewInt(1));
            var environment = new Mock<IExecutionEnvironment>();
            environment.Setup(executionEnvironment => executionEnvironment.Eval("1", 0, false, false)).Returns(atomResult);
            var args = new object[] { resultsCollection, environment.Object, 0 };
            //------------Execute Test---------------------------
            privateObject.Invoke("AddResultDebugInputs", args);
            //------------Assert Results-------------------------
            var debugInputs = activity.GetDebugInputs(environment.Object, 0);
            Assert.IsTrue(debugInputs.Single().ResultsList.All(p=>p.Operator == ""));
        }
    }
}
