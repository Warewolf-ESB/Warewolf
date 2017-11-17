using Dev2.Common.Common;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using Dev2.Studio.Core.Factories;
using Dev2.Common;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.ViewModels;
using System;

namespace Dev2.Studio
{
    public static class ResourceExtensionHelper
    {
        public static IContextualResourceModel HandleResourceNotInResourceFolder(string filePath, string fileName, Common.Interfaces.Studio.Controller.IPopupController popupController, ShellViewModel shellViewModel)
        {
            IContextualResourceModel resourceModel = null;
            IServerRepository serverRepo = null;
            Resource resource = null;
            var saveResource = popupController.ShowResourcesNotInCorrectPath();
            if (saveResource == MessageBoxResult.OK)
            {
                using (var stream = File.OpenRead(filePath))
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        var resourceContent = streamReader.ReadToEnd();
                        serverRepo = CustomContainer.Get<IServerRepository>();
                        var serviceXml = XDocument.Parse(resourceContent);
                        resource = new Resource(resourceContent.ToStringBuilder().ToXElement());
                        resourceModel = ResourceModelFactory.CreateResourceModel(serverRepo.ActiveServer, resource, serviceXml);
                    }
                }
                if (resourceModel == null && resource.ResourceType.Contains("Source"))
                {
                    var moveSource = popupController.ShowCanNotMoveResource() == MessageBoxResult.OK;
                    if (moveSource)
                    {
                        File.Move(filePath, Path.Combine(EnvironmentVariables.ResourcePath, Path.GetFileName(filePath)));
                        shellViewModel?.ExplorerViewModel?.RefreshEnvironment(serverRepo.ActiveServer.EnvironmentID);
                        serverRepo.ActiveServer.ExplorerRepository.LoadExplorer();
                        serverRepo.ReloadServers();
                        resourceModel = serverRepo.ActiveServer.ResourceRepository.LoadContextualResourceModel(resource.ResourceID);
                    }
                }
                return resourceModel;
            }
            else
            {
                return resourceModel;
            }
        }
    }
}