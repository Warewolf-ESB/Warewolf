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
        FrameworkElement FindTabByResourceModel(IResourceModel resource);
        bool TabExists(IResourceModel resource);
        void PersistTabs();
        List<IContextualResourceModel> GetOpenContextualResourceModels();
        void PersistTabs(ItemCollection tabcollection);
        ViewModelDialogResults GetServiceInputDataFromUser(IServiceDebugInfoModel input, out DebugTO debugTO);
        void StartDebuggingSession(DebugTO input, IEnvironmentModel environment);
        TabControl Manager { get; set; }
        FrameworkElement ActiveDocument { get; }
        object ActiveDocumentDataContext { get; }
        ObservableCollection<FrameworkElement> Tabs { get; set; }
        ContentControl PropertyPane { get; set; }
        ContentControl OutputPane { get; set; }
        ContentControl NavigationPane { get; set; }
        ContentControl DataMappingPane { get; set; }
        ContentControl DataListPane { get; set; }

        IList<IWorkspaceItem> WorkspaceItems { get; }
    }
}
