/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Newtonsoft.Json;

namespace Dev2.Diagnostics.Debug
{
    public class DebugDispatcher : IDebugDispatcher
    {
        private readonly ConcurrentDictionary<Guid, IDebugWriter> _writers = new ConcurrentDictionary<Guid, IDebugWriter>();
        private static bool _shutdownRequested;
        static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
        };

        static DebugDispatcher _instance;
        public static DebugDispatcher Instance => _instance ?? (_instance = new DebugDispatcher());

        static DebugDispatcher()
        {
        }

        // Prevent instantiation
        DebugDispatcher()
        {

        }
        /// <summary>
        /// Gets the number of writers.
        /// </summary>
        public int Count => _writers.Count;

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
        

        /// <summary>
        /// Removes the specified workspace from the dispatcher.
        /// </summary>
        /// <param name="workspaceId">The ID of workspace to be removed.</param>
        public void Remove(Guid workspaceId)
        {
            IDebugWriter writer;
            _writers.TryRemove(workspaceId, out writer);
        }
        
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

        public void Shutdown()
        {
            _shutdownRequested = true;           
        }
        
        public void Write(IDebugState debugState,bool isTestExecution,string testName, bool isRemoteInvoke = false, string remoteInvokerId = null, string parentInstanceId = null, IList<IDebugState> remoteDebugItems = null)
        {
            if(debugState == null)
            {
                return;
            }

            if (isTestExecution)
            {
                TestDebugMessageRepo.Instance.AddDebugItem(debugState.SourceResourceID,testName,debugState);
                return;
            }

            
            if(isRemoteInvoke)
            {
                RemoteDebugMessageRepo.Instance.AddDebugItem(remoteInvokerId, debugState);
                return;
            }
            
            if(remoteDebugItems != null)
            {
                Guid parentId;
                Guid.TryParse(parentInstanceId, out parentId);
                foreach(var item in remoteDebugItems)
                {
                    item.WorkspaceID = debugState.WorkspaceID;
                    item.OriginatingResourceID = debugState.OriginatingResourceID;
                    item.ClientID = debugState.ClientID;
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

            if (debugState.IsFinalStep())
            {
                IDebugWriter writer;
                if ((writer = Instance.Get(debugState.WorkspaceID)) != null)
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
            if(debugState != null)
            {
                DebugMessageRepo.Instance.AddDebugItem(debugState.ClientID,debugState.SessionID,debugState);

            }
        }
        
    }
}
