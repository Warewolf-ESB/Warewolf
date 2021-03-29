/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
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
using Warewolf.Driver.Persistence;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
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
        public void HangfireScheduler_ScheduleJob_Success()
        {
            var identity = new MockPrincipal();
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(identity.Name)}
            };

            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();

            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
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

            var identity = new MockPrincipal(WindowsIdentity.GetCurrent().Name);
            var currentPrincipal = new GenericPrincipal(identity, new[] {"Role1", "Roll2"});
            Thread.CurrentPrincipal = currentPrincipal;

            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns(currentPrincipal.Identity);

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.StateNotifier).Returns(mockStateNotifier.Object);
            dataObjectMock.Setup(o => o.ExecutingUser).Returns(mockPrincipal.Object);
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(currentPrincipal.Identity.Name)}
            };

            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();

            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
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

        static IExecutionEnvironment CreateExecutionEnvironment()
        {
            return new ExecutionEnvironment();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_ResumeJob_OverrideIsTrue_Success()
        {
            var executionEnvironment = CreateExecutionEnvironment();
            executionEnvironment.Assign("[[UUID]]", "public", 0);
            executionEnvironment.Assign("[[JourneyName]]", "whatever", 0);
            var env = executionEnvironment.ToJson();
            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(o => o.LogAdditionalDetail(It.IsAny<Audit>(), "ResumeJob")).Verifiable();

            var identity = new MockPrincipal(WindowsIdentity.GetCurrent().Name);
            var currentPrincipal = new GenericPrincipal(identity, new[] {"Role1", "Roll2"});
            Thread.CurrentPrincipal = currentPrincipal;

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns(currentPrincipal.Identity);

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.StateNotifier).Returns(mockStateNotifier.Object);
            dataObjectMock.Setup(o => o.ExecutingUser).Returns(mockPrincipal.Object);
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder(env)},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(currentPrincipal.Identity.Name)}
            };

            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();

            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
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

            var newexecutionEnvironment = CreateExecutionEnvironment();
            newexecutionEnvironment.Assign("[[UUID]]", "public", 0);
            newexecutionEnvironment.Assign("[[JourneyName]]", "whatever", 0);

            var result = scheduler.ResumeJob(dataObjectMock.Object, jobId, true, newexecutionEnvironment.ToJson());
            Assert.AreEqual(GlobalConstants.Success, result);

            mockResumableExecutionContainer.Verify(o => o.Execute(out errors, 0), Times.Once);
            mockStateNotifier.Verify(o => o.LogAdditionalDetail(It.IsAny<Audit>(), "ResumeJob"), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        [Timeout(120000)]
        [ExpectedException(typeof(Exception))]
        public void HangfireScheduler_ResumeJob_WorkflowResumeReturnsErrors_Failed()
        {
            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(o => o.LogAdditionalDetail(It.IsAny<Audit>(), "ResumeJob")).Verifiable();

            var identity = new MockPrincipal(WindowsIdentity.GetCurrent().Name);
            var currentPrincipal = new GenericPrincipal(identity, new[] {"Role1", "Roll2"});
            Thread.CurrentPrincipal = currentPrincipal;
            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns(currentPrincipal.Identity);

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.StateNotifier).Returns(mockStateNotifier.Object);
            dataObjectMock.Setup(o => o.ExecutingUser).Returns(mockPrincipal.Object);

            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(currentPrincipal.Identity.Name)}
            };

            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();

            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
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
            Assert.AreEqual("ErrorMessage", result);

            mockResumableExecutionContainer.Verify(o => o.Execute(out errors, 0), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        [Timeout(120000)]
        public void HangfireScheduler_GetSuspendedEnvironment_Success()
        {
            var executionEnvironment = CreateExecutionEnvironment();
            executionEnvironment.Assign("[[UUID]]", "public", 0);
            executionEnvironment.Assign("[[JourneyName]]", "whatever", 0);
            var env = executionEnvironment.ToJson();

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(o => o.LogAdditionalDetail(It.IsAny<Audit>(), "ResumeJob")).Verifiable();

            var identity = new MockPrincipal();
            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns(identity);

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.StateNotifier).Returns(mockStateNotifier.Object);
            dataObjectMock.Setup(o => o.ExecutingUser).Returns(mockPrincipal.Object);

            var resourceId = Guid.Parse("ab04663e-1e09-4338-8f61-a06a7ae5ebab");
            var startActivityId = Guid.Parse("4032a11e-4fb3-4208-af48-b92a0602ab4b");
            var versionNumber = 1;
            var executingUser = identity.Name;
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(resourceId.ToString())},
                {"environment", new StringBuilder(env)},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder(versionNumber.ToString())},
                {"currentuserprincipal", new StringBuilder(executingUser)}
            };

            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();

            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            mockPersistedValues.Setup(o => o.ResourceId).Returns(resourceId);
            mockPersistedValues.Setup(o => o.SuspendedEnvironment).Returns(env);
            mockPersistedValues.Setup(o => o.StartActivityId).Returns(startActivityId);
            mockPersistedValues.Setup(o => o.VersionNumber).Returns(versionNumber);
            mockPersistedValues.Setup(o => o.ExecutingUser).Returns(mockPrincipal.Object);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
            var jobId = scheduler.ScheduleJob(suspendOption, suspendOptionValue, values);


             var persistedValues = scheduler.GetPersistedValues(jobId);
             Assert.AreEqual(resourceId, persistedValues.ResourceId);
             Assert.AreEqual(env, persistedValues.SuspendedEnvironment);
             Assert.AreEqual(startActivityId, persistedValues.StartActivityId);
             Assert.AreEqual(versionNumber, persistedValues.VersionNumber);
             Assert.AreEqual(executingUser, persistedValues.ExecutingUser.Identity.Name);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        [Timeout(120000)]
        [ExpectedException(typeof(Exception))]
        public void HangfireScheduler_GetPersistedValues_InvalidSuspendID_Fails()
        {
            var executionEnvironment = CreateExecutionEnvironment();
            executionEnvironment.Assign("[[UUID]]", "public", 0);
            executionEnvironment.Assign("[[JourneyName]]", "whatever", 0);
            var env = executionEnvironment.ToJson();

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
                {"environment", new StringBuilder(env)},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(WindowsIdentity.GetCurrent().Name)}
            };
            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
            var result = scheduler.GetPersistedValues("100050");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        [Timeout(120000)]
        [ExpectedException(typeof(Exception))]
        public void HangfireScheduler_GetPersistedValues_EnqueuedState_Fails()
        {
            var executionEnvironment = CreateExecutionEnvironment();
            executionEnvironment.Assign("[[UUID]]", "public", 0);
            executionEnvironment.Assign("[[JourneyName]]", "whatever", 0);
            var env = executionEnvironment.ToJson();

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(o => o.LogAdditionalDetail(It.IsAny<Audit>(), "ResumeJob")).Verifiable();
            var identity = new MockPrincipal();
            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns(identity);

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.StateNotifier).Returns(mockStateNotifier.Object);
            dataObjectMock.Setup(o => o.ExecutingUser).Returns(mockPrincipal.Object);

            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder(env)},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(identity.Name)}
            };

            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();

            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
            var jobId = scheduler.ScheduleJob(suspendOption, suspendOptionValue, values);

            var state = new EnqueuedState();
            client.ChangeState(jobId, state, ScheduledState.StateName);

            var result = scheduler.GetPersistedValues(jobId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        [Timeout(120000)]
        [ExpectedException(typeof(Exception))]
        public void HangfireScheduler_GetPersistedValues_ManuallyResumedState_Fails()
        {
            var executionEnvironment = CreateExecutionEnvironment();
            executionEnvironment.Assign("[[UUID]]", "public", 0);
            executionEnvironment.Assign("[[JourneyName]]", "whatever", 0);
            var env = executionEnvironment.ToJson();

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(o => o.LogAdditionalDetail(It.IsAny<Audit>(), "ResumeJob")).Verifiable();
            var identity = new MockPrincipal();
            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns(identity);

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.StateNotifier).Returns(mockStateNotifier.Object);
            dataObjectMock.Setup(o => o.ExecutingUser).Returns(mockPrincipal.Object);

            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder(env)},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(identity.Name)}
            };

            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();

            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
            var jobId = scheduler.ScheduleJob(suspendOption, suspendOptionValue, values);

            var state = new Persistence.ManuallyResumedState("");
            client.ChangeState(jobId, state, ScheduledState.StateName);

            var result = scheduler.GetPersistedValues(jobId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        [ExpectedException(typeof(Exception))]
        public void HangfireScheduler_ResumeJob_InManuallyResumedState_Failed()
        {
            var dataObjectMock = new Mock<IDSFDataObject>();
            var identity = new MockPrincipal();
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(identity.Name)}
            };

            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);

            var jobId = scheduler.ScheduleJob(enSuspendOption.SuspendForSeconds, "1", values);
            var manuallyResumedState = new Persistence.ManuallyResumedState("");
            client.ChangeState(jobId, manuallyResumedState, ScheduledState.StateName);

            var result = scheduler.ResumeJob(dataObjectMock.Object, jobId, false, "NewEnvironment");
            Assert.AreEqual("Failed: " + ErrorResource.ManualResumptionAlreadyResumed, result);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        [ExpectedException(typeof(Exception))]
        public void HangfireScheduler_ResumeJob_InvalidSuspendId_Failed()
        {
            var dataObjectMock = new Mock<IDSFDataObject>();
            var identity = new MockPrincipal();
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(identity.Name)}
            };

            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
            var result = scheduler.ResumeJob(dataObjectMock.Object, "321654", false, "NewEnvironment");
            Assert.AreEqual("Failed: " + ErrorResource.ManualResumptionSuspensionEnvBlank, result);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        [ExpectedException(typeof(Exception))]
        public void HangfireScheduler_ResumeJob_InEnqueuedState_Failed()
        {
            var dataObjectMock = new Mock<IDSFDataObject>();
            var identity = new MockPrincipal();
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(identity.Name)}
            };

            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);

            var jobId = scheduler.ScheduleJob(enSuspendOption.SuspendForSeconds, "1", values);
            var state = new EnqueuedState();
            client.ChangeState(jobId, state, ScheduledState.StateName);

            var result = scheduler.ResumeJob(dataObjectMock.Object, jobId, false, "NewEnvironment");
            Assert.AreEqual("Failed: " + ErrorResource.ManualResumptionEnqueued, result);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        [ExpectedException(typeof(Exception))]
        public void HangfireScheduler_ResumeJob_InSucceededState_Failed()
        {
            var dataObjectMock = new Mock<IDSFDataObject>();
            var identity = new MockPrincipal();
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(identity.Name)}
            };

            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);

            var jobId = scheduler.ScheduleJob(enSuspendOption.SuspendForSeconds, "1", values);
            var o = new object();
            var state = new SucceededState(o, 1, 1);
            client.ChangeState(jobId, state, ScheduledState.StateName);

            var result = scheduler.ResumeJob(dataObjectMock.Object, jobId, false, "NewEnvironment");
            Assert.AreEqual("Failed: " + ErrorResource.ManualResumptionAlreadyResumed, result);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_CalculateResumptionDate_SuspendUntil_Success()
        {
            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
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
            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
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
            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
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
            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
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
            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
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
            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
            var suspendOption = enSuspendOption.SuspendForMonths;
            var suspendOptionValue = "1";

            var suspensionDate = DateTime.Now;
            var resumptionDate = scheduler.CalculateResumptionDate(suspensionDate, suspendOption, suspendOptionValue);
            Assert.AreEqual(suspensionDate.AddMonths(int.Parse(suspendOptionValue)).ToString(), resumptionDate.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_ResumeJob_FromFailedState_Success()
        {
            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(o => o.LogAdditionalDetail(It.IsAny<Audit>(), "ResumeJob")).Verifiable();

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.StateNotifier).Returns(mockStateNotifier.Object);

            var identity = new MockPrincipal(WindowsIdentity.GetCurrent().Name);
            var currentPrincipal = new GenericPrincipal(identity, new[] {"Role1", "Roll2"});
            Thread.CurrentPrincipal = currentPrincipal;

            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("NewEnvironment")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(currentPrincipal.Identity.Name)}
            };

            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();

            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);
            var jobId = scheduler.ScheduleJob(suspendOption, suspendOptionValue, values);

            var state = new EnqueuedState();
            client.ChangeState(jobId, state, FailedState.StateName);

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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(HangfireScheduler))]
        [ExpectedException(typeof(Exception))]
        public void HangfireScheduler_ManualResumeWithOverrideJob_HasErrors_True()
        {
            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);

            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();
            var identity = new MockPrincipal();
            var expectedStartActivityId = "";
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder(expectedStartActivityId)},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(identity.Name)}
            };

            var mockEnvironment = new Mock<IExecutionEnvironment>();
            mockEnvironment.Setup(o => o.HasErrors()).Returns(true);
            mockEnvironment.Setup(o => o.FetchErrors()).Returns("error message");

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment).Returns(mockEnvironment.Object);

            var jobId = scheduler.ScheduleJob(suspendOption, suspendOptionValue, values);
            scheduler.ManualResumeWithOverrideJob(mockDataObject.Object, jobId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_ManualResumeWithOverrideJob_HasErrors_False()
        {
            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);

            var suspendOption = enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();
            var identity = new MockPrincipal();
            var expectedStartActivityId = "";
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder(expectedStartActivityId)},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(identity.Name)}
            };

            var mockEnvironment = new Mock<IExecutionEnvironment>();
            mockEnvironment.Setup(o => o.HasErrors()).Returns(false);

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment).Returns(mockEnvironment.Object);

            var jobId = scheduler.ScheduleJob(suspendOption, suspendOptionValue, values);
            var withOverrideJob = scheduler.ManualResumeWithOverrideJob(mockDataObject.Object, jobId);
            Assert.AreEqual(GlobalConstants.Success, withOverrideJob);
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
    public enum MockPrincipalBehavior
    {
        AlwaysReturnTrue,
        WhiteList,
        BlackList
    }

    public class MockPrincipal : IPrincipal, IIdentity
    {
        private HashSet<String> Roles { get; set; }
        public MockPrincipalBehavior Behavior { get; set; }

        public MockPrincipal(String name = "TestUser", MockPrincipalBehavior behavior = MockPrincipalBehavior.AlwaysReturnTrue)
        {
            Roles = new HashSet<String>();
            Name = name;
            IsAuthenticated = true;
            AuthenticationType = "FakeAuthentication";
        }

        public void AddRoles(params String[] roles)
        {
            Behavior = MockPrincipalBehavior.WhiteList;

            if (roles == null || roles.Length == 0) return;

            var rolesToAdd = roles.Where(r => !Roles.Contains(r));

            foreach (var role in rolesToAdd)
                Roles.Add(role);
        }

        public void IgnoreRoles(params String[] roles)
        {
            Behavior = MockPrincipalBehavior.BlackList;

            AddRoles(roles);
        }

        public void RemoveRoles(params String[] roles)
        {
            if (roles == null || roles.Length == 0) return;

            var rolesToAdd = roles.Where(r => Roles.Contains(r));

            foreach (var role in rolesToAdd)
                Roles.Remove(role);
        }

        public void RemoveAllRoles()
        {
            Roles.Clear();
        }

        #region IPrincipal Members

        public IIdentity Identity
        {
            get { return this; }
        }

        public bool IsInRole(string role)
        {
            if (Behavior == MockPrincipalBehavior.AlwaysReturnTrue)
                return true;

            var isInlist = Roles.Contains(role);

            if (Behavior == MockPrincipalBehavior.BlackList)
                return !isInlist;

            return isInlist;
        }

        #endregion

        #region IIdentity Members

        public string AuthenticationType { get; set; }

        public bool IsAuthenticated { get; set; }

        public string Name { get; set; }

        #endregion
    }
}