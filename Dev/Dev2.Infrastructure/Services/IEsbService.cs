
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Communication;
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
        IExecuteMessage ClearLog(Guid workspaceID, string directory);

        /// <summary>
        /// Fetches the compile messages.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="serviceID">The service unique identifier.</param>
        /// <param name="filterList">The filter list.</param>
        /// <returns></returns>
        ICompileMessageList FetchCompileMessages(Guid workspaceID, Guid serviceID, string filterList);

        /// <summary>
        /// Fetches the current server log.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <returns></returns>
        IExecuteMessage FetchCurrentServerLog(Guid workspaceID);

        /// <summary>
        /// Fetches the dependent compile messages.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="serviceID">The service unique identifier.</param>
        /// <param name="filterList">The filter list.</param>
        /// <returns></returns>
        ICompileMessageList FetchDependantCompileMessages(Guid workspaceID, Guid serviceID, string filterList);

        /// <summary>
        /// Fetches the dependent compile messages.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="invokerID">The invoker unique identifier.</param>
        /// <returns></returns>
        IList<IDebugState> FetchDependantCompileMessages(Guid workspaceID, Guid invokerID);

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
