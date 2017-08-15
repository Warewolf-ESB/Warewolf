using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Activities.Designers2.Core.ActionRegion;
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
    public class DotNetActionRegionTest
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetActionRegion_Constructor")]
        public void DotNetActionRegion_Constructor_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity() { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>());
            DotNetSourceRegion sourceRegion = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            DotNetNamespaceRegion dotNetNamespaceRegion = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            dotNetNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            DotNetActionRegion dotNetActionRegion = new DotNetActionRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()), sourceRegion, dotNetNamespaceRegion);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, dotNetActionRegion.Errors.Count);
            Assert.IsTrue(dotNetActionRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetActionRegion_ConstructorWithSelectedAction")]
        public void DotNetActionRegion_ConstructorWithSelectedAction_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var dotNetsrc = new PluginSourceDefinition { Id = id, Name = "johnny" };
            var action = new PluginAction { FullName = "bravo", Method = "bravo", ReturnType = typeof(string), Variables = new List<INameValue>()};
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { dotNetsrc });

            DotNetSourceRegion sourceRegion = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));
            sourceRegion.SelectedSource = dotNetsrc;

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            DotNetNamespaceRegion dotNetNamespaceRegion = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            dotNetNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            DotNetActionRegion dotNetActionRegion = new DotNetActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, dotNetNamespaceRegion);
            dotNetActionRegion.SelectedAction = action;

            //------------Assert Results-------------------------
            Assert.AreEqual(action, dotNetActionRegion.SelectedAction);
            Assert.AreEqual(action.FullName, dotNetActionRegion.SelectedAction.FullName);
            Assert.AreEqual(action.Method, dotNetActionRegion.SelectedAction.Method);
            Assert.AreEqual(typeof(string), dotNetActionRegion.SelectedAction.ReturnType);
            Assert.AreEqual(0, dotNetActionRegion.SelectedAction.Variables.Count);
            Assert.IsTrue(dotNetActionRegion.CanRefresh());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetActionRegion_ChangeActionSomethingChanged")]
        public void DotNetActionRegion_ChangeActionSomethingChanged_ExpectedChange_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var dotNetsrc = new PluginSourceDefinition { Id = id };
            var evt = false;
            var s2 = new PluginSourceDefinition() { Id = Guid.NewGuid() };
            var action = new PluginAction { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { dotNetsrc, s2 });

            DotNetSourceRegion sourceRegion = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            DotNetNamespaceRegion dotNetNamespaceRegion = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            dotNetNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            DotNetActionRegion dotNetActionRegion = new DotNetActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, dotNetNamespaceRegion);
            dotNetActionRegion.SomethingChanged += (a, b) => { evt = true; };
            dotNetActionRegion.SelectedAction = action;

            //------------Assert Results-------------------------
            Assert.IsTrue(evt);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetActionRegion_ChangeActionSomethingChanged")]
        public void DotNetActionRegion_ChangeActionSomethingChanged_RestoreRegion_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var dotNetsrc = new PluginSourceDefinition { Id = id, Name = "bob" };
            var action = new PluginAction { FullName = "bravo" };

            var s2 = new PluginSourceDefinition { Id = Guid.NewGuid(), Name = "bob" };
            var action1 = new PluginAction { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { dotNetsrc, s2 });

            DotNetSourceRegion sourceRegion = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            DotNetNamespaceRegion dotNetNamespaceRegion = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            dotNetNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            DotNetActionRegion dotNetActionRegion = new DotNetActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, dotNetNamespaceRegion);

            var clone1 = new Mock<IToolRegion>();
            var clone2 = new Mock<IToolRegion>();
            var dep1 = new Mock<IToolRegion>();
            dep1.Setup(a => a.CloneRegion()).Returns(clone1.Object);

            var dep2 = new Mock<IToolRegion>();
            dep2.Setup(a => a.CloneRegion()).Returns(clone2.Object);
            dotNetActionRegion.Dependants = new List<IToolRegion> { dep1.Object, dep2.Object };
            dotNetActionRegion.SelectedAction = action;
            dotNetActionRegion.SelectedAction = action1;

            //------------Assert Results-------------------------
            dep1.Verify(a => a.RestoreRegion(clone1.Object), Times.Never);
            dep2.Verify(a => a.RestoreRegion(clone2.Object), Times.Never);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetActionRegion_ChangeActionSomethingChanged")]
        public void DotNetActionRegion_ChangeActionSomethingChanged_RegionsNotRestored_Invalid()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var dotNetsrc = new PluginSourceDefinition() { Id = id };
            var action = new PluginAction { FullName = "bravo" };

            var s2 = new PluginSourceDefinition { Id = Guid.NewGuid() };
            var action1 = new PluginAction { FullName = "bravo1" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { dotNetsrc, s2 });

            DotNetSourceRegion sourceRegion = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            DotNetNamespaceRegion dotNetNamespaceRegion = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            dotNetNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            DotNetActionRegion dotNetActionRegion = new DotNetActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, dotNetNamespaceRegion);

            var clone1 = new Mock<IToolRegion>();
            var clone2 = new Mock<IToolRegion>();
            var dep1 = new Mock<IToolRegion>();
            dep1.Setup(a => a.CloneRegion()).Returns(clone1.Object);

            var dep2 = new Mock<IToolRegion>();
            dep2.Setup(a => a.CloneRegion()).Returns(clone2.Object);
            dotNetActionRegion.Dependants = new List<IToolRegion> { dep1.Object, dep2.Object };
            dotNetActionRegion.SelectedAction = action;
            dotNetActionRegion.SelectedAction = action1;

            //------------Assert Results-------------------------
            dep1.Verify(a => a.RestoreRegion(clone1.Object), Times.Never);
            dep2.Verify(a => a.RestoreRegion(clone2.Object), Times.Never);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetActionRegion_ChangeActionSomethingChanged")]
        public void DotNetActionRegion_ChangeActionSomethingChanged_CloneRegion_ExpectedClone()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var dotNetsrc = new PluginSourceDefinition { Id = id };
            var s2 = new PluginSourceDefinition { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { dotNetsrc, s2 });

            DotNetSourceRegion sourceRegion = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            DotNetNamespaceRegion dotNetNamespaceRegion = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            dotNetNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            DotNetActionRegion dotNetActionRegion = new DotNetActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, dotNetNamespaceRegion);
            var cloned = dotNetActionRegion.CloneRegion();

            //------------Assert Results-------------------------
            Assert.AreEqual(cloned.IsEnabled, dotNetActionRegion.IsEnabled);
            Assert.AreEqual(((DotNetActionRegion)cloned).SelectedAction, dotNetActionRegion.SelectedAction);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetActionRegion_ChangeActionSomethingChanged")]
        public void DotNetActionRegion_ChangeActionSomethingChanged_RestoreRegion_ExpectedRestore()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var dotNetsrc = new PluginSourceDefinition { Id = id };
            var s2 = new PluginSourceDefinition { Id = Guid.NewGuid() };
            var action = new PluginAction { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { dotNetsrc, s2 });

            DotNetSourceRegion sourceRegion = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            DotNetNamespaceRegion dotNetNamespaceRegion = new DotNetNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            dotNetNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            DotNetActionRegion dotNetActionRegion = new DotNetActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, dotNetNamespaceRegion);
            
            DotNetActionRegion dotNetActionRegionToRestore = new DotNetActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, dotNetNamespaceRegion);
            dotNetActionRegionToRestore.IsEnabled = false;
            dotNetActionRegionToRestore.SelectedAction = action;

            dotNetActionRegion.RestoreRegion(dotNetActionRegionToRestore);

            //------------Assert Results-------------------------
            Assert.AreEqual(dotNetActionRegion.SelectedAction, action);
            Assert.IsFalse(dotNetActionRegion.IsEnabled);
        }
    }
}
