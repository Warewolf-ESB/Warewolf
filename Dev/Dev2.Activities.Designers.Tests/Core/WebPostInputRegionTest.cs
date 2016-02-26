using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class WebPostInputRegionTest
    {

        [TestMethod]
        public void TestInputCtor()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceSource>();

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebGetActivity()));
            var region = new WebPostInputRegion( ModelItemUtils.CreateModelItem(act),srcreg);
            Assert.AreEqual(region.MaxHeight,265);
            Assert.AreEqual(region.MinHeight, 265);
            Assert.AreEqual(region.CurrentHeight, 265);
            Assert.AreEqual(region.IsVisible, false);
            Assert.AreEqual(region.HeadersHeight, 60);
            Assert.AreEqual(region.Errors.Count,0);

        }

        [TestMethod]
        public void TestInputCtorEmpty()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceSource>();

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebGetActivity()));
            var region = new WebPostInputRegion();
            Assert.AreEqual(region.MaxHeight, 265);
            Assert.AreEqual(region.MinHeight, 265);
            Assert.AreEqual(region.CurrentHeight, 265);
            Assert.AreEqual(region.IsVisible, false);


        }


        [TestMethod]
        public void TestClone()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceSource>();

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebGetActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), srcreg);
            region.PostData = "bob";
            Assert.AreEqual(region.MaxHeight, 265);
            Assert.AreEqual(region.MinHeight, 265);
            Assert.AreEqual(region.CurrentHeight, 265);
            Assert.AreEqual(region.IsVisible, false);
            Assert.AreEqual(region.HeadersHeight, 60);
            Assert.AreEqual(region.Errors.Count, 0);
            var clone = region.CloneRegion() as WebPostInputRegion;
            if(clone != null)
            {
                Assert.AreEqual(clone.MaxHeight, 265);
                Assert.AreEqual(clone.MinHeight, 265);
                Assert.AreEqual(clone.CurrentHeight, 265);
                Assert.AreEqual(clone.IsVisible, false);
                Assert.AreEqual(clone.HeadersHeight, 265);
                Assert.AreEqual(clone.Errors.Count, 0);
                Assert.AreEqual(clone.PostData,"bob");
            }
        }


        [TestMethod]
        public void Test_HeightChangedUpdatesMain()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceSource>();
            bool Called = false;
            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebGetActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), srcreg);
            region.HeightChanged += (a, b) => { Called = true; };
            region.Headers.Add(new NameValue());
            Assert.AreEqual(region.MaxHeight, 295);
            Assert.IsTrue(Called);
        }

        [TestMethod]
        public void TestInputAddHeaderExpectHeightChanges()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceSource>();

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebGetActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), srcreg);
            Assert.AreEqual(region.MaxHeight, 265);
            Assert.AreEqual(region.MinHeight, 265);
            Assert.AreEqual(region.CurrentHeight, 265);
            Assert.AreEqual(region.IsVisible, false);
            region.Headers.Add(new NameValue());
            Assert.AreEqual(region.MaxHeight, 295);
            Assert.AreEqual(region.MinHeight, 295);
            Assert.AreEqual(region.CurrentHeight, 295);
        }

        [TestMethod]
        public void TestInputAddHeaderExpectHeightChangesPastThree()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceSource>();

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebGetActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), srcreg);
            Assert.AreEqual(region.MaxHeight, 265);
            Assert.AreEqual(region.MinHeight, 265);
            Assert.AreEqual(region.CurrentHeight, 265);
            Assert.AreEqual(region.IsVisible, false);
            region.Headers.Add(new NameValue());
            region.Headers.Add(new NameValue());
            region.Headers.Add(new NameValue());
            region.Headers.Add(new NameValue());
            Assert.AreEqual(region.MaxHeight, 325);
            Assert.AreEqual(region.MinHeight, 325);
            Assert.AreEqual(region.CurrentHeight, 325);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WebInputRegion_RestoreFromPrevios")]
        public void WebInputRegion_RestoreFromPrevios_Restore_ExpectValuesChanged()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceSource>();

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebGetActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), srcreg);
            var regionToRestore = new WebPostInputRegionClone();
            regionToRestore.MinHeight = 265;
            regionToRestore.MaxHeight = 265;
            regionToRestore.CurrentHeight = 265;
            regionToRestore.IsVisible = true;
            regionToRestore.QueryString = "blob";
            regionToRestore.Headers = new ObservableCollection<INameValue>{new NameValue("a","b")};
            //------------Execute Test---------------------------
            region.RestoreRegion(regionToRestore);
            //------------Assert Results-------------------------

            Assert.AreEqual(region.MaxHeight, 265);
            Assert.AreEqual(region.MinHeight, 265);
            Assert.AreEqual(region.CurrentHeight, 265);
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
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceSource>();

            var mod = new Mock<IWebServiceModel>();
            var  lst = new List<IWebServiceSource> { new WebServiceSourceDefinition(){HostName = "bob",DefaultQuery = "Dave"} , new WebServiceSourceDefinition(){HostName = "f",DefaultQuery = "g"} };
            mod.Setup(a => a.RetrieveSources()).Returns(lst);
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebGetActivity()));
            var region = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), srcreg);
            var regionToRestore = new WebPostInputRegion(ModelItemUtils.CreateModelItem(act), srcreg);

            srcreg.SelectedSource = lst[0];
            Assert.AreEqual(region.QueryString,"Dave");
            Assert.AreEqual(region.RequestUrl, "bob");

        }

    }
}