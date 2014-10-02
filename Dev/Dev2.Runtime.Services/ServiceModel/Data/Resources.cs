
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
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Collections;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dev2.Runtime.ServiceModel
{
    public class Resources : ExceptionManager
    {
        #region Static RootFolders/Elements

        public static volatile Dictionary<ResourceType, string> RootFolders = new Dictionary<ResourceType, string>
        {
            { ResourceType.Unknown, "Services" },
            { ResourceType.Server, "Sources" },
            { ResourceType.DbService, "Services" },
            { ResourceType.PluginService, "Sources" },
            { ResourceType.PluginSource, "Sources" },
            { ResourceType.DbSource, "Sources" }
        };

        internal static volatile Dictionary<ResourceType, string> RootElements = new Dictionary<ResourceType, string>
        {
            { ResourceType.Unknown, "Service" },
            { ResourceType.Server, "Source" },
            { ResourceType.DbService, "Service" },
            { ResourceType.PluginService, "Service" },
            { ResourceType.PluginSource, "Source" },
            { ResourceType.DbSource, "Source" }
        };

        #endregion

        #region Sources

        // POST: Service/Resources/Sources
        public ResourceList Sources(string args, Guid workspaceID, Guid dataListID)
        {
            var result = new ResourceList();
            try
            {
                dynamic argsObj = JObject.Parse(args);
                result = Read(workspaceID, ParseResourceType(argsObj.resourceType.Value));
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion

        #region PathsAndNames

        // POST: Service/Resources/PathsAndNames
        public string PathsAndNames(string args, Guid workspaceID, Guid dataListID)
        {
            if(!String.IsNullOrEmpty(args))
            {
                var resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), args);

                var paths = new SortedSet<string>(new CaseInsensitiveStringComparer());
                var names = new SortedSet<string>(new CaseInsensitiveStringComparer());
                if(resourceType == ResourceType.Server)
                {
                    names.Add("localhost"); // auto-added to studio on startup
                }
                ResourceIterator.Instance.Iterate(new[] { RootFolders[resourceType] }, workspaceID, iteratorResult =>
                {
                    string value;
                    if(iteratorResult.Values.TryGetValue(1, out value))
                    {
                        names.Add(value);
                    }
                    if(iteratorResult.Values.TryGetValue(2, out value))
                    {
                        paths.Add(value);
                    }
                    return true;
                }, new ResourceDelimiter
                {
                    ID = 1,
                    Start = " Name=\"",
                    End = "\" "
                }, new ResourceDelimiter
                {
                    ID = 2,
                    Start = "<Category>",
                    End = "</Category>"
                });

                return JsonConvert.SerializeObject(new { Names = names, Paths = paths });
            }
            return null;
        }

        #endregion

        #region Paths

        // POST: Service/Resources/Paths
        public string Paths(string args, Guid workspaceID, Guid dataListID)
        {
            var result = new SortedSet<string>(new CaseInsensitiveStringComparer());

            ResourceIterator.Instance.IterateAll(workspaceID, iteratorResult =>
            {
                string value;
                if(iteratorResult.Values.TryGetValue(1, out value))
                {
                    result.Add(value);
                }
                return true;
            }, new ResourceDelimiter
            {
                ID = 1,
                Start = "<Category>",
                End = "</Category>"
            });

            return JsonConvert.SerializeObject(result);
        }

        #endregion


        /////////////////////////////////////////////////////////////////
        // Static Helper methods
        /////////////////////////////////////////////////////////////////

        #region Read

        public static ResourceList Read(Guid workspaceID, ResourceType resourceType)
        {
            var resources = new ResourceList();
            var resourceTypeStr = resourceType.ToString();

            ResourceIterator.Instance.Iterate(new[] { RootFolders[resourceType] }, workspaceID, iteratorResult =>
            {
                var isResourceType = false;
                string value;

                if(iteratorResult.Values.TryGetValue(1, out value))
                {
                    // Check ResourceType attribute
                    isResourceType = value.Equals(resourceTypeStr, StringComparison.InvariantCultureIgnoreCase);
                }
                else if(iteratorResult.Values.TryGetValue(5, out value))
                {
                    // This is here for legacy XML!
                    #region Check Type attribute

                    enSourceType sourceType;
                    if(iteratorResult.Values.TryGetValue(5, out value) && Enum.TryParse(value, out sourceType))
                    {
                        switch(sourceType)
                        {
                            case enSourceType.SqlDatabase:
                            case enSourceType.MySqlDatabase:
                                isResourceType = resourceType == ResourceType.DbSource;
                                break;
                            case enSourceType.WebService:
                                break;
                            case enSourceType.DynamicService:
                                isResourceType = resourceType == ResourceType.DbService;
                                break;
                            case enSourceType.Plugin:
                                isResourceType = resourceType == ResourceType.PluginService || resourceType == ResourceType.PluginSource;
                                break;
                            case enSourceType.Dev2Server:
                                isResourceType = resourceType == ResourceType.Server;
                                break;
                        }
                    }

                    #endregion
                }
                if(isResourceType)
                {
                    // older resources may not have an ID yet!!
                    iteratorResult.Values.TryGetValue(2, out value);
                    Guid resourceID;
                    Guid.TryParse(value, out resourceID);

                    string resourceName;
                    iteratorResult.Values.TryGetValue(3, out resourceName);
                    string resourcePath;
                    iteratorResult.Values.TryGetValue(4, out resourcePath);
                    resources.Add(ReadResource(resourceID, resourceType, resourceName, resourcePath, iteratorResult.Content));
                }
                return true;
            }, new ResourceDelimiter
            {
                ID = 1,
                Start = " ResourceType=\"",
                End = "\" "
            }, new ResourceDelimiter
            {
                ID = 2,
                Start = " ID=\"",
                End = "\" "
            }, new ResourceDelimiter
            {
                ID = 3,
                Start = " Name=\"",
                End = "\" "
            }, new ResourceDelimiter
            {
                ID = 4,
                Start = "<Category>",
                End = "</Category>"
            }, new ResourceDelimiter
            {
                ID = 5,
                Start = " Type=\"",
                End = "\" "
            });
            return resources;
        }

        #endregion

        #region ReadXml

        public static string ReadXml(Guid workspaceID, ResourceType resourceType, string resourceID)
        {
            return ReadXml(workspaceID, RootFolders[resourceType], resourceID);
        }

        public static string ReadXml(Guid workspaceID, string directoryName, string resourceID)
        {
            var result = String.Empty;
            Guid id;
            var delimiterStart = Guid.TryParse(resourceID, out id) ? " ID=\"" : " Name=\"";

            ResourceIterator.Instance.Iterate(new[] { directoryName }, workspaceID, iteratorResult =>
            {
                string value;
                if(iteratorResult.Values.TryGetValue(1, out value) && resourceID.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = iteratorResult.Content;
                    return false;
                }
                return true;
            }, new ResourceDelimiter { ID = 1, Start = delimiterStart, End = "\" " });
            return result;
        }

        #endregion

        #region ReadResource

        public static Resource ReadResource(Guid resourceID, ResourceType resourceType, string resourceName, string resourcePath, string content)
        {
            ResourceDelimiter delimiter;
            string delimiterValue;

            switch(resourceType)
            {
                case ResourceType.DbSource:
                    delimiter = new ResourceDelimiter { ID = 1, Start = " ConnectionString=\"", End = "\" " };
                    delimiter.TryGetValue(content, out delimiterValue);
                    return new DbSource { ResourceID = resourceID, ResourceType = resourceType, ResourceName = resourceName, ResourcePath = resourcePath, ConnectionString = delimiterValue };

                case ResourceType.PluginSource:
                    string assemblyLocation;
                    string assemblyName;
                    delimiter = new ResourceDelimiter { ID = 1, Start = " AssemblyLocation=\"", End = "\" " };
                    delimiter.TryGetValue(content, out assemblyLocation);
                    delimiter = new ResourceDelimiter { ID = 1, Start = " AssemblyName=\"", End = "\" " };
                    delimiter.TryGetValue(content, out assemblyName);
                    return new PluginSource { ResourceID = resourceID, ResourceType = resourceType, ResourceName = resourceName, ResourcePath = resourcePath, AssemblyLocation = assemblyLocation, AssemblyName = assemblyName };
            }

            return new Resource { ResourceID = resourceID, ResourceType = resourceType, ResourceName = resourceName, ResourcePath = resourcePath };
        }

        #endregion

        // TODO: Remove save and use ResourceCatalog directly
        #region Save

        public static void Save(Guid workspaceID, string directoryName, DynamicServiceObjectBase resource)
        {
            Save(workspaceID, directoryName, resource.Name, resource.ResourceDefinition);
        }

        public static void Save(string workspacePath, string directoryName, DynamicServiceObjectBase resource)
        {
            Save(workspacePath, directoryName, resource.Name, resource.ResourceDefinition);
        }

        public static void Save(Guid workspaceID, string directoryName, string resourceName, string resourceXml)
        {
            var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);
            Save(workspacePath, directoryName, resourceName, resourceXml);
        }

        public static void Save(string workspacePath, string directoryName, string resourceName, string resourceXml)
        {
            ResourceCatalog.Save(workspacePath, directoryName, resourceName, resourceXml);
        }

        #endregion

        #region ParseResourceType

        internal static ResourceType ParseResourceType(string resourceTypeStr)
        {
            ResourceType resourceType;
            if(!Enum.TryParse(resourceTypeStr, out resourceType))
            {
                resourceType = ResourceType.Unknown;
            }
            return resourceType;
        }

        #endregion
    }
}
