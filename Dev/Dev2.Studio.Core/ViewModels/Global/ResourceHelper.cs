
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
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core
{
    public static class ResourceHelper
    {
        public static IContextualResourceModel GetContextualResourceModel(object dataContext)
        {
            IContextualResourceModel resourceModel = null;

            TypeSwitch.Do(
                dataContext,
                TypeSwitch.Case<IContextualResourceModel>(x => resourceModel = x),
                TypeSwitch.Case<IWorkflowDesignerViewModel>(x => resourceModel = x.ResourceModel),
                TypeSwitch.Case<IServiceDebugInfoModel>(x => resourceModel = x.ResourceModel),
                TypeSwitch.Case<ILayoutGridViewModel>(x => resourceModel = x.ResourceModel),
                TypeSwitch.Case<IWebActivity>(x => resourceModel = x.ResourceModel));

            return resourceModel;
        }

        public static string GetIconPath(IContextualResourceModel resource)
        {
            string iconPath = resource.IconPath;
            if(string.IsNullOrEmpty(resource.UnitTestTargetWorkflowService))
            {
                if(string.IsNullOrEmpty(resource.IconPath))
                {
                    iconPath = ResourceType.WorkflowService.GetIconLocation();
                }
            }
            else
            {
                iconPath = string.IsNullOrEmpty(resource.IconPath)
                               ? string.Empty
                               : resource.IconPath;
            }
            return iconPath;
        }

        /// <summary>
        /// Gets the display name associated with a specific resource and environment - used for tab headers
        /// </summary>
        /// <param name="resourceModel">The resource model.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/06/03</date>
        public static string GetDisplayName(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null)
            {
                return String.Empty;
            }
            string displayName = resourceModel.ResourceName;
            if(resourceModel.Environment != null && !resourceModel.Environment.IsLocalHost)
            {
                displayName += " - " + resourceModel.Environment.Name;
            }
            if(!resourceModel.IsWorkflowSaved)
            {
                displayName += " *";
            }
            return displayName;
        }
    }
}
