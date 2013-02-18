using System.Threading;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dev2.Core.Tests
{
    /// <summary>
    /// Summary description for EnvironmentRepositoryTest
    /// </summary>
    [TestClass]
    public class EnviromentRepositoryTest
    {
        #region Variables

        /// <summary>
        /// Created variables used in tests globally 
        /// </summary>
        readonly Mock<IEnvironmentModel> _environmentmodel = new Mock<IEnvironmentModel>();
        Mock<IEnvironmentConnection> _environmentConnection = new Mock<IEnvironmentConnection>();
        IFrameworkRepository<IEnvironmentModel> _repos;
        readonly Object _lock = new object();
        const string Test = "result";

        static ImportServiceContext ImportServiceContext;

        private static readonly object TestGuard = new object();

        #endregion Variables

        #region Properties

        public IEnvironmentModel EnvironmentModel { get; set; }

        #endregion Properties

        #region Constructor and TestContext

        TestContext testContextInstance;

        // <summary>
        //Gets or sets the result context which provides
        //information about and functionality for the current result run.
        //</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #endregion Constructor and TestContext

        #region Additional result attributes

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            ImportServiceContext = CompositionInitializer.InitializeMockedMainViewModel();
        }

        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()] 
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each result 
        [TestInitialize()]
        public void EnvironmentRepositoryTestsInitialize()
        {
            Monitor.Enter(TestGuard);

            lock(_lock)
            {
                ImportService.CurrentContext = ImportServiceContext;

                _repos = new EnvironmentRepository();

                //Clear out any exiting environments
                foreach (var item in _repos.All().ToList())
                {
                    _repos.Remove(item);
                }

                _environmentmodel.Setup(prop => prop.WebServerAddress).Returns(new Uri("http://localhost:77/dsf"));
                _environmentmodel.Setup(prop => prop.Name).Returns(Test);
                _environmentmodel.Setup(prop => prop.WebServerPort).Returns(1234);
                _environmentmodel.Setup(prop => prop.Connect());

                EnvironmentModel = _environmentmodel.Object;
            }
        }


        [TestCleanup]
        public void MyTestCleanup()
        {
            Monitor.Exit(TestGuard);
        }
        

        #endregion

        #region Connect Tests

        //5559 check test
        /// <summary>
        /// Unit result that connects to a availible environment
        /// </summary>
        [TestMethod()]
        public void Connect_EnvironmentAvailable_EnvironmentAddedToEnvironmentRepositoryAsAvailable()
        {
            lock(_lock)
            {
                //Arrange
                _repos.Save(_environmentmodel.Object);
                //Act
                ICollection<IEnvironmentModel> returnedEnv = _repos.All();
                var ret = returnedEnv.First(c => c.Name == Test);
                //Assert
                Assert.AreEqual(_environmentmodel.Object.Name, ret.Name);
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
                //Arrange
                _environmentmodel.Setup(prop => prop.IsConnected).Returns(false);
                _repos.Save(_environmentmodel.Object);
                //Act
                ICollection<IEnvironmentModel> returnedEnv = _repos.All();
                var ret = returnedEnv.First(c => c.Name == Test);
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
                Mock<IEnvironmentModel> envModel = new Mock<IEnvironmentModel>();
                envModel.Setup(c => c.Name).Returns("result");

                _repos.Save(envModel.Object);

                Assert.IsTrue(_repos.All().Count == 1);
            }

        }

        //5559 check test
        [TestMethod]
        public void Save_EnvironmentModelToFileExistingEnvironment_ExpectedEnvironmentAddedtoEnvironmentRepository()
        {
            lock(_lock)
            {
                Mock<IEnvironmentModel> envModel = Dev2MockFactory.SetupEnvironmentModel();
                envModel.Setup(model => model.Name).Returns("Test");
                //Save the First Environment Model to the Repo
                _repos.Save(envModel.Object);

                //Save the Environment Again
                _repos.Save(envModel.Object);

                Assert.IsInstanceOfType(_repos.FindSingle(repo => repo.Name == "Test"), typeof(IEnvironmentModel));
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

                //Save the First Environment Model to the Repo
                _repos.Save(envModel.Object);

                //Save the Environment Again
                _repos.Save(secondEntryEnvModel.Object);

                Assert.AreEqual(2, _repos.All().Count);
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
                //Act
                ICollection<IEnvironmentModel> returnedEnv = _repos.All();
                _repos.Save(_environmentmodel.Object);

                Mock<IEnvironmentModel> secondEnvironmentModel = Dev2MockFactory.SetupEnvironmentModel();
                secondEnvironmentModel.Setup(e => e.Name).Returns("NonExistantEnvironment");

                _repos.Remove(secondEnvironmentModel.Object);
                int envs = _repos.All().Count;

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

            lock (_lock)
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
            //        new EnvironmentModel { EnvironmentConnection = new EnvironmentConnection(), ID = Guid.NewGuid(), DsfAddress = new Uri("http://127.0.0.1:77/dsf"), Name = "Test1", WebServerPort = 1234 }
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

        public Mock<IEnvironmentModel> CreateMockEnvironment(params string[] sources)
        {

            lock(_lock)
            {
                var dsfChannel = new Mock<IStudioClientContext>();
                dsfChannel.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", sources)));
                dsfChannel.Setup(c => c.AccountID).Returns(It.IsAny<Guid>());

                var rand = new Random();
                var securityContext = new Mock<IFrameworkSecurityContext>();
                securityContext.Setup(s => s.Roles).Returns(new string[0]);

                var env = new Mock<IEnvironmentModel>();
                env.Setup(e => e.EnvironmentConnection).Returns(new EnvironmentConnection { SecurityContext = securityContext.Object });
                env.Setup(e => e.DsfChannel).Returns(dsfChannel.Object);
                env.Setup(e => e.ID).Returns(Guid.NewGuid());
                env.Setup(e => e.Name).Returns(string.Format("Server_{0}", rand.Next(1, 100)));
                env.Setup(e => e.DsfAddress).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
                env.Setup(e => e.WebServerAddress).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
                env.Setup(e => e.WebServerPort).Returns(rand.Next(1, 100));

                return env;
            }
        }

        #endregion

        #region CreateTestEnvironmentWithSecurityContext

        public EnvironmentModel CreateTestEnvironmentWithSecurityContext()
        {
            lock(_lock)
            {
                var securityContext = new Mock<IFrameworkSecurityContext>();
                securityContext.Setup(s => s.Roles).Returns(new string[0]);

                var eventAggregator = new Mock<IEventAggregator>();

                IEnvironmentConnection environmentConnection = ImportService.GetExportValue<IEnvironmentConnection>();

                return new EnvironmentModel(eventAggregator.Object, securityContext.Object, environmentConnection)
                {
                    EnvironmentConnection = new EnvironmentConnection { SecurityContext = securityContext.Object },
                    ID = Guid.NewGuid(),
                    DsfAddress = new Uri("http://127.0.0.1:77/dsf"),
                    Name = "Test1",
                    WebServerPort = 1234
                };
            }
        }

        #endregion
    }



}
