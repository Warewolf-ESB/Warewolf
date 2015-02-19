using System;
using System.Text;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Views.WorkflowDesigner;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class WorkflowServiceDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowServiceDesignerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkflowServiceDesignerViewModel_Constructor_NullResource_ThrowsException()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
           new WorkflowServiceDesignerViewModel(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowServiceDesignerViewModel_Constructor")]
        public void WorkflowServiceDesignerViewModel_Constructor_ShouldCreateWorkflowDesignerView()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            var viewModel = new WorkflowServiceDesignerViewModel(new Mock<IXamlResource>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.DesignerView);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowServiceDesignerViewModel_Constructor")]
        public void WorkflowServiceDesignerViewModel_Constructor_ResourceHasNoXAML_ShouldCreateWorkflow()
        {
            //------------Setup for test--------------------------
            var mockResource = new Mock<IXamlResource>();
            mockResource.Setup(resource => resource.Xaml).Returns((StringBuilder)null);
            mockResource.Setup(resource => resource.ResourceName).Returns("NewWF");
            //------------Execute Test---------------------------
            var viewModel = new WorkflowServiceDesignerViewModel(mockResource.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsNewWorkflow);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkflowServiceDesignerViewModel_Constructor")]
        public void WorkflowServiceDesignerViewModel_Constructor_ResourceHasXAML_ShouldSetDesignerTextToXAML()
        {
            //------------Setup for test--------------------------
            var mockResource = new Mock<IXamlResource>();
            mockResource.Setup(resource => resource.Xaml).Returns(new StringBuilder("This is my WF"));
            mockResource.Setup(resource => resource.ResourceName).Returns("NewWF");
            //------------Execute Test---------------------------
            var viewModel = new WorkflowServiceDesignerViewModel(mockResource.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.IsNewWorkflow);
            Assert.AreEqual("This is my WF",viewModel.DesignerText);
        }
    }
}
