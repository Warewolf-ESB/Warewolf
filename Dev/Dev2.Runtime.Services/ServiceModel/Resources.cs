
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Data.Binary_Objects;
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
        /*
         * !!!!! Important. Donot cleanup resharper violations here. Accessed by webs. If you change names remove methods, eth then the webs break.
         * 
         */
        #region Static RootFolders/Elements

        public static volatile Dictionary<ResourceType, string> RootFolders = new Dictionary<ResourceType, string>
        {
            { ResourceType.Unknown, "Resources" },
            { ResourceType.Server, "Resources" },
            { ResourceType.DbService, "Resources" },
            { ResourceType.DbSource, "Resources" },
            { ResourceType.PluginService, "Resources" },
            { ResourceType.PluginSource, "Resources" },
            { ResourceType.EmailSource, "Resources" },
            { ResourceType.WebSource, "Resources" },
            { ResourceType.WebService, "Resources" },
            { ResourceType.WorkflowService, "Resources" },
        };

        #endregion

        #region Sources

        // POST: Service/Resources/Sources
        public ResourceList Sources(string args, Guid workspaceId, Guid dataListId)
        {
            var result = new ResourceList();
            try
            {
                dynamic argsObj = JObject.Parse(args);
                string resourceTypeStr = argsObj.resourceType.Value;
                result = Read(workspaceId, ParseResourceType(resourceTypeStr));
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }
        // POST: Service/Resources/Services
        public ResourceList Services(string args, Guid workspaceId, Guid dataListId)
        {
            var result = new ResourceList();
            try
            {
                var resourceType = ParseResourceType(args);
                if(resourceType == ResourceType.WorkflowService)
                {
                    result = Read(workspaceId, resourceType);
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
        public string PathsAndNames(string args, Guid workspaceId, Guid dataListId)
        {
            args = args.Replace("\\\\", "\\");
            args = args.Replace("root", "");
            var explorerItem = ServerExplorerRepository.Instance.Load(workspaceId);
            // ReSharper disable MaximumChainedReferences
            var folders = explorerItem.Descendants().Where(item => item.ResourceType == ResourceType.Folder
                                        && (args == "" ? item.ResourcePath == item.DisplayName : GetResourceParent(item.ResourcePath) == args))
                                            .Select(item => item.DisplayName);
            // ReSharper restore MaximumChainedReferences
            var paths = new SortedSet<string>(folders);
            // ReSharper disable MaximumChainedReferences
            var resources = explorerItem.Descendants().Where(item => (item.ResourceType > ResourceType.Unknown && item.ResourceType <= ResourceType.ServerSource)
                                        && (args == "" ? item.ResourcePath == item.DisplayName : GetResourceParent(item.ResourcePath) == args))
                                            .Select(item => item.DisplayName);
            // ReSharper restore MaximumChainedReferences
            var names = new SortedSet<string>(resources);
            var pathsAndNames = JsonConvert.SerializeObject(new PathsAndNamesTO { Names = names, Paths = paths });
            return pathsAndNames;
        }



        string GetResourceParent(string resourcePath)
        {
            return resourcePath.Contains("\\") ? resourcePath.Substring(0, resourcePath.LastIndexOf("\\", StringComparison.Ordinal)) : resourcePath;
        }

        //2013.08.26: Ashley Lewis for bug 10208 - Workflows are stored in the same folder but are distinct to the save dialog

        #endregion

        #region Paths

        // POST: Service/Resources/Paths
        public string Paths(string args, Guid workspaceId, Guid dataListId)
        {
            var result = new SortedSet<string>(new CaseInsensitiveStringComparer());

            ResourceIterator.Instance.IterateAll(workspaceId, iteratorResult =>
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

        public static ResourceList Read(Guid workspaceId, ResourceType resourceType)
        {
            var resources = new ResourceList();
            var explorerItem = ServerExplorerRepository.Instance.Load(resourceType, workspaceId);
            explorerItem.Descendants().ForEach(item =>
            {
                if(item.ResourceType == resourceType)
                {
                    resources.Add(new Resource { ResourceID = item.ResourceId, ResourceType = resourceType, ResourceName = item.DisplayName, ResourcePath = item.ResourcePath });
                }
            });
            return resources;
        }

        #endregion

        #region ReadXml

        public static string ReadXml(Guid workspaceId, ResourceType resourceType, string resourceId)
        {
            return ReadXml(workspaceId, "Resources", resourceId);
        }

        public static string ReadXml(Guid workspaceId, string directoryName, string resourceId)
        {
            var result = String.Empty;
            Guid id;
            var delimiterStart = Guid.TryParse(resourceId, out id) ? " ID=\"" : " Name=\"";

            ResourceIterator.Instance.Iterate(directoryName, workspaceId, iteratorResult =>
            {
                string value;
                if(iteratorResult.Values.TryGetValue(1, out value) && resourceId.Equals(value, StringComparison.InvariantCultureIgnoreCase))
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

        public string DataListInputVariables(string resourceId, Guid workspaceId, Guid dataListId)
        {
            Guid rsId;
            if(!Guid.TryParse(resourceId, out rsId))
            {
                RaiseError(new ArgumentException("Invalid ResouceID."));
                return "";
            }
            var resource = ResourceCatalog.Instance.GetResource(workspaceId, rsId);
            if(resource == null)
            {
                RaiseError(new ArgumentException("Workflow not found."));
                return "";
            }
            var services = ResourceCatalog.Instance.GetDynamicObjects(resource);

            var tmp = services.FirstOrDefault();
            var result = new StringBuilder("<DataList></DataList>");

            if(tmp != null)
            {
                result = tmp.DataListSpecification;
            }

            using(var sr = new StringReader(result.ToString()))
            {

                var dataListSpec = XElement.Load(sr);
                if(dataListSpec.HasElements)
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
                    enDev2ColumnArgumentDirection columnIoDirection;
                    if(Enum.TryParse(value, true, out columnIoDirection))
                    {
                        return columnIoDirection == enDev2ColumnArgumentDirection.Input || columnIoDirection == enDev2ColumnArgumentDirection.Both;
                    }
                }
            }
            return false;
        }
    }

// ReSharper disable InconsistentNaming
    public class PathsAndNamesTO
// ReSharper restore InconsistentNaming
    {
        public SortedSet<string> Names { get; set; }
        public SortedSet<string> Paths { get; set; }
    }

    public class DataListVariable
    {
        public string Name { get; set; }
    }

    public static class TreeEx
    {
        public static IEnumerable<IExplorerItem> Descendants(this IExplorerItem root)
        {
            var nodes = new Stack<IExplorerItem>(new[] { root });
            while(nodes.Any())
            {
                IExplorerItem node = nodes.Pop();
                yield return node;
                if(node.Children != null)
                {
                    foreach(var n in node.Children) nodes.Push(n);
                }
            }
        }
    }
}
