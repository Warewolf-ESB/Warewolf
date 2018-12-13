/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Data.ServiceModel;
using Dev2.Data.Settings;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Common;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Interfaces.Core;

namespace Dev2.Studio.Interfaces
{
    public interface IResourceRepository : IDisposable
    {
        void UpdateWorkspace();
        void DeployResource(IResourceModel resource, string savePath);
        ExecuteMessage DeleteResource(IResourceModel resource);
        ExecuteMessage ResumeWorkflowExecution(IResourceModel resource,string environment,Guid startActivityId, string versionNumber);
        void Add(IResourceModel resource);
        void UpdateServer(IServer server);
        bool IsLoaded { get; }
        bool DoesResourceExistInRepo(IResourceModel resource);
        ExecuteMessage SaveToServer(IResourceModel instanceObj);
        ExecuteMessage SaveToServer(IResourceModel instanceObj, string reason);
        void DeployResources(IServer targetEnviroment, IServer sourceEnviroment, IDeployDto dto);
        ExecuteMessage FetchResourceDefinition(IServer targetEnv, Guid workspaceId, Guid resourceModelId, bool prepaireForDeployment);
        List<T> FindSourcesByType<T>(IServer targetEnvironment, enSourceType sourceType);
        List<IResourceModel> FindResourcesByID(IServer targetEnvironment, IEnumerable<string> guids, ResourceType resourceType);
        IList<T> GetResourceList<T>(IServer targetEnvironment) where T : new();
        Settings ReadSettings(IServer currentEnv);
        ExecuteMessage WriteSettings(IServer currentEnv, Settings settings);
        ExecuteMessage SaveServerSettings(IServer currentEnv, ServerSettingsData serverSettingsData);
        ServerSettingsData GetServerSettings(IServer currentEnv);
        DbTableList GetDatabaseTables(DbSource dbSource);
        List<SharepointListTo> GetSharepointLists(SharepointSource source);
        DbColumnList GetDatabaseTableColumns(DbSource dbSource, DbTable dbTable);
        List<ISharepointFieldTo> GetSharepointListFields(ISharepointSource source, SharepointListTo list, bool onlyEditable);
        ExecuteMessage GetDependenciesXml(IContextualResourceModel resourceModel, bool getDependsOnMe);
        bool HasDependencies(IContextualResourceModel resourceModel);
        ExecuteMessage StopExecution(IContextualResourceModel resourceModel);
        ICollection<IResourceModel> All();

        ICollection<IResourceModel> Find(Expression<Func<IResourceModel, bool>> expression);
        IResourceModel FindSingle(Expression<Func<IResourceModel, bool>> expression);
        IResourceModel FindSingle(Expression<Func<IResourceModel, bool>> expression, bool fetchDefinition);
        IResourceModel FindSingle(Expression<Func<IResourceModel, bool>> expression, bool fetchDefinition, bool prepairForDeployment);
        ExecuteMessage Save(IResourceModel instanceObj);
        void Load(bool force);
        void ReLoadResources();
        ExecuteMessage DeleteResourceFromWorkspace(IResourceModel resource);
        IResourceModel LoadResourceFromWorkspace(Guid resourceId, Guid? workspaceId);
        IContextualResourceModel LoadContextualResourceModel(Guid resourceId);
        Task<ExecuteMessage> GetDependenciesXmlAsync(IContextualResourceModel resourceModel, bool getDependsOnMe);
        Task<IContextualResourceModel> LoadContextualResourceModelAsync(Guid resourceId);
        TestSaveResult SaveTests(IResourceModel resourceId, List<IServiceTestModelTO> tests);
        List<IServiceTestModelTO> LoadResourceTests(Guid resourceId);
        List<IServiceTestModelTO> LoadAllTests();
        void DeleteResourceTest(Guid resourceId, string testName);
        List<IServiceTestModelTO> LoadResourceTestsForDeploy(Guid resourceId);
        IServiceTestModelTO ExecuteTest(IContextualResourceModel resourceModel, string testName);

        Task<ExecuteMessage> DeleteResourceFromWorkspaceAsync(IContextualResourceModel resourceModel);
        List<ISearchResult> Filter(ISearch searchValue);
    }
}