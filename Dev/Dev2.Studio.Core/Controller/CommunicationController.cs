
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
using System.Text;
using System.Windows;
using Dev2.Common.Interfaces.Studio.Controller;
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
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <returns></returns>
        public T ExecuteCommand<T>(IEnvironmentConnection connection, Guid workspaceId)
        {
            return ExecuteCommand<T>(connection, workspaceId, Guid.Empty);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="dataListId">The data list unique identifier.</param>
        /// <returns></returns>
        public T ExecuteCommand<T>(IEnvironmentConnection connection, Guid workspaceId, Guid dataListId)
        {
            // build the service request payload ;)
            var serializer = new Dev2JsonSerializer();

            if(connection == null || !connection.IsConnected)
            {
                if(connection != null)
                {
                    var popupController = CustomContainer.Get<IPopupController>();
                    if(popupController != null)
                    {
                        popupController.Show(string.Format("Server: {0} has disconnected.", connection.DisplayName) + Environment.NewLine +
                                                                     "Please reconnect before performing any actions", "Disconnected Server", MessageBoxButton.OK, MessageBoxImage.Information, "");
                    }
                }
            }
            else
            {

                // now bundle it up into a nice string builder ;)
                if(ServicePayload == null)
                {
                    ServicePayload = new EsbExecuteRequest();
                }

                ServicePayload.ServiceName = ServiceName;
                StringBuilder toSend = serializer.SerializeToBuilder(ServicePayload);
                var payload = connection.ExecuteCommand(toSend, workspaceId, dataListId);

                return serializer.Deserialize<T>(payload);
            }
            return default(T);
        }

    }
}
