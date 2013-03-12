
namespace Dev2.Studio.Core
{
    public enum MediatorMessages
    {
        //
        // UI Messages
        //
        AddHelpDocument,

        SetActivePage,
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

        //
        // Intellisense Messages
        //
        UpdateIntelisense,

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
        SaveResource,
        DebugResource, //Use this message to start debugging a resource
        DeployResources,
        ExecuteResource,

        AddWebsiteDesigner,
        AddWorkflowDesigner,
        AddWebpageDesigner,

        ShowWebpartWizard,
        ShowEditResourceWizard,

        SaveResourceModel,

        //
        // Environment Messages
        //
        SetActiveEnvironment,
        UpdateExplorer,
        UpdateDeploy,

        //
        // IDebugWriter
        //
        DebugWriterAppend,
        DebugWriterWrite,

        RemoveServerFromExplorer,
        AddServerToExplorer

    }
}
