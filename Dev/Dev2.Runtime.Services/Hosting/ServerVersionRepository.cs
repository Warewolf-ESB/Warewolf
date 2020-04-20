#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Explorer;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using ServiceStack.Common.Extensions;
using Warewolf.Data;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.Hosting
{
    public class ServerVersionRepository : IServerVersionRepository
    {
        readonly IVersionStrategy _versionStrategy;
        readonly IResourceCatalog _catalogue;
        readonly IDirectory _directory;
        readonly IFile _file;
        readonly IFilePath _filePath;
        protected static readonly object LockObject = new object();
        readonly string _rootPath;
        readonly string _envVersionFolder;
        readonly string _resourcePath;

        public ServerVersionRepository(IVersionStrategy versionStrategy, IResourceCatalog catalogue, IDirectory directory, string rootPath, IFile file, IFilePath filePath)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>
            {          {"versionStrategy", versionStrategy},
                    {"catalogue", catalogue},
                    {"directory", directory},
                    {"rootPath", rootPath},
                    {"file", file}
            }
            );
            _versionStrategy = versionStrategy;
            _catalogue = catalogue;
            _directory = directory;
            _rootPath = rootPath;
            _file = file;
            _filePath = filePath;
            _envVersionFolder = EnvironmentVariables.VersionsPath;
            _resourcePath = EnvironmentVariables.ResourcePath;
        }

        public IList<IExplorerItem> GetVersions(Guid resourceId)
        {
            var resource = _catalogue.GetResource(Guid.Empty, resourceId);
            if (resource?.VersionInfo == null)
            {
                return new List<IExplorerItem>();
            }
            var versionFolder = _filePath.Combine(_envVersionFolder, resourceId.ToString());
            var files = _directory.GetFiles(versionFolder);
            return files.Select(a => CreateVersionFromFilePath(a, resource, _envVersionFolder))
                        .OrderByDescending(a => a.VersionInfo.DateTimeStamp)
                        .Take(GlobalConstants.VersionCount)
                        .ToList();
        }
        string GetVersionFolderFromResource(string resourcePath)
        {
            var path = _filePath.Combine(_rootPath, GetDirectoryFromResource(resourcePath), GlobalConstants.VersionFolder);
            _directory.CreateIfNotExists(path);

            return path;
        }

        string GetFolderFromResource(string resourcePath)
        {
            var path = _filePath.Combine(_rootPath, GetDirectoryFromResource(resourcePath));
            _directory.CreateIfNotExists(path);

            return path;
        }



        public void MoveVersions(Guid resourceId, string newPath, string resourcePath)
        {
            var resource = _catalogue.GetResource(Guid.Empty, resourceId);

            if (resource?.VersionInfo == null)
            {
                return;
            }

            var files = _directory.GetFiles(_envVersionFolder).Where(a => a.Contains(resource.VersionInfo.VersionId.ToString()));
            IEnumerable<string> enumerable = files as IList<string> ?? files.ToList();

            enumerable.ForEach(a => _file.Move(a, _filePath.Combine(_envVersionFolder, _filePath.GetFileName(a))));
        }

        public StringBuilder GetVersion(IVersionInfo version, string resourcePath)
        {
            var versionFolder = _filePath.Combine(_envVersionFolder, version.ResourceId.ToString());
            var v = _directory.GetFiles(versionFolder)
                .FirstOrDefault(p => _filePath.GetFileName(p)
                .StartsWith(version.VersionNumber, StringComparison.Ordinal));
            if (string.IsNullOrEmpty(v))
            {
                throw new VersionNotFoundException("Version Does not exist");
            }

            return new StringBuilder(_file.ReadAllText(v));
        }

        static IExplorerItem CreateVersionFromFilePath(string path, IResource resource, string resourcePath) => new ServerExplorerItem(CreateNameFromPath(path), resource.ResourceID, "Version", new List<IExplorerItem>(), Permissions.View, resourcePath)
        {
            VersionInfo = CreateVersionInfoFromFilePath(path, resource.ResourceID),
            IsResourceVersion = true
        };

        static IVersionInfo CreateVersionInfoFromFilePath(string path, Guid resourceId)
        {
            var name = new FileInfo(path).Name;
            var parts = name.Split('_');
            if (parts.Length != 3)
            {
                throw new Exception(ErrorResource.InvalidVersion);
            }

            return new VersionInfo(new DateTime(long.Parse(parts[1])), parts[2], "", parts[0], resourceId, resourceId);
        }

        static string CreateNameFromPath(string path)
        {
            var name = new FileInfo(path).Name;
            var parts = name.Split('_');
            if (parts.Length != 3)
            {
                throw new Exception(ErrorResource.InvalidVersion);
            }

            return $"v.{parts[0]}  {new DateTime(long.Parse(parts[1]))}  {parts[2].Replace(".xml", "")}";
        }

        static string GetDirectoryFromResource(string resourcePath)
        {
            if (resourcePath.Contains("\\"))
            {
                return resourcePath.Substring(0, resourcePath.LastIndexOf('\\'));
            }

            return "";
        }

        public IRollbackResult RollbackTo(Guid resourceId, string versionNumber)
        {
            var res = _catalogue.GetResource(Guid.Empty, resourceId);
            var resourcePath = res.GetResourcePath(Guid.Empty);
            var xmlBuilder = GetVersion(new VersionInfo(DateTime.MinValue, "", "", versionNumber, resourceId, res.VersionInfo.VersionId), resourcePath);
            var xml = xmlBuilder.ToXElement();
            var oldResource = new Resource(xml);
            StoreAndDeleteCurrentIfRenamed(res, oldResource, resourcePath);
            UpdateVersionInfoIfNotExists(resourceId, xml, res);
            var savePath = res.GetSavePath();
            _catalogue.SaveResource(Guid.Empty, xml.ToStringBuilder(), savePath, "Rollback", "WorkflowService");
            if (oldResource.ResourceName != res.ResourceName)
            {
                _catalogue.GetResource(Guid.Empty, res.ResourceID).ResourceName = oldResource.ResourceName;
            }

            return new RollbackResult { DisplayName = oldResource.ResourceName, VersionHistory = GetVersions(resourceId) };
        }

        static void UpdateVersionInfoIfNotExists(Guid resourceId, XElement xml, IResource res)
        {
            var versionInfo = xml.Elements("VersionInfo").FirstOrDefault();
            if (versionInfo != null)
            {
                versionInfo.SetAttributeValue("DateTimeStamp", DateTime.Now);
                versionInfo.SetAttributeValue("Reason", "Rolback");
                versionInfo.SetAttributeValue("User", res.VersionInfo.User);
                versionInfo.SetAttributeValue("VersionNumber", 1 + int.Parse(res.VersionInfo.VersionNumber));
                versionInfo.SetAttributeValue("ResourceId", resourceId);
                versionInfo.SetAttributeValue("VersionId", res.VersionInfo.VersionId);
            }
        }

        void StoreAndDeleteCurrentIfRenamed(IResource res, Resource oldResource, string resourcePath)
        {
            if (res.ResourceName != oldResource.ResourceName)
            {
                StoreVersion(res, "unknown", "Rollback", Guid.Empty, resourcePath);
                _catalogue.DeleteResource(Guid.Empty, res.ResourceName, res.ResourceType, false);
            }
        }

        public IList<IExplorerItem> DeleteVersion(Guid resourceId, string versionNumber, string resourcePath)
        {
            var resource = _catalogue.GetResource(Guid.Empty, resourceId);
            var versionFolder = GetVersionFolderPath(resource.ResourceID.ToString());
            var allVersions = _directory.GetFiles(versionFolder);
            var version = allVersions.FirstOrDefault(p => _filePath.GetFileName(p)
                .StartsWith(versionNumber, StringComparison.Ordinal));
            _file.Delete(version);
            if (_directory.GetFiles(versionFolder).Length < 1)
            {
                _directory.Delete(versionFolder, true);
            }
            return GetVersions(resourceId);
        }

        string GetVersionFolderPath(string resourceId)
        {
            var versionFolder = _filePath.Combine(_envVersionFolder, resourceId);
            return versionFolder;
        }
        public void StoreVersion(IResource resource, string userName, string reason, Guid workSpaceId, string resourcePath)
        {
            var versionFolder = GetVersionFolderPath(resource.ResourceID.ToString());
            var _userName = userName;
            if (workSpaceId == Guid.Empty)
            {
                if (string.IsNullOrEmpty(_userName))
                {
                    _userName = Thread.CurrentPrincipal.Identity.Name;
                }

                lock (LockObject)
                {
                   var old = _catalogue.GetResource(Guid.Empty, resource.ResourceID);
                    if (old == null)
                    { return; }
                    var versions = GetVersions(resource.ResourceID).FirstOrDefault();
                    old.VersionInfo = _versionStrategy.GetCurrentVersion(resource, versions?.VersionInfo, _userName, reason);

                    var fileName = $"{old.VersionInfo.VersionNumber}_{GetDateString(old.VersionInfo.DateTimeStamp)}_{reason}.bite";
                    if (!_file.Exists(_filePath.Combine(versionFolder, fileName)))
                    {
                        var sourceFile = _filePath.Combine(GetFolderFromResource(old.GetResourcePath(workSpaceId)), old.ResourceName) + ".bite";
                        if (_file.Exists(sourceFile))
                        {
                            _directory.CreateIfNotExists(versionFolder);
                            _file.Copy(sourceFile, _filePath.Combine(versionFolder, fileName));
                        }
                    }

                    resource.VersionInfo = _versionStrategy.GetNextVersion(resource, old, _userName, reason);

                }
            }
        }

        static string GetDateString(DateTime dateTimeStamp) => dateTimeStamp.Ticks.ToString(CultureInfo.InvariantCulture);

        public void CleanUpOldVersionControlStructure(IDirectory directory)
        {
            Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () => { PerformCleanUp(directory); });
        }

        void PerformCleanUp(IDirectory directory)
        {
            var resources = _catalogue.GetResources(GlobalConstants.ServerWorkspaceID).Where(p => !p.ResourceType.Equals("ReservedService"));

            foreach (var item in resources)
            {
                if (item?.VersionInfo == null)
                {
                    continue;
                }
                var versionPath = item.GetResourcePath(GlobalConstants.ServerWorkspaceID);
                var path = GetVersionFolderFromResource(versionPath);
                var files = _directory.GetFiles(path).Where(a => a.Contains(item.VersionInfo.VersionId.ToString()));
                var folderName = _filePath.Combine(_envVersionFolder, item.ResourceID.ToString());
                foreach (var pathForVersion in files)
                {
                    directory.CreateIfNotExists(folderName);
                    var parts = _filePath.GetFileName(pathForVersion).Split('_');
                    var name = string.Format("{0}_{1}_{2}", parts[1], parts[2], parts[3]);
                    var destination = _filePath.Combine(folderName, name);
                    if (!_file.Exists(destination))
                    {
                        _file.Move(pathForVersion, destination);
                    }
                }
            }
            try
            {
                const string partialName = "VersionControl";
                var dirs = directory.GetDirectories(_resourcePath, "*" + partialName + "*");
                foreach (var item in dirs)
                {
                    directory.Delete(item, true);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, "Warewolf Error");
            }
        }

        public int GetLatestVersionNumber(Guid resourceId)
        {
            // TODO: bugfix: version number can not be defined by the total count of version files in a directory
            var path = GetVersionFolderPath(resourceId.ToString());
            var files = _directory.GetFiles(path);
            return files.Count() + 1;
        }
    }
}
