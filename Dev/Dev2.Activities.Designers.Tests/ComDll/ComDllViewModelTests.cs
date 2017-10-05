using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.ComDLL;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.ToolBase.DotNet;
using Dev2.Providers.Errors;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
using Warewolf.Testing;




namespace Dev2.Activities.Designers.Tests.ComDll
{
    [TestClass]
    public class ComDllViewModelTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComDllViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ComDllViewModel_Constructor_NullModelItem_Exception()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            new ComDllViewModel(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComDllViewModel_Constructor")]
        public void ComDllViewModel_Constructor_Valid_ShouldSetupViewModel()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------
            var vm = new ComDllViewModel(CreateModelItem(), ps.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.ModelItem);
            Assert.IsTrue(vm.HasLargeView);
            Assert.IsNotNull(vm.ManageServiceInputViewModel);
            Assert.AreEqual(46, vm.LabelWidth);
            Assert.AreEqual("Done", vm.ButtonDisplayValue);
            Assert.IsTrue(vm.ShowLarge);
            Assert.AreEqual(Visibility.Visible, vm.ThumbVisibility);
            Assert.AreEqual(Visibility.Collapsed, vm.ShowExampleWorkflowLink);
            Assert.IsNotNull(vm.DesignValidationErrors);
            Assert.IsNotNull(vm.FixErrorsCommand);
            Assert.IsNotNull(vm.SourceRegion);
            Assert.IsNotNull(vm.NamespaceRegion);
            Assert.IsNotNull(vm.ActionRegion);
            Assert.IsNotNull(vm.InputArea);
            Assert.IsNotNull(vm.OutputsRegion);
            Assert.IsNotNull(vm.ErrorRegion);
            Assert.IsNotNull(vm.Regions);
            Assert.AreEqual(7, vm.Regions.Count);
            Assert.IsTrue(vm.OutputsRegion.OutputMappingEnabled);
            Assert.IsNotNull(vm.TestInputCommand);
            Assert.IsNotNull(vm.Properties);
            Assert.AreEqual(1, vm.Properties.Count);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComDllViewModel_Validate")]
        public void ComDllViewModel_Validate_HasErrorsIfNoSource()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------
            var vm = new ComDllViewModel(CreateModelItem(), ps.Object);
            //------------Assert Results-------------------------
            vm.Validate();

            Assert.AreEqual(1, vm.Errors.Count);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComDllViewModel_GenerateOutputsVisible")]
        public void ComDllViewModel_GenerateOutputsVisible_Set_SetsOtherProperties()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new ComDllViewModel(CreateModelItem(), ps.Object);
            vm.ManageServiceInputViewModel.InputArea.IsEnabled = false;
            vm.ManageServiceInputViewModel.OutputArea.IsEnabled = false;
            //------------Assert Results-------------------------
            vm.GenerateOutputsVisible = true;

            Assert.IsTrue(vm.GenerateOutputsVisible);
            Assert.IsTrue(vm.ManageServiceInputViewModel.InputArea.IsEnabled);
            Assert.IsFalse(vm.ManageServiceInputViewModel.OutputArea.IsEnabled);

            vm.GenerateOutputsVisible = false;
            Assert.IsFalse(vm.GenerateOutputsVisible);
            Assert.IsFalse(vm.ManageServiceInputViewModel.InputArea.IsEnabled);
            Assert.IsFalse(vm.ManageServiceInputViewModel.OutputArea.IsEnabled);

        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComDllViewModel_ToModel")]
        public void ComDllViewModel_ToModel()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new ComDllViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            var modelx = vm.ToModel();
            Assert.IsNotNull(modelx);

        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComDllViewModel_ToModel")]
        public void ComDllViewModel_ClearValidationMessage()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new ComDllViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.ClearValidationMemoWithNoFoundError();
            Assert.AreEqual(vm.DesignValidationErrors[0].Message, String.Empty);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComDllViewModel_SetDisplayName")]
        public void ComDllViewModel_SetDisplayName()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new ComDllViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.SetDisplayName("dsfbob_builer");
            PrivateObject p = new PrivateObject(vm);
            Assert.AreEqual(p.GetProperty("DisplayName"), "Com DLLdsfbob_builer");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComDllViewModel_Handle")]
        public void ComDllViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var ps = SetupEmptyMockSource();
            var viewModel = new ComDllViewModel(CreateModelItemWithValues(), ps.Object);
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComDllViewModel_SetDisplayName")]
        public void ComDllViewModel_ErrorMessage()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new ComDllViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.ErrorMessage(new AccessViolationException("bob"), true);
            Assert.IsTrue(vm.Errors.Count > 0);
            Assert.AreEqual("bob", vm.Errors[0].Message);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComDllViewModel_SetDisplayName")]
        public void ComDllViewModel_FixErrors()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new ComDllViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.FixErrorsCommand.Execute(null);
            Assert.IsTrue(vm.IsWorstErrorReadOnly);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComDllViewModel_SetDisplayName")]
        public void ComDllViewModel_Test()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new ComDllViewModel(CreateModelItemWithValues(), ps.Object);
            vm.TestProcedure();
            //------------Assert Results-------------------------



        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildRegions_GivenNamespacesRegionHasErrors_ShouldhaveErrors()
        {
            //---------------Set up test pack-------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);

            var ps = new Mock<IComPluginServiceModel>();
            ps.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { new ComPluginSourceDefinition() { Id = id } });
            ps.Setup(a => a.GetNameSpaces(It.IsAny<IComPluginSource>())).Throws(new BadImageFormatException());
            ps.Setup(a => a.GetActions(It.IsAny<IComPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new ObservableCollection<IPluginAction>() { new PluginAction() { FullName = "bob", Inputs = new List<IServiceInput>() } });

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var vm = new ComDllViewModel(CreateModelItemWithValues(), ps.Object) { SourceRegion = null };
            var buildRegions = vm.BuildRegions();
            //---------------Test Result -----------------------
            Assert.AreEqual(buildRegions.Single(region => region is INamespaceToolRegion<INamespaceItem>).Errors.Count, 1);
        }

        private static readonly Guid id = Guid.NewGuid();
        private static Mock<IComPluginServiceModel> SetupEmptyMockSource()
        {
            var ps = new Mock<IComPluginServiceModel>();
            ps.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { new ComPluginSourceDefinition() { Id = id } });
            ps.Setup(a => a.GetNameSpaces(It.IsAny<IComPluginSource>())).Returns(new ObservableCollection<INamespaceItem>() { new NamespaceItem() { FullName = "f" } });
            ps.Setup(a => a.GetActions(It.IsAny<IComPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new ObservableCollection<IPluginAction>() { new PluginAction() { FullName = "bob", Inputs = new List<IServiceInput>() } });
            return ps;
        }

        static ModelItem CreateModelItem()
        {
            var activity = new DsfComDllActivity();
            return ModelItemUtils.CreateModelItem(activity);
        }

        static ModelItem CreateModelItemWithValues()
        {
            var activity = new DsfComDllActivity
            {
                Method = new PluginAction() { FullName = "bob", Inputs = new List<IServiceInput>() { new ServiceInput() { Name = "a", Value = "b" } } },
                Namespace = new NamespaceItem() { AssemblyLocation = "d", AssemblyName = "e", FullName = "f", MethodName = "g" },
                SourceId = id
            };
            return ModelItemUtils.CreateModelItem(activity);
        }
    }
}
