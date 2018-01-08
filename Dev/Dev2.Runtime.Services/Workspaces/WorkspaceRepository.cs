/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Runtime.Interfaces;



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
        public static readonly string ServerWorkspacePath = EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID);

        readonly ConcurrentDictionary<string, Guid> _userMap;
        readonly ConcurrentDictionary<Guid, IWorkspace> _items = new ConcurrentDictionary<Guid, IWorkspace>();
        readonly IResourceCatalog _resourceCatalog;

        static readonly object WorkspaceLock = new object();
        static readonly object UserMapLock = new object();

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
        readonly object _readLock = new object();

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

        public WorkspaceRepository()
            : this(ResourceCatalog.Instance)
        {
        }

        // PBI 9363 - 2013.05.29 - TWR: Added for testing 
        public WorkspaceRepository(IResourceCatalog resourceCatalog)
        {
            if(resourceCatalog == null)
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
        /// Gets the number of items in the repository.
        /// </summary>
        public int Count => _items.Count;

        #endregion

        #region ServerWorkspace

        /// <summary>
        /// Gets the server workspace.
        /// </summary>
        public IWorkspace ServerWorkspace => Get(ServerWorkspaceID);

        #endregion

        #endregion

        public Guid GetWorkspaceID(WindowsIdentity identity)
        {
            Guid workspaceID;
            try
            {
                var userID = identity.Name;
                if (identity.User != null)
                {
                    userID = identity.User.Value;
                }
                
                if (!_userMap.TryGetValue(userID, out workspaceID))
                {
                    workspaceID = Guid.NewGuid();
                    _userMap.TryAdd(userID, workspaceID);
                    WriteUserMap(_userMap);
                }
            }
            catch (Exception ex)
            {
                workspaceID = ServerWorkspaceID;
                Dev2Logger.Error(ex.Message, workspaceID.ToString());
            }
            return workspaceID;
        }

        #region Get

        public IWorkspace Get(Guid workspaceID) => Get(workspaceID, false, true);

        public IWorkspace Get(Guid workspaceID, bool force) => Get(workspaceID, force, true);

        public IWorkspace Get(Guid workspaceID, bool force, bool loadResources)
        {
            lock(_readLock)
            {
                // PBI 9363 - 2013.05.29 - TWR: Added loadResources parameter
                if (force || !_items.TryGetValue(workspaceID, out IWorkspace workspace))
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
        /// Refreshes all workspaces from storage.
        /// </summary>
        public void RefreshWorkspaces()
        {
            //TODO, 2012-10-24, This is a temporary implementation which brute force 
            //                  refreshes all client workspaces by removing them,
            //                  no changes in the client workspaces are preserved.
            var worksSpacesToRemove = _items.Keys.Where(k => k != ServerWorkspaceID).ToList();
            foreach (Guid workspaceGuid in worksSpacesToRemove)
            {
                _items.TryRemove(workspaceGuid, out IWorkspace workspace);
            }
        }

        #endregion

        #region GetLatest
        
        public void GetLatest(IWorkspace workspace, IList<string> servicesToIgnore)
        {
            lock(_readLock)
            {
                
                var filesToIgnore = servicesToIgnore.Select(s => s += ".bite").ToList();
                
                var targetPath = EnvironmentVariables.GetWorkspacePath(workspace.ID);
                _resourceCatalog.SyncTo(ServerWorkspacePath, targetPath, true, true, filesToIgnore);
            }
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

            _items.TryRemove(workspace.ID, out IWorkspace result);
            Delete(workspace.ID);
        }

        #endregion

        #region File Handling

        // TODO: Refactor file serialization handling into separate testable class

        #region Read

        IWorkspace Read(Guid workdspaceID)
        {
            // force a lock on the file system ;)
            lock (WorkspaceLock)
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
                            return (IWorkspace)formatter.Deserialize(stream);
                        }

                        catch (Exception ex)

                        {
                            Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
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
            return Path.Combine(EnvironmentVariables.WorkspacePath, workspaceID + ".bite");
        }

        #endregion

        static string GetUserMapFileName()
        {
            return Path.Combine(EnvironmentVariables.WorkspacePath, "workspaces.bite");
        }

        static ConcurrentDictionary<string, Guid> ReadUserMap()
        {
            // force a lock on the file system ;)
            lock(UserMapLock)
            {
                var filePath = GetUserMapFileName();
                var fileExists = File.Exists(filePath);
                using(var stream = File.Open(filePath, FileMode.OpenOrCreate))
                {
                    var formatter = new BinaryFormatter();
                    if(fileExists)
                    {
                        try
                        {
                            return (ConcurrentDictionary<string, Guid>)formatter.Deserialize(stream);
                        }
                         
                        catch(Exception ex)
                        
                        {
                            Dev2Logger.Error("WorkspaceRepository", ex, GlobalConstants.WarewolfError);
                            // Deserialization failed so overwrite with new one.
                        }
                    }

                    var result = new ConcurrentDictionary<string, Guid>();
                    formatter.Serialize(stream, result);
                    return result;
                }
            }
        }

        static void WriteUserMap(ConcurrentDictionary<string, Guid> userMap)
        {
            var filePath = GetUserMapFileName();
            using(var stream = File.Open(filePath, FileMode.OpenOrCreate))
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
