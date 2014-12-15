
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
using System.Windows;
using System.Windows.Controls;
using Dev2.Util.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework;

namespace Dev2.Core.Tests.UtilsTests
{
    /// <summary>
    /// Summary description for NewWorkflowNamesTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FrameWorkElementExtensionsTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

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
        public void FrameWorkElementExtensions_UnitTest_BringToFrontWhereContainsNoElements_DoesNotError()
        {
            //------------Setup for test--------------------------
            var autoLayoutGrid = new AutoLayoutGrid();
            var frameworkElement = new FrameworkElement();
            autoLayoutGrid.Children.Add(frameworkElement);
            //------------Execute Test---------------------------
            frameworkElement.BringToFront();
            //------------Assert Results-------------------------
        }

        [TestMethod]
        public void FrameWorkElementExtensions_UnitTest_BringToFrontWhereElementHasParents_SetsZIndex()
        {
            //------------Setup for test--------------------------
            var autoLayoutGrid = new AutoLayoutGrid();
            var frameworkElement = new FrameworkElement();
            var backElementElement = new FrameworkElement();
            var backGrid = new AutoLayoutGrid();
            autoLayoutGrid.Children.Add(frameworkElement);
            backGrid.Children.Add(backElementElement);
            autoLayoutGrid.Children.Add(backGrid);
            //------------Execute Test---------------------------
            frameworkElement.BringToFront();
            //------------Assert Results-------------------------
            int zIndex = Panel.GetZIndex(frameworkElement);
            Assert.AreEqual(1, zIndex);
        }

        [TestMethod]
        public void FrameWorkElementExtensions_UnitTest_BringToFrontWhereNullParentElement_DoesNotError()
        {
            //------------Setup for test--------------------------
            var frameworkElement = new FrameworkElement();
            //------------Execute Test---------------------------
            frameworkElement.BringToFront();
            //------------Assert Results-------------------------
        }
    }
}
