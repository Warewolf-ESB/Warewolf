using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;

namespace Dev2.Tests.Logging
{
    public class DummyDebugProvider : IDebugProvider
    {
        private Guid _serverID = Guid.NewGuid();
        private Guid _workflow1ID = Guid.Parse("D72A6E61-EC79-43D4-99EC-9E26DB9A0A4B");
        private Guid _workflow2ID = Guid.NewGuid();
        private Guid _assign1ID = Guid.NewGuid();
        private Guid _assign2ID = Guid.NewGuid();
        private Guid _assign3ID = Guid.NewGuid();
        private DateTime _startDate = DateTime.Now;

        public Guid ServerID
        {
            get { return _serverID; }
            set { _serverID = value; }
        }

        public Guid Workflow1ID
        {
            get { return _workflow1ID; }
            set { _workflow1ID = value; }
        }

        public Guid Workflow2ID
        {
            get { return _workflow2ID; }
            set { _workflow2ID = value; }
        }

        public Guid Assign1ID
        {
            get { return _assign1ID; }
            set { _assign1ID = value; }
        }

        public Guid Assign2ID
        {
            get { return _assign2ID; }
            set { _assign2ID = value; }
        }

        public Guid Assign3ID
        {
            get { return _assign3ID; }
            set { _assign3ID = value; }
        }

        public IEnumerable<IDebugState> GetDebugStates(string serverWebUri, DirectoryPath directory, FilePath path)
        {
            var list = new List<DebugState>
            {
                new DebugState
                {
                    StateType = StateType.Before,
                    ServerID = ServerID,
                    ParentID = Guid.Empty,
                    ID = Workflow1ID,
                    DisplayName = "Workflow1",
                    HasError = false,
                    Name = "DsfActivity",
                    ActivityType = ActivityType.Workflow,
                    StartTime = _startDate,
                    EndTime = _startDate.AddMinutes(1)
                },
                new DebugState
                {
                    StateType = StateType.All,
                    ServerID = ServerID,
                    ParentID = Workflow1ID,
                    ID = Assign1ID,
                    DisplayName = "Assign1",
                    HasError = false,
                    Name = "Assign",
                    ActivityType = ActivityType.Step,
                    StartTime = _startDate.AddMinutes(1),
                    EndTime = _startDate.AddMinutes(2)
                },
                new DebugState
                {
                    StateType = StateType.Before,
                    ServerID = ServerID,
                    ParentID = Workflow1ID,
                    ID = Workflow2ID,
                    DisplayName = "Workflow2",
                    HasError = false,
                    Name = "DsfActivity",
                    ActivityType = ActivityType.Step,
                    StartTime = _startDate.AddMinutes(2),
                    EndTime = _startDate.AddMinutes(3)
                },
                new DebugState
                {
                    StateType = StateType.All,
                    ServerID = ServerID,
                    ParentID = Workflow2ID,
                    ID = Assign2ID,
                    DisplayName = "Assign2",
                    HasError = false,
                    Name = "Assign",
                    ActivityType = ActivityType.Step,
                    StartTime = _startDate.AddMinutes(3),
                    EndTime = _startDate.AddMinutes(4)
                },
                new DebugState
                {
                    StateType = StateType.After,
                    ServerID = ServerID,
                    ParentID = Workflow1ID,
                    ID = Workflow2ID,
                    DisplayName = "Workflow2",
                    HasError = false,
                    Name = "DsfActivity",
                    ActivityType = ActivityType.Step,
                    StartTime = _startDate.AddMinutes(4),
                    EndTime = _startDate.AddMinutes(5)
                },
                new DebugState
                {
                    StateType = StateType.After,
                    ServerID = ServerID,
                    ParentID = Guid.Empty,
                    ID = Workflow1ID,
                    DisplayName = "Workflow1",
                    HasError = false,
                    Name = "DsfActivity",
                    ActivityType = ActivityType.Workflow,
                    StartTime = _startDate.AddMinutes(5),
                    EndTime = _startDate.AddMinutes(6)
                },
                new DebugState
                {
                    StateType = StateType.All,
                    ServerID = ServerID,
                    ParentID = Guid.Empty,
                    ID = Assign3ID,
                    DisplayName = "Assign3",
                    HasError = false,
                    Name = "Assign",
                    ActivityType = ActivityType.Step,
                    StartTime = _startDate.AddMinutes(6),
                    EndTime = _startDate.AddMinutes(7)
                }
            };
            return list;
        }

        public DebugState GetDebugState()
        {
            return new DebugState
                {
                    StateType = StateType.Before,
                    ServerID = ServerID,
                    ParentID = Guid.Empty,
                    ID = Workflow1ID,
                    OriginatingResourceID = Workflow1ID,
                    DisplayName = "TestWorkflow",
                    HasError = false,
                    Name = "DsfActivity",
                    ActivityType = ActivityType.Workflow,
                    StartTime = _startDate,
                    EndTime = _startDate.AddMinutes(1)
                };
        }

        #region Implementation of IDebugProvider

        public IEnumerable<IDebugState> GetDebugStates(string serverWebUri, IDirectoryPath directory, IFilePath path)
        {
            yield break;
        }

        #endregion
    }

}
