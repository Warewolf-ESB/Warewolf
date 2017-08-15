using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace Dev2.Activities.Designers.Tests.Core.DotNet
{
    [TestClass]
    public class DotNetSourceRegionTest
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetSourceRegion_Constructor")]
        public void DotNetSourceRegion_Constructor_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var src = new Mock<IPluginServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>());

            //------------Execute Test---------------------------
            DotNetSourceRegion region = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfPluginActivity()));

            //------------Assert Results-------------------------
            Assert.AreEqual(1, region.Errors.Count);
            Assert.IsTrue(region.IsEnabled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetSourceRegion_ConstructorWithSelectedSource")]
        public void DotNetSourceRegion_ConstructorWithSelectedSource_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity() { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var pluginSrc = new PluginSourceDefinition() { Id = id };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { pluginSrc });

            //------------Execute Test---------------------------
            DotNetSourceRegion region = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act));

            //------------Assert Results-------------------------
            Assert.AreEqual(pluginSrc, region.SelectedSource);
            Assert.IsTrue(region.CanEditSource());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetSourceRegion_ChangeSourceSomethingChanged")]
        public void DotNetSourceRegion_ChangeSourceSomethingChanged_ExpectedChange_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity() { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var pluginSrc = new PluginSourceDefinition() { Id = id };
            var evt = false;
            var s2 = new PluginSourceDefinition() { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { pluginSrc, s2 });

            //------------Execute Test---------------------------
            DotNetSourceRegion region = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act));
            region.SomethingChanged += (a, b) => { evt = true; };
            region.SelectedSource = s2;

            //------------Assert Results-------------------------
            Assert.IsTrue(evt);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetSourceRegion_ChangeSourceSomethingChanged")]
        public void DotNetSourceRegion_ChangeSourceSomethingChanged_RestoreRegion_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity() { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var pluginSrc = new PluginSourceDefinition() { Id = id, Name = "bob" };

            var s2 = new PluginSourceDefinition() { Id = Guid.NewGuid(), Name = "bob" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { pluginSrc, s2 });

            //------------Execute Test---------------------------
            DotNetSourceRegion region = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act));

            var clone1 = new Mock<IToolRegion>();
            var clone2 = new Mock<IToolRegion>();
            var dep1 = new Mock<IToolRegion>();
            dep1.Setup(a => a.CloneRegion()).Returns(clone1.Object);

            var dep2 = new Mock<IToolRegion>();
            dep2.Setup(a => a.CloneRegion()).Returns(clone2.Object);
            region.Dependants = new List<IToolRegion> { dep1.Object, dep2.Object };
            region.SelectedSource = s2;
            region.SelectedSource = pluginSrc;

            //------------Assert Results-------------------------
            dep1.Verify(a => a.RestoreRegion(clone1.Object), Times.Never);
            dep2.Verify(a => a.RestoreRegion(clone2.Object), Times.Never);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetSourceRegion_ChangeSourceSomethingChanged")]
        public void DotNetSourceRegion_ChangeSourceSomethingChanged_RegionsNotRestored_Invalid()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity() { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var pluginSrc = new PluginSourceDefinition() { Id = id };

            var s2 = new PluginSourceDefinition() { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { pluginSrc, s2 });

            //------------Execute Test---------------------------
            DotNetSourceRegion region = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act));

            var clone1 = new Mock<IToolRegion>();
            var clone2 = new Mock<IToolRegion>();
            var dep1 = new Mock<IToolRegion>();
            dep1.Setup(a => a.CloneRegion()).Returns(clone1.Object);

            var dep2 = new Mock<IToolRegion>();
            dep2.Setup(a => a.CloneRegion()).Returns(clone2.Object);
            region.Dependants = new List<IToolRegion> { dep1.Object, dep2.Object };
            region.SelectedSource = s2;
            region.SelectedSource = pluginSrc;

            //------------Assert Results-------------------------
            dep1.Verify(a => a.RestoreRegion(clone1.Object), Times.Never);
            dep2.Verify(a => a.RestoreRegion(clone2.Object), Times.Never);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetSourceRegion_ChangeSourceSomethingChanged")]
        public void DotNetSourceRegion_ChangeSourceSomethingChanged_CloneRegion_ExpectedClone()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity() { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var pluginSrc = new PluginSourceDefinition() { Id = id };
            var s2 = new PluginSourceDefinition() { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { pluginSrc, s2 });

            //------------Execute Test---------------------------
            DotNetSourceRegion region = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act));
            var cloned = region.CloneRegion();

            //------------Assert Results-------------------------
            Assert.AreEqual(cloned.IsEnabled, region.IsEnabled);
            Assert.AreEqual(((DotNetSourceRegion)cloned).SelectedSource, region.SelectedSource);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DotNetSourceRegion_ChangeSourceSomethingChanged")]
        public void DotNetSourceRegion_ChangeSourceSomethingChanged_RestoreRegion_ExpectedRestore()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfPluginActivity() { SourceId = id };
            var src = new Mock<IPluginServiceModel>();
            var pluginSrc = new PluginSourceDefinition() { Id = id };
            var s2 = new PluginSourceDefinition() { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>() { pluginSrc, s2 });

            //------------Execute Test---------------------------
            DotNetSourceRegion region = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act));
            
            DotNetSourceRegion regionToRestore = new DotNetSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act));
            regionToRestore.IsEnabled = false;
            regionToRestore.SelectedSource = s2;

            region.RestoreRegion(regionToRestore);

            //------------Assert Results-------------------------
            Assert.AreEqual(region.SelectedSource, s2);
            Assert.IsFalse(region.IsEnabled);
        }
    }
}
