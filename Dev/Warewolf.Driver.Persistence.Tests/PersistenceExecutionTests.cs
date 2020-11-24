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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Data.Interfaces.Enums;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

namespace Warewolf.Driver.Persistence.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class PersistenceExecutionTests
    {
        private string _settingsFilePath;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _settingsFilePath = @"C:\ProgramData\Warewolf\Server Settings\persistencesettings.json";
            if (File.Exists(_settingsFilePath)) return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath));
                File.WriteAllText(_settingsFilePath,
                    "{\"EncryptDataSource\":false,\"PersistenceDataSource\":{\"Payload\":\"{\\r\\n  \\\"$id\\\": \\\"1\\\",\\r\\n  \\\"$type\\\": \\\"Dev2.Runtime.ServiceModel.Data.DbSource, Dev2.Runtime.Services\\\",\\r\\n  \\\"ServerType\\\": \\\"SqlDatabase\\\",\\r\\n  \\\"Server\\\": \\\"tuptn-stg01.premier.local\\\",\\r\\n  \\\"DatabaseName\\\": \\\"Hangfire\\\",\\r\\n  \\\"Port\\\": 1433,\\r\\n  \\\"ConnectionTimeout\\\": 30,\\r\\n  \\\"AuthenticationType\\\": \\\"User\\\",\\r\\n  \\\"UserID\\\": \\\"warewolf\\\",\\r\\n  \\\"Password\\\": \\\"W4r3w0lf##$$\\\",\\r\\n  \\\"DataList\\\": \\\"\\\",\\r\\n  \\\"ConnectionString\\\": \\\"Data Source=tuptn-stg01.premier.local,1433;Initial Catalog=Hangfire;User ID=warewolf;Password=W4r3w0lf##$$;;Connection Timeout=30\\\",\\r\\n  \\\"IsSource\\\": true,\\r\\n  \\\"IsService\\\": false,\\r\\n  \\\"IsFolder\\\": false,\\r\\n  \\\"IsReservedService\\\": false,\\r\\n  \\\"IsServer\\\": false,\\r\\n  \\\"IsResourceVersion\\\": false,\\r\\n  \\\"Version\\\": null,\\r\\n  \\\"ResourceID\\\": \\\"355e6cd6-2ed2-4409-8348-1e616b031bcf\\\",\\r\\n  \\\"ResourceType\\\": \\\"SqlDatabase\\\",\\r\\n  \\\"ResourceName\\\": \\\"Hangfire_Database\\\",\\r\\n  \\\"IsValid\\\": false,\\r\\n  \\\"Errors\\\": [],\\r\\n  \\\"ReloadActions\\\": false,\\r\\n  \\\"UserPermissions\\\": 0,\\r\\n  \\\"HasDataList\\\": false,\\r\\n  \\\"VersionInfo\\\": {\\r\\n    \\\"$id\\\": \\\"2\\\",\\r\\n    \\\"$type\\\": \\\"Dev2.Runtime.ServiceModel.Data.VersionInfo, Dev2.Data\\\",\\r\\n    \\\"DateTimeStamp\\\": \\\"2020-10-14T09:56:56.7360852+02:00\\\",\\r\\n    \\\"Reason\\\": \\\"Save\\\",\\r\\n    \\\"User\\\": \\\"Unknown\\\",\\r\\n    \\\"VersionNumber\\\": \\\"1\\\",\\r\\n    \\\"ResourceId\\\": \\\"355e6cd6-2ed2-4409-8348-1e616b031bcf\\\",\\r\\n    \\\"VersionId\\\": \\\"55b0c7eb-2eb2-4c4c-afa6-5075ba6fbdd4\\\"\\r\\n  }\\r\\n}\",\"Name\":\"Hangfire_Database\",\"Value\":\"355e6cd6-2ed2-4409-8348-1e616b031bcf\"},\"Enable\":true,\"PersistenceScheduler\":\"Hangfire\",\"PrepareSchemaIfNecessary\":true,\"DashboardHostname\":\"http://localhost\",\"DashboardPort\":\"5001\",\"DashboardName\":\"hangfire\",\"ServerName\":\"server\"}");
            }
            catch (IOException)
            {
            }
        }

        [TestCleanup]
        public void CleanupContainer()
        {
            try
            {
                File.Delete(_settingsFilePath);
            }
            catch (IOException)
            {
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PersistenceExecution))]
        public void PersistenceExecution_CreateAndScheduleJob_Success()
        {
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")}
            };

            const enSuspendOption suspendOption = enSuspendOption.SuspendForDays;
            const string suspendOptionValue = "1";

            var mockPersistenceScheduler = new Mock<IPersistenceScheduler>();
            mockPersistenceScheduler.Setup(o => o.ScheduleJob(suspendOption, suspendOptionValue, values)).Verifiable();

            var scheduler = new PersistenceExecution(mockPersistenceScheduler.Object);
            scheduler.CreateAndScheduleJob(suspendOption, suspendOptionValue, values);

            mockPersistenceScheduler.Verify(o => o.ScheduleJob(suspendOption, suspendOptionValue, values), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PersistenceExecution))]
        public void PersistenceExecution_ResumeJob_Success()
        {
            var mockDataObject = new Mock<IDSFDataObject>();
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")}
            };

            const enSuspendOption suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString(CultureInfo.InvariantCulture);
            const string expectedJobId = "1234";
            const string environment = "environment";

            var mockPersistenceScheduler = new Mock<IPersistenceScheduler>();
            mockPersistenceScheduler.Setup(o => o.ScheduleJob(suspendOption, suspendOptionValue, values))
                .Returns(expectedJobId).Verifiable();
            mockPersistenceScheduler.Setup(o => o.ResumeJob(mockDataObject.Object, expectedJobId, true, environment))
                .Returns(GlobalConstants.Success).Verifiable();

            var scheduler = new PersistenceExecution(mockPersistenceScheduler.Object);
            var jobId = scheduler.CreateAndScheduleJob(suspendOption, suspendOptionValue, values);
            var result = scheduler.ResumeJob(mockDataObject.Object, expectedJobId, true, environment);
            Assert.AreEqual(GlobalConstants.Success, result);
        }
    }
}