using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Input;

namespace Dev2.Common.Tests.Core
{
    [TestClass]
    public class ServerSourceTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_ServerName()
        {
            var testServerName = "TestServerName";
            var serverSource = new ServerSource();

            serverSource.ServerName = testServerName;

            Assert.AreEqual(testServerName, serverSource.ServerName);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_Address()
        {
            var testAddress = "https://ddkksfsw:3143";
            var serverSource = new ServerSource();

            serverSource.Address = testAddress;

            Assert.AreEqual(testAddress, serverSource.Address);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_AuthenticationType()
        {
            var expectedAuthenticationType = AuthenticationType.User;
            var serverSource = new ServerSource();

            serverSource.AuthenticationType = expectedAuthenticationType;

            Assert.AreEqual(expectedAuthenticationType, serverSource.AuthenticationType);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_UserName()
        {
            var testUserName = "TestUserName";
            var serverSource = new ServerSource();

            serverSource.UserName = testUserName;

            Assert.AreEqual(testUserName, serverSource.UserName);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_Password()
        {
            var testPassword = "TestPassword";
            var serverSource = new ServerSource();

            serverSource.Password = testPassword;

            Assert.AreEqual(testPassword, serverSource.Password);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_TestCommand()
        {
            var serverSource = new ServerSource();
            serverSource.TestCommand = TestCommand;
            Assert.AreEqual(TestCommand, serverSource.TestCommand);
        }
        public ICommand TestCommand { get; set; }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_TestMessage()
        {
            var testMessage = "TestMessage";
            var serverSource = new ServerSource();

            serverSource.TestMessage = testMessage;

            Assert.AreEqual(testMessage, serverSource.TestMessage);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_ID()
        {
            var testID = Guid.NewGuid();
            var serverSource = new ServerSource();

            serverSource.ID = testID;

            Assert.AreEqual(testID, serverSource.ID);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_Name()
        {
            var testName = "TestName";
            var serverSource = new ServerSource();

            serverSource.Name = testName;

            Assert.AreEqual(testName, serverSource.Name);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_ResourcePath()
        {
            var resourcePath = "ResourcePath";
            var serverSource = new ServerSource();

            serverSource.ResourcePath = resourcePath;

            Assert.AreEqual(resourcePath, serverSource.ResourcePath);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_Equals_OtherIsNull_ReturnFalse()
        {
            //---------------Set up test pack-------------------
            var serverSource = new ServerSource
            {
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(serverSource.Equals(null), "Equals operator can't compare to null.");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_Equals_ReturnTrue()
        {
            //---------------Set up test pack-------------------
            var serverSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };
            var otherServerSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(serverSource.Equals(otherServerSource));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_Equals_ReturnFalse_NameNotEqual()
        {
            //---------------Set up test pack-------------------
            var serverSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };
            var otherServerSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest2"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(serverSource.Equals(otherServerSource));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_Equals_ReturnFalse_PasswordNotEqual()
        {
            //---------------Set up test pack-------------------
            var serverSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };
            var otherServerSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest",
                Password = "passwordTest2",
                Name = "nameTest"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(serverSource.Equals(otherServerSource));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_Equals_ReturnFalse_UsernameNotEqual()
        {
            //---------------Set up test pack-------------------
            var serverSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };
            var otherServerSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest2",
                Password = "passwordTest",
                Name = "nameTest"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(serverSource.Equals(otherServerSource));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_Equals_ReturnFalse_AuthenticationTypeNotEqual()
        {
            //---------------Set up test pack-------------------
            var serverSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.Public,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };
            var otherServerSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest2",
                Password = "passwordTest",
                Name = "nameTest"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(serverSource.Equals(otherServerSource));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_Equals_ReturnFalse_AddressNotEqual()
        {
            //---------------Set up test pack-------------------
            var serverSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.Public,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };
            var otherServerSource = new ServerSource
            {
                Address = "http://test-wrong.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest2",
                Password = "passwordTest",
                Name = "nameTest"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(serverSource.Equals(otherServerSource));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_IServerSource_Equals_ReturnFalse_AddressNotEqual()
        {
            //---------------Set up test pack-------------------

            IServerSource serverSource = new ServerSource()
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.Public,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };
            IServerSource otherServerSource = new ServerSource
            {
                Address = "http://test-wrong.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest2",
                Password = "passwordTest",
                Name = "nameTest"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(serverSource.Equals(otherServerSource));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_IServerSource_Equals_ReturnTrue()
        {
            //---------------Set up test pack-------------------
            IServerSource serverSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };
            IServerSource otherServerSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(serverSource.Equals(otherServerSource));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_IServerSource_Equals_ReturnFalse_NameNotEqual()
        {
            //---------------Set up test pack-------------------
            IServerSource serverSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };
            IServerSource otherServerSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest2"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(serverSource.Equals(otherServerSource));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_IServerSource_Equals_ReturnFalse_PasswordNotEqual()
        {
            //---------------Set up test pack-------------------
            IServerSource serverSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };
            IServerSource otherServerSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest",
                Password = "passwordTest2",
                Name = "nameTest"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(serverSource.Equals(otherServerSource));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_IServerSource_Equals_ReturnFalse_UsernameNotEqual()
        {
            //---------------Set up test pack-------------------
            IServerSource serverSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };
            IServerSource otherServerSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest2",
                Password = "passwordTest",
                Name = "nameTest"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(serverSource.Equals(otherServerSource));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_IServerSource_Equals_ReturnFalse_AuthenticationTypeNotEqual()
        {
            //---------------Set up test pack-------------------
            IServerSource serverSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.Public,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };
            IServerSource otherServerSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "usernameTest2",
                Password = "passwordTest",
                Name = "nameTest"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(serverSource.Equals(otherServerSource));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSource))]
        public void ServerSource_IServerSource_Equals_ReturnFalse_OtherIsNull()
        {
            //---------------Set up test pack-------------------
            IServerSource serverSource = new ServerSource
            {
                Address = "http://test.com",
                AuthenticationType = AuthenticationType.Public,
                UserName = "usernameTest",
                Password = "passwordTest",
                Name = "nameTest"
            };

            //---------------Assert Precondition----------------
            Assert.IsFalse(serverSource.Equals(null));
        }
    }
}
