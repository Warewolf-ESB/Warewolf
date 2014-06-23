using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Dev2.Common;
using Dev2.Studio.Core.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Configuration
{
    [TestClass]    
    public class RuntimeConfigurationAssemblyRepositoryTests
    {
        #region Fields

        static TestContext _testContext;

        #endregion

        #region Setup

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            _testContext = testContext;
        }

        #endregion

        #region Methods

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddWhereHashIsNullExpectedArgumentException()
        {
            byte[] assemblyData = RuntimeConfigurationAssemblyRepositoryHelperMethods.GetTestAssemblyData();
            string hash = null;
            string repositoryPath = GetUniqueRepositoryPath();
            RuntimeConfigurationAssemblyRepository configurationAssemblyRepository = new RuntimeConfigurationAssemblyRepository(repositoryPath);

            // Add assembly
            configurationAssemblyRepository.Add(hash, assemblyData);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddWhereHashIsEmptyExpectedArgumentException()
        {
            byte[] assemblyData = RuntimeConfigurationAssemblyRepositoryHelperMethods.GetTestAssemblyData();
            string hash = "";
            string repositoryPath = GetUniqueRepositoryPath();
            RuntimeConfigurationAssemblyRepository configurationAssemblyRepository = new RuntimeConfigurationAssemblyRepository(repositoryPath);

            // Add assembly
            configurationAssemblyRepository.Add(hash, assemblyData);
        }

        [TestMethod]
        public void AddExpectedAssemblyWrittenToDiskInCorrectFolder()
        {
            byte[] assemblyData = RuntimeConfigurationAssemblyRepositoryHelperMethods.GetTestAssemblyData();
            string hash = "ABC";
            string repositoryPath = GetUniqueRepositoryPath();
            string assemblyPath = Path.Combine(repositoryPath, hash);
            assemblyPath = Path.Combine(assemblyPath, GlobalConstants.Dev2RuntimeConfigurationAssemblyName);
            RuntimeConfigurationAssemblyRepository configurationAssemblyRepository = new RuntimeConfigurationAssemblyRepository(repositoryPath);

            // Ensure repository is empty
            Assert.AreEqual(0, configurationAssemblyRepository.AllHashes().Count(), "The repository isn't empty, this test requries an empty repository.");

            // Add assembly
            configurationAssemblyRepository.Add(hash, assemblyData);

            // Check file was written correctly
            byte[] savedAssemblyData = File.ReadAllBytes(assemblyPath);
            CollectionAssert.AreEqual(assemblyData, savedAssemblyData, "The assembly wasn't written to disk correctly.");
        }

        [TestMethod]
        public void AllHashesExpectedAllDirectoriesInTheRepositoryDirectory()
        {
            List<string> expectedHashes = new List<string>
            {
                "ABC",
                "XYZ"
            };

            string repositoryPath = GetUniqueRepositoryPath();
            RuntimeConfigurationAssemblyRepository configurationAssemblyRepository = new RuntimeConfigurationAssemblyRepository(repositoryPath);

            // Create hash paths
            foreach(string hash in expectedHashes)
            {
                string hashPath = Path.Combine(repositoryPath, hash);
                Directory.CreateDirectory(hashPath);
            }

            // Get and check hashes
            List<string> actualHashes = configurationAssemblyRepository.AllHashes().ToList();
            CollectionAssert.AreEqual(expectedHashes, actualHashes, "The hashes retrieved from the repository don't match the hashes that were created for this test.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LoadWhereHashIsNullExpectedArgumentException()
        {
            string hash = null;
            string repositoryPath = GetUniqueRepositoryPath();
            RuntimeConfigurationAssemblyRepository configurationAssemblyRepository = new RuntimeConfigurationAssemblyRepository(repositoryPath);

            // Load assembly
            configurationAssemblyRepository.Load(hash);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LoadWhereHashIsEmptyExpectedArgumentException()
        {
            string hash = "";
            string repositoryPath = GetUniqueRepositoryPath();
            RuntimeConfigurationAssemblyRepository configurationAssemblyRepository = new RuntimeConfigurationAssemblyRepository(repositoryPath);

            // Load assembly
            configurationAssemblyRepository.Load(hash);
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void LoadWhereHashDoesntExistExpectedArgumentException()
        {
            string hash = "CAKE";
            string repositoryPath = GetUniqueRepositoryPath();
            RuntimeConfigurationAssemblyRepository configurationAssemblyRepository = new RuntimeConfigurationAssemblyRepository(repositoryPath);

            // Load assembly
            configurationAssemblyRepository.Load(hash);
        }

        [TestMethod]
        public void LoadExpectedAssemblyLoadedFromDisk()
        {
            byte[] assemblyData = RuntimeConfigurationAssemblyRepositoryHelperMethods.GetTestAssemblyData();
            string hash = "ABC";
            string repositoryPath = GetUniqueRepositoryPath();
            string hashPath = Path.Combine(repositoryPath, hash);
            string assemblyPath = Path.Combine(hashPath, GlobalConstants.Dev2RuntimeConfigurationAssemblyName);
            RuntimeConfigurationAssemblyRepository configurationAssemblyRepository = new RuntimeConfigurationAssemblyRepository(repositoryPath);

            // Ensure repository is empty
            Assert.AreEqual(0, configurationAssemblyRepository.AllHashes().Count(), "The repository isn't empty, this test requries an empty repository.");

            // Write assembly to disk
            Directory.CreateDirectory(hashPath);
            File.WriteAllBytes(assemblyPath, assemblyData);

            // Load assembly
            Assembly actualAssembly = configurationAssemblyRepository.Load(hash);

            // Check that loaded assembly macthes the expected
            Assembly expectedAssembly = typeof(RuntimeConfigurationAssemblyRepositoryTests).Assembly;
            Assert.AreEqual(expectedAssembly.FullName, actualAssembly.FullName, "The incorrect assembly loaded from disk.");
        }

        [TestMethod]
        public void LoadMultipleTimesExpectedAssemblyLoadedFromDiskThenCached()
        {
            byte[] assemblyData = RuntimeConfigurationAssemblyRepositoryHelperMethods.GetTestAssemblyData();
            string hash = "ABC";
            string repositoryPath = GetUniqueRepositoryPath();
            string hashPath = Path.Combine(repositoryPath, hash);
            string assemblyPath = Path.Combine(hashPath, GlobalConstants.Dev2RuntimeConfigurationAssemblyName);
            RuntimeConfigurationAssemblyRepository configurationAssemblyRepository = new RuntimeConfigurationAssemblyRepository(repositoryPath);

            // Ensure repository is empty
            Assert.AreEqual(0, configurationAssemblyRepository.AllHashes().Count(), "The repository isn't empty, this test requries an empty repository.");

            // Write assembly to disk
            Directory.CreateDirectory(hashPath);
            File.WriteAllBytes(assemblyPath, assemblyData);

            // Load assembly multiple times
            Assembly firstAssembly = configurationAssemblyRepository.Load(hash);
            Assembly secondAssembly = configurationAssemblyRepository.Load(hash);

            // Check that loaded assembly macthes the expected
            Assert.AreEqual(firstAssembly, secondAssembly, "New instances of the assembly are being created on each load, the assembly should be cached per hash then the cached instance returned everytime there after.");
        }

        [TestMethod]
        public void GetUserControlRetrievesFromCacheIfExist()
        {
            string hash = "ABC";
            string repositoryPath = GetUniqueRepositoryPath();
            var repo = new RuntimeConfigurationAssemblyRepository(repositoryPath);
            var control = new UserControl();
            repo.UserControlCache.Add(hash, control);
            var retrievedcontrol = repo.GetUserControlForAssembly(hash);
            Assert.IsTrue(ReferenceEquals(control, retrievedcontrol));
        }

        [TestMethod]
        public void GetUserControlReturnsNullIfNotExist()
        {
            string hash = "ABC";
            string repositoryPath = GetUniqueRepositoryPath();
            var repo = new RuntimeConfigurationAssemblyRepository(repositoryPath);
            var control = new UserControl();
            repo.UserControlCache.Add(hash, control);
            var retrievedcontrol = repo.GetUserControlForAssembly("DEF");
            Assert.IsNull(retrievedcontrol);
        }

        #endregion

        #region Helper Methods

        string GetUniqueRepositoryPath()
        {
            return Path.Combine(_testContext.TestRunDirectory, Guid.NewGuid().ToString());
        }

        #endregion
    }
}
