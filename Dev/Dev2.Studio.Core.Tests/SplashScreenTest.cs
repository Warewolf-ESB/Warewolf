
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Studio.StartupResources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    /// <summary>
    /// Summary description for SplashScreenTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SplashScreenTest
    {

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
        [TestInitialize()]
        public void MyTestInitialize()
        {

        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //f
        #endregion

        #region Show Tests

        /// <summary>
        /// Tests that the splash image is never removed from the Studio project
        /// </summary>
        [TestMethod]
        public void SplashScreenTest_ImageAvailable_Test()
        {
            try
            {
                Dev2SplashScreen.Show();
            }
            catch(TypeInitializationException)
            {
                Assert.Fail("Splash Screen on startup failed");
            }
            Dev2SplashScreen.Close(TimeSpan.FromSeconds(1));
            Assert.IsTrue(true);
        }

        #endregion Show Tests
    }
}
