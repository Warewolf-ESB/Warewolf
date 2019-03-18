#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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