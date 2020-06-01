/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Factories;
using Dev2.Interfaces;
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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Calculate Activity Tests

        [TestMethod]
        [Timeout(60000)]
        public void GetActivityFieldsOffDsfCalculateActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfCalculateActivity();
            activity.Expression = "[[Expression]]";
            activity.Result = "[[Result]]";
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
        public void GetActivityFieldsOffDsfCountRecordsetActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var nullHandlerActivity = new DsfCountRecordsetNullHandlerActivity();
            nullHandlerActivity.RecordsetName = "[[RecordsetName]]";
            nullHandlerActivity.CountNumber = "[[CountNumber]]";
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(nullHandlerActivity);
            var expected = new List<string> { "[[RecordsetName]]", "[[CountNumber]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetActivityFieldsOffDsfRecordsetLengthActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfRecordsetNullhandlerLengthActivity();
            activity.RecordsetName = "[[RecordsetName]]";
            activity.RecordsLength = "[[CountNumber]]";
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
        public void GetActivityFieldsOffDsfDateTimeActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfDateTimeActivity();
            activity.DateTime = "[[DateTime]]";
            activity.InputFormat = "[[InputFormat]]";
            activity.OutputFormat = "[[OutputFormat]]";
            activity.TimeModifierAmountDisplay = "[[TimeModifierAmountDisplay]]";
            activity.Result = "[[Result]]";
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
        public void GetActivityFieldsOffDsfDateTimeDifferenceActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfDateTimeDifferenceActivity();
            activity.Input1 = "[[Input1]]";
            activity.Input2 = "[[Input2]]";
            activity.InputFormat = "[[InputFormat]]";
            activity.Result = "[[Result]]";
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
        public void GetActivityFieldsOffDsfDeleteRecordActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var nullHandlerActivity = new DsfDeleteRecordNullHandlerActivity();
            nullHandlerActivity.RecordsetName = "[[RecordsetName]]";
            nullHandlerActivity.Result = "[[Result]]";
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
        public void GetActivityFieldsOffDsfExecuteCommandLineActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfExecuteCommandLineActivity();
            activity.CommandFileName = "[[CommandFileName]]";
            activity.CommandResult = "[[CommandResult]]";
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
        public void GetActivityFieldsOffDsfForEachActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfForEachActivity();
            activity.ForEachElementName = "[[ForEachElementName]]";
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
        public void GetActivityFieldsOffDsfIndexActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfIndexActivity();
            activity.InField = "[[InField]]";
            activity.Characters = "[[Characters]]";
            activity.Result = "[[Result]]";
            activity.StartIndex = "[[StartIndex]]";
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
        public void GetActivityFieldsOffDsfNumberFormatActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfNumberFormatActivity();
            activity.Expression = "[[Expression]]";
            activity.RoundingDecimalPlaces = "[[RoundingDecimalPlaces]]";
            activity.DecimalPlacesToShow = "[[DecimalPlacesToShow]]";
            activity.Result = "[[Result]]";
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
        public void GetActivityFieldsOffDsfReplaceActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfReplaceActivity();
            activity.FieldsToSearch = "[[FieldsToSearch]]";
            activity.Find = "[[Find]]";
            activity.ReplaceWith = "[[ReplaceWith]]";
            activity.Result = "[[Result]]";
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
        public void GetActivityFieldsOffDsfSortRecordsActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfSortRecordsActivity();
            activity.SortField = "[[SortField]]";
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
        public void GetActivityFieldsOffDsfFileReadActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfFileRead();
            activity.InputPath = "[[InputPath]]";
            activity.Password = "[[Password]]";
            activity.PrivateKeyFile = "[[KeyFile]]";
            activity.Username = "[[Username]]";
            activity.Result = "[[Result]]";
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
        public void GetActivityFieldsOffDsfFileWriteActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfFileWrite();
            activity.FileContents = "[[FileContents]]";
            activity.OutputPath = "[[OutputPath]]";
            activity.Password = "[[Password]]";
            activity.PrivateKeyFile = "[[KeyFile]]";
            activity.Username = "[[Username]]";
            activity.Result = "[[Result]]";
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            var actual = strategy.GetActivityFields(activity);
            var expected = new List<string> { "[[FileContents]]", "[[OutputPath]]", "[[Password]]", "[[Username]]","[[KeyFile]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region FolderRead Activity Tests

        [TestMethod]
        [Timeout(60000)]
        public void GetActivityFieldsOffDsfFolderReadActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfFolderRead();
            activity.InputPath = "[[InputPath]]";
            activity.Password = "[[Password]]";
            activity.PrivateKeyFile = "[[PrivateKey]]";
            activity.Username = "[[Username]]";
            activity.Result = "[[Result]]";
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
        public void GetActivityFieldsOffDsfPathCopyActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfPathCopy();
            activity.InputPath = "[[InputPath]]";
            activity.OutputPath = "[[OutputPath]]";
            activity.Password = "[[Password]]";
            activity.PrivateKeyFile = "[[KeyFile]]";
            activity.Username = "[[Username]]";
            activity.DestinationPassword = "[[DestPassword]]";
            activity.DestinationUsername = "[[DestUsername]]";
            activity.DestinationPrivateKeyFile = "[[DestKeyFile]]";
            activity.Result = "[[Result]]";
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
        public void GetActivityFieldsOffDsfPathCreateActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfPathCreate();
            activity.OutputPath = "[[OutputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.PrivateKeyFile = "[[KeyFile]]";
            activity.Result = "[[Result]]";
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
        public void GetActivityFieldsOffDsfPathDeleteActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfPathDelete();
            activity.InputPath = "[[InputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.PrivateKeyFile = "[[KeyFile]]";
            activity.Result = "[[Result]]";
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
        public void GetActivityFieldsOffDsfPathMoveActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfPathMove();
            activity.InputPath = "[[InputPath]]";
            activity.OutputPath = "[[OutputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.PrivateKeyFile = "[[KeyFile]]";
            activity.DestinationPassword = "[[DestPassword]]";
            activity.DestinationUsername = "[[DestUsername]]";
            activity.DestinationPrivateKeyFile = "[[DestKeyFile]]";
            activity.Result = "[[Result]]";
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
        public void GetActivityFieldsOffDsfPathRenameActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfPathRename();
            activity.InputPath = "[[InputPath]]";
            activity.OutputPath = "[[OutputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.PrivateKeyFile = "[[KeyFile]]";
            activity.DestinationPassword = "[[DestPassword]]";
            activity.DestinationUsername = "[[DestUsername]]";
            activity.DestinationPrivateKeyFile = "[[DestKeyFile]]";
            activity.Result = "[[Result]]";
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
        public void GetActivityFieldsOffDsfUnZipActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfUnZip();
            activity.ArchivePassword = "[[ArchivePassword]]";
            activity.OutputPath = "[[OutputPath]]";
            activity.InputPath = "[[InputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.PrivateKeyFile = "[[KeyFile]]";
            activity.DestinationPassword = "[[DestPassword]]";
            activity.DestinationUsername = "[[DestUsername]]";
            activity.DestinationPrivateKeyFile = "[[DestKeyFile]]";
            activity.Result = "[[Result]]";
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
        public void GetActivityFieldsOffDsfZipActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var activity = new DsfZip();
            activity.ArchiveName = "[[ArchiveName]]";
            activity.ArchivePassword = "[[ArchivePassword]]";
            activity.CompressionRatio = "[[CompressionRatio]]";
            activity.InputPath = "[[InputPath]]";
            activity.OutputPath = "[[OutputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.PrivateKeyFile = "[[KeyFile]]";
            activity.DestinationPassword = "[[DestPassword]]";
            activity.DestinationUsername = "[[DestUsername]]";
            activity.DestinationPrivateKeyFile = "[[DestKeyFile]]";
            activity.Result = "[[Result]]";
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
