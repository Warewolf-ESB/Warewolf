/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Dev2.Common.Tests.Core
{
    [TestClass]
    public class EmailServiceSourceDefinitionTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_Validate()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedHost = "testHost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testuser";
            const int expectedPort = 4040;
            const int expectedTimeout = 30;
            const string expectedResourceName = "testResourceName";
            const bool expectedEnableSsl = false;
            const string expectedEmailFrom = "testEmailFrom";
            const string expectedEmailTo = "testEmailTo";

            var mockEmailSource = new Mock<IEmailSource>();
            mockEmailSource.Setup(emailSource => emailSource.ResourceID).Returns(expectedResourceID);
            mockEmailSource.Setup(emailSource => emailSource.Host).Returns(expectedHost);
            mockEmailSource.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailSource.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailSource.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailSource.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailSource.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailSource.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            var emailServiceSourceDefinition = new EmailServiceSourceDefinition(mockEmailSource.Object)
            {
                EmailFrom = expectedEmailFrom,
                EmailTo = expectedEmailTo
            };

            Assert.AreEqual(expectedResourceID, emailServiceSourceDefinition.Id);
            Assert.AreEqual(expectedHost, emailServiceSourceDefinition.HostName);
            Assert.AreEqual(expectedPassword, emailServiceSourceDefinition.Password);
            Assert.AreEqual(expectedUserName, emailServiceSourceDefinition.UserName);
            Assert.AreEqual(string.Empty, emailServiceSourceDefinition.Path);
            Assert.AreEqual(expectedPort, emailServiceSourceDefinition.Port);
            Assert.AreEqual(expectedTimeout, emailServiceSourceDefinition.Timeout);
            Assert.AreEqual(expectedResourceName, emailServiceSourceDefinition.ResourceName);
            Assert.AreEqual(expectedEnableSsl, emailServiceSourceDefinition.EnableSsl);
            Assert.AreEqual(expectedEmailFrom, emailServiceSourceDefinition.EmailFrom);
            Assert.AreEqual(expectedEmailTo, emailServiceSourceDefinition.EmailTo);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_Equals_EmailServiceSource_Null_Expected_False()
        {
            var emailServiceSourceDefinition = new EmailServiceSourceDefinition();

            const IEmailServiceSource emailSource = null;

            var isEqual = emailServiceSourceDefinition.Equals(emailSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_Equals_EmailServiceSource_Expected_True()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedHost = "testHost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testuser";
            const int expectedPort = 4040;
            const int expectedTimeout = 30;
            const string expectedResourceName = "testResourceName";
            const bool expectedEnableSsl = false;

            var mockEmailSource = new Mock<IEmailSource>();
            mockEmailSource.Setup(emailSource => emailSource.ResourceID).Returns(expectedResourceID);
            mockEmailSource.Setup(emailSource => emailSource.Host).Returns(expectedHost);
            mockEmailSource.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailSource.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailSource.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailSource.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailSource.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailSource.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            var emailServiceSourceDefinition = new EmailServiceSourceDefinition(mockEmailSource.Object);

            var mockEmailServiceSource = new Mock<IEmailServiceSource>();
            mockEmailServiceSource.Setup(emailSource => emailSource.HostName).Returns(expectedHost);
            mockEmailServiceSource.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailServiceSource.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailServiceSource.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailServiceSource.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailServiceSource.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailServiceSource.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            var isEqual = emailServiceSourceDefinition.Equals(mockEmailServiceSource.Object);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_ReferenceEquals_EmailServiceSource_Expected_True()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedHost = "testHost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testuser";
            const int expectedPort = 4040;
            const int expectedTimeout = 30;
            const string expectedResourceName = "testResourceName";
            const bool expectedEnableSsl = false;

            var mockEmailSource = new Mock<IEmailSource>();
            mockEmailSource.Setup(emailSource => emailSource.ResourceID).Returns(expectedResourceID);
            mockEmailSource.Setup(emailSource => emailSource.Host).Returns(expectedHost);
            mockEmailSource.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailSource.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailSource.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailSource.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailSource.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailSource.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            IEmailServiceSource emailServiceSourceDefinition = new EmailServiceSourceDefinition(mockEmailSource.Object);

            var isEqual = emailServiceSourceDefinition.Equals(emailServiceSourceDefinition);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_Equals_EmailServiceSource_Expected_False()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedHost = "testHost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testuser";
            const int expectedPort = 4040;
            const int expectedTimeout = 30;
            const string expectedResourceName = "testResourceName";
            const bool expectedEnableSsl = false;

            var mockEmailSource = new Mock<IEmailSource>();
            mockEmailSource.Setup(emailSource => emailSource.ResourceID).Returns(expectedResourceID);
            mockEmailSource.Setup(emailSource => emailSource.Host).Returns(expectedHost);
            mockEmailSource.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailSource.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailSource.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailSource.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailSource.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailSource.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            var emailServiceSourceDefinition = new EmailServiceSourceDefinition(mockEmailSource.Object);

            var mockEmailServiceSource = new Mock<IEmailServiceSource>();
            mockEmailServiceSource.Setup(emailSource => emailSource.HostName).Returns("NewHost");
            mockEmailServiceSource.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailServiceSource.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailServiceSource.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailServiceSource.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailServiceSource.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailServiceSource.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            var isEqual = emailServiceSourceDefinition.Equals(mockEmailServiceSource.Object);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_Equals_EmailServiceSourceDefinition_Null_Expected_False()
        {
            var emailServiceSourceDefinition = new EmailServiceSourceDefinition();

            const EmailServiceSourceDefinition nullEmailServiceSourceDefinition = null;

            var isEqual = emailServiceSourceDefinition.Equals(nullEmailServiceSourceDefinition);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_ReferenceEquals_EmailServiceSourceDefinition_Expected_True()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedHost = "testHost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testuser";
            const int expectedPort = 4040;
            const int expectedTimeout = 30;
            const string expectedResourceName = "testResourceName";
            const bool expectedEnableSsl = false;

            var mockEmailSource = new Mock<IEmailSource>();
            mockEmailSource.Setup(emailSource => emailSource.ResourceID).Returns(expectedResourceID);
            mockEmailSource.Setup(emailSource => emailSource.Host).Returns(expectedHost);
            mockEmailSource.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailSource.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailSource.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailSource.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailSource.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailSource.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            var emailServiceSourceDefinition = new EmailServiceSourceDefinition(mockEmailSource.Object);

            var isEqual = emailServiceSourceDefinition.Equals(emailServiceSourceDefinition);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_Equals_EmailServiceSourceDefinition_Expected_True()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedHost = "testHost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testuser";
            const int expectedPort = 4040;
            const int expectedTimeout = 30;
            const string expectedResourceName = "testResourceName";
            const bool expectedEnableSsl = false;

            var mockEmailSource = new Mock<IEmailSource>();
            mockEmailSource.Setup(emailSource => emailSource.ResourceID).Returns(expectedResourceID);
            mockEmailSource.Setup(emailSource => emailSource.Host).Returns(expectedHost);
            mockEmailSource.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailSource.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailSource.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailSource.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailSource.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailSource.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            var emailServiceSourceDefinition = new EmailServiceSourceDefinition(mockEmailSource.Object);
            var emailServiceSourceDefinitionDup = new EmailServiceSourceDefinition(mockEmailSource.Object);

            var isEqual = emailServiceSourceDefinition.Equals(emailServiceSourceDefinitionDup);
            Assert.IsTrue(isEqual);
            Assert.IsTrue(emailServiceSourceDefinition == emailServiceSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_Equals_EmailServiceSourceDefinition_Expected_False()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedHost = "testHost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testuser";
            const int expectedPort = 4040;
            const int expectedTimeout = 30;
            const string expectedResourceName = "testResourceName";
            const bool expectedEnableSsl = false;

            var mockEmailSource = new Mock<IEmailSource>();
            mockEmailSource.Setup(emailSource => emailSource.ResourceID).Returns(expectedResourceID);
            mockEmailSource.Setup(emailSource => emailSource.Host).Returns(expectedHost);
            mockEmailSource.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailSource.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailSource.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailSource.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailSource.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailSource.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            var emailServiceSourceDefinition = new EmailServiceSourceDefinition(mockEmailSource.Object);

            var mockEmailSourceDup = new Mock<IEmailSource>();
            mockEmailSourceDup.Setup(emailSource => emailSource.ResourceID).Returns(expectedResourceID);
            mockEmailSourceDup.Setup(emailSource => emailSource.Host).Returns("NewHost");
            mockEmailSourceDup.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailSourceDup.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailSourceDup.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailSourceDup.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailSourceDup.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailSourceDup.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            var emailServiceSourceDefinitionDup = new EmailServiceSourceDefinition(mockEmailSourceDup.Object);

            var isEqual = emailServiceSourceDefinition.Equals(emailServiceSourceDefinitionDup);
            Assert.IsFalse(isEqual);
            Assert.IsTrue(emailServiceSourceDefinition != emailServiceSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_Equals_Object_Null_Expected_False()
        {
            var emailServiceSourceDefinition = new EmailServiceSourceDefinition();

            const object emailSource = null;

            var isEqual = emailServiceSourceDefinition.Equals(emailSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_Equals_Object_Expected_True()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedHost = "testHost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testuser";
            const int expectedPort = 4040;
            const int expectedTimeout = 30;
            const string expectedResourceName = "testResourceName";
            const bool expectedEnableSsl = false;

            var mockEmailSource = new Mock<IEmailSource>();
            mockEmailSource.Setup(emailSource => emailSource.ResourceID).Returns(expectedResourceID);
            mockEmailSource.Setup(emailSource => emailSource.Host).Returns(expectedHost);
            mockEmailSource.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailSource.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailSource.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailSource.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailSource.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailSource.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            var emailServiceSourceDefinition = new EmailServiceSourceDefinition(mockEmailSource.Object);

            object emailSourceObj = emailServiceSourceDefinition;

            var isEqual = emailServiceSourceDefinition.Equals(emailSourceObj);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_Equals_Object_Expected_False()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedHost = "testHost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testuser";
            const int expectedPort = 4040;
            const int expectedTimeout = 30;
            const string expectedResourceName = "testResourceName";
            const bool expectedEnableSsl = false;

            var mockEmailSource = new Mock<IEmailSource>();
            mockEmailSource.Setup(emailSource => emailSource.ResourceID).Returns(expectedResourceID);
            mockEmailSource.Setup(emailSource => emailSource.Host).Returns(expectedHost);
            mockEmailSource.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailSource.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailSource.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailSource.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailSource.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailSource.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            var emailServiceSourceDefinition = new EmailServiceSourceDefinition(mockEmailSource.Object);

            var mockEmailSourceDup = new Mock<IEmailSource>();
            mockEmailSourceDup.Setup(emailSource => emailSource.ResourceID).Returns(expectedResourceID);
            mockEmailSourceDup.Setup(emailSource => emailSource.Host).Returns("NewHost");
            mockEmailSourceDup.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailSourceDup.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailSourceDup.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailSourceDup.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailSourceDup.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailSourceDup.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            object emailSourceObj = new EmailServiceSourceDefinition(mockEmailSourceDup.Object);

            var isEqual = emailServiceSourceDefinition.Equals(emailSourceObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_Equals_Object_GetType_Expected_False()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedHost = "testHost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testuser";
            const int expectedPort = 4040;
            const int expectedTimeout = 30;
            const string expectedResourceName = "testResourceName";
            const bool expectedEnableSsl = false;

            var mockEmailSource = new Mock<IEmailSource>();
            mockEmailSource.Setup(emailSource => emailSource.ResourceID).Returns(expectedResourceID);
            mockEmailSource.Setup(emailSource => emailSource.Host).Returns(expectedHost);
            mockEmailSource.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailSource.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailSource.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailSource.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailSource.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailSource.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            var emailServiceSourceDefinition = new EmailServiceSourceDefinition(mockEmailSource.Object);

            var emailSourceObj = new object();

            var isEqual = emailServiceSourceDefinition.Equals(emailSourceObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_GetHashCode_Not_Equal_To_Zero()
        {
            var expectedResourceID = Guid.NewGuid();
            const string expectedHost = "testHost";
            const string expectedPassword = "test123";
            const string expectedUserName = "testuser";
            const int expectedPort = 4040;
            const int expectedTimeout = 30;
            const string expectedResourceName = "testResourceName";
            const bool expectedEnableSsl = false;

            var mockEmailSource = new Mock<IEmailSource>();
            mockEmailSource.Setup(emailSource => emailSource.ResourceID).Returns(expectedResourceID);
            mockEmailSource.Setup(emailSource => emailSource.Host).Returns(expectedHost);
            mockEmailSource.Setup(emailSource => emailSource.Password).Returns(expectedPassword);
            mockEmailSource.Setup(emailSource => emailSource.UserName).Returns(expectedUserName);
            mockEmailSource.Setup(emailSource => emailSource.Port).Returns(expectedPort);
            mockEmailSource.Setup(emailSource => emailSource.Timeout).Returns(expectedTimeout);
            mockEmailSource.Setup(emailSource => emailSource.ResourceName).Returns(expectedResourceName);
            mockEmailSource.Setup(emailSource => emailSource.EnableSsl).Returns(expectedEnableSsl);

            var emailServiceSourceDefinition = new EmailServiceSourceDefinition(mockEmailSource.Object);

            var hashCode = emailServiceSourceDefinition.GetHashCode();

            Assert.AreNotEqual(0, hashCode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(EmailServiceSourceDefinition))]
        public void EmailServiceSourceDefinition_GetHashCode_Expect_Zero()
        {
            var mockEmailSource = new Mock<IEmailSource>();

            var emailServiceSourceDefinition = new EmailServiceSourceDefinition(mockEmailSource.Object);

            var hashCode = emailServiceSourceDefinition.GetHashCode();

            Assert.AreEqual(0, hashCode);
        }
    }
}
