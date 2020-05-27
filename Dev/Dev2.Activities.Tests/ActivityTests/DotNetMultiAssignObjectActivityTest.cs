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
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Collections.Generic;
using System;
using System.Globalization;
using Warewolf.Storage;
using Moq;
using WarewolfParserInterop;
using System.Linq;
using Dev2.Common.State;
using Dev2.Utilities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]

    public class DotNetMultiAssignObjectActivityTest : BaseActivityUnitTest
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
            var act = new DsfDotNetMultiAssignObjectActivity { OutputMapping = null, FieldsCollection = fieldCollection };
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
        public void MultiAssignObjectDateFormat_Expected_MultiAssignObjectCorrectlySetsCorrectDateValues()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[@testName1.date]]", "!~calculation~!now()!~~calculation~!", fieldCollection.Count));
            SetupArguments(
                            ActivityStrings.NewScalarShape
                          , ActivityStrings.NewScalarShape
                          , fieldCollection);
            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "@testName1.date", out string actual, out string error);

            // remove test datalist
            var a = DateTime.ParseExact(actual, GlobalConstants.Dev2DotNetDefaultDateTimeFormat, CultureInfo.InvariantCulture);
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
                    Action = new DsfDotNetMultiAssignObjectActivity { OutputMapping = null, FieldsCollection = fieldCollection }
                };
            }
            else
            {
                TestStartNode = new FlowStep
                {
                    Action = new DsfDotNetMultiAssignObjectActivity { OutputMapping = outputMapping, FieldsCollection = fieldCollection }
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

            var act = new DsfDotNetMultiAssignObjectActivity { FieldsCollection = fieldsCollection };

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

            var act = new DsfDotNetMultiAssignObjectActivity { FieldsCollection = fieldsCollection };

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

            var act = new DsfDotNetMultiAssignObjectActivity { FieldsCollection = fieldsCollection };

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

            var act = new DsfDotNetMultiAssignObjectActivity { FieldsCollection = fieldsCollection };

            //------------Execute Test---------------------------

            var inputs = act.GetForEachOutputs();

            //------------Assert Results-------------------------

            Assert.AreEqual("[[@Pet.Name]]", inputs[0].Value);
            Assert.AreEqual("[[result]]", inputs[0].Name);
        }


        [TestMethod]
        [Timeout(60000)]
        [TestCategory("DsfMultiAssignActivity_Equality")]
        public void DsfDotNetMultiAssignObjectActivity_WhenDifferentFieldCollectionData_SHouldBeNotEqual()
        {
            var fieldCollection = new List<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[a]]", "12", fieldCollection.Count));
            var activity1 = new DsfDotNetMultiAssignObjectActivity { OutputMapping = null, FieldsCollection = fieldCollection };

            var fieldCollection2 = new List<AssignObjectDTO>();
            fieldCollection2.Add(new AssignObjectDTO("[[a]]", "111", fieldCollection2.Count));
            var activity2 = new DsfDotNetMultiAssignObjectActivity
            {
                UniqueID = activity1.UniqueID, // simulate this assign being from a copied/cloned workflow
                OutputMapping = null,
                FieldsCollection = fieldCollection2
            };

            Assert.IsFalse(activity1.Equals(activity2));
        }
        [TestMethod]
        [Timeout(60000)]
        [TestCategory("DsfMultiAssignActivity_Equality")]
        public void DsfDotNetMultiAssignObjectActivity_WhenSameFieldCollectionData_ShouldBeEqual()
        {
            var fieldCollection = new List<AssignObjectDTO>();
            fieldCollection.Add(new AssignObjectDTO("[[a]]", "12", fieldCollection.Count));
            var activity1 = new DsfDotNetMultiAssignObjectActivity { OutputMapping = null, FieldsCollection = fieldCollection };

            var fieldCollection2 = new List<AssignObjectDTO>();
            fieldCollection2.Add(new AssignObjectDTO("[[a]]", "12", fieldCollection2.Count));
            var activity2 = new DsfDotNetMultiAssignObjectActivity
            {
                UniqueID = activity1.UniqueID, // simulate this assign being from a copied/cloned workflow
                OutputMapping = null,
                FieldsCollection = fieldCollection2
            };

            Assert.IsTrue(activity1.Equals(activity2));
            activity2.CreateBookmark = true;
            Assert.IsFalse(activity1.Equals(activity2));
            activity2.CreateBookmark = false;
            activity2.UpdateAllOccurrences = true;
            Assert.IsFalse(activity1.Equals(activity2));
            activity2.UpdateAllOccurrences = false;
            Assert.IsTrue(activity1.Equals(activity2));
        }


        [TestMethod]
        [Timeout(60000)]
        [TestCategory("DsfMultiAssignActivity")]
        public void DsfDotNetMultiAssignObjectActivity_DebugModeAddsDebugItems()
        {
            var env = new ExecutionEnvironment();
            env.AssignJson(new AssignValue("[[@names(1).Name]]", "Mary"), 0);
            env.AssignJson(new AssignValue("[[@names(2).Name]]", "Jane"), 0);


            var data = new Mock<IDSFDataObject>();
            env.Assign("[[val]]", "asdf", 0);
            data.Setup(o => o.Environment).Returns(() => env);
            data.Setup(o => o.IsDebugMode()).Returns(() => true);

            var ob = new DsfDotNetMultiAssignObjectActivity
            {
                FieldsCollection = new List<AssignObjectDTO>
                {
                    new AssignObjectDTO("[[@names().Name]]", "[[val]]", 0)
                }
            };
            ob.Execute(data.Object, 0);

            var a = ob.GetDebugOutputs(env, 0);
            Assert.AreEqual("[[@names(3).Name]]", a[0].ResultsList[1].Variable);

            ob = new DsfDotNetMultiAssignObjectActivity
            {
                FieldsCollection = new List<AssignObjectDTO>
                {
                    new AssignObjectDTO("[[@obj.names().Name]]", "[[val]]", 0),
                    new AssignObjectDTO("[[@obj.names().Name]]", "[[val]]", 0)
                }
            };
            ob.Execute(data.Object, 0);

            var b = ob.GetDebugOutputs(env, 0);
            Assert.AreEqual("[[@obj.names(1).Name]]", b[0].ResultsList[1].Variable);
            Assert.AreEqual("[[@obj.names(2).Name]]", b[1].ResultsList[1].Variable);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory("DsfMultiAssignActivity")]
        public void DsfDotNetMultiAssignObjectActivity_DebugModeAddsDebugItems2()
        {
            var env = new ExecutionEnvironment();
            env.AssignJson(new AssignValue("[[@names(1).Name]]", "Mary"), 0);
            env.AssignJson(new AssignValue("[[@names(2).Name]]", "Jane"), 0);


            var data = new Mock<IDSFDataObject>();
            env.Assign("[[vals().newNames]]", "asdf", 0);
            data.Setup(o => o.Environment).Returns(() => env);
            data.Setup(o => o.IsDebugMode()).Returns(() => true);

            var ob = new DsfDotNetMultiAssignObjectActivity
            {
                FieldsCollection = new List<AssignObjectDTO>
                {
                    new AssignObjectDTO("[[vals().newNames]]", "[[@names().Name]]", 0)
                }
            };
            ob.Execute(data.Object, 0);

            var a = ob.GetDebugOutputs(env, 0);
            Assert.AreEqual("[[vals(1).newNames]]", a[0].ResultsList[1].Variable);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory("DsfMultiAssignActivity")]
        public void DsfDotNetMultiAssignObjectActivity_ToNewRecordset_DebugModeAddsDebugItems()
        {
            var env = new ExecutionEnvironment();
            env.AssignJson(new AssignValue("[[@names(1).Name]]", "Mary"), 0);
            env.AssignJson(new AssignValue("[[@names(2).Name]]", "Jane"), 0);
            env.Assign("[[list().name]]", "asdf", 0);

            var data = new Mock<IDSFDataObject>();
            data.Setup(o => o.Environment).Returns(() => env);
            data.Setup(o => o.IsDebugMode()).Returns(() => true);

            var ob = new DsfDotNetMultiAssignObjectActivity
            {
                FieldsCollection = new List<AssignObjectDTO>
                {
                    new AssignObjectDTO("[[list().name]]", "[[list().name]]", 0)
                }
            };
            ob.Execute(data.Object, 0);

            var a = ob.GetDebugOutputs(env, 0);
            Assert.AreEqual("asdf", a[0].ResultsList[1].Value);
            Assert.AreEqual("[[list(1).name]]", a[0].ResultsList[1].Variable);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void DsfDotnetMultiAssignObject_GetState_Returns_Input_And_Outputs()
        {
            var fieldCollection = new ObservableCollection<AssignObjectDTO>
            {
                new AssignObjectDTO("[[test.value1]]", "[[test..value2]]", 1)
            };
            var act = new DsfDotNetMultiAssignObjectActivity
            {
                FieldsCollection = fieldCollection
            };
            var stateItems = act.GetState();
            Assert.AreEqual(1, stateItems.Count());
            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name="Fields Collection",
                    Type = StateVariable.StateType.InputOutput,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(fieldCollection)
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
    }        
}
