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
using System.Collections.Generic;
using Dev2.Common;

namespace Warewolf.Auditing
{
    public class StateNotifierFactory : IStateNotifierFactory
    {
        public IStateNotifier New(IExecutionContext dataObject)
        {
            if (Config.Server.EnableDetailedLogging)
            {
                var stateNotifier = new StateNotifier();
                var listener = new StateAuditLogger(new WebSocketPool());
                stateNotifier.Subscribe(listener.NewStateListener(dataObject));
                return stateNotifier;
            }

            return null;
        }
    }
    public class StateNotifier : IStateNotifier
    {
        public void Dispose()
        {
            foreach (var listener in _stateListeners)
            {
                if (listener is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public void LogAdditionalDetail(object detail, string callerName)
        {
            foreach (var stateListener in _stateListeners)
            {
                stateListener.LogAdditionalDetail(detail, callerName);
            }
        }

        public void LogExecuteCompleteState(object activity)
        {
            foreach (var stateListener in _stateListeners)
            {
                stateListener.LogExecuteCompleteState(activity);
            }
        }

        public void LogExecuteException(Exception e, object activity)
        {
            foreach (var stateListener in _stateListeners)
            {
                stateListener.LogExecuteException(e, activity);
            }
        }

        public void LogStopExecutionState(object activity)
        {
            foreach (var stateListener in _stateListeners)
            {
                stateListener.LogStopExecutionState(activity);
            }
        }

        public void LogActivityExecuteState(object nextActivityObject)
        {
            foreach (var stateListener in _stateListeners)
            {
                stateListener.LogActivityExecuteState(nextActivityObject);
            }
        }

        readonly IList<IStateListener> _stateListeners = new List<IStateListener>();
        public void Subscribe(IStateListener listener)
        {
            Console.WriteLine("_stateListeners.Add    " + _stateListeners.Count);
            _stateListeners.Add(listener);
        }
    }
}
