using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.WebService;
using Moq;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Core;

namespace Dev2.Activities.Designers.Tests.Core
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class WebSourceRegionTest
    {
        public WebSourceRegionTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Ctor()
        {
            var src = new Mock<IWebServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource>());
            WebSourceRegion region = new WebSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfWebGetActivity()));
            Assert.AreEqual(region.CurrentHeight, 20);
            Assert.AreEqual(region.MaxHeight, 20);
            Assert.AreEqual(region.MinHeight, 20);
            Assert.AreEqual(region.Errors.Count,1);
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
    }

    [TestClass]
    public class WebInputRegionTest
    {

        [TestMethod]
        public void TestInputCtor()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceSource>();

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource> {  });
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebGetActivity()));
            var region = new WebGetInputRegion( ModelItemUtils.CreateModelItem(act),srcreg);
            Assert.AreEqual(region.MaxHeight,195);
            Assert.AreEqual(region.MinHeight, 300);
            Assert.AreEqual(region.CurrentHeight, 300);
            Assert.AreEqual(region.IsVisible, false);

        }

        [TestMethod]
        public void TestInputAddHeaderExpectHeightChanges()
        {
            var id = Guid.NewGuid();
            var act = new DsfWebGetActivity() { SourceId = id };
            var src = new Mock<IWebServiceSource>();

            var mod = new Mock<IWebServiceModel>();
            mod.Setup(a => a.RetrieveSources()).Returns(new List<IWebServiceSource> { });
            WebSourceRegion srcreg = new WebSourceRegion(mod.Object, ModelItemUtils.CreateModelItem(new DsfWebGetActivity()));
            var region = new WebGetInputRegion(ModelItemUtils.CreateModelItem(act), srcreg);
            Assert.AreEqual(region.MaxHeight, 195);
            Assert.AreEqual(region.MinHeight, 300);
            Assert.AreEqual(region.CurrentHeight, 300);
            Assert.AreEqual(region.IsVisible, false);
            region.Headers.Add(new NameValue());
            Assert.AreEqual(region.MaxHeight, 240);
            Assert.AreEqual(region.MinHeight, 300);
            Assert.AreEqual(region.CurrentHeight, 300);
        }
    }
}
