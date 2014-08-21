using Dev2.Collections;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IResourceModel : IWorkSurfaceObject
    {
        Guid ID { get; set; }
        Permissions UserPermissions { get; set; }
        bool IsAuthorized(AuthorizationContext authorizationContext);

        string Inputs { get; set; }
        string Outputs { get; set; }
        bool AllowCategoryEditing { get; set; }
        string Category { get; set; }
        string Comment { get; set; }
        string DataTags { get; set; }
        string DisplayName { get; set; }
        string Error { get; }
        bool HasErrors { get; }
        string HelpLink { get; set; }
        string IconPath { get; set; }
        bool IsDebugMode { get; set; }
        bool IsVersionResource { get; set; }
        bool RequiresSignOff { get; set; }
        string ResourceName { get; set; }
        ResourceType ResourceType { get; set; }
        StringBuilder WorkflowXaml { get; set; }
        List<string> TagList { get; }
        string Tags { get; set; }
        string this[string columnName] { get; }
        StringBuilder ToServiceDefinition();    
        string UnitTestTargetWorkflowService { get; set; }
        string DataList { get; set; }
        bool IsDatabaseService { get; set; }
        bool IsPluginService { get; set; }
        bool IsResourceService { get; set; }
        bool IsWorkflowSaved { get; set; }
        Version Version { get; set; }
        string ServerResourceType { get; set; }
        IVersionInfo VersionInfo { get; set; }
        void Update(IResourceModel resourceModel);
        string ConnectionString { get; set; }
        bool IsValid { get; set; }
        IObservableReadOnlyList<IErrorInfo> Errors { get; }
        IObservableReadOnlyList<IErrorInfo> FixedErrors { get; }

        IList<IErrorInfo> GetErrors(Guid instanceId);
        void AddError(IErrorInfo error);
        void RemoveError(IErrorInfo error);
        void Commit();
        void Rollback();
    }
}
