﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Dev2.Activities.Designers2.Net_DLL;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
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
        public void DotNetDllViewModel_Constructor_Valid_ShouldSetupViewModel()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------
            var vm = new DotNetDllViewModel(CreateModelItem(),ps.Object);
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
            Assert.AreEqual(7,vm.Regions.Count);
            Assert.IsTrue(vm.OutputsRegion.OutputMappingEnabled);
            Assert.IsNotNull(vm.TestInputCommand);
            Assert.IsNotNull(vm.Properties);
            Assert.AreEqual(1,vm.Properties.Count);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DotNetDllViewModel_Validate")]
        public void DotNetDllViewModel_Validate_HasErrorsIfNoSource()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------
            var vm = new DotNetDllViewModel(CreateModelItem(), ps.Object);
            //------------Assert Results-------------------------
            vm.Validate();

            Assert.AreEqual(vm.Errors.Count,1);
            Assert.AreEqual("Plugin get must be validated before minimising",vm.Errors[0].Message);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DotNetDllViewModel_GenerateOutputsVisible")]
        public void DotNetDllViewModel_GenerateOutputsVisible_Set_SetsOtherProperties()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllViewModel(CreateModelItem(), ps.Object);
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
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DotNetDllViewModel_ToModel")]
        public void DotNetDllViewModel_ToModel()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            var modelx = vm.ToModel();
            Assert.IsNotNull(modelx);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DotNetDllViewModel_ToModel")]
        public void DotNetDllViewModel_ClearValidationMessage()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.ClearValidationMemoWithNoFoundError();
            Assert.AreEqual(vm.DesignValidationErrors[0].Message,String.Empty);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DotNetDllViewModel_SetDisplayName")]
        public void DotNetDllViewModel_SetDisplayName()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.SetDisplayName("dsfbob_builer");
            PrivateObject p = new PrivateObject(vm);
            Assert.AreEqual(p.GetProperty("DisplayName"),"DotNet DLL Connectordsfbob_builer");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DotNetDllViewModel_SetDisplayName")]
        public void DotNetDllViewModel_ErrorMessage()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.ErrorMessage(new AccessViolationException("bob"),true );
            Assert.IsTrue(vm.Errors.Count>0);
            Assert.AreEqual("bob",vm.Errors[0].Message);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DotNetDllViewModel_SetDisplayName")]
        public void DotNetDllViewModel_FixErrors()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllViewModel(CreateModelItemWithValues(), ps.Object);

            //------------Assert Results-------------------------
            vm.FixErrorsCommand.Execute(null);
            Assert.IsTrue(vm.IsWorstErrorReadOnly);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DotNetDllViewModel_SetDisplayName")]
        public void DotNetDllViewModel_UpdateWorstDesignError()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllViewModel(CreateModelItemWithValues(), ps.Object);
            vm.DesignValidationErrors.Add(new ErrorInfo(){Message = "bob error",ErrorType = ErrorType.Critical});
            PrivateObject p= new PrivateObject(vm);
            p.Invoke("UpdateWorstError");
          var inf =   p.GetProperty("WorstDesignError") as ErrorInfo;
            //------------Assert Results-------------------------


            Assert.AreEqual("bob error", inf.Message);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DotNetDllViewModel_SetDisplayName")]
        public void DotNetDllViewModel_Test()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            //------------Execute Test---------------------------

            var vm = new DotNetDllViewModel(CreateModelItemWithValues(), ps.Object);
            vm.TestProcedure();
            //------------Assert Results-------------------------



        }


        private static readonly Guid id = Guid.NewGuid();
        private static Mock<IPluginServiceModel> SetupEmptyMockSource()
        {
            var ps = new Mock<IPluginServiceModel>();
            ps.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>(){new PluginSourceDefinition(){Id = id}});
            ps.Setup(a => a.GetNameSpaces(It.IsAny<IPluginSource>())).Returns(new ObservableCollection<INamespaceItem>(){new NamespaceItem(){FullName = "f"}});
            ps.Setup(a => a.GetActions(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new ObservableCollection<IPluginAction>(){new PluginAction(){FullName = "bob", Inputs = new List<IServiceInput>()}});
            return ps;
        }

        static ModelItem CreateModelItem()
        {
            var activity = new DsfDotNetDllActivity();
            return ModelItemUtils.CreateModelItem(activity);
        }

        static ModelItem CreateModelItemWithValues()
        {
            var activity = new DsfDotNetDllActivity();
            activity.Method = new PluginAction(){FullName = "bob", Inputs = new List<IServiceInput>(){new ServiceInput(){Name = "a",Value = "b"}}};
            activity.Namespace = new NamespaceItem(){AssemblyLocation = "d",AssemblyName = "e",FullName = "f",MethodName = "g"};
            activity.SourceId = id;
            return ModelItemUtils.CreateModelItem(activity);
        }
    }
}
