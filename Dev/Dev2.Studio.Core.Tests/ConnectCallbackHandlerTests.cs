using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Webs.Callbacks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Dev2.Core.Tests
{

    // SN : 23-01-2013 : Missing Tests 
    // 
    // Saving with Invalid connection
    // Dev2 Set with invalid environment connection

    [TestClass]
    public class ConnectCallbackHandlerTests
    {
        const string ConnectionID = "1478649D-CF54-4D0D-8E26-CA9B81454B66";
        const string ConnectionCategory = "TestCategory";
        const string ConnectionName = "TestConnection";
        const string ConnectionAddress = "http://RSAKLFBRANDONPA:77/dsf";
        const int ConnectionWebServerPort = 1234;

        // Keys are case-sensitive!
        static readonly string ConnectionJson =
            "{\"ResourceID\":\"" + ConnectionID +
            "\",\"ResourceName\":\"" + ConnectionName +
            "\",\"ResourcePath\":\"TEST\",\"ResourceType\":\"Dev2Server\",\"Address\":\"" + ConnectionAddress +
            "\",\"AuthenticationType\":\"Windows\",\"UserName\":\"\",\"Password\":\"\",\"WebServerPort\":" + ConnectionWebServerPort + "}";

        static ImportServiceContext ImportContext;

        #region Class/TestInitialize

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            ImportContext = new ImportServiceContext();
            ImportService.CurrentContext = ImportContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });
            ImportService.AddExportedValueToContainer<IFrameworkSecurityContext>(new MockSecurityProvider(""));
            ImportService.AddExportedValueToContainer<IEventAggregator>(new EventAggregator());

        }

        [TestInitialize]
        public void TestInitialize()
        {
            ImportService.CurrentContext = ImportContext;
        }

        #endregion

        #region Save

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithNullParameters_Expected_ThrowsArgumentNullException()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var handler = new ConnectCallbackHandler();
            handler.Save(null, null, null, null, 0, null);
            handler.Save(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(UriFormatException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithInValidConnectionUri_Expected_ThrowsUriFormatException()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var handler = new ConnectCallbackHandler();
            handler.Save(ConnectionID, "xxx", "xxx", "xxx", 0, null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithInValidConnectionID_Expected_ThrowsFormatException()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var handler = new ConnectCallbackHandler();
            handler.Save("xxx", "xxx", "xxx", "xxx", 0, null);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithValidConnection_Expected_InvokesAddResourceService()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var dsfChannel = new Mock<IStudioClientContext>();
            dsfChannel.Setup(c => c.WorkspaceID).Returns(It.IsAny<Guid>());
            dsfChannel.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("").Verifiable();

            var targetEnv = new Mock<IEnvironmentModel>();
            targetEnv.Setup(e => e.DsfChannel).Returns(dsfChannel.Object);

            var rand = new Random();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            targetEnv.Setup(e => e.Connection).Returns(connection.Object);

            var repo = new TestEnvironmentRespository();
            var handler = new ConnectCallbackHandler(repo);
            handler.Save(ConnectionID, ConnectionCategory, ConnectionAddress, ConnectionName, ConnectionWebServerPort, targetEnv.Object);

            connection.Verify(c => c.ExecuteCommand(
                It.Is<string>(xml =>
                              xml.IndexOf("<Service>AddResourceService</Service>", StringComparison.InvariantCultureIgnoreCase) != -1
                              && xml.IndexOf("<ResourceXml>&lt;Source ID=\"" + ConnectionID + "\"", StringComparison.InvariantCultureIgnoreCase) != -1
                                       ),
                It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithValidConnection_Expected_InvokesSaveEnvironment()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var currentRepository = new Mock<IEnvironmentRepository>();
            currentRepository.Setup(e => e.Save(It.IsAny<IEnvironmentModel>())).Verifiable();
            currentRepository.Setup(e => e.Fetch(It.IsAny<IServer>())).Returns(new Mock<IEnvironmentModel>().Object);

            var handler = new ConnectCallbackHandler(currentRepository.Object);
            handler.Save(ConnectionID, ConnectionCategory, ConnectionAddress, ConnectionName, ConnectionWebServerPort, null);

            // ReSharper disable PossibleUnintendedReferenceComparison - expected to be the same instance
            currentRepository.Verify(r => r.Save(It.Is<IEnvironmentModel>(s => s == handler.Server.Environment)));
            // ReSharper restore PossibleUnintendedReferenceComparison
        }


        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithValidConnection_Expected_InvokesSaveEnvironmentWithCategory()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            // BUG: 8786 - TWR - 2013.02.20
            var currentRepository = new Mock<IEnvironmentRepository>();
            currentRepository.Setup(e => e.Save(It.IsAny<IEnvironmentModel>())).Verifiable();

            var env = new Mock<IEnvironmentModel>();
            env.SetupProperty(e => e.Category); // start tracking sets/gets to this property

            currentRepository.Setup(e => e.Fetch(It.IsAny<IServer>())).Returns(env.Object);

            var handler = new ConnectCallbackHandler(currentRepository.Object);
            handler.Save(ConnectionID, ConnectionCategory, ConnectionAddress, ConnectionName, ConnectionWebServerPort, null);

            // ReSharper disable PossibleUnintendedReferenceComparison - expected to be the same instance
            currentRepository.Verify(r => r.Save(It.Is<IEnvironmentModel>(s => s.Category == ConnectionCategory)));
            // ReSharper restore PossibleUnintendedReferenceComparison
        }


        #endregion

        #region Save

        [TestMethod]
        [ExpectedException(typeof(JsonReaderException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithInvalidJson_Expected_ThrowsJsonReaderException()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var handler = new ConnectCallbackHandler();
            handler.Save("xxxxx", null);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithValidJson_Expected_InvokesSave()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var currentRepository = new Mock<IEnvironmentRepository>();
            currentRepository.Setup(e => e.Save(It.IsAny<IEnvironmentModel>())).Verifiable();
            currentRepository.Setup(e => e.Fetch(It.IsAny<IServer>())).Returns(new Mock<IEnvironmentModel>().Object);

            var handler = new ConnectCallbackHandler(currentRepository.Object);
            handler.Save(ConnectionJson, null);

            // ReSharper disable PossibleUnintendedReferenceComparison - expected to be the same instance
            currentRepository.Verify(r => r.Save(It.Is<IEnvironmentModel>(s => s == handler.Server.Environment)));
            // ReSharper restore PossibleUnintendedReferenceComparison
        }

        #endregion

    }
}
