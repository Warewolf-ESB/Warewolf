using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    /// <summary>
    /// Summary description for EnvironmentRepositoryTest
    /// </summary>
    [TestClass]
    public class EnviromentRepositoryTest
    {
        // Needed for DefaultEnvironment initialization!!!
        static ImportServiceContext _importServiceContext;

        readonly Object _lock = new object();
        private static readonly object TestGuard = new object();


        #region MyClass/TestInitialize

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);

            var eventAggregator = new Mock<IEventAggregator>();

            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>());
            ImportService.AddExportedValueToContainer(securityContext.Object);
            ImportService.AddExportedValueToContainer(eventAggregator.Object);

            _importServiceContext = importServiceContext;
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Monitor.Enter(TestGuard);
            ImportService.CurrentContext = _importServiceContext;
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            Monitor.Exit(TestGuard);
        }


        #endregion

        #region Connect Tests

        [TestMethod()]
        public void Connect_EnvironmentAvailable_EnvironmentAddedToEnvironmentRepositoryAsAvailable()
        {
            lock(_lock)
            {
                var repos = new EnvironmentRepository();
                var env = CreateMockEnvironment();

                //Arrange
                repos.Save(env.Object);

                //Act
                var returnedEnv = repos.All();
                var ret = returnedEnv.FirstOrDefault(c => c.Name == env.Object.Name);
                //Assert
                Assert.IsNotNull(ret);
            }
        }

        /// <summary>
        /// Unit result that connects to a unavailible Environment
        /// </summary>
        //5559 check test
        //[TestMethod()]
        public void Connect_UnavailibleEnviroment_ExpectedEnvironmentAddedIsConnectedSetToFalse()
        {
            lock(_lock)
            {
                var repos = new EnvironmentRepository();
                var env = CreateMockEnvironment();

                //Arrange
                env.Setup(prop => prop.IsConnected).Returns(false);
                repos.Save(env.Object);
                //Act
                ICollection<IEnvironmentModel> returnedEnv = repos.All();
                var ret = returnedEnv.First(c => c.Name == env.Object.Name);
                //Assert
                Assert.IsFalse(ret.IsConnected);
            }
        }

        #endregion

        #region Save To and Load From file Tests

        //5559 check test
        ///// <summary>
        ///// Unit result that saves a environment to a file.
        ///// </summary>
        [TestMethod()]
        public void Save_EnvironmentModelToFile_ExpectedEnvironmentRespositoryUpdatedWithSavedEnvironment()
        {
            lock(_lock)
            {
                var repos = new EnvironmentRepository();
                
                Mock<IEnvironmentModel> envModel = new Mock<IEnvironmentModel>();
                envModel.Setup(c => c.Name).Returns("result");

                repos.Save(envModel.Object);

                Assert.IsTrue(repos.All().Count == 1);
            }

        }

        //5559 check test
        [TestMethod]
        public void Save_EnvironmentModelToFileExistingEnvironment_ExpectedEnvironmentAddedtoEnvironmentRepository()
        {
            lock(_lock)
            {
                var repos = new EnvironmentRepository();
               
                Mock<IEnvironmentModel> envModel = Dev2MockFactory.SetupEnvironmentModel();
                envModel.Setup(model => model.Name).Returns("Test");
                //Save the First Environment Model to the Repo
                repos.Save(envModel.Object);

                //Save the Environment Again
                repos.Save(envModel.Object);

                Assert.IsInstanceOfType(repos.FindSingle(repo => repo.Name == "Test"), typeof(IEnvironmentModel));
            }
        }

        //5559 check test
        [TestMethod]
        public void Save_EnviromentModelToExistingListOfEnvironments_ExpectedEnvrionmentListUpdatedOnTheRepository()
        {
            lock(_lock)
            {
                Mock<IEnvironmentModel> envModel = Dev2MockFactory.SetupEnvironmentModel();
                Mock<IEnvironmentModel> secondEntryEnvModel = Dev2MockFactory.SetupEnvironmentModel();

                envModel.Setup(model => model.Name).Returns("Test");
                secondEntryEnvModel.Setup(model => model.Name).Returns("SecondEnvironmentModel");

                var repos = new EnvironmentRepository();
                
                //Save the First Environment Model to the Repo
                repos.Save(envModel.Object);

                //Save the Environment Again
                repos.Save(secondEntryEnvModel.Object);

                Assert.AreEqual(2, repos.All().Count);
            }
        }

        #endregion

        #region Remove Tests

        //5559 check test
        //<summary>
        //Unit result that Removes an environment
        //</summary>
        [TestMethod()]
        public void RemoveEnvironmentFromSavedEnvironments()
        {
            ////Act
            //ICollection<IEnvironmentModel> returnedEnv = repos.All();
            //repos.Save(_environmentmodel.Object);
            //repos.Remove(_environmentmodel.Object);
            //int envs = repos.All().Count(c => c.Name == test);
            ////Assert
            //Assert.IsTrue(envs == 0);  
            Assert.Inconclusive();
        }

        //5559 check test
        //[TestMethod]
        public void RemoveEnvironmentEnvironmentDoesNotExist_Expected_NoEnvironmentsRemovedFromRepository()
        {
            lock(_lock)
            {
                var repos = new EnvironmentRepository();
                var env = CreateMockEnvironment();

                //Act
                ICollection<IEnvironmentModel> returnedEnv = repos.All();
                repos.Save(env.Object);

                Mock<IEnvironmentModel> secondEnvironmentModel = Dev2MockFactory.SetupEnvironmentModel();
                secondEnvironmentModel.Setup(e => e.Name).Returns("NonExistantEnvironment");

                repos.Remove(secondEnvironmentModel.Object);
                int envs = repos.All().Count;

                //Assert
                Assert.IsTrue(envs == 1);
            }
        }

        #endregion Remove Tests

        #region Location Tests

        [TestMethod]
        public void EnvironmentRepositoryUsesTheCurrentUsersAppDataFolder()
        {
            lock(_lock)
            {
                string expected = Path.Combine(new string[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    StringResources.App_Data_Directory,
                    StringResources.Environments_Directory
                });

                string actual = EnvironmentRepository.GetEnvironmentsDirectory();

                // Test

                Assert.AreEqual(expected, actual);
            }
        }

        #endregion Location Tests


        public IFilePersistenceProvider FilePersistenceProvider { get; set; }

        #region LookupEnvironments

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LookupEnvironments_With_NullParameters_Expected_ThrowsArgumentNullException()
        {
            var result = EnvironmentRepository.LookupEnvironments(null);
        }

        [TestMethod]
        public void LookupEnvironments_With_NoEnvironmentIDs_Expected_ReturnsListOfServers()
        {
            lock(_lock)
            {
                var env = CreateMockEnvironment(Server1Source);
                var result = EnvironmentRepository.LookupEnvironments(env.Object);

                Assert.AreEqual(1, result.Count);
            }
        }

        [TestMethod]
        public void LookupEnvironments_With_InvalidParameters_Expected_ReturnsEmptyList()
        {
            lock(_lock)
            {
                var env = CreateMockEnvironment(Server1Source);
                var result = EnvironmentRepository.LookupEnvironments(env.Object, new List<string> { "xxx", "aaa" });
                // Test
                Assert.AreEqual(0, result.Count);
            }
        }

        [TestMethod]
        public void LookupEnvironments_With_InvalidEnvironmentID_Expected_ReturnsEmptyList()
        {
            lock(_lock)
            {
                var env = CreateMockEnvironment(
                    "<Source ID=\"{5E8EB586-1D63-4C9F-9A35-CD05ACC2B6}\" ConnectionString=\"AppServerUri=//127.0.0.1:77/dsf;WebServerPort=1234\" Name=\"TheName\" Type=\"Dev2Server\"><DisplayName>The Name</DisplayName></Source>");

                var result = EnvironmentRepository.LookupEnvironments(env.Object, new List<string> { Server1ID, "{5E8EB586-1D63-4C9F-9A35-CD05ACC2B607}" });
                Assert.AreEqual(0, result.Count);
            }
        }

        [TestMethod]
        public void LookupEnvironments_With_InvalidEnvironmentAppServerUri_Expected_ReturnsEmptyList()
        {
            lock(_lock)
            {
                var env = CreateMockEnvironment(
                    "<Source ID=\"{5E8EB586-1D63-4C9F-9A35-CD05ACC2B607}\" ConnectionString=\"AppServerUri=//127.0.0.1:77/dsf;WebServerPort=1234\" Name=\"TheName\" Type=\"Dev2Server\"><DisplayName>The Name</DisplayName></Source>");

                var result = EnvironmentRepository.LookupEnvironments(env.Object, new List<string> { Server1ID, "{5E8EB586-1D63-4C9F-9A35-CD05ACC2B607}" });
                Assert.AreEqual(0, result.Count);
            }
        }

        [TestMethod]
        public void LookupEnvironments_With_InvalidEnvironmentWebServerPort_Expected_ReturnsEmptyList()
        {
            lock(_lock)
            {
                var env = CreateMockEnvironment(
                    "<Source ID=\"{5E8EB586-1D63-4C9F-9A35-CD05ACC2B607}\" ConnectionString=\"AppServerUri=http://127.0.0.1:77/dsf;WebServerPort=12a34\" Name=\"TheName\" Type=\"Dev2Server\"><DisplayName>The Name</DisplayName></Source>");

                var result = EnvironmentRepository.LookupEnvironments(env.Object, new List<string> { Server1ID, "{5E8EB586-1D63-4C9F-9A35-CD05ACC2B607}" });
                Assert.AreEqual(0, result.Count);
            }
        }

        [TestMethod]
        public void LookupEnvironments_With_OneValidEnvironmentID_Expected_ReturnsOneEnvironment()
        {
            lock(_lock)
            {
                var env = CreateMockEnvironment(Server1Source);
                var result = EnvironmentRepository.LookupEnvironments(env.Object, new List<string> { Server1ID });
                Assert.AreEqual(1, result.Count);
            }
        }

        #endregion

        #region ReadFile

        [TestMethod]
        public void ReadFile_With_NonExistingFile_Expected_ReturnsEmptyList()
        {
            lock(_lock)
            {
                var path = EnvironmentRepository.GetEnvironmentsFilePath();
                var bakPath = BackupFile(path);

                var rep = new EnvironmentRepository();
                var result = rep.ReadFile();

                RestoreFile(path, bakPath);

                Assert.AreEqual(0, result.Count);
            }
        }

        [TestMethod]
        public void ReadFile_With_OneEnvironment_Expected_ReturnsOneEnvironment()
        {

            lock(_lock)
            {
                var path = EnvironmentRepository.GetEnvironmentsFilePath();

                string backPath;

                backPath = RetryUtility.RetryMethod<string>(() => BackupFile(path), 15, 1000, null);

                var xml = new XElement("Environments", new XElement("Environment", "{A9185F8F-3F57-45A6-B3AB-9E506D1BA1DF}"));
                xml.Save(path);

                var rep = new EnvironmentRepository();
                var result = rep.ReadFile();

                RetryUtility.RetryAction(() => DeleteFile(path), 15, 1000);
                RetryUtility.RetryAction(() => RestoreFile(path, backPath), 15, 1000);

                Assert.AreEqual(1, result.Count);
            }
            //Assert.Inconclusive();
        }

        #endregion

        #region WriteFile

        [TestMethod]
        public void WriteFile_With_NonExistingFile_Expected_CreatesFile()
        {
            //lock(_lock)
            //{
            //    var path = EnvironmentRepository.GetEnvironmentsFilePath();
            //    var bakPath = BackupFile(path);

            //    var rep = new EnvironmentRepository();
            //    rep.WriteFile();
            //    var exists = File.Exists(path);

            //    DeleteFile(path);

            //    RestoreFile(path, bakPath);

            //    Assert.AreEqual(true, exists);
            //}
            Assert.Inconclusive();
        }

        [TestMethod]
        public void WriteFile_With_ExistingFile_Expected_UpdatesFile()
        {
            //lock(_lock)
            //{
            //    var path = EnvironmentRepository.GetEnvironmentsFilePath();
            //    var bakPath = BackupFile(path);

            //    // Create empty file
            //    var rep = new EnvironmentRepository();
            //    rep.WriteFile();

            //    var xml = XElement.Load(path);
            //    var expected = xml.Descendants("Environment").Count() + 1;

            //    // Create file with one entry
            //    rep = new EnvironmentRepository(new[]
            //    {
            //        new EnvironmentModel { EnvironmentConnection = new EnvironmentConnection(), ID = Guid.NewGuid(), Connection.AppServerUri = new Uri("http://127.0.0.1:77/dsf"), Name = "Test1", WebServerPort = 1234 }
            //    });
            //    rep.WriteFile();

            //    xml = XElement.Load(path);
            //    var actual = xml.Descendants("Environment").Count();

            //    DeleteFile(path);
            //    RestoreFile(path, bakPath);

            //    Assert.AreEqual(expected, actual);
            //}
            Assert.Inconclusive();

        }

        #endregion

        #region Backup/Restore File
        string BackupFile(string path)
        {
            // Wait until it is safe to enter.
            var bakPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path) + ".bak");
            if(File.Exists(bakPath))
            {
                File.Delete(bakPath);
            }
            if(File.Exists(path))
            {
                File.Move(path, bakPath);
            }
            return bakPath;
        }

        void RestoreFile(string path, string bakPath)
        {
            if(File.Exists(path))
            {
                File.Delete(path);
            }
            if(File.Exists(bakPath))
            {
                File.Move(bakPath, path);
            }
            // Release the Mutex.
        }

        void DeleteFile(string path)
        {

            File.Delete(path);
        }

        #endregion

        #region Static CreateMockEnvironment

        public static readonly string Server1Source = "<Source ID=\"{70238921-FDC7-4F7A-9651-3104EEDA1211}\" Name=\"MyDevServer\" Type=\"Dev2Server\" ConnectionString=\"AppServerUri=http://127.0.0.1:77/dsf;WebServerPort=1234\" ServerID=\"d53bbcc5-4794-4dfa-b096-3aa815692e66\"><TypeOf>Dev2Server</TypeOf><DisplayName>My Dev Server</DisplayName></Source>";
        public static readonly string Server1ID = "{70238921-FDC7-4F7A-9651-3104EEDA1211}";

        public static Mock<IEnvironmentModel> CreateMockEnvironment(params string[] sources)
        {
            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);

            var eventAggregator = new Mock<IEventAggregator>();

            var rand = new Random();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.EventAggregator).Returns(eventAggregator.Object);
            connection.Setup(c => c.SecurityContext).Returns(securityContext.Object);
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", sources)));

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Connection).Returns(connection.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ID).Returns(Guid.NewGuid());
            env.Setup(e => e.Name).Returns(string.Format("Server_{0}", rand.Next(1, 100)));

            return env;
        }

        #endregion

    }



}
