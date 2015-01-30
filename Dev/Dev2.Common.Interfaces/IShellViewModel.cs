using System;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.ServerDialogue;
using Dev2.Common.Interfaces.Studio.ViewModels;

namespace Dev2.Common.Interfaces
{
    public interface IShellViewModel
    {
        void AddService(IResource resource);
        void DeployService(IExplorerItemViewModel resourceToDeploy);
        void UpdateHelpDescriptor(IHelpDescriptor helpDescriptor);
        void NewResource(ResourceType? type);
        void SaveService();
        void ExecuteService();
        void OpenScheduler();
        void OpenSettings();
        IServer ActiveServer { get; set; }
        IExplorerTreeItem ActiveItem { get; set; }
        IServer LocalhostServer { get; set; }
        void Handle(Exception err);
        bool ShowPopup(IPopupMessage getDeleteConfirmation);
        void RemoveServiceFromExplorer(IExplorerItemViewModel explorerItemViewModel);
        event Action ActiveServerChanged;
        event Action ActiveItemChanged;

        Task<bool> CheckForNewVersion();

        void DisplayDialogForNewVersion();

        void OpenVersion(Guid ResourceId, string VersionNumber);

        void ExecuteOnDispatcher(Action action);

        void ServerSourceAdded(IServerSource source);
    }
}