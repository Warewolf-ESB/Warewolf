using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dev2.Data.Binary_Objects;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Collections;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceStack.Common.Extensions;

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
            { ResourceType.DbSource, "Sources" },
            { ResourceType.PluginService, "Services" },
            { ResourceType.PluginSource, "Sources" },
            { ResourceType.EmailSource, "Sources" },
            { ResourceType.WebSource, "Sources" },
            { ResourceType.WebService, "Services" },
            { ResourceType.WorkflowService, "Services" },
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
                string resourceTypeStr = argsObj.resourceType.Value;
                result = Read(workspaceID, ParseResourceType(resourceTypeStr));
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }
        // POST: Service/Resources/Services
        public ResourceList Services(string args, Guid workspaceID, Guid dataListID)
        {
            var result = new ResourceList();
            try
            {
                var resourceType = ParseResourceType(args);
                if(resourceType == ResourceType.WorkflowService)
                {
                    result = Read(workspaceID, resourceType);
                }
                else
                {
                    throw new ArgumentException("Resource Type passed not WorkflowService");
                }
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
            var getSearchPath = GetWebsSensitiveServiceType((ResourceType) Enum.Parse(typeof (ResourceType), args));

            var paths = new SortedSet<string>(new CaseInsensitiveStringComparer());
            var names = new SortedSet<string>(new CaseInsensitiveStringComparer());

            if (!String.IsNullOrEmpty(args))
            {
                ResourceIterator.Instance.Iterate(RootFolders.Select(c => c.Value).Distinct().ToArray() , workspaceID, iteratorResult =>
                {
                    string resourceType;
                    if (iteratorResult.Values.TryGetValue(3, out resourceType))
                    {
                        string name;
                        if (iteratorResult.Values.TryGetValue(1, out name))
                        {
                            names.Add(name);
                        }
                        string category;
                        if (iteratorResult.Values.TryGetValue(2, out category))
                        {
                            if (GetWebsSensitiveServiceType((ResourceType)Enum.Parse(typeof(ResourceType), resourceType)) == getSearchPath)
                            {
                                //2013.05.20: Ashley Lewis for PBI 8858 - studio paths are in upper case in the explorer
                                paths.Add(category.ToUpper());
                            }
                        }
                    }
                    return true;
                }, new ResourceDelimiter
                {
                    ID = 1,
                    Start = " Name=\"",
                    End = "\""
                }, new ResourceDelimiter
                {
                    ID = 2,
                    Start = "<Category>",
                    End = "</Category>"
                }, new ResourceDelimiter
                {
                    ID = 3,
                    Start = "ResourceType=\"",
                    End = "\""
                });
            }
            var pathsAndNames = JsonConvert.SerializeObject(new PathsAndNamesTO { Names = names, Paths = paths });
            return pathsAndNames;
        }

        //2013.08.26: Ashley Lewis for bug 10208 - Workflows are stored in the same folder but are distinct to the save dialog
        string GetWebsSensitiveServiceType(ResourceType type)
        {
            string gottenPath;
            if(type != ResourceType.WorkflowService)
            {
                gottenPath = RootFolders[type];
            }
            else
            {
                gottenPath = "Workflow";
            }
            return gottenPath;
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

        private static readonly object o = new object();

        public static ResourceList Read(Guid workspaceID, ResourceType resourceType)
        {
            var resources = new ResourceList();
            var resourceTypeStr = resourceType.ToString();

            lock(o)
            {


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
                                    isResourceType = resourceType == ResourceType.PluginService;
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
                    End = "\""
                }, new ResourceDelimiter
                {
                    ID = 2,
                    Start = " ID=\"",
                    End = "\""
                }, new ResourceDelimiter
                {
                    ID = 3,
                    Start = " Name=\"",
                    End = "\""
                }, new ResourceDelimiter
                {
                    ID = 4,
                    Start = "<Category>",
                    End = "</Category>"
                }, new ResourceDelimiter
                {
                    ID = 5,
                    Start = " Type=\"",
                    End = "\""
                });
            }
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
            }, new ResourceDelimiter { ID = 1, Start = delimiterStart, End = "\"" });
            return result;
        }

        #endregion

        #region ReadResource

        static Resource ReadResource(Guid resourceID, ResourceType resourceType, string resourceName, string resourcePath, string content)
        {
            ResourceDelimiter delimiter;

            switch(resourceType)
            {
                case ResourceType.DbSource:
                    delimiter = new ResourceDelimiter { ID = 1, Start = " ConnectionString=\"", End = "\"" };
                    string delimiterValue;
                    delimiter.TryGetValue(content, out delimiterValue);
                    return new DbSource
                    {
                        ResourceID = resourceID,
                        ResourceType = resourceType,
                        ResourceName = resourceName,
                        ResourcePath = resourcePath,
                        ConnectionString = delimiterValue
                    };
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

        public string DataListInputVariables(string resourceID, Guid workspaceID, Guid dataListID)
        {
            Guid rsID;
            if(!Guid.TryParse(resourceID, out rsID))
            {
                RaiseError(new ArgumentException("Invalid ResouceID."));
                return "";
            }
            var resource = ResourceCatalog.Instance.GetResource(workspaceID, rsID);
            if(resource == null)
            {
                RaiseError(new ArgumentException("Workflow not found."));
                return "";
            }
            var services = ResourceCatalog.Instance.GetDynamicObjects(resource);

            var tmp = services.FirstOrDefault();
            var result = "<DataList></DataList>";

            if(tmp != null)
            {
                result = tmp.DataListSpecification;
            }

            using (var sr = new StringReader(result))
            {

                var dataListSpec = XElement.Load(sr);
                if (dataListSpec.HasElements)
                {
                    var validElements = new List<DataListVariable>();
                    var xElements = dataListSpec.Elements();
                    xElements.ForEach(element => GetInputElements(element, validElements));
                    var dataListInputVariables = JsonConvert.SerializeObject(validElements);
                    return dataListInputVariables;
                }

                return string.Empty;
            }
        }

        static void GetInputElements(XElement element, List<DataListVariable> validElements)
        {
            if(IsInputElement(element) && !element.HasElements)
            {
                validElements.Add(new DataListVariable { Name = element.Name.LocalName });
            }
        }

        static bool IsInputElement(XElement element)
        {
            if(element.HasAttributes)
            {
                XAttribute xAttribute = element.Attribute("ColumnIODirection");
                if(xAttribute != null)
                {
                    var value = xAttribute.Value;
                    enDev2ColumnArgumentDirection columnIODirection;
                    if(Enum.TryParse(value, true, out columnIODirection))
                    {
                        return columnIODirection == enDev2ColumnArgumentDirection.Input || columnIODirection == enDev2ColumnArgumentDirection.Both;
                    }
                }
            }
            return false;
        }
    }

    public class PathsAndNamesTO
    {
        public SortedSet<string> Names { get; set; }
        public SortedSet<string> Paths { get; set; }
    }

    public class DataListVariable
    {
        public string Name { get; set; }
    }
}