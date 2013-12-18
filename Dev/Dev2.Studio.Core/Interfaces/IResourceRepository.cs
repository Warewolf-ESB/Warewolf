using Caliburn.Micro;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IResourceRepository : IDisposable
    {
        List<IResourceModel> ReloadResource(Guid resourceID, ResourceType resourceType, IEqualityComparer<IResourceModel> equalityComparer, bool fetchXAML);
        void UpdateWorkspace(IList<IWorkspaceItem> workspaceItems);
        // ReSharper disable once InconsistentNaming
        void Rename(string resourceID, string newName);
        void DeployResource(IResourceModel resource);
        ExecuteMessage DeleteResource(IResourceModel resource);
        bool ResourceExist(IResourceModel resource);
        bool IsReservedService(string resourceName);
        bool IsWorkflow(string resourceName);
        void Add(IResourceModel resource);
        void ForceLoad();

        bool IsLoaded { get; set; } // BUG 9276 : TWR : 2013.04.19 - added IsLoaded check to prevent unnecessary loading of resources
        void RefreshResource(Guid resourceID);
        bool IsInCache(Guid id);
        bool DoesResourceExistInRepo(IResourceModel resource);
        void RemoveFromCache(Guid resourceID);
        void RenameCategory(string oldCategory, string newCategory, ResourceType resourceType);
        ExecuteMessage SaveToServer(IResourceModel instanceObj);

        void DeployResources(IEnvironmentModel targetEnviroment, IEnvironmentModel sourceEnviroment, IDeployDTO dto, IEventAggregator eventPublisher);
        ExecuteMessage FetchResourceDefinition(IEnvironmentModel targetEnv, Guid workspaceID, Guid resourceModelID);
        List<T> FindSourcesByType<T>(IEnvironmentModel targetEnvironment, enSourceType sourceType);
        List<IResourceModel> FindResourcesByID(IEnvironmentModel targetEnvironment, IEnumerable<string> guids, ResourceType resourceType);
        ExecuteMessage ReadSettings(string key, string value, IEnvironmentModel currentEnv);
        ExecuteMessage WriteSettings(string key, string value, IEnvironmentModel currentEnv);
        string GetServerLogTempPath(IEnvironmentModel environmentModel);
        DbTableList GetDatabaseTables(DbSource dbSource);
        DbColumnList GetDatabaseTableColumns(DbSource dbSource, DbTable dbTable);
        ExecuteMessage GetDependenciesXml(IContextualResourceModel resourceModel, bool getDependsOnMe);
        List<string> GetDependanciesOnList(List<IContextualResourceModel> resourceModels, IEnvironmentModel environmentModel, bool getDependsOnMe = false);
        List<IResourceModel> GetUniqueDependencies(IContextualResourceModel resourceModel);
        bool HasDependencies(IContextualResourceModel resourceModel);

        ExecuteMessage StopExecution(IContextualResourceModel resourceModel);
        void AddEnvironment(IEnvironmentModel targetEnvironment, IEnvironmentModel environment);
        ExecuteMessage SaveResource(IEnvironmentModel targetEnvironment, StringBuilder resourceDefinition, Guid workspaceID);
        void RemoveEnvironment(IEnvironmentModel targetEnvironment, IEnvironmentModel environment);

        ICollection<IResourceModel> All();
        ICollection<IResourceModel> Find(Expression<Func<IResourceModel, bool>> expression);
        IResourceModel FindSingle(Expression<Func<IResourceModel, bool>> expression);
        ExecuteMessage Save(IResourceModel instanceObj);
        void Save(ICollection<IResourceModel> instanceObjs);
        event EventHandler ItemAdded;
        void Load();
        void Remove(IResourceModel instanceObj);
        void Remove(ICollection<IResourceModel> instanceObjs);
    }
}
