/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.FindMissingStrategyTest
{
    /// <summary>
    /// Summary description for StaticActivityFindMissingStrategyTests
    /// </summary>
    [TestClass]
    public class StaticActivityFindMissingStrategyTests
    {
        TestContext _testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }

       

        #region Calculate Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfCalculate_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfCalculateActivity
            {
                Expression = "[[Expression]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[Expression]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region CoutRecordset Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfCountRecordset_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var nullHandlerActivity = new DsfCountRecordsetNullHandlerActivity
            {
                RecordsetName = "[[RecordsetName]]",
                CountNumber = "[[CountNumber]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(nullHandlerActivity);
            var expected = new List<string> { "[[RecordsetName]]", "[[CountNumber]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfRecordsetLength_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfRecordsetNullhandlerLengthActivity
            {
                RecordsetName = "[[RecordsetName]]",
                RecordsLength = "[[CountNumber]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[RecordsetName]]", "[[CountNumber]]" };
            CollectionAssert.AreEqual(expected, actual);
        }


        #endregion

        #region DateTime Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfDateTime_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfDateTimeActivity
            {
                DateTime = "[[DateTime]]",
                InputFormat = "[[InputFormat]]",
                OutputFormat = "[[OutputFormat]]",
                TimeModifierAmountDisplay = "[[TimeModifierAmountDisplay]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[DateTime]]", "[[InputFormat]]", "[[OutputFormat]]", "[[TimeModifierAmountDisplay]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region DateTimeDifference Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfDateTimeDifference_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfDateTimeDifferenceActivity
            {
                Input1 = "[[Input1]]",
                Input2 = "[[Input2]]",
                InputFormat = "[[InputFormat]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[Input1]]", "[[Input2]]", "[[InputFormat]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region DeleteRecords Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfDeleteRecord_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var nullHandlerActivity = new DsfDeleteRecordNullHandlerActivity
            {
                RecordsetName = "[[RecordsetName]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(nullHandlerActivity);
            var expected = new List<string> { "[[RecordsetName]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region ExecuteCommandLine Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfExecuteCommandLine_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfExecuteCommandLineActivity
            {
                CommandFileName = "[[CommandFileName]]",
                CommandResult = "[[CommandResult]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[CommandFileName]]", "[[CommandResult]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion
        

        #region ForEach Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfForEach_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfForEachActivity
            {
                ForEachElementName = "[[ForEachElementName]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[ForEachElementName]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region Index Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfIndex_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfIndexActivity
            {
                InField = "[[InField]]",
                Characters = "[[Characters]]",
                Result = "[[Result]]",
                StartIndex = "[[StartIndex]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[InField]]", "[[Characters]]", "[[Result]]", "[[StartIndex]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region NumberFormat Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfNumberFormat_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfNumberFormatActivity
            {
                Expression = "[[Expression]]",
                RoundingDecimalPlaces = "[[RoundingDecimalPlaces]]",
                DecimalPlacesToShow = "[[DecimalPlacesToShow]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[Expression]]", "[[RoundingDecimalPlaces]]", "[[DecimalPlacesToShow]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region Replace Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfReplace_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfReplaceActivity
            {
                FieldsToSearch = "[[FieldsToSearch]]",
                Find = "[[Find]]",
                ReplaceWith = "[[ReplaceWith]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[FieldsToSearch]]", "[[Find]]", "[[ReplaceWith]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region SortRecords Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfSortRecords_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfSortRecordsActivity
            {
                SortField = "[[SortField]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[SortField]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region File Ops Tests

        #region FileRead Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfFileRead_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfFileRead
            {
                InputPath = "[[InputPath]]",
                Password = "[[Password]]",
                PrivateKeyFile = "[[KeyFile]]",
                Username = "[[Username]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[InputPath]]", "[[Password]]", "[[Username]]", "[[KeyFile]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region FileWrite Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfFileWrite__Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfFileWrite
            {
                FileContents = "[[FileContents]]",
                OutputPath = "[[OutputPath]]",
                Password = "[[Password]]",
                PrivateKeyFile = "[[KeyFile]]",
                Username = "[[Username]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[FileContents]]", "[[OutputPath]]", "[[Password]]", "[[Username]]","[[KeyFile]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfFileWriteWithBase64_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfFileWriteWithBase64
            {
                FileContents = "[[FileContents]]",
                OutputPath = "[[OutputPath]]",
                Password = "[[Password]]",
                PrivateKeyFile = "[[KeyFile]]",
                Username = "[[Username]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[FileContents]]", "[[OutputPath]]", "[[Password]]", "[[Username]]", "[[KeyFile]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region FolderRead Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfFolderRead_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfFolderRead
            {
                InputPath = "[[InputPath]]",
                Password = "[[Password]]",
                PrivateKeyFile = "[[PrivateKey]]",
                Username = "[[Username]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "False", "False", "False", "[[InputPath]]", "[[Password]]", "[[Username]]","[[PrivateKey]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region PathCopy Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfPathCopy_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfPathCopy
            {
                InputPath = "[[InputPath]]",
                OutputPath = "[[OutputPath]]",
                Password = "[[Password]]",
                PrivateKeyFile = "[[KeyFile]]",
                Username = "[[Username]]",
                DestinationPassword = "[[DestPassword]]",
                DestinationUsername = "[[DestUsername]]",
                DestinationPrivateKeyFile = "[[DestKeyFile]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[InputPath]]", "[[OutputPath]]", "[[DestUsername]]", "[[DestKeyFile]]", "[[DestPassword]]", "[[Password]]", "[[Username]]", "[[KeyFile]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region PathCreate Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfPathCreate_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfPathCreate
            {
                OutputPath = "[[OutputPath]]",
                Password = "[[Password]]",
                Username = "[[Username]]",
                PrivateKeyFile = "[[KeyFile]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[OutputPath]]", "[[Password]]", "[[Username]]","[[KeyFile]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region PathDelete Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfPathDelete_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfPathDelete
            {
                InputPath = "[[InputPath]]",
                Password = "[[Password]]",
                Username = "[[Username]]",
                PrivateKeyFile = "[[KeyFile]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[InputPath]]", "[[Password]]", "[[Username]]","[[KeyFile]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region PathMove Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfPathMove_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfPathMove
            {
                InputPath = "[[InputPath]]",
                OutputPath = "[[OutputPath]]",
                Password = "[[Password]]",
                Username = "[[Username]]",
                PrivateKeyFile = "[[KeyFile]]",
                DestinationPassword = "[[DestPassword]]",
                DestinationUsername = "[[DestUsername]]",
                DestinationPrivateKeyFile = "[[DestKeyFile]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[InputPath]]", "[[OutputPath]]", "[[DestUsername]]","[[DestKeyFile]]", "[[DestPassword]]", "[[Password]]", "[[Username]]","[[KeyFile]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region PathRename Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfPathRename_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfPathRename
            {
                InputPath = "[[InputPath]]",
                OutputPath = "[[OutputPath]]",
                Password = "[[Password]]",
                Username = "[[Username]]",
                PrivateKeyFile = "[[KeyFile]]",
                DestinationPassword = "[[DestPassword]]",
                DestinationUsername = "[[DestUsername]]",
                DestinationPrivateKeyFile = "[[DestKeyFile]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[InputPath]]", "[[OutputPath]]", "[[DestUsername]]","[[DestKeyFile]]", "[[DestPassword]]", "[[Password]]", "[[Username]]","[[KeyFile]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region UnZip Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfUnZip_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfUnZip
            {
                ArchivePassword = "[[ArchivePassword]]",
                OutputPath = "[[OutputPath]]",
                InputPath = "[[InputPath]]",
                Password = "[[Password]]",
                Username = "[[Username]]",
                PrivateKeyFile = "[[KeyFile]]",
                DestinationPassword = "[[DestPassword]]",
                DestinationUsername = "[[DestUsername]]",
                DestinationPrivateKeyFile = "[[DestKeyFile]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[ArchivePassword]]", "[[InputPath]]", "[[OutputPath]]", "[[DestUsername]]","[[DestKeyFile]]", "[[DestPassword]]", "[[Password]]", "[[Username]]","[[KeyFile]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region Zip Activity Tests

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(Dev2FindMissingStrategyFactory))]
        public void Dev2FindMissingStrategyFactory_GetActivityFieldsOff_DsfZip_Activity_ExpectAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfZip
            {
                ArchiveName = "[[ArchiveName]]",
                ArchivePassword = "[[ArchivePassword]]",
                CompressionRatio = "[[CompressionRatio]]",
                InputPath = "[[InputPath]]",
                OutputPath = "[[OutputPath]]",
                Password = "[[Password]]",
                Username = "[[Username]]",
                PrivateKeyFile = "[[KeyFile]]",
                DestinationPassword = "[[DestPassword]]",
                DestinationUsername = "[[DestUsername]]",
                DestinationPrivateKeyFile = "[[DestKeyFile]]",
                Result = "[[Result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> {"[[ArchivePassword]]", "[[ArchiveName]]", "[[CompressionRatio]]", "[[InputPath]]", "[[OutputPath]]", "[[DestUsername]]","[[DestKeyFile]]", "[[DestPassword]]", "[[Password]]", "[[Username]]","[[KeyFile]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #endregion
    }
}
