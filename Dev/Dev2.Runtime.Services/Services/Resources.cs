using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime.Collections;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Security;
using Dev2.Runtime.Services.Data;
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

        #region PathsAndNames

        public string PathsAndNames(string args, Guid workspaceID, Guid dataListID)
        {
            var sourceType = (enSourceType)Enum.Parse(typeof(enSourceType), args);

            var paths = new SortedSet<string>(new CaseInsensitiveStringComparer());
            var names = new SortedSet<string>(new CaseInsensitiveStringComparer());
            if(sourceType == enSourceType.Dev2Server)
            {
                names.Add("localhost"); // auto-added to studio on startup
            }
            IterateFiles(new[] { RootFolders[sourceType] }, workspaceID, (delimiter, token, content) =>
            {
                switch(delimiter.ID)
                {
                    case 1:
                        names.Add(token);
                        break;
                    case 2:
                        paths.Add(token);
                        break;
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

        #endregion

        #region Paths

        public string Paths(string args, Guid workspaceID, Guid dataListID)
        {
            var result = new SortedSet<string>(new CaseInsensitiveStringComparer());

            IterateFiles(workspaceID, (delimiter, category, content) =>
            {
                result.Add(category);
                return true;
            }, new Delimiter
            {
                Start = "<Category>",
                End = "</Category>"
            });

            return JsonConvert.SerializeObject(result);
        }

        #endregion

        #region Read

        public static string Read(Guid workspaceID, enSourceType sourceType, string resourceID)
        {
            var result = string.Empty;

            IterateFiles(new[] { RootFolders[sourceType] }, workspaceID, (delimiter, id, content) =>
            {
                if(resourceID.Equals(id, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = content;
                    return false;
                }
                return true;
            }, new Delimiter
            {
                Start = " ID=\"",
                End = "\" "
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

        static void IterateFiles(Guid workspaceID, Func<Delimiter, string, string, bool> action, params Delimiter[] delimiters)
        {
            IterateFiles(RootFolders.Values.Distinct(), workspaceID, action, delimiters);
        }

        static void IterateFiles(IEnumerable<string> folders, Guid workspaceID, Func<Delimiter, string, string, bool> action, params Delimiter[] delimiters)
        {
            if(delimiters == null || delimiters.Length == 0 || action == null || folders == null)
            {
                return;
            }

            var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);
            foreach(var path in folders.Select(folder => Path.Combine(workspacePath, folder)))
            {
                var files = Directory.GetFiles(path, "*.xml");
                foreach(var file in files)
                {
                    // XML parsing will add overhead - so just read file and use string ops instead
                    var content = File.ReadAllText(file);
                    foreach(var delimiter in delimiters)
                    {
                        var startTokenLength = delimiter.Start.Length;
                        var startIdx = content.IndexOf(delimiter.Start, StringComparison.InvariantCultureIgnoreCase);
                        if(startIdx == -1)
                        {
                            continue;
                        }
                        startIdx += startTokenLength;
                        var endIdx = content.IndexOf(delimiter.End, startIdx, StringComparison.InvariantCultureIgnoreCase);
                        var length = endIdx - startIdx;
                        if(length > 0)
                        {
                            var tokenValue = content.Substring(startIdx, length);
                            if(!action(delimiter, tokenValue, content))
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        #endregion

    }
}