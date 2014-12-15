
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    /// <summary>
    /// Summary description for WebResourceViewModelTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WebResourceViewModelTest
    {
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
        }

        #endregion

        #region Ctor Test

        [TestMethod]

        public void ConstructorTestWithNullRoot()
        {
            WebResourceViewModel tmp = new WebResourceViewModel(null);

            Assert.IsNull(tmp.Parent);
        }

        [TestMethod]
        public void ConstructorTestWithNullRootChildrenContainerNotNull()
        {
            WebResourceViewModel tmp = new WebResourceViewModel(null);

            Assert.IsNotNull(tmp.Children);
        }

        #endregion

        #region AddChild Tests
        // ReSharper disable InconsistentNaming

        [TestMethod]
        public void AddChild_Expected_ObjectAddedToResourceViewModel()
        {
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
        public void SetParent_Expected_ChangesTheParentToTheParentPassedIn()
        {
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
