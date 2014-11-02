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
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
using Dev2.Runtime.ServiceModel.Data;
using ServiceStack.Common.Extensions;

namespace Dev2.Runtime.Hosting
{
    public class ServerVersionRepository : IServerVersionRepository
    {
        protected static readonly object LockObject = new object();
        private readonly IResourceCatalog _catalogue;
        private readonly IDirectory _directory;
        private readonly IFile _file;
        private readonly string _rootPath;
        private readonly IVersionStrategy _versionStrategy;

        public ServerVersionRepository(IVersionStrategy versionStrategy, IResourceCatalog catalogue,
            IDirectory directory, string rootPath, IFile file)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>
            {
                {"versionStrategy", versionStrategy},
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
            IResource resource = _catalogue.GetResource(Guid.Empty, resourceId);

            if (resource == null || resource.VersionInfo == null)
            {
                return new List<IExplorerItem>();
            }
            string path = GetVersionFolderFromResource(resource);

// ReSharper disable ImplicitlyCapturedClosure
            IEnumerable<string> files =
                _directory.GetFiles(path).Where(a => a.Contains(resource.VersionInfo.VersionId.ToString()));
// ReSharper restore ImplicitlyCapturedClosure
            return
                files.Select(a => CreateVersionFromFilePath(a, resource))
                    .OrderByDescending(a => a.VersionInfo.DateTimeStamp)
                    .Take(GlobalConstants.VersionCount)
                    .ToList();
        }

        public void MoveVersions(Guid resourceId, string newPath)
        {
            IResource resource = _catalogue.GetResource(Guid.Empty, resourceId);

            if (resource == null || resource.VersionInfo == null)
            {
                return;
            }
            string path = GetVersionFolderFromResource(resource);

            // ReSharper disable ImplicitlyCapturedClosure
            IEnumerable<string> files =
                _directory.GetFiles(path).Where(a => a.Contains(resource.VersionInfo.VersionId.ToString()));
            string versionPath = Path.Combine(ServerExplorerRepository.DirectoryStructureFromPath(newPath),
                "VersionControl");
            if (!_directory.Exists(versionPath))
                _directory.CreateIfNotExists(versionPath);
            // ReSharper restore ImplicitlyCapturedClosure
            IEnumerable<string> enumerable = files as IList<string> ?? files.ToList();
            // ReSharper disable once AssignNullToNotNullAttribute
            enumerable.ForEach(a => _file.Move(a, Path.Combine(versionPath, Path.GetFileName(a))));
        }

        public StringBuilder GetVersion(IVersionInfo version)
        {
            IResource resource = _catalogue.GetResource(Guid.Empty, version.ResourceId);
            string path = GetVersionFolderFromResource(resource);
            string files =
                _directory.GetFiles(path)
                    .FirstOrDefault(
                        a =>
                            a.Contains(string.Format("{0}_{1}_", resource.VersionInfo.VersionId.ToString(),
                                version.VersionNumber)));
            if (string.IsNullOrEmpty(files))
                throw new VersionNotFoundException("Version Does not exist");

            return new StringBuilder(_file.ReadAllText(files));
        }

        public IExplorerItem GetLatestVersionNumber(Guid resourceId)
        {
            return null;
        }

        public IRollbackResult RollbackTo(Guid resourceId, string versionNumber)
        {
            IResource res = _catalogue.GetResource(Guid.Empty, resourceId);
            StringBuilder xmlBuilder =
                GetVersion(new VersionInfo(DateTime.MinValue, "", "", versionNumber, resourceId,
                    res.VersionInfo.VersionId));
            XElement xml = xmlBuilder.ToXElement();
            var oldResource = new Resource(xml);
            UpdateCategoryIfRenamed(res, oldResource, xml);
            StoreAndDeleteCurrentIfRenamed(res, oldResource);
            UpdateVersionInfoIfNotExists(resourceId, xml, res);
            _catalogue.SaveResource(Guid.Empty, xml.ToStringBuilder(), null, "Rollback", "Unknown");
            if (oldResource.ResourceName != res.ResourceName)
                _catalogue.GetResource(Guid.Empty, res.ResourceID).ResourceName = oldResource.ResourceName;
            return new RollbackResult {DisplayName = oldResource.ResourceName, VersionHistory = GetVersions(resourceId)};
        }

        public IList<IExplorerItem> DeleteVersion(Guid resourceId, string versionNumber)
        {
            IResource resource = _catalogue.GetResource(Guid.Empty, resourceId);
            string path = GetVersionFolderFromResource(resource);
            string files =
                _directory.GetFiles(path)
                    .FirstOrDefault(
                        a =>
                            a.Contains(string.Format("{0}_{1}_", resource.VersionInfo.VersionId.ToString(),
                                versionNumber)));
            _file.Delete(files);
            return GetVersions(resourceId);
        }

        public void StoreVersion(IResource resource, string userName, string reason, Guid workSpaceId)
        {
            if (workSpaceId == Guid.Empty)
            {
                lock (LockObject)
                {
                    IResource old = _catalogue.GetResource(Guid.Empty, resource.ResourceID);
                    if (old != null)
                    {
                        IExplorerItem versions = GetVersions(resource.ResourceID).FirstOrDefault();
                        old.VersionInfo = _versionStrategy.GetCurrentVersion(resource,
                            versions == null ? null : versions.VersionInfo, userName, reason);
                        string folderPath = GetVersionFolderFromResource(resource);

                        string fileName = string.Format("{0}_{1}_{2}_{3}.xml", old.VersionInfo.VersionId,
                            old.VersionInfo.VersionNumber, GetDateString(old.VersionInfo.DateTimeStamp), reason);
                        if (!_file.Exists(Path.Combine(folderPath, fileName)))
                            //todo: remove this and stop save on workspace
                        {
                            string sourceFile = Path.Combine(GetFolderFromResource(resource), old.ResourceName) + ".xml";
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


        private string GetVersionFolderFromResource(IResource resource)
        {
            string path = Path.Combine(_rootPath, GetDirectoryFromResource(resource), GlobalConstants.VersionFolder);
            _directory.CreateIfNotExists(path);


            return path;
        }

        private string GetFolderFromResource(IResource resource)
        {
            string path = Path.Combine(_rootPath, GetDirectoryFromResource(resource));
            _directory.CreateIfNotExists(path);


            return path;
        }

        private IExplorerItem CreateVersionFromFilePath(string path, IResource resource)
        {
            return new ServerExplorerItem(CreateNameFromPath(path), resource.ResourceID, ResourceType.Version,
                new List<IExplorerItem>(), Permissions.View, resource.ResourcePath)
            {
                VersionInfo = CreateVersionInfoFromFilePath(path, resource.ResourceID)
            };
        }

        private IVersionInfo CreateVersionInfoFromFilePath(string path, Guid resourceId)
        {
            string name = new FileInfo(path).Name;
            string[] parts = name.Split(new[] {'_'});
            if (parts.Length != 4)
                throw new Exception("Invalid Version found");
            return new VersionInfo(new DateTime(long.Parse(parts[2])), parts[3], "", parts[1], resourceId,
                Guid.Parse(parts[0]));
        }

        private string CreateNameFromPath(string path)
        {
            string name = new FileInfo(path).Name;
            string[] parts = name.Split(new[] {'_'});
            if (parts.Length != 4)
                throw new Exception("Invalid Version found");
            return String.Format("v.{0}  {1}  {2}", parts[1], new DateTime(long.Parse(parts[2])),
                parts[3].Replace(".xml", ""));
        }

        private static string GetDirectoryFromResource(IResource resource)
        {
            if (resource.ResourcePath.Contains("\\"))
                return resource.ResourcePath.Substring(0, resource.ResourcePath.LastIndexOf('\\'));
            return "";
        }

        private static void UpdateVersionInfoIfNotExists(Guid resourceId, XElement xml, IResource res)
        {
            XElement versionInfo = xml.Elements("VersionInfo").FirstOrDefault();
            if (versionInfo != null)
            {
                versionInfo.SetAttributeValue("DateTimeStamp", DateTime.Now);
                versionInfo.SetAttributeValue("Reason", "Rolback");
                versionInfo.SetAttributeValue("User", res.VersionInfo.User);
                versionInfo.SetAttributeValue("VersionNumber", 1 + int.Parse((res.VersionInfo.VersionNumber)));
                versionInfo.SetAttributeValue("ResourceId", resourceId);
                versionInfo.SetAttributeValue("VersionId", res.VersionInfo.VersionId);
            }
        }

        private void StoreAndDeleteCurrentIfRenamed(IResource res, Resource oldResource)
        {
            if (res.ResourceName != oldResource.ResourceName)
            {
                StoreVersion(res, "unknown", "Rollback", Guid.Empty);
                _catalogue.DeleteResource(Guid.Empty, res.ResourceName, res.ResourceType.ToString(), null, false);
            }
        }

        private static void UpdateCategoryIfRenamed(IResource res, Resource oldResource, XElement xml)
        {
            if (res.ResourcePath != null && res.ResourcePath != res.ResourceName) // we are not in the root folder
            {
                string oldPath = res.ResourcePath.Substring(0,
                    1 + res.ResourcePath.LastIndexOf("\\", StringComparison.Ordinal));
                string newPath = oldResource.ResourcePath.Substring(0,
                    1 + res.ResourcePath.LastIndexOf("\\", StringComparison.Ordinal));
                if (oldPath != newPath)
                {
                    oldResource.ResourcePath = newPath + oldResource.ResourceName;
                    xml.SetAttributeValue("Category", newPath + oldResource.ResourceName);
                }
            }
        }

        private string GetDateString(DateTime dateTimeStamp)
        {
            return dateTimeStamp.Ticks.ToString(CultureInfo.InvariantCulture);
        }

        #endregion
    }
}