/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using Warewolf.Configuration;
using Warewolf.Data;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class PersistenceSettingsTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettings))]
        public void PersistenceSettings_SaveIfNotExists()
        {
            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false).Verifiable();
            mockIFile.Setup(o => o.WriteAllText(PersistenceSettings.SettingsPath, It.IsAny<string>()));
            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(o => o.CreateIfNotExists(Path.GetDirectoryName(PersistenceSettings.SettingsPath))).Returns(PersistenceSettings.SettingsPath);

            //act
            var auditSettings = new PersistenceSettings("some path", mockIFile.Object, mockDirectory.Object);
            auditSettings.SaveIfNotExists();

            //assert
            mockIFile.Verify();
            mockDirectory.Verify();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettings))]
        public void PersistenceSettings_Get_Settings()
        {
            const string expectedDashboardEndpoint = @"http://localhost";
            const string expectedDashboardPort = @"5001";
            const string expectedDashboardName = @"HangfireDashboard";

            //arrange
            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false);
            var mockDirectory = new Mock<IDirectory>();

            var hangfireSettings = new PersistenceSettings("some path", mockIFile.Object, mockDirectory.Object);
            hangfireSettings.PersistenceScheduler = "Hangfire";
            hangfireSettings.Enable = true;
            hangfireSettings.EncryptDataSource = false;
            hangfireSettings.PrepareSchemaIfNecessary = true;
            hangfireSettings.ServerName = "LocalServer";
            hangfireSettings.DashboardEndpoint = expectedDashboardEndpoint;
            hangfireSettings.DashboardPort = expectedDashboardPort;
            hangfireSettings.DashboardName = expectedDashboardName;
            hangfireSettings.PersistenceDataSource = new NamedGuidWithEncryptedPayload();
            var result = hangfireSettings.Get();

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedDashboardEndpoint, result.DashboardEndpoint);
            Assert.AreEqual(expectedDashboardName, result.DashboardName);
            Assert.AreEqual(expectedDashboardPort, result.DashboardPort);
            Assert.AreEqual("Hangfire", result.PersistenceScheduler);
            Assert.AreEqual(new NamedGuidWithEncryptedPayload(), hangfireSettings.PersistenceDataSource);
            Assert.AreEqual(false, result.EncryptDataSource);
            Assert.AreEqual(true, result.PrepareSchemaIfNecessary);
            Assert.AreEqual(true, result.Enable);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettings))]
        public void PersistenceSettings_Set_Get_LoggingDataSource()
        {
            var expectedHangfireSettingsData = new PersistenceSettingsData
            {
                EncryptDataSource = true,
                PersistenceDataSource = new NamedGuidWithEncryptedPayload
                {
                    Name = "Data Source",
                    Value = Guid.Empty,
                    Payload = "foo"
                },
            };
            Assert.AreEqual(expectedHangfireSettingsData.PersistenceDataSource.Value, Guid.Empty);
            Assert.AreEqual(expectedHangfireSettingsData.PersistenceDataSource.Name, "Data Source");
            Assert.AreEqual(expectedHangfireSettingsData.PersistenceDataSource.Payload, "foo");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettings))]
        public void PersistenceSettingsData_Equals_ExpectTrue()
        {
            var data1 = new PersistenceSettingsData
            {
                PersistenceScheduler = "Hangfire",
                Enable = true,
                PersistenceDataSource = new NamedGuidWithEncryptedPayload
                {
                    Name = "Data Source",
                    Value = Guid.Empty,
                    Payload = "foo"
                },
                EncryptDataSource = true,
                DashboardEndpoint = "DashboardEndpoint",
                DashboardName = "Dashboardname",
                DashboardPort = "5001",
                PrepareSchemaIfNecessary = true,
                ServerName = "servername"
            };
            var data2 = new PersistenceSettingsData
            {
                PersistenceScheduler = "Hangfire",
                Enable = true,
                PersistenceDataSource = new NamedGuidWithEncryptedPayload
                {
                    Name = "Data Source",
                    Value = Guid.Empty,
                    Payload = "foo"
                },
                EncryptDataSource = true,
                DashboardEndpoint = "DashboardEndpoint",
                DashboardName = "Dashboardname",
                DashboardPort = "5001",
                PrepareSchemaIfNecessary = true,
                ServerName = "servername"
            };
            Assert.IsTrue(data1.Equals(data2));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettings))]
        public void PersistenceSettingsData_Equals_ExpectFalse()
        {
            var data1 = new PersistenceSettingsData();
            data1.PersistenceScheduler = "Hangfire";
            data1.PersistenceDataSource.Payload = "foo";
            data1.EncryptDataSource = true;
            data1.DashboardEndpoint = "DashboardEndpoint";
            data1.DashboardName = "Dashboardname";
            data1.DashboardPort = "5001";
            data1.PrepareSchemaIfNecessary = true;
            data1.ServerName = "servername";

            var data2 = new PersistenceSettingsData();
            data2.PersistenceScheduler = "Hangfire";
            data2.PersistenceDataSource.Payload = "foo2";
            data2.EncryptDataSource = false;
            data2.DashboardEndpoint = "DashboardEndpoint";
            data2.DashboardName = "Dashboardname";
            data2.DashboardPort = "5001";
            data2.PrepareSchemaIfNecessary = true;
            data2.ServerName = "servername";
            Assert.IsFalse(data1.Equals(data2));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PersistenceSettings))]
        public void PersistenceSettingsData_Clone_Success()
        {
            var data1 = new PersistenceSettingsData
            {
                PersistenceScheduler = "Hangfire",
                Enable = true,
                PersistenceDataSource = new NamedGuidWithEncryptedPayload
                {
                    Name = "Data Source",
                    Value = Guid.Empty,
                    Payload = "foo1"
                },
                EncryptDataSource = true,
                DashboardEndpoint = "DashboardEndpoint",
                DashboardName = "Dashboardname",
                DashboardPort = "5001",
                PrepareSchemaIfNecessary = true,
                ServerName = "servername"
            };

            var data2 = data1.Clone();
            Assert.AreNotEqual(data1.GetHashCode(), data2.GetHashCode());
            Assert.IsFalse(ReferenceEquals(data1, data2));

            Assert.IsTrue(data1.Equals(data2));
            data1.PersistenceDataSource.Payload = "foo2";
            Assert.IsFalse(data1.Equals(data2));
        }
    }
}