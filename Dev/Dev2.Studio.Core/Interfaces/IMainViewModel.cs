using System.Collections.Generic;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.ViewModels.Base;
using System;
using System.Windows.Input;
using Dev2.Workspaces;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IMainViewModel
    {
        event ResourceEventHandler OnApplicationExitRequest;

        ILayoutObjectViewModel ActiveCell { get; }
        ILayoutGridViewModel ActivePage { get; }
        string Title { get; }
        string OutputMessage { get; set; }
        bool CanSave { get; }
        bool CanDebug { get; }

        IFrameworkSecurityContext SecurityContext { get; }
        IWebCommunication WebCommunication { get; set; }
        IList<IWorkspaceItem> WorkspaceItems { get; }

        ICommand SaveCommand { get; }
        ICommand DebugCommand { get; }
        ICommand DeployCommand { get; }
        ICommand ExitCommand { get; }
        ICommand EditCommand { get; }
        ICommand ViewInBrowserCommand { get; }
        ICommand OpenWebsiteCommand { get; }
        ICommand RunCommand { get; }
        RelayCommand<string> NewResourceCommand { get; }

        void AddDependencyVisualizerDocument(IContextualResourceModel resource);
        void AddHelpDocument(IResourceModel resource);
        void AddStartTabs();
        void AddWorkflowDocument(IResourceModel resource);
        void Debug(IContextualResourceModel model);
        void DisplayOutput(string message, bool popupWindow = true);
        void Run(IContextualResourceModel resourceModel);
        void SetActivePage(ILayoutObjectViewModel cell);
        void ViewInBrowser();
        void Save(IContextualResourceModel resource, bool showWindow = true);
        void Build(IContextualResourceModel resource, bool showWindow = true, bool deploy = true);
        void AddNewResource(string resourceType);

    }
}
