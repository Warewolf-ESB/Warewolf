using System;
using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class DeleteResourcesMessage : IMessage
    {
        public DeleteResourcesMessage(ICollection<IContextualResourceModel> resourceModels, bool showDialog = true, Action actionToDoOnDelete = null)
        {
            ActionToDoOnDelete = actionToDoOnDelete;
            ShowDialog = showDialog;
            ResourceModels = resourceModels;
        }

        public ICollection<IContextualResourceModel> ResourceModels;

        public Action ActionToDoOnDelete { get; set; }
        public bool ShowDialog { get; set; }
    }
}
