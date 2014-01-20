using Dev2.Studio.Core.Models;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class AddWorkSurfaceMessage : IMessage
    {
        public IWorkSurfaceObject WorkSurfaceObject { get; set; }

        public bool ShowDebugWindowOnLoad { get; set; }

        public AddWorkSurfaceMessage()
        {
        }

        public AddWorkSurfaceMessage(IWorkSurfaceObject resource)
        {
            WorkSurfaceObject = resource;
        }
    }
}