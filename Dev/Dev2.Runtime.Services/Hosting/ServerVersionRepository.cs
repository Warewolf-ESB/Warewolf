/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.Hosting
{
    public class ServerVersionRepository : IServerVersionRepository
    {
        private readonly IVersionStrategy _versionStrategy;
        private readonly IResourceCatalog _catalogue;
        private readonly IDirectory _directory;
        private readonly IFile _file;
        protected static readonly object LockObject = new object();
        readonly string _rootPath;

        public ServerVersionRepository(IVersionStrategy versionStrategy, IResourceCatalog catalogue, IDirectory directory, string rootPath, IFile file)
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
        }

        #region Implementation of IVersionRepository

        public IList<IExplorerItem> GetVersions(Guid resourceId)
        {
            
            var resource = _catalogue.GetResource(Guid.Empty, resourceId);

            if (resource?.VersionInfo == null)
            {
                return new List<IExplorerItem>();
            }
            var versionPath = resource.GetResourcePath(GlobalConstants.ServerWorkspaceID);
            var path = GetVersionFolderFromResource(versionPath);

            // ReSharper disable ImplicitlyCapturedClosure
            var files = _directory.GetFiles(path).Where(a => a.Contains(resource.VersionInfo.VersionId.ToString()));
            // ReSharper restore ImplicitlyCapturedClosure
            return files.Select(a => CreateVersionFromFilePath(a, resource, path)).OrderByDescending(a => a.VersionInfo.DateTimeStamp).Take(GlobalConstants.VersionCount).ToList();
        }

        public void MoveVersions(Guid resourceId, string newPath, string resourcePath)
        {
            var resource = _catalogue.GetResource(Guid.Empty, resourceId);

            if (resource?.VersionInfo == null)
            {
                return;
            }
            var path = GetVersionFolderFromResource(resourcePath);

            // ReSharper disable ImplicitlyCapturedClosure
            var files = _directory.GetFiles(path).Where(a => a.Contains(resource.VersionInfo.VersionId.ToString()));
            var versionPath = Path.Combine(ServerExplorerRepository.DirectoryStructureFromPath(newPath), "VersionControl");
            if (!_directory.Exists(versionPath))
                _directory.CreateIfNotExists(versionPath);
            // ReSharper restore ImplicitlyCapturedClosure
            IEnumerable<string> enumerable = files as IList<string> ?? files.ToList();
            // ReSharper disable once AssignNullToNotNullAttribute
            enumerable.ForEach(a => _file.Move(a, Path.Combine(versionPath, Path.GetFileName(a))));
        }

        public StringBuilder GetVersion(IVersionInfo version, string resourcePath)
        {
            var resource = _catalogue.GetResource(Guid.Empty, version.ResourceId);
            var path = GetVersionFolderFromResource(resourcePath);
            var files = _directory.GetFiles(path).FirstOrDefault(a => a.Contains(string.Format("{0}_{1}_", resource.VersionInfo.VersionId.ToString(), version.VersionNumber)));
            if (string.IsNullOrEmpty(files))
                throw new VersionNotFoundException("Version Does not exist");

            return new StringBuilder(_file.ReadAllText(files));
        }

        string GetVersionFolderFromResource(string resourcePath)
        {
            var path = Path.Combine(_rootPath, GetDirectoryFromResource(resourcePath), GlobalConstants.VersionFolder);
            _directory.CreateIfNotExists(path);
            
            return path;
        }

        string GetFolderFromResource(string resourcePath)
        {
            var path = Path.Combine(_rootPath, GetDirectoryFromResource(resourcePath));
            _directory.CreateIfNotExists(path);

            return path;
        }

        IExplorerItem CreateVersionFromFilePath(string path, IResource resource, string resourcePath)
        {
            return new ServerExplorerItem(CreateNameFromPath(path), resource.ResourceID, "Version", new List<IExplorerItem>(), Permissions.View, resourcePath)
            {
                VersionInfo = CreateVersionInfoFromFilePath(path, resource.ResourceID),
                IsResourceVersion = true
            };
        }

        IVersionInfo CreateVersionInfoFromFilePath(string path, Guid resourceId)
        {
            var name = new FileInfo(path).Name;
            var parts = name.Split('_');
            if (parts.Length != 4)
                throw new Exception(ErrorResource.InvalidVersion);
            return new VersionInfo(new DateTime(long.Parse(parts[2])), parts[3], "", parts[1], resourceId, Guid.Parse(parts[0]));
        }

        string CreateNameFromPath(string path)
        {
            var name = new FileInfo(path).Name;
            var parts = name.Split('_');
            if (parts.Length != 4)
                throw new Exception(ErrorResource.InvalidVersion);
            return $"v.{parts[1]}  {new DateTime(long.Parse(parts[2]))}  {parts[3].Replace(".xml", "")}";
        }

        static string GetDirectoryFromResource(string resourcePath)
        {
            if (resourcePath.Contains("\\"))
                return resourcePath.Substring(0, resourcePath.LastIndexOf('\\'));
            return "";
        }

        public IRollbackResult RollbackTo(Guid resourceId, string versionNumber)
        {
            var res = _catalogue.GetResource(Guid.Empty, resourceId);
            var resourcePath = res.GetResourcePath(Guid.Empty);
            var xmlBuilder = GetVersion(new VersionInfo(DateTime.MinValue, "", "", versionNumber, resourceId, res.VersionInfo.VersionId), resourcePath);
            var xml = xmlBuilder.ToXElement();
            Resource oldResource = new Resource(xml);
            StoreAndDeleteCurrentIfRenamed(res, oldResource, resourcePath);
            UpdateVersionInfoIfNotExists(resourceId, xml, res);
            _catalogue.SaveResource(Guid.Empty, xml.ToStringBuilder(), "", "Rollback", "WorkflowService");
            if (oldResource.ResourceName != res.ResourceName)
                _catalogue.GetResource(Guid.Empty, res.ResourceID).ResourceName = oldResource.ResourceName;
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
            var path = GetVersionFolderFromResource(resourcePath);
            var files = _directory.GetFiles(path).FirstOrDefault(a => a.Contains($"{resource.VersionInfo.VersionId.ToString()}_{versionNumber}_"));
            _file.Delete(files);
            return GetVersions(resourceId);
        }

        public void StoreVersion(IResource resource, string userName, string reason, Guid workSpaceId, string resourcePath)
        {
            if (workSpaceId == Guid.Empty)
            {
                if (string.IsNullOrEmpty(userName))
                    userName = Thread.CurrentPrincipal.Identity.Name;

                lock (LockObject)
                {
                    var old = _catalogue.GetResource(Guid.Empty, resource.ResourceID);
                    if (old != null)
                    {
                        var versions = GetVersions(resource.ResourceID).FirstOrDefault();
                        old.VersionInfo = _versionStrategy.GetCurrentVersion(resource, versions?.VersionInfo, userName, reason);
                        var folderPath = GetVersionFolderFromResource(old.GetResourcePath(workSpaceId));

                        var fileName = $"{old.VersionInfo.VersionId}_{old.VersionInfo.VersionNumber}_{GetDateString(old.VersionInfo.DateTimeStamp)}_{reason}.xml";
                        if (!_file.Exists(Path.Combine(folderPath, fileName))) //todo: remove this and stop save on workspace
                        {
                            var sourceFile = Path.Combine(GetFolderFromResource(old.GetResourcePath(workSpaceId)), old.ResourceName) + ".xml";
                            if (_file.Exists(sourceFile))
                            {
                                _file.Copy(sourceFile, Path.Combine(folderPath, fileName));
                            }
                        }

                        resource.VersionInfo = _versionStrategy.GetNextVersion(resource, old, userName, reason);
                    }
                }
            }
        }

        string GetDateString(DateTime dateTimeStamp)
        {
            return dateTimeStamp.Ticks.ToString(CultureInfo.InvariantCulture);
        }

        #endregion
    }
}
