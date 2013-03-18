using Dev2.Studio.Core.AppResources.Enums;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Dev2.Studio.Core.Interfaces 
{
    public interface IResourceModel
    {
        Guid ID { get; set; }
        bool AllowCategoryEditing { get; set; }
        string AuthorRoles { get; set; }
        string Category { get; set; }
        string Comment { get; set; }
        string DataTags { get; set; }
        string DisplayName { get; set; }
        string Error { get; }
        bool HasErrors { get; }
        string HelpLink { get; set; }
        string IconPath { get; set; }
        bool IsDebugMode { get; set; }
        bool RequiresSignOff { get; set; }
        string ResourceName { get; set; }
        ResourceType ResourceType { get; set; }
        string ServiceDefinition { get; set; }
        string WorkflowXaml { get; set; }
        List<string> TagList { get; }
        string Tags { get; set; }
        string this[string columnName] { get; }
        string ToServiceDefinition();
        string UnitTestTargetWorkflowService { get; set; }
        string DataList { get; set; }
        Activity WorkflowActivity { get; }
        bool IsDatabaseService { get; set; }
        bool IsResourceService { get; set; }
        bool IsWorkflowSaved(string viewModelServiceDef);
        Version Version { get; set; }
        void Update(IResourceModel resourceModel);
    }
}
