/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.SignalR.Wrappers;
using Dev2.Studio.Core.Interfaces;
using Warewolf.Resource.Errors;
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable CheckNamespace

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
        T ExecuteCommand<T>(IEnvironmentConnection connection, Guid workspaceId) where T : class;

        /// <summary>
        /// Executes the command async.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <returns></returns>
        Task<T> ExecuteCommandAsync<T>(IEnvironmentConnection connection, Guid workspaceId) where T : class;

        Task<T> ExecuteCompressedCommandAsync<T>(IEnvironmentConnection connection, Guid workspaceId) where T : class;

        void FetchResourceAffectedMessages(IEnvironmentConnection connection, Guid resourceId);
    }

    public class CommunicationController : ICommunicationController
    {
        public CommunicationController()
        {
            ServicePayload = new EsbExecuteRequest();
        }

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

        private static string ContainsAuthorizationError(string authorizationError, out bool containsAuthorization)
        {
            var authorizationErrors = new List<string>
            {
                ErrorResource.NotAuthorizedToCreateException,
                ErrorResource.NotAuthorizedToViewException,
                ErrorResource.NotAuthorizedToAdministratorException,
                ErrorResource.NotAuthorizedToContributeException,
                ErrorResource.NotAuthorizedToDeployFromException,
                ErrorResource.NotAuthorizedToExecuteException,
                ErrorResource.NotAuthorizedToDeployToException,
            };
            if (string.IsNullOrEmpty(authorizationError))
            {
                containsAuthorization = false;
                return "";
            }
            containsAuthorization = authorizationErrors.Any(err => err.ToUpper().Contains(authorizationError.ToUpper()));

            return authorizationError;
        }

        private static void ShowAuthorizationErrorPopup(string ex)
        {
            var popupController = CustomContainer.Get<IPopupController>();
            popupController?.Show(ex, ErrorResource.ServiceNotAuthorizedExceptionHeader, MessageBoxButton.OK,
                MessageBoxImage.Error, "", false, false, true, false, false, false);
        }


        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <returns></returns>
        public T ExecuteCommand<T>(IEnvironmentConnection connection, Guid workspaceId) where T : class
        {
            var serializer = new Dev2JsonSerializer();
            var popupController = CustomContainer.Get<IPopupController>();
            if (connection == null || !connection.IsConnected)
            {
                IsConnectionValid(connection, popupController);
            }
            else
            {
                if (ServicePayload == null)
                {
                    ServicePayload = new EsbExecuteRequest();
                }

                ServicePayload.ServiceName = ServiceName;
                StringBuilder toSend = serializer.SerializeToBuilder(ServicePayload);
                StringBuilder payload = connection.ExecuteCommand(toSend, workspaceId);
                ValidatePayload(connection, payload, popupController);
                var executeCommand = serializer.Deserialize<T>(payload);
                if (executeCommand == null)
                {
                    var execMessage = serializer.Deserialize<ExecuteMessage>(payload);
                    if (execMessage != null)
                    {
                        return CheckAuthorization<T>(execMessage);
                    }
                }
                else
                {
                    if (typeof(T) == typeof(ExecuteMessage))
                    {
                        return CheckAuthorization<T>(executeCommand as ExecuteMessage);
                    }
                    return executeCommand;
                }
            }
            return default(T);
        }


        private static T CheckAuthorization<T>(ExecuteMessage message) where T : class
        {
            if (message != null)
            {
                bool containsAuthorization;
                var s = ContainsAuthorizationError(message.Message.ToString(), out containsAuthorization);
                if (containsAuthorization)
                {
                    ShowAuthorizationErrorPopup(s);
                    if (typeof(T) == typeof(IExplorerRepositoryResult))
                    {
                        var explorerRepositoryResult = new ExplorerRepositoryResult(ExecStatus.Fail, s);
                        return explorerRepositoryResult as T;
                    }
                    if (typeof(T) == typeof(ExecuteMessage))
                    {
                        var returnMessage = new ExecuteMessage
                        {
                            HasError = true,
                            Message = new StringBuilder(s)
                        };
                        return returnMessage as T;
                    }
                }
                else
                {
                    if (typeof(T) == typeof(ExecuteMessage))
                    {
                        return message as T;
                    }
                }
            }
            return default(T);
        }

        private static void ValidatePayload(IEnvironmentConnection connection, StringBuilder payload, IPopupController popupController)
        {
            if (payload == null || payload.Length == 0)
            {
                if (connection.HubConnection != null && popupController != null && connection.HubConnection.State == ConnectionStateWrapped.Disconnected)
                {
                    popupController.Show(ErrorResource.ServerconnectionDropped + Environment.NewLine + "Please ensure that your server is still running and your network connection is working."
                        , "Server dropped", MessageBoxButton.OK, MessageBoxImage.Information, "", false, false, true, false, false, false);
                }
            }
        }

        private static void IsConnectionValid(IEnvironmentConnection connection, IPopupController popupController)
        {
            if (connection != null)
            {
                try
                {
                    if (!connection.IsConnecting)
                    {
                        popupController?.Show(string.Format(ErrorResource.ServerDisconnected, connection.DisplayName) + Environment.NewLine +
                                              ErrorResource.ServerReconnectForActions, ErrorResource.ServerDisconnectedHeader, MessageBoxButton.OK,
                            MessageBoxImage.Information, "", false, false, true, false, false, false);
                    }
                }
                catch (Exception e)
                {
                    Dev2Logger.Error("Error popup", e);
                }
            }
        }

        public void FetchResourceAffectedMessages(IEnvironmentConnection connection, Guid resourceId)
        {
            connection.FetchResourcesAffectedMemo(resourceId);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <returns></returns>
        public async Task<T> ExecuteCommandAsync<T>(IEnvironmentConnection connection, Guid workspaceId) where T : class
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
                        popupController?.Show(string.Format(ErrorResource.ServerDisconnected, connection.DisplayName) + Environment.NewLine +
                                              ErrorResource.ServerReconnectForActions, ErrorResource.ServerDisconnectedHeader, MessageBoxButton.OK,
                                              MessageBoxImage.Information, "", false, false, true, false, false, false);
                    }
                }
            }
            else
            {
                try
                {
                    if (ServicePayload == null)
                    {
                        ServicePayload = new EsbExecuteRequest();
                    }

                    ServicePayload.ServiceName = ServiceName;
                    StringBuilder toSend = serializer.SerializeToBuilder(ServicePayload);
                    var payload = await connection.ExecuteCommandAsync(toSend, workspaceId);
                    var executeCommand = serializer.Deserialize<T>(payload);
                    if (executeCommand == null)
                    {
                        var execMessage = serializer.Deserialize<ExecuteMessage>(payload);
                        if (execMessage != null)
                        {
                            return CheckAuthorization<T>(execMessage);
                        }
                    }
                    else
                    {
                        if (typeof(T) == typeof(ExecuteMessage))
                        {
                            return CheckAuthorization<T>(executeCommand as ExecuteMessage);
                        }
                        return executeCommand;
                    }

                }
                catch (ServiceNotAuthorizedException ex)
                {
                    ShowAuthorizationErrorPopup(ex.Message);
                    return default(T);
                }
                catch (AggregateException ex)
                {
                    var aggregateException = ex.Flatten();
                    var baseException = aggregateException.GetBaseException();
                    var isAuthorizationError = baseException is ServiceNotAuthorizedException;
                    if (isAuthorizationError)
                    {
                        ShowAuthorizationErrorPopup(ex.Message);
                        return default(T);
                    }

                }
            }
            return default(T);
        }

        public async Task<T> ExecuteCompressedCommandAsync<T>(IEnvironmentConnection connection, Guid workspaceId) where T : class
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
                        popupController?.Show(string.Format(ErrorResource.ServerDisconnected, connection.DisplayName) + Environment.NewLine +
                                              ErrorResource.ServerReconnectForActions, ErrorResource.ServerDisconnectedHeader, MessageBoxButton.OK,
                                              MessageBoxImage.Information, "", false, false, true, false, false, false);
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


        public T ExecuteCompressedCommand<T>(IEnvironmentConnection connection, Guid workspaceId) where T : class
        {
            // build the service request payload ;)
            var serializer = new Dev2JsonSerializer();

            if (connection == null)
            { return default(T); }
            if (!connection.IsConnected && !connection.IsConnecting)
            {
                var popupController = CustomContainer.Get<IPopupController>();
                popupController?.Show(string.Format(ErrorResource.ServerDisconnected, connection.DisplayName) + Environment.NewLine +
                                      ErrorResource.ServerReconnectForActions, ErrorResource.ServerDisconnectedHeader, MessageBoxButton.OK,
                                      MessageBoxImage.Information, "", false, false, true, false, false, false);
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
                    if (payload == null || payload.Length == 0)
                    {
                        return default(T);
                    }
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
