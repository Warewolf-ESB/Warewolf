using System;
using System.Text;
using Dev2.Communication;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Controller
{
    public interface ICommunicationController
    {
        string ServiceName { get; set; }
        EsbExecuteRequest ServicePayload { get; }

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
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <returns></returns>
        T ExecuteCommand<T>(IEnvironmentConnection connection, Guid workspaceID);

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="dataListID">The data list unique identifier.</param>
        /// <returns></returns>
        T ExecuteCommand<T>(IEnvironmentConnection connection, Guid workspaceID, Guid dataListID);
    }

    public class CommunicationController : ICommunicationController
    {
        public string ServiceName { get; set; }

        public EsbExecuteRequest ServicePayload { get; private set; }

        /// <summary>
        /// Adds the payload argument.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void AddPayloadArgument(string key, string value)
        {
            AddPayloadArgument(key, new StringBuilder(value));
        }

        /// <summary>
        /// Adds the payload argument.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void AddPayloadArgument(string key, StringBuilder value)
        {
            if(ServicePayload == null)
            {
                ServicePayload = new EsbExecuteRequest();
            }

            ServicePayload.AddArgument(key, value);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <returns></returns>
        public T ExecuteCommand<T>(IEnvironmentConnection connection, Guid workspaceID)
        {
            return ExecuteCommand<T>(connection, workspaceID, Guid.Empty);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="dataListID">The data list unique identifier.</param>
        /// <returns></returns>
        public T ExecuteCommand<T>(IEnvironmentConnection connection, Guid workspaceID, Guid dataListID)
        {
            // build the service request payload ;)
            var serializer = new Dev2JsonSerializer();

            if(connection == null || !connection.IsConnected)
            {
                throw new Exception("No connected environment found to perform operation on.");
            }

            // now bundle it up into a nice string builder ;)
            if (ServicePayload == null)
            {
                ServicePayload = new EsbExecuteRequest();
            }

            ServicePayload.ServiceName = ServiceName;
            StringBuilder toSend = serializer.SerializeToBuilder(ServicePayload);
            var payload = connection.ExecuteCommand(toSend, workspaceID, dataListID);

            return serializer.Deserialize<T>(payload);
        }

    }
}
