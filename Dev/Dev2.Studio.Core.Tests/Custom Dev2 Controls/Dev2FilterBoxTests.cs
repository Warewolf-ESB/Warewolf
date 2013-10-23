using Dev2.Core.Tests.Utils;
using Dev2.CustomControls;
using Dev2.Studio.CustomControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.Core.Tests.Custom_Dev2_Controls
{
    /// <summary>
    /// Summary description for Dev2FilterBoxTests
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    [Ignore]
    public class Dev2FilterBoxTests
    {
        public Dev2FilterBoxTests()
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

        [TestMethod]
        public void Dev2FiterTextBoxWhenClickingButtonExpectedTextBoxToBeCleared()
        {
            FilterTextBox filterTextBox = new FilterTextBox();
            filterTextBox.CreateVisualTree();
            filterTextBox.TheTextBox.Text = "TestData";
            filterTextBox.FilterButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.IsTrue(string.IsNullOrEmpty(filterTextBox.TheTextBox.Text));
        }
    }
}
