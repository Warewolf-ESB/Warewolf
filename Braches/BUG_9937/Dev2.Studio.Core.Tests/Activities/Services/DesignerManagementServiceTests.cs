using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using Caliburn.Micro;
using Dev2.Composition;
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

        [TestInitialize]
        public void TestInit()
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });

            ImportService.AddExportedValueToContainer(new Mock<IEventAggregator>().Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Instantiate_Where_RootModelIsNull_ExpectedArgumentNullException()
        {
            var designerManagementService = new DesignerManagementService(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Instantiate_Where_ResourceRepositoryIsNull_ExpectedArgumentNullException()
        {
            var rootModel = new Mock<IContextualResourceModel>();
            var designerManagementService = new DesignerManagementService(rootModel.Object, null);
        }


        [TestMethod]
        public void GetResourceModel_Where_ModelItemIsNull_Expected_Null()
        {
            Mock<IResourceRepository> resourceRepository = Dev2MockFactory.SetupFrameworkRepositoryResourceModelMock();
            var rootModel = new Mock<IContextualResourceModel>();
            var designerManagementService = new DesignerManagementService(rootModel.Object, resourceRepository.Object);

            IContextualResourceModel expected = null;
            IContextualResourceModel actual = designerManagementService.GetResourceModel(null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Ignore] // This test is silly with messaging ?!
        public void GetResourceModel_Where_ResourceModelExistsForModelItem_Expected_MatchingResourceModel()
        {
            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock();
            Mock<IResourceRepository> resourceRepository = Dev2MockFactory.SetupFrameworkRepositoryResourceModelMock(resourceModel, new List<IResourceModel>());

            DsfActivity Act = new DsfActivity();
            Act.ServiceName = "Test";
            ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);

            var rootModel = new Mock<IContextualResourceModel>();
            var designerManagementService = new DesignerManagementService(rootModel.Object, resourceRepository.Object);

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

            var rootModel = new Mock<IContextualResourceModel>();
            var designerManagementService = new DesignerManagementService(rootModel.Object, resourceRepository.Object);

            IContextualResourceModel expected = null;
            IContextualResourceModel actual = designerManagementService.GetResourceModel(testItem);

            Assert.AreEqual(expected, actual);
        }

        //2013.02.11: Ashley Lewis - Bug 8846
        [TestMethod]
        public void GetResourceModelWhereServiceNamePropertyIsNullExpectedNull()
        {
            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock();
            Mock<IResourceRepository> resourceRepository = Dev2MockFactory.SetupFrameworkRepositoryResourceModelMock(resourceModel, new List<IResourceModel>());

            ModelItem testItem = TestModelItemFactory.CreateModelItem(new DsfActivity());

            var rootModel = new Mock<IContextualResourceModel>();
            var designerManagementService = new DesignerManagementService(rootModel.Object, resourceRepository.Object);

            IContextualResourceModel actual = designerManagementService.GetResourceModel(testItem);

            Assert.AreEqual(null, actual);
        }
    }
}
