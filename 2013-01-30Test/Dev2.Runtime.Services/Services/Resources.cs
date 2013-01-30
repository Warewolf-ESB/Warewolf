using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime.Collections;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Dev2.Runtime.Services
{
    public class Resources : ExceptionManager
    {
        #region RootFolders/Elements

        internal static volatile Dictionary<enSourceType, string> RootFolders = new Dictionary<enSourceType, string>
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

        #region Paths

        public string Paths(string args, Guid workspaceID, Guid dataListID)
        {
            var result = new SortedSet<string>(new CaseInsensitiveStringComparer());

            IterateFiles("<Category>", "</Category>", workspaceID, (category, content) =>
            {
                result.Add(category);
                return true;
            });

            return JsonConvert.SerializeObject(result);
        }

        #endregion

        #region Read

        public static string Read(Guid workspaceID, enSourceType sourceType, string resourceID)
        {
            var result = string.Empty;

            IterateFiles(new[] { RootFolders[sourceType] }, " ID=\"", "\" ", workspaceID, (id, content) =>
            {
                if(resourceID.Equals(id, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = content;
                    return false;
                }
                return true;
            });
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

            var versionDirectory = string.Format("{0}\\{1}", directoryName, "VersionControl");
            if(!Directory.Exists(versionDirectory))
            {
                Directory.CreateDirectory(versionDirectory);
            }

            var fileName = string.Format("{0}\\{1}.xml", directoryName, resourceName);

            if(File.Exists(fileName))
            {
                var count = Directory.GetFiles(versionDirectory, string.Format("{0}*.xml", resourceName)).Count();

                File.Copy(fileName, string.Format("{0}\\{1}.V{2}.xml", versionDirectory, resourceName, (count + 1).ToString(CultureInfo.InvariantCulture)), true);
            }

            var signedXml = HostSecurityProvider.Instance.SignXml(resourceXml);
            File.WriteAllText(fileName, signedXml, Encoding.UTF8);
        }

        #endregion


        #region IterateFiles

        static void IterateFiles(string tokenStart, string tokenEnd, Guid workspaceID, Func<string, string, bool> action)
        {
            IterateFiles(RootFolders.Values.Distinct(), tokenStart, tokenEnd, workspaceID, action);
        }

        static void IterateFiles(IEnumerable<string> folders, string tokenStart, string tokenEnd, Guid workspaceID, Func<string, string, bool> action)
        {
            var startTokenLength = tokenStart.Length;

            var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);
            foreach(var path in folders.Select(folder => Path.Combine(workspacePath, folder)))
            {
                var files = Directory.GetFiles(path, "*.xml");
                foreach(var file in files)
                {
                    // XML parsing will add overhead - so just read file and use string ops instead
                    var content = File.ReadAllText(file);
                    var startIdx = content.IndexOf(tokenStart, StringComparison.InvariantCultureIgnoreCase);
                    if(startIdx == -1)
                    {
                        continue;
                    }
                    startIdx += startTokenLength;
                    var endIdx = content.IndexOf(tokenEnd, startIdx, StringComparison.InvariantCultureIgnoreCase);
                    var length = endIdx - startIdx;
                    if(length > 0)
                    {
                        var tokenValue = content.Substring(startIdx, length);
                        if(!action(tokenValue, content))
                        {
                            return;
                        }
                    }
                }
            }
        }

        #endregion

    }
}