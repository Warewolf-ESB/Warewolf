using Dev2.Core.Tests.Utils;
using Dev2.Studio.CustomControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.Core.Tests.Custom_Dev2_Controls
{
    /// <summary>
    /// Summary description for Dev2FilterBoxTests
    /// </summary>
    [TestClass]
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
        public void Dev2FiterTextBoxWhenClickingButtonExpectedTextBoxToBeCleared()
        {
            Dev2FilterTextBox filterTextBox = new Dev2FilterTextBox();
            filterTextBox.CreateVisualTree();
            filterTextBox.FilterTextBox.Text = "TestData";
            filterTextBox.FilterButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.IsTrue(string.IsNullOrEmpty(filterTextBox.FilterTextBox.Text));
        }
    }
}
