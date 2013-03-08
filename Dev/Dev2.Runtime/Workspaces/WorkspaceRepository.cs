using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Dev2.Common;
using Dev2.DynamicServices;

namespace Dev2.Workspaces
{
    /// <summary>
    /// A workspace repository.
    /// </summary>
    public class WorkspaceRepository : IWorkspaceRepository
    {
        /// <summary>
        /// The server workspace ID.
        /// </summary>
        public static readonly Guid ServerWorkspaceID = Guid.Empty;

        readonly ConcurrentDictionary<Guid, IWorkspace> _items = new ConcurrentDictionary<Guid, IWorkspace>();

        private static object _workspaceLock = new object();

        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        static volatile WorkspaceRepository _instance;
        static readonly object SyncRoot = new Object();

        /// <summary>
        /// Gets the repository instance.
        /// </summary>
        public static WorkspaceRepository Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(SyncRoot)
                    {
                        if(_instance == null)
                        {
                            _instance = new WorkspaceRepository();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Initialization

        // Prevent instantiation
        private WorkspaceRepository()
        {
            RepositoryPath = GlobalConstants.WorkspacePath;
            Directory.CreateDirectory(RepositoryPath);
            Get(ServerWorkspaceID, true);
        }

        #endregion

        #region Properties

        #region RepositoryPath

        /// <summary>
        /// Gets the repository path.
        /// </summary>
        public string RepositoryPath
        {
            get;
            private set;
        }

        #endregion

        #region Count

        /// <summary>
        /// Gets the number of items in the repository.
        /// </summary>
        public int Count
        {
            get
            {
                return _items.Count;
            }
        }

        #endregion

        #region ServerWorkspace

        /// <summary>
        /// Gets the server workspace.
        /// </summary>
        public IWorkspace ServerWorkspace
        {
            get
            {
                return Get(ServerWorkspaceID);
            }
        }

        #endregion

        #region Endpoint

        public DynamicServicesEndpoint Endpoint
        {
            get;
            set;
        }

        #endregion

        #endregion

        #region Get

        /// <summary>
        /// Gets the <see cref="IWorkspace" /> with the specified ID from storage if it does not exist in the repository.
        /// </summary>
        /// <param name="workspaceID">The workdspace ID to be queried.</param>
        /// <param name="force"><code>true</code> if the workspace should be re-read even it is found; <code>false</code> otherwise.</param>
        /// <returns>
        /// The <see cref="IWorkspace" /> with the specified ID, or <code>null</code> if not found.
        /// </returns>
        public IWorkspace Get(Guid workspaceID, bool force = false)
        {
            IWorkspace workspace;
            if(force || !_items.TryGetValue(workspaceID, out workspace))
            {
                workspace = Read(workspaceID);

                // HACK: Workaround for leaving server resource at root so that we can use DynamicServicesHost
                // TODO: Remove DynamicServicesHost dependency and use DynamicServiceRepository
                var workspacePath = workspaceID == ServerWorkspaceID
                    ? GlobalConstants.ApplicationPath
                    : Path.Combine(RepositoryPath, workspaceID.ToString());

                if(workspaceID != ServerWorkspaceID)
                {
                    ServerWorkspace.Host.CopyTo(workspacePath);
                }

                workspace.Host = new DynamicServicesHost(Endpoint, workspacePath);
                _items[workspaceID] = workspace;
            }
            return workspace;
        }

        #endregion

        #region RefreshWorkspaces

        /// <summary>
        /// Refreshes all workspaces from storage.
        /// </summary>
        public void RefreshWorkspaces()
        {
            //TODO, 2012-10-24, This is a temporary implementation which brute force 
            //                  refreshes all client workspaces by removing them,
            //                  no changes in the client workspaces are preserved.
            List<Guid> worksSpacesToRemove = _items.Keys.Where(k => k != ServerWorkspaceID).ToList();
            foreach(Guid workspaceGuid in worksSpacesToRemove)
            {
                IWorkspace workspace;
                _items.TryRemove(workspaceGuid, out workspace);
            }
        }

        #endregion

        #region GetLatest

        /// <summary>
        /// Overwrites this workspace with the server versions except for those provided.
        /// </summary>
        /// <param name="workspace">The workspace to be queried.</param>
        /// <param name="servicesToIgnore">The services being to be ignored.</param>
        public void GetLatest(IWorkspace workspace, IList<string> servicesToIgnore)
        {
            List<string> filesToIgnore = servicesToIgnore.Select(s => s += ".xml").ToList();
            var targetPath = Path.Combine(RepositoryPath, workspace.ID.ToString());
            ServerWorkspace.Host.SyncTo(targetPath, true, true, filesToIgnore);
            workspace.Host.RestoreResources(new string[]{"Sources","Services"});
        }

        #endregion

        #region Save

        /// <summary>
        /// Saves the specified workspace to storage.
        /// </summary>
        /// <param name="workspace">The workspace to be saved.</param>
        public void Save(IWorkspace workspace)
        {
            if(workspace == null)
            {
                return;
            }

            // The workspace is expected to only contain client-side values
            // To avoid nulling out server-side values we persist it first
            // as this only seriliazes client-side values.
            Write(workspace);

            // Force reload of the new version
            Get(workspace.ID, true);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes the specified workspace from storage.
        /// </summary>
        /// <param name="workspace">The workspace to be deleted.</param>
        public void Delete(IWorkspace workspace)
        {
            if(workspace == null)
            {
                return;
            }

            IWorkspace result;
            _items.TryRemove(workspace.ID, out result);
            Delete(workspace.ID);
        }

        #endregion

        #region File Handling

        // TODO: Refactor file serialization handling into separate testable class

        #region Read

        private IWorkspace Read(Guid workdspaceID)
        {
            // force a lock on the file system ;)
            lock (_workspaceLock)
            {
                var filePath = GetFileName(workdspaceID);
                var fileExists = File.Exists(filePath);
                using (var stream = File.Open(filePath, FileMode.OpenOrCreate))
                {
                    var formatter = new BinaryFormatter();
                    if (fileExists)
                    {
                        try
                        {
                            return (IWorkspace) formatter.Deserialize(stream);
                        }
                            // ReSharper disable EmptyGeneralCatchClause 
                        catch
                            // ReSharper restore EmptyGeneralCatchClause
                        {
                            // Deserialization failed so overwrite with new one.
                        }
                    }

                    var result = new Workspace(workdspaceID);
                    formatter.Serialize(stream, result);
                    return result;
                }
            }
        }

        #endregion

        #region Write

        void Write(IWorkspace workspace)
        {
            if(workspace == null)
            {
                return;
            }

            var filePath = GetFileName(workspace.ID);
            using(var stream = File.Open(filePath, FileMode.OpenOrCreate))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, workspace);
            }
        }

        #endregion

        #region Delete

        void Delete(Guid workspaceID)
        {
            var filePath = GetFileName(workspaceID);
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        #endregion

        #region GetFileName

        string GetFileName(Guid workspaceID)
        {
            return Path.Combine(RepositoryPath, workspaceID + ".uws");
        }

        #endregion

        #endregion
    }
}
