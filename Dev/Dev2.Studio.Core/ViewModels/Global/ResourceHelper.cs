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
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core
{
    public static class ResourceHelper
    {
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
            if (resourceModel.Environment != null && !resourceModel.Environment.IsLocalHost)
            {
                if (!resourceModel.Environment.Name.Contains("localhost"))
                {
                    displayName += " - " + resourceModel.Environment.Name.Replace("(Connected)", "");
                }
                
            }
            if(!resourceModel.IsWorkflowSaved)
            {
                displayName += " *";
            }
            return displayName;
        }
    }
}
