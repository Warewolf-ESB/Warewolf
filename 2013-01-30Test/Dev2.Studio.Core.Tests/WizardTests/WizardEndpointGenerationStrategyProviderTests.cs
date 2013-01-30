using System;
using Dev2.Composition;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Wizards;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.WizardTests
{
    [TestClass]
    public class WizardEndpointGenerationStrategyProviderTests
    {
        #region RegisterEndpointGenerationStrategies

        [TestMethod]
        public void RegisterEndpointGenerationStrategies_Where_ServiceLocatorIsPresent_Expected_Success()
        {
            ImportService.CurrentContext = CompositionInitializer.InitializeMockedMainViewModelStudioCore();
            
            Mock<IServiceLocator> mockServiceLocator = new Mock<IServiceLocator>();
            mockServiceLocator.Setup(s => s.RegisterEnpoint(It.IsAny<string>(), It.IsAny<Func<Tuple<IContextualResourceModel, DataListMergeOpsTO, Guid>, Uri>>())).Verifiable();
            mockServiceLocator.Setup(s => s.RegisterEnpoint(It.IsAny<string>(), It.IsAny<Func<Tuple<string, IEnvironmentModel, DataListMergeOpsTO, Guid>, Uri>>())).Verifiable();
            mockServiceLocator.Setup(s => s.RegisterEnpoint(It.IsAny<string>(), It.IsAny<Func<Tuple<IEnvironmentModel, DataListMergeOpsTO, Guid>, Uri>>())).Verifiable();
            mockServiceLocator.Setup(s => s.RegisterEnpoint(It.IsAny<string>(), It.IsAny<Func<Tuple<string, IEnvironmentModel, DataListMergeOpsTO, Guid>, Uri>>())).Verifiable();
            mockServiceLocator.Setup(s => s.RegisterEnpoint(It.IsAny<string>(), It.IsAny<Func<Tuple<string, IEnvironmentModel, Guid>, Uri>>())).Verifiable();

            WizardEndpointGenerationStrategyProvider WizardEndpointGenerationStrategyProvider = new WizardEndpointGenerationStrategyProvider();
            WizardEndpointGenerationStrategyProvider.RegisterEndpointGenerationStrategies(mockServiceLocator.Object);

            mockServiceLocator.Verify(s => s.RegisterEnpoint(It.IsAny<string>(), It.IsAny<Func<Tuple<IContextualResourceModel, DataListMergeOpsTO, Guid>, Uri>>()), Times.AtLeast(1));
            mockServiceLocator.Verify(s => s.RegisterEnpoint(It.IsAny<string>(), It.IsAny<Func<Tuple<string, IEnvironmentModel, DataListMergeOpsTO, Guid>, Uri>>()), Times.AtLeast(1));
            mockServiceLocator.Verify(s => s.RegisterEnpoint(It.IsAny<string>(), It.IsAny<Func<Tuple<IEnvironmentModel, DataListMergeOpsTO, Guid>, Uri>>()), Times.AtLeast(1));
            mockServiceLocator.Verify(s => s.RegisterEnpoint(It.IsAny<string>(), It.IsAny<Func<Tuple<string, IEnvironmentModel, DataListMergeOpsTO, Guid>, Uri>>()), Times.AtLeast(1));
            mockServiceLocator.Verify(s => s.RegisterEnpoint(It.IsAny<string>(), It.IsAny<Func<Tuple<string, IEnvironmentModel, Guid>, Uri>>()), Times.AtLeast(1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterEndpointGenerationStrategies_Where_ServiceLocatorIsNull_Expected_Exception()
        {
            ImportService.CurrentContext = CompositionInitializer.InitializeMockedMainViewModelStudioCore();

            WizardEndpointGenerationStrategyProvider WizardEndpointGenerationStrategyProvider = new WizardEndpointGenerationStrategyProvider();
            WizardEndpointGenerationStrategyProvider.RegisterEndpointGenerationStrategies(null);
        }

        #endregion RegisterEndpointGenerationStrategies

        #region Endpoint Generation Strategies

        [TestMethod]
        public void ResourceActivityWizardEndpointGenerationStrategy()
        {
            ImportService.CurrentContext = CompositionInitializer.InitializeMockedMainViewModelStudioCore();

            ServiceLocator serviceLocator = new ServiceLocator();
            ImportService.SatisfyImports(serviceLocator);

            DataListMergeOpsTO dataListMergeOpsTO = new DataListMergeOpsTO(new Guid("{12345678-9123-4567-8912-345678912345}"));
            Tuple<IContextualResourceModel, DataListMergeOpsTO, Guid> data = new Tuple<IContextualResourceModel, DataListMergeOpsTO, Guid>(Dev2MockFactory.SetupResourceModelMock().Object, dataListMergeOpsTO, new Guid("{12345678-1111-4567-8912-345678912345}"));

            Uri endpoint = serviceLocator.GetEndpoint(WizardEndpointGenerationStrategyProvider.ResourceActivityWizardServiceKey, data);

            string expected = "http://localhost:1234/services/Test.wiz?DatalistOutMergeDepth=Data_With_Blank_OverWrite&DatalistOutMergeFrequency=OnCompletion&DatalistOutMergeID=12345678-9123-4567-8912-345678912345&DatalistOutMergeType=Union&DatalistInMergeDepth=Data_With_Blank_OverWrite&DatalistInMergeID=12345678-9123-4567-8912-345678912345&DatalistInMergeType=Union&ExecutionCallbackID=12345678-1111-4567-8912-345678912345";
            string actual = endpoint.AbsoluteUri;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CodedActivityWizardEndpointGenerationStrategy()
        {
            ImportService.CurrentContext = CompositionInitializer.InitializeMockedMainViewModelStudioCore();

            ServiceLocator serviceLocator = new ServiceLocator();
            ImportService.SatisfyImports(serviceLocator);

            DataListMergeOpsTO dataListMergeOpsTO = new DataListMergeOpsTO(new Guid("{12345678-9123-4567-8912-345678912345}"));
            Tuple<string, IEnvironmentModel, DataListMergeOpsTO, Guid> data = new Tuple<string, IEnvironmentModel, DataListMergeOpsTO, Guid>("DateTimeTool", Dev2MockFactory.SetupEnvironmentModel().Object, dataListMergeOpsTO, new Guid("{12345678-1111-4567-8912-345678912345}"));

            Uri endpoint = serviceLocator.GetEndpoint(WizardEndpointGenerationStrategyProvider.CodedActivityWizardServiceKey, data);

            string expected = "http://localhost:1234/services/Dev2DateTimeToolWizard?DatalistOutMergeDepth=Data_With_Blank_OverWrite&DatalistOutMergeFrequency=OnCompletion&DatalistOutMergeID=12345678-9123-4567-8912-345678912345&DatalistOutMergeType=Union&DatalistInMergeDepth=Data_With_Blank_OverWrite&DatalistInMergeID=12345678-9123-4567-8912-345678912345&DatalistInMergeType=Union&ExecutionCallbackID=12345678-1111-4567-8912-345678912345";
            string actual = endpoint.AbsoluteUri;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ActivitySettingsWizardEndpointGenerationStrategy()
        {
            ImportService.CurrentContext = CompositionInitializer.InitializeMockedMainViewModelStudioCore();

            ServiceLocator serviceLocator = new ServiceLocator();
            ImportService.SatisfyImports(serviceLocator);

            DataListMergeOpsTO dataListMergeOpsTO = new DataListMergeOpsTO(new Guid("{12345678-9123-4567-8912-345678912345}"));
            Tuple<IEnvironmentModel, DataListMergeOpsTO, Guid> data = new Tuple<IEnvironmentModel, DataListMergeOpsTO, Guid>(Dev2MockFactory.SetupEnvironmentModel().Object, dataListMergeOpsTO, new Guid("{12345678-1111-4567-8912-345678912345}"));

            Uri endpoint = serviceLocator.GetEndpoint(WizardEndpointGenerationStrategyProvider.ActivitySettingsWizardServiceKey, data);

            string expected = "http://localhost:1234/services/Dev2ActivityGeneralSettingsWizard?DatalistOutMergeDepth=Data_With_Blank_OverWrite&DatalistOutMergeFrequency=OnCompletion&DatalistOutMergeID=12345678-9123-4567-8912-345678912345&DatalistOutMergeType=Union&DatalistInMergeDepth=Data_With_Blank_OverWrite&DatalistInMergeID=12345678-9123-4567-8912-345678912345&DatalistInMergeType=Union&ExecutionCallbackID=12345678-1111-4567-8912-345678912345";
            string actual = endpoint.AbsoluteUri;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ServiceWithDataListMergeAndExecutionCallBackEndpointGenerationStrategy()
        {
            ImportService.CurrentContext = CompositionInitializer.InitializeMockedMainViewModelStudioCore();

            ServiceLocator serviceLocator = new ServiceLocator();
            ImportService.SatisfyImports(serviceLocator);

            DataListMergeOpsTO dataListMergeOpsTO = new DataListMergeOpsTO(new Guid("{12345678-9123-4567-8912-345678912345}"));
            Tuple<string, IEnvironmentModel, DataListMergeOpsTO, Guid> data = new Tuple<string, IEnvironmentModel, DataListMergeOpsTO, Guid>("InsertServiceNameHere", Dev2MockFactory.SetupEnvironmentModel().Object, dataListMergeOpsTO, new Guid("{12345678-1111-4567-8912-345678912345}"));

            Uri endpoint = serviceLocator.GetEndpoint(WizardEndpointGenerationStrategyProvider.ServiceWithDataListMergeAndExecutionCallBackKey, data);

            string expected = "http://localhost:1234/services/InsertServiceNameHere?DatalistOutMergeDepth=Data_With_Blank_OverWrite&DatalistOutMergeFrequency=OnCompletion&DatalistOutMergeID=12345678-9123-4567-8912-345678912345&DatalistOutMergeType=Union&DatalistInMergeDepth=Data_With_Blank_OverWrite&DatalistInMergeID=12345678-9123-4567-8912-345678912345&DatalistInMergeType=Union&ExecutionCallbackID=12345678-1111-4567-8912-345678912345";
            string actual = endpoint.AbsoluteUri;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ServiceWithExecutionCallBackEndpointGenerationStrategy()
        {
            ImportService.CurrentContext = CompositionInitializer.InitializeMockedMainViewModelStudioCore();

            ServiceLocator serviceLocator = new ServiceLocator();
            ImportService.SatisfyImports(serviceLocator);

            Tuple<string, IEnvironmentModel, Guid> data = new Tuple<string, IEnvironmentModel, Guid>("InsertServiceNameHere", Dev2MockFactory.SetupEnvironmentModel().Object, new Guid("{12345678-1111-4567-8912-345678912345}"));

            Uri endpoint = serviceLocator.GetEndpoint(WizardEndpointGenerationStrategyProvider.ServiceWithExecutionCallBackKey, data);

            string expected = "http://localhost:1234/services/InsertServiceNameHere?ExecutionCallbackID=12345678-1111-4567-8912-345678912345";
            string actual = endpoint.AbsoluteUri;

            Assert.AreEqual(expected, actual);
        }

        #endregion Endpoint Generation Strategies
    }
}
