
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Studio.Core.Activities.Services;
using Dev2.Studio.Core.Interfaces;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Activities.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    [Ignore] //TODO: Fix so not dependant on resource file or localize resource file to test project
    public class DesignerManagementServiceTests
    {

        [TestInitialize]
        public void Initialize()
        {
            AppSettings.LocalHost = "http://localhost:3142";
        }

        [TestMethod]
        [TestCategory("DesignerManagementService_Constructor")]
        [Description("DesignerManagementService must throw null argument exception if root model is null.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DesignerManagementService_UnitTest_ConstructorWithNullRootModel_ThrowsArgumentNullException()
        {
            new DesignerManagementService(null, null);
        }

        [TestMethod]
        [TestCategory("DesignerManagementService_Constructor")]
        [Description("DesignerManagementService must throw null argument exception if resource repository is null.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DesignerManagementService_UnitTest_ConstructorWithNullResourceRepository_ThrowsArgumentNullException()
        {
            var rootModel = new Mock<IContextualResourceModel>();
            new DesignerManagementService(rootModel.Object, null);
        }


        [TestMethod]
        [TestCategory("DesignerManagementService_GetRootResourceModel")]
        [Description("DesignerManagementService GetRootResourceModel must return the same root model given to its constructor.")]
        [Owner("Trevor Williams-Ros")]
        public void DesignerManagementService_UnitTest_GetResourceModel_SameAsConstructorInstance()
        {
            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock();
            Mock<IResourceRepository> resourceRepository = Dev2MockFactory.SetupFrameworkRepositoryResourceModelMock(resourceModel, new List<IResourceModel>());

            var designerManagementService = new DesignerManagementService(resourceModel.Object, resourceRepository.Object);

            IContextualResourceModel expected = resourceModel.Object;
            IContextualResourceModel actual = designerManagementService.GetRootResourceModel();

            Assert.AreEqual(expected, actual);
        }
    }
}
