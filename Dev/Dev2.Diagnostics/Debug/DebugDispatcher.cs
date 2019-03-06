/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Logging;
using Newtonsoft.Json;

namespace Dev2.Diagnostics.Debug
{
    /**
     * Uses EsbHub to write DebugState data from Warewolf Server to the Studio. This state data
     * is received by the Studio as events in ServerProxyWithoutChunking as a SendDebugState event
     */
    internal class DebugDispatcherImplementation : IDebugDispatcher
    {
        readonly ILogger _dev2Logger;
        public DebugDispatcherImplementation()
            : this(new DefaultLogger())
        { }
        public DebugDispatcherImplementation(ILogger logger)
        {
            _dev2Logger = logger;
        }
        readonly ConcurrentDictionary<Guid, IDebugWriter> _writers = new ConcurrentDictionary<Guid, IDebugWriter>();
        bool _shutdownRequested;
        static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
        };

        public int Count => _writers.Count;

        public void Add(Guid workspaceId, IDebugWriter writer)
        {
            if (writer == null || _shutdownRequested)
            {
                return;
            }
            _writers.TryAdd(workspaceId, writer);
        }


        public void Remove(Guid workspaceId)
        {
            _writers.TryRemove(workspaceId, out IDebugWriter writer);
        }

        /// <summary>
        /// Gets the writer for the given workspace ID.
        /// </summary>
        /// <param name="workspaceId">The workspace ID to be queried.</param>
        /// <returns>The <see cref="IDebugWriter"/> with the specified ID, or <code>null</code> if not found.</returns>
        public IDebugWriter Get(Guid workspaceId)
        {
            _writers.TryGetValue(workspaceId, out IDebugWriter writer);
            return writer;
        }

        public void Shutdown()
        {
            _shutdownRequested = true;
        }

        public void Write(IDebugState debugState) => Write(debugState, false, false, "", false, null, null, null);
        public void Write(IDebugState debugState, bool isTestExecution, bool isDebugFromWeb, string testName) => Write(debugState, isTestExecution, isDebugFromWeb, testName, false, null, null, null);
        public void Write(IDebugState debugState, bool isTestExecution, bool isDebugFromWeb, string testName, bool isRemoteInvoke, string remoteInvokerId) => Write(debugState, isTestExecution, isDebugFromWeb, testName, isRemoteInvoke, remoteInvokerId, null, null);
        public void Write(IDebugState debugState, bool isTestExecution, bool isDebugFromWeb, string testName, bool isRemoteInvoke, string remoteInvokerId, string parentInstanceId, IList<IDebugState> remoteDebugItems)
        {
            if (debugState == null)
            {
                return;
            }

            if (isTestExecution)
            {
                TestDebugMessageRepo.Instance.AddDebugItem(debugState.SourceResourceID, testName, debugState);
                return;
            }
            if (isDebugFromWeb)
            {
                WebDebugMessageRepo.Instance.AddDebugItem(debugState.ClientID, debugState.SessionID, debugState);
                return;
            }


            if (isRemoteInvoke)
            {
                RemoteDebugMessageRepo.Instance.AddDebugItem(remoteInvokerId, debugState);
                return;
            }

            if (remoteDebugItems != null)
            {
                Guid.TryParse(parentInstanceId, out Guid parentId);
                foreach (var item in remoteDebugItems)
                {
                    item.WorkspaceID = debugState.WorkspaceID;
                    item.OriginatingResourceID = debugState.OriginatingResourceID;
                    item.ClientID = debugState.ClientID;
                    if (Guid.TryParse(remoteInvokerId, out Guid remoteEnvironmentId))
                    {
                        item.EnvironmentID = remoteEnvironmentId;
                    }
                    if (item.ParentID == Guid.Empty)
                    {
                        item.ParentID = parentId;
                    }
                    QueueWrite(item);
                }

                remoteDebugItems.Clear();
            }
            _dev2Logger.Debug($"EnvironmentID: {debugState.EnvironmentID} Debug:{debugState.DisplayName}", GlobalConstants.WarewolfDebug);
            QueueWrite(debugState);

            if (debugState.IsFinalStep())
            {
                IDebugWriter writer;
                if ((writer = Get(debugState.WorkspaceID)) != null)
                {
                    var allDebugStates = DebugMessageRepo.Instance.FetchDebugItems(debugState.ClientID, debugState.SessionID);
                    foreach (var state in allDebugStates)
                    {
                        var serializeObject = JsonConvert.SerializeObject(state, SerializerSettings);
                        writer.Write(serializeObject);
                    }
                }
            }
        }

        static void QueueWrite(IDebugState debugState)
        {
            if (debugState != null)
            {
                DebugMessageRepo.Instance.AddDebugItem(debugState.ClientID, debugState.SessionID, debugState);

            }
        }
    }

    public static class DebugDispatcher
    {
        static IDebugDispatcher _instance;
        static readonly object _lock = new object();
        public static IDebugDispatcher Instance
        {
            get
            {
                if (_instance is null)
                {
                    lock (_lock)
                    {
                        if (_instance is null)
                        {
                            _instance = new DebugDispatcherImplementation();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
