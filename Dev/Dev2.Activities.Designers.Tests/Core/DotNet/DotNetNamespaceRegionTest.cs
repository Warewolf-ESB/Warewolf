using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Activities.Designers2.Core.NamespaceRegion;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;



namespace Dev2.Activities.Designers.Tests.Core.DotNet
{
    [TestClass]
    public class DotNetNamespaceRegionTest
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetNamespaceRegion_Constructor")]
        public void DotNetNamespaceRegion_Constructor_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var src = new Mock<IPluginServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>());
            DotNetSourceRegion sourceRegion = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));

            //------------Execute Test---------------------------
            DotNetNamespaceRegion dotNetNamespaceRegion = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()), sourceRegion);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, dotNetNamespaceRegion.Errors.Count);
            Assert.IsTrue(dotNetNamespaceRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetNamespaceRegion_ConstructorWithSelectedNamespace")]
        public void DotNetNamespaceRegion_ConstructorWithSelectedNamespace_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity() { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var pluginSrc = new PluginSourceDefinition() { Id = id, Name = "johnny" };
            var namespaceItem = new NamespaceItem { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { pluginSrc });

            DotNetSourceRegion sourceRegion = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));
            sourceRegion.SelectedSource = pluginSrc;

            //------------Execute Test---------------------------
            DotNetNamespaceRegion dotNetNamespaceRegion = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            dotNetNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Assert Results-------------------------
            Assert.AreEqual(namespaceItem, dotNetNamespaceRegion.SelectedNamespace);
            Assert.IsTrue(dotNetNamespaceRegion.CanRefresh());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetNamespaceRegion_ChangeNamespaceSomethingChanged")]
        public void DotNetNamespaceRegion_ChangeNamespaceSomethingChanged_ExpectedChange_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity() { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var pluginSrc = new PluginSourceDefinition() { Id = id };
            var evt = false;
            var s2 = new PluginSourceDefinition() { Id = Guid.NewGuid() };
            var namespaceItem = new NamespaceItem { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { pluginSrc, s2 });

            DotNetSourceRegion sourceRegion = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));

            //------------Execute Test---------------------------
            DotNetNamespaceRegion dotNetNamespaceRegion = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            dotNetNamespaceRegion.SomethingChanged += (a, b) => { evt = true; };
            dotNetNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Assert Results-------------------------
            Assert.IsTrue(evt);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetNamespaceRegion_ChangeNamespaceSomethingChanged")]
        public void DotNetNamespaceRegion_ChangeNamespaceSomethingChanged_RestoreRegion_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity() { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var pluginSrc = new PluginSourceDefinition() { Id = id, Name = "bob" };
            var namespaceItem = new NamespaceItem { FullName = "bravo" };

            var s2 = new PluginSourceDefinition() { Id = Guid.NewGuid(), Name = "bob" };
            var namespaceItem1 = new NamespaceItem { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { pluginSrc, s2 });

            DotNetSourceRegion sourceRegion = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));

            //------------Execute Test---------------------------
            DotNetNamespaceRegion dotNetNamespaceRegion = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);

            var clone1 = new Mock<IToolRegion>();
            var clone2 = new Mock<IToolRegion>();
            var dep1 = new Mock<IToolRegion>();
            dep1.Setup(a => a.CloneRegion()).Returns(clone1.Object);

            var dep2 = new Mock<IToolRegion>();
            dep2.Setup(a => a.CloneRegion()).Returns(clone2.Object);
            dotNetNamespaceRegion.Dependants = new List<IToolRegion> { dep1.Object, dep2.Object };
            dotNetNamespaceRegion.SelectedNamespace = namespaceItem;
            dotNetNamespaceRegion.SelectedNamespace = namespaceItem1;

            //------------Assert Results-------------------------
            dep1.Verify(a => a.RestoreRegion(clone1.Object), Times.Never);
            dep2.Verify(a => a.RestoreRegion(clone2.Object), Times.Never);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetNamespaceRegion_ChangeNamespaceSomethingChanged")]
        public void DotNetNamespaceRegion_ChangeNamespaceSomethingChanged_RegionsNotRestored_Invalid()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity() { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var pluginSrc = new PluginSourceDefinition() { Id = id };
            var namespaceItem = new NamespaceItem { FullName = "bravo" };

            var s2 = new PluginSourceDefinition() { Id = Guid.NewGuid() };
            var namespaceItem1 = new NamespaceItem { FullName = "bravo1" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { pluginSrc, s2 });

            DotNetSourceRegion sourceRegion = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));

            //------------Execute Test---------------------------
            DotNetNamespaceRegion dotNetNamespaceRegion = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);

            var clone1 = new Mock<IToolRegion>();
            var clone2 = new Mock<IToolRegion>();
            var dep1 = new Mock<IToolRegion>();
            dep1.Setup(a => a.CloneRegion()).Returns(clone1.Object);

            var dep2 = new Mock<IToolRegion>();
            dep2.Setup(a => a.CloneRegion()).Returns(clone2.Object);
            dotNetNamespaceRegion.Dependants = new List<IToolRegion> { dep1.Object, dep2.Object };
            dotNetNamespaceRegion.SelectedNamespace = namespaceItem;
            dotNetNamespaceRegion.SelectedNamespace = namespaceItem1;

            //------------Assert Results-------------------------
            dep1.Verify(a => a.RestoreRegion(clone1.Object), Times.Never);
            dep2.Verify(a => a.RestoreRegion(clone2.Object), Times.Never);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetNamespaceRegion_ChangeNamespaceSomethingChanged")]
        public void DotNetNamespaceRegion_ChangeNamespaceSomethingChanged_CloneRegion_ExpectedClone()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity() { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var pluginSrc = new PluginSourceDefinition() { Id = id };
            var s2 = new PluginSourceDefinition() { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { pluginSrc, s2 });

            DotNetSourceRegion sourceRegion = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));

            //------------Execute Test---------------------------
            DotNetNamespaceRegion dotNetNamespaceRegion = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            var cloned = dotNetNamespaceRegion.CloneRegion();

            //------------Assert Results-------------------------
            Assert.AreEqual(cloned.IsEnabled, dotNetNamespaceRegion.IsEnabled);
            Assert.AreEqual(((DotNetNamespaceRegion)cloned).SelectedNamespace, dotNetNamespaceRegion.SelectedNamespace);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetNamespaceRegion_ChangeNamespaceSomethingChanged")]
        public void DotNetNamespaceRegion_ChangeNamespaceSomethingChanged_RestoreRegion_ExpectedRestore()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity() { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var pluginSrc = new PluginSourceDefinition() { Id = id };
            var s2 = new PluginSourceDefinition() { Id = Guid.NewGuid() };
            var namespaceItem = new NamespaceItem { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { pluginSrc, s2 });

            DotNetSourceRegion sourceRegion = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));

            //------------Execute Test---------------------------
            DotNetNamespaceRegion dotNetNamespaceRegion = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            
            DotNetNamespaceRegion dotNetNamespaceRegionToRestore = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            dotNetNamespaceRegionToRestore.IsEnabled = false;
            dotNetNamespaceRegionToRestore.SelectedNamespace = namespaceItem;

            dotNetNamespaceRegion.RestoreRegion(dotNetNamespaceRegionToRestore);

            //------------Assert Results-------------------------
            Assert.AreEqual(dotNetNamespaceRegion.SelectedNamespace, namespaceItem);
            Assert.IsFalse(dotNetNamespaceRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetNamespaces_GivenHasError_ShouldAddIntoErrors()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity() { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var pluginSrc = new PluginSourceDefinition() { Id = id };
            var s2 = new PluginSourceDefinition() { Id = Guid.NewGuid() };
            var namespaceItem = new NamespaceItem { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { pluginSrc, s2 });
            src.Setup(a => a.GetNameSpaces(It.IsAny<IPluginSource>())).Throws(new BadImageFormatException());

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var modelItem = ModelItemUtils.CreateModelItem(new DsfPluginActivity());
            DotNetSourceRegion dotNetSourceRegion = new DotNetSourceRegion(src.Object, modelItem);
            var mockPluginSource = new Mock<IPluginSource>();
            dotNetSourceRegion.SelectedSource = mockPluginSource.Object;
            DotNetNamespaceRegion sourceRegion = new DotNetNamespaceRegion(src.Object, modelItem, dotNetSourceRegion);

            //---------------Test Result -----------------------
            Assert.AreEqual(sourceRegion.Errors.Count, 1);
            Assert.AreEqual(sourceRegion.Errors.Count(s => s.Contains("Format of the executable (.exe) or library (.dll) is invalid")), 1);
        }
    }
}