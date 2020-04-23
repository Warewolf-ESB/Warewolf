﻿/*
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
using System.Text;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Configuration;
using Warewolf.Data;
using Warewolf.Streams;
using Warewolf.UnitTestAttributes;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class AuditingSettingsTests
    {
        [TestMethod]
        [TestCategory(nameof(AuditingSettingsData))]
        public void AuditingSettingsData_Constants()
        {
            Assert.AreEqual(@"ws://127.0.0.1:5000/ws", AuditingSettings.DefaultEndpoint);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditingSettings))]
        public void AuditingSettings_SaveIfNotExists()
        {
            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false).Verifiable();
            mockIFile.Setup(o => o.WriteAllText(AuditingSettings.SettingsPath, It.IsAny<string>()));
            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(o => o.CreateIfNotExists(Path.GetDirectoryName(AuditingSettings.SettingsPath))).Returns(AuditingSettings.SettingsPath);

            //act
            var auditSettings = new AuditingSettings("some path", mockIFile.Object, mockDirectory.Object);
            auditSettings.SaveIfNotExists();

            //assert
            mockIFile.Verify();
            mockDirectory.Verify();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditingSettings))]
        public void AuditingSettings_Get_Settings()
        {
            const string expectedEndpoint = @"ws://127.0.0.1:5000/ws";

            //arrange
            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false);
            var mockDirectory = new Mock<IDirectory>();

            var auditSettings = new AuditingSettings("some path", mockIFile.Object, mockDirectory.Object);

            var result = auditSettings.Get();

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEndpoint, result.Endpoint);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditingSettings))]
        public void AuditingSettingsData_Set_Get_LoggingDataSource()
        {
            var dependency = new Depends(Depends.ContainerType.AnonymousElasticsearch);
            var hostName = "http://" + dependency.Container.IP;

            var source = new ElasticsearchSource
            {
                AuthenticationType = AuthenticationType.Anonymous,
                Port = dependency.Container.Port,
                HostName = hostName,
                SearchIndex = "warewolflogstests"
            };
            var serializer = new Dev2JsonSerializer();
            var jsonSource = serializer.Serialize(source);
            var expectedAuditingSettingsData = new AuditingSettingsData
            {
                Endpoint = "ws://127.0.0.1:5000/ws",
                LoggingDataSource = new NamedGuidWithEncryptedPayload
                {
                    Name = "Data Source",
                    Value = Guid.Empty,
                    Payload = jsonSource
                },
            };
            IDeserializer deserializer = new JsonSerializer();
            var bytes = UTF8Encoding.UTF8.GetBytes(expectedAuditingSettingsData.LoggingDataSource.Payload);
            var result = deserializer.Deserialize<ElasticsearchSource>(bytes);

            Assert.AreEqual(expectedAuditingSettingsData.LoggingDataSource.Value, Guid.Empty);
            Assert.AreEqual(expectedAuditingSettingsData.LoggingDataSource.Name, "Data Source");
            Assert.AreEqual(source, result);
        }

        [TestMethod]
        public void AuditSettingsData_Equals_ExpectTrue()
        {
            var data1 = new AuditingSettingsData();
            var data2 = new AuditingSettingsData();

            Assert.IsTrue(data1.Equals(data2));

            data1.LoggingDataSource.Payload = "foo";
            data2.LoggingDataSource.Payload = "foo";

            Assert.IsTrue(data1.Equals(data2));

        }

        [TestMethod]
        public void AuditSettingsData_Equals_ExpectFalse()
        {
            var data1 = new AuditingSettingsData();
            data1.LoggingDataSource.Payload = "foo";
            var data2 = new AuditingSettingsData();
            data2.LoggingDataSource.Payload = "foo2";

            Assert.IsFalse(data1.Equals(data2));
            //Assert.IsTrue(data1.Equals(data2));

        }

        [TestMethod]
        public void AuditSettingsData_Clone_ExpectUnique()
        {
            var data1 = new AuditingSettingsData();
            data1.LoggingDataSource.Payload = "foo";
            var data2 = data1.Clone();
            Assert.AreNotEqual(data1.GetHashCode(), data2.GetHashCode());
            Assert.IsFalse(ReferenceEquals(data1, data2));
            Assert.IsFalse(ReferenceEquals(data1.LoggingDataSource, data2.LoggingDataSource));

            Assert.IsTrue(data1.Equals(data2));

            data1.LoggingDataSource.Payload = "foo2";

            Assert.IsFalse(data1.Equals(data2));

        }
    }
}
