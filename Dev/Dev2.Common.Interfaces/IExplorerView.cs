using System.Collections.Generic;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Common.Interfaces
{
    public interface IExplorerView : IView
    {
        IEnvironmentViewModel OpenEnvironmentNode(string nodeName);
        List<IExplorerItemViewModel> GetFoldersVisible();
        IExplorerItemViewModel OpenFolderNode(string folderName);
        int GetVisibleChildrenCount(string folderName);
        void PerformFolderRename(string originalFolderName, string newFolderName);
        void PerformSearch(string searchTerm);

        void AddNewFolder(string folder, string server);

        void VerifyItemExists(string path);

        void DeletePath(string path);

        void AddNewFolderFromPath(string path);

        void AddNewResource(string path, string itemType);

        void AddResources(int resourceNumber, string path, string type);

        int GetResourcesVisible (string path);
    }
}
