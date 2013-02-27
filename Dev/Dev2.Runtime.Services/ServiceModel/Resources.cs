using Dev2.Common;
using Dev2.Common.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Collections;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Dev2.Runtime.ServiceModel
{
    public class Resources : ExceptionManager
    {
        #region Static RootFolders/Elements

        public static volatile Dictionary<ResourceType, string> RootFolders = new Dictionary<ResourceType, string>
        {
            { ResourceType.Unknown, "Services" },
            { ResourceType.DbSource, "Sources" },
            { ResourceType.DbService, "Services" },
            { ResourceType.Plugin, "Plugins" },
            { ResourceType.Server, "Sources" }
        };

        internal static volatile Dictionary<ResourceType, string> RootElements = new Dictionary<ResourceType, string>
        {
            { ResourceType.Unknown, "Service" },
            { ResourceType.DbSource, "Source" },
            { ResourceType.DbService, "Service" },
            { ResourceType.Plugin, "Plugin" },
            { ResourceType.Server, "Source" }
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
                ResourceIterator.Iterate(new[] { RootFolders[resourceType] }, workspaceID, iteratorResult =>
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
                }, new Delimiter
                {
                    ID = 1,
                    Start = " Name=\"",
                    End = "\" "
                }, new Delimiter
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

            ResourceIterator.Iterate(RootFolders.Values.Distinct(), workspaceID, iteratorResult =>
            {
                string value;
                if(iteratorResult.Values.TryGetValue(1, out value))
                {
                    result.Add(value);
                }
                return true;
            }, new Delimiter
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

            ResourceIterator.Iterate(new[] { RootFolders[resourceType] }, workspaceID, iteratorResult =>
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
                                isResourceType = resourceType == ResourceType.Plugin;
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
            }, new Delimiter
            {
                ID = 1,
                Start = " ResourceType=\"",
                End = "\" "
            }, new Delimiter
            {
                ID = 2,
                Start = " ID=\"",
                End = "\" "
            }, new Delimiter
            {
                ID = 3,
                Start = " Name=\"",
                End = "\" "
            }, new Delimiter
            {
                ID = 4,
                Start = "<Category>",
                End = "</Category>"
            }, new Delimiter
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

            ResourceIterator.Iterate(new[] { directoryName }, workspaceID, iteratorResult =>
            {
                string value;
                if(iteratorResult.Values.TryGetValue(1, out value) && resourceID.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = iteratorResult.Content;
                    return false;
                }
                return true;
            }, new Delimiter { ID = 1, Start = delimiterStart, End = "\" " });
            return result;
        }

        #endregion

        #region ReadResource

        static Resource ReadResource(Guid resourceID, ResourceType resourceType, string resourceName, string resourcePath, string content)
        {
            Delimiter delimiter;
            string delimiterValue;

            switch(resourceType)
            {
                case ResourceType.DbSource:
                    delimiter = new Delimiter { ID = 1, Start = " ConnectionString=\"", End = "\" " };
                    delimiter.TryGetValue(content, out delimiterValue);
                    return new DbSource
                    {
                        ResourceID = resourceID, 
                        ResourceType = resourceType, 
                        ResourceName = resourceName, 
                        ResourcePath = resourcePath, 
                        ConnectionString = delimiterValue
                    };
            }

            return new Resource { ResourceID = resourceID, ResourceType = resourceType, ResourceName = resourceName, ResourcePath = resourcePath };
        }

        #endregion


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
            directoryName = Path.Combine(workspacePath, directoryName);
            if(!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            var versionDirectory = String.Format("{0}\\{1}", directoryName, "VersionControl");
            if(!Directory.Exists(versionDirectory))
            {
                Directory.CreateDirectory(versionDirectory);
            }

            var fileName = String.Format("{0}\\{1}.xml", directoryName, resourceName);

            if(File.Exists(fileName))
            {
                var count = Directory.GetFiles(versionDirectory, String.Format("{0}*.xml", resourceName)).Count();

                File.Copy(fileName, String.Format("{0}\\{1}.V{2}.xml", versionDirectory, resourceName, (count + 1).ToString(CultureInfo.InvariantCulture)), true);

                // Remove readonly attribute if it is set
                FileAttributes attributes = File.GetAttributes(fileName);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(fileName, attributes ^ FileAttributes.ReadOnly);
                }
            }

            var signedXml = HostSecurityProvider.Instance.SignXml(resourceXml);
            File.WriteAllText(fileName, signedXml, Encoding.UTF8);
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