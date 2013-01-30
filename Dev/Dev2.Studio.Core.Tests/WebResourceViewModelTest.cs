using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Moq;

namespace Dev2.Core.Tests.ViewModelTest {
    /// <summary>
    /// Summary description for WebResourceViewModelTest
    /// </summary>
    [TestClass]
    public class WebResourceViewModelTest {

        private TestContext testContextInstance; 

        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional result attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first result in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each result 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each result has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Ctor Test
        
        [TestMethod]
        public void ConstructorTestWithNullRoot() {
            WebResourceViewModel tmp = new WebResourceViewModel(null);

            Assert.IsNull(tmp.Parent);
        }

        [TestMethod]
        public void ConstructorTestWithNullRootChildrenContainerNotNull() {
            WebResourceViewModel tmp = new WebResourceViewModel(null);

            Assert.IsNotNull(tmp.Children);
        }
        
        #endregion

        #region AddChild Tests

        [TestMethod]
        public void AddChild_Expected_ObjectAddedToResourceViewModel() {
            WebResourceViewModel tmp = new WebResourceViewModel(null);
            Mock<IWebResourceViewModel> tmpChild = new Mock<IWebResourceViewModel>();

            tmp.AddChild(tmpChild.Object);

            Assert.ReferenceEquals(tmpChild.Object, tmp.Children.First());
        }

        #endregion AddChild Tests

        #region SetParent Tests

        [TestMethod]
        public void SetParent_Expected_ChangesTheParentToTheParentPassedIn() {
            WebResourceViewModel tmp = new WebResourceViewModel(null);
            Mock<IWebResourceViewModel> tmpParent = new Mock<IWebResourceViewModel>();

            tmp.SetParent(tmpParent.Object);

            Assert.ReferenceEquals(tmpParent.Object, tmp.Parent);
        }

        #endregion SetParent Tests

    }
}
