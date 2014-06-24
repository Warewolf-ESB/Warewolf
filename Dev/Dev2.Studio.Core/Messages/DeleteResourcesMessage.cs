using System;
using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class DeleteResourcesMessage : IMessage
    {
        public DeleteResourcesMessage(ICollection<IContextualResourceModel> resourceModels, string folderName, bool showDialog = true, Action actionToDoOnDelete = null)
        {
            FolderName = folderName;
            ActionToDoOnDelete = actionToDoOnDelete;
            ShowDialog = showDialog;
            ResourceModels = resourceModels;
        }

        public ICollection<IContextualResourceModel> ResourceModels;

        public string FolderName { get; set; }
        public Action ActionToDoOnDelete { get; set; }
        public bool ShowDialog { get; set; }
    }
}
