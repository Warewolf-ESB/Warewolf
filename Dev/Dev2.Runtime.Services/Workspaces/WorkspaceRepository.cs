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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Runtime.Hosting;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace Dev2.Workspaces
{
    /// <summary>
    ///     A workspace repository.
    /// </summary>
    public class WorkspaceRepository : IWorkspaceRepository
    {
        /// <summary>
        ///     The server workspace ID.
        /// </summary>
        public static readonly Guid ServerWorkspaceID = Guid.Empty;

        public static readonly string ServerWorkspacePath =
            EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID);

        private static readonly object WorkspaceLock = new object();
        private static readonly object UserMapLock = new object();

        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        private static volatile WorkspaceRepository _instance;
        private static readonly object SyncRoot = new Object();
        private readonly object _readLock = new object();

        /// <summary>
        ///     Gets the repository instance.
        /// </summary>
        public static WorkspaceRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
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

        public WorkspaceRepository()
            : this(ResourceCatalog.Instance)
        {
        }

        // PBI 9363 - 2013.05.29 - TWR: Added for testing 
        public WorkspaceRepository(IResourceCatalog resourceCatalog)
        {
            if (resourceCatalog == null)
            {
                throw new ArgumentNullException("resourceCatalog");
            }
            _resourceCatalog = resourceCatalog;
            Directory.CreateDirectory(EnvironmentVariables.WorkspacePath);

            _userMap = ReadUserMap();
            // ResourceCatalog constructor calls LoadFrequentlyUsedServices() 
            // which loads the server workspace resources so don't do it here
            Get(ServerWorkspaceID, true, false);
        }

        #endregion

        #region Properties

        #region Count

        /// <summary>
        ///     Gets the number of items in the repository.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        #endregion

        #region ServerWorkspace

        /// <summary>
        ///     Gets the server workspace.
        /// </summary>
        public IWorkspace ServerWorkspace
        {
            get { return Get(ServerWorkspaceID); }
        }

        #endregion

        #endregion

        private readonly ConcurrentDictionary<Guid, IWorkspace> _items = new ConcurrentDictionary<Guid, IWorkspace>();
        private readonly IResourceCatalog _resourceCatalog;
        private readonly ConcurrentDictionary<string, Guid> _userMap;

        public Guid GetWorkspaceID(WindowsIdentity identity)
        {
            string userID = identity.Name;
            if (identity.User != null)
            {
                userID = identity.User.Value;
            }
            Guid workspaceID;
            if (!_userMap.TryGetValue(userID, out workspaceID))
            {
                workspaceID = Guid.NewGuid();
                _userMap.TryAdd(userID, workspaceID);
                WriteUserMap(_userMap);
            }
            return workspaceID;
        }

        #region Get

        /// <summary>
        ///     Gets the <see cref="IWorkspace" /> with the specified ID from storage if it does not exist in the repository.
        /// </summary>
        /// <param name="workspaceID">The workdspace ID to be queried.</param>
        /// <param name="force">
        ///     <code>true</code> if the workspace should be re-read even it is found; <code>false</code>
        ///     otherwise.
        /// </param>
        /// <param name="loadResources"><code>true</code> if resources should be loaded; <code>false</code> otherwise.</param>
        /// <returns>
        ///     The <see cref="IWorkspace" /> with the specified ID, or <code>null</code> if not found.
        /// </returns>
        public IWorkspace Get(Guid workspaceID, bool force = false, bool loadResources = true)
        {
            lock (_readLock)
            {
                // PBI 9363 - 2013.05.29 - TWR: Added loadResources parameter
                IWorkspace workspace;
                if (force || !_items.TryGetValue(workspaceID, out workspace))
                {
                    workspace = Read(workspaceID);
                    if (loadResources)
                    {
                        _resourceCatalog.LoadWorkspace(workspaceID);
                    }
                    _items[workspaceID] = workspace;
                }
                return workspace;
            }
        }

        #endregion

        #region RefreshWorkspaces

        /// <summary>
        ///     Refreshes all workspaces from storage.
        /// </summary>
        public void RefreshWorkspaces()
        {
            //TODO, 2012-10-24, This is a temporary implementation which brute force 
            //                  refreshes all client workspaces by removing them,
            //                  no changes in the client workspaces are preserved.
            List<Guid> worksSpacesToRemove = _items.Keys.Where(k => k != ServerWorkspaceID).ToList();
            foreach (Guid workspaceGuid in worksSpacesToRemove)
            {
                IWorkspace workspace;
                _items.TryRemove(workspaceGuid, out workspace);
            }
        }

        #endregion

        #region GetLatest

        /// <summary>
        ///     Overwrites this workspace with the server versions except for those provided.
        /// </summary>
        /// <param name="workspace">The workspace to be queried.</param>
        /// <param name="servicesToIgnore">The services being to be ignored.</param>
        public void GetLatest(IWorkspace workspace, IList<string> servicesToIgnore)
        {
            lock (_readLock)
            {
                // ReSharper disable RedundantAssignment
                List<string> filesToIgnore = servicesToIgnore.Select(s => s += ".xml").ToList();
                // ReSharper restore RedundantAssignment
                string targetPath = EnvironmentVariables.GetWorkspacePath(workspace.ID);
                _resourceCatalog.SyncTo(ServerWorkspacePath, targetPath, true, true, filesToIgnore);
            }
        }

        #endregion

        #region Save

        /// <summary>
        ///     Saves the specified workspace to storage.
        /// </summary>
        /// <param name="workspace">The workspace to be saved.</param>
        public void Save(IWorkspace workspace)
        {
            if (workspace == null)
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
        ///     Deletes the specified workspace from storage.
        /// </summary>
        /// <param name="workspace">The workspace to be deleted.</param>
        public void Delete(IWorkspace workspace)
        {
            if (workspace == null)
            {
                return;
            }

            IWorkspace result;
            _items.TryRemove(workspace.ID, out result);
            Delete(workspace.ID);
        }

        #endregion

        #region File Handling

        #region Read

        private IWorkspace Read(Guid workdspaceID)
        {
            // force a lock on the file system ;)
            lock (WorkspaceLock)
            {
                string filePath = GetFileName(workdspaceID);
                bool fileExists = File.Exists(filePath);
                using (FileStream stream = File.Open(filePath, FileMode.OpenOrCreate))
                {
                    var formatter = new BinaryFormatter();
                    if (fileExists)
                    {
                        try
                        {
                            return (IWorkspace) formatter.Deserialize(stream);
                        }
                            // ReSharper disable EmptyGeneralCatchClause 
                        catch (Exception ex)
                            // ReSharper restore EmptyGeneralCatchClause
                        {
                            Dev2Logger.Log.Error(ex);
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

        private void Write(IWorkspace workspace)
        {
            if (workspace == null)
            {
                return;
            }

            string filePath = GetFileName(workspace.ID);
            using (FileStream stream = File.Open(filePath, FileMode.OpenOrCreate))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, workspace);
            }
        }

        #endregion

        #region Delete

        private void Delete(Guid workspaceID)
        {
            string filePath = GetFileName(workspaceID);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        #endregion

        #region GetFileName

        private string GetFileName(Guid workspaceID)
        {
            return Path.Combine(EnvironmentVariables.WorkspacePath, workspaceID + ".uws");
        }

        #endregion

        // TODO: Refactor file serialization handling into separate testable class

        private static string GetUserMapFileName()
        {
            return Path.Combine(EnvironmentVariables.WorkspacePath, "workspaces.uws");
        }

        private static ConcurrentDictionary<string, Guid> ReadUserMap()
        {
            // force a lock on the file system ;)
            lock (UserMapLock)
            {
                string filePath = GetUserMapFileName();
                bool fileExists = File.Exists(filePath);
                using (FileStream stream = File.Open(filePath, FileMode.OpenOrCreate))
                {
                    var formatter = new BinaryFormatter();
                    if (fileExists)
                    {
                        try
                        {
                            return (ConcurrentDictionary<string, Guid>) formatter.Deserialize(stream);
                        }
                            // ReSharper disable EmptyGeneralCatchClause 
                        catch (Exception ex)
                            // ReSharper restore EmptyGeneralCatchClause
                        {
                            Dev2Logger.Log.Error("WorkspaceRepository", ex);
                            // Deserialization failed so overwrite with new one.
                        }
                    }

                    var result = new ConcurrentDictionary<string, Guid>();
                    formatter.Serialize(stream, result);
                    return result;
                }
            }
        }

        private static void WriteUserMap(ConcurrentDictionary<string, Guid> userMap)
        {
            string filePath = GetUserMapFileName();
            using (FileStream stream = File.Open(filePath, FileMode.OpenOrCreate))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, userMap);
            }
        }

        #endregion

        public ICollection<Guid> GetWorkspaceGuids()
        {
            return _items.Keys;
        }
    }
}