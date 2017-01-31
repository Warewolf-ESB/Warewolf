using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.ActionRegion;
using Dev2.Activities.Designers2.Core.ConstructorRegion;
using Dev2.Activities.Designers2.Core.InputRegion;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.DotNet;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;

// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.Core.DotNet
{
    [TestClass]
    public class DotNetMethodRegionTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetMethodRegion_Constructor")]
        public void DotNetMethodRegion_Constructor_IsNew_ValidateDependencies()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(dotNetMethodRegion);
            Assert.AreEqual("DotNetMethodRegion", dotNetMethodRegion.ToolRegionName);
            Assert.AreEqual(0, dotNetMethodRegion.Dependants.Count);
            Assert.AreEqual(false, dotNetMethodRegion.IsMethodExpanded);
            Assert.AreEqual(false, dotNetMethodRegion.IsRefreshing);
            Assert.AreEqual(true, dotNetMethodRegion.IsEnabled);
            Assert.AreEqual(0, dotNetMethodRegion.Errors.Count);
            Assert.AreEqual(70, dotNetMethodRegion.LabelWidth);
            Assert.AreEqual(1, dotNetMethodRegion.MethodsToRun.Count);
            Assert.IsNotNull(dotNetMethodRegion.RefreshMethodsCommand);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetMethodRegion_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DotNetMethodRegion_Constructor_IsNew_ValidateShellVmDependencies()
            => new DotNetMethodRegion(default(IShellViewModel), new ActionInputDatatalistMapper());

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DotNetMethodRegion_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DotNetMethodRegion_Constructor_IsNew_ValidateMapperVmDependencies()
            => new DotNetMethodRegion(new Mock<IShellViewModel>().Object, default(IActionInputDatatalistMapper));

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SelectedMethod_GivenIsSet_ShouldFireIsVoidChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;

            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsVoid")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.SelectedMethod = new PluginAction();
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SelectedMethod_GivenIsSet_ShouldFireIsMethodChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;

            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Method")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.SelectedMethod = new PluginAction();
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SelectedMethod_GivenIsSet_ShouldFireIsRecordsetNameChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;

            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "RecordsetName")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.SelectedMethod = new PluginAction();
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SelectedMethod_GivenIsSet_ShouldFireIsRecordsetIsObjectChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;

            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsObject")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.SelectedMethod = new PluginAction();
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SelectedMethod_GivenIsSet_ShouldFireIsRecordsetIsObjectNameChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;

            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ObjectName")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.SelectedMethod = new PluginAction();
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SelectedMethod_GivenIsSet_ShouldFireIsRecordsetIsObjectResultChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;

            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ObjectResult")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.SelectedMethod = new PluginAction();
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SelectedMethod_GivenIsSet_ShouldFireIsRecordsetIsInputsChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;

            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Inputs")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.SelectedMethod = new PluginAction();
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SelectedMethod_GivenIsSet_ShouldFireIsRecordsetIsInputsEmptyRowsChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;

            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsInputsEmptyRows")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.SelectedMethod = new PluginAction() { Method = "A" };
            Assert.IsTrue(wasCalled);
            Assert.AreEqual("A", dotNetMethodRegion.SelectedMethod.Method);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsVoid_GivenIsSet_ShouldPropertyChangesChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;

            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsVoid")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.IsVoid = true;
            Assert.IsTrue(wasCalled);
            Assert.AreEqual(true, dotNetMethodRegion.IsVoid);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RecordsetName_GivenIsSet_ShouldPropertyChangesChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;
            var pluginAction = new PluginAction() { };
            var methodInfo = typeof(DotNetMethodRegion).GetMethod("SetSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(dotNetMethodRegion, new object[] { pluginAction });
            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "RecordsetName")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.RecordsetName = "g";
            Assert.IsTrue(wasCalled);
            Assert.AreEqual("g", dotNetMethodRegion.RecordsetName);
            Assert.AreEqual("g", pluginAction.OutputVariable);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsObject_GivenIsSet_ShouldPropertyChangesChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;
            var pluginAction = new PluginAction() { IsObject = true };
            var methodInfo = typeof(DotNetMethodRegion).GetMethod("SetSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(dotNetMethodRegion, new object[] { pluginAction });

            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsObject")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.IsObject = true;
            Assert.IsTrue(wasCalled);
            Assert.AreEqual(true, dotNetMethodRegion.IsObject);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ObjectName_GivenIsSet_ShouldPropertyChangesChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            shellVm.Setup(model => model.UpdateCurrentDataListWithObjectFromJson(It.IsAny<string>(), It.IsAny<string>()));
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;
            var pluginAction = new PluginAction() { };
            var methodInfo = typeof(DotNetMethodRegion).GetMethod("SetSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(dotNetMethodRegion, new object[] { pluginAction });
            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ObjectName")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.IsObject = true;
            dotNetMethodRegion.ObjectResult = "ObjectName";
            dotNetMethodRegion.ObjectName = "[[@ObjectName]]";
            Assert.IsTrue(wasCalled);
            Assert.AreEqual("[[@ObjectName]]", dotNetMethodRegion.ObjectName);
            shellVm.Verify(model => model.UpdateCurrentDataListWithObjectFromJson(It.IsAny<string>(), It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ObjectName_GivenIsSetToEmpty_ShouldPropertyChangesChangesNoUpdatesToDatalist()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            shellVm.Setup(model => model.UpdateCurrentDataListWithObjectFromJson(It.IsAny<string>(), It.IsAny<string>()));
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;
            var pluginAction = new PluginAction() { };
            var methodInfo = typeof(DotNetMethodRegion).GetMethod("SetSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(dotNetMethodRegion, new object[] { pluginAction });
            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ObjectName")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.IsObject = true;
            dotNetMethodRegion.ObjectResult = "ObjectName";
            dotNetMethodRegion.ObjectName = null;
            Assert.IsTrue(wasCalled);
            Assert.AreEqual("", dotNetMethodRegion.SelectedMethod.OutputVariable);
            Assert.AreEqual("", dotNetMethodRegion.ObjectName);
            shellVm.Verify(model => model.UpdateCurrentDataListWithObjectFromJson(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Inputs_GivenIsSet_ShouldPropertyChangesChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var pluginAction = new PluginAction() { };
            var methodInfo = typeof(DotNetMethodRegion).GetMethod("SetSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(dotNetMethodRegion, new object[] { pluginAction });
            //------------Execute Test---------------------------

            var wasCalled = false;
            var wasInputsEmptyCalled = false;

            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Inputs")
                    wasCalled = true;
                if (args.PropertyName == "IsInputsEmptyRows")
                    wasInputsEmptyCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.IsObject = true;
            dotNetMethodRegion.Inputs = new List<IServiceInput>();
            Assert.IsTrue(wasCalled);
            Assert.AreEqual(0, dotNetMethodRegion.Inputs.Count);

            dotNetMethodRegion.Inputs = null;
            Assert.IsTrue(wasInputsEmptyCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ObjectResult_GivenIsSet_ShouldPropertyChangesChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var pluginAction = new PluginAction() { };
            var methodInfo = typeof(DotNetMethodRegion).GetMethod("SetSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(dotNetMethodRegion, new object[] { pluginAction });
            //------------Execute Test---------------------------
            var wasCalled = false;

            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ObjectResult")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.ObjectResult = "ObjectResult";
            Assert.IsTrue(wasCalled);
            Assert.AreEqual("ObjectResult", dotNetMethodRegion.ObjectResult);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsMethodExpanded_GivenIsSet_ShouldPropertyChangesChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;

            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsMethodExpanded")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.IsMethodExpanded = true;
            Assert.IsTrue(wasCalled);
            Assert.AreEqual(true, dotNetMethodRegion.IsMethodExpanded);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsRefreshing_GivenIsSet_ShouldPropertyChangesChanges()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var wasCalled = false;

            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsRefreshing")
                    wasCalled = true;
            };
            //---------------Test Result -----------------------
            dotNetMethodRegion.IsRefreshing = true;
            Assert.IsTrue(wasCalled);
            Assert.AreEqual(true, dotNetMethodRegion.IsRefreshing);
        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CloneRegion_GivenRegion_ShouldCopyInputs()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object)
            {
                SelectedMethod = new PluginAction()
                {
                    Inputs = new List<IServiceInput>() { new ServiceInput("a", "a"), new ServiceInput("b", "b") }
                }
            };
            var cloneRegion = dotNetMethodRegion.CloneRegion();
            //---------------Test Result -----------------------
            Assert.AreEqual(2, ((DotNetMethodRegion)cloneRegion).Inputs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CloneRegion_GivenRegion_ShouldCopyIsEnabled()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object)
            {
                IsEnabled = false
            };
            var pluginAction = new PluginAction() { };
            var methodInfo = typeof(DotNetMethodRegion).GetMethod("SetSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(dotNetMethodRegion, new object[] { pluginAction });

            //------------Execute Test---------------------------

            var cloneRegion = dotNetMethodRegion.CloneRegion();
            //---------------Test Result -----------------------
            Assert.AreEqual(false, cloneRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CloneRegion_GivenRegion_ShouldCopyFullName()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object)
            {
                SelectedMethod = new PluginAction()
                {
                    FullName = "f",
                    Inputs = new List<IServiceInput>() { new ServiceInput("a", "a"), new ServiceInput("b", "b") }
                }
            };
            var cloneRegion = dotNetMethodRegion.CloneRegion();
            //---------------Test Result -----------------------
            Assert.AreEqual("f", ((DotNetMethodRegion)cloneRegion).SelectedMethod.FullName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CloneRegion_GivenRegion_ShouldCopyMethod()
        {
            //------------Setup for test--------------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);

            //------------Execute Test---------------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object)
            {
                SelectedMethod = new PluginAction()
                {
                    Method = "f",
                    Inputs = new List<IServiceInput>() { new ServiceInput("a", "a"), new ServiceInput("b", "b") }
                }
            };
            var cloneRegion = dotNetMethodRegion.CloneRegion();
            //---------------Test Result -----------------------
            Assert.AreEqual("f", ((DotNetMethodRegion)cloneRegion).SelectedMethod.Method);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RestoreRegion_GivenDotnetRegion_ShouldFireSelectedMethodChanged()
        {
            //---------------Set up test pack-------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);
            var wasCalled = false;
            //---------------Execute Test ----------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var dotNetMethodRegionToRestore = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object)
            {
                SelectedMethod = new PluginAction()
            };
            dotNetMethodRegion.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "SelectedMethod")
                {
                    wasCalled = true;
                }
            };
            dotNetMethodRegion.RestoreRegion(dotNetMethodRegionToRestore);
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RestoreRegion_GivenDotnetRegion_ShouldSetSelectedMethod()
        {
            //---------------Set up test pack-------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);
            //---------------Execute Test ----------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var dotNetMethodRegionToRestore = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object)
            {
                SelectedMethod = new PluginAction() { Method = "a" }
            };
            dotNetMethodRegion.RestoreRegion(dotNetMethodRegionToRestore);
            //---------------Test Result -----------------------
            Assert.AreEqual(dotNetMethodRegion.SelectedMethod.Method, dotNetMethodRegionToRestore.SelectedMethod.Method);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CanRefresh_GivenSourceSelected_ShouldreturnTrue()
        {
            //---------------Set up test pack-------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            nameSpaceRegion.Setup(region => region.SelectedNamespace).Returns(new NamespaceItem());
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);
            //---------------Execute Test ----------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var canRefresh = dotNetMethodRegion.CanRefresh();
            //---------------Test Result -----------------------
            Assert.IsTrue(canRefresh);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CanRefresh_GivenSourceNotSelected_ShouldreturnFalse()
        {
            //---------------Set up test pack-------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new Mock<IPluginSource>().Object);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            nameSpaceRegion.Setup(region => region.SelectedNamespace);
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);
            //---------------Execute Test ----------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var canRefresh = dotNetMethodRegion.CanRefresh();
            //---------------Test Result -----------------------
            Assert.IsFalse(canRefresh);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CanRefresh_GivenNamespaceNotSelected_ShouldreturnFalse()
        {
            //---------------Set up test pack-------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource);
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            nameSpaceRegion.Setup(region => region.SelectedNamespace).Returns(new NamespaceItem());
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);
            //---------------Execute Test ----------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var canRefresh = dotNetMethodRegion.CanRefresh();
            //---------------Test Result -----------------------
            Assert.IsFalse(canRefresh);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UpdateBasedOnNamespace_GivenSource_ShouldLoadMethodsToRun()
        {
            //---------------Set up test pack-------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new PluginSourceDefinition());
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            nameSpaceRegion.Setup(region => region.SelectedNamespace).Returns(new NamespaceItem());
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);
            //---------------Execute Test ----------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var methodInfo = typeof(DotNetMethodRegion).GetMethod("UpdateBasedOnNamespace", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(dotNetMethodRegion, new object[] { });
            //---------------Test Result -----------------------
            serviceModel.Verify(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()));
            Assert.IsNull(dotNetMethodRegion.SelectedMethod);
            Assert.IsTrue(dotNetMethodRegion.IsActionEnabled);
            Assert.IsTrue(dotNetMethodRegion.IsEnabled);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RefreshMethodsCommand_GivenSource_ShouldLoadMethodsToRun()
        {
            //---------------Set up test pack-------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new PluginSourceDefinition());
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            nameSpaceRegion.Setup(region => region.SelectedNamespace).Returns(new NamespaceItem());
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);
            //---------------Execute Test ----------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);

            //---------------Test Result -----------------------
            Assert.IsTrue(dotNetMethodRegion.RefreshMethodsCommand.CanExecute(null));
            dotNetMethodRegion.RefreshMethodsCommand.Execute(null);
            serviceModel.Verify(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()));
            Assert.IsFalse(dotNetMethodRegion.IsRefreshing);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SourceOnSomethingChanged_GivenSource_ShouldLoadMethodsToRun()
        {
            //---------------Set up test pack-------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new PluginSourceDefinition());
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            nameSpaceRegion.Setup(region => region.SelectedNamespace).Returns(new NamespaceItem());
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);
            //---------------Execute Test ----------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);
            var methodInfo = typeof(DotNetMethodRegion).GetMethod("SourceOnSomethingChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            var errorHandler = false;
            var sometingChanged = false;
            dotNetMethodRegion.ErrorsHandler += (o, list) =>
            {
                errorHandler = true;
            };
            dotNetMethodRegion.SomethingChanged += (o, args) =>
            {
                sometingChanged = true;
            };
            var wasCalled = false;
            dotNetMethodRegion.PropertyChanged += (o, args) =>
            {
                if (args.PropertyName == "IsEnabled")
                {
                    wasCalled = true;
                }
            };
            object sender = new DotNetConstructorRegion();
            IToolRegion regionIncoming = new DotNetInputRegion();
            methodInfo.Invoke(dotNetMethodRegion, new[] { sender, regionIncoming });
            
            Assert.IsTrue(wasCalled);
            Assert.IsTrue(errorHandler);
            Assert.IsTrue(sometingChanged);
            //---------------Test Result -----------------------
            serviceModel.Verify(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()));

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewObjectResult_GivenObjectResult_ShouldObjectPopup()
        {
            //---------------Set up test pack-------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new PluginSourceDefinition());
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            nameSpaceRegion.Setup(region => region.SelectedNamespace).Returns(new NamespaceItem());
            var shellVm = new Mock<IShellViewModel>();
            var mock = new Mock<IJsonObjectsView>();
            mock.Setup(view => view.ShowJsonString(It.IsAny<string>()));
            CustomContainer.Register(shellVm.Object);
            CustomContainer.RegisterInstancePerRequestType<IJsonObjectsView>(() => mock.Object);
            //---------------Execute Test ----------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);

            //---------------Test Result -----------------------
            Assert.IsNotNull(dotNetMethodRegion.JsonObjectsView);
            Assert.IsTrue(dotNetMethodRegion.ViewObjectResult.CanExecute(null));
            dotNetMethodRegion.ViewObjectResult.Execute(null);
            mock.Verify(view => view.ShowJsonString(It.IsAny<string>()));

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ViewObjectResult_GivenObjectServiceInput_ShouldObjectPopup()
        {
            //---------------Set up test pack-------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Returns(new List<IPluginAction>()
                        {
                            new PluginAction()
                        });
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new PluginSourceDefinition());
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            nameSpaceRegion.Setup(region => region.SelectedNamespace).Returns(new NamespaceItem());
            var shellVm = new Mock<IShellViewModel>();
            var mock = new Mock<IJsonObjectsView>();
            mock.Setup(view => view.ShowJsonString(It.IsAny<string>()));
            CustomContainer.Register(shellVm.Object);
            CustomContainer.RegisterInstancePerRequestType<IJsonObjectsView>(() => mock.Object);
            //---------------Execute Test ----------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);

            //---------------Test Result -----------------------
            Assert.IsNotNull(dotNetMethodRegion.JsonObjectsView);
            Assert.IsTrue(dotNetMethodRegion.ViewObjectResultForParameterInput.CanExecute(null));
            dotNetMethodRegion.ViewObjectResultForParameterInput.Execute(new ServiceInput() {Dev2ReturnType = "Object"});
            mock.Verify(view => view.ShowJsonString(It.IsAny<string>()));

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RefreshMethodsCommand_GivenThrowsError_ShouldAddErrors()
        {
            //---------------Set up test pack-------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Throws(new AccessViolationException());
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new PluginSourceDefinition());
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            nameSpaceRegion.Setup(region => region.SelectedNamespace).Returns(new NamespaceItem());
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);
            //---------------Execute Test ----------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object);

            //---------------Test Result -----------------------
            var count = dotNetMethodRegion.Errors.Count;
            Assert.AreEqual(1, count);
            Assert.AreEqual("Attempted to read or write protected memory. This is often an indication that other memory is corrupt.", dotNetMethodRegion.Errors.Single());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsObjectEnabled_GivenIsNotObject_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Throws(new AccessViolationException());
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new PluginSourceDefinition());
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            nameSpaceRegion.Setup(region => region.SelectedNamespace).Returns(new NamespaceItem());
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);
            //---------------Execute Test ----------------------
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object) { IsObject = false };
            //---------------Test Result -----------------------
            Assert.IsTrue(dotNetMethodRegion.IsObjectEnabled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsInputsEmptyRows_GivenNoInputs_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var serviceModel = new Mock<IPluginServiceModel>();
            serviceModel.Setup(model => model.GetActionsWithReturns(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>()))
                        .Throws(new AccessViolationException());
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfEnhancedDotNetDllActivity);
            var pluginSourceRegion = new Mock<ISourceToolRegion<IPluginSource>>();
            pluginSourceRegion.Setup(region => region.SelectedSource).Returns(new PluginSourceDefinition());
            var nameSpaceRegion = new Mock<INamespaceToolRegion<INamespaceItem>>();
            nameSpaceRegion.Setup(region => region.SelectedNamespace).Returns(new NamespaceItem());
            var shellVm = new Mock<IShellViewModel>();
            CustomContainer.Register(shellVm.Object);
            var dotNetMethodRegion = new DotNetMethodRegion(serviceModel.Object, modelItem, pluginSourceRegion.Object, nameSpaceRegion.Object) { IsObject = false };
            var pluginAction = new PluginAction()
            {
                Inputs = new List<IServiceInput>()
            };
            var methodInfo = typeof(DotNetMethodRegion).GetMethod("SetSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(dotNetMethodRegion, new object[] { pluginAction });
            //---------------Execute Test ----------------------
            
            //---------------Test Result -----------------------
            Assert.IsTrue(dotNetMethodRegion.IsInputsEmptyRows);
        }
    }
}
