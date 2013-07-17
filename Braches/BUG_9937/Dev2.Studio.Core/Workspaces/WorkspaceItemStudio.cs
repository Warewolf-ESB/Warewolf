using Dev2.Studio.Core.Interfaces;

namespace Dev2.Workspaces
{
    public partial class WorkspaceItem
    {
        public bool Equals(IContextualResourceModel other)
        {
            if (other == null || other.Environment == null)
            {
                return false;
            }
            return ID == other.ID && EnvironmentID == other.Environment.ID;
        }
    }
}
