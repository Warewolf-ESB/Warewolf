#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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