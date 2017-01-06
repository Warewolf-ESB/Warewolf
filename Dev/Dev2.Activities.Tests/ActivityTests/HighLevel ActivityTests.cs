/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using ActivityUnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable ReturnTypeCanBeEnumerable.Local

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Tests that the Properties have not changed on the activities
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class HighLevel_ActivityTests : BaseActivityUnitTest
    {
        #region Fields

        private static List<Type> _activityList;

        #endregion Fields

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion Test Context

        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            _activityList = GetAllActivities();
        }

        #endregion Additional test attributes

        #region Check Property Count Tests

        [TestMethod]
        public void DsfZip_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfZip");
            if (type == null)
            {
                Assert.Fail("Could not find DsfZip.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            var count = properties.Length;
            Assert.IsTrue(count == 3);
        }

        [TestMethod]
        public void DsfUnZip_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfUnZip");
            if (type == null)
            {
                Assert.Fail("Could not find DsfUnZip.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            var count = properties.Length;
            Assert.IsTrue(count == 1);
        }

        [TestMethod]
        public void DsfPathDelete_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfPathDelete");
            if (type == null)
            {
                Assert.Fail("Could not find DsfPathDelete.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);

            Assert.IsTrue(properties.Length == 1);
        }

        [TestMethod]
        public void DsfPathCreate_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfPathCreate");
            if (type == null)
            {
                Assert.Fail("Could not find DsfPathCreate.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Length == 2);
        }

        [TestMethod]
        public void DsfFolderRead_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfFolderRead");
            if (type == null)
            {
                Assert.Fail("Could not find DsfFolderRead.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Length == 4);
        }

        [TestMethod]
        public void DsfFileRead_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfFileRead");
            if (type == null)
            {
                Assert.Fail("Could not find DsfFileRead.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Length == 1);
        }

        [TestMethod]
        public void DsfSortRecordsActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfSortRecordsActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfSortRecordsActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Length == 2);
        }

        [TestMethod]
        public void DsfMultiAssignActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfMultiAssignActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfMultiAssignActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Length == 4);
        }

        [TestMethod]
        public void DsfMultiAssignObjectActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfMultiAssignObjectActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfMultiAssignObjectActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Length == 4);
        }

        [TestMethod]
        public void DsfForEachActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfForEachActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfForEachActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.AreEqual(14, properties.Length);
        }

        [TestMethod]
        public void DsfDateTimeActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfDateTimeActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfDateTimeActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Length == 7);
        }

        [TestMethod]
        public void DsfDataSplitActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfDataSplitActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfDataSplitActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.AreEqual(4, properties.Length);
        }

        [TestMethod]
        public void DsfCountRecordsetActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfCountRecordsetNullHandlerActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfCountRecordsetNullHandlerActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Length == 3);
        }

        [TestMethod]
        public void DsfDeleteRecordsetActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfDeleteRecordNullHandlerActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfDeleteRecordNullHandlerActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Length == 3);
        }

        [TestMethod]
        public void DsfRecordsetLengthActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfRecordsetNullhandlerLengthActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfRecordsetNullhandlerLengthActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Length == 3);
        }

        [TestMethod]
        public void DsfCalculateActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfCalculateActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfCalculateActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Length == 2);
        }

        [TestMethod]
        public void DsfCaseConvertActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfCaseConvertActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfCaseConvertActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Length == 1);
        }

        [TestMethod]
        public void DsfBaseConvertActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfBaseConvertActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfBaseConvertActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Length == 1);
        }

        #endregion Check Property Count Tests

        #region Check Property Names

        [TestMethod]
        public void DsfMultiAssignActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfMultiAssignActivity");

            if (type == null)
            {
                Assert.Fail("Could not find DsfMultiAssignActivity.");
            }

            MemberInfo[] allMembers = GetMembers(type, "FieldsCollection");

            Assert.AreEqual(true, allMembers != null && allMembers.Length == 1);
        }

        [TestMethod]
        public void DsfMultiAssignObjectActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfMultiAssignObjectActivity");

            if (type == null)
            {
                Assert.Fail("Could not find DsfMultiAssignObjectActivity.");
            }

            MemberInfo[] allMembers = GetMembers(type, "FieldsCollection");

            Assert.AreEqual(true, allMembers != null && allMembers.Length == 1);
        }

        [TestMethod]
        public void DsfCaseConvertActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfCaseConvertActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfCaseConvertActivity.");
            }

            MemberInfo[] allMembers = GetMembers(type, "ConvertCollection");

            Assert.AreEqual(true, allMembers != null && allMembers.Length == 1);
        }

        [TestMethod]
        public void DsfBaseConvertActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfBaseConvertActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfBaseConvertActivity.");
            }

            MemberInfo[] allMembers = GetMembers(type, "ConvertCollection");

            Assert.AreEqual(true, allMembers != null && allMembers.Length == 1);
        }

        [TestMethod]
        public void DsfCalculateActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfCalculateActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfCalculateActivity.");
            }

            MemberInfo[] Expression = GetMembers(type, "Expression");
            MemberInfo[] Result = GetMembers(type, "Result");
            Result = Result.Where(c => c.GetType().UnderlyingSystemType == typeof(string)).ToArray();
            Assert.IsTrue(Expression != null && Result != null && Expression.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }

        [TestMethod]
        public void DsfCountRecordSetActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfCountRecordsetNullHandlerActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfCountRecordsetNullHandlerActivity.");
            }

            MemberInfo[] RecordsetName = GetMembers(type, "RecordsetName");
            MemberInfo[] CountNumber = GetMembers(type, "CountNumber");
            Assert.IsTrue(RecordsetName != null && CountNumber != null && RecordsetName.Length == 1 && CountNumber.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }

        [TestMethod]
        public void DsfDataSplitActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfDataSplitActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfDataSplitActivity.");
            }

            MemberInfo[] ResultsCollection = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "ResultsCollection");
            MemberInfo[] ReverseOrder = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "ReverseOrder");
            MemberInfo[] SourceString = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "SourceString");
            Assert.IsTrue(ResultsCollection != null && ReverseOrder != null && SourceString != null && ResultsCollection.Length == 1 && ResultsCollection.Length == 1 && SourceString.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }

        [TestMethod]
        public void DsfDateTimeActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfDateTimeActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfDateTimeActivity.");
            }

            MemberInfo[] DateTime = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "DateTime");
            MemberInfo[] InputFormat = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "InputFormat");
            MemberInfo[] OutputFormat = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "OutputFormat");
            MemberInfo[] TimeModifierType = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "TimeModifierType");
            MemberInfo[] TimeModifierAmountDisplay = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "TimeModifierAmountDisplay");
            type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "TimeModifierAmount");
            MemberInfo[] Result = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "Result");

            Assert.IsTrue(DateTime != null && DateTime.Length == 1
                          && InputFormat != null && InputFormat.Length == 1
                          && OutputFormat != null && OutputFormat.Length == 1
                          && TimeModifierType != null && TimeModifierType.Length == 1
                          && TimeModifierAmountDisplay != null && TimeModifierAmountDisplay.Length == 1
                          && Result != null, "This will fail on designer binding, please update this before proceeding");
        }

        [TestMethod]
        public void DsfSortRecordSetActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfSortRecordsActivity");
            if (type == null)
            {
                Assert.Fail("Could not find DsfSortRecordsActivity.");
            }

            MemberInfo[] SortField = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "SortField");
            MemberInfo[] SelectedSort = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "SelectedSort");

            Assert.IsTrue(SortField != null && SortField.Length == 1
                          && SelectedSort != null & SortField.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }

        #endregion Check Property Names

        #region Private Methods

        private static List<Type> GetAllActivities()
        {
            var type = typeof(DsfActivityAbstract<bool>);

            List<Type> types = type.Assembly.GetTypes()
                                .Where(t => type.IsAssignableFrom(t))
                                .ToList();

            type = typeof(DsfActivityAbstract<string>);

            types.AddRange(type.Assembly.GetTypes()
                                .Where(t => type.IsAssignableFrom(t))
                                .ToList());
            return types;
        }

        private PropertyInfo[] GetPropertyInfo(Type type)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public
                                                           | BindingFlags.DeclaredOnly
                                                           | BindingFlags.Instance
                                                           | BindingFlags.CreateInstance);

            return properties;
        }

        private MemberInfo[] GetMembers(Type type, string methodName)
        {
            MemberInfo[] memberInfo = type.FindMembers(MemberTypes.Property,
                                                       BindingFlags.Public | BindingFlags.Instance,
                                                       Type.FilterName, methodName);

            return memberInfo;
        }

        #endregion Private Methods
    }
}