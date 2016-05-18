
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Webs.Callbacks;
using Newtonsoft.Json;
using Warewolf.Studio.AntiCorruptionLayer;
using Warewolf.Studio.ViewModels;

namespace Dev2.Webs
{
    public static class SaveDialogHelper
    {

        #region ShowDialog(IEnvironmentModel environment, ResourceType resourceType, string resourceID = null)


        #endregion

        #region ShowSaveDialog

        public static void ShowNewWorkflowSaveDialog(IContextualResourceModel resourceModel, string resourceId = null, bool addToTabManager = true)
        {
            ShowSaveDialog(resourceModel, new SaveNewWorkflowCallbackHandler(EventPublishers.Aggregator, EnvironmentRepository.Instance, resourceModel, addToTabManager));
        }

        static async void ShowSaveDialog(IContextualResourceModel resourceModel, WebsiteCallbackHandler callbackHandler, Action loaded = null)
        {
            try
            {
                if (resourceModel == null)
                {
                    throw new ArgumentNullException("resourceModel");
                }
                IEnvironmentModel environment = resourceModel.Environment;

                if (environment == null)
                {
                    // ReSharper disable NotResolvedInText
                    throw new ArgumentNullException("environment");
                }

                EnvironmentRepository.Instance.ActiveEnvironment = environment;

                IServer server = new Server(environment);
                if (server.Permissions == null)
                {
                    server.Permissions = new List<IWindowsGroupPermission>();
                    server.Permissions.AddRange(environment.AuthorizationService.SecurityService.Permissions);
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
                var env = new EnvironmentViewModel(server, CustomContainer.Get<IShellViewModel>(), true);

                var header = string.IsNullOrEmpty(resourceModel.Category) ? "Unsaved Item" : resourceModel.Category;
                var lastHeaderIndexOf = header.LastIndexOf("\\", StringComparison.Ordinal);
                if (lastHeaderIndexOf != -1)
                {
                    header = header.Substring(lastHeaderIndexOf, header.Length - lastHeaderIndexOf);
                    header = header.Replace("\\", "");
                }

                var requestViewModel = await RequestServiceNameViewModel.CreateAsync(env, selectedPath, header);

                if (loaded != null)
                {
                    loaded();
                }
                var messageBoxResult = requestViewModel.ShowSaveDialog();
                if (messageBoxResult == MessageBoxResult.OK)
                {
                    var value = new { resourceName = requestViewModel.ResourceName.Name, resourcePath = requestViewModel.ResourceName.Path };
                    var serializeObject = JsonConvert.SerializeObject(value);
                    callbackHandler.Save(serializeObject, environment);
                }
            }
            catch (Exception)
            {
                if (loaded != null)
                {
                    loaded();
                }
                throw;
            }
        }


        #endregion

        public static void ShowNewWorkflowSaveDialog(IContextualResourceModel resourceModel, string resourceId, bool addToTabManager, Action action)
        {
            ShowSaveDialog(resourceModel, new SaveNewWorkflowCallbackHandler(EventPublishers.Aggregator, EnvironmentRepository.Instance, resourceModel, addToTabManager), action);
        }
    }
}
