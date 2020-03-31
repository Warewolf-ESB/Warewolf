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

        public void Write(WriteArgs writeArgs)
        {
            if (writeArgs.debugState == null)
            {
                return;
            }

            if (writeArgs.isTestExecution)
            {
                TestDebugMessageRepo.Instance.AddDebugItem(writeArgs.debugState.SourceResourceID, writeArgs.testName, writeArgs.debugState);
                return;
            }
            if (writeArgs.isDebugFromWeb)
            {
                WebDebugMessageRepo.Instance.AddDebugItem(writeArgs.debugState.ClientID, writeArgs.debugState.SessionID, writeArgs.debugState);
                return;
            }

            if (writeArgs.isRemoteInvoke)
            {
                RemoteDebugMessageRepo.Instance.AddDebugItem(writeArgs.remoteInvokerId, writeArgs.debugState);
                return;
            }

            if (writeArgs.remoteDebugItems != null)
            {
                SetRemoteDebugItems(writeArgs);
            }

            _dev2Logger.Debug($"EnvironmentID: {writeArgs.debugState.EnvironmentID} Debug:{writeArgs.debugState.DisplayName}", GlobalConstants.WarewolfDebug);

            if (writeArgs.debugState != null)
            {
                DebugMessageRepo.Instance.AddDebugItem(writeArgs.debugState.ClientID, writeArgs.debugState.SessionID, writeArgs.debugState);
            }

            DebugStateFinalStep(writeArgs);
        }

        private static void SetRemoteDebugItems(WriteArgs writeArgs)
        {
            Guid.TryParse(writeArgs.parentInstanceId, out var parentId);
            foreach (var item in writeArgs.remoteDebugItems)
            {
                item.WorkspaceID = writeArgs.debugState.WorkspaceID;
                item.OriginatingResourceID = writeArgs.debugState.OriginatingResourceID;
                item.ClientID = writeArgs.debugState.ClientID;
                if (Guid.TryParse(writeArgs.remoteInvokerId, out var remoteEnvironmentId))
                {
                    item.EnvironmentID = remoteEnvironmentId;
                }
                if (item.ParentID == Guid.Empty)
                {
                    item.ParentID = parentId;
                }
                if (item != null)
                {
                    DebugMessageRepo.Instance.AddDebugItem(item.ClientID, item.SessionID, item);
                }
            }

            writeArgs.remoteDebugItems.Clear();
        }

        // TODO: fix this, we should not be storing debug items until the execution is completed before sending them to the studio
        private void DebugStateFinalStep(WriteArgs writeArgs)
        {
            if (writeArgs.debugState.IsFinalStep())
            {
                IDebugWriter writer;
                if ((writer = Get(writeArgs.debugState.WorkspaceID)) != null)
                {
                    var allDebugStates = DebugMessageRepo.Instance.FetchDebugItems(writeArgs.debugState.ClientID, writeArgs.debugState.SessionID);
                    foreach (var state in allDebugStates)
                    {
                        var serializeObject = JsonConvert.SerializeObject(state, SerializerSettings);
                        writer.Write(serializeObject);
                    }
                }
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
