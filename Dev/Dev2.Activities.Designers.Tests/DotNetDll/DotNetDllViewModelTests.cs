using System;
using System.Activities.Presentation.Model;
using System.Windows;
using Dev2.Activities.Designers2.Net_DLL;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Testing;

// ReSharper disable InconsistentNaming
// ReSharper disable ObjectCreationAsStatement

namespace Dev2.Activities.Designers.Tests.DotNetDll
{
    [TestClass]
    public class DotNetDllViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DotNetDllViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DotNetDllViewModel_Constructor_NullModelItem_Exception()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            new DotNetDllViewModel(null);
            //------------Assert Results-------------------------
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DotNetDllViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DotNetDllViewModel_Constructor_Valid_ShouldSetupViewModel()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            //------------Execute Test---------------------------
            var vm = new DotNetDllViewModel(CreateModelItem(),new Mock<IPluginServiceModel>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.ModelItem);
            Assert.IsTrue(vm.HasLargeView);
            Assert.IsNotNull(vm.ManageServiceInputViewModel);
            Assert.AreEqual(46,vm.LabelWidth);
            Assert.AreEqual("Done",vm.ButtonDisplayValue);
            Assert.IsTrue(vm.ShowLarge);
            Assert.AreEqual(Visibility.Visible,vm.ThumbVisibility);
            Assert.AreEqual(Visibility.Collapsed,vm.ShowExampleWorkflowLink);
            Assert.IsNotNull(vm.DesignValidationErrors);
            Assert.IsNotNull(vm.FixErrorsCommand);
            Assert.IsNotNull(vm.SourceRegion);
            Assert.IsNotNull(vm.NamespaceRegion);
            Assert.IsNotNull(vm.ActionRegion);
            Assert.IsNotNull(vm.InputArea);
            Assert.IsNotNull(vm.OutputsRegion);
            Assert.IsNotNull(vm.ErrorRegion);
            Assert.IsNotNull(vm.Regions);
            Assert.AreEqual(6,vm.Regions.Count);
            Assert.IsTrue(vm.OutputsRegion.OutputMappingEnabled);
            Assert.IsNotNull(vm.TestInputCommand);
            Assert.IsNotNull(vm.Properties);
            Assert.AreEqual(3,vm.Properties.Count);
        }

        static ModelItem CreateModelItem()
        {
            var activity = new DsfDotNetDllActivity();
            return ModelItemUtils.CreateModelItem(activity);
        }
    }
}
