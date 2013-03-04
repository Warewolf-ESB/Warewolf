using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.Hosting
{
    public class ResourceCatalog
    {
        readonly ConcurrentDictionary<Guid, List<IResource>> _workspaceResources = new ConcurrentDictionary<Guid, List<IResource>>();
        readonly ConcurrentDictionary<Guid, object> _workspaceLocks = new ConcurrentDictionary<Guid, object>();
        readonly object _loadLock = new object();

        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        static volatile ResourceCatalog _instance;
        static readonly object SyncRoot = new Object();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static ResourceCatalog Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(SyncRoot)
                    {
                        if(_instance == null)
                        {
                            _instance = new ResourceCatalog();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        public int Count
        {
            get
            {
                return _workspaceResources.Count;
            }
        }

        public List<IResource> this[Guid workspaceID]
        {
            get
            {
                return _workspaceResources.GetOrAdd(workspaceID, LoadImpl);
            }
        }

        #region GetContents

        public string GetContents(Guid workspaceID, string resourceName)
        {
            var resources = this[workspaceID];
            var resource = resources.FirstOrDefault(r => string.Equals(r.ResourceName, resourceName, StringComparison.InvariantCultureIgnoreCase));
            return resource == null ? null : File.ReadAllText(resource.FilePath);
        }

        public string GetContents(Guid workspaceID, Guid resourceID, string version = null)
        {
            var resource = GetResource(workspaceID, resourceID, version);
            return resource == null ? null : File.ReadAllText(resource.FilePath);
        }

        public string GetContents(Guid workspaceID, IResource resource)
        {
            return resource == null ? null : File.ReadAllText(resource.FilePath);
        }

        #endregion

        #region GetResource

        public IResource GetResource(Guid workspaceID, Guid resourceID, string version = null)
        {
            var resources = this[workspaceID];
            var resource = new Resource { ResourceID = resourceID, Version = version };
            var index = resources.IndexOf(resource);
            return index == -1 ? null : resources[index];
        }

        #endregion

        #region Load

        public void Load(Guid workspaceID)
        {
            _workspaceResources.AddOrUpdate(workspaceID,
                id => LoadImpl(workspaceID),
                (id, resources) => LoadImpl(workspaceID));
        }

        #endregion

        #region LoadImpl

        List<IResource> LoadImpl(Guid workspaceID)
        {
            object workspaceLock;
            lock(_loadLock)
            {
                workspaceLock = _workspaceLocks.GetOrAdd(workspaceID, guid => new object());
            }
            lock(workspaceLock)
            {
                var folders = ServiceModel.Resources.RootFolders.Values.Distinct();
                var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);
                var task = LoadAsync(workspacePath, false, folders.ToArray());
                task.Wait();
                return task.Result;
            }
        }

        #endregion

        #region LoadAsync

        public static async Task<List<IResource>> LoadAsync(string workspacePath, bool includeContentsInResources, params string[] folders)
        {
            if(string.IsNullOrEmpty(workspacePath))
            {
                throw new ArgumentNullException("workspacePath");
            }
            if(folders == null)
            {
                throw new ArgumentNullException("folders");
            }

            var catalog = new List<IResource>();

            if(folders.Length == 0 || !Directory.Exists(workspacePath))
            {
                return catalog;
            }

            var tasks = new List<Task>();
            var sourceStreams = new List<FileStream>();
            try
            {
                foreach(var path in folders.Where(f => !string.IsNullOrEmpty(f)).Select(f => Path.Combine(workspacePath, f)))
                {
                    if(!Directory.Exists(path))
                    {
                        continue;
                    }
                    var files = Directory.GetFiles(path, "*.xml");
                    foreach(var file in files)
                    {
                        // Use the FileStream class, which has an option that causes asynchronous I/O to occur at the operating system level.  
                        // In many cases, this will avoid blocking a ThreadPool thread.  
                        var sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
                        sourceStreams.Add(sourceStream);

                        var buffer = new byte[sourceStream.Length];
                        var filePath = file;
                        var theTask = sourceStream
                            .ReadAsync(buffer, 0, buffer.Length)
                            .ContinueWith(task =>
                            {
                                XElement xml;
                                using(var xmlStream = new MemoryStream(buffer))
                                {
                                    xml = XElement.Load(xmlStream);
                                }
                                var isValid = HostSecurityProvider.Instance.VerifyXml(xml.ToString(SaveOptions.None));
                                if(isValid)
                                {
                                    var resource = new Resource(xml)
                                    {
                                        FilePath = filePath,
                                        Contents = includeContentsInResources ? xml.ToString(SaveOptions.DisableFormatting) : null
                                    };
                                    catalog.Add(resource);
                                }
                            });
                        tasks.Add(theTask);
                    }
                }

                await TaskEx.WhenAll(tasks);
            }
            finally
            {
                // Close all FileStream instances in a finally block after the tasks are complete. 
                // If each FileStream was instead created in a using statement, the FileStream 
                // might be disposed of before the task was complete
                foreach(var sourceStream in sourceStreams)
                {
                    sourceStream.Close();
                }
            }
            return catalog;
        }

        #endregion

        #region Read/WriteTextAsync

        // Use the FileStream class, which has an option that causes asynchronous I/O to occur at the operating system level.  
        // In many cases, this will avoid blocking a ThreadPool thread.  

        public static async Task WriteTextAsync(string filePath, string text)
        {
            var encodedText = Encoding.Unicode.GetBytes(text);

            using(var sourceStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            }
        }

        public static async Task<string> ReadTextAsync(string filePath)
        {
            using(var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                var sb = new StringBuilder();

                var buffer = new byte[0x1000];
                int numRead;
                while((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    var text = Encoding.Unicode.GetString(buffer, 0, numRead);
                    sb.Append(text);
                }

                return sb.ToString();
            }
        }

        #endregion

        #region Copy

        public void Copy(string resourceName, Guid sourceWorkspaceID, Guid targetWorkspaceID)
        {
            var resources = this[sourceWorkspaceID];
            var resource = resources.FirstOrDefault(r => string.Equals(r.ResourceName, resourceName, StringComparison.InvariantCultureIgnoreCase));
            if(resource != null)
            {
                var workspacePath = GlobalConstants.GetWorkspacePath(targetWorkspaceID);
                var xml = File.ReadAllText(resource.FilePath);
                Save(workspacePath, ServiceModel.Resources.RootFolders[resource.ResourceType], resource.ResourceName, xml);
            }
        }

        #endregion

        #region Save

        public void Save(Guid workspaceID, IResource resource)
        {
            // Must use resource.ToXml()!
            Save(workspaceID, resource.ResourceType, resource.ResourceName, resource.ToXml().ToString(SaveOptions.DisableFormatting));
        }

        public void Save(Guid workspaceID, ResourceType resourceType, string resourceName, string resourceXml)
        {
            var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);
            Save(workspacePath, ServiceModel.Resources.RootFolders[resourceType], resourceName, resourceXml);
        }

        // TODO: Make Save(string workspacePath ...) private
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
                if((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(fileName, attributes ^ FileAttributes.ReadOnly);
                }
            }

            var signedXml = HostSecurityProvider.Instance.SignXml(resourceXml);
            File.WriteAllText(fileName, signedXml, Encoding.UTF8);
        }

        #endregion

        #region Rollback

        public bool Rollback(Guid workspaceID, Guid resourceID, string fromVersion, string toVersion)
        {
            var resource = GetResource(workspaceID, resourceID, fromVersion);
            if(resource != null)
            {
                var folder = Path.GetDirectoryName(resource.FilePath);
                if(folder != null)
                {
                    var fileName = Path.Combine(folder, "VersionControl", string.Format("{0}.V{1}.xml", resource.ResourceName, toVersion));
                    if(File.Exists(fileName))
                    {
                        var fileContent = File.ReadAllText(fileName);
                        var isValid = HostSecurityProvider.Instance.VerifyXml(fileContent);
                        if(isValid)
                        {
                            File.WriteAllText(resource.FilePath, fileContent);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion

        #region SyncTo

        public void SyncTo(string sourceWorkspacePath, string targetWorkspacePath, bool overwrite = true, bool delete = true, IList<string> filesToIgnore = null)
        {
            if(filesToIgnore == null)
            {
                filesToIgnore = new List<string>();
            }

            var directories = new List<string> { "Sources", "Services" };

            foreach(var directory in directories)
            {
                var source = new DirectoryInfo(Path.Combine(sourceWorkspacePath, directory));
                var destination = new DirectoryInfo(Path.Combine(targetWorkspacePath, directory));

                if(!source.Exists)
                {
                    continue;
                }

                if(!destination.Exists)
                {
                    destination.Create();
                }

                //
                // Get the files from the source and desitnations folders, excluding the files which are to be ignored
                //
                var sourceFiles = source.GetFiles().Where(f => !filesToIgnore.Contains(f.Name)).ToList();
                var destinationFiles = destination.GetFiles().Where(f => !filesToIgnore.Contains(f.Name)).ToList();

                //
                // Calculate the files which are to be copied from source to destination, this respects the override parameter
                //

                var filesToCopyFromSource = new List<FileInfo>();

                if(overwrite)
                {
                    filesToCopyFromSource.AddRange(sourceFiles);
                }
                else
                {
                    filesToCopyFromSource.AddRange(sourceFiles
                        // ReSharper disable SimplifyLinqExpression
                        .Where(sf => !destinationFiles.Any(df => String.Compare(df.Name, sf.Name, StringComparison.OrdinalIgnoreCase) == 0)));
                    // ReSharper restore SimplifyLinqExpression
                }

                //
                // Calculate the files which are to be deleted from the destination, this respects the delete parameter
                //
                var filesToDeleteFromDestination = new List<FileInfo>();
                if(delete)
                {
                    filesToDeleteFromDestination.AddRange(destinationFiles
                        // ReSharper disable SimplifyLinqExpression
                        .Where(sf => !sourceFiles.Any(df => String.Compare(df.Name, sf.Name, StringComparison.OrdinalIgnoreCase) == 0)));
                    // ReSharper restore SimplifyLinqExpression
                }

                //
                // Copy files from source to desination
                //
                foreach(var file in filesToCopyFromSource)
                {
                    file.CopyTo(Path.Combine(destination.FullName, file.Name), true);
                }
            }
        }

        #endregion

        #region FindBySourceType

        public List<IResource> FindBySourceType(Guid workspaceID, enSourceType sourceType)
        {
            var resourceType = ResourceType.Unknown;
            switch(sourceType)
            {
                case enSourceType.SqlDatabase:
                case enSourceType.MySqlDatabase:
                    resourceType = ResourceType.DbSource;
                    break;
                case enSourceType.Plugin:
                    resourceType = ResourceType.PluginSource;
                    break;
                case enSourceType.Dev2Server:
                    resourceType = ResourceType.Server;
                    break;
            }

            var resources = this[workspaceID];
            return resources.FindAll(r => r.ResourceType == resourceType);
        }

        #endregion

        #region FindByID

        public List<IResource> FindByID(Guid workspaceID, string guidCsv)
        {
            if(guidCsv == null)
            {
                throw new ArgumentNullException("guidCsv");
            }

            var guids = new List<Guid>();
            foreach(var guidStr in guidCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                Guid guid;
                if(Guid.TryParse(guidStr, out guid))
                {
                    guids.Add(guid);
                }
            }

            var resources = this[workspaceID];
            return (from resource in resources
                    from guid in guids
                    where resource.ResourceID == guid
                    select resource).ToList();
        }

        #endregion

        #region FindByName

        public List<IResource> FindByName(Guid workspaceID, ResourceType resourceType, string resourceName)
        {
            var resources = this[workspaceID];
            return resources.FindAll(r => r.ResourceType == resourceType && string.Equals(r.ResourceName, resourceName, StringComparison.InvariantCultureIgnoreCase));
        }

        #endregion
    }
}