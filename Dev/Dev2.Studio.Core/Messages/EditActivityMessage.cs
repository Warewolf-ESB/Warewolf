using System;
using System.Activities.Presentation.Model;

namespace Dev2.Studio.Core.Messages
{
    public class EditActivityMessage : IMessage
    {
        public ModelItem ModelItem { get; set; }
        public Guid ParentEnvironmentID { get; set; }
        public EnvironmentRepository EnvironmentRepository { get; set; }

        public EditActivityMessage(ModelItem modelItem, Guid parentEnvironmentID, EnvironmentRepository environmentRepository)
        {
            ModelItem = modelItem;
            ParentEnvironmentID = parentEnvironmentID;
            EnvironmentRepository = environmentRepository ?? EnvironmentRepository.Instance;
        }
    }
}