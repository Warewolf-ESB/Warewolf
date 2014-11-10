
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
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Data.ServiceModel
{
    public static class ResourceTypeConverter
    {
        public const string TypeWorkflowService = "WorkflowService";
        public const string TypeService = "Service";
        public const string TypeSource = "Source";
        public const string TypeReservedService = "ReservedService";
        public const string TypeWildcard = "*";

        #region ToResourceTypes

        /// <summary>
        /// Converts studio resource type strings (WorkflowService, Service, Source, ReservedService, or *) to an array of <see cref="ResourceType"/>'s.
        /// </summary>
        /// <param name="typeStr">The type string: WorkflowService, Service, ReservedService, Source or *.</param>
        /// <param name="returnAllWhenNoMatch">Indicates that all types should be returned when no match is found.</param>
        /// <returns>An array of <see cref="ResourceType"/>'s.</returns>
        public static ResourceType[] ToResourceTypes(string typeStr, bool returnAllWhenNoMatch = true)
        {
            ResourceType[] resourceTypes;
            switch(typeStr)
            {
                case TypeWorkflowService:
                    resourceTypes = new[] { ResourceType.WorkflowService };
                    break;

                case TypeService:
                    resourceTypes = new[] { ResourceType.DbService, ResourceType.PluginService, ResourceType.WebService };
                    break;

                case TypeSource:
                    resourceTypes = new[] { ResourceType.Server, ResourceType.DbSource, ResourceType.PluginSource, ResourceType.WebSource, ResourceType.EmailSource, ResourceType.ServerSource };
                    break;

                case TypeReservedService:
                    resourceTypes = new[] { ResourceType.ReservedService };
                    break;

                default: // "*"
                    if(returnAllWhenNoMatch)
                    {
                        var values = Enum.GetValues(typeof(ResourceType));
                        resourceTypes = new ResourceType[values.Length];
                        for(var i = 0; i < values.Length; i++)
                        {
                            resourceTypes[i] = (ResourceType)values.GetValue(i);
                        }
                    }
                    else
                    {
                        resourceTypes = new ResourceType[0];
                    }
                    break;
            }
            return resourceTypes;
        }

        #endregion

        #region ToTypeString

        /// <summary>
        /// Converts the given resource type to a studio resource type string (WorkflowService, Service, Source, ReservedService, or *).
        /// </summary>
        /// <param name="resourceType">The type of the resource to be converted.</param>
        /// <returns>A studio resource type string.</returns>
        public static string ToTypeString(ResourceType resourceType)
        {
            switch(resourceType)
            {
                case ResourceType.Server:
                case ResourceType.DbSource:
                case ResourceType.PluginSource:
                case ResourceType.EmailSource:
                case ResourceType.WebSource:
                case ResourceType.ServerSource:
                case ResourceType.OauthSource:
                    return TypeSource;

                case ResourceType.PluginService:
                case ResourceType.DbService:
                case ResourceType.WebService:
                    return TypeService;

                case ResourceType.WorkflowService:
                    return TypeWorkflowService;

                case ResourceType.ReservedService:
                    return TypeReservedService;
                default:
                    return TypeWildcard;
            }
        }

        #endregion

        #region ToResourceType

        public static ResourceType ToResourceType(enSourceType sourceType)
        {
            var resourceType = ResourceType.Unknown;
            switch(sourceType)
            {
                case enSourceType.SqlDatabase:
                case enSourceType.MySqlDatabase:
                    resourceType = ResourceType.DbSource;
                    break;

                case enSourceType.Plugin:
                    resourceType = ResourceType.PluginSource;
                    break;

                case enSourceType.Dev2Server:
                    resourceType = ResourceType.Server;
                    break;

                case enSourceType.EmailSource:
                    resourceType = ResourceType.EmailSource;
                    break;

                case enSourceType.WebSource:
                    resourceType = ResourceType.WebSource;
                    break;

                case enSourceType.WebService:
                    resourceType = ResourceType.WebService;
                    break;
                case enSourceType.OauthSource:
                    resourceType = ResourceType.OauthSource;
                    break;
            }
            return resourceType;
        }

        #endregion

    }
}
