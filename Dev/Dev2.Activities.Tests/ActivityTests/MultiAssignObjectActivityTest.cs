/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using ActivityUnitTests;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class MultiAssignObjectActivityTest : BaseActivityUnitTest
    {
        #region MultiAssignObject Functionality Tests

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithValue()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[test.value1]]", "somevalue", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected = "somevalue";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "test.value1", out actual, out error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithSpecialCharsInValue()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[test.value1]]", "somevalue@#", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected = "somevalue@#";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "test.value1", out actual, out error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithNewValue()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[test.value1]]", "somevalue1", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[test.value1]]", "somevalue2", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected = "somevalue2";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "test.value1", out actual, out error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithMultipleValues()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[test.value1]]", "somevalue1", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[test.value2]]", "somevalue2", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[test.value3]]", "somevalue3", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[test.value4]]", "somevalue4", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected1 = "somevalue1", expected2 = "somevalue2", expected3 = "somevalue3", expected4 = "somevalue4";
            string actual1, actual2, actual3, actual4;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "test.value1", out actual1, out error);
            GetScalarValueFromEnvironment(result.Environment, "test.value2", out actual2, out error);
            GetScalarValueFromEnvironment(result.Environment, "test.value3", out actual3, out error);
            GetScalarValueFromEnvironment(result.Environment, "test.value4", out actual4, out error);

            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual(expected2, actual2);
            Assert.AreEqual(expected3, actual3);
            Assert.AreEqual(expected4, actual4);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithAnEmptyField()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[test.value]]", "", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "test.value", out actual, out error);

            Assert.IsTrue(actual == string.Empty);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MutiAssignObjectInvalidJsonObjectLeft()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[test..value]]", "testData", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            Assert.IsTrue(result.Environment.HasErrors());
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MutiAssignObjectInvalidJsonObjectRight()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[test.value1]]", "[[test..value2]]", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            IDSFDataObject result = ExecuteProcess();

            Assert.IsTrue(result.Environment.HasErrors());
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithValue4LayersDeap()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[test.value1.value2.value3.value4]]", "somevalue", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected = "somevalue";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "test.value1.value2.value3.value4", out actual, out error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithCalculatedValue()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[test.value1]]", GlobalConstants.CalculateTextConvertPrefix + "SUM(1,2,3) + 1" + GlobalConstants.CalculateTextConvertSuffix, fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected = "7";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "test.value1", out actual, out error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithCalculatedValueFromJson()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[test.value1]]", "1", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[test.value2]]", "2", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[test.value3]]", "3", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[test.total1]]", GlobalConstants.CalculateTextConvertPrefix +
                "SUM([[test.value1]], [[test.value2]], [[test.value3]]) + 1" + 
                GlobalConstants.CalculateTextConvertSuffix, fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected = "7";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "test.total1", out actual, out error);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectArrayWithMultipleValues()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[test.value(1)]]", "somevalue1", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[test.value(2)]]", "somevalue2", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[test.value(3)]]", "somevalue3", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[test.value(4)]]", "somevalue4", fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected1 = "somevalue1", expected2 = "somevalue2", expected3 = "somevalue3", expected4 = "somevalue4";
            string actual1, actual2, actual3, actual4;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "test.value(1)", out actual1, out error);
            GetScalarValueFromEnvironment(result.Environment, "test.value(2)", out actual2, out error);
            GetScalarValueFromEnvironment(result.Environment, "test.value(3)", out actual3, out error);
            GetScalarValueFromEnvironment(result.Environment, "test.value(4)", out actual4, out error);

            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual(expected2, actual2);
            Assert.AreEqual(expected3, actual3);
            Assert.AreEqual(expected4, actual4);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivity_FunctionalityTests")]
        public void MultiAssignObjectWithCalculatedValueFromJsonArray()
        {
            var fieldCollection = new ObservableCollection<ActivityDTO>();
            fieldCollection.Add(new ActivityDTO("[[test.value(1)]]", "1", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[test.value(2)]]", "2", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[test.value(3)]]", "3", fieldCollection.Count));
            fieldCollection.Add(new ActivityDTO("[[test.total1]]", GlobalConstants.CalculateTextConvertPrefix +
                "SUM([[test.value(*)]]) + 1" + GlobalConstants.CalculateTextConvertSuffix, fieldCollection.Count));

            SetupArguments(
                           ActivityStrings.scalarShape
                         , ActivityStrings.scalarShape
                         , fieldCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected = "7";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "test.total1", out actual, out error);

            Assert.AreEqual(expected, actual);
        }

        #endregion MultiAssignObject Functionality Tests

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, ObservableCollection<ActivityDTO> fieldCollection, string outputMapping = null)
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

        #endregion Private Test Methods
    }
}