using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Dev2.Studio.Core.Account;

namespace Dev2.Core.Tests.Account
{
    /// <summary>
    /// Summary description for UserAccountProviderTestst
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class UserAccountProviderTests
    {
        public UserAccountProviderTests()
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
        public void DefaultUserAccountProvider_Expected_NewGuidCreateForUserName()
        {
            IUserAccountProvider UserAccount = new UserAccountProvider();
            Guid userName = Guid.Empty;
            Assert.IsTrue(Guid.TryParse(UserAccount.UserName, out userName));
        }

        [TestMethod]
        public void NewUserAccountWithValidDetails_Expected_UserAccountInformationGenerated()
        {
            string userName = "Tester";
            string password = "test123";
            IUserAccountProvider UserAccount = new UserAccountProvider(userName, password);
            
            Assert.AreEqual(userName, UserAccount.UserName);
            Assert.AreEqual(password, UserAccount.Password);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NewUserAccountWithEmptyUserNameDetails_Expected_ArgumentNullExceptionThrown()
        {
            string userName = string.Empty;
            string password = "test123";
            IUserAccountProvider UserAccount = new UserAccountProvider(userName, password);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NewUserAccountWithEmptyPasswordDetails_Expected_ArgumentNullExceptionThrown()
        {
            string userName = "Tester";
            string password = string.Empty;
            IUserAccountProvider UserAccount = new UserAccountProvider(userName, password);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NewUserAccountWithEmptyUserNameAndPasswordDetails_Expected_ArgumentNullExceptionThrown()
        {
            string userName = string.Empty;
            string password = string.Empty;
            IUserAccountProvider UserAccount = new UserAccountProvider(userName, password);
        }
    }
}
