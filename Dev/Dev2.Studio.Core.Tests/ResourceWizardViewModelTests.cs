using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Composition;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ResourceWizardViewModelTests
    {
        #region Variables


        #endregion Variables

        #region Additional result attributes

        #endregion Additional result attributes

        #region Dev2Done Tests

        [TestMethod]
        [Ignore]
        public void Dev2Done_Where_ResourceModelExists_Expected_ResourceModelUpdated()
        {
            var mockEventAggregator = Dev2MockFactory.SetupMockEventAggregator();
            ImportService.CurrentContext =
                CompositionInitializer.InitializeWithMockEventAggregator(mockEventAggregator);

            Mock<IContextualResourceModel> _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            ResourceWizardViewModel resourceWizardViewModel = new ResourceWizardViewModel(_mockResourceModel.Object);

            resourceWizardViewModel.Dev2SetValue(@"<Dev2WizardPayload>
  <ResourceName>123</ResourceName>
  <ResourceType>WorkflowService</ResourceType>
  <Category></Category>
  <Comment></Comment>
  <Tags></Tags>
  <IconPath></IconPath>
  <HelpLink></HelpLink>
</Dev2WizardPayload>");
            resourceWizardViewModel.Dev2Done();
            _mockResourceModel.VerifySet(c => c.ResourceName = "123");
            //_mockResourceModel.VerifySet(c => c.ResourceType = enResourceType.WorkflowService);
            _mockResourceModel.VerifySet(c => c.Category = "");
            _mockResourceModel.VerifySet(c => c.Comment = "");
            _mockResourceModel.VerifySet(c => c.Tags = "");
            //_mockResourceModel.VerifySet(c => c.IconPath = "");
            _mockResourceModel.VerifySet(c => c.HelpLink = "");

        }

        [TestMethod]
        [Ignore]
        public void TestDev2Done_Where_ResourceModelIsNew_Expected_ResourceModelAdded()
        {
            var mockEventAggregator = Dev2MockFactory.SetupMockEventAggregator();
            ImportService.CurrentContext =
                CompositionInitializer.InitializeWithMockEventAggregator(mockEventAggregator);

            List<IResourceModel> resources = new List<IResourceModel>();

            Mock<IContextualResourceModel> _mockResourceModel = Dev2MockFactory.SetupResourceModelMock(resources);
            ResourceWizardViewModel resourceWizardViewModel = new ResourceWizardViewModel(_mockResourceModel.Object);

            resourceWizardViewModel.Dev2SetValue(@"<Dev2WizardPayload>
  <ResourceName>123</ResourceName>
  <ResourceType>WorkflowService</ResourceType>
  <Category></Category>
  <Comment></Comment>
  <Tags></Tags>
  <IconPath></IconPath>
  <HelpLink></HelpLink>
</Dev2WizardPayload>");
            resourceWizardViewModel.Dev2Done();
            CollectionAssert.Contains(resources, _mockResourceModel.Object);
        }

        [TestMethod]
        [Ignore]
        public void TestDev2Done_Where_ResourceModelSavedByWizard_Expected_UpdateExplorerMediatorMessageSent()
        {
            var mockEventAggregator = Dev2MockFactory.SetupMockEventAggregator();

            ImportService.CurrentContext =
                CompositionInitializer.InitializeWithMockEventAggregator(mockEventAggregator);

            Mock<IContextualResourceModel> _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            _mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.Source);
            ResourceWizardViewModel resourceWizardViewModel = new ResourceWizardViewModel(_mockResourceModel.Object);

            resourceWizardViewModel.Dev2SetValue(@"<Dev2WizardPayload>
  <ResourceName>123</ResourceName>
  <ResourceType>Source</ResourceType>
  <Category></Category>
  <Comment></Comment>
  <Tags></Tags>
  <IconPath></IconPath>
  <HelpLink></HelpLink>
</Dev2WizardPayload>");

            mockEventAggregator.Setup(c => c.Publish(It.IsAny<UpdateResourceMessage>()))
                               .Callback<object>(msg =>
                                   {
                                       var resourcemodelMsg = (UpdateResourceMessage)msg;
                                       Assert.IsTrue(resourcemodelMsg.ResourceModel == _mockResourceModel.Object);
                                   });

            resourceWizardViewModel.Dev2Done();
        }

        #endregion Dev2Done Tests

        #region Dev2Set Tests

        [TestMethod]
        public void Dev2Set_Where_ResponseIsUnsupported_Expected_MessageShown()
        {
            Mock<IWebCommunicationResponse> _mockedWebCommunicationResponse = new Mock<IWebCommunicationResponse>();
            _mockedWebCommunicationResponse.Setup(w => w.ContentType).Returns("text/xml");

            Mock<IWebCommunication> _mockedWebCommunication = new Mock<IWebCommunication>();
            _mockedWebCommunication.Setup(w => w.Post(It.IsAny<string>(), It.IsAny<string>())).Returns(_mockedWebCommunicationResponse.Object);

            var importServiceContext = CompositionInitializer.InitializeResourceWizardViewModelTests(_mockedWebCommunication.Object);
            ImportService.CurrentContext = importServiceContext;

            Mock<IContextualResourceModel> _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            ResourceWizardViewModel resourceWizardViewModel = new ResourceWizardViewModel(_mockResourceModel.Object);

            bool messageShown = false;
            Mock<IPopupController> _mockPopup = new Mock<IPopupController>();
            _mockPopup.Setup(p => p.Show()).Callback(() => messageShown = true);

            resourceWizardViewModel.PopupProvider = _mockPopup.Object;

            resourceWizardViewModel.Dev2Set("", "");

            Assert.IsTrue(messageShown);
        }

        [TestMethod]
        public void Dev2Set_Where_ResponseIsNull_Expected_MessageShown()
        {
            Mock<IWebCommunication> _mockedWebCommunication = new Mock<IWebCommunication>();
            _mockedWebCommunication.Setup(w => w.Post(It.IsAny<string>(), It.IsAny<string>())).Returns<object>(null);

            var importServiceContext = CompositionInitializer.InitializeResourceWizardViewModelTests(_mockedWebCommunication.Object);
            ImportService.CurrentContext = importServiceContext;

            Mock<IContextualResourceModel> _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            ResourceWizardViewModel resourceWizardViewModel = new ResourceWizardViewModel(_mockResourceModel.Object);

            bool messageShown = false;
            Mock<IPopupController> _mockPopup = new Mock<IPopupController>();
            _mockPopup.Setup(p => p.Show()).Callback(() => messageShown = true);

            resourceWizardViewModel.PopupProvider = _mockPopup.Object;

            resourceWizardViewModel.Dev2Set("", "");

            Assert.IsTrue(messageShown);
        }

        [TestMethod]
        public void Dev2Set_Where_ValidResponse_Expected_NoException()
        {
            Mock<IWebCommunication> _mockedWebCommunication = new Mock<IWebCommunication>();
            _mockedWebCommunication.Setup(w => w.Post(It.IsAny<string>(), It.IsAny<string>())).Returns<object>(null);

            var importServiceContext = CompositionInitializer.InitializeResourceWizardViewModelTests(_mockedWebCommunication.Object);
            ImportService.CurrentContext = importServiceContext;

            Mock<IContextualResourceModel> _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();
            ResourceWizardViewModel resourceWizardViewModel = new ResourceWizardViewModel(_mockResourceModel.Object);

            Mock<IMainViewModel> mainVM = Dev2MockFactory.SetupMainViewModel(); ;

            Mock<IPopupController> _mockPopup = new Mock<IPopupController>();

            resourceWizardViewModel.PopupProvider = _mockPopup.Object;

            resourceWizardViewModel.Dev2Set("", "");
        }

        #endregion Dev2Set Test
    }
}
