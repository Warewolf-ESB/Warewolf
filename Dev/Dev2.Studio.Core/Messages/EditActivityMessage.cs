using System;
using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class EditActivityMessage : IMessage
    {
        public ModelItem ModelItem { get; set; }
        public Guid ParentEnvironmentID { get; set; }
        public IEnvironmentRepository EnvironmentRepository { get; set; }

        public EditActivityMessage(ModelItem modelItem, Guid parentEnvironmentID, IEnvironmentRepository environmentRepository)
        {
            ModelItem = modelItem;
            ParentEnvironmentID = parentEnvironmentID;
            EnvironmentRepository = environmentRepository ?? Core.EnvironmentRepository.Instance;
        }
    }
}