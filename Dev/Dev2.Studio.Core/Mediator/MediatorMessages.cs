
namespace Dev2.Studio.Core
{
    public enum MediatorMessages
    {
        //
        // UI Messages
        //
        AddHelpDocument,
        AddStuidoShortcutKeysPage,

        ShowExplorer,
        ShowStartPage,
        ShowStartTabs,

        SetActivePage,

        TabContextChanged,
        DisplayAboutDialogue,
        UpdateWebpagePreview,

        ConfigureSwitchExpression,
        ConfigureCaseExpression,
        EditCaseExpression, // 28.01.2013 - Travis.Frisigner - Added for case edits ;)
        ConfigureDecisionExpression,

        //7-12-2012 - Massimo.Guerrera - Added for PBI 6665
        ShowActivityWizard,
        EditActivity,
        ShowActivitySettingsWizard,
        ShowHelpTab,
        DoesActivityHaveWizard,
        HasWizard,
        GetMappingViewModel,

        CloseWizard,

        SelectItemInDeploy,

        //ShowDataInOutputWindow,
        //AppendDataToOutputWindow,

        //
        // Intellisense Messages
        //
        UpdateIntelisense,

        //
        // Mapping Messages
        //
        RemoveDataMapping,
        UpdateDataMapping,

        //
        // Datalist Messages
        //
        UpdateDataList,
        DataListItemSelected,
        FindMissingDataListItems,
        AddMissingDataListItems,
        RemoveUnusedDataListItems,
        AddRemoveDataListItems,

        //
        // Resource Messages
        //
        BuildResource,
        SaveResource,
        DebugResource, //Use this message to start debugging a resource
        DeployResources,
        ExecuteResource,

        DeleteServiceExplorerResource,
        DeleteSourceExplorerResource,
        DeleteWorkflowExplorerResource,

        AddWebsiteDesigner,
        AddWorkflowDesigner,
        AddWebpageDesigner,
        AddResourceDocument,
        WorkflowActivitySelected,

        ShowDependencyGraph,
        ShowNewResourceWizard,
        ShowWebpartWizard,
        ShowEditResourceWizard,

        SaveResourceModel,
        BindViewToViewModel,

        //
        // Environment Messages
        //
        SetActiveEnvironment,
        UpdateExplorer,
        UpdateDeploy,

        //
        // Workspace Messages
        //
        SaveWorkspaceItems,

        //
        // IDebugWriter
        //
        DebugWriterAppend,
        DebugWriterWrite,

        RemoveServerFromExplorer,
        AddServerToExplorer

    }
}
