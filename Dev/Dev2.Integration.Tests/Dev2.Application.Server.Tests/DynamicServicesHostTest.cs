
using Dev2.DynamicServices;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests
{
    /// <summary>
    /// Summary description for DynamicServicesHostTest
    /// </summary>
    [TestClass]
    public class DynamicServicesHostTest
    {

        #region Test Variables

        const string ServicesDir = "Services";
        const string SourcesDir = "Sources";
        static readonly string[] DirectoryNames = new[] { SourcesDir, ServicesDir };
        static string _resourceDir;
        static string _resourceFilePath;
        static string _illegalExtensionResourceFilePath;
        static string _illegalResourceFilePath;
        static string _duplicateResourceFilePath;

        #endregion Test Variables

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get;
            set;
        }

        #region Class Initialize/Cleanup

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _resourceDir = Path.Combine(context.TestRunDirectory, Path.GetRandomFileName());
            _resourceFilePath = Path.Combine(Path.Combine(_resourceDir, ServicesDir), "CalculateTool_StaticValues_Sum" + ".xml");
            _illegalResourceFilePath = Path.Combine(Path.Combine(_resourceDir, ServicesDir), "CalculateTool_StaticValues_Sum_Illegal" + ".xml");
            _illegalExtensionResourceFilePath = Path.Combine(Path.Combine(_resourceDir, ServicesDir), "CalculateTool_StaticValues_Sum" + ".xml.bak");
            _duplicateResourceFilePath = Path.Combine(Path.Combine(_resourceDir, ServicesDir), "CalculateTool_StaticValues_Sum_Duplicate" + ".xml");

            foreach (var directoryName in DirectoryNames)
            {
                Directory.CreateDirectory(Path.Combine(_resourceDir, directoryName));
            }

            Directory.SetCurrentDirectory(_resourceDir);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            if (Directory.Exists(_resourceDir))
            {
                try
                {
                    Directory.Delete(_resourceDir, true);
                }
                catch (Exception ex)
                {
                    return;
                }
            }
        }

        [TestInitialize]
        public void TestInit()
        {
            RemoveResourceFile();
            RemoveResourceFileWithIllegalName();
            RemoveDuplicateResourceFile();
        }

        #endregion

        #region RestoreResources Tests

        [TestMethod]
        public void RestoreResourcesServicesListCountIncreasesByOneAfterAdd()
        {
            var host = new DynamicServicesHost();
            var countBefore = host.Services.Count;
            
            AddResourceFile();
            host.RestoreResources(DirectoryNames);

            var countAfter = host.Services.Count;

            Assert.AreEqual(countBefore + 1, countAfter);
        }

        [TestMethod]
        public void RestoreResourcesServicesListContainsResourceAfterAdd()
        {
            var host = new DynamicServicesHost();

            AddResourceFile();
            host.RestoreResources(DirectoryNames);
            var exists = host.Services.Exists(s => s.Name == "CalculateTool_StaticValues_Sum");
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void RestoreResourcesServicesListCountDecreasesByOneAfterRemove()
        {
            AddResourceFile();

            var host = new DynamicServicesHost();
            var countBefore = host.Services.Count;
            
            RemoveResourceFile();
            host.RestoreResources(DirectoryNames);

            var countAfter = host.Services.Count;

            Assert.AreEqual(countBefore - 1, countAfter);
        }

        [TestMethod]
        public void RestoreResourcesServicesListDoesNotContainResourceAfterAdd()
        {
            AddResourceFile();
            var host = new DynamicServicesHost();

            RemoveResourceFile();
            host.RestoreResources(DirectoryNames);

            var exists = host.Services.Exists(s => s.Name == TestResource.Calculate_RecordSet_Subtract_Name);
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void RestoreResources_GivenResourceWithIllegalName_Expected_ResourceNotLoaded()
        {
            var host = new DynamicServicesHost();
            var countBefore = host.Services.Count;

            AddResourceFileWithIllegalName();
            host.RestoreResources(DirectoryNames);

            var countAfter = host.Services.Count;

            Assert.AreEqual(countBefore, countAfter);
        }

        [TestMethod]
        public void RestoreResources_GivenResourceWithIllegalExtension_Expected_ResourceNotLoaded()
        {
            var host = new DynamicServicesHost();
            var countBefore = host.Services.Count;

            AddResourceFileWithIllegalExtension();
            host.RestoreResources(DirectoryNames);

            var countAfter = host.Services.Count;

            Assert.AreEqual(countBefore, countAfter);
        }

        [TestMethod]
        public void RestoreResources_GivenDuplicateResource_Expected_OnlyOneResourceLoaded()
        {
            var host = new DynamicServicesHost();
            var countBefore = host.Services.Count;

            AddResourceFile();
            AddDuplcateResourceFile();
            host.RestoreResources(DirectoryNames);

            var countAfter = host.Services.Count;

            Assert.AreEqual(countBefore + 1, countAfter);
        }

        #endregion RestoreResources Tests

        #region EmptyToNull Test

        [TestMethod]
        public void TestDBNullInsert_Expected_clientID()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestDBEmptyToNull", "testType=insert");
            string result = TestHelper.PostDataToWebserver(postData);

            Assert.Inconclusive();
            //Assert.IsTrue((result.IndexOf("<userID>") > 0));
        }

        [TestMethod]
        public void TestDBNullLogicNullValue_Expected_ZZZ()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestDBEmptyToNull", "testType=logic&nullLogicValue=");
            string result = TestHelper.PostDataToWebserver(postData);

            Assert.Inconclusive();
            //Assert.IsTrue((result.IndexOf("ZZZ - Null value passed in") > 0));
        }

        [TestMethod]
        public void TestDBNullLogicNotNullValue_Expected_AAA()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestDBEmptyToNull", "testType=logic&nullLogicValue=dummy");
            string result = TestHelper.PostDataToWebserver(postData);

            Assert.Inconclusive();
            //Assert.IsTrue((result.IndexOf("AAA - Non null value passed in") > 0));
        }
        [TestMethod]
        public void TestDBNullLogicEmptyNullConvertOffValue_Expected_AAA()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestDBEmptyToNull", "testType=notActive&nullLogicValue=");
            string result = TestHelper.PostDataToWebserver(postData);

            Assert.Inconclusive();
            //Assert.IsTrue((result.IndexOf("AAA - Non null value passed in") > 0));
        }

        [TestMethod]
        public void TestPluginNull_Expected_AnonymousSend()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestPluginEmptyToNull", "testType=nullActive&sender=");
            string result = TestHelper.PostDataToWebserver(postData);

            Assert.Inconclusive();
            //Assert.IsTrue((result.IndexOf("Anonymous email sent") > 0));
        }

        [TestMethod]
        public void TestPluginNonNull_Expected_FromInResult()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestPluginEmptyToNull", "testType=nullActive&sender=test@domain.local");
            string result = TestHelper.PostDataToWebserver(postData);

            Assert.Inconclusive();
            //Assert.IsTrue((result.IndexOf("from test@domain.local") > 0));
        }

        #endregion EmptyToNull Test

        #region Helper methods

        static void AddResourceFile()
        {
            RemoveResourceFile();
            File.WriteAllText(_resourceFilePath, TestResource.ServerSignedService);
        }

        static void AddResourceFileWithIllegalName()
        {
            RemoveResourceFileWithIllegalName();
            File.WriteAllText(_illegalResourceFilePath, TestResource.ServerSignedService);
        }

        static void AddResourceFileWithIllegalExtension()
        {
            RemoveResourceFileWithIllegalName();
            File.WriteAllText(_illegalExtensionResourceFilePath, TestResource.ServerSignedService);
        }

        static void AddDuplcateResourceFile()
        {
            RemoveResourceFileWithIllegalName();
            File.WriteAllText(_duplicateResourceFilePath, TestResource.ServerSignedService);
        }

        static void RemoveResourceFile()
        {
            if (File.Exists(_resourceFilePath))
            {
                try
                {
                    File.Delete(_resourceFilePath);
                }
                catch
                {
                }
            }
        }

        static void RemoveResourceFileWithIllegalName()
        {
            if (File.Exists(_illegalResourceFilePath))
            {
                try
                {
                    File.Delete(_illegalResourceFilePath);
                }
                catch(Exception)
                {
                }
            }
        }

        static void RemoveResourceFileWithIllegalExtension()
        {
            if (File.Exists(_illegalExtensionResourceFilePath))
            {
                try
                {
                    File.Delete(_illegalExtensionResourceFilePath);
                }
                catch (Exception)
                {
                }
            }
        }

        static void RemoveDuplicateResourceFile()
        {
            if (File.Exists(_duplicateResourceFilePath))
            {
                try
                {
                    File.Delete(_duplicateResourceFilePath);
                }
                catch (Exception)
                {
                }
            }
        }

        #endregion

    }
}
