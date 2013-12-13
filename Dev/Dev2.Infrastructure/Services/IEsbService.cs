using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Communication;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Services
{
    public interface IEsbService
    {
        /// <summary>
        /// Clears the log.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        Task<ExecuteMessage> ClearLog(Guid workspaceID, string directory);

        /// <summary>
        /// Fetches the compile messages.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="serviceID">The service unique identifier.</param>
        /// <param name="filterList">The filter list.</param>
        /// <returns></returns>
        Task<CompileMessageList> FetchCompileMessages(Guid workspaceID, Guid serviceID, string filterList);

        /// <summary>
        /// Fetches the current server log.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <returns></returns>
        Task<ExecuteMessage> FetchCurrentServerLog(Guid workspaceID);

        /// <summary>
        /// Fetches the dependant compile messages.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="serviceID">The service unique identifier.</param>
        /// <param name="filterList">The filter list.</param>
        /// <returns></returns>
        Task<CompileMessageList> FetchDependantCompileMessages(Guid workspaceID, Guid serviceID, string filterList);

        /// <summary>
        /// Fetches the dependant compile messages.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="invokerID">The invoker unique identifier.</param>
        /// <returns></returns>
        Task<IList<DebugState>> FetchDependantCompileMessages(Guid workspaceID, Guid invokerID);

        /// <summary>
        /// Gets the database columns for table.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="database">The database.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        Task<DbColumnList> GetDatabaseColumnsForTable(Guid workspaceID, string database, string tableName);

        /// <summary>
        /// Gets the database tables.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="database">The database.</param>
        /// <returns></returns>
        Task<DbTableList> GetDatabaseTables(Guid workspaceID, string database);

        /// <summary>
        /// Terminates the execution.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceID">The resource unique identifier.</param>
        /// <returns></returns>
        Task<ExecuteMessage> TerminateExecution(Guid workspaceID, Guid resourceID);

        /// <summary>
        /// Fetches the debug item file.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="debugItemFilePath">The debug item file path.</param>
        /// <returns></returns>
        Task<ExecuteMessage> FetchDebugItemFile(Guid workspaceID, string debugItemFilePath);

        /// <summary>
        /// Updates the workspace item.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="itemXml">The item XML.</param>
        /// <param name="isLocal">if set to <c>true</c> [is local].</param>
        /// <returns></returns>
        Task<ExecuteMessage> UpdateWorkspaceItem(Guid workspaceID, string itemXml, bool isLocal);
    }
}
