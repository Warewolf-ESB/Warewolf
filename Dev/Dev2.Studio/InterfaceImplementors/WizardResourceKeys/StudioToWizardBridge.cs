
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
using Dev2.Data.Util;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.InterfaceImplementors.WizardResourceKeys
{

    public static class StudioToWizardBridge
    {

        /// <summary>
        /// Returns the correct wizard endpoint depending upon its type
        /// </summary>
        /// <param name="theModel"></param>
        /// <returns></returns>
        public static string SelectWizard(IResourceModel theModel)
        {
            const string Result = "Dev2ServiceDetails"; // defaults to the service wizard

            return Result;
        }

        /// <summary>
        /// Perform the translation between studio and server resouce types
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="serviceDef"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public static string ConvertStudioToWizardType(string resourceType, string serviceDef, string category)
        {
            string result = resourceType;

            if(resourceType == "DatabaseService")
            {
                result = "Database";
            }
            else if(resourceType == "ResourceService")
            {
                result = "Plugin";
            }
            else if(resourceType == "ResourceSource")
            {
                result = "Plugin";
            }
            else if(resourceType == "Service" && (serviceDef == null || serviceDef.IndexOf("Type=\"Plugin\"", StringComparison.Ordinal) > 0))
            {
                result = "Plugin";
            }
            else if(resourceType == "Service" && serviceDef.IndexOf("Type=\"Plugin\"", StringComparison.Ordinal) < 0)
            {
                result = "Database";
            }
            else if(resourceType == "Source" && (serviceDef == null || serviceDef.IndexOf("AssemblyLocation=", StringComparison.Ordinal) > 0))
            {
                result = "Plugin";
            }
            else if(resourceType == "Source" && serviceDef.IndexOf("AssemblyLocation=", StringComparison.Ordinal) < 0)
            {
                result = "Database";
            }
            else if(resourceType == "HumanInterfaceProcess" || category == "Webpage")
            {
                result = "Webpage";
            }
            else if(category == "Website")
            {
                result = "Website";
            }
            else if(resourceType == "WorkflowService")
            {
                result = "Workflow";
            }

            return result;
        }

        /// <summary>
        /// Builds up the POST data for editing a resource
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="rm"></param>
        /// <returns></returns>
        public static string BuildStudioEditPayload(string resourceType, IResourceModel rm)
        {
            StringBuilder result = new StringBuilder();
            string resType = string.Empty;

            // add service type
            result.Append(ResourceKeys.Dev2ServiceType);
            result.Append("=");
            result.Append(resType);

            // add service name
            result.Append("&");
            result.Append(ResourceKeys.Dev2ServiceName);
            result.Append("=");
            result.Append(rm.ResourceName);

            // add category
            result.Append("&");
            result.Append(ResourceKeys.Dev2Category);
            result.Append("=");
            result.Append(rm.Category);

            // add help
            result.Append("&");
            result.Append(ResourceKeys.Dev2Help);
            result.Append("=");
            result.Append(rm.HelpLink); // rm.HelpLink

            // add icon
            result.Append("&");
            result.Append(ResourceKeys.Dev2Icon);
            result.Append("=");
            result.Append(rm.IconPath);

            // add comment
            result.Append("&");
            result.Append(ResourceKeys.Dev2Description);
            result.Append("=");
            result.Append(rm.Comment);

            // add tags
            result.Append("&");
            result.Append(ResourceKeys.Dev2Tags);
            result.Append("=");
            result.Append(rm.Tags);

            string serviceDef = string.Empty; //rm.ServiceDefinition;

            if(serviceDef.IndexOf(" SourceName=", StringComparison.Ordinal) > 0)
            {
                // we have 
                string sourceName = DataListUtil.ExtractAttribute(serviceDef, "Action", "SourceName");
                string sourceMethod = DataListUtil.ExtractAttribute(serviceDef, "Action", "SourceMethod");

                // add source method
                result.Append("&");
                result.Append(ResourceKeys.Dev2SourceMethod);
                result.Append("=");
                result.Append(sourceMethod);

                // add source name
                result.Append("&");
                result.Append(ResourceKeys.Dev2SourceName);
                result.Append("=");
                result.Append(sourceName);

                result.Append("&");
                result.Append(ResourceKeys.Dev2StudioExe);
                result.Append("=");
                result.Append("yes");
            }
            else if(serviceDef.IndexOf("<Source", StringComparison.Ordinal) >= 0)
            {
                // we have a source to process 
                if(resType == "Plugin")
                {
                    result.Append("&");
                    result.Append(ResourceKeys.Dev2SourceManagementSource);
                    result.Append("=");
                    result.Append(rm.ResourceName);
                }
                else if(resType == "Database")
                {
                    result.Append("&");
                    result.Append(ResourceKeys.Dev2SourceManagementDatabaseSource);
                    result.Append("=");
                    result.Append(rm.ResourceName);
                }

                result.Append("&");
                result.Append(ResourceKeys.Dev2SourceName);
                result.Append("=");
                result.Append(rm.ResourceName);

                result.Append("&");
                result.Append(ResourceKeys.Dev2StudioExe);
                result.Append("=");
                result.Append("yes");

            }

            return result.ToString();
        }

        public static string BuildUri(IContextualResourceModel resourceModel, string resName)
        {
            string uriString = "/services/" + SelectWizard(resourceModel);
            if(resourceModel.ResourceType == ResourceType.WorkflowService ||
                resourceModel.ResourceType == ResourceType.Service)
            {
                uriString += "?" + ResourceKeys.Dev2ServiceType + "=" + resName;
            }
            return uriString;
        }

        public static string GetUriString(IContextualResourceModel resourceModel, bool includeArgs)
        {
            string resName = string.Empty;

            var requestUri = new Uri(resourceModel.Environment.Connection.WebServerUri,
                                     BuildUri(resourceModel, resName));

            string uriString = requestUri.AbsoluteUri;

            if(includeArgs)
            {
                string args =
                    BuildStudioEditPayload(resourceModel.ResourceType.ToString(), resourceModel);
                uriString = Browser.FormatUrl(requestUri.AbsoluteUri, args);
            }
            return uriString;
        }

        public static Uri GetWorkflowUrl(IContextualResourceModel resourceModel)
        {
            var relativeUrl = String.Format("/services/{0}?wid={1}", resourceModel.ResourceName,
                                            resourceModel.Environment.Connection.WorkspaceID);
            return new Uri(resourceModel.Environment.Connection.WebServerUri, relativeUrl);
        }
    }
}
