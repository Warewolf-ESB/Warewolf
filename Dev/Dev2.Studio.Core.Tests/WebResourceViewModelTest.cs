using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests {
    /// <summary>
    /// Summary description for WebResourceViewModelTest
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class WebResourceViewModelTest {
        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional result attributes
        // Use TestInitialize to run code before running each result 
        [TestInitialize]
        public void MyTestInitialize() 
        { 
            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();
        }

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

            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            ReferenceEquals(tmpChild.Object, tmp.Children.First());
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        #endregion AddChild Tests

        #region SetParent Tests

        [TestMethod]
        public void SetParent_Expected_ChangesTheParentToTheParentPassedIn() {
            WebResourceViewModel tmp = new WebResourceViewModel(null);
            Mock<IWebResourceViewModel> tmpParent = new Mock<IWebResourceViewModel>();

            tmp.SetParent(tmpParent.Object);

            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            ReferenceEquals(tmpParent.Object, tmp.Parent);
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        #endregion SetParent Tests

    }
}
