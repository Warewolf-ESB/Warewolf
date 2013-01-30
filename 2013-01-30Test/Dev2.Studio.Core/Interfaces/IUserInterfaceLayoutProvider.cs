using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Dev2.Session;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Workspaces;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IUserInterfaceLayoutProvider
    {
        bool RemoveDocument(object document);
        void SetActiveTab(IResourceModel resourceModel);
        object FindTabByResourceModel(IResourceModel resource);
        bool TabExists(IResourceModel resource);
        void PersistTabs();
        IContextualResourceModel GetContextualResourceModel(object dataContext);
        List<IContextualResourceModel> GetOpenContextualResourceModels();
        void PersistTabs(ItemCollection tabcollection);
        ViewModelDialogResults GetServiceInputDataFromUser(IServiceDebugInfoModel input, out DebugTO debugTO);
        void StartDebuggingSession(DebugTO input, IEnvironmentModel environment);
        object Manager { get; set; }
        object ActiveDocument { get; }
        object ActiveDocumentDataContext { get; }
        ObservableCollection<FrameworkElement> Tabs { get; set; }
        object PropertyPane { get; set; }
        object OutputPane { get; set; }
        object NavigationPane { get; set; }
        object DataMappingPane { get; set; }
        object DataListPane { get; set; }

        IList<IWorkspaceItem> WorkspaceItems { get; }
    }
}
