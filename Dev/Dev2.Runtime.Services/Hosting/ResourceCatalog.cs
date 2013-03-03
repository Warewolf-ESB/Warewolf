using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.Hosting
{
    public class ResourceCatalog : IResourceCatalog
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
                var folders = Resources.RootFolders.Values.Distinct();
                var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);
                var task = LoadAsync(workspacePath, folders.ToArray());
                task.Wait();
                return task.Result;
            }
        }

        #endregion

        #region LoadAsync

        public static async Task<List<IResource>> LoadAsync(string workspacePath, params string[] folders)
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
                                var resource = new Resource(xml) { Contents = xml.ToString(SaveOptions.DisableFormatting), FilePath = filePath };
                                catalog.Add(resource);
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
    }
}