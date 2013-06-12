using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;

namespace Dev2.Studio.Core.Messages
{
    public class AddWorkSurfaceMessage : IMessage
    {
        public IWorkSurfaceObject WorkSurfaceObject { get; set; }

        public AddWorkSurfaceMessage(IWorkSurfaceObject resource)
        {
            WorkSurfaceObject = resource;
        }
    }
}