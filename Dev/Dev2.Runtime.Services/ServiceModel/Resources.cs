using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime.Collections;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dev2.Runtime.ServiceModel
{
    public class Resources : ExceptionManager
    {
        #region Static RootFolders/Elements

        public static volatile Dictionary<enSourceType, string> RootFolders = new Dictionary<enSourceType, string>
        {
            { enSourceType.SqlDatabase, "Sources" },
            { enSourceType.MySqlDatabase, "Sources" },
            { enSourceType.WebService, "Sources" },
            { enSourceType.DynamicService, "Services" },
            { enSourceType.ManagementDynamicService, "Services" },
            { enSourceType.Plugin, "Plugins" },
            { enSourceType.Unknown, "Sources" },
            { enSourceType.Dev2Server, "Sources" }
        };

        internal static volatile Dictionary<enSourceType, string> RootElements = new Dictionary<enSourceType, string>
        {
            { enSourceType.SqlDatabase, "Source" },
            { enSourceType.MySqlDatabase, "Source" },
            { enSourceType.WebService, "Source" },
            { enSourceType.DynamicService, "Service" },
            { enSourceType.ManagementDynamicService, "Service" },
            { enSourceType.Plugin, "Plugin" },
            { enSourceType.Unknown, "Source" },
            { enSourceType.Dev2Server, "Source" }
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
                var sourceType = (enSourceType)Enum.Parse(typeof(enSourceType), args);

                var paths = new SortedSet<string>(new CaseInsensitiveStringComparer());
                var names = new SortedSet<string>(new CaseInsensitiveStringComparer());
                if(sourceType == enSourceType.Dev2Server)
                {
                    names.Add("localhost"); // auto-added to studio on startup
                }
                ResourceIterator.Iterate(new[] { RootFolders[sourceType] }, workspaceID, iteratorResult =>
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

        public static ResourceList Read(Guid workspaceID, enSourceType resourceType)
        {
            var resources = new ResourceList();
            var resourceTypeStr = resourceType.ToString();

            ResourceIterator.Iterate(new[] { RootFolders[resourceType] }, workspaceID, iteratorResult =>
            {
                string value;
                if(iteratorResult.Values.TryGetValue(1, out value) && value.Equals(resourceTypeStr, StringComparison.InvariantCultureIgnoreCase))
                {
                    // older resources may not have an ID yet!!
                    iteratorResult.Values.TryGetValue(2, out value);
                    Guid resourceID;
                    Guid.TryParse(value, out resourceID);

                    resources.Add(new Resource
                    {
                        ResourceID = resourceID,
                        ResourceName = iteratorResult.Values[3],
                        ResourceType = resourceType
                    });
                }
                return true;
            }, new Delimiter
            {
                ID = 1,
                Start = " Type=\"",
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
            });
            return resources;
        }


        #endregion

        #region ReadXml

        public static string ReadXml(Guid workspaceID, enSourceType resourceType, string resourceID)
        {
            var result = String.Empty;

            ResourceIterator.Iterate(new[] { RootFolders[resourceType] }, workspaceID, iteratorResult =>
            {
                string value;
                if(iteratorResult.Values.TryGetValue(1, out value) && resourceID.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = iteratorResult.Content;
                    return false;
                }
                return true;
            }, new Delimiter { ID = 1, Start = " ID=\"", End = "\" " });
            return result;
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
            }

            var signedXml = HostSecurityProvider.Instance.SignXml(resourceXml);
            File.WriteAllText(fileName, signedXml, Encoding.UTF8);
        }

        #endregion

        #region ParseResourceType

        internal static enSourceType ParseResourceType(string resourceTypeStr)
        {
            enSourceType resourceType;
            if(!Enum.TryParse(resourceTypeStr, out resourceType))
            {
                resourceType = enSourceType.SqlDatabase;
            }
            return resourceType;
        }

        #endregion
    }
}