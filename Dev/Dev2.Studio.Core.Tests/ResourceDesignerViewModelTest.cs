
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Text;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{


    /// <summary>
    ///This is a result class for ResourceDesignerViewModelTest and is intended
    ///to contain all ResourceDesignerViewModelTest Unit Tests
    ///</summary>
    [TestClass]
    public class ResourceDesignerViewModelTest
    {

        ResourceDesignerViewModel _target;

        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void MyTestInitialize()
        {


            var m = new Mock<IContextualResourceModel>();
            m.Setup(c => c.WorkflowXaml).Returns(new StringBuilder("result"));
            m.Setup(c => c.ResourceType).Returns(ResourceType.Service);

            IContextualResourceModel model = m.Object;
            _target = new ResourceDesignerViewModel(model, null);
        }


        #region DefaultDefinition Tests
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Tests that the Default Service Definition for Services
        ///</summary>
        [TestMethod]
        public void DefaultDefinition_ServiceType_Expected_ServiceDefinitionBuiltForService()
        {
            Mock<IContextualResourceModel> m = new Mock<IContextualResourceModel>();
            m.Setup(c => c.WorkflowXaml).Returns(new StringBuilder(string.Empty)).Verifiable();
            m.Setup(c => c.ResourceType).Returns(ResourceType.Service);
            _target = new ResourceDesignerViewModel(m.Object, null);
#pragma warning disable 168
            var serviceDefinition = _target.ServiceDefinition;
#pragma warning restore 168
            m.Verify(c => c.WorkflowXaml, Times.Exactly(3));
        }

        /// <summary>
        /// Tests that the Default Service Definition for Sources
        ///</summary>
        [TestMethod]
        public void DefaultDefinition_SourceType_Expected_ServiceDefinitionBuiltForService()
        {
            Mock<IContextualResourceModel> m = new Mock<IContextualResourceModel>();
            m.Setup(c => c.WorkflowXaml).Returns(new StringBuilder(string.Empty)).Verifiable();
            m.Setup(c => c.ResourceType).Returns(ResourceType.Source);
            _target = new ResourceDesignerViewModel(m.Object, null);
#pragma warning disable 168
            var actual = _target.ServiceDefinition;
#pragma warning restore 168
            m.Verify(c => c.WorkflowXaml, Times.Exactly(3));
        }

        #endregion DefaultDefinition Tests

        #region UpdateServiceDefinition Tests

        /// <summary>
        ///A result for UpdateServiceDefinition
        ///</summary>
        [TestMethod]
        public void UpdateServiceDefinition()
        {

            _target.ServiceDefinition = new StringBuilder("result");

            Assert.IsTrue(_target.ServiceDefinition.ToString() == "result");
        }

        #endregion UpdateServiceDefinition Tests
    }
}
