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
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Dev2.Common.Tests.Core.Database
{
    [TestClass]
    public class DbSourceDefinitionTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_Validate()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDatabaseName = "warewolfDb";
            var expectedResourceID = Guid.NewGuid();
            const string expectedSavePath = "Path\\ResourcePath";
            const string expectedResourceName = "testResource";
            const string expectedPassword = "test123";
            const string expectedServer = "localhost";
            const int expectedConnectionTimeout = 30;
            const enSourceType expectedServerType = enSourceType.SqlDatabase;
            const string expectedUserId = "userId";

            var mockDb = new Mock<IDb>();
            mockDb.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDb.Setup(db => db.DatabaseName).Returns(expectedDatabaseName);
            mockDb.Setup(db => db.ResourceID).Returns(expectedResourceID);
            mockDb.Setup(db => db.GetSavePath()).Returns(expectedSavePath);
            mockDb.Setup(db => db.ResourceName).Returns(expectedResourceName);
            mockDb.Setup(db => db.Password).Returns(expectedPassword);
            mockDb.Setup(db => db.Server).Returns(expectedServer);
            mockDb.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);
            mockDb.Setup(db => db.ServerType).Returns(expectedServerType);
            mockDb.Setup(db => db.UserID).Returns(expectedUserId);

            var dbSourceDefinition = new DbSourceDefinition(mockDb.Object)
            {
                ReloadActions = true
            };

            Assert.IsTrue(dbSourceDefinition.ReloadActions);

            Assert.AreEqual(expectedAuthenticationType, dbSourceDefinition.AuthenticationType);
            Assert.AreEqual(expectedDatabaseName, dbSourceDefinition.DbName);
            Assert.AreEqual(expectedResourceID, dbSourceDefinition.Id);
            Assert.AreEqual(expectedSavePath, dbSourceDefinition.Path);
            Assert.AreEqual(expectedResourceName, dbSourceDefinition.Name);
            Assert.AreEqual(expectedPassword, dbSourceDefinition.Password);
            Assert.AreEqual(expectedServer, dbSourceDefinition.ServerName);
            Assert.AreEqual(expectedConnectionTimeout, dbSourceDefinition.ConnectionTimeout);
            Assert.AreEqual(expectedServerType, dbSourceDefinition.Type);
            Assert.AreEqual(expectedUserId, dbSourceDefinition.UserName);
            Assert.AreEqual(expectedResourceName, dbSourceDefinition.ToString());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_Equals_DbSource_Null_Expected_False()
        {
            var dbSourceDefinition = new DbSourceDefinition();

            const IDbSource dbSource = null;

            var isEqual = dbSourceDefinition.Equals(dbSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_Equals_DbSource_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDatabaseName = "warewolfDb";
            var expectedResourceID = Guid.NewGuid();
            const string expectedPassword = "test123";
            const string expectedServer = "localhost";
            const int expectedConnectionTimeout = 30;
            const enSourceType expectedServerType = enSourceType.SqlDatabase;
            const string expectedUserId = "userId";

            var mockDb = new Mock<IDb>();
            mockDb.Setup(db => db.Server).Returns(expectedServer);
            mockDb.Setup(db => db.ServerType).Returns(expectedServerType);
            mockDb.Setup(db => db.UserID).Returns(expectedUserId);
            mockDb.Setup(db => db.Password).Returns(expectedPassword);
            mockDb.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDb.Setup(db => db.ResourceID).Returns(expectedResourceID);
            mockDb.Setup(db => db.DatabaseName).Returns(expectedDatabaseName);
            mockDb.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);

            var dbSourceDefinition = new DbSourceDefinition(mockDb.Object);

            var mockDbSource = new Mock<IDbSource>();
            mockDbSource.Setup(db => db.ServerName).Returns(expectedServer);
            mockDbSource.Setup(db => db.Type).Returns(expectedServerType);
            mockDbSource.Setup(db => db.UserName).Returns(expectedUserId);
            mockDbSource.Setup(db => db.Password).Returns(expectedPassword);
            mockDbSource.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDbSource.Setup(db => db.Id).Returns(expectedResourceID);
            mockDbSource.Setup(db => db.DbName).Returns(expectedDatabaseName);
            mockDbSource.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);

            var isEqual = dbSourceDefinition.Equals(mockDbSource.Object);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_ReferenceEquals_DbSource_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDatabaseName = "warewolfDb";
            var expectedResourceID = Guid.NewGuid();
            const string expectedPassword = "test123";
            const string expectedServer = "localhost";
            const int expectedConnectionTimeout = 30;
            const enSourceType expectedServerType = enSourceType.SqlDatabase;
            const string expectedUserId = "userId";

            var mockDb = new Mock<IDb>();
            mockDb.Setup(db => db.Server).Returns(expectedServer);
            mockDb.Setup(db => db.ServerType).Returns(expectedServerType);
            mockDb.Setup(db => db.UserID).Returns(expectedUserId);
            mockDb.Setup(db => db.Password).Returns(expectedPassword);
            mockDb.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDb.Setup(db => db.ResourceID).Returns(expectedResourceID);
            mockDb.Setup(db => db.DatabaseName).Returns(expectedDatabaseName);
            mockDb.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);

            IDbSource dbSourceDefinition = new DbSourceDefinition(mockDb.Object);

            var isEqual = dbSourceDefinition.Equals(dbSourceDefinition);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_Equals_DbSource_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDatabaseName = "warewolfDb";
            var expectedResourceID = Guid.NewGuid();
            const string expectedPassword = "test123";
            const string expectedServer = "localhost";
            const int expectedConnectionTimeout = 30;
            const enSourceType expectedServerType = enSourceType.SqlDatabase;
            const string expectedUserId = "userId";

            var mockDb = new Mock<IDb>();
            mockDb.Setup(db => db.Server).Returns(expectedServer);
            mockDb.Setup(db => db.ServerType).Returns(expectedServerType);
            mockDb.Setup(db => db.UserID).Returns(expectedUserId);
            mockDb.Setup(db => db.Password).Returns(expectedPassword);
            mockDb.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDb.Setup(db => db.ResourceID).Returns(expectedResourceID);
            mockDb.Setup(db => db.DatabaseName).Returns(expectedDatabaseName);
            mockDb.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);

            var dbSourceDefinition = new DbSourceDefinition(mockDb.Object);

            var mockDbSource = new Mock<IDbSource>();
            mockDbSource.Setup(db => db.ServerName).Returns("remoteServer");
            mockDbSource.Setup(db => db.Type).Returns(expectedServerType);
            mockDbSource.Setup(db => db.UserName).Returns(expectedUserId);
            mockDbSource.Setup(db => db.Password).Returns(expectedPassword);
            mockDbSource.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDbSource.Setup(db => db.Id).Returns(expectedResourceID);
            mockDbSource.Setup(db => db.DbName).Returns(expectedDatabaseName);
            mockDbSource.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);

            var isEqual = dbSourceDefinition.Equals(mockDbSource.Object);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_Equals_DbSourceDefinition_Null_Expected_False()
        {
            var dbSourceDefinition = new DbSourceDefinition();

            const DbSourceDefinition nullDbSourceDefinition = null;

            var isEqual = dbSourceDefinition.Equals(nullDbSourceDefinition);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_ReferenceEquals_DbSourceDefinition_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDatabaseName = "warewolfDb";
            var expectedResourceID = Guid.NewGuid();
            const string expectedPassword = "test123";
            const string expectedServer = "localhost";
            const int expectedConnectionTimeout = 30;
            const enSourceType expectedServerType = enSourceType.SqlDatabase;
            const string expectedUserId = "userId";

            var mockDb = new Mock<IDb>();
            mockDb.Setup(db => db.Server).Returns(expectedServer);
            mockDb.Setup(db => db.ServerType).Returns(expectedServerType);
            mockDb.Setup(db => db.UserID).Returns(expectedUserId);
            mockDb.Setup(db => db.Password).Returns(expectedPassword);
            mockDb.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDb.Setup(db => db.ResourceID).Returns(expectedResourceID);
            mockDb.Setup(db => db.DatabaseName).Returns(expectedDatabaseName);
            mockDb.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);

            var dbSourceDefinition = new DbSourceDefinition(mockDb.Object);

            var isEqual = dbSourceDefinition.Equals(dbSourceDefinition);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_Equals_DbSourceDefinition_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDatabaseName = "warewolfDb";
            var expectedResourceID = Guid.NewGuid();
            const string expectedPassword = "test123";
            const string expectedServer = "localhost";
            const int expectedConnectionTimeout = 30;
            const enSourceType expectedServerType = enSourceType.SqlDatabase;
            const string expectedUserId = "userId";

            var mockDb = new Mock<IDb>();
            mockDb.Setup(db => db.Server).Returns(expectedServer);
            mockDb.Setup(db => db.ServerType).Returns(expectedServerType);
            mockDb.Setup(db => db.UserID).Returns(expectedUserId);
            mockDb.Setup(db => db.Password).Returns(expectedPassword);
            mockDb.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDb.Setup(db => db.ResourceID).Returns(expectedResourceID);
            mockDb.Setup(db => db.DatabaseName).Returns(expectedDatabaseName);
            mockDb.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);

            var dbSourceDefinition = new DbSourceDefinition(mockDb.Object);
            var dbSourceDefinitionDup = new DbSourceDefinition(mockDb.Object);

            var isEqual = dbSourceDefinition.Equals(dbSourceDefinitionDup);
            Assert.IsTrue(isEqual);
            Assert.IsTrue(dbSourceDefinition == dbSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_Equals_DbSourceDefinition_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDatabaseName = "warewolfDb";
            var expectedResourceID = Guid.NewGuid();
            const string expectedPassword = "test123";
            const string expectedServer = "localhost";
            const int expectedConnectionTimeout = 30;
            const enSourceType expectedServerType = enSourceType.SqlDatabase;
            const string expectedUserId = "userId";

            var mockDb = new Mock<IDb>();
            mockDb.Setup(db => db.Server).Returns(expectedServer);
            mockDb.Setup(db => db.ServerType).Returns(expectedServerType);
            mockDb.Setup(db => db.UserID).Returns(expectedUserId);
            mockDb.Setup(db => db.Password).Returns(expectedPassword);
            mockDb.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDb.Setup(db => db.ResourceID).Returns(expectedResourceID);
            mockDb.Setup(db => db.DatabaseName).Returns(expectedDatabaseName);
            mockDb.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);

            var dbSourceDefinition = new DbSourceDefinition(mockDb.Object);

            var mockDbSource = new Mock<IDb>();
            mockDb.Setup(db => db.Server).Returns("remoteServer");
            mockDb.Setup(db => db.ServerType).Returns(expectedServerType);
            mockDb.Setup(db => db.UserID).Returns(expectedUserId);
            mockDb.Setup(db => db.Password).Returns(expectedPassword);
            mockDb.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDb.Setup(db => db.ResourceID).Returns(expectedResourceID);
            mockDb.Setup(db => db.DatabaseName).Returns(expectedDatabaseName);
            mockDb.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);

            var dbSourceDefinitionDup = new DbSourceDefinition(mockDbSource.Object);

            var isEqual = dbSourceDefinition.Equals(dbSourceDefinitionDup);
            Assert.IsFalse(isEqual);
            Assert.IsTrue(dbSourceDefinition != dbSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_Equals_Object_Null_Expected_False()
        {
            var dbSourceDefinition = new DbSourceDefinition();

            const object dbSource = null;

            var isEqual = dbSourceDefinition.Equals(dbSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_Equals_Object_Expected_True()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDatabaseName = "warewolfDb";
            var expectedResourceID = Guid.NewGuid();
            const string expectedPassword = "test123";
            const string expectedServer = "localhost";
            const int expectedConnectionTimeout = 30;
            const enSourceType expectedServerType = enSourceType.SqlDatabase;
            const string expectedUserId = "userId";

            var mockDb = new Mock<IDb>();
            mockDb.Setup(db => db.Server).Returns(expectedServer);
            mockDb.Setup(db => db.ServerType).Returns(expectedServerType);
            mockDb.Setup(db => db.UserID).Returns(expectedUserId);
            mockDb.Setup(db => db.Password).Returns(expectedPassword);
            mockDb.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDb.Setup(db => db.ResourceID).Returns(expectedResourceID);
            mockDb.Setup(db => db.DatabaseName).Returns(expectedDatabaseName);
            mockDb.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);

            var dbSourceDefinition = new DbSourceDefinition(mockDb.Object);

            object dbSource = dbSourceDefinition;

            var isEqual = dbSourceDefinition.Equals(dbSource);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_Equals_Object_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDatabaseName = "warewolfDb";
            var expectedResourceID = Guid.NewGuid();
            const string expectedPassword = "test123";
            const string expectedServer = "localhost";
            const int expectedConnectionTimeout = 30;
            const enSourceType expectedServerType = enSourceType.SqlDatabase;
            const string expectedUserId = "userId";

            var mockDb = new Mock<IDb>();
            mockDb.Setup(db => db.Server).Returns(expectedServer);
            mockDb.Setup(db => db.ServerType).Returns(expectedServerType);
            mockDb.Setup(db => db.UserID).Returns(expectedUserId);
            mockDb.Setup(db => db.Password).Returns(expectedPassword);
            mockDb.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDb.Setup(db => db.ResourceID).Returns(expectedResourceID);
            mockDb.Setup(db => db.DatabaseName).Returns(expectedDatabaseName);
            mockDb.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);

            var dbSourceDefinition = new DbSourceDefinition(mockDb.Object);

            var mockDbDup = new Mock<IDb>();
            mockDbDup.Setup(db => db.Server).Returns("remoteServer");
            mockDbDup.Setup(db => db.ServerType).Returns(expectedServerType);
            mockDbDup.Setup(db => db.UserID).Returns(expectedUserId);
            mockDbDup.Setup(db => db.Password).Returns(expectedPassword);
            mockDbDup.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDbDup.Setup(db => db.ResourceID).Returns(expectedResourceID);
            mockDbDup.Setup(db => db.DatabaseName).Returns(expectedDatabaseName);
            mockDbDup.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);

            object dbSource = new DbSourceDefinition(mockDbDup.Object);

            var isEqual = dbSourceDefinition.Equals(dbSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_Equals_Object_GetType_Expected_False()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDatabaseName = "warewolfDb";
            var expectedResourceID = Guid.NewGuid();
            const string expectedPassword = "test123";
            const string expectedServer = "localhost";
            const int expectedConnectionTimeout = 30;
            const enSourceType expectedServerType = enSourceType.SqlDatabase;
            const string expectedUserId = "userId";

            var mockDb = new Mock<IDb>();
            mockDb.Setup(db => db.Server).Returns(expectedServer);
            mockDb.Setup(db => db.ServerType).Returns(expectedServerType);
            mockDb.Setup(db => db.UserID).Returns(expectedUserId);
            mockDb.Setup(db => db.Password).Returns(expectedPassword);
            mockDb.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDb.Setup(db => db.ResourceID).Returns(expectedResourceID);
            mockDb.Setup(db => db.DatabaseName).Returns(expectedDatabaseName);
            mockDb.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);

            var dbSourceDefinition = new DbSourceDefinition(mockDb.Object);

            var dbSource = new object();

            var isEqual = dbSourceDefinition.Equals(dbSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_GetHashCode_Not_Equal_To_Zero()
        {
            const AuthenticationType expectedAuthenticationType = AuthenticationType.Public;
            const string expectedDatabaseName = "warewolfDb";
            const string expectedPassword = "test123";
            const string expectedServer = "localhost";
            const int expectedConnectionTimeout = 30;
            const enSourceType expectedServerType = enSourceType.SqlDatabase;
            const string expectedUserId = "userId";

            var mockDb = new Mock<IDb>();
            mockDb.Setup(db => db.Server).Returns(expectedServer);
            mockDb.Setup(db => db.ServerType).Returns(expectedServerType);
            mockDb.Setup(db => db.UserID).Returns(expectedUserId);
            mockDb.Setup(db => db.Password).Returns(expectedPassword);
            mockDb.Setup(db => db.AuthenticationType).Returns(expectedAuthenticationType);
            mockDb.Setup(db => db.ConnectionTimeout).Returns(expectedConnectionTimeout);
            mockDb.Setup(db => db.DatabaseName).Returns(expectedDatabaseName);

            var dbSourceDefinition = new DbSourceDefinition(mockDb.Object);

            var hashCode = dbSourceDefinition.GetHashCode();

            Assert.AreNotEqual(0, hashCode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DbSourceDefinition))]
        public void DbSourceDefinition_GetHashCode_Expect_Zero()
        {
            var mockDb = new Mock<IDb>();

            var dbSourceDefinition = new DbSourceDefinition(mockDb.Object);

            var hashCode = dbSourceDefinition.GetHashCode();

            Assert.AreEqual(0, hashCode);
        }
    }
}
