using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.DynamicServices;

namespace Dev2.DynamicServices.Test
{
    /// <summary>
    /// Summary description for NetworkTest
    /// </summary>
    [TestClass]
    [Ignore]
    public class NetworkTest
    {
        public NetworkTest()
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
        [ExpectedException(typeof(ArgumentException))]
        public void FileSystem_Constructor_Test()
        {
            new StudioFileSystem("", new List<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FileSystem_Constructor_InvalidPath_Test()
        {
            char[] invalid = System.IO.Path.GetInvalidPathChars();
            new StudioFileSystem("C:\\abc" + invalid[0] + "asd\\", new List<string>());
        }

        [TestMethod]
        public void FileSystem_GetEnsuredPath_File_ExpectNull_Test()
        {
            StudioFileSystem file = new StudioFileSystem(Environment.CurrentDirectory + Guid.NewGuid(), new List<string>());
            string relativePath = file.GetEnsuredPath("relativePath.temp");
            Assert.AreEqual<string>(null, relativePath);
        }

        [TestMethod]
        public void FileSystem_GetEnsuredPath_Directory_ExpectNull_Test()
        {
            StudioFileSystem file = new StudioFileSystem(Environment.CurrentDirectory + Guid.NewGuid(), new List<string>());
            string relativePath = file.GetEnsuredPath("relativeDirectory");
            Assert.AreEqual<string>(null, relativePath);
        }

        [TestMethod]
        public void FileSystem_GetRelativePath_ExpectNotNull_Test()
        {
            StudioFileSystem file = new StudioFileSystem(Environment.CurrentDirectory + Guid.NewGuid(), new List<string>());
            string relativePath = file.GetRelativePath("relativePath.temp");
            Assert.AreNotEqual<string>(null, relativePath);
        }

        [TestMethod]
        public void FileSystem_GetRelativePath_ExpectRooted_Test()
        {
            StudioFileSystem file = new StudioFileSystem(Environment.CurrentDirectory + Guid.NewGuid(), new List<string>());
            string relativePath = file.GetRelativePath("relativePath.temp");
            Assert.AreEqual<bool>(true, System.IO.Path.IsPathRooted(relativePath));
        }

        /// <summary>
        /// Null server provided to constructor, constructor must throw an ArgumentNullException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AccountSystem_Constructor_Test()
        {
            new StudioAccountProvider("", false, null);
        }

        /// <summary>
        /// Valid server created, access servers account provider with auto account creation turned on. Expect a non null account in all situations.
        /// </summary>
        [TestMethod]
        public void AccountSystem_AutoAccountCreation_ExpectNotNull_Test()
        {
            StudioNetworkServer server = new StudioNetworkServer("test", new StudioFileSystem(Environment.CurrentDirectory + Guid.NewGuid(), new List<string>()), null, Guid.NewGuid());
            server.AccountProvider.ClearAccounts();
            StudioAccount account = server.AccountProvider.GetAccount("testaccount");
            server.Dispose();

            Assert.AreNotEqual<StudioAccount>(null, account);
        }

        /// <summary>
        /// Valid server created, access servers account provider with auto account creation turned on. Expect the same account for the same name in all situations.
        /// </summary>
        [TestMethod]
        public void AccountSystem_AutoAccountCreation_ExpectEqual_Test()
        {
            StudioNetworkServer server = new StudioNetworkServer("test", new StudioFileSystem(Environment.CurrentDirectory + Guid.NewGuid(), new List<string>()), null, Guid.NewGuid());
            server.AccountProvider.ClearAccounts();
            StudioAccount account1 = server.AccountProvider.GetAccount("testaccount");
            StudioAccount account2 = server.AccountProvider.GetAccount("testaccount");

            server.Dispose();

            Assert.AreEqual<StudioAccount>(account1, account2);
        }

        /// <summary>
        /// Valid server created, access servers account provider with auto account creation turned on. Expect a different account for different names in all situations.
        /// </summary>
        [TestMethod]
        public void AccountSystem_AutoAccountCreation_ExpectNotEqual_Test()
        {
            StudioNetworkServer server = new StudioNetworkServer("test", new StudioFileSystem(Environment.CurrentDirectory + Guid.NewGuid(), new List<string>()), null, Guid.NewGuid());
            server.AccountProvider.ClearAccounts();
            StudioAccount account1 = server.AccountProvider.GetAccount("testaccount");
            StudioAccount account2 = server.AccountProvider.GetAccount("testaccount2");

            server.Dispose();

            Assert.AreNotEqual<StudioAccount>(account1, account2);
        }

        /// <summary>
        /// Valid server created, access servers account provider with auto account creation turned off. Expect a null account for any name given that no accounts have
        /// been added.
        /// </summary>
        [TestMethod]
        public void AccountSystem_NoAutoAccountCreation_ExpectNull_Test()
        {
            StudioNetworkServer server = new StudioNetworkServer("test", new StudioFileSystem(Environment.CurrentDirectory + Guid.NewGuid(), new List<string>()), null, Guid.NewGuid(), false);
            server.AccountProvider.ClearAccounts();
            StudioAccount account1 = server.AccountProvider.GetAccount("testaccount");

            server.Dispose();

            Assert.AreEqual<StudioAccount>(null, account1);
        }

        /// <summary>
        /// Valid server created, access servers account provider with auto account creation turned off. Manually created a new account,
        /// Expect a non null account given the correct username
        /// </summary>
        [TestMethod]
        public void AccountSystem_NoAutoAccountCreation_AccountManuallyCreated_ExpectNonNull_Test()
        {
            StudioNetworkServer server = new StudioNetworkServer("test", new StudioFileSystem(Environment.CurrentDirectory + Guid.NewGuid(), new List<string>()), null, Guid.NewGuid(), false);
            server.AccountProvider.ClearAccounts();
            server.AccountProvider.CreateAccount("TestAccount", "abcd123efg");
            StudioAccount account1 = server.AccountProvider.GetAccount("TestAccount");
            server.Dispose();
            Assert.AreNotEqual<StudioAccount>(null, account1);
        }

        /// <summary>
        /// Valid server created, access servers account provider with auto account creation turned off. Manually created a new account with a blank username,
        /// expecting an argument exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AccountSystem_NoAutoAccountCreation_AccountManuallyCreated_Username_ExpectException_Test()
        {
            StudioNetworkServer server = new StudioNetworkServer("test", new StudioFileSystem(Environment.CurrentDirectory + Guid.NewGuid(), new List<string>()), null, Guid.NewGuid(), false);
            server.AccountProvider.ClearAccounts();
            server.AccountProvider.CreateAccount("", "abcd123efg");
            server.Dispose();
        }

        /// <summary>
        /// Valid server created, access servers account provider with auto account creation turned off. Manually created a new account with a blank password,
        /// expecting an argument exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AccountSystem_NoAutoAccountCreation_AccountManuallyCreated_Password_ExpectException_Test()
        {
            StudioNetworkServer server = new StudioNetworkServer("test", new StudioFileSystem(Environment.CurrentDirectory + Guid.NewGuid(), new List<string>()), null, Guid.NewGuid(), false);
            server.AccountProvider.ClearAccounts();
            server.AccountProvider.CreateAccount("TestAccount", "");
            server.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudioNetwork_Constructor_Test()
        {
            new StudioNetworkServer("asd", null, null, Guid.NewGuid());
        }
    }
}
