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




namespace Dev2.Activities.Designers.Tests.Core.ComDll
{
    [TestClass]
    public class ComNamespaceRegionTest
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComDllNamespaceRegion_Constructor")]
        public void ComDllNamespaceRegion_Constructor_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var src = new Mock<IComPluginServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>());
            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));

            //------------Execute Test---------------------------
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()), sourceRegion);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, comNamespaceRegion.Errors.Count);
            Assert.IsTrue(comNamespaceRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComDllNamespaceRegion_ConstructorWithSelectedNamespace")]
        public void ComDllNamespaceRegion_ConstructorWithSelectedNamespace_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity() { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            var pluginSrc = new ComPluginSourceDefinition() { Id = id, ResourceName = "johnny" };
            var namespaceItem = new NamespaceItem { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { pluginSrc });

            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));
            sourceRegion.SelectedSource = pluginSrc;

            //------------Execute Test---------------------------
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            comNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Assert Results-------------------------
            Assert.AreEqual(namespaceItem, comNamespaceRegion.SelectedNamespace);
            Assert.IsTrue(comNamespaceRegion.CanRefresh());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComNamespaceRegion_ChangeNamespaceSomethingChanged")]
        public void ComNamespaceRegion_ChangeNamespaceSomethingChanged_ExpectedChange_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity() { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            var pluginSrc = new ComPluginSourceDefinition() { Id = id };
            var evt = false;
            var s2 = new ComPluginSourceDefinition() { Id = Guid.NewGuid() };
            var namespaceItem = new NamespaceItem { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { pluginSrc, s2 });

            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));

            //------------Execute Test---------------------------
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            comNamespaceRegion.SomethingChanged += (a, b) => { evt = true; };
            comNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Assert Results-------------------------
            Assert.IsTrue(evt);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComNamespaceRegion_ChangeNamespaceSomethingChanged")]
        public void ComNamespaceRegion_ChangeNamespaceSomethingChanged_RestoreRegion_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity() { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            var pluginSrc = new ComPluginSourceDefinition() { Id = id, ResourceName = "bob" };
            var namespaceItem = new NamespaceItem { FullName = "bravo" };

            var s2 = new ComPluginSourceDefinition() { Id = Guid.NewGuid(), ResourceName = "bob" };
            var namespaceItem1 = new NamespaceItem { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { pluginSrc, s2 });

            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));

            //------------Execute Test---------------------------
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);

            var clone1 = new Mock<IToolRegion>();
            var clone2 = new Mock<IToolRegion>();
            var dep1 = new Mock<IToolRegion>();
            dep1.Setup(a => a.CloneRegion()).Returns(clone1.Object);

            var dep2 = new Mock<IToolRegion>();
            dep2.Setup(a => a.CloneRegion()).Returns(clone2.Object);
            comNamespaceRegion.Dependants = new List<IToolRegion> { dep1.Object, dep2.Object };
            comNamespaceRegion.SelectedNamespace = namespaceItem;
            comNamespaceRegion.SelectedNamespace = namespaceItem1;

            //------------Assert Results-------------------------
            dep1.Verify(a => a.RestoreRegion(clone1.Object), Times.Never);
            dep2.Verify(a => a.RestoreRegion(clone2.Object), Times.Never);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComNamespaceRegion_ChangeNamespaceSomethingChanged")]
        public void ComNamespaceRegion_ChangeNamespaceSomethingChanged_RegionsNotRestored_Invalid()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity() { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            var pluginSrc = new ComPluginSourceDefinition() { Id = id };
            var namespaceItem = new NamespaceItem { FullName = "bravo" };

            var s2 = new ComPluginSourceDefinition() { Id = Guid.NewGuid() };
            var namespaceItem1 = new NamespaceItem { FullName = "bravo1" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { pluginSrc, s2 });

            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));

            //------------Execute Test---------------------------
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);

            var clone1 = new Mock<IToolRegion>();
            var clone2 = new Mock<IToolRegion>();
            var dep1 = new Mock<IToolRegion>();
            dep1.Setup(a => a.CloneRegion()).Returns(clone1.Object);

            var dep2 = new Mock<IToolRegion>();
            dep2.Setup(a => a.CloneRegion()).Returns(clone2.Object);
            comNamespaceRegion.Dependants = new List<IToolRegion> { dep1.Object, dep2.Object };
            comNamespaceRegion.SelectedNamespace = namespaceItem;
            comNamespaceRegion.SelectedNamespace = namespaceItem1;

            //------------Assert Results-------------------------
            dep1.Verify(a => a.RestoreRegion(clone1.Object), Times.Never);
            dep2.Verify(a => a.RestoreRegion(clone2.Object), Times.Never);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComNamespaceRegion_ChangeNamespaceSomethingChanged")]
        public void ComNamespaceRegion_ChangeNamespaceSomethingChanged_CloneRegion_ExpectedClone()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity() { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            var pluginSrc = new ComPluginSourceDefinition() { Id = id };
            var s2 = new ComPluginSourceDefinition() { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { pluginSrc, s2 });

            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));

            //------------Execute Test---------------------------
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            var cloned = comNamespaceRegion.CloneRegion();

            //------------Assert Results-------------------------
            Assert.AreEqual(cloned.IsEnabled, comNamespaceRegion.IsEnabled);
            Assert.AreEqual(((ComNamespaceRegion)cloned).SelectedNamespace, comNamespaceRegion.SelectedNamespace);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComNamespaceRegion_ChangeNamespaceSomethingChanged")]
        public void ComNamespaceRegion_ChangeNamespaceSomethingChanged_RestoreRegion_ExpectedRestore()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity() { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            var pluginSrc = new ComPluginSourceDefinition() { Id = id };
            var s2 = new ComPluginSourceDefinition() { Id = Guid.NewGuid() };
            var namespaceItem = new NamespaceItem { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { pluginSrc, s2 });

            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));

            //------------Execute Test---------------------------
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            
            ComNamespaceRegion comNamespaceRegionToRestore = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            comNamespaceRegionToRestore.IsEnabled = false;
            comNamespaceRegionToRestore.SelectedNamespace = namespaceItem;

            comNamespaceRegion.RestoreRegion(comNamespaceRegionToRestore);

            //------------Assert Results-------------------------
            Assert.AreEqual(comNamespaceRegion.SelectedNamespace, namespaceItem);
            Assert.IsFalse(comNamespaceRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetNamespaces_GivenHasError_ShouldAddIntoErrors()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity() { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            var pluginSrc = new ComPluginSourceDefinition() { Id = id };
            var s2 = new ComPluginSourceDefinition() { Id = Guid.NewGuid() };
            var namespaceItem = new NamespaceItem { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { pluginSrc, s2 });
            src.Setup(a => a.GetNameSpaces(It.IsAny<IComPluginSource>())).Throws(new BadImageFormatException());

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var modelItem = ModelItemUtils.CreateModelItem(new DsfComDllActivity());
            ComSourceRegion dotNetSourceRegion = new ComSourceRegion(src.Object, modelItem);
            var mockPluginSource = new Mock<IComPluginSource>();
            dotNetSourceRegion.SelectedSource = mockPluginSource.Object;
            ComNamespaceRegion sourceRegion = new ComNamespaceRegion(src.Object, modelItem, dotNetSourceRegion);

            //---------------Test Result -----------------------
            Assert.AreEqual(sourceRegion.Errors.Count, 1);
            Assert.AreEqual(sourceRegion.Errors.Count(s => s.Contains("Format of the executable (.exe) or library (.dll) is invalid")), 1);
        }
    }
}