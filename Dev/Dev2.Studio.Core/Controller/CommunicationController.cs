#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Studio.Interfaces;
using Warewolf.Resource.Errors;




namespace Dev2.Controller
{
    public interface ICommunicationController
    {
        string ServiceName { get; set; }
        EsbExecuteRequest ServicePayload { get; }
        
        void AddPayloadArgument(string key, string value);
        
        void AddPayloadArgument(string key, StringBuilder value);
        
        T ExecuteCommand<T>(IEnvironmentConnection connection, Guid workspaceId) where T : class;
        T ExecuteCommand<T>(IEnvironmentConnection connection, Guid workspaceId, int timeout) where T : class;
        
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

        public void AddPayloadArgument(string key, string value)
        {
            AddPayloadArgument(key, new StringBuilder(value));
        }

        public void AddPayloadArgument(string key, StringBuilder value)
        {
            if (ServicePayload == null)
            {
                ServicePayload = new EsbExecuteRequest();
            }

            ServicePayload.AddArgument(key, value);
        }

        static string ContainsAuthorizationError(string authorizationError, out bool containsAuthorization)
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

        static void ShowAuthorizationErrorPopup(string ex)
        {
            var popupController = CustomContainer.Get<IPopupController>();
            popupController?.Show(ex, ErrorResource.ServiceNotAuthorizedExceptionHeader, MessageBoxButton.OK,
                MessageBoxImage.Error, "", false, false, true, false, false, false);
        }

        public T ExecuteCommand<T>(IEnvironmentConnection connection, Guid workspaceId) where T : class
        {
            return ExecuteCommand<T>(connection, workspaceId, 60000);
        }
        public T ExecuteCommand<T>(IEnvironmentConnection connection, Guid workspaceId, int timeout) where T : class
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
                var toSend = serializer.SerializeToBuilder(ServicePayload);
                var payload = connection.ExecuteCommand(toSend, workspaceId, timeout);
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


        static T CheckAuthorization<T>(ExecuteMessage message) where T : class
        {
            if (message != null)
            {
                var s = ContainsAuthorizationError(message.Message.ToString(), out bool containsAuthorization);
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

        static void ValidatePayload(IEnvironmentConnection connection, StringBuilder payload, IPopupController popupController)
        {
            if ((payload == null || payload.Length == 0) && connection.HubConnection != null && popupController != null && connection.HubConnection.State == ConnectionStateWrapped.Disconnected && Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    popupController.Show(ErrorResource.ServerconnectionDropped + Environment.NewLine + ErrorResource.EnsureConnectionToServerWorking
                    , ErrorResource.ServerDroppedErrorHeading, MessageBoxButton.OK, MessageBoxImage.Information, "", false, false, true, false, false, false);
                });
            }
        }

        static void IsConnectionValid(IEnvironmentConnection connection, IPopupController popupController)
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
                    Dev2Logger.Error("Error popup", e, "Warewolf Error");
                }
            }
        }

        public void FetchResourceAffectedMessages(IEnvironmentConnection connection, Guid resourceId) => connection.FetchResourcesAffectedMemo(resourceId);

        public async Task<T> ExecuteCommandAsync<T>(IEnvironmentConnection connection, Guid workspaceId) where T : class
        {
            // build the service request payload ;)
            var serializer = new Dev2JsonSerializer();

            if (connection == null || !connection.IsConnected)
            {
                if (connection != null && !connection.IsConnecting)
                {
                    var popupController = CustomContainer.Get<IPopupController>();
                    popupController?.Show(string.Format(ErrorResource.ServerDisconnected, connection.DisplayName) + Environment.NewLine +
                                            ErrorResource.ServerReconnectForActions, ErrorResource.ServerDisconnectedHeader, MessageBoxButton.OK,
                                            MessageBoxImage.Information, "", false, false, true, false, false, false);
                }
                return default(T);
            }
            try
            {
                if (ServicePayload == null)
                {
                    ServicePayload = new EsbExecuteRequest();
                }

                ServicePayload.ServiceName = ServiceName;
                var toSend = serializer.SerializeToBuilder(ServicePayload);
                var payload = await connection.ExecuteCommandAsync(toSend, workspaceId).ConfigureAwait(true);
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
            return default(T);
        }

        public async Task<T> ExecuteCompressedCommandAsync<T>(IEnvironmentConnection connection, Guid workspaceId) where T : class
        {
            // build the service request payload ;)
            var serializer = new Dev2JsonSerializer();

            if (connection == null || !connection.IsConnected)
            {
                if (connection != null && !connection.IsConnecting)
                {
                    var popupController = CustomContainer.Get<IPopupController>();
                    popupController?.Show(string.Format(ErrorResource.ServerDisconnected, connection.DisplayName) + Environment.NewLine +
                                          ErrorResource.ServerReconnectForActions, ErrorResource.ServerDisconnectedHeader, MessageBoxButton.OK,
                                          MessageBoxImage.Information, "", false, false, true, false, false, false);
                }

            }
            else
            {
                if (ServicePayload == null)
                {
                    ServicePayload = new EsbExecuteRequest();
                }

                ServicePayload.ServiceName = ServiceName;
                var toSend = serializer.SerializeToBuilder(ServicePayload);
                var payload = await connection.ExecuteCommandAsync(toSend, workspaceId).ConfigureAwait(true);

                if (payload.Length > 0)
                {
                    try
                    {
                        var message = serializer.Deserialize<CompressedExecuteMessage>(payload).GetDecompressedMessage();
                        return serializer.Deserialize<T>(message);
                    }
                    catch (NullReferenceException e)
                    {
                        Dev2Logger.Debug("fallback to non compressed", e, "Warewolf Debug");
                        var val = serializer.Deserialize<T>(payload);
                        return val;
                    }
                }
            }
            return default(T);
        }


        public T ExecuteCompressedCommand<T>(IEnvironmentConnection connection, Guid workspaceId) where T : class
        {
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
                var toSend = serializer.SerializeToBuilder(ServicePayload);
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
                    Dev2Logger.Debug("fallback to non compressed", e, "Warewolf Debug");
                    return serializer.Deserialize<T>(payload);

                }

            }
            return default(T);
        }
    }
}
