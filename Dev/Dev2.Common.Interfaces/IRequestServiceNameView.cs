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
    }
}