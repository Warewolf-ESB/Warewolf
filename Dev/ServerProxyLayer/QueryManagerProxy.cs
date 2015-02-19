using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Warewolf.Core;

namespace Warewolf.Studio.ServerProxyLayer
{
    public class QueryManagerProxy:ProxyBase, IQueryManager{



        public QueryManagerProxy(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection):base(communicationControllerFactory,connection)
        {

        }


        #region Implementation of IQueryManager

        /// <summary>
        /// Gets the dependencies of a resource. a dependency referes to a nested resource
        /// </summary>
        /// <param name="resourceId">the resource</param>
        /// <returns>a list of tree dependencies</returns>
        public IList<IResource> FetchDependencies(Guid resourceId)
        {

            return FetchDependantsFromServerService(resourceId, true);
        }

        IList<IResource> FetchDependantsFromServerService(Guid resourceId, bool getDependsOnMe)
        {
            var comsController = CommunicationControllerFactory.CreateController("FindDependencyService");
            comsController.AddPayloadArgument("ResourceId", resourceId.ToString());
            comsController.AddPayloadArgument("GetDependsOnMe", getDependsOnMe.ToString());

            var workspaceId = Connection.WorkspaceID;
            var payload = comsController.ExecuteCommand<IList<IResource>>(Connection, workspaceId);

            return payload;
        }

        /// <summary>
        /// Get the list of items that use this resource a nested resource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public IList<IResource> FetchDependants(Guid resourceId)
        {
            return FetchDependantsFromServerService(resourceId, false);
        }

        /// <summary>
        /// Fetch a heavy weight reource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public StringBuilder FetchResourceXaml(Guid resourceId)
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchResourceDefinitionService" );
            comsController.AddPayloadArgument("ResourceId", resourceId.ToString());

            var result = comsController.ExecuteCommand<StringBuilder>(Connection, Connection.WorkspaceID);
            return result;
        }

        /// <summary>
        /// Get a list of tables froma db source
        /// </summary>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public IList<IDbTable> FetchTables(Guid sourceId)
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchTablesService" );
            comsController.AddPayloadArgument("ResourceId", sourceId.ToString());
       
            var result = comsController.ExecuteCommand<IList<IDbTable>>(Connection, Connection.WorkspaceID);
            return result;
        }

        public IResourceDefinition FetchResource(Guid resourceId)
        {
            var comsController = CommunicationControllerFactory.CreateController("FindResourcesByID");
            comsController.AddPayloadArgument("GuidCsv", resourceId.ToString());
            comsController.AddPayloadArgument("ResourceType", ResourceType.WorkflowService.ToString());

            var result = comsController.ExecuteCommand<List<SerializableResource>>(Connection, Connection.WorkspaceID);
            return result.First();

        }

        /// <summary>
        /// Fetch the resource including the xaml
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public IXamlResource FetchResourceWithXaml(Guid resourceId)
        {
            var resource = FetchResource(resourceId);
            var xaml = FetchResourceXaml(resourceId);
            return new XamlResource(resource, xaml);
        }

        /// <summary>
        /// Loads the Tree.
        /// </summary>
        /// <returns></returns>
        public async Task<IExplorerItem> Load()
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchExplorerItemsService");

            var workspaceId = Connection.WorkspaceID;
            var result = await comsController.ExecuteCommandAsync<IExplorerItem>(Connection, workspaceId);
            return result;
        }
        #endregion

        public IList<IToolDescriptor> FetchTools()
        {
            var comsController = CommunicationControllerFactory.CreateController("FetchToolsService");

            var workspaceId = Connection.WorkspaceID;
            var result =  comsController.ExecuteCommand<IList<IToolDescriptor>>(Connection, workspaceId);
            return result;
        }
    }
         
    
}