using Dev2.Common.Common;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using Dev2.Studio.Core.Factories;

namespace Dev2.Studio
{
    public static class ResourceExtensionHelper
    {
        public static IContextualResourceModel HandleResourceNotInResourceFolder(string filePath, string fileName, Common.Interfaces.Studio.Controller.IPopupController popupController)
        {
            var saveResource = popupController.ShowResourcesNotInCorrectPath();
            if (saveResource == MessageBoxResult.OK)
            {
                using (var stream = File.OpenRead(filePath))
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        var resourceContent = streamReader.ReadToEnd();
                        var serverRepo = CustomContainer.Get<IServerRepository>();
                        var serviceXml = XDocument.Parse(resourceContent);
                        var resource = new Resource(resourceContent.ToStringBuilder().ToXElement());
                        var resourceModel = ResourceModelFactory.CreateResourceModel(serverRepo.ActiveServer, resource, serviceXml, out bool isWorkflow);
                        if (!isWorkflow)
                        {
                            popupController.ShowCanNotMoveResource();
                        }
                        return resourceModel;
                    }
                }
            }
            else
            {
                return null;
            }
        }
    }
}