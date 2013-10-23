using System;
using System.Collections.Generic;
using Dev2.Communication;
using Dev2.Data.Enums;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Runtime.Hosting;
using Dev2.Tests.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;

namespace Dev2.DynamicServices.Test
{
    /// <summary>
    /// Summary description for NetworkTest
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class NetworkTest
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
        /// Valid server created, access servers account provider with auto account creation turned off. Manually created a new account,
        /// Expect a non null account given the correct username
        /// </summary>
        [TestMethod]
        public void StudioNetworkServerObservesMessagesFromCompileMessageRepo()
        {
            var server = new MockStudioNetworkServer("test", new StudioFileSystem(Environment.CurrentDirectory + Guid.NewGuid(), new List<string>()), null, Guid.NewGuid(), false);
            var message = new CompileMessageTO { ServiceID = Guid.NewGuid(), ServiceName = "Test Service", MessageType = CompileMessageType.ResourceSaved };

            //exe
            CompileMessageRepo.Instance.AddMessage(Guid.NewGuid(), new[] { message });

            var memo = (DesignValidationMemo)server.WriteEventProviderMemos[0];

            Assert.IsNotNull(memo);
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

        [TestMethod]
        [TestCategory("StudioNetworkServerUnitTest")]
        [Description("Test for StudioNetworkServer's 'UpdateMappingChangedMemo' method: A valid memo and message is passed to StudioNetworkServer and the memo is updated with the message error")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void StudioNetworkServer_StudioNetworkServerUnitTest_UpdateMappingChangedMemo_MemoUpdated()
        // ReSharper restore InconsistentNaming
        {
            //init
            var server = new MockStudioNetworkServer("test", new StudioFileSystem(Environment.CurrentDirectory + Guid.NewGuid(), new List<string>()), null, Guid.NewGuid(), false);
            var message = new CompileMessageTO { UniqueID = Guid.NewGuid(), ServiceID = Guid.NewGuid(), WorkspaceID = Guid.NewGuid(), MessageType = CompileMessageType.MappingChange, MessagePayload = "Test Error Message", ServiceName = "Test Service" };

            //exe
            server.TestOnCompilerMessageReceived(new[] { message });

            //asserts
            Assert.AreEqual(2, server.WriteEventProviderMemos.Count);
            foreach(DesignValidationMemo memo in server.WriteEventProviderMemos)
            {
                Assert.AreEqual(message.ServiceID, memo.ServiceID, "Memo service name not updated with compiler message service name");
                Assert.AreEqual(message.WorkspaceID, memo.WorkspaceID, "Memo workspace ID not updated");
                Assert.IsFalse(memo.IsValid, "Error memo not invalidated");
                Assert.AreEqual(1, memo.Errors.Count, "The wrong number of errors was added to the memo");
                Assert.AreEqual(message.MessagePayload, memo.Errors[0].FixData, "The wrong error fix data was added to the memo");
                Assert.AreEqual(message.UniqueID, memo.Errors[0].InstanceID, "The error instacen ID was added to the memo");
            }

            var serviceMemo = (DesignValidationMemo)server.WriteEventProviderMemos[0];
            var unqiueMemo = (DesignValidationMemo)server.WriteEventProviderMemos[1];
            Assert.AreEqual(message.UniqueID, unqiueMemo.InstanceID, "Memo ID not updated with compiler message unique ID");
            Assert.AreEqual(message.ServiceID, serviceMemo.InstanceID, "Memo ID not updated with compiler message service ID");
        }

        [TestMethod]
        [TestCategory("StudioNetworkServerUnitTest")]
        [Description("Test for StudioNetworkServer's 'UpdateResourceSavedMemo' method: A valid memo and message is passed to StudioNetworkServer and the memo is updated with the message error")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void StudioNetworkServer_StudioNetworkServerUnitTest_UpdateResourceSavedMemo_MemoUpdated()
        // ReSharper restore InconsistentNaming
        {
            //init
            var server = new MockStudioNetworkServer("test", new StudioFileSystem(Environment.CurrentDirectory + Guid.NewGuid(), new List<string>()), null, Guid.NewGuid(), false);
            var message = new CompileMessageTO { ServiceID = Guid.NewGuid(), WorkspaceID = Guid.NewGuid(), MessageType = CompileMessageType.ResourceSaved, ServiceName = "Test Service" };

            //exe
            server.TestOnCompilerMessageReceived(new[] { message });


            //asserts
            Assert.AreEqual(1, server.WriteEventProviderMemos.Count);

            var memo = (DesignValidationMemo)server.WriteEventProviderMemos[0];
            Assert.AreEqual(message.ServiceID, memo.InstanceID, "Memo ID not updated with compiler message service ID");
            Assert.AreEqual(message.ServiceID, memo.ServiceID, "Memo service name not updated with compiler message service name");
            Assert.AreEqual(message.WorkspaceID, memo.WorkspaceID, "Memo workspace ID not updated");
            Assert.IsTrue(memo.IsValid, "Resource saved with invalid memo");
        }

    }
}
