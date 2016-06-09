using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Common.Interfaces
{
    public interface IRequestServiceNameView : IView
    {
        void ShowView();
        void RequestClose();
        bool HasServer(string serverName);
        void CreateNewFolder(string newFolderName, string rootPath);
        IExplorerView GetExplorerView();
        void OpenFolder(string folderName);
        void EnterName(string serviceName);
        bool IsSaveButtonEnabled();
        string GetValidationMessage();
        List<IExplorerTreeItem> GetFoldersVisible();

        void Filter(string filter);

        void Cancel();

        void PerformActionOnContextMenu(string menuAction, string itemName, string path);

        IExplorerTreeItem GetCurrentItem();

        void CreateNewFolderInFolder(string newFolderName, string currentFolder);

        void Save();
    }
}