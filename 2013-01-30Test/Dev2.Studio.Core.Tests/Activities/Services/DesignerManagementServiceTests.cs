using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Core.Tests.Utils;
using Dev2.Studio.Core.Activities.Services;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Activities.Services
{
    [TestClass]
    public class DesignerManagementServiceTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Instantiate_Where_ResourceRepositoryIsNull_ExpectedArgumentNullException()
        {
            DesignerManagementService designerManagementService = new DesignerManagementService(null);
        }

        [TestMethod]
        public void GetResourceModel_Where_ModelItemIsNull_Expected_Null()
        {
            Mock<IResourceRepository> resourceRepository = Dev2MockFactory.SetupFrameworkRepositoryResourceModelMock();
            DesignerManagementService designerManagementService = new DesignerManagementService(resourceRepository.Object);

            IContextualResourceModel expected = null;
            IContextualResourceModel actual = designerManagementService.GetResourceModel(null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetResourceModel_Where_ResourceModelExistsForModelItem_Expected_MatchingResourceModel()
        {
            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock();
            Mock<IResourceRepository> resourceRepository = Dev2MockFactory.SetupFrameworkRepositoryResourceModelMock(resourceModel, new List<IResourceModel>());

            DsfActivity Act = new DsfActivity();
            Act.ServiceName = "Test";
            ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);

            DesignerManagementService designerManagementService = new DesignerManagementService(resourceRepository.Object);

            IContextualResourceModel expected = resourceModel.Object;
            IContextualResourceModel actual = designerManagementService.GetResourceModel(testItem);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetResourceModel_Where_ResourceModelDoesntExistForModelItem_Expected_Null()
        {
            Mock<IResourceRepository> resourceRepository = Dev2MockFactory.SetupFrameworkRepositoryResourceModelMock();

            DsfActivity Act = new DsfActivity();
            Act.ServiceName = "NonExistent";
            ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);

            DesignerManagementService designerManagementService = new DesignerManagementService(resourceRepository.Object);

            IContextualResourceModel expected = null;
            IContextualResourceModel actual = designerManagementService.GetResourceModel(testItem);

            Assert.AreEqual(expected, actual);
        }
    }
}
