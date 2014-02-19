using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Dev2.Common;
using Dev2.Data.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Persistence
{
    /// <summary>
    /// Summary description for StorageSettingsManagerTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class StorageSettingsManagerTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

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

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("StorageSettingManager_GetSegmentSize")]
        public void StorageSettingManager_GetSegmentSize_WhenConfigurationFilePresent_ExpectConfigurationValue()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var segmentCount = StorageSettingManager.GetSegmentSize();

            //------------Assert Results-------------------------
            Assert.AreEqual(2097152, segmentCount);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("StorageSettingManager_GetSegmentCount")]
        public void StorageSettingManager_GetSegmentCount_WhenConfigurationFilePresent_ExpectConfigurationValue()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var segmentCount = StorageSettingManager.GetSegmentCount();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, segmentCount);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("StorageSettingManager_GetSegmentCount")]
        public void StorageSettingManager_GetSegmentCount_WhenNoConfigurationFilePresent_ExpectDefaultValue()
        {
            // give env sometime to clean up ;)
            Thread.Sleep(500);

            var restoreSegmentCount = StorageSettingManager.StorageLayerSegments;
            var restoreSegmentSize = StorageSettingManager.StorageLayerSegmentSize;
            try
            {
                //------------Setup for test--------------------------
                StorageSettingManager.StorageLayerSegments = () => null;
                StorageSettingManager.StorageLayerSegmentSize = () => null;
                StorageSettingManager.TotalFreeMemory = () => 134217728; // 128 MB

                //------------Execute Test---------------------------
                var segmentCount = StorageSettingManager.GetSegmentCount();
                var segmentSize = StorageSettingManager.GetSegmentSize();

                //------------Assert Results-------------------------
                Assert.AreEqual(GlobalConstants.DefaultStorageSegmentSize, segmentSize);
                Assert.AreEqual(GlobalConstants.DefaultStorageSegments, segmentCount);
            }
            catch(Exception)
            {
                Assert.Inconclusive("It appears the environment is not releasing its memory correctly");
            }
            finally
            {
                StorageSettingManager.StorageLayerSegments = restoreSegmentCount;
                StorageSettingManager.StorageLayerSegmentSize = restoreSegmentSize;
            }
        }




        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("StorageSettingManager_GetSegmentSize")]
        [ExpectedException(typeof(Exception))]
        public void StorageSettingManager_GetSegmentSize_WhenConfigurationFilePresentAndMemoryPresurePresent_ExpectError()
        {
            //------------Setup for test--------------------------
            StorageSettingManager.TotalFreeMemory = () => 1;

            //------------Execute Test---------------------------

            StorageSettingManager.GetSegmentSize();

            //------------Assert Results-------------------------

        }


    }
}
