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




namespace Dev2.Activities.Designers.Tests.Core.ComDll
{
    [TestClass]
    public class ComActionRegionTest
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComActionRegion_Constructor")]
        public void ComActionRegion_Constructor_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity() { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>());
            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            comNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            ComActionRegion dotNetActionRegion = new ComActionRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()), sourceRegion, comNamespaceRegion);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, dotNetActionRegion.Errors.Count);
            Assert.IsTrue(dotNetActionRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComActionRegion_ConstructorWithSelectedAction")]
        public void ComActionRegion_ConstructorWithSelectedAction_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            var dotNetsrc = new ComPluginSourceDefinition { Id = id, ResourceName = "johnny" };
            var action = new PluginAction { FullName = "bravo", Method = "bravo", ReturnType = typeof(string), Variables = new List<INameValue>()};
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { dotNetsrc });

            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));
            sourceRegion.SelectedSource = dotNetsrc;

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            comNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            ComActionRegion dotNetActionRegion = new ComActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, comNamespaceRegion);
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
        [TestCategory("ComActionRegion_ChangedSelectedAction")]
        public void ComActionRegion_ChangeSelectedAction_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            var dotNetsrc = new ComPluginSourceDefinition { Id = id, ResourceName = "johnny" };
            var action = new PluginAction { FullName = "bravo", Method = "bravo", ReturnType = typeof(string), Variables = new List<INameValue>()};
            var action2 = new PluginAction { FullName = "simpson", Method = "simpson", ReturnType = typeof(string), Variables = new List<INameValue>()};
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { dotNetsrc });

            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));
            sourceRegion.SelectedSource = dotNetsrc;

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            comNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            ComActionRegion dotNetActionRegion = new ComActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, comNamespaceRegion);
            dotNetActionRegion.SelectedAction = action;

            //------------Assert Results-------------------------
            Assert.AreEqual(action, dotNetActionRegion.SelectedAction);
            Assert.AreEqual(action.FullName, dotNetActionRegion.SelectedAction.FullName);
            Assert.AreEqual(action.Method, dotNetActionRegion.SelectedAction.Method);
            Assert.AreEqual(typeof(string), dotNetActionRegion.SelectedAction.ReturnType);
            Assert.AreEqual(0, dotNetActionRegion.SelectedAction.Variables.Count);
            Assert.IsTrue(dotNetActionRegion.CanRefresh());

            dotNetActionRegion.SelectedAction = action2;
            Assert.AreEqual(action2, dotNetActionRegion.SelectedAction);
            Assert.AreEqual(action2.FullName, dotNetActionRegion.SelectedAction.FullName);
            Assert.AreEqual(action2.Method, dotNetActionRegion.SelectedAction.Method);
            Assert.AreEqual(typeof(string), dotNetActionRegion.SelectedAction.ReturnType);
            Assert.AreEqual(0, dotNetActionRegion.SelectedAction.Variables.Count);
            Assert.IsTrue(dotNetActionRegion.CanRefresh());

            dotNetActionRegion.SelectedAction = action;
            Assert.AreEqual(action, dotNetActionRegion.SelectedAction);
            Assert.AreEqual(action.FullName, dotNetActionRegion.SelectedAction.FullName);
            Assert.AreEqual(action.Method, dotNetActionRegion.SelectedAction.Method);
            Assert.AreEqual(typeof(string), dotNetActionRegion.SelectedAction.ReturnType);
            Assert.AreEqual(0, dotNetActionRegion.SelectedAction.Variables.Count);
            Assert.IsTrue(dotNetActionRegion.CanRefresh());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComActionRegion_ChangeActionSomethingChanged")]
        public void ComActionRegion_ChangeActionSomethingChanged_ExpectedChange_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            var dotNetsrc = new ComPluginSourceDefinition { Id = id };
            var evt = false;
            var s2 = new ComPluginSourceDefinition() { Id = Guid.NewGuid() };
            var action = new PluginAction { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { dotNetsrc, s2 });

            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            comNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            ComActionRegion dotNetActionRegion = new ComActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, comNamespaceRegion);
            dotNetActionRegion.SomethingChanged += (a, b) => { evt = true; };
            dotNetActionRegion.SelectedAction = action;

            //------------Assert Results-------------------------
            Assert.IsTrue(evt);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComActionRegion_ChangeActionSomethingChanged")]
        public void ComActionRegion_ChangeActionSomethingChanged_RestoreRegion_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            var dotNetsrc = new ComPluginSourceDefinition { Id = id, ResourceName = "bob" };
            var action = new PluginAction { FullName = "bravo" };

            var s2 = new ComPluginSourceDefinition { Id = Guid.NewGuid(), ResourceName = "bob" };
            var action1 = new PluginAction { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { dotNetsrc, s2 });

            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            comNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            ComActionRegion dotNetActionRegion = new ComActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, comNamespaceRegion);

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
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComActionRegion_ChangeActionSomethingChanged")]
        public void ComActionRegion_ChangeActionSomethingChanged_RegionsNotRestored_Invalid()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            var dotNetsrc = new ComPluginSourceDefinition() { Id = id };
            var action = new PluginAction { FullName = "bravo" };

            var s2 = new ComPluginSourceDefinition { Id = Guid.NewGuid() };
            var action1 = new PluginAction { FullName = "bravo1" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { dotNetsrc, s2 });

            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            comNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            ComActionRegion dotNetActionRegion = new ComActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, comNamespaceRegion);

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
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComActionRegion_ChangeActionSomethingChanged")]
        public void ComActionRegion_ChangeActionSomethingChanged_CloneRegion_ExpectedClone()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            var dotNetsrc = new ComPluginSourceDefinition { Id = id };
            var s2 = new ComPluginSourceDefinition { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { dotNetsrc, s2 });

            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            comNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            ComActionRegion dotNetActionRegion = new ComActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, comNamespaceRegion);
            var cloned = dotNetActionRegion.CloneRegion();

            //------------Assert Results-------------------------
            Assert.AreEqual(cloned.IsEnabled, dotNetActionRegion.IsEnabled);
            Assert.AreEqual(((ComActionRegion)cloned).SelectedAction, dotNetActionRegion.SelectedAction);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComActionRegion_ChangeActionSomethingChanged")]
        public void ComActionRegion_ChangeActionSomethingChanged_RestoreRegion_ExpectedRestore()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfComDllActivity { SourceId = id };
            var src = new Mock<IComPluginServiceModel>();
            var dotNetsrc = new ComPluginSourceDefinition { Id = id };
            var s2 = new ComPluginSourceDefinition { Id = Guid.NewGuid() };
            var action = new PluginAction { FullName = "bravo" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IComPluginSource>() { dotNetsrc, s2 });

            ComSourceRegion sourceRegion = new ComSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfComDllActivity()));

            var namespaceItem = new NamespaceItem { FullName = "johnny" };
            ComNamespaceRegion comNamespaceRegion = new ComNamespaceRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion);
            comNamespaceRegion.SelectedNamespace = namespaceItem;

            //------------Execute Test---------------------------
            ComActionRegion dotNetActionRegion = new ComActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, comNamespaceRegion);
            
            ComActionRegion dotNetActionRegionToRestore = new ComActionRegion(src.Object, ModelItemUtils.CreateModelItem(act), sourceRegion, comNamespaceRegion);
            dotNetActionRegionToRestore.IsEnabled = false;
            dotNetActionRegionToRestore.SelectedAction = action;

            dotNetActionRegion.RestoreRegion(dotNetActionRegionToRestore);

            //------------Assert Results-------------------------
            Assert.AreEqual(dotNetActionRegion.SelectedAction, action);
            Assert.IsFalse(dotNetActionRegion.IsEnabled);
        }
    }
}
