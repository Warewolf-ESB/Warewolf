using Dev2.Common.ServiceModel;
using System;

namespace Dev2.Runtime.Hosting
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
                    resourceTypes = new[] { ResourceType.DbService, ResourceType.PluginService };
                    break;

                case TypeSource:
                    resourceTypes = new[] { ResourceType.DbSource, ResourceType.PluginSource, ResourceType.Server };
                    break;

                case TypeReservedService:
                    resourceTypes = new[] { ResourceType.ReservedService };
                    break;

                default: // "*"
                    if (returnAllWhenNoMatch)
                    {
                        var values = Enum.GetValues(typeof(ResourceType));
                        resourceTypes = new ResourceType[values.Length];
                        for (var i = 0; i < values.Length; i++)
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
                    return TypeSource;

                case ResourceType.WorkflowService:
                    return TypeWorkflowService;

                case ResourceType.PluginService:
                case ResourceType.DbService:
                    return TypeService;

                case ResourceType.ReservedService:
                    return TypeReservedService;

                default:
                    return TypeWildcard;
            }
        }

        #endregion

    }
}