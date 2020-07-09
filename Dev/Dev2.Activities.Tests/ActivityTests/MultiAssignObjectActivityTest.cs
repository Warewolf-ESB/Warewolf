/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using ActivityUnitTests;
using Dev2.Common;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using System.Collections.ObjectModel;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Collections.Generic;
using System;
using System.Linq;
using Dev2.Common.State;
using Dev2.Utilities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    
    public class MultiAssignObjectActivityTest : BaseActivityUnitTest
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithValue()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@test.value1]]", "somevalue", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            var result = ExecuteProcess();
            const string expected = "somevalue";
            GetScalarValueFromEnvironment(result.Environment, "@test.value1", out string actual, out string error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfMultiAssignObjectActivity_GetOutputs")]
        public void DsfMultiAssignObjectActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@test.value1]]", "somevalue", fieldCollection.Count));
            var act = new DsfMultiAssignObjectActivity { OutputMapping = null, FieldsCollection = fieldCollection };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[@test.value1]]", outputs[0]);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectTopLevelJSONAssign()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@test]]", "{\"Name\":\"Iris\",\"Age\":30}", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            var result = ExecuteProcess();
            var expected1 = "Iris";
            var expected2 = "30";
            GetScalarValueFromEnvironment(result.Environment, "@test.Name", out string actual1, out string error);
            GetScalarValueFromEnvironment(result.Environment, "@test.Age", out string actual2, out error);

            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual(expected2, actual2);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithSpecialCharsInValue()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@test.value1]]", "somevalue@#", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            var result = ExecuteProcess();
            const string expected = "somevalue@#";
            GetScalarValueFromEnvironment(result.Environment, "@test.value1", out string actual, out string error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithNewValue()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@test.value1]]", "somevalue1", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@test.value1]]", "somevalue2", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            var result = ExecuteProcess();
            const string expected = "somevalue2";
            GetScalarValueFromEnvironment(result.Environment, "@test.value1", out string actual, out string error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithMultipleValues()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@test.value1]]", "somevalue1", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@test.value2]]", "somevalue2", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@test.value3]]", "somevalue3", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@test.value4]]", "somevalue4", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            var result = ExecuteProcess();
            const string expected1 = "somevalue1", expected2 = "somevalue2", expected3 = "somevalue3", expected4 = "somevalue4";
            GetScalarValueFromEnvironment(result.Environment, "@test.value1", out string actual1, out string error);
            GetScalarValueFromEnvironment(result.Environment, "@test.value2", out string actual2, out error);
            GetScalarValueFromEnvironment(result.Environment, "@test.value3", out string actual3, out error);
            GetScalarValueFromEnvironment(result.Environment, "@test.value4", out string actual4, out error);

            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual(expected2, actual2);
            Assert.AreEqual(expected3, actual3);
            Assert.AreEqual(expected4, actual4);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithAnEmptyField()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@test.value]]", "", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "@test.value", out string actual, out string error);

            Assert.IsTrue(actual == string.Empty);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MutiAssignObjectInvalidJsonObjectLeft()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[test..value]]", "testData", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            var result = ExecuteProcess();

            Assert.IsTrue(result.Environment.HasErrors());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MutiAssignObjectInvalidJsonObjectRight()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[test.value1]]", "[[test..value2]]", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            var result = ExecuteProcess();

            Assert.IsTrue(result.Environment.HasErrors());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithValue4LayersDeap()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@test.value1.value2.value3.value4]]", "somevalue", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            var result = ExecuteProcess();
            const string expected = "somevalue";
            GetScalarValueFromEnvironment(result.Environment, "@test.value1.value2.value3.value4", out string actual, out string error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithCalculatedValue()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@test.value1]]", GlobalConstants.CalculateTextConvertPrefix + "SUM(1,2,3) + 1" + GlobalConstants.CalculateTextConvertSuffix, fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            var result = ExecuteProcess();
            const string expected = "7";
            GetScalarValueFromEnvironment(result.Environment, "@test.value1", out string actual, out string error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithCalculatedValueFromJson()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@test.value1]]", "1", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@test.value2]]", "2", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@test.value3]]", "3", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@test.total1]]", GlobalConstants.CalculateTextConvertPrefix +
                "SUM([[@test.value1]], [[@test.value2]], [[@test.value3]]) + 1" +
                GlobalConstants.CalculateTextConvertSuffix, fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            var result = ExecuteProcess();
            const string expected = "7";
            GetScalarValueFromEnvironment(result.Environment, "@test.total1", out string actual, out string error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectArrayWithMultipleValues()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@test.value(1)]]", "somevalue1", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@test.value(2)]]", "somevalue2", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@test.value(3)]]", "somevalue3", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@test.value(4)]]", "somevalue4", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            var result = ExecuteProcess();
            const string expected1 = "somevalue1", expected2 = "somevalue2", expected3 = "somevalue3", expected4 = "somevalue4";
            GetScalarValueFromEnvironment(result.Environment, "@test.value(1)", out string actual1, out string error);
            GetScalarValueFromEnvironment(result.Environment, "@test.value(2)", out string actual2, out error);
            GetScalarValueFromEnvironment(result.Environment, "@test.value(3)", out string actual3, out error);
            GetScalarValueFromEnvironment(result.Environment, "@test.value(4)", out string actual4, out error);

            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual(expected2, actual2);
            Assert.AreEqual(expected3, actual3);
            Assert.AreEqual(expected4, actual4);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithCalculatedValueFromJsonArray()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@test.value(1)]]", "1", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@test.value(2)]]", "2", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@test.value(3)]]", "3", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@test.total1]]", GlobalConstants.CalculateTextConvertPrefix +
                "SUM([[@test.value(*)]]) + 1" + GlobalConstants.CalculateTextConvertSuffix, fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            var result = ExecuteProcess();
            const string expected = "4";
            GetScalarValueFromEnvironment(result.Environment, "@test.total1", out string actual, out string error);

            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithJsonArrayMultiplePropertiesAtSameLevel()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@Pet.Owner(1).Name]]", "Bob", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Pet.Owner(1).Tel(1).Name]]", "Home", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Pet.Owner(1).Tel(2).Name]]", "Work", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Pet.Owner(2).Name]]", "Dora", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Pet.Owner(2).Tel(1).Name]]", "Mobile", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Pet.Owner(2).Tel(2).Name]]", "Skype", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Pet.Owner(2).Tel(3).Name]]", "Personal", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Pet.Owner(2).Name]]", "Explorer", fieldCollection.Count));


            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            var result = ExecuteProcess();
            const string expected = "Bob";
            GetScalarValueFromEnvironment(result.Environment, "@Pet.Owner(1).Name", out string actual, out string error);
            Assert.AreEqual(expected, actual);
            GetScalarValueFromEnvironment(result.Environment, "@Pet.Owner(1).Tel(1).Name", out actual, out error);
            Assert.AreEqual("Home", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Pet.Owner(1).Tel(2).Name", out actual, out error);
            Assert.AreEqual("Work", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Pet.Owner(2).Name", out actual, out error);
            Assert.AreEqual("Explorer", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Pet.Owner(2).Tel(1).Name", out actual, out error);
            Assert.AreEqual("Mobile", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Pet.Owner(2).Tel(2).Name", out actual, out error);
            Assert.AreEqual("Skype", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Pet.Owner(2).Tel(3).Name", out actual, out error);
            Assert.AreEqual("Personal", actual);

        }

        void SetupArguments(string currentDL, string testData, ObservableCollection<AssignObjectDTO> fieldCollection, string outputMapping = null)
        {
            if (outputMapping == null)
            {
                TestStartNode = new FlowStep
                {
                    Action = new DsfMultiAssignObjectActivity { OutputMapping = null, FieldsCollection = fieldCollection }
                };
            }
            else
            {
                TestStartNode = new FlowStep
                {
                    Action = new DsfMultiAssignObjectActivity { OutputMapping = outputMapping, FieldsCollection = fieldCollection }
                };
            }
            TestData = testData;
            CurrentDl = currentDL;
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory("DsfMultiAssignObjectActivity_UpdateForEachInputs")]
        public void DsfMultiAssignObjectActivity_UpdateForEachInputs_WhenContainsMatchingStarAndOtherData_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<AssignObjectDTO>
            {
                new AssignObjectDTO("[[@Pet.Owner(1).Name]]", "Bob", 1),
            };

            var act = new DsfMultiAssignObjectActivity { FieldsCollection = fieldsCollection };

            //------------Execute Test---------------------------

            act.UpdateForEachInputs(new List<Tuple<string, string>>
            {
                new Tuple<string, string>("[[@Pet.Owner(*).Name]]", "[[@Pet.Owner(1).Name]]"),
            });

            //------------Assert Results-------------------------

            var collection = act.FieldsCollection;

            Assert.AreEqual("Bob", collection[0].FieldValue);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory("DsfMultiAssignObjectActivity_UpdateForEachOutputs")]
        public void DsfMultiAssignObjectActivity_UpdateForEachOutputs_WhenContainsMatchingStar_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<AssignObjectDTO>
            {
                new AssignObjectDTO("[[@Pet.Owner(1).Name]]", "Bob", 1),
            };

            var act = new DsfMultiAssignObjectActivity { FieldsCollection = fieldsCollection };

            //------------Execute Test---------------------------

            act.UpdateForEachOutputs(new List<Tuple<string, string>>
            {
                new Tuple<string, string>("[[@Pet.Owner(*).Name]]", "[[@Pet.Owner(1).Name]]"),
            });

            //------------Assert Results-------------------------

            var collection = act.FieldsCollection;

            Assert.AreEqual("Bob", collection[0].FieldValue);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory("DsfMultiAssignActivity_GetForEachInputs")]
        public void DsfMultiAssignActivity_GetForEachInputs_Normal_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<AssignObjectDTO>
            {
                new AssignObjectDTO("[[@Pet.Name]]", "[[result]]", 1),
            };

            var act = new DsfMultiAssignObjectActivity { FieldsCollection = fieldsCollection };

            //------------Execute Test---------------------------

            var inputs = act.GetForEachInputs();

            //------------Assert Results-------------------------

            Assert.AreEqual("[[@Pet.Name]]", inputs[0].Name);
            Assert.AreEqual("[[result]]", inputs[0].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory("DsfMultiAssignActivity_GetForEachOutputs")]
        public void DsfMultiAssignActivity_GetForEachOutputs_Normal_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<AssignObjectDTO>
            {
                new AssignObjectDTO("[[@Pet.Name]]", "[[result]]", 1),
            };

            var act = new DsfMultiAssignObjectActivity { FieldsCollection = fieldsCollection };

            //------------Execute Test---------------------------

            var inputs = act.GetForEachOutputs();

            //------------Assert Results-------------------------

            Assert.AreEqual("[[@Pet.Name]]", inputs[0].Value);
            Assert.AreEqual("[[result]]", inputs[0].Name);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory("DsfMultiAssignActivity_GetForEachOutputs")]
        public void DsfMultiAssignObjectActivity_GetState_Returns_Inputs_And_Outputs()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<AssignObjectDTO>
            {
                new AssignObjectDTO("[[@Pet.Name]]", "[[result]]", 1),
            };
            var act = new DsfMultiAssignObjectActivity { FieldsCollection = fieldsCollection };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(1, stateItems.Count());
            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name="Fields Collection",
                    Type = StateVariable.StateType.InputOutput,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(fieldsCollection)
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
        [Owner("Candice Daniel")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithJsonArrayMultiplePropertiesWithoutIndex()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers().Name]]", "Bob", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers().Surname]]", "Dill", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers().Age]]", "20", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers().Gender]]", "Male", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers().Name]]", "Jane", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers().Surname]]", "Doe", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers().Age]]", "30", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers().Gender]]", "Female", fieldCollection.Count));

            SetupArguments(ActivityStrings.scalarShape, ActivityStrings.scalarShape, fieldCollection);

            var result = ExecuteProcess();

            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(1).Name", out string actual, out string error);
            Assert.AreEqual("Bob", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(1).Surname", out actual, out error);
            Assert.AreEqual("Dill", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(1).Age", out actual, out error);
            Assert.AreEqual("20", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(1).Gender", out actual, out error);
            Assert.AreEqual("Male", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(2).Name", out actual, out error);
            Assert.AreEqual("Jane", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(2).Surname", out actual, out error);
            Assert.AreEqual("Doe", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(2).Age", out actual, out error);
            Assert.AreEqual("30", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(2).Gender", out actual, out error);
            Assert.AreEqual("Female", actual);

        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithJsonArrayMultiplePropertiesWithIndex()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Name]]", "Warewolf", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Location]]", "Ireland", fieldCollection.Count));

            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers(1).Name]]", "Bob", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers(1).Surname]]", "Dill", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers(1).Age]]", "20", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers(1).Gender]]", "Male", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers(2).Name]]", "Jane", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers(2).Surname]]", "Doe", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers(2).Age]]", "30", fieldCollection.Count));
            fieldCollection.Add(new AssignObjectDTO("[[@Org.Workers(2).Gender]]", "Female", fieldCollection.Count));

            SetupArguments(ActivityStrings.scalarShape, ActivityStrings.scalarShape, fieldCollection);

            var result = ExecuteProcess();

            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(1).Name", out string actual, out string error);
            Assert.AreEqual("Bob", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(1).Surname", out actual, out error);
            Assert.AreEqual("Dill", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(1).Age", out actual, out error);
            Assert.AreEqual("20", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(1).Gender", out actual, out error);
            Assert.AreEqual("Male", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(2).Name", out actual, out error);
            Assert.AreEqual("Jane", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(2).Surname", out actual, out error);
            Assert.AreEqual("Doe", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(2).Age", out actual, out error);
            Assert.AreEqual("30", actual);
            GetScalarValueFromEnvironment(result.Environment, "@Org.Workers(2).Gender", out actual, out error);
            Assert.AreEqual("Female", actual);

        }


    }
}