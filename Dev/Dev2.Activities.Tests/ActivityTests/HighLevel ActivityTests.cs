using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ActivityUnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Tests that the Properties have not changed on the activities 
    /// </summary>
    [TestClass]
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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            _activityList = GetAllActivities();
        }

        #endregion

        #region Check Property Count Tests

        [TestMethod]
        public void DsfZip_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfZip");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfZip.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 5);
        }

        [TestMethod]
        public void DsfUnZip_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfUnZip");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfUnZip.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 4);
        }

        [TestMethod]
        public void DsfPathRename_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfPathRename");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfPathRename.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 3);
        }

        [TestMethod]
        public void DsfPathMove_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfPathMove");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfPathMove.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 3);
        }

        [TestMethod]
        public void DsfPathDelete_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfPathDelete");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfPathDelete.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);

            Assert.IsTrue(properties.Count() == 1);
        }

        [TestMethod]
        public void DsfPathCreate_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfPathCreate");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfPathCreate.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 2);
        }

        [TestMethod]
        public void DsfPathCopy_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfPathCopy");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfPathCopy.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 3);
        }

        [TestMethod]
        public void DsfFolderRead_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfFolderRead");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfFolderRead.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 1);
        }

        [TestMethod]
        public void DsfFileWrite_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfFileWrite");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfFileWrite.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 4);
        }

        [TestMethod]
        public void DsfFileRead_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfFileRead");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfFileRead.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 1);
        }

        [TestMethod]
        public void TransformActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "TransformActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find TransformActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 5);
        }

        [TestMethod]
        public void DsfWebSiteActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfWebSiteActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfWebSiteActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 2);
        }

        [TestMethod]
        public void DsfWebPageActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfWebPageActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfWebPageActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 6);
        }

        [TestMethod]
        public void DsfSortRecordsActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfSortRecordsActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfSortRecordsActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 2);
        }

        [TestMethod]
        public void DsfReturnToCallerActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfReturnToCallerActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfReturnToCallerActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 0);
        }

        [TestMethod]
        public void DsfRemoveActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfRemoveActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfRemoveActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 1);
        }

        [TestMethod]
        public void DsfRemoteActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfRemoteActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfRemoteActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 1);
        }

        [TestMethod]
        public void DsfMultiAssignActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfMultiAssignActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfMultiAssignActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 4);
        }

        [TestMethod]
        public void DsfHttpActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfHttpActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfHttpActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 4);
        }

        [TestMethod]
        public void DsfForEachActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfForEachActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfForEachActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.AreEqual(14, properties.Count());
        }

        [TestMethod]
        public void DsfFileForEachActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfFileForEachActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfFileForEachActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 12);
        }

        [TestMethod]
        public void DsfDateTimeActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfDateTimeActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfDateTimeActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 7);
        }

        [TestMethod]
        public void DsfDataValidationActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfDataValidationActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfDataValidationActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 1);
        }

        [TestMethod]
        public void DsfDataSplitActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfDataSplitActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfDataSplitActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 3);
        }

        [TestMethod]
        public void DsfTagCountActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfTagCountActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfTagCountActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 1);
        }

        [TestMethod]
        public void DsfCountRecordsetActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfCountRecordsetActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfCountRecordsetActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 2);
        }

        [TestMethod]
        public void DsfCheckpointActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfCheckpointActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfCheckpointActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 1);
        }

        [TestMethod]
        public void DsfCalculateActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfCalculateActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfCalculateActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 2);
        }

        [TestMethod]
        public void DsfAssignActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfAssignActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfAssignActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 5);
        }

        [TestMethod]
        public void DsfAssertActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "AssertActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfAssertActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 2);
        }

        [TestMethod]
        public void DsfFindRecordsActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfFindRecordsActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfFindRecordsActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 6);
        }

        [TestMethod]
        public void DsfCaseConvertActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfCaseConvertActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfCaseConvertActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 1);
        }

        [TestMethod]
        public void DsfBaseConvertActivity_Property_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfBaseConvertActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfBaseConvertActivity.");
            }
            PropertyInfo[] properties = GetPropertyInfo(type);
            Assert.IsTrue(properties.Count() == 1);
        }

        #endregion Check Property Count Tests

        #region Check Property Names

        //[TestMethod]
        //public void DsfDateTimeDifference_Property_Name_Check_Expected_No_Change_To_Properties()
        //{
        //    Type type = _activityList.FirstOrDefault(c => c.Name == "DsfDateTimeDifferenceActivity");
        //    if (type == null)
        //    {
        //        Assert.Fail("Couldnt find DsfDateTimeDifferenceActivity.");
        //    }

        //    MemberInfo[] Input1 = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "Input1");
        //    MemberInfo[] Input2 = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "Input1");
        //    MemberInfo[] InputFormate = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "InputFormate");
        //    MemberInfo[] OutputType = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "OutputType");
        //    MemberInfo[] Result = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "Result");

        //    Assert.IsTrue(DateTime != null && DateTime.Length == 1
        //                  && Input1 != null && InputFormat.Length == 1
        //                  && Input2 != null && OutputFormat.Length == 1
        //                  && TimeModifierType != null && TimeModifierType.Length == 1
        //                  && TimeModifierAmountDisplay != null && TimeModifierAmountDisplay.Length == 1
        //                  && Result != null, "This will fail on designer binding, please update this before proceeding");

        //}

        [TestMethod]
        public void DsfMultiAssignActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfMultiAssignActivity");

            if(type == null)
            {
                Assert.Fail("Couldnt find DsfMultiAssignActivity.");
            }

            MemberInfo[] allMembers = GetMembers(type, "FieldsCollection");

            Assert.AreEqual(true, allMembers != null && allMembers.Length == 1);

        }

        [TestMethod]
        public void DsfCaseConvertActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfCaseConvertActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfCaseConvertActivity.");
            }

            MemberInfo[] allMembers = GetMembers(type, "ConvertCollection");

            Assert.AreEqual(true, allMembers != null && allMembers.Length == 1);

        }

        [TestMethod]
        public void DsfBaseConvertActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfBaseConvertActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfBaseConvertActivity.");
            }

            MemberInfo[] allMembers = GetMembers(type, "ConvertCollection");

            Assert.AreEqual(true, allMembers != null && allMembers.Length == 1);

        }


        [TestMethod]
        public void DsfCalculateActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfCalculateActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfCalculateActivity.");
            }

            MemberInfo[] Expression = GetMembers(type, "Expression");
            MemberInfo[] Result = GetMembers(type, "Result");
            Result = Result.Where(c => c.GetType().UnderlyingSystemType == typeof(string)).ToArray();
            Assert.IsTrue(Expression != null && Result != null && Expression.Length == 1, "This will fail on designer binding, please update this before proceeding");

        }


        [TestMethod]
        public void DsfCountRecordSetActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfCountRecordsetActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfCountRecordsetActivity.");
            }

            MemberInfo[] RecordsetName = GetMembers(type, "RecordsetName");
            MemberInfo[] CountNumber = GetMembers(type, "CountNumber");
            Assert.IsTrue(RecordsetName != null && CountNumber != null && RecordsetName.Length == 1 && CountNumber.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }


        [TestMethod]
        public void DsfAssertActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "AssertActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find AssertActivity.");
            }

            MemberInfo[] DataExpression = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "DataExpression");
            MemberInfo[] DataTags = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "DataTags");
            Assert.IsTrue(DataExpression != null && DataTags != null && DataExpression.Length == 1 && DataTags.Length == 1, "This will fail on designer binding, please update this before proceeding");

        }

        [TestMethod]
        public void DsfAssignActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfAssignActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfAssignActivity.");
            }

            MemberInfo[] FieldName = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "FieldName");
            MemberInfo[] FieldValue = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "FieldValue");
            Assert.IsTrue(FieldName != null && FieldValue != null && FieldName.Length == 1 && FieldValue.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }

        [TestMethod]
        public void DsfCountTagActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfTagCountActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfTagCountActivity.");
            }

            MemberInfo[] TagName = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "TagName");
            Assert.IsTrue(TagName != null && TagName.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }

        [TestMethod]
        public void DsfDataSplitActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfDataSplitActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfDataSplitActivity.");
            }

            MemberInfo[] ResultsCollection = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "ResultsCollection");
            MemberInfo[] ReverseOrder = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "ReverseOrder");
            MemberInfo[] SourceString = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "SourceString");
            Assert.IsTrue(ResultsCollection != null && ReverseOrder != null && SourceString != null && ResultsCollection.Length == 1 && ResultsCollection.Length == 1 && SourceString.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }

        [TestMethod]
        public void DsfDataValidationActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfDataValidationActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfDataValidationActivity.");
            }

            MemberInfo[] DataValidationMap = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "DataValidationMap");
            Assert.IsTrue(DataValidationMap != null && DataValidationMap.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }

        [TestMethod]
        public void DsfDateTimeActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfDateTimeActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfDateTimeActivity.");
            }

            MemberInfo[] DateTime = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "DateTime");
            MemberInfo[] InputFormat = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "InputFormat");
            MemberInfo[] OutputFormat = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "OutputFormat");
            MemberInfo[] TimeModifierType = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "TimeModifierType");
            MemberInfo[] TimeModifierAmountDisplay = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "TimeModifierAmountDisplay");
            MemberInfo[] TimeModifierAmount = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "TimeModifierAmount");
            MemberInfo[] Result = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "Result");

            Assert.IsTrue(DateTime != null && DateTime.Length == 1
                          && InputFormat != null && InputFormat.Length == 1
                          && OutputFormat != null && OutputFormat.Length == 1
                          && TimeModifierType != null && TimeModifierType.Length == 1
                          && TimeModifierAmountDisplay != null && TimeModifierAmountDisplay.Length == 1
                          && Result != null, "This will fail on designer binding, please update this before proceeding");
        }

        [TestMethod]
        public void DsfFileForEachActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfFileForEachActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfFileForEachActivity.");
            }

            MemberInfo[] FailOnFirstError = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "FailOnFirstError");
            MemberInfo[] SkipRows = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "SkipRows");
            MemberInfo[] FileURI = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "FileURI");
            MemberInfo[] UserName = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "UserName");
            MemberInfo[] Password = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "Password");
            MemberInfo[] DataFormat = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "DataFormat");
            MemberInfo[] FooterFormat = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "FooterFormat");
            MemberInfo[] HeaderFormat = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "HeaderFormat");
            MemberInfo[] HeaderFunc = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "HeaderFunc");
            MemberInfo[] DataFunc = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "DataFunc");
            MemberInfo[] FooterFunc = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "FooterFunc");
            MemberInfo[] ExceptionFunc = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "ExceptionFunc");

            Assert.IsTrue(FailOnFirstError != null && FailOnFirstError.Length == 1
                          && SkipRows != null && SkipRows.Length == 1
                          && FileURI != null && FileURI.Length == 1
                          && UserName != null && UserName.Length == 1
                          && Password != null && Password.Length == 1
                          && DataFormat != null && DataFormat.Length == 1
                          && FooterFormat != null && FooterFormat.Length == 1
                          && HeaderFormat != null && HeaderFormat.Length == 1
                          && HeaderFunc != null && HeaderFunc.Length == 1
                          && DataFunc != null && DataFunc.Length == 1
                          && FooterFunc != null && FooterFunc.Length == 1
                          && ExceptionFunc != null && FooterFunc.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }


        [TestMethod]
        public void DsfFindRecordSetActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfFindRecordsActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfFindRecordsActivity.");
            }

            MemberInfo[] FieldsToSearch = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "FieldsToSearch");
            MemberInfo[] SearchType = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "SearchType");
            MemberInfo[] SearchCriteria = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "SearchCriteria");
            MemberInfo[] StartIndex = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "StartIndex");
            MemberInfo[] MatchCase = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "MatchCase");
            MemberInfo[] Result = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "Result");


            Assert.IsTrue(FieldsToSearch != null && FieldsToSearch.Length == 1
                          && SearchType != null && SearchType.Length == 1
                          && SearchCriteria != null && SearchCriteria.Length == 1
                          && StartIndex != null && StartIndex.Length == 1
                          && StartIndex != null && StartIndex.Length == 1
                          && MatchCase != null && MatchCase.Length == 1
                          && Result != null, "This will fail on designer binding, please update this before proceeding");
        }


        [TestMethod]
        public void DsfForEachActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfForEachActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfForEachActivity.");
            }

            MemberInfo[] FromDisplayName = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "FromDisplayName");
            MemberInfo[] ForEachElementName = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "ForEachElementName");
            MemberInfo[] DataFunc = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "DataFunc");
            MemberInfo[] FailOnFirstError = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "FailOnFirstError");
            MemberInfo[] ElementName = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "ElementName");
            MemberInfo[] PreservedDataList = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "PreservedDataList");


            Assert.IsTrue(FromDisplayName != null && FromDisplayName.Length == 1
                          && ForEachElementName != null && ForEachElementName.Length == 1
                          && DataFunc != null && DataFunc.Length == 1
                          && FailOnFirstError != null && FailOnFirstError.Length == 1
                          && ElementName != null && ElementName.Length == 1
                          && PreservedDataList != null && PreservedDataList.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }


        [TestMethod]
        public void DsfHttpActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfHttpActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfHttpActivity.");
            }

            MemberInfo[] HttpMethod = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "HttpMethod");
            MemberInfo[] FieldName = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "FieldName");
            MemberInfo[] Uri = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "Uri");
            MemberInfo[] PostData = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "PostData");


            Assert.IsTrue(HttpMethod != null && HttpMethod.Length == 1
                          && FieldName != null && FieldName.Length == 1
                          && Uri != null && Uri.Length == 1
                          && PostData != null && PostData.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }


        [TestMethod]
        public void DsfRemoteActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfRemoteActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfRemoteActivity.");
            }

            MemberInfo[] ServiceAddress = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "ServiceAddress");


            Assert.IsTrue(ServiceAddress != null && ServiceAddress.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }


        [TestMethod]
        public void DsfRemoveActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfRemoveActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfRemoveActivity.");
            }

            MemberInfo[] RemoveText = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "RemoveText");


            Assert.IsTrue(RemoveText != null && RemoveText.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }

        [TestMethod]
        public void DsfSortRecordSetActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfSortRecordsActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfSortRecordsActivity.");
            }

            MemberInfo[] SortField = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "SortField");
            MemberInfo[] SelectedSort = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "SelectedSort");


            Assert.IsTrue(SortField != null && SortField.Length == 1
                          && SelectedSort != null & SortField.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }

        [TestMethod]
        public void DsfWebPageActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfWebPageActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfWebPageActivity.");
            }

            MemberInfo[] XMLConfiguration = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "XMLConfiguration");
            MemberInfo[] WebsiteServiceName = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "WebsiteServiceName");
            MemberInfo[] WebsiteRegionName = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "WebsiteRegionName");
            MemberInfo[] IsPreview = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "IsPreview");


            Assert.IsTrue(XMLConfiguration != null && XMLConfiguration.Length == 1
                          && WebsiteServiceName != null & WebsiteServiceName.Length == 1
                          && WebsiteRegionName != null && WebsiteRegionName.Length == 1
                          && IsPreview != null && IsPreview.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }

        [TestMethod]
        public void DsfWebsiteActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "DsfWebSiteActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find DsfWebSiteActivity.");
            }

            MemberInfo[] XMLConfiguration = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "XMLConfiguration");
            MemberInfo[] Html = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "Html");

            Assert.IsTrue(XMLConfiguration != null && XMLConfiguration.Length == 1
                          && Html != null & Html.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }

        [TestMethod]
        public void DsfTransformActivity_Property_Name_Check_Expected_No_Change_To_Properties()
        {
            Type type = _activityList.FirstOrDefault(c => c.Name == "TransformActivity");
            if(type == null)
            {
                Assert.Fail("Couldnt find TransformActivity.");
            }

            MemberInfo[] Transformation = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "Transformation");
            MemberInfo[] TransformElementName = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "TransformElementName");
            MemberInfo[] Aggregate = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "Aggregate");
            MemberInfo[] RootTag = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "RootTag");
            MemberInfo[] RemoveSourceTagsAfterTransformation = type.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, "RemoveSourceTagsAfterTransformation");

            Assert.IsTrue(Transformation != null && Transformation.Length == 1
                          && TransformElementName != null & TransformElementName.Length == 1
                          && Aggregate != null && Aggregate.Length == 1
                          && RootTag != null && RootTag.Length == 1
                          && RemoveSourceTagsAfterTransformation != null && RemoveSourceTagsAfterTransformation.Length == 1, "This will fail on designer binding, please update this before proceeding");
        }

        #endregion Check Property Names

        #region Private Methods

        private static List<Type> GetAllActivities()
        {
            var type = typeof(DsfActivityAbstract<bool>);

            List<Type> types = type.Assembly.GetTypes()
                                .Where(t => (type.IsAssignableFrom(t)))
                                .ToList();

            type = typeof(DsfActivityAbstract<string>);

            types.AddRange(type.Assembly.GetTypes()
                                .Where(t => (type.IsAssignableFrom(t)))
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
