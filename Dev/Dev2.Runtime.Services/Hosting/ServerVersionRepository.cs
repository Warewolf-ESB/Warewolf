using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;
using System.Linq;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Explorer;
using Dev2.Runtime.ServiceModel.Data;
using ServiceStack.Common.Extensions;

namespace Dev2.Runtime.Hosting
{
    public class ServerVersionRepository : IServerVersionRepository
    {
        private readonly IVersionStrategy _versionStrategy;
        private readonly IResourceCatalog _catalogue;
        private readonly IDirectory _directory;
        private readonly IFile _file;
        protected static readonly object LockObject= new object();  
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

            if(resource == null || resource.VersionInfo == null)
            {
                return new List<IExplorerItem>();
            }
            var path = GetVersionFolderFromResource(resource);

// ReSharper disable ImplicitlyCapturedClosure
            var files = _directory.GetFiles(path).Where(a => a.Contains(resource.VersionInfo.VersionId.ToString()));
// ReSharper restore ImplicitlyCapturedClosure
            return files.Select(a => CreateVersionFromFilePath(a, resource)).OrderByDescending(a => a.VersionInfo.DateTimeStamp).Take(GlobalConstants.VersionCount).ToList();
        }

        public void MoveVersions(Guid resourceId,string newPath)
        {
            var resource = _catalogue.GetResource(Guid.Empty, resourceId);

            if (resource == null || resource.VersionInfo == null)
            {
                return;
            }
            var path = GetVersionFolderFromResource(resource);

            // ReSharper disable ImplicitlyCapturedClosure
            var files = _directory.GetFiles(path).Where(a => a.Contains(resource.VersionInfo.VersionId.ToString()));
            var versionPath = Path.Combine( ServerExplorerRepository.DirectoryStructureFromPath(newPath),"VersionControl");
            if(!_directory.Exists(versionPath))
                _directory.CreateIfNotExists(versionPath);
            // ReSharper restore ImplicitlyCapturedClosure
            IEnumerable<string> enumerable = files as IList<string> ?? files.ToList();
            // ReSharper disable once AssignNullToNotNullAttribute
            enumerable.ForEach(a=>_file.Move(a, Path.Combine(versionPath,Path.GetFileName(a))));
          }

        public StringBuilder GetVersion(IVersionInfo version)
        {

            var resource = _catalogue.GetResource(Guid.Empty, version.ResourceId);
            var path = GetVersionFolderFromResource(resource);
            var files = _directory.GetFiles(path).FirstOrDefault(a => a.Contains(string.Format("{0}_{1}_", resource.VersionInfo.VersionId.ToString(), version.VersionNumber)));
            if(string.IsNullOrEmpty(files))
                throw  new VersionNotFoundException("Version Does not exist");

            return new StringBuilder(_file.ReadAllText(files));
        }



        string GetVersionFolderFromResource(IResource resource)
        {
            var path = Path.Combine(_rootPath, GetDirectoryFromResource(resource), GlobalConstants.VersionFolder);
            _directory.CreateIfNotExists(path);


            return path;
        }

        string GetFolderFromResource(IResource resource)
        {
            var path = Path.Combine(_rootPath, GetDirectoryFromResource(resource));
            _directory.CreateIfNotExists(path);


            return path;
        }

        IExplorerItem CreateVersionFromFilePath(string path, IResource resource)
        {
            return new ServerExplorerItem(CreateNameFromPath(path), resource.ResourceID, ResourceType.Version, new List<IExplorerItem>(), Permissions.View, resource.ResourcePath)
                {
                    VersionInfo = CreateVersionInfoFromFilePath(path, resource.ResourceID)
                };
        }

        IVersionInfo CreateVersionInfoFromFilePath(string path, Guid resourceId)
        {
            var name = new FileInfo(path).Name;
            var parts = name.Split(new[] { '_' });
            if(parts.Length != 4)
                throw new Exception("Invalid Version found");
            return new VersionInfo(new DateTime(long.Parse(parts[2])), parts[3], "", parts[1], resourceId, Guid.Parse(parts[0]));
        }

        string CreateNameFromPath(string path)
        {
            var name = new FileInfo(path).Name;
            var parts = name.Split(new[] { '_' });
            if(parts.Length != 4)
                throw new Exception("Invalid Version found");
            return String.Format("v.{0}  {1}  {2}", parts[1], new DateTime(long.Parse(parts[2])), parts[3].Replace(".xml", ""));
        }

        static string GetDirectoryFromResource(IResource resource)
        {
            if(resource.ResourcePath.Contains("\\"))
                return resource.ResourcePath.Substring(0, resource.ResourcePath.LastIndexOf('\\'));
            return "";
        }
        
