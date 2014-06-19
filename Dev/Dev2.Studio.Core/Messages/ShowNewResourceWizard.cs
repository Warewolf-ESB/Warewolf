// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class ShowNewResourceWizard : IMessage
    {
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }

        public ShowNewResourceWizard(string resourceType)
        {
            ResourceType = resourceType;
        }

        public ShowNewResourceWizard(string resourceType, string resourcePath)
        {
            ResourceType = resourceType;
            ResourcePath = resourcePath;
        }
    }
}
