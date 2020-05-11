using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace Warewolf.Studio.Models.Tests
{
    [TestClass]
    public class ExchangeServiceModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeServiceModel))]
        public void ExchangeServiceModel_Validate_RetrieveSources()
        {
            var guid = Guid.NewGuid();

            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.EnvironmentID).Returns(guid);

            var mockQueryManager = new Mock<IQueryManager>();
            var mockExchangeSource = new Mock<IExchangeSource>().Object;
            var exchangeSources = new List<IExchangeSource> { mockExchangeSource };
            mockQueryManager.Setup(query => query.FetchExchangeSources()).Returns(exchangeSources);

            mockServer.Setup(server => server.QueryProxy).Returns(mockQueryManager.Object);

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(shell => shell.SetActiveServer(guid));

            var exchangeServiceModel = new ExchangeServiceModel(mockServer.Object, mockShellViewModel.Object);

            mockShellViewModel.Verify(model => model.SetActiveServer(guid), Times.Once());

            var sources = exchangeServiceModel.RetrieveSources();
            Assert.AreEqual(mockExchangeSource, sources[0]);
            mockQueryManager.Verify(model => model.FetchExchangeSources(), Times.Once());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeServiceModel))]
        public void ExchangeServiceModel_Validate_CreateNewSource()
        {
            var guid = Guid.NewGuid();

            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.EnvironmentID).Returns(guid);

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(shell => shell.SetActiveServer(guid));
            mockShellViewModel.Setup(shell => shell.NewExchangeSource(string.Empty));

            var exchangeServiceModel = new ExchangeServiceModel(mockServer.Object, mockShellViewModel.Object);

            mockShellViewModel.Verify(model => model.SetActiveServer(guid), Times.Once());

            exchangeServiceModel.CreateNewSource();
            mockShellViewModel.Verify(model => model.NewExchangeSource(string.Empty), Times.Once());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeServiceModel))]
        public void ExchangeServiceModel_Validate_EditSource()
        {
            var guid = Guid.NewGuid();

            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.EnvironmentID).Returns(guid);

            var mockExchangeSource = new Mock<IExchangeSource>().Object;

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(shell => shell.SetActiveServer(guid));
            mockShellViewModel.Setup(shell => shell.EditResource(mockExchangeSource));

            var exchangeServiceModel = new ExchangeServiceModel(mockServer.Object, mockShellViewModel.Object);

            mockShellViewModel.Verify(model => model.SetActiveServer(guid), Times.Once());

            exchangeServiceModel.EditSource(mockExchangeSource);
            mockShellViewModel.Verify(model => model.EditResource(mockExchangeSource), Times.Once());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExchangeServiceModel))]
        public void ExchangeServiceModel_Validate_UpdateRepository()
        {
            var guid = Guid.NewGuid();

            var mockStudioUpdateManager = new Mock<IStudioUpdateManager>().Object;

            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.EnvironmentID).Returns(guid);
            mockServer.Setup(server => server.UpdateRepository).Returns(mockStudioUpdateManager);

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(shell => shell.SetActiveServer(guid));

            var exchangeServiceModel = new ExchangeServiceModel(mockServer.Object, mockShellViewModel.Object);

            mockShellViewModel.Verify(model => model.SetActiveServer(guid), Times.Once());

            Assert.AreEqual(mockStudioUpdateManager, exchangeServiceModel.UpdateRepository);
        }
    }
}
