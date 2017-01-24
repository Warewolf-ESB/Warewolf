using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.Core.ActionRegion;
using Dev2.Activities.Designers2.Net_Dll_Enhanced;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.ToolBase.DotNet;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
using Warewolf.Testing;
// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.DotNetDll
{
    [TestClass]
    public class DotNetDllEnhancedViewModelTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DotNetDllEnhancedViewModel_Constructor_NullModelItem_Exception()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            new DotNetDllEnhancedViewModel(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_Constructor")]
        public void DotNetDllEnhancedViewModel_Constructor_Valid_ShouldSetupViewModel()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------
            var vm = new DotNetDllEnhancedViewModel(CreateModelItem(), ps.Object);
            vm.MethodRegion = new DotNetMethodRegion();
            //------------Assert Results-------------------------
            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.ModelItem);
            Assert.IsTrue(vm.HasLargeView);
            Assert.AreEqual(46, vm.LabelWidth);
            Assert.AreEqual("Done", vm.ButtonDisplayValue);
            Assert.IsTrue(vm.ShowLarge);
            Assert.AreEqual(Visibility.Visible, vm.ThumbVisibility);
            Assert.AreEqual(Visibility.Collapsed, vm.ShowExampleWorkflowLink);
            Assert.IsNotNull(vm.DesignValidationErrors);
            Assert.IsNotNull(vm.FixErrorsCommand);
            Assert.IsNotNull(vm.SourceRegion);
            Assert.IsNotNull(vm.NamespaceRegion);
            Assert.IsNotNull(vm.MethodRegion);
            Assert.IsNotNull(vm.InputArea);
            Assert.IsNotNull(vm.OutputsRegion);
            Assert.IsNotNull(vm.ErrorRegion);
            Assert.IsNotNull(vm.Regions);
            Assert.AreEqual(7, vm.Regions.Count);
            Assert.IsTrue(vm.OutputsRegion.OutputMappingEnabled);
            Assert.IsNotNull(vm.Properties);
            Assert.AreEqual(1, vm.Properties.Count);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_Validate")]
        public void DotNetDllEnhancedViewModel_Validate_HasErrorsIfNoSource()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------
            var vm = new DotNetDllEnhancedViewModel(CreateModelItem(), ps.Object);
            //------------Assert Results-------------------------
            vm.Validate();

            Assert.AreEqual(2, vm.Errors.Count);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_ToModel")]
        public void DotNetDllEnhancedViewModel_ToModel()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            var modelx = vm.ToModel();
            Assert.IsNotNull(modelx);

        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_ToModel")]
        public void DotNetDllEnhancedViewModel_ClearValidationMessage()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.ClearValidationMemoWithNoFoundError();
            Assert.AreEqual(vm.DesignValidationErrors[0].Message, String.Empty);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_SetDisplayName")]
        public void DotNetDllEnhancedViewModel_SetDisplayName()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.SetDisplayName("dsfbob_builer");
            PrivateObject p = new PrivateObject(vm);
            Assert.AreEqual(p.GetProperty("DisplayName"), "DotNet DLLdsfbob_builer");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetDllEnhancedViewModel_Handle")]
        public void DotNetDllEnhancedViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var ps = SetupEmptyMockSource();
            var viewModel = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object);
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_SetDisplayName")]
        public void DotNetDllEnhancedViewModel_ErrorMessage()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.ErrorMessage(new AccessViolationException("bob"), true);
            Assert.IsTrue(vm.Errors.Count > 0);
            Assert.AreEqual("bob", vm.Errors[0].Message);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_SetDisplayName")]
        public void DotNetDllEnhancedViewModel_FixErrors()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.FixErrorsCommand.Execute(null);
            Assert.IsTrue(vm.IsWorstErrorReadOnly);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetDllEnhancedViewModel_SetDisplayName")]
        public void DotNetDllEnhancedViewModel_UpdateWorstDesignError()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object);
            vm.DesignValidationErrors.Add(new ErrorInfo() { Message = "bob error", ErrorType = ErrorType.Critical });
            PrivateObject p = new PrivateObject(vm);
            p.Invoke("UpdateWorstError");
            var inf = p.GetProperty("WorstDesignError") as ErrorInfo;
            //------------Assert Results-------------------------

            Assert.IsNotNull(inf);
            Assert.AreEqual("bob error", inf.Message);
        }
        

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildRegions_GivenNamespacesRegionHasErrors_ShouldhaveErrors()
        {
            //---------------Set up test pack-------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);

            var ps = new Mock<IPluginServiceModel>();
            ps.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { new PluginSourceDefinition() { Id = id } });
            ps.Setup(a => a.GetNameSpacesWithJsonRetunrs(It.IsAny<IPluginSource>())).Throws(new BadImageFormatException());
            ps.Setup(a => a.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new ObservableCollection<IPluginAction>() { new PluginAction() { FullName = "bob", Inputs = new List<IServiceInput>() } });

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var vm = new DotNetDllEnhancedViewModel(CreateModelItemWithValues(), ps.Object) { SourceRegion = null };
            var buildRegions = vm.BuildRegions();
            //---------------Test Result -----------------------
            Assert.AreEqual(buildRegions.Single(region => region is INamespaceToolRegion<INamespaceItem>).Errors.Count, 1);
        }

        private static readonly Guid id = Guid.NewGuid();
        private static Mock<IPluginServiceModel> SetupEmptyMockSource()
        {
            var ps = new Mock<IPluginServiceModel>();
            ps.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { new PluginSourceDefinition() { Id = id } });
            ps.Setup(a => a.GetNameSpacesWithJsonRetunrs(It.IsAny<IPluginSource>())).Returns(new ObservableCollection<INamespaceItem>() { new NamespaceItem() { FullName = "f" } });
            ps.Setup(a => a.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new ObservableCollection<IPluginAction>() { new PluginAction() { FullName = "bob", Inputs = new List<IServiceInput>() } });
            return ps;
        }

        static ModelItem CreateModelItem()
        {
            var activity = new DsfEnhancedDotNetDllActivity();
            return ModelItemUtils.CreateModelItem(activity);
        }

        static ModelItem CreateModelItemWithValues()
        {
            var activity = new DsfEnhancedDotNetDllActivity
            {
                MethodsToRun = new List<IPluginAction>(new[]
                {
                    new PluginAction() { FullName = "bob", Inputs = new List<IServiceInput>() { new ServiceInput() { Name = "a", Value = "b" } } }
                }),
                Namespace = new NamespaceItem() { AssemblyLocation = "d", AssemblyName = "e", FullName = "f", MethodName = "g" },
                SourceId = id
            };
            return ModelItemUtils.CreateModelItem(activity);
        }
    }
}