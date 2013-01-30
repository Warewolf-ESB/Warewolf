using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Studio.Core.Interfaces;
using Moq;
using Dev2.Studio.ViewModels.Explorer;
using Dev2.Composition;

namespace Dev2.Core.Tests
{
    /// <summary>
    /// Summary description for ConnectViewModelTest
    /// </summary>
    [TestClass]
    public class ConnectViewModelTest
    {
        private Mock<IEnvironmentModel> _mockEnvironmentModel;
        private Mock<IMainViewModel> _mockMainViewModel; 
        private Mock<IFilePersistenceProvider> _mockFilePersistance;
        private Mock<IResourceRepository> _mockResourceModel;
        private Mock<IContextualResourceModel> _mockResource;        
        private ConnectViewModel connectViewmodel;

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional result attributes
        // Use TestInitialize to run code before running each result 
        [TestInitialize()]
        public void MyTestInitialize()
        {
          _mockResourceModel = Dev2MockFactory.SetupFrameworkRepositoryResourceModelMock();
          _mockEnvironmentModel = new Mock<IEnvironmentModel>();
          _mockMainViewModel = Dev2MockFactory.SetupMainViewModel();
          _mockFilePersistance = new Mock<IFilePersistenceProvider>();
          _mockResource = Dev2MockFactory.SetupResourceModelMock();
          connectViewmodel = new ConnectViewModel();
        }

        #endregion

        [TestMethod]
        public void ConnectCommand_ExpectSuccess()
        {

            connectViewmodel.WebServerPort = "1234";
            connectViewmodel.Name = "TestServer";
            connectViewmodel.DsfAddress = "http://127.0.0.1:77/dsf";

            connectViewmodel.OkayCommand.Execute("abc");

            Assert.AreEqual("http://127.0.0.1:77/dsf", connectViewmodel.Server.AppAddress);
            
        }

        [TestMethod()]
        public void ConnectCommand_ExpectFailure()
        {
                connectViewmodel.OkayCommand.Execute("abc");
                Assert.AreEqual(null, connectViewmodel.Server.AppAddress);
        }

        [TestMethod()]
        public void CancelCommand()
        {
            connectViewmodel.CancelCommand.Execute("abc");

            Assert.AreEqual(0, connectViewmodel.Server.Servers.Count);
        }

        [TestMethod]
        public void ConnectCommandExpectLocalhostResolveToSpecificLocalIP()
        {
            connectViewmodel.WebServerPort = "1234";
            connectViewmodel.Name = "TestServer";
            connectViewmodel.DsfAddress = "http://LoCaLhoSt:77/dsf";

            connectViewmodel.OkayCommand.Execute("abc");

            Assert.AreEqual("http://127.0.0.1:77/dsf", connectViewmodel.Server.AppAddress);
        }
    }
}
