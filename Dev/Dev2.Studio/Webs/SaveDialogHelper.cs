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
using System.Windows;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Webs.Callbacks;
using Newtonsoft.Json;
using Warewolf.Studio.ViewModels;

namespace Dev2.Webs
{
    public static class SaveDialogHelper
    {
        public static void ShowNewWorkflowSaveDialog(IContextualResourceModel resourceModel) => ShowNewWorkflowSaveDialog(resourceModel, null, true);
        public static void ShowNewWorkflowSaveDialog(IContextualResourceModel resourceModel, string resourceId, bool addToTabManager) => ShowSaveDialog(resourceModel, new SaveNewWorkflowCallbackHandler(EventPublishers.Aggregator, ServerRepository.Instance, resourceModel, addToTabManager));
        internal static void ShowNewWorkflowSaveDialog(IContextualResourceModel contextualResourceModel, bool loadingFromServer, string originalPath)
            => ShowSaveDialog(contextualResourceModel
               , new SaveNewWorkflowCallbackHandler(EventPublishers.Aggregator, ServerRepository.Instance, contextualResourceModel, true)
               , loadingFromServer: loadingFromServer
               , originalPath: originalPath);

        static async void ShowSaveDialog(IContextualResourceModel resourceModel, WebsiteCallbackHandler callbackHandler, Action loaded = null, bool loadingFromServer = true, string originalPath = "")
        {
            try
            {
                if (resourceModel == null)
                {
                    throw new ArgumentNullException(nameof(resourceModel));
                }
                var server = resourceModel.Environment;
                ServerRepository.Instance.ActiveServer = server ?? throw new ArgumentNullException("environment");

                if (server.Permissions == null)
                {
                    server.Permissions = new List<IWindowsGroupPermission>();
                    server.Permissions.AddRange(server.AuthorizationService.SecurityService.Permissions);
                }
                if (resourceModel.Category == null)
                {
                    resourceModel.Category = "";
                }

                var selectedPath = resourceModel.Category.Contains("Unassigned") || string.IsNullOrEmpty(resourceModel.Category) ? "" : resourceModel.Category;
                var lastIndexOf = selectedPath.LastIndexOf("\\", StringComparison.Ordinal);
                if (lastIndexOf != -1)
                {
                    selectedPath = selectedPath.Substring(0, lastIndexOf);
                }
                selectedPath = selectedPath.Replace("\\", "\\\\");

                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                var environmentViewModel = mainViewModel?.ExplorerViewModel?.Environments.FirstOrDefault(model => model.Server.EnvironmentID == resourceModel.Environment.EnvironmentID);
                if (environmentViewModel == null)
                {
                    return;
                }

                var header = string.IsNullOrEmpty(resourceModel.Category) ? "Unsaved Item" : resourceModel.Category;
                var lastHeaderIndexOf = header.LastIndexOf("\\", StringComparison.Ordinal);
                if (lastHeaderIndexOf != -1)
                {
                    header = header.Substring(lastHeaderIndexOf, header.Length - lastHeaderIndexOf);
                    header = header.Replace("\\", "");
                }

                var requestViewModel = await RequestServiceNameViewModel.CreateAsync(environmentViewModel, selectedPath, header);

                var messageBoxResult = requestViewModel.ShowSaveDialog();
                if (messageBoxResult == MessageBoxResult.OK)
                {
                    var value = new { resourceName = requestViewModel.ResourceName.Name, resourcePath = requestViewModel.ResourceName.Path, resourceLoadingFromServer = loadingFromServer, OriginalPath = originalPath };
                    var serializeObject = JsonConvert.SerializeObject(value);
                    callbackHandler.Save(serializeObject, server);
                }
                else
                {
                    if (!loadingFromServer)
                    {
                        mainViewModel.CloseResource(resourceModel, server.EnvironmentID);
                    }
                }
                loaded?.Invoke();
            }
            catch (Exception)
            {
                loaded?.Invoke();
                throw;
            }
        }
        
        public static void ShowNewWorkflowSaveDialog(IContextualResourceModel resourceModel, string resourceId, bool addToTabManager, Action action)
        {
            ShowSaveDialog(resourceModel, new SaveNewWorkflowCallbackHandler(EventPublishers.Aggregator, ServerRepository.Instance, resourceModel, addToTabManager), action);
        }
    }
}
