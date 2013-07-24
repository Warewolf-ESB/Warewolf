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
    }
}
