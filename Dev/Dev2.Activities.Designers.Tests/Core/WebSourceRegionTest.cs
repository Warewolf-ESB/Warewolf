using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Interfaces.WebService;
using Moq;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ToolBase;

// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.Core
{
    /// <summary>
    /// Summary description for SharepointReadListTests
    /// </summary>
    [TestClass]
    public class WebSourceRegionTest
    {


        [TestMethod]
        public void Ctor()
        {
            var src = new Mock<IWebServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion region = new WebSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfWebGetActivity()));
            Assert.AreEqual(1,region.Errors.Count);
            Assert.IsTrue(region.IsEnabled);
        }
        [TestMethod]
        public void CtorWitSelectedSrc()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceModel>();
            var websrc = new WebServiceSourceDefinition() { Id = id };
            src.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource> { websrc});
            WebSourceRegion region = new WebSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act));
            Assert.AreEqual(websrc, region.SelectedSource);
            Assert.IsTrue(region.CanEditSource());
        }

        [TestMethod]
        public void ChangeSrcExpectSomethingChanged()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceModel>();
            var websrc = new WebServiceSourceDefinition() { Id = id };
            var Evt = false;
            var s2 = new WebServiceSourceDefinition() { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource> { websrc ,s2});
            WebSourceRegion region = new WebSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act));
            region.SomethingChanged += (a, b) => { Evt = true; };
            region.SelectedSource = s2;
            Assert.IsTrue(Evt);
        }


        [TestMethod]
        public void ChangeSelectedSource_ExpectRegionsRestored()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceModel>();
            var websrc = new WebServiceSourceDefinition() { Id = id ,HostName = "bob"};

            var s2 = new WebServiceSourceDefinition() { Id = Guid.NewGuid(), HostName = "bob" };
            src.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource> { websrc, s2 });
            WebSourceRegion region = new WebSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act));

            var clone1 = new Mock<IToolRegion>();
            var clone2 = new Mock<IToolRegion>();
            var dep1 = new Mock<IToolRegion>();
            dep1.Setup(a => a.CloneRegion()).Returns(clone1.Object);
         
            var dep2 = new Mock<IToolRegion>();
            dep2.Setup(a => a.CloneRegion()).Returns(clone2.Object);
            region.Dependants = new List<IToolRegion>{dep1.Object,dep2.Object};
            region.SelectedSource = s2;
            region.SelectedSource = websrc;
            dep1.Verify(a=>a.RestoreRegion(clone1.Object));
            dep2.Verify(a => a.RestoreRegion(clone2.Object));
        }

        [TestMethod]
        public void ChangeSelectedSource_ExpectRegionsNotRestoredInvalid()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceModel>();
            var websrc = new WebServiceSourceDefinition() { Id = id };

            var s2 = new WebServiceSourceDefinition() { Id = Guid.NewGuid()};
            src.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource> { websrc, s2 });
            WebSourceRegion region = new WebSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act));

            var clone1 = new Mock<IToolRegion>();
            var clone2 = new Mock<IToolRegion>();
            var dep1 = new Mock<IToolRegion>();
            dep1.Setup(a => a.CloneRegion()).Returns(clone1.Object);

            var dep2 = new Mock<IToolRegion>();
            dep2.Setup(a => a.CloneRegion()).Returns(clone2.Object);
            region.Dependants = new List<IToolRegion> { dep1.Object, dep2.Object };
            region.SelectedSource = s2;
            region.SelectedSource = websrc;
            dep1.Verify(a => a.RestoreRegion(clone1.Object),Times.Never);
            dep2.Verify(a => a.RestoreRegion(clone2.Object), Times.Never);
        }

        [TestMethod]
        public void CloneRegionExpectClone()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceModel>();
            var websrc = new WebServiceSourceDefinition() { Id = id };
            var s2 = new WebServiceSourceDefinition() { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource> { websrc, s2 });
            WebSourceRegion region = new WebSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act));
            var cloned = region.CloneRegion();
            Assert.AreEqual(((WebSourceRegion) cloned).SelectedSource,region.SelectedSource);
        }

        [TestMethod]
        public void Restore_Region_ExpectRestore()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceModel>();
            var websrc = new WebServiceSourceDefinition() { Id = id };
            var s2 = new WebServiceSourceDefinition() { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource> { websrc, s2 });
            WebSourceRegion region = new WebSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act));
            // ReSharper disable once UseObjectOrCollectionInitializer
            WebSourceRegion regionToRestore = new WebSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act));
            regionToRestore.SelectedSource = s2;

            region.RestoreRegion(regionToRestore);

            Assert.AreEqual(region.SelectedSource,s2);
        }
    }
}
