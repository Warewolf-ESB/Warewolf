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
using System.IO;
using System.Security.Principal;
using System.Text;
using Dev2;
using Dev2.Common;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime;
using Dev2.Runtime.Interfaces;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.States;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Auditing;
using enActionType = Dev2.Common.Interfaces.Core.DynamicServices.enActionType;

namespace Warewolf.Driver.Drivers.HangfireScheduler.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class HangfireSchedulerTests
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
                    "{\"EncryptDataSource\":true,\"PersistenceDataSource\":{\"Payload\":\"{\\r\\n  \\\"$id\\\": \\\"1\\\",\\r\\n  \\\"$type\\\": \\\"Dev2.Runtime.ServiceModel.Data.DbSource, Dev2.Runtime.Services\\\",\\r\\n  \\\"ServerType\\\": \\\"SqlDatabase\\\",\\r\\n  \\\"Server\\\": \\\"tuptn-stg01.premier.local\\\",\\r\\n  \\\"DatabaseName\\\": \\\"Hangfire\\\",\\r\\n  \\\"Port\\\": 1433,\\r\\n  \\\"ConnectionTimeout\\\": 30,\\r\\n  \\\"AuthenticationType\\\": \\\"User\\\",\\r\\n  \\\"UserID\\\": \\\"warewolf\\\",\\r\\n  \\\"Password\\\": \\\"W4r3w0lf##$$\\\",\\r\\n  \\\"DataList\\\": \\\"\\\",\\r\\n  \\\"ConnectionString\\\": \\\"Data Source=tuptn-stg01.premier.local,1433;Initial Catalog=Hangfire;User ID=warewolf;Password=W4r3w0lf##$$;;Connection Timeout=30\\\",\\r\\n  \\\"IsSource\\\": true,\\r\\n  \\\"IsService\\\": false,\\r\\n  \\\"IsFolder\\\": false,\\r\\n  \\\"IsReservedService\\\": false,\\r\\n  \\\"IsServer\\\": false,\\r\\n  \\\"IsResourceVersion\\\": false,\\r\\n  \\\"Version\\\": null,\\r\\n  \\\"ResourceID\\\": \\\"355e6cd6-2ed2-4409-8348-1e616b031bcf\\\",\\r\\n  \\\"ResourceType\\\": \\\"SqlDatabase\\\",\\r\\n  \\\"ResourceName\\\": \\\"Hangfire_Database\\\",\\r\\n  \\\"IsValid\\\": false,\\r\\n  \\\"Errors\\\": [],\\r\\n  \\\"ReloadActions\\\": false,\\r\\n  \\\"UserPermissions\\\": 0,\\r\\n  \\\"HasDataList\\\": false,\\r\\n  \\\"VersionInfo\\\": {\\r\\n    \\\"$id\\\": \\\"2\\\",\\r\\n    \\\"$type\\\": \\\"Dev2.Runtime.ServiceModel.Data.VersionInfo, Dev2.Data\\\",\\r\\n    \\\"DateTimeStamp\\\": \\\"2020-10-14T09:56:56.7360852+02:00\\\",\\r\\n    \\\"Reason\\\": \\\"Save\\\",\\r\\n    \\\"User\\\": \\\"Unknown\\\",\\r\\n    \\\"VersionNumber\\\": \\\"1\\\",\\r\\n    \\\"ResourceId\\\": \\\"355e6cd6-2ed2-4409-8348-1e616b031bcf\\\",\\r\\n    \\\"VersionId\\\": \\\"55b0c7eb-2eb2-4c4c-afa6-5075ba6fbdd4\\\"\\r\\n  }\\r\\n}\",\"Name\":\"Hangfire_Database\",\"Value\":\"355e6cd6-2ed2-4409-8348-1e616b031bcf\"},\"Enable\":true,\"PersistenceScheduler\":\"Hangfire\",\"PrepareSchemaIfNecessary\":true,\"DashboardHostname\":\"http://localhost\",\"DashboardPort\":\"5001\",\"DashboardName\":\"hangfire\",\"ServerName\":\"server\"}");
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
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_Default_Constructor()
        {
            var scheduler = new Persistence.Drivers.HangfireScheduler();
            Assert.IsNotNull(scheduler);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_ScheduleJob_Success()
        {
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")}
            };

            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();

            var jobstorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobstorage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobstorage);
            var jobId = scheduler.ScheduleJob(suspendOption, suspendOptionValue, values);
            Assert.IsNotNull(jobId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_ResumeJob_OverrideIsFalse_Success()
        {
            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(o => o.LogAdditionalDetail(It.IsAny<Audit>(), "ResumeJob")).Verifiable();

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns(WindowsIdentity.GetCurrent());

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.StateNotifier).Returns(mockStateNotifier.Object);
            dataObjectMock.Setup(o => o.ExecutingUser).Returns(mockPrincipal.Object);
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(WindowsIdentity.GetCurrent().Name)}
            };

            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();

            var jobstorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobstorage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobstorage);
            var jobId = scheduler.ScheduleJob(suspendOption, suspendOptionValue, values);

            var errors = new ErrorResultTO();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetService(GlobalConstants.ServerWorkspaceID, It.IsAny<Guid>(), "")).Returns(CreateServiceEntry());

            CustomContainer.Register(resourceCatalog.Object);

            var mockResumableExecutionContainer = new Mock<IResumableExecutionContainer>();
            mockResumableExecutionContainer.Setup(o => o.Execute(out errors, 0)).Verifiable();

            var mockResumableExecutionContainerFactory = new Mock<IResumableExecutionContainerFactory>();
            mockResumableExecutionContainerFactory.Setup(o => o.New(It.IsAny<Guid>(), It.IsAny<ServiceAction>(), It.IsAny<DsfDataObject>()))
                .Returns(mockResumableExecutionContainer.Object);
            CustomContainer.Register(mockResumableExecutionContainerFactory.Object);

            var result = scheduler.ResumeJob(dataObjectMock.Object, jobId, false, "NewEnvironment");
            Assert.AreEqual(GlobalConstants.Success, result);

            mockResumableExecutionContainer.Verify(o => o.Execute(out errors, 0), Times.Once);
            mockStateNotifier.Verify(o => o.LogAdditionalDetail(It.IsAny<Audit>(), "ResumeJob"), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_ResumeJob_OverrideIsTrue_Success()
        {
            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(o => o.LogAdditionalDetail(It.IsAny<Audit>(), "ResumeJob")).Verifiable();

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns(WindowsIdentity.GetCurrent());

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.StateNotifier).Returns(mockStateNotifier.Object);
            dataObjectMock.Setup(o => o.ExecutingUser).Returns(mockPrincipal.Object);
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(WindowsIdentity.GetCurrent().Name)}
            };

            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();

            var jobstorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobstorage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobstorage);
            var jobId = scheduler.ScheduleJob(suspendOption, suspendOptionValue, values);

            var errors = new ErrorResultTO();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetService(GlobalConstants.ServerWorkspaceID, It.IsAny<Guid>(), "")).Returns(CreateServiceEntry());

            CustomContainer.Register(resourceCatalog.Object);

            var mockResumableExecutionContainer = new Mock<IResumableExecutionContainer>();
            mockResumableExecutionContainer.Setup(o => o.Execute(out errors, 0)).Verifiable();

            var mockResumableExecutionContainerFactory = new Mock<IResumableExecutionContainerFactory>();
            mockResumableExecutionContainerFactory.Setup(o => o.New(It.IsAny<Guid>(), It.IsAny<ServiceAction>(), It.IsAny<DsfDataObject>()))
                .Returns(mockResumableExecutionContainer.Object);
            CustomContainer.Register(mockResumableExecutionContainerFactory.Object);

            var result = scheduler.ResumeJob(dataObjectMock.Object, jobId, true, "NewEnvironment_Override");
            Assert.AreEqual(GlobalConstants.Success, result);

            mockResumableExecutionContainer.Verify(o => o.Execute(out errors, 0), Times.Once);
            mockStateNotifier.Verify(o => o.LogAdditionalDetail(It.IsAny<Audit>(), "ResumeJob"), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        [Timeout(120000)]
        public void HangfireScheduler_ResumeJob_WorkflowResumeReturnsErrors_Failed()
        {
            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(o => o.LogAdditionalDetail(It.IsAny<Audit>(), "ResumeJob")).Verifiable();

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns(WindowsIdentity.GetCurrent());

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.StateNotifier).Returns(mockStateNotifier.Object);
            dataObjectMock.Setup(o => o.ExecutingUser).Returns(mockPrincipal.Object);

            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(WindowsIdentity.GetCurrent().Name)}
            };

            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();

            var jobstorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobstorage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobstorage);
            var jobId = scheduler.ScheduleJob(suspendOption, suspendOptionValue, values);

            var errors = new ErrorResultTO();
            errors.AddError("ErrorMessage");
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetService(GlobalConstants.ServerWorkspaceID, It.IsAny<Guid>(), "")).Returns(CreateServiceEntry());

            CustomContainer.Register(resourceCatalog.Object);

            var mockResumableExecutionContainer = new Mock<IResumableExecutionContainer>();
            mockResumableExecutionContainer.Setup(o => o.Execute(out errors, 0)).Verifiable();

            var mockResumableExecutionContainerFactory = new Mock<IResumableExecutionContainerFactory>();
            mockResumableExecutionContainerFactory.Setup(o => o.New(It.IsAny<Guid>(), It.IsAny<ServiceAction>(), It.IsAny<DsfDataObject>()))
                .Returns(mockResumableExecutionContainer.Object);
            CustomContainer.Register(mockResumableExecutionContainerFactory.Object);

            var result = scheduler.ResumeJob(dataObjectMock.Object, jobId, false, "NewEnvironment");
            Assert.AreEqual(GlobalConstants.Failed, result);

            mockResumableExecutionContainer.Verify(o => o.Execute(out errors, 0), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_ResumeJob_InManuallyResumedState_Failed()
        {
            var dataObjectMock = new Mock<IDSFDataObject>();
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")}
            };

            var jobstorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobstorage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobstorage);

            var jobId = scheduler.ScheduleJob(enSuspendOption.SuspendForSeconds, "1", values);
            var manuallyResumedState = new Persistence.ManuallyResumedState("");
            client.ChangeState(jobId, manuallyResumedState, ScheduledState.StateName);

            var result = scheduler.ResumeJob(dataObjectMock.Object, jobId, false, "NewEnvironment");
            Assert.AreEqual(GlobalConstants.Failed, result);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_ResumeJob_InEnqueuedState_Failed()
        {
            var dataObjectMock = new Mock<IDSFDataObject>();
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")}
            };

            var jobstorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobstorage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobstorage);

            var jobId = scheduler.ScheduleJob(enSuspendOption.SuspendForSeconds, "1", values);
            var state = new EnqueuedState();
            client.ChangeState(jobId, state, ScheduledState.StateName);

            var result = scheduler.ResumeJob(dataObjectMock.Object, jobId, false, "NewEnvironment");
            Assert.AreEqual(GlobalConstants.Failed, result);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_ResumeJob_InSucceededState_Failed()
        {
            var dataObjectMock = new Mock<IDSFDataObject>();
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")}
            };

            var jobstorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobstorage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobstorage);

            var jobId = scheduler.ScheduleJob(enSuspendOption.SuspendForSeconds, "1", values);
            var o = new object();
            var state = new SucceededState(o, 1, 1);
            client.ChangeState(jobId, state, ScheduledState.StateName);

            var result = scheduler.ResumeJob(dataObjectMock.Object, jobId, false, "NewEnvironment");
            Assert.AreEqual(GlobalConstants.Failed, result);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_CalculateResumptionDate_SuspendUntil_Success()
        {
            var jobstorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobstorage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobstorage);
            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();
            var suspensionDate = DateTime.Now;
            var resumptionDate = scheduler.CalculateResumptionDate(suspensionDate, suspendOption, suspendOptionValue);
            Assert.AreEqual(DateTime.Parse(suspendOptionValue).ToString(), resumptionDate.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_CalculateResumptionDate_SuspendForDays_Success()
        {
            var jobstorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobstorage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobstorage);
            var suspendOption = enSuspendOption.SuspendForDays;
            var suspendOptionValue = "1";

            var suspensionDate = DateTime.Now;
            var resumptionDate = scheduler.CalculateResumptionDate(suspensionDate, suspendOption, suspendOptionValue);
            Assert.AreEqual(suspensionDate.AddDays(int.Parse(suspendOptionValue)).ToString(), resumptionDate.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_CalculateResumptionDate_SuspendForHours_Success()
        {
            var jobstorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobstorage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobstorage);
            var suspendOption = enSuspendOption.SuspendForHours;
            var suspendOptionValue = "1";

            var suspensionDate = DateTime.Now;
            var resumptionDate = scheduler.CalculateResumptionDate(suspensionDate, suspendOption, suspendOptionValue);
            Assert.AreEqual(suspensionDate.AddHours(int.Parse(suspendOptionValue)).ToString(), resumptionDate.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_CalculateResumptionDate_SuspendForMinutes_Success()
        {
            var jobstorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobstorage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobstorage);
            var suspendOption = enSuspendOption.SuspendForMinutes;
            var suspendOptionValue = "1";

            var suspensionDate = DateTime.Now;
            var resumptionDate = scheduler.CalculateResumptionDate(suspensionDate, suspendOption, suspendOptionValue);
            Assert.AreEqual(suspensionDate.AddMinutes(int.Parse(suspendOptionValue)).ToString(), resumptionDate.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_CalculateResumptionDate_SuspendForSeconds_Success()
        {
            var jobstorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobstorage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobstorage);
            var suspendOption = enSuspendOption.SuspendForSeconds;
            var suspendOptionValue = "1";

            var suspensionDate = DateTime.Now;
            var resumptionDate = scheduler.CalculateResumptionDate(suspensionDate, suspendOption, suspendOptionValue);
            Assert.AreEqual(suspensionDate.AddSeconds(int.Parse(suspendOptionValue)).ToString(), resumptionDate.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_CalculateResumptionDate_SuspendForMonths_Success()
        {
            var jobstorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobstorage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobstorage);
            var suspendOption = enSuspendOption.SuspendForMonths;
            var suspendOptionValue = "1";

            var suspensionDate = DateTime.Now;
            var resumptionDate = scheduler.CalculateResumptionDate(suspensionDate, suspendOption, suspendOptionValue);
            Assert.AreEqual(suspensionDate.AddMonths(int.Parse(suspendOptionValue)).ToString(), resumptionDate.ToString());
        }

        private static DynamicService CreateServiceEntry()
        {
            var newDs = new DynamicService {Name = HandlesType()};
            var sa = new ServiceAction {Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType()};
            newDs.Actions.Add(sa);

            return newDs;
        }

        private static string HandlesType() => "WorkflowResume";
    }
}