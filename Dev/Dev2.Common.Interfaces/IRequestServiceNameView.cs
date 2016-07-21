using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Common.Interfaces
{
    public interface IRequestServiceNameView : IView
    {
        void ShowView();

        void RequestClose();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        bool HasServer(string serverName);

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void CreateNewFolder(string newFolderName, string rootPath);

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        IExplorerView GetExplorerView();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void OpenFolder(string folderName);

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void EnterName(string serviceName);

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        bool IsSaveButtonEnabled();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        string GetValidationMessage();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        List<IExplorerTreeItem> GetFoldersVisible();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void Filter(string filter);

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void Cancel();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void PerformActionOnContextMenu(string menuAction, string itemName, string path);

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        IExplorerTreeItem GetCurrentItem();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void CreateNewFolderInFolder(string newFolderName, string currentFolder);

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void Save();
    }
}