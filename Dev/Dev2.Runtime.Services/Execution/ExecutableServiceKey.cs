using System;

namespace Dev2.Runtime.ESB.WF
{
    public class ExecutableServiceKey : IEquatable<ExecutableServiceKey>
    {
        public ExecutableServiceKey(Guid workspaceID, Guid resourceID)
        {
            WorkspaceID = workspaceID;
            ResourceID = resourceID;
        }

        public Guid WorkspaceID { get; private set; }
        public Guid ResourceID { get; private set; }

        public bool Equals(ExecutableServiceKey other)
        {
            return WorkspaceID.Equals(other.WorkspaceID) && ResourceID.Equals(other.ResourceID);
        }
    }
}
