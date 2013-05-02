using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.ViewModels.Explorer;
using Moq;

namespace Dev2.Core.Tests
{
    /// <summary>
    /// Summary description for ConnectViewModelTest
    /// </summary>
    [TestClass]
    public class ConnectViewModelTest
    {
        static ImportServiceContext _importServiceContext;
        ConnectViewModel _connectViewmodel;

        #region MyTestInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
            _importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = _importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>());

            var mockEventAggregator = new Mock<IEventAggregator>();
            ImportService.AddExportedValueToContainer(mockEventAggregator.Object);

            var mockSecurityContext = new Mock<IFrameworkSecurityContext>();
            ImportService.AddExportedValueToContainer(mockSecurityContext.Object);

            var wizardEngine = new Mock<IWizardEngine>();
            ImportService.AddExportedValueToContainer(wizardEngine.Object);
            }

        [TestInitialize]
        public void MyTestInitialize()
        {
            ImportService.CurrentContext = _importServiceContext;

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("xxxx");

            var targetEnvironment = new Mock<IEnvironmentModel>();
            targetEnvironment.Setup(e => e.Connection).Returns(connection.Object);

            _connectViewmodel = new ConnectViewModel(new TestEnvironmentRespository(targetEnvironment.Object, new[] { targetEnvironment.Object }), targetEnvironment.Object);
        }

        #endregion

        [TestMethod]
        public void ConnectOkayCommandWithValidIPAddressExpectedSuccess()
        {
            _connectViewmodel.WebServerPort = "1234";
            _connectViewmodel.Name = "TestServer";
            _connectViewmodel.DsfAddress = "http://127.0.0.1:77/dsf";

            _connectViewmodel.OkayCommand.Execute("abc");
            
            Assert.AreEqual("http://127.0.0.1:77/dsf", _connectViewmodel.Server.AppAddress);
        }

        [TestMethod]
        public void ConnectOkayCommandWithValidDnsAddressExpectedResolvesSuccessfully()
        {
            _connectViewmodel.WebServerPort = "1234";
            _connectViewmodel.Name = "TestServer";
            _connectViewmodel.DsfAddress = "http://LoCaLhoSt:77/dsf";

            _connectViewmodel.OkayCommand.Execute("abc");

            Assert.AreEqual("http://127.0.0.1:77/dsf", _connectViewmodel.Server.AppAddress);
        }

        [TestMethod]
        public void ConnectOkayCommandWithInValidAddressExpectedFailure()
        {
            _connectViewmodel.OkayCommand.Execute("abc");
            Assert.AreEqual(null, _connectViewmodel.Server.AppAddress);
        }

        [TestMethod]
        public void ConnectCancelCommandWithInValidAddressExpectedDoesNotAdd()
        {
            _connectViewmodel.CancelCommand.Execute("abc");

            Assert.AreEqual(0, _connectViewmodel.Server.Servers.Count);
        }
    }
}
