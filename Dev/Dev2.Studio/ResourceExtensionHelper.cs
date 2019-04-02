#pragma warning disable
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
        public static async Task<IContextualResourceModel> HandleResourceNotInResourceFolderAsync(string filePath, Common.Interfaces.Studio.Controller.IPopupController popupController, IShellViewModel shellViewModel, IFile file, IFilePath path, IServerRepository serverRepository)
        {
            var saveResource = popupController.ShowResourcesNotInCorrectPath();
            if (saveResource == MessageBoxResult.OK)
            {
                ReadFileContent(filePath, shellViewModel, file, out IContextualResourceModel resourceModel, serverRepository, out IResource resource);
                if (resourceModel == null && (resource.ResourceType != "WorkflowService" || resource.ResourceType != "Workflow"))
                {
                    var moveSource = popupController.ShowCanNotMoveResource() == MessageBoxResult.OK;
                    if (moveSource)
                    {
                        var destination = path.Combine(EnvironmentVariables.ResourcePath, path.GetFileName(filePath));
                        file.Move(filePath, destination);
                        await shellViewModel.ExplorerViewModel.RefreshEnvironment(serverRepository.ActiveServer.EnvironmentID);
                        resourceModel = serverRepository.ActiveServer.ResourceRepository.LoadContextualResourceModel(resource.ResourceID);
                    }
                }
                var ctResourceModel = resourceModel;
                if (resourceModel != null)
                {
                    shellViewModel.OpenResource(resourceModel.ID, shellViewModel.ActiveServer.EnvironmentID, shellViewModel.ActiveServer, resourceModel);
                }
                return ctResourceModel;
            }
            return null;
        }
        public static async Task<IContextualResourceModel> HandleResourceInResourceFolderAndOtherDir(string filePath, Common.Interfaces.Studio.Controller.IPopupController popupController, IShellViewModel shellViewModel, IFile file, IFilePath path, IServerRepository serverRepository)
        {
            ReadFileContent(filePath, shellViewModel, file, out IContextualResourceModel resourceModel, serverRepository, out IResource resource);
            if (resourceModel == null && (resource.ResourceType != "WorkflowService" || resource.ResourceType != "Workflow"))
            {
                popupController.ShowSourceAlreadyExistOpenFromResources();
                return resourceModel;
            }
            if (resourceModel != null)
            {
                resourceModel.IsNewWorkflow = true;
                resourceModel.IsNotWarewolfPath = true;
                shellViewModel.OpenResource(resourceModel.ID, shellViewModel.ActiveServer.EnvironmentID, shellViewModel.ActiveServer, resourceModel);
            }
            return resourceModel;
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