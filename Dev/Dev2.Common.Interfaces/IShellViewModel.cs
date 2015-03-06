using System;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.ServerDialogue;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels;

namespace Dev2.Common.Interfaces
{
    public interface IShellViewModel
    {
        void AddService(Guid resourceId,IServer server);
        void DeployService(IExplorerItemViewModel resourceToDeploy);
        void UpdateHelpDescriptor(IHelpDescriptor helpDescriptor);
        void NewResource(ResourceType? type, Guid selectedId);
        void SaveService();
        void ExecuteService();
        void OpenScheduler();
        void OpenSettings();
        IServer ActiveServer { get; set; }
        object ActiveItem { get; set; }
        IServer LocalhostServer { get; set; }
        bool MenuExpanded { get; set; }
        double MenuPanelWidth { get; set; }

        void Handle(Exception err);
        bool ShowPopup(IPopupMessage getDeleteConfirmation);
        void RemoveServiceFromExplorer(IExplorerItemViewModel explorerItemViewModel);
        event Action ActiveServerChanged;
        event Action ActiveItemChanged;

        Task<bool> CheckForNewVersion();

        void DisplayDialogForNewVersion();

        void OpenVersion(Guid resourceId, string versionNumber);

        void ExecuteOnDispatcher(Action action);

        void ServerSourceAdded(IServerSource source);

        void EditResource(IDbSource selectedSource);
    }
}