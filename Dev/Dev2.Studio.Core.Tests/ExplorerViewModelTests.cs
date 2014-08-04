using Dev2.AppResources.Repositories;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.CustomControls.Connections;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.Explorer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ExplorerViewModelTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsNullExceptionForEnvironmentRepo()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ExplorerViewModel(EventPublishers.Aggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, null, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, new Mock<IMainViewModel>().Object, connectControlViewModel: new Mock<IConnectControlViewModel>().Object);
            // ReSharper restore ObjectCreationAsStatement
        }

        private static IEnvironmentRepository GetEnvironmentRepository(Mock<IEnvironmentModel> mockEnvironment)
        {
            var repo = new TestLoadEnvironmentRespository(mockEnvironment.Object) { IsLoaded = true };
            return repo;
        }
    }
}
