using Dev2.Common.Common;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using Dev2.Studio.Core.Factories;
using Dev2.Common;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Studio
{   
    public static class ResourceExtensionHelper
    {
        public static async Task<IContextualResourceModel> HandleResourceNotInResourceFolderAsync(string filePath, string fileName, Common.Interfaces.Studio.Controller.IPopupController popupController, IShellViewModel shellViewModel, IFile file, IFilePath path, IServerRepository serverRepository)
        {
            IContextualResourceModel resourceModel = null;
            var serverRepo = serverRepository;
            IResource resource = null;
            if (filePath.Contains(EnvironmentVariables.VersionsPath))
            {
                return resourceModel;
            }
            var saveResource = popupController.ShowResourcesNotInCorrectPath();
            if (saveResource == MessageBoxResult.OK)
            {
                ReadFileContent(filePath, shellViewModel, file, out resourceModel, serverRepo, out resource);
                if (resourceModel == null && (resource.ResourceType != "WorkflowService" || resource.ResourceType != "Workflow"))
                {
                    var moveSource = popupController.ShowCanNotMoveResource() == MessageBoxResult.OK;
                    if (moveSource)
                    {
                        var destination = path.Combine(EnvironmentVariables.ResourcePath, path.GetFileName(filePath));
                        file.Move(filePath, destination);
                        await shellViewModel.ExplorerViewModel.RefreshEnvironment(serverRepo.ActiveServer.EnvironmentID);
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

        static void ReadFileContent(string filePath, IShellViewModel shellViewModel, IFile file, out IContextualResourceModel resourceModel, IServerRepository serverRepo, out IResource resource)
        {
            using (var stream = file.OpenRead(filePath))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    var resourceContent = streamReader.ReadToEnd();
                    var serviceXml = XDocument.Parse(resourceContent);
                    resource = shellViewModel.CreateResourceFromStreamContent(resourceContent);
                    resourceModel = ResourceModelFactory.CreateResourceModel(serverRepo.ActiveServer, resource, serviceXml);
                }
            }
        }
    }
}