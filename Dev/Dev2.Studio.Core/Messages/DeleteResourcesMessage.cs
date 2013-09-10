using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class DeleteResourcesMessage : IMessage
    {
        public DeleteResourcesMessage(ICollection<IContextualResourceModel> resourceModels, bool showDialog = true)
        {
            ShowDialog = showDialog;
            ResourceModels = resourceModels;
        }

        public ICollection<IContextualResourceModel> ResourceModels;

        public bool ShowDialog { get; set; }
    }
}
