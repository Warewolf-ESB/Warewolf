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
                var environmentViewModel = mainViewModel?.ExplorerViewModel?.Environments?.FirstOrDefault(model => model.Server.EnvironmentID == resourceModel.Environment.EnvironmentID);
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
