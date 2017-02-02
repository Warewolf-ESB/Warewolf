/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.Communication;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Scheduler.Test
{
    [TestClass]
    public class ScheduledResourceModelTest
    {
        private Mock<IDev2TaskService> _mockService;
        private string _folderId;
        private string _agentPath;
        private Mock<ITaskServiceConvertorFactory> _convertorFactory;
        private Mock<ITaskFolder> _folder;
        private Mock<ISecurityWrapper> _wrapper;

        [TestInitialize]
        public void Init()
        {
            _mockService = new Mock<IDev2TaskService>();

            _convertorFactory = new Mock<ITaskServiceConvertorFactory>();
            _folderId = "WareWolf";
            _agentPath = "AgentPath";
            _folder = new Mock<ITaskFolder>();
            _wrapper = new Mock<ISecurityWrapper>();
            _wrapper.Setup(a => a.IsWindowsAuthorised(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(true);
            _wrapper.Setup(a => a.IsWarewolfAuthorised(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(true);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_Constructor")]
        public void ScheduledResourceModel_Constructor_ShouldConstruct()
        {
            _mockService.Setup(a => a.GetFolder(_folderId)).Returns(_folder.Object);
            _folder.Setup(a => a.ValidTasks).Returns(new List<IDev2Task>());
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   @"c:\", _wrapper.Object, a => a.WorkflowName);
            Assert.AreEqual(_convertorFactory.Object, model.ConvertorFactory);
            Assert.AreEqual(_folderId, model.WarewolfFolderPath);
            Assert.AreEqual(_agentPath, model.WarewolfAgentPath);
            Assert.AreEqual(_mockService.Object, model.TaskService);
            Assert.AreEqual(0, model.ScheduledResources.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScheduledResourceModel_Constructor_ShouldThrowErrorIfArgsNull()
        {
            _mockService.Setup(a => a.GetFolder(_folderId)).Returns(_folder.Object);
            _folder.Setup(a => a.ValidTasks).Returns(new List<IDev2Task>());
            try
            {
                // ReSharper disable ObjectCreationAsStatement
                new ScheduledResourceModel(null, null, null, null, null, null, null);
                // ReSharper restore ObjectCreationAsStatement
            }
            catch (Exception e)
            {
                var actual = e.Message;
                var expected = @"The following arguments are not allowed to be null: taskService
warewolfFolderId
warewolfAgentPath
taskServiceFactory
debugHistoryPath
securityWrapper
";
                FixBreaks(ref expected, ref actual);
                Assert.AreEqual(expected, actual);
                throw;
            }


        }
        private void FixBreaks(ref string expected, ref string actual)
        {
            expected = new StringBuilder(expected).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
            actual = new StringBuilder(actual).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_ScheduledResources")]
        public void ScheduledResourceModel_Constructor_ShouldSelectedCorrectResources()
        {
            //setup
            SetupSingleTask();
            //create
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath,
                                                   _convertorFactory.Object, @"c:\", _wrapper.Object, a => a.WorkflowName);

            Assert.AreEqual(1,
                            model.ScheduledResources.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_ScheduledResources")]
        public void ScheduledResourceModel_Constructor_ShouldSelectedCorrectResourcesWithId()
        {
            //setup
            SetupSingleTaskWithId();
            //create
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath,
                                                   _convertorFactory.Object, @"c:\", _wrapper.Object, a => a.WorkflowName);

            Assert.AreEqual(1,
                            model.ScheduledResources.Count);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_ScheduledResources")]
        public void ScheduledResourceModel_Get_ShouldErrorInvalidMessages()
        {
            //setup
            SetupSingleTask();
            //create
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath,
                                                   _convertorFactory.Object, @"c:\", _wrapper.Object, a => a.WorkflowName);

            Assert.AreEqual(1,
                            model.ScheduledResources.Count);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_ScheduledResources")]
        public void ScheduledResourceModel_CorrectSelectedResources()
        {
            SetupSingleTask();

            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   @"c:\", _wrapper.Object, d => d.WorkflowName);
            IScheduledResource a = model.ScheduledResources.First();
            Assert.AreEqual("bob", a.Name);
            Assert.AreEqual("a", a.WorkflowName);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_Delete")]
        public void ScheduledResourceModel_ShouldDeleteTest_Valid()
        {
            SetupSingleTask();

            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   @"c:\", _wrapper.Object, a => a.WorkflowName);
            _mockService.Setup(a => a.GetFolder(_folderId)).Returns(_folder.Object);
            _folder.Setup(a => a.ValidTasks).Returns(new List<IDev2Task>());
            var mockFolder = new Mock<ITaskFolder>();

            _mockService.Setup(a => a.GetFolder("WareWolf")).Returns(mockFolder.Object);
            mockFolder.Setup(a => a.TaskExists("Dora")).Returns(true);
            mockFolder.Setup(a => a.DeleteTask("Dora", false)).Verifiable();
            var resource = new Mock<IScheduledResource>();
            resource.Setup(a => a.Name).Returns("Dora");
            model.DeleteSchedule(resource.Object);
            mockFolder.Verify(a => a.DeleteTask("Dora", false));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_Delete")]
        public void ScheduledResourceModel_Delete_InValid()
        {
            //setup
            SetupSingleTask();
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   @"c:\", _wrapper.Object, a => a.WorkflowName);
            var mockFolder = new Mock<ITaskFolder>();
            var resource = new Mock<IScheduledResource>();

            //expectations
            _mockService.Setup(a => a.GetFolder(_folderId)).Returns(_folder.Object);
            _folder.Setup(a => a.ValidTasks).Returns(new List<IDev2Task>());
            _mockService.Setup(a => a.GetFolder("WareWolf")).Returns(mockFolder.Object);
            mockFolder.Setup(a => a.TaskExists("Dora")).Returns(false);
            mockFolder.Setup(a => a.DeleteTask("Dora", false)).Verifiable();
            resource.Setup(a => a.Name).Returns("Dora");

            //test
            model.DeleteSchedule(resource.Object);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_History")]
        public void ScheduledResourceModel_HistoryTest_CorrectTaskEventsSelected()
        {
            // setup 
            var startTime = new DateTime(2000, 1, 1);
            var endTime = new DateTime(2003, 1, 1);
            var log = new MockTaskEventLog
                {
                    new MockTaskEvent(Guid.NewGuid(), 12, "Task Started", startTime, "12345", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "2", new DateTime(2001, 1, 1), "12346", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "3", new DateTime(2002, 1, 1), "12347", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "Task Completed", endTime, "12348", "dave")
                };
            var dirHelper = new Mock<IDirectoryHelper>();
            var fileHelper = new Mock<IFileHelper>();
            var res = new Mock<IScheduledResource>();
            //setup expectancies
            dirHelper.Setup(a => a.GetFiles(@"c:\")).Returns(new[] { "b_12345_Bob" });
            fileHelper.Setup(a => a.ReadAllText("b_12345_Bob")).Returns("");
            res.Setup(a => a.Name).Returns("Bob");
            _convertorFactory.Setup(a => a.CreateTaskEventLog(It.IsAny<string>())).Returns(log);

            var history = RunOutput(startTime, endTime);
            Assert.AreEqual(startTime, history.First().DebugOutput.First().StartTime);
            Assert.AreEqual(endTime, history.Last().DebugOutput.First().EndTime);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_History")]
        public void ScheduledResourceModel_HistoryTestDebugCreated()
        {
            var startTime = new DateTime(2000, 1, 1);
            var endTime = new DateTime(2003, 1, 1);
            //setup
            var log = new MockTaskEventLog
                {
                    new MockTaskEvent(Guid.NewGuid(), 12, "Task Started", startTime, "12345", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "2", new DateTime(2001, 1, 1), "12345", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "3", new DateTime(2002, 1, 1), "12345", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "Task Completed", endTime, "12345", "dave")
                };
            // this should return two history items without any debug output
            var dirHelper = new Mock<IDirectoryHelper>();
            var fileHelper = new Mock<IFileHelper>();
            var res = new Mock<IScheduledResource>();

            //expectations
            dirHelper.Setup(a => a.GetFiles(@"c:\")).Returns(new[] { "b_12345_Bob" });
            const string content = "[{\"$type\":\"Dev2.Diagnostics.Debug.DebugState, Dev2.Diagnostics\",\"ID\":\"cd902be2-a202-4d54-8c07-c5f56bae97fe\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"EnvironmentID\":\"00000000-0000-0000-0000-000000000000\",\"ClientID\":\"00000000-0000-0000-0000-000000000000\",\"StateType\":64,\"DisplayName\":\"dave\",\"HasError\":false,\"ErrorMessage\":\"Service [ dave ] not found.\",\"Version\":\"\",\"Name\":\"DynamicServicesInvoker\",\"ActivityType\":0,\"Duration\":\"00:00:00\",\"DurationString\":\"PT0S\",\"StartTime\":\"2014-03-20T17:23:14.0224329+02:00\",\"EndTime\":\"2014-03-20T17:23:14.0224329+02:00\",\"Inputs\":[],\"Outputs\":[],\"Server\":\"\",\"WorkspaceID\":\"00000000-0000-0000-0000-000000000000\",\"OriginalInstanceID\":\"00000000-0000-0000-0000-000000000000\",\"OriginatingResourceID\":\"00000000-0000-0000-0000-000000000000\",\"IsSimulation\":false,\"Message\":null,\"NumberOfSteps\":0,\"Origin\":\"\",\"ExecutionOrigin\":0,\"ExecutionOriginDescription\":null,\"ExecutingUser\":null,\"SessionID\":\"00000000-0000-0000-0000-000000000000\"}]";
            fileHelper.Setup(a => a.ReadAllText("b_12345_Bob"))
                      .Returns(content);
            res.Setup(a => a.Name).Returns("Bob");
            _convertorFactory.Setup(a => a.CreateTaskEventLog(It.IsAny<string>())).Returns(log);

            //test
            // ReSharper disable UseObjectOrCollectionInitializer
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   // ReSharper restore UseObjectOrCollectionInitializer
                                                   @"c:\", _wrapper.Object, a => a.WorkflowName);
            model.DirectoryHelper = dirHelper.Object;
            model.FileHelper = fileHelper.Object;
            //var history = RunOutput(startTime, endTime, "Bob");
            var serializer = new Dev2JsonSerializer();
            var debugStates = serializer.Deserialize<List<IDebugState>>(content).First();
            IList<IResourceHistory> history = model.CreateHistory(res.Object);
            Assert.AreEqual(1, history.Count);
            Assert.AreEqual(debugStates.StartTime, history.First().DebugOutput.First().StartTime);
            Assert.AreEqual(debugStates.EndTime, history.First().DebugOutput.First().EndTime);
            Assert.AreEqual(history.Last().DebugOutput.Count, 1);
            Assert.AreEqual("Bob", history.Last().UserName);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_History")]
        public void ScheduledResourceModel_HistoryTestStatusNotFoundIfNoDebugDebugCreated()
        {
            var startTime = new DateTime(2000, 1, 1);
            var endTime = new DateTime(2003, 1, 1);
            //setup
            var log = new MockTaskEventLog
                {
                    new MockTaskEvent(Guid.NewGuid(), 12, "Task Started", startTime, "12345", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "2", new DateTime(2001, 1, 1), "12346", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "3", new DateTime(2002, 1, 1), "12347", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "Task Completed", endTime, "12348", "dave")
                };
            // this should return two history items without any debug output
            var dirHelper = new Mock<IDirectoryHelper>();
            var fileHelper = new Mock<IFileHelper>();
            var res = new Mock<IScheduledResource>();

            //expectations
            dirHelper.Setup(a => a.GetFiles(@"c:\")).Returns(new[] { "" });
            fileHelper.Setup(a => a.ReadAllText("b_12345_Bob"))
                      .Returns(
                          "[{\"$type\":\"Dev2.Diagnostics.Debug.DebugState, Dev2.Diagnostics\",\"ID\":\"cd902be2-a202-4d54-8c07-c5f56bae97fe\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"EnvironmentID\":\"00000000-0000-0000-0000-000000000000\",\"ClientID\":\"00000000-0000-0000-0000-000000000000\",\"StateType\":64,\"DisplayName\":\"dave\",\"HasError\":true,\"ErrorMessage\":\"Service [ dave ] not found.\",\"Version\":\"\",\"Name\":\"DynamicServicesInvoker\",\"ActivityType\":0,\"Duration\":\"00:00:00\",\"DurationString\":\"PT0S\",\"StartTime\":\"2014-03-20T17:23:14.0224329+02:00\",\"EndTime\":\"2014-03-20T17:23:14.0224329+02:00\",\"Inputs\":[],\"Outputs\":[],\"Server\":\"\",\"WorkspaceID\":\"00000000-0000-0000-0000-000000000000\",\"OriginalInstanceID\":\"00000000-0000-0000-0000-000000000000\",\"OriginatingResourceID\":\"00000000-0000-0000-0000-000000000000\",\"IsSimulation\":false,\"Message\":null,\"NumberOfSteps\":0,\"Origin\":\"\",\"ExecutionOrigin\":0,\"ExecutionOriginDescription\":null,\"ExecutingUser\":null,\"SessionID\":\"00000000-0000-0000-0000-000000000000\"}]");
            res.Setup(a => a.Name).Returns("Bob");
            _convertorFactory.Setup(a => a.CreateTaskEventLog(It.IsAny<string>())).Returns(log);

            //test
            // ReSharper disable UseObjectOrCollectionInitializer
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   // ReSharper restore UseObjectOrCollectionInitializer
                                                   @"c:\", _wrapper.Object, a => a.WorkflowName);
            model.DirectoryHelper = dirHelper.Object;
            model.FileHelper = fileHelper.Object;
            IList<IResourceHistory> history = model.CreateHistory(res.Object);
            //WE ONLY RETURN EXECUTED HISTORY
            Assert.AreEqual(0, history.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_History")]
        public void ScheduledResourceModel_HistoryTestStatusFailedWindowsSchedulerError()
        {
            //setup
            var log = new MockTaskEventLog
                {
                    new MockTaskEvent(Guid.NewGuid(), 104, "Task Started", new DateTime(2000, 1, 1), "12345", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 104, "2", new DateTime(2001, 1, 1), "12346", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 104, "3", new DateTime(2002, 1, 1), "12347", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 104, "Task Completed", new DateTime(2003, 1, 1), "12348", "dave")
                };
            // this should return two history items without any debug output
            var dirHelper = new Mock<IDirectoryHelper>();
            var fileHelper = new Mock<IFileHelper>();
            var res = new Mock<IScheduledResource>();

            //expectations
            dirHelper.Setup(a => a.GetFiles(@"c:\")).Returns(new[] { "b_12345_Bob" });
            fileHelper.Setup(a => a.ReadAllText("b_12345_Bob"))
                      .Returns(
                          "[{\"$type\":\"Dev2.Diagnostics.Debug.DebugState, Dev2.Diagnostics\",\"ID\":\"cd902be2-a202-4d54-8c07-c5f56bae97fe\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"EnvironmentID\":\"00000000-0000-0000-0000-000000000000\",\"ClientID\":\"00000000-0000-0000-0000-000000000000\",\"StateType\":64,\"DisplayName\":\"dave\",\"HasError\":true,\"ErrorMessage\":\"Service [ dave ] not found.\",\"Version\":\"\",\"Name\":\"DynamicServicesInvoker\",\"ActivityType\":0,\"Duration\":\"00:00:00\",\"DurationString\":\"PT0S\",\"StartTime\":\"2014-03-20T17:23:14.0224329+02:00\",\"EndTime\":\"2014-03-20T17:23:14.0224329+02:00\",\"Inputs\":[],\"Outputs\":[],\"Server\":\"\",\"WorkspaceID\":\"00000000-0000-0000-0000-000000000000\",\"OriginalInstanceID\":\"00000000-0000-0000-0000-000000000000\",\"OriginatingResourceID\":\"00000000-0000-0000-0000-000000000000\",\"IsSimulation\":false,\"Message\":null,\"NumberOfSteps\":0,\"Origin\":\"\",\"ExecutionOrigin\":0,\"ExecutionOriginDescription\":null,\"ExecutingUser\":null,\"SessionID\":\"00000000-0000-0000-0000-000000000000\"}]");
            res.Setup(a => a.Name).Returns("Bob");
            _convertorFactory.Setup(a => a.CreateTaskEventLog(It.IsAny<string>())).Returns(log);

            //test
            // ReSharper disable UseObjectOrCollectionInitializer
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   // ReSharper restore UseObjectOrCollectionInitializer
                                                   @"c:\", _wrapper.Object, a => a.WorkflowName);
            model.DirectoryHelper = dirHelper.Object;
            model.FileHelper = fileHelper.Object;
            IList<IResourceHistory> history = model.CreateHistory(res.Object);

            Assert.AreEqual(1, history.Count);
            Assert.AreEqual(ScheduleRunStatus.Error, history.Last().TaskHistoryOutput.Success);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_History")]
        public void ScheduledResourceModel_HistoryTestDebugCreated_StatusFailureIfDebugHasError()
        {
            //setup
            var log = new MockTaskEventLog
                {
                    new MockTaskEvent(Guid.NewGuid(), 12, "Task Started", new DateTime(2000, 1, 1), "12345", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "2", new DateTime(2001, 1, 1), "12346", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "3", new DateTime(2002, 1, 1), "12347", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "Task Completed", new DateTime(2003, 1, 1), "12348", "dave")
                };
            // this should return two history items without any debug output
            var dirHelper = new Mock<IDirectoryHelper>();
            var fileHelper = new Mock<IFileHelper>();
            var res = new Mock<IScheduledResource>();

            //expectations
            dirHelper.Setup(a => a.GetFiles(@"c:\")).Returns(new[] { "b_12345_Bob" });
            fileHelper.Setup(a => a.ReadAllText("b_12345_Bob"))
                      .Returns(
                          "[{\"$type\":\"Dev2.Diagnostics.Debug.DebugState, Dev2.Diagnostics\",\"ID\":\"05d4e815-61bf-49ad-b46c-b6f0e0e2e839\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"EnvironmentID\":\"00000000-0000-0000-0000-000000000000\",\"ClientID\":\"00000000-0000-0000-0000-000000000000\",\"StateType\":64,\"DisplayName\":\"BUGS/Bug_11889\",\"HasError\":true,\"ErrorMessage\":\"Service [ BUGS/Bug_11889 ] not found.\",\"Version\":\"\",\"Name\":\"EsbServiceInvoker\",\"ActivityType\":0,\"Duration\":\"00:00:00\",\"DurationString\":\"PT0S\",\"StartTime\":\"2014-07-24T12:49:28.4006805+02:00\",\"EndTime\":\"2014-07-24T12:49:28.4006805+02:00\",\"Inputs\":[],\"Outputs\":[],\"Server\":\"\",\"WorkspaceID\":\"00000000-0000-0000-0000-000000000000\",\"OriginalInstanceID\":\"00000000-0000-0000-0000-000000000000\",\"OriginatingResourceID\":\"00000000-0000-0000-0000-000000000000\",\"IsSimulation\":false,\"Message\":null,\"NumberOfSteps\":0,\"Origin\":\"\",\"ExecutionOrigin\":0,\"ExecutionOriginDescription\":null,\"ExecutingUser\":null,\"SessionID\":\"00000000-0000-0000-0000-000000000000\",\"WorkSurfaceMappingId\":\"00000000-0000-0000-0000-000000000000\"}]");
            res.Setup(a => a.Name).Returns("Bob");
            _convertorFactory.Setup(a => a.CreateTaskEventLog(It.IsAny<string>())).Returns(log);

            //test
            // ReSharper disable UseObjectOrCollectionInitializer
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   // ReSharper restore UseObjectOrCollectionInitializer
                                                   @"c:\", _wrapper.Object, a => a.WorkflowName);
            model.DirectoryHelper = dirHelper.Object;
            model.FileHelper = fileHelper.Object;
            IList<IResourceHistory> history = model.CreateHistory(res.Object);

            Assert.AreEqual(1, history.Count);
            Assert.AreEqual(history.Last().DebugOutput.Count, 1);
            Assert.AreEqual("Bob", history.Last().UserName);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_Save")]
        public void ScheduledResourceModel_SaveTestValid()
        {
            //create objects
            SetupSingleTask();
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                  @"c:\", _wrapper.Object, a => a.WorkflowName);
            var task = new Mock<IDev2TaskDefinition>();
            var resourceToSave = new Mock<IScheduledResource>();
            var action = new Mock<IExecAction>();
            var mockSettings = new Mock<ITaskSettings>();
            var trigger = new Mock<IScheduleTrigger>();
            var nativeTrigger = new Mock<ITrigger>();

            //setup expectations
            _convertorFactory.Setup(a => a.CreateExecAction("WareWolf", "a b", null)).Returns(action.Object);
            resourceToSave.Setup(a => a.Name).Returns("henry");
            resourceToSave.Setup(a => a.Trigger).Returns(trigger.Object);
            resourceToSave.Setup(a => a.WorkflowName).Returns("b");
            resourceToSave.Setup(a => a.UserName).Returns("user");
            resourceToSave.Setup(a => a.Password).Returns("pwd");
            task.Setup(a => a.Settings).Returns(mockSettings.Object);
            trigger.Setup(a => a.Trigger).Returns(nativeTrigger.Object);
            task.Setup(a => a.AddTrigger(nativeTrigger.Object)).Verifiable();
            _mockService.Setup(a => a.NewTask()).Returns(task.Object);
            _mockService.Setup(a => a.GetFolder(_folderId)).Returns(_folder.Object);
            task.Setup(a => a.AddTrigger(nativeTrigger.Object)).Verifiable();
            _folder.Setup(a => a.ValidTasks).Returns(new List<IDev2Task>());
            var mockFolder = new Mock<ITaskFolder>();
            _mockService.Setup(a => a.GetFolder("WareWolf")).Returns(mockFolder.Object);
            var resource = new Mock<IScheduledResource>();
            resource.Setup(a => a.Name).Returns("Dora");
            string errorMessage;
            //run test
            model.Save(resourceToSave.Object, out errorMessage);
            mockFolder.Verify(
                a =>
                a.RegisterTaskDefinition("henry", task.Object, TaskCreation.CreateOrUpdate, "user", "pwd",
                                         TaskLogonType.InteractiveTokenOrPassword, null));
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_Save")]
        public void ScheduledResourceModel_SaveInvalidWindowsUserPermissions()
        {
            //create objects
            SetupSingleTask();
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                  @"c:\", _wrapper.Object, a => a.WorkflowName);
            var resourceToSave = new Mock<IScheduledResource>();

            //setup expectations
            _wrapper.Setup(
                a => a.IsWindowsAuthorised(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(false);
            //run test
            string errorMessage;
            model.Save(resourceToSave.Object, out errorMessage);
            Assert.AreEqual(Warewolf.Resource.Errors.ErrorResource.ScheduledResourceLogOnAsBatchErrorTest, errorMessage);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_Save")]
        public void ScheduledResourceModel_SaveInvalidWarewolfUserPermissions()
        {
            //create objects
            SetupSingleTask();
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                  @"c:\", _wrapper.Object, a => a.WorkflowName);
            var resourceToSave = new Mock<IScheduledResource>();

            //setup expectations
            _wrapper.Setup(
                a => a.IsWarewolfAuthorised(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(false);
            resourceToSave.Setup(a => a.WorkflowName).Returns("bob");
            //run test
            string errorMessage;
            model.Save(resourceToSave.Object, out errorMessage);
            Assert.AreEqual(Warewolf.Resource.Errors.ErrorResource.ScheduledResourceInvalidUserPermissionErrorTest, errorMessage);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_Save")]
        public void ScheduledResourceModel_SaveInvalidName()
        {
            //create objects
            SetupSingleTask();
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                  @"c:\", _wrapper.Object, a => a.WorkflowName);
            var resourceToSave = new Mock<IScheduledResource>();

            //setup expectations
            _wrapper.Setup(
                a => a.IsWarewolfAuthorised(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(true);
            resourceToSave.Setup(a => a.Name).Returns("bob?");
            //run test
            string errorMessage;
            model.Save(resourceToSave.Object, out errorMessage);
            Assert.AreEqual("The task name may not contain the following characters \\/:*?\"<>| .", errorMessage);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ScheduledResourceModel_Save")]
        public void ScheduledResourceModel_SaveTest_UserPassword()
        {
            SetupSingleTask();
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   @"c:\", _wrapper.Object, a => a.WorkflowName);

            var task = new Mock<IDev2TaskDefinition>();
            var resourceToSave = new Mock<IScheduledResource>();
            var action = new Mock<IExecAction>();

            var trigger = new Mock<IScheduleTrigger>();
            var nativeTrigger = new Mock<ITrigger>();
            var mockSettings = new Mock<ITaskSettings>();
            var resource = new Mock<IScheduledResource>();
            _convertorFactory.Setup(a => a.CreateExecAction("WareWolf", "a b", null)).Returns(action.Object);
            resourceToSave.Setup(a => a.Name).Returns("henry");
            resourceToSave.Setup(a => a.Trigger).Returns(trigger.Object);
            resourceToSave.Setup(a => a.WorkflowName).Returns("b");
            task.Setup(a => a.Settings).Returns(mockSettings.Object);
            trigger.Setup(a => a.Trigger).Returns(nativeTrigger.Object);
            task.Setup(a => a.AddTrigger(nativeTrigger.Object)).Verifiable();
            _mockService.Setup(a => a.NewTask()).Returns(task.Object);
            _mockService.Setup(a => a.GetFolder(_folderId)).Returns(_folder.Object);
            task.Setup(a => a.AddTrigger(nativeTrigger.Object)).Verifiable();
            _folder.Setup(a => a.ValidTasks).Returns(new List<IDev2Task>());
            var mockFolder = new Mock<ITaskFolder>();
            _mockService.Setup(a => a.GetFolder("WareWolf")).Returns(mockFolder.Object);
            resource.Setup(a => a.Name).Returns("Dora");


            model.Save(resourceToSave.Object, "user", "pwd");
            mockFolder.Verify(
                a =>
                a.RegisterTaskDefinition("henry", task.Object, TaskCreation.CreateOrUpdate, "user", "pwd",
                                         TaskLogonType.InteractiveTokenOrPassword, null));
            task.Verify(a => a.AddTrigger(It.IsAny<ITrigger>()));
        }


        private void SetupSingleTask()
        {
            _mockService.Setup(a => a.GetFolder(_folderId)).Returns(_folder.Object);
            var task1 = new Mock<IDev2Task>();
            var task2 = new Mock<IDev2Task>();
            var action1 = new Mock<IExecAction>();
            var trigger1 = new Mock<ITrigger>();
            var definition = new Mock<IDev2TaskDefinition>();
            var settings = new Mock<ITaskSettings>();
            definition.Setup(a => a.Data).Returns("bob");
            definition.Setup(a => a.Settings).Returns(settings.Object);
            settings.Setup(a => a.Enabled).Returns(true);

            _convertorFactory.Setup(a => a.CreateExecAction(action1.Object)).Returns(action1.Object);
            task1.Setup(a => a.IsValidDev2Task()).Returns(true);
            task1.Setup(a => a.Definition).Returns(definition.Object);
            task2.Setup(a => a.IsValidDev2Task()).Returns(false);
            task1.Setup(a => a.Action).Returns(action1.Object);
            task1.Setup(a => a.Trigger).Returns(trigger1.Object);
            action1.Setup(a => a.Arguments).Returns("\"Workflow:a\" \"TaskName:b\"");
            _folder.Setup(a => a.ValidTasks).Returns(new List<IDev2Task> { task1.Object, task2.Object });
        }

        private void SetupSingleTaskWithId()
        {
            _mockService.Setup(a => a.GetFolder(_folderId)).Returns(_folder.Object);
            var task1 = new Mock<IDev2Task>();
            var task2 = new Mock<IDev2Task>();
            var action1 = new Mock<IExecAction>();
            var trigger1 = new Mock<ITrigger>();
            var definition = new Mock<IDev2TaskDefinition>();
            var settings = new Mock<ITaskSettings>();
            definition.Setup(a => a.Data).Returns("bob");
            definition.Setup(a => a.Settings).Returns(settings.Object);
            settings.Setup(a => a.Enabled).Returns(true);

            _convertorFactory.Setup(a => a.CreateExecAction(action1.Object)).Returns(action1.Object);
            task1.Setup(a => a.IsValidDev2Task()).Returns(true);
            task1.Setup(a => a.Definition).Returns(definition.Object);
            task2.Setup(a => a.IsValidDev2Task()).Returns(false);
            task1.Setup(a => a.Action).Returns(action1.Object);
            task1.Setup(a => a.Trigger).Returns(trigger1.Object);
            action1.Setup(a => a.Arguments).Returns("\"Workflow:a\" \"TaskName:b\" \"ResourceId:" + Guid.NewGuid() + "\"");
            _folder.Setup(a => a.ValidTasks).Returns(new List<IDev2Task> { task1.Object, task2.Object });
        }

        private List<IResourceHistory> RunOutput(DateTime starTime, DateTime endTime, string username = null)
        {
            var esbMethod = new GetScheduledResourceHistory();
            var security = new Mock<ISecurityWrapper>();
            esbMethod.SecurityWrapper = security.Object;
            var history = new List<IResourceHistory>
                {
                    new ResourceHistory("", new List<IDebugState>
                    {
                        new DebugState
                        {
                         StartTime = starTime
                         ,EndTime = endTime
                        }
                    },
                    new EventInfo(starTime, TimeSpan.MaxValue, endTime, ScheduleRunStatus.Error, "115"),
                        username)
                };
            return history;
        }
        // ReSharper restore InconsistentNaming

    }


    public class MockTaskEventLog : List<ITaskEvent>, ITaskEventLog
    {
        IEnumerator<ITaskEvent> IEnumerable<ITaskEvent>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public new long Count => base.Count;
    }

    public class MockTaskEvent : ITaskEvent
    {
        public MockTaskEvent(Guid? activityId, int eventId, string taskCategory, DateTime? timeCreated,
                             string correlation, string userId)
        {
            UserId = userId;
            Correlation = correlation;
            TimeCreated = timeCreated;
            TaskCategory = taskCategory;
            EventId = eventId;
            ActivityId = activityId;
        }

        public Guid? ActivityId { get; private set; }
        public int EventId { get; private set; }
        public string TaskCategory { get; private set; }
        public DateTime? TimeCreated { get; private set; }
        public string Correlation { get; private set; }
        public string UserId { get; private set; }
    }
}
