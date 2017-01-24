using System;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core.ActionRegion;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.DotNet;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
        public void DotNetMethodRegion_Constructor_IsNew_ValidateShellVmDependencies() => new DotNetMethodRegion(default(IShellViewModel));
    }
}
