/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Data.Settings;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.AppResources.Enums;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Studio.Core.Interfaces
{
    public interface IResourceRepository : IDisposable
    {
        List<IResourceModel> ReloadResource(Guid resourceId, ResourceType resourceType, IEqualityComparer<IResourceModel> equalityComparer, bool fetchXaml);
        void UpdateWorkspace();
        void DeployResource(IResourceModel resource);
        ExecuteMessage DeleteResource(IResourceModel resource);
        void Add(IResourceModel resource);
        void ForceLoad();
        Task<bool> ForceLoadAsync();
        bool IsLoaded { get; }
        bool DoesResourceExistInRepo(IResourceModel resource);
        ExecuteMessage SaveToServer(IResourceModel instanceObj);
        void DeployResources(IEnvironmentModel targetEnviroment, IEnvironmentModel sourceEnviroment, IDeployDto dto);
        ExecuteMessage FetchResourceDefinition(IEnvironmentModel targetEnv, Guid workspaceId, Guid resourceModelId, bool prepaireForDeployment);
        List<T> FindSourcesByType<T>(IEnvironmentModel targetEnvironment, enSourceType sourceType);
        List<IResourceModel> FindResourcesByID(IEnvironmentModel targetEnvironment, IEnumerable<string> guids, ResourceType resourceType);
        IList<T> GetResourceList<T>(IEnvironmentModel targetEnvironment) where T : new();
        Settings ReadSettings(IEnvironmentModel currentEnv);
        ExecuteMessage WriteSettings(IEnvironmentModel currentEnv, Settings settings);
        DbTableList GetDatabaseTables(DbSource dbSource);
        List<SharepointListTo> GetSharepointLists(SharepointSource source);
        DbColumnList GetDatabaseTableColumns(DbSource dbSource, DbTable dbTable);
        List<ISharepointFieldTo> GetSharepointListFields(ISharepointSource source, SharepointListTo list, bool onlyEditable);
        ExecuteMessage GetDependenciesXml(IContextualResourceModel resourceModel, bool getDependsOnMe);
        bool HasDependencies(IContextualResourceModel resourceModel);
        ExecuteMessage StopExecution(IContextualResourceModel resourceModel);
        ICollection<IResourceModel> All();
        ICollection<IResourceModel> Find(Expression<Func<IResourceModel, bool>> expression);
        IResourceModel FindSingle(Expression<Func<IResourceModel, bool>> expression, bool fetchDefinition = false, bool prepairForDeployment = false);
        ExecuteMessage Save(IResourceModel instanceObj);
        void Load();
        ExecuteMessage DeleteResourceFromWorkspace(IResourceModel resource);
        void LoadResourceFromWorkspace(Guid resourceId, Guid? workspaceId);
        IContextualResourceModel LoadContextualResourceModel(Guid resourceId);
        Task<ExecuteMessage> GetDependenciesXmlAsync(IContextualResourceModel resourceModel, bool getDependsOnMe);
        Task<IContextualResourceModel> LoadContextualResourceModelAsync(Guid resourceId);
    }
}
