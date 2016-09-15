/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Newtonsoft.Json;

namespace Dev2.Diagnostics.Debug
{
    public class DebugDispatcher : IDebugDispatcher
    {
        // The Guid is the workspace ID of the writer
        private readonly ConcurrentDictionary<Guid, IDebugWriter> _writers = new ConcurrentDictionary<Guid, IDebugWriter>();
        private static readonly ConcurrentQueue<IDebugState> WriterQueue = new ConcurrentQueue<IDebugState>();
        private static readonly Thread WriterThread = new Thread(WriteLoop);
        private static readonly ManualResetEventSlim WriteWaithandle = new ManualResetEventSlim(false);
        private static readonly object WaitHandleGuard = new object();
        private static bool _shutdownRequested;
        static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
        };
        #region Singleton Instance

        static DebugDispatcher _instance;
        public static DebugDispatcher Instance => _instance ?? (_instance = new DebugDispatcher());

        #endregion

        #region Initialization

        static DebugDispatcher()
        {
            WriterThread.Start();
        }

        // Prevent instantiation
        DebugDispatcher()
        {

        }

        #endregion

        #region Properties

        #region Count

        /// <summary>
        /// Gets the number of writers.
        /// </summary>
        public int Count => _writers.Count;

        #endregion

        #endregion

        #region Add
        /// <summary>
        /// Adds the specified writer to the dispatcher.
        /// </summary>
        /// <param name="workspaceId">The ID of the workspace to which the writer belongs.</param>
        /// <param name="writer">The writer to be added.</param>
        public void Add(Guid workspaceId, IDebugWriter writer)
        {
            if (writer == null || _shutdownRequested)
            {
                return;
            }
            _writers.TryAdd(workspaceId, writer);
        }

        #endregion

        #region Remove

        /// <summary>
        /// Removes the specified workspace from the dispatcher.
        /// </summary>
        /// <param name="workspaceId">The ID of workspace to be removed.</param>
        public void Remove(Guid workspaceId)
        {
            IDebugWriter writer;
            _writers.TryRemove(workspaceId, out writer);
        }

        #endregion

        #region Get

        /// <summary>
        /// Gets the writer for the given workspace ID.
        /// </summary>
        /// <param name="workspaceId">The workspace ID to be queried.</param>
        /// <returns>The <see cref="IDebugWriter"/> with the specified ID, or <code>null</code> if not found.</returns>
        public IDebugWriter Get(Guid workspaceId)
        {
            IDebugWriter writer;
            _writers.TryGetValue(workspaceId, out writer);
            return writer;
        }

        #endregion

        #region Shutdown

        public void Shutdown()
        {
            _shutdownRequested = true;
            lock(WaitHandleGuard)
            {
                while(WriterQueue.Count > 0)
                {
                    IDebugState debugState;
                    WriterQueue.TryDequeue(out debugState);
                }
                WriteWaithandle.Set();
            }
        }

        #endregion Shutdown

        #region Write

        public void Write(IDebugState debugState,bool isTestExecution=false,string testName="", bool isRemoteInvoke = false, string remoteInvokerId = null, string parentInstanceId = null, IList<IDebugState> remoteDebugItems = null)
        {
            if(debugState == null)
            {
                return;
            }

            if (isTestExecution)
            {
                TestDebugMessageRepo.Instance.AddDebugItem(debugState.OriginatingResourceID,testName,debugState);
                return;
            }

            
            // Serialize debugState to a local repo so calling server can manage the data 
            if(isRemoteInvoke)
            {
                RemoteDebugMessageRepo.Instance.AddDebugItem(remoteInvokerId, debugState);
                return;
            }

            // local dispatch 
            // do we have any remote objects to dispatch locally? 
            if(remoteDebugItems != null)
            {
                Guid parentId;
                Guid.TryParse(parentInstanceId, out parentId);
                foreach(var item in remoteDebugItems)
                {
                    // re-jigger it so it will dispatch and display
                    item.WorkspaceID = debugState.WorkspaceID;
                    item.OriginatingResourceID = debugState.OriginatingResourceID;
                   // item.Server = remoteInvokerId;
                    Guid remoteEnvironmentId;
                    if(Guid.TryParse(remoteInvokerId, out remoteEnvironmentId))
                    {
                        item.EnvironmentID = remoteEnvironmentId;
                    }
                    if(item.ParentID == Guid.Empty)
                    {
                        item.ParentID = parentId;
                    }
                    QueueWrite(item);
                }

                remoteDebugItems.Clear();
            }
            Dev2Logger.Debug(string.Format("EnvironmentID: {0} Debug:{1}", debugState.EnvironmentID, debugState.DisplayName));
            QueueWrite(debugState);
        }

        public bool IsQueueEmpty => WriterQueue.IsEmpty;

        #endregion

        #region QueueWrite

        // BUG 9706 - 2013.06.22 - TWR : refactored
        static void QueueWrite(IDebugState debugState)
        {
            if(debugState != null)
            {
                lock(WaitHandleGuard)
                {
                    WriterQueue.Enqueue(debugState);
                    WriteWaithandle.Set();
                }
            }
        }

        #endregion

        #region WriteLoop

        private static void WriteLoop()
        {
            while(true)
            {
                WriteWaithandle.Wait();

                if(_shutdownRequested)
                {
                    return;
                }

                IDebugState debugState;
                if(WriterQueue.TryDequeue(out debugState))
                {
                    if(debugState != null)
                    {
                        IDebugWriter writer;
                        if((writer = Instance.Get(debugState.WorkspaceID)) != null)
                        {
                            var serializeObject = JsonConvert.SerializeObject(debugState, _serializerSettings);
                            writer.Write(serializeObject);
                        }
                    }
                }

                lock(WaitHandleGuard)
                {
                    if(WriterQueue.Count == 0)
                    {
                        WriteWaithandle.Reset();
                    }
                }
            }
        }

        #endregion WriteLoop
    }
}
