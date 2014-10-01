
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Activities;
using Dev2.Enums;
using Dev2.Factories;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.FindMissingStrategyTest
{
    /// <summary>
    /// Summary description for StaticActivityFindMissingStrategyTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class StaticActivityFindMissingStrategyTests
    {
        private TestContext _testContextInstance;

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
        public void GetActivityFieldsOffDsfCalculateActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfCalculateActivity activity = new DsfCalculateActivity();
            activity.Expression = "[[Expression]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[Expression]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region CoutRecordset Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfCountRecordsetActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfCountRecordsetActivity activity = new DsfCountRecordsetActivity();
            activity.RecordsetName = "[[RecordsetName]]";
            activity.CountNumber = "[[CountNumber]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[RecordsetName]]", "[[CountNumber]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetActivityFieldsOffDsfRecordsetLengthActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfRecordsetLengthActivity activity = new DsfRecordsetLengthActivity();
            activity.RecordsetName = "[[RecordsetName]]";
            activity.RecordsLength = "[[CountNumber]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[RecordsetName]]", "[[CountNumber]]" };
            CollectionAssert.AreEqual(expected, actual);
        }


        #endregion

        #region DateTime Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfDateTimeActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfDateTimeActivity activity = new DsfDateTimeActivity();
            activity.DateTime = "[[DateTime]]";
            activity.InputFormat = "[[InputFormat]]";
            activity.OutputFormat = "[[OutputFormat]]";
            activity.TimeModifierAmountDisplay = "[[TimeModifierAmountDisplay]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[DateTime]]", "[[InputFormat]]", "[[OutputFormat]]", "[[TimeModifierAmountDisplay]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region DateTimeDifference Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfDateTimeDifferenceActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfDateTimeDifferenceActivity activity = new DsfDateTimeDifferenceActivity();
            activity.Input1 = "[[Input1]]";
            activity.Input2 = "[[Input2]]";
            activity.InputFormat = "[[InputFormat]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[Input1]]", "[[Input2]]", "[[InputFormat]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region DeleteRecords Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfDeleteRecordActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfDeleteRecordActivity activity = new DsfDeleteRecordActivity();
            activity.RecordsetName = "[[RecordsetName]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[RecordsetName]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region ExecuteCommandLine Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfExecuteCommandLineActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfExecuteCommandLineActivity activity = new DsfExecuteCommandLineActivity();
            activity.CommandFileName = "[[CommandFileName]]";
            activity.CommandResult = "[[CommandResult]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[CommandFileName]]", "[[CommandResult]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region FindRecords Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfFindRecordsActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfFindRecordsActivity activity = new DsfFindRecordsActivity();
            activity.FieldsToSearch = "[[FieldsToSearch]]";
            activity.SearchCriteria = "[[SearchCriteria]]";
            activity.Result = "[[Result]]";
            activity.StartIndex = "[[StartIndex]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[FieldsToSearch]]", "[[SearchCriteria]]", "[[Result]]", "[[StartIndex]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region ForEach Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfForEachActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfForEachActivity activity = new DsfForEachActivity();
            activity.ForEachElementName = "[[ForEachElementName]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[ForEachElementName]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region Index Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfIndexActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfIndexActivity activity = new DsfIndexActivity();
            activity.InField = "[[InField]]";
            activity.Characters = "[[Characters]]";
            activity.Result = "[[Result]]";
            activity.StartIndex = "[[StartIndex]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[InField]]", "[[Characters]]", "[[Result]]", "[[StartIndex]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region NumberFormat Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfNumberFormatActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfNumberFormatActivity activity = new DsfNumberFormatActivity();
            activity.Expression = "[[Expression]]";
            activity.RoundingDecimalPlaces = "[[RoundingDecimalPlaces]]";
            activity.DecimalPlacesToShow = "[[DecimalPlacesToShow]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[Expression]]", "[[RoundingDecimalPlaces]]", "[[DecimalPlacesToShow]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region Replace Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfReplaceActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfReplaceActivity activity = new DsfReplaceActivity();
            activity.FieldsToSearch = "[[FieldsToSearch]]";
            activity.Find = "[[Find]]";
            activity.ReplaceWith = "[[ReplaceWith]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[FieldsToSearch]]", "[[Find]]", "[[ReplaceWith]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region SortRecords Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfSortRecordsActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfSortRecordsActivity activity = new DsfSortRecordsActivity();
            activity.SortField = "[[SortField]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[SortField]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region File Ops Tests

        #region FileRead Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfFileReadActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfFileRead activity = new DsfFileRead();
            activity.InputPath = "[[InputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[InputPath]]", "[[Password]]", "[[Username]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region FileWrite Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfFileWriteActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfFileWrite activity = new DsfFileWrite();
            activity.FileContents = "[[FileContents]]";
            activity.OutputPath = "[[OutputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[FileContents]]", "[[OutputPath]]", "[[Password]]", "[[Username]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region FolderRead Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfFolderReadActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfFolderRead activity = new DsfFolderRead();
            activity.InputPath = "[[InputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "False", "False", "False", "[[InputPath]]", "[[Password]]", "[[Username]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region PathCopy Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfPathCopyActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfPathCopy activity = new DsfPathCopy();
            activity.InputPath = "[[InputPath]]";
            activity.OutputPath = "[[OutputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.DestinationPassword = "[[DestPassword]]";
            activity.DestinationUsername = "[[DestUsername]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[InputPath]]", "[[OutputPath]]", "[[DestUsername]]", "[[DestPassword]]", "[[Password]]", "[[Username]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region PathCreate Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfPathCreateActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfPathCreate activity = new DsfPathCreate();
            activity.OutputPath = "[[OutputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[OutputPath]]", "[[Password]]", "[[Username]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region PathDelete Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfPathDeleteActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfPathDelete activity = new DsfPathDelete();
            activity.InputPath = "[[InputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[InputPath]]", "[[Password]]", "[[Username]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region PathMove Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfPathMoveActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfPathMove activity = new DsfPathMove();
            activity.InputPath = "[[InputPath]]";
            activity.OutputPath = "[[OutputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.DestinationPassword = "[[DestPassword]]";
            activity.DestinationUsername = "[[DestUsername]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[InputPath]]", "[[OutputPath]]", "[[DestUsername]]", "[[DestPassword]]", "[[Password]]", "[[Username]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region PathRename Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfPathRenameActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfPathRename activity = new DsfPathRename();
            activity.InputPath = "[[InputPath]]";
            activity.OutputPath = "[[OutputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.DestinationPassword = "[[DestPassword]]";
            activity.DestinationUsername = "[[DestUsername]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[InputPath]]", "[[OutputPath]]", "[[DestUsername]]", "[[DestPassword]]", "[[Password]]", "[[Username]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region UnZip Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfUnZipActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfUnZip activity = new DsfUnZip();
            activity.ArchivePassword = "[[ArchivePassword]]";
            activity.OutputPath = "[[OutputPath]]";
            activity.InputPath = "[[InputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.DestinationPassword = "[[DestPassword]]";
            activity.DestinationUsername = "[[DestUsername]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[ArchivePassword]]", "[[InputPath]]", "[[OutputPath]]", "[[DestUsername]]", "[[DestPassword]]", "[[Password]]", "[[Username]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region Zip Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfZipActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfZip activity = new DsfZip();
            activity.ArchiveName = "[[ArchiveName]]";
            activity.ArchivePassword = "[[ArchivePassword]]";
            activity.CompressionRatio = "[[CompressionRatio]]";
            activity.InputPath = "[[InputPath]]";
            activity.OutputPath = "[[OutputPath]]";
            activity.Password = "[[Password]]";
            activity.Username = "[[Username]]";
            activity.DestinationPassword = "[[DestPassword]]";
            activity.DestinationUsername = "[[DestUsername]]";
            activity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.StaticActivity);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> {"[[ArchivePassword]]", "[[ArchiveName]]", "[[CompressionRatio]]", "[[InputPath]]", "[[OutputPath]]", "[[DestUsername]]", "[[DestPassword]]", "[[Password]]", "[[Username]]", "[[Result]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #endregion
    }
}