        public IExplorerItem GetLatestVersionNumber(Guid resourceId)
        {
            return null;
        }

        public IRollbackResult RollbackTo(Guid resourceId, string versionNumber)
        {
            var res = _catalogue.GetResource(Guid.Empty, resourceId);
            var xmlBuilder = GetVersion(new VersionInfo(DateTime.MinValue, "", "", versionNumber, resourceId, res.VersionInfo.VersionId));
            var xml = xmlBuilder.ToXElement();
            Resource oldResource = new Resource(xml);
            UpdateCategoryIfRenamed(res, oldResource, xml);
            StoreAndDeleteCurrentIfRenamed(res, oldResource);
            UpdateVersionInfoIfNotExists(resourceId, xml, res);
            _catalogue.SaveResource(Guid.Empty, xml.ToStringBuilder(),null,"Rollback","Unknown");
            if (oldResource.ResourceName != res.ResourceName)
                _catalogue.GetResource(Guid.Empty, res.ResourceID).ResourceName = oldResource.ResourceName; 
            return new RollbackResult{DisplayName = oldResource.ResourceName, VersionHistory = GetVersions(resourceId)};
        }

        static void UpdateVersionInfoIfNotExists(Guid resourceId, XElement xml, IResource res)
        {
            var versionInfo = xml.Elements("VersionInfo").FirstOrDefault();
            if(versionInfo != null)
            {
                versionInfo.SetAttributeValue("DateTimeStamp", DateTime.Now);
                versionInfo.SetAttributeValue("Reason", "Rolback");
                versionInfo.SetAttributeValue("User", res.VersionInfo.User);
                versionInfo.SetAttributeValue("VersionNumber", 1 + int.Parse((res.VersionInfo.VersionNumber)));
                versionInfo.SetAttributeValue("ResourceId", resourceId);
                versionInfo.SetAttributeValue("VersionId", res.VersionInfo.VersionId);
            }
        }

        void StoreAndDeleteCurrentIfRenamed(IResource res, Resource oldResource)
        {
            if(res.ResourceName != oldResource.ResourceName)
            {
                StoreVersion(res, "unknown", "Rollback", Guid.Empty);
                _catalogue.DeleteResource(Guid.Empty, res.ResourceName, res.ResourceType.ToString(), null, false);
            }
        }

        static void UpdateCategoryIfRenamed(IResource res, Resource oldResource, XElement xml)
        {
            if(res.ResourcePath != null && res.ResourcePath != res.ResourceName) // we are not in the root folder
            {
                var oldPath = res.ResourcePath.Substring(0, 1 + res.ResourcePath.LastIndexOf("\\", StringComparison.Ordinal));
                var newPath = oldResource.ResourcePath.Substring(0, 1 + res.ResourcePath.LastIndexOf("\\", StringComparison.Ordinal));
                if(oldPath != newPath)
                {
                    oldResource.ResourcePath = newPath + oldResource.ResourceName;
                    xml.SetAttributeValue("Category", newPath + oldResource.ResourceName);
                }
            }
        }

        public IList<IExplorerItem> DeleteVersion(Guid resourceId, string versionNumber)
        {
            var resource = _catalogue.GetResource(Guid.Empty, resourceId);
            var path = GetVersionFolderFromResource(resource);
            var files = _directory.GetFiles(path).FirstOrDefault(a => a.Contains(string.Format("{0}_{1}_", resource.VersionInfo.VersionId.ToString(), versionNumber)));
            _file.Delete(files);
            return GetVersions(resourceId);
        }

        public void StoreVersion(IResource resource, string userName, string reason, Guid workSpaceId)
        {
            if (workSpaceId == Guid.Empty)
            {


                lock (LockObject)
                {
                    var old = _catalogue.GetResource(Guid.Empty, resource.ResourceID);
                    if (old != null)
                    {
                        var versions = GetVersions(resource.ResourceID).FirstOrDefault();
                        old.VersionInfo = _versionStrategy.GetCurrentVersion(resource, versions==null? null :versions.VersionInfo, userName, reason);
                        var folderPath = GetVersionFolderFromResource(resource);

                        var fileName = string.Format("{0}_{1}_{2}_{3}.xml", old.VersionInfo.VersionId, old.VersionInfo.VersionNumber, GetDateString(old.VersionInfo.DateTimeStamp), reason);
                        if (!_file.Exists(Path.Combine(folderPath, fileName))) //todo: remove this and stop save on workspace
                        {
                            var sourceFile = Path.Combine(GetFolderFromResource(resource), old.ResourceName) + ".xml";
                            if(_file.Exists(sourceFile))
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