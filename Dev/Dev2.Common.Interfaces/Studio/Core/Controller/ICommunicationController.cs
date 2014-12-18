using System;
using System.Text;

namespace Dev2.Common.Interfaces.Studio.Core.Controller
{
    public interface ICommunicationController
    {
        string ServiceName { get; set; }
        IEsbExecuteRequest ServicePayload { get; }

        /// <summary>
        /// Adds the payload argument.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void AddPayloadArgument(string key, string value);

        /// <summary>
        /// Adds the payload argument.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void AddPayloadArgument(string key, StringBuilder value);

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <returns></returns>
        T ExecuteCommand<T>(IEnvironmentConnection connection, Guid workspaceId);

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="dataListId">The data list unique identifier.</param>
        /// <returns></returns>
        T ExecuteCommand<T>(IEnvironmentConnection connection, Guid workspaceId, Guid dataListId);
    }
}