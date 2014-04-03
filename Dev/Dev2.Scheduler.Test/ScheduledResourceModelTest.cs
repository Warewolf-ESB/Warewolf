using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Scheduler.Interfaces;
using Dev2.Services.Security;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;

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
            new Mock<IAuthorizationService>();
            _convertorFactory = new Mock<ITaskServiceConvertorFactory>();
            _folderId = "WareWolf";
            _agentPath = "AgentPath";
            _folder = new Mock<ITaskFolder>();
            _wrapper = new Mock<ISecurityWrapper>();
            _wrapper.Setup(a => a.IsWindowsAuthorised(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(true);
            _wrapper.Setup(a => a.IsWarewolfAuthorised(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(true);
            new Mock<ITaskCollection>();
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ScheduledResourceModel_Constructor")]
        public void TaskSheduler_ScheduledResourceModel_ShouldConstruct()
        {
            _mockService.Setup(a => a.GetFolder(_folderId)).Returns(_folder.Object);
            _folder.Setup(a => a.ValidTasks).Returns(new List<IDev2Task>());
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   @"c:\", _wrapper.Object);
            Assert.AreEqual(_convertorFactory.Object, model.ConvertorFactory);
            Assert.AreEqual(_folderId, model.WarewolfFolderPath);
            Assert.AreEqual(_agentPath, model.WarewolfAgentPath);
            Assert.AreEqual(_mockService.Object, model.TaskService);
            Assert.AreEqual(0, model.ScheduledResources.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ScheduledResourceModel_ScheduledResources")]
        public void TaskSheduler_ScheduledResourceModel_ShouldSelectedCorrectResources()
        {
            //setup
            SetupSingleTask();
            //create
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath,
                                                   _convertorFactory.Object, @"c:\", _wrapper.Object);

            Assert.AreEqual(1,
                            model.ScheduledResources.Count());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ScheduledResourceModel_ScheduledResourcesInvalid")]
        public void TaskSheduler_ScheduledResourceModel_ShouldErrorInvalidMessages()
        {
            //setup
            SetupSingleTask();
            //create
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath,
                                                   _convertorFactory.Object, @"c:\", _wrapper.Object);

            Assert.AreEqual(1,
                            model.ScheduledResources.Count());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ScheduledResourceModel_ScheduledResources")]
        public void TaskSheduler_ScheduledResourceModel_CorrectSelectedResources()
        {
            SetupSingleTask();

            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   @"c:\", _wrapper.Object);
            IScheduledResource a = model.ScheduledResources.First();
            Assert.AreEqual("bob", a.Name);
            Assert.AreEqual("a", a.WorkflowName);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ScheduledResourceModel_Should_DeleteValid")]
        public void TaskSheduler_ScheduledResourceModel_ShouldDeleteTest_Valid()
        {
            SetupSingleTask();

            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   @"c:\", _wrapper.Object);
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
        [TestCategory("TaskSheduler_ScheduledResourceModel_ShouldNotDeleteInvalid")]
        public void ScheduledResourceModel_DeleteTest_InValid()
        {
            //setup
            SetupSingleTask();
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   @"c:\", _wrapper.Object);
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
        [TestCategory("TaskSheduler_ScheduledResourceModel_ShouldSelectHistory")]
        public void ScheduledResourceModel_HistoryTestTaskEventsSelected()
        {
            // setup 
            var log = new MockTaskEventLog
                {
                    new MockTaskEvent(Guid.NewGuid(), 12, "Task Started", new DateTime(2000, 1, 1), "12345", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "2", new DateTime(2001, 1, 1), "12346", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "3", new DateTime(2002, 1, 1), "12347", "dave"),
                    new MockTaskEvent(Guid.NewGuid(), 12, "Task Completed", new DateTime(2003, 1, 1), "12348", "dave")
                };
            var dirHelper = new Mock<IDirectoryHelper>();
            var fileHelper = new Mock<IFileHelper>();
            var res = new Mock<IScheduledResource>();
            //setup expectancies
            dirHelper.Setup(a => a.GetFiles(@"c:\")).Returns(new[] { "b_12345_Bob" });
            fileHelper.Setup(a => a.ReadAllText("b_12345_Bob")).Returns("");
            res.Setup(a => a.Name).Returns("Bob");
            _convertorFactory.Setup(a => a.CreateTaskEventLog(It.IsAny<string>())).Returns(log);

            //construct
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   @"c:\", _wrapper.Object);

            IList<IResourceHistory> history = model.CreateHistory(res.Object);
            Assert.AreEqual(2, history.Count);
            Assert.AreEqual(new DateTime(2003, 1, 1), history.First().TaskHistoryOutput.StartDate);
            Assert.AreEqual(new DateTime(2000, 1, 1), history.Last().TaskHistoryOutput.EndDate);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ScheduledResourceModel_History")]
        public void ScheduledResourceModel_HistoryTestDebugCreated()
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
                          "[{\"$type\":\"Dev2.Diagnostics.DebugState, Dev2.Diagnostics\",\"ID\":\"cd902be2-a202-4d54-8c07-c5f56bae97fe\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"EnvironmentID\":\"00000000-0000-0000-0000-000000000000\",\"ClientID\":\"00000000-0000-0000-0000-000000000000\",\"StateType\":64,\"DisplayName\":\"dave\",\"HasError\":true,\"ErrorMessage\":\"Service [ dave ] not found.\",\"Version\":\"\",\"Name\":\"DynamicServicesInvoker\",\"ActivityType\":0,\"Duration\":\"00:00:00\",\"DurationString\":\"PT0S\",\"StartTime\":\"2014-03-20T17:23:14.0224329+02:00\",\"EndTime\":\"2014-03-20T17:23:14.0224329+02:00\",\"Inputs\":[],\"Outputs\":[],\"Server\":\"\",\"WorkspaceID\":\"00000000-0000-0000-0000-000000000000\",\"OriginalInstanceID\":\"00000000-0000-0000-0000-000000000000\",\"OriginatingResourceID\":\"00000000-0000-0000-0000-000000000000\",\"IsSimulation\":false,\"Message\":null,\"NumberOfSteps\":0,\"Origin\":\"\",\"ExecutionOrigin\":0,\"ExecutionOriginDescription\":null,\"ExecutingUser\":null,\"SessionID\":\"00000000-0000-0000-0000-000000000000\"}]");
            res.Setup(a => a.Name).Returns("Bob");
            _convertorFactory.Setup(a => a.CreateTaskEventLog(It.IsAny<string>())).Returns(log);

            //test
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   @"c:\", _wrapper.Object);
            model.DirectoryHelper = dirHelper.Object;
            model.FileHelper = fileHelper.Object;
            IList<IResourceHistory> history = model.CreateHistory(res.Object);

            Assert.AreEqual(2, history.Count);
            Assert.AreEqual(new DateTime(2003, 1, 1), history.First().TaskHistoryOutput.StartDate);
            Assert.AreEqual(new DateTime(2000, 1, 1), history.Last().TaskHistoryOutput.EndDate);
            Assert.AreEqual(history.Last().DebugOutput.Count, 1);
            Assert.AreEqual("Bob", history.Last().UserName);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ScheduledResourceModel_ScheduledResources")]
        public void ScheduledResourceModel_SaveTest()
        {
            //create objects
            SetupSingleTask();
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                  @"c:\", _wrapper.Object);
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
        [TestCategory("TaskSheduler_ScheduledResourceModel_Save")]
        public void ScheduledResourceModel_SaveInvalidWindowsUserPermissions()
        {
            //create objects
            SetupSingleTask();
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                  @"c:\", _wrapper.Object);
            var resourceToSave = new Mock<IScheduledResource>();

            //setup expectations
            _wrapper.Setup(
                a => a.IsWindowsAuthorised(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(false);
            //run test
            string errorMessage;
            model.Save(resourceToSave.Object, out errorMessage);
            Assert.AreEqual(@"This task requires that the user account specified has 'Log On As Batch' job rights. 
Please contact your Windows System Administrator.", errorMessage);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ScheduledResourceModel_Save")]
        public void ScheduledResourceModel_SaveInvalidWarewolfUserPermissions()
        {
            //create objects
            SetupSingleTask();
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                  @"c:\", _wrapper.Object);
            var resourceToSave = new Mock<IScheduledResource>();

            //setup expectations
            _wrapper.Setup(
                a => a.IsWarewolfAuthorised(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(false);
            resourceToSave.Setup(a => a.WorkflowName).Returns("bob");
            //run test
            string errorMessage;
            model.Save(resourceToSave.Object, out errorMessage);
            Assert.AreEqual(@"This Workflow requires that you have Execute permission on the 'bob' Workflow. 
Please contact your Warewolf System Administrator.", errorMessage);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ScheduledResourceModel_ScheduledResources")]
        public void ScheduledResourceModel_SaveTest_UserPassword()
        {
            SetupSingleTask();
            var model = new ScheduledResourceModel(_mockService.Object, _folderId, _agentPath, _convertorFactory.Object,
                                                   @"c:\", _wrapper.Object);

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
    }


    public class MockTaskEventLog : List<ITaskEvent>, ITaskEventLog
    {
        IEnumerator<ITaskEvent> IEnumerable<ITaskEvent>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public new long Count
        {
            get { return Count; }
        }
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
