using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Custom_Dev2_Controls.Connections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;

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
            handler.Save(null, null, null, 0, null);
        }

        [TestMethod]
        [ExpectedException(typeof(UriFormatException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithInValidConnectionUri_Expected_ThrowsUriFormatException()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var handler = new ConnectCallbackHandler();
            handler.Save(ConnectionID, "xxx", "xxx", 0, null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithInValidConnectionID_Expected_ThrowsFormatException()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var handler = new ConnectCallbackHandler();
            handler.Save("xxx", "xxx", "xxx", 0, null);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithValidConnection_Expected_InvokesAddResourceService()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var dsfChannel = new Mock<IStudioClientContext>();
            dsfChannel.Setup(c => c.AccountID).Returns(It.IsAny<Guid>());
            dsfChannel.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();

            var targetEnv = new Mock<IEnvironmentModel>();
            targetEnv.Setup(e => e.DsfChannel).Returns(dsfChannel.Object);

            var handler = new ConnectCallbackHandler();
            handler.Save(ConnectionID, ConnectionAddress, ConnectionName, ConnectionWebServerPort, targetEnv.Object);

            dsfChannel.Verify(c => c.ExecuteCommand(
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
            var currentRepository = new Mock<IFrameworkRepository<IEnvironmentModel>>();
            currentRepository.Setup(e => e.Save(It.IsAny<IEnvironmentModel>())).Verifiable();

            var handler = new ConnectCallbackHandler { CurrentEnvironmentRepository = currentRepository.Object };
            handler.Save(ConnectionID, ConnectionAddress, ConnectionName, ConnectionWebServerPort, null);

            // ReSharper disable PossibleUnintendedReferenceComparison - expected to be the same instance
            currentRepository.Verify(r => r.Save(It.Is<IEnvironmentModel>(s => s == handler.Server.Environment)));
            // ReSharper restore PossibleUnintendedReferenceComparison
        }

        #endregion

        #region Dev2SetValue

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Dev2SetValue_WithNullParameters_Expected_ThrowsArgumentNullException()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var handler = new ConnectCallbackHandler();
            handler.Dev2SetValue(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(JsonReaderException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Dev2SetValue_WithInvalidJson_Expected_ThrowsJsonReaderException()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var handler = new ConnectCallbackHandler();
            handler.Dev2SetValue("xxxxx", null);
        }


        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Dev2SetValue_WithValidJson_Expected_InvokesSave()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var currentRepository = new Mock<IFrameworkRepository<IEnvironmentModel>>();
            currentRepository.Setup(e => e.Save(It.IsAny<IEnvironmentModel>())).Verifiable();

            var handler = new ConnectCallbackHandler { CurrentEnvironmentRepository = currentRepository.Object };
            handler.Dev2SetValue(ConnectionJson, null);

            // ReSharper disable PossibleUnintendedReferenceComparison - expected to be the same instance
            currentRepository.Verify(r => r.Save(It.Is<IEnvironmentModel>(s => s == handler.Server.Environment)));
            // ReSharper restore PossibleUnintendedReferenceComparison
        }

        #endregion

    }
}
