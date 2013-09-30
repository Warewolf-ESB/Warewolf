using System;

namespace Dev2.Messages
{
    public class UpdateWorksurfaceFlowNodeDisplayName
    {
        public UpdateWorksurfaceFlowNodeDisplayName(Guid workflowDesignerResourceID, string oldName, string newName)
        {
            WorkflowDesignerResourceID = workflowDesignerResourceID;
            OldName = oldName;
            NewName = newName;
        }

        public string OldName { get; set; }
        public string NewName { get; set; }
        public Guid WorkflowDesignerResourceID { get; set; }
    }
}
