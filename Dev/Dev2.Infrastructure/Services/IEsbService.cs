using System;
using System.Collections.Generic;
using Dev2.Communication;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Services
{
    /// <summary>
    /// Resource Services - Was for EsbHub, NOW to give fixed interfacing from Studio via an EsbServiceCatalog 
    /// </summary>
    public interface IEsbService
    {
        /// <summary>
        /// Clears the log.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        ExecuteMessage ClearLog(Guid workspaceID, string directory);

        /// <summary>
        /// Fetches the compile messages.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="serviceID">The service unique identifier.</param>
        /// <param name="filterList">The filter list.</param>
        /// <returns></returns>
        CompileMessageList FetchCompileMessages(Guid workspaceID, Guid serviceID, string filterList);

        /// <summary>
        /// Fetches the current server log.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <returns></returns>
        ExecuteMessage FetchCurrentServerLog(Guid workspaceID);

        /// <summary>
        /// Fetches the dependent compile messages.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="serviceID">The service unique identifier.</param>
        /// <param name="filterList">The filter list.</param>
        /// <returns></returns>
        CompileMessageList FetchDependantCompileMessages(Guid workspaceID, Guid serviceID, string filterList);

        /// <summary>
        /// Fetches the dependent compile messages.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="invokerID">The invoker unique identifier.</param>
        /// <returns></returns>
        IList<DebugState> FetchDependantCompileMessages(Guid workspaceID, Guid invokerID);

        /// <summary>
        /// Gets the database columns for table.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="database">The database.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        DbColumnList GetDatabaseColumnsForTable(Guid workspaceID, string database, string tableName);

        /// <summary>
        /// Gets the database tables.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="database">The database.</param>
        /// <returns></returns>
        DbTableList GetDatabaseTables(Guid workspaceID, string database);

        /// <summary>
        /// Terminates the execution.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceID">The resource unique identifier.</param>
        /// <returns></returns>
        ExecuteMessage TerminateExecution(Guid workspaceID, Guid resourceID);

        /// <summary>
        /// Fetches the debug item file.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="debugItemFilePath">The debug item file path.</param>
        /// <returns></returns>
        ExecuteMessage FetchDebugItemFile(Guid workspaceID, string debugItemFilePath);

        /// <summary>
        /// Updates the workspace item.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="itemXml">The item XML.</param>
        /// <param name="isLocal">if set to <c>true</c> [is local].</param>
        /// <returns></returns>
        ExecuteMessage UpdateWorkspaceItem(Guid workspaceID, string itemXml, bool isLocal);
    }
}
