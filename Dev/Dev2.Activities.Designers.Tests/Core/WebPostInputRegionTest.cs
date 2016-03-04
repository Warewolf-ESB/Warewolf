using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebService;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.Core
{
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class WebPostInputRegionTest
    {
        [TestMethod]
        public void TestInputCtor()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebPostActivity() { SourceId = id };

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebPostActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), srcreg);
            Assert.AreEqual(region.MaxHeight, 160);
            Assert.AreEqual(region.MinHeight, 160);
            Assert.AreEqual(region.CurrentHeight, 160);
            Assert.AreEqual(region.IsVisible, false);
            Assert.AreEqual(region.HeadersHeight, 60);
            Assert.AreEqual(region.Errors.Count, 0);
        }

        [TestMethod]
        public void TestInputCtorEmpty()
        {
            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            var region = new WebPostInputRegion();
            Assert.AreEqual(region.MaxHeight, 160);
            Assert.AreEqual(region.MinHeight, 160);
            Assert.AreEqual(region.CurrentHeight, 160);
            Assert.AreEqual(region.IsVisible, false);
        }

        [TestMethod]
        public void TestClone()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebPostActivity() { SourceId = id };

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebPostActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), srcreg) { PostData = "bob" };
            Assert.AreEqual(region.MaxHeight, 160);
            Assert.AreEqual(region.MinHeight, 160);
            Assert.AreEqual(region.CurrentHeight, 160);
            Assert.AreEqual(region.IsVisible, false);
            Assert.AreEqual(region.HeadersHeight, 60);
            Assert.AreEqual(region.Errors.Count, 0);
            var clone = region.CloneRegion() as WebPostInputRegion;
            if (clone != null)
            {
                Assert.AreEqual(clone.MaxHeight, 265);
                Assert.AreEqual(clone.MinHeight, 265);
                Assert.AreEqual(clone.CurrentHeight, 265);
                Assert.AreEqual(clone.IsVisible, false);
                Assert.AreEqual(clone.HeadersHeight, 265);
                Assert.AreEqual(clone.Errors.Count, 0);
                Assert.AreEqual(clone.PostData, "bob");
            }
        }

        [TestMethod]
        public void Test_HeightChangedUpdatesMain()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebPostActivity() { SourceId = id };
            bool Called = false;
            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebPostActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), srcreg);
            region.HeightChanged += (a, b) => { Called = true; };
            region.Headers.Add(new NameValue());
            Assert.AreEqual(region.MaxHeight, 190);
            Assert.IsTrue(Called);
        }

        [TestMethod]
        public void TestInputAddHeaderExpectHeightChanges()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebPostActivity() { SourceId = id };

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebPostActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), srcreg);
            Assert.AreEqual(region.MaxHeight, 160);
            Assert.AreEqual(region.MinHeight, 160);
            Assert.AreEqual(region.CurrentHeight, 160);
            Assert.AreEqual(region.IsVisible, false);
            region.Headers.Add(new NameValue());
            Assert.AreEqual(region.MaxHeight, 190);
            Assert.AreEqual(region.MinHeight, 190);
            Assert.AreEqual(region.CurrentHeight, 190);
        }

        [TestMethod]
        public void TestInputAddHeaderExpectHeightChangesPastThree()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebPostActivity() { SourceId = id };

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebPostActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), srcreg);
            Assert.AreEqual(region.MaxHeight, 160);
            Assert.AreEqual(region.MinHeight, 160);
            Assert.AreEqual(region.CurrentHeight, 160);
            Assert.AreEqual(region.IsVisible, false);
            region.Headers.Add(new NameValue());
            region.Headers.Add(new NameValue());
            region.Headers.Add(new NameValue());
            region.Headers.Add(new NameValue());
            Assert.AreEqual(region.MaxHeight, 220);
            Assert.AreEqual(region.MinHeight, 220);
            Assert.AreEqual(region.CurrentHeight, 220);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WebInputRegion_RestoreFromPrevios")]
        public void WebInputRegion_RestoreFromPrevios_Restore_ExpectValuesChanged()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfWebPostActivity() { SourceId = id };

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebPostActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), srcreg);
            var regionToRestore = new WebPostInputRegionClone
            {
                MinHeight = 265,
                MaxHeight = 265,
                CurrentHeight = 265,
                IsVisible = true,
                QueryString = "blob",
                Headers = new ObservableCollection<INameValue> { new NameValue("a", "b") }
            };
            //------------Execute Test---------------------------
            region.RestoreRegion(regionToRestore);
            //------------Assert Results-------------------------

            Assert.AreEqual(region.MaxHeight, 160);
            Assert.AreEqual(region.MinHeight, 160);
            Assert.AreEqual(region.CurrentHeight, 160);
            Assert.AreEqual(region.QueryString, "blob");
            Assert.AreEqual(region.Headers.First().Name, "a");
            Assert.AreEqual(region.Headers.First().Value, "b");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WebInputRegion_RestoreFromPrevios")]
        public void WebInputRegion_SrcChanged_UpdateValues()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfWebPostActivity() { SourceId = id };

            var mod = new Mock<IWebServiceModel>();
            var lst = new List<IWebServiceSource> { new WebServiceSourceDefinition() { HostName = "bob", DefaultQuery = "Dave" }, new WebServiceSourceDefinition() { HostName = "f", DefaultQuery = "g" } };
            mod.Setup(a => a.RetrieveSources()).Returns(lst);
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebPostActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), srcreg);

            srcreg.SelectedSource = lst[0];
            Assert.AreEqual(region.QueryString, "Dave");
            Assert.AreEqual(region.RequestUrl, "bob");
        }
    }
}