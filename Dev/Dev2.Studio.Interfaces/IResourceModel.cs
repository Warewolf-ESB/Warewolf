#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Core.Collections;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Security;
using Dev2.Studio.Interfaces.Enums;
using Warewolf.Data;

namespace Dev2.Studio.Interfaces
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
        bool IsDebugMode { get; set; }
        bool IsVersionResource { get; set; }
        string ResourceName { get; set; }
        ResourceType ResourceType { get; set; }
        StringBuilder WorkflowXaml { get; set; }
        List<string> TagList { get; }
        string Tags { get; set; }
        string this[string columnName] { get; }
        StringBuilder ToServiceDefinition();
        StringBuilder ToServiceDefinition(bool prepairForDeployment);
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

        string GetSavePath();
        Guid OriginalId { get; set; }
    }
}
