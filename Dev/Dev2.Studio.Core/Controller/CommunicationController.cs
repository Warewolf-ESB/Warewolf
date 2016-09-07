/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.SignalR.Wrappers;
using Dev2.Studio.Core.Interfaces;
using Warewolf.Resource.Errors;

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
        /// Executes the command async.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <returns></returns>
        Task<T> ExecuteCommandAsync<T>(IEnvironmentConnection connection, Guid workspaceId);

        Task<T> ExecuteCompressedCommandAsync<T>(IEnvironmentConnection connection, Guid workspaceId);
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
            if (ServicePayload == null)
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

            if (connection == null || !connection.IsConnected)
            {
                if (connection != null)
                {
                    try
                    {
                        if (!connection.IsConnecting)
                        {
                            var popupController = CustomContainer.Get<IPopupController>();
                            popupController?.Show(string.Format(ErrorResource.ServerDissconnected, connection.DisplayName) + Environment.NewLine +
                                                  "Please reconnect before performing any actions", "Disconnected Server", MessageBoxButton.OK, 
                                                  MessageBoxImage.Information, "", false, false, true, false);
                        }
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Error("Error popup", e);
                    }
                }
            }
            else
            {

                // now bundle it up into a nice string builder ;)
                if (ServicePayload == null)
                {
                    ServicePayload = new EsbExecuteRequest();
                }

                ServicePayload.ServiceName = ServiceName;
                StringBuilder toSend = serializer.SerializeToBuilder(ServicePayload);
                var payload = connection.ExecuteCommand(toSend, workspaceId);
                if (payload == null || payload.Length == 0)
                {
                    var popupController = CustomContainer.Get<IPopupController>();
                    if (connection.HubConnection != null && popupController != null && connection.HubConnection.State == ConnectionStateWrapped.Disconnected)
                    {
                        popupController.Show(ErrorResource.ServerconnectionDropped + Environment.NewLine + "Please ensure that your server is still running and your network connection is working."
                                            , "Server dropped", MessageBoxButton.OK, MessageBoxImage.Information, "", false, false, true, false);
                    }
                }
                return serializer.Deserialize<T>(payload);
            }
            return default(T);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <returns></returns>
        public async Task<T> ExecuteCommandAsync<T>(IEnvironmentConnection connection, Guid workspaceId)
        {
            // build the service request payload ;)
            var serializer = new Dev2JsonSerializer();

            if (connection == null || !connection.IsConnected)
            {
                if (connection != null)
                {
                    if (!connection.IsConnecting)
                    {
                        var popupController = CustomContainer.Get<IPopupController>();
                        popupController?.Show(string.Format(ErrorResource.ServerDissconnected, connection.DisplayName) + Environment.NewLine +
                                              "Please reconnect before performing any actions", "Disconnected Server", MessageBoxButton.OK, 
                                              MessageBoxImage.Information, "", false, false, true, false);
                    }
                }
            }
            else
            {

                // now bundle it up into a nice string builder ;)
                if (ServicePayload == null)
                {
                    ServicePayload = new EsbExecuteRequest();
                }

                ServicePayload.ServiceName = ServiceName;
                StringBuilder toSend = serializer.SerializeToBuilder(ServicePayload);
                var payload = await connection.ExecuteCommandAsync(toSend, workspaceId);

                return serializer.Deserialize<T>(payload);
            }
            return default(T);
        }


        public async Task<T> ExecuteCompressedCommandAsync<T>(IEnvironmentConnection connection, Guid workspaceId)
        {
            // build the service request payload ;)
            var serializer = new Dev2JsonSerializer();

            if (connection == null || !connection.IsConnected)
            {
                if (connection != null)
                {
                    if (!connection.IsConnecting)
                    {
                        var popupController = CustomContainer.Get<IPopupController>();
                        popupController?.Show(string.Format(ErrorResource.ServerDissconnected, connection.DisplayName) + Environment.NewLine +
                                              "Please reconnect before performing any actions", "Disconnected Server", MessageBoxButton.OK, 
                                              MessageBoxImage.Information, "", false, false, true, false);
                    }
                }
            }
            else
            {

                // now bundle it up into a nice string builder ;)
                if (ServicePayload == null)
                {
                    ServicePayload = new EsbExecuteRequest();
                }

                ServicePayload.ServiceName = ServiceName;
                StringBuilder toSend = serializer.SerializeToBuilder(ServicePayload);
                var payload = await connection.ExecuteCommandAsync(toSend, workspaceId);

                try
                {
                    var message = serializer.Deserialize<CompressedExecuteMessage>(payload).GetDecompressedMessage();
                    return serializer.Deserialize<T>(message);
                }
                catch (NullReferenceException e)
                {
                    Dev2Logger.Debug("fallback to non compressed", e);
                return serializer.Deserialize<T>(payload);

                }
            }
            return default(T);
        }


        public T ExecuteCompressedCommand<T>(IEnvironmentConnection connection, Guid workspaceId)
        {
            // build the service request payload ;)
            var serializer = new Dev2JsonSerializer();

            if (connection == null || !connection.IsConnected)
            {
                if (connection != null)
                {
                    if (!connection.IsConnecting)
                    {
                        var popupController = CustomContainer.Get<IPopupController>();
                        popupController?.Show(string.Format(ErrorResource.ServerDissconnected, connection.DisplayName) + Environment.NewLine +
                                              "Please reconnect before performing any actions", "Disconnected Server", MessageBoxButton.OK, 
                                              MessageBoxImage.Information, "", false, false, true, false);
                    }
                }
            }
            else
            {

                // now bundle it up into a nice string builder ;)
                if (ServicePayload == null)
                {
                    ServicePayload = new EsbExecuteRequest();
                }

                ServicePayload.ServiceName = ServiceName;
                StringBuilder toSend = serializer.SerializeToBuilder(ServicePayload);
                var payload = connection.ExecuteCommand(toSend, workspaceId);
                try
                {
                    var message = serializer.Deserialize<CompressedExecuteMessage>(payload).GetDecompressedMessage();
                    return serializer.Deserialize<T>(message);
                }
                catch (NullReferenceException e)
                {
                    Dev2Logger.Debug("fallback to non compressed", e);
                    return serializer.Deserialize<T>(payload);

                }

            }
            return default(T);
        }
    }
}
