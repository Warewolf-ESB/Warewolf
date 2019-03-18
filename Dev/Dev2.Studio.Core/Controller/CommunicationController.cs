#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
                var payload = connection.ExecuteCommand(toSend, workspaceId);
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
