using System;
using System.Collections.Generic;

namespace Dev2.Studio.Core {
    /// <summary>
    /// This class is an implementation detail of the Mediator class.
    /// </summary>
    internal class MessageToActionsMap {
        #region Fields

        private readonly Dictionary<MediatorMessages, List<WeakAction>> _map;
        //private readonly Dictionary<string, WeakAction> _suspendMap;
        private readonly Dictionary<string,WeakAction> _callbackLookup;
        private int _idx = 0;

        #endregion // Fields

        #region Constructor

        internal MessageToActionsMap() {
            _map = new Dictionary<MediatorMessages, List<WeakAction>>();
            _callbackLookup = new Dictionary<string, WeakAction>();
        }

        #endregion // Constructor

        #region Methods

        internal string AddAction(MediatorMessages message, Action<object> callback) {
            string result = string.Empty;

            if (callback == null)
                throw new ArgumentNullException("callback");

            if (!_map.ContainsKey(message))
                _map[message] = new List<WeakAction>();

            WeakAction wa = new WeakAction(callback);
            _map[message].Add(wa);

            string key = message.ToString() + _idx;
            _idx++;

            _callbackLookup[key] = wa;
            result = key;
            return result;
        }

        internal void RemoveAction(MediatorMessages message , string key) {
            try {
                WeakAction wa = _callbackLookup[key];
                _map[message].Remove(wa);                
            }
            catch (Exception) {

            }
        }

        internal void RemoveAllActionsForMessage(MediatorMessages message) {
            try {
                IList<string> mediatorCallbackMaps = new List<string>();

                foreach (string mediatorMessage in _callbackLookup.Keys) {
                    if (mediatorMessage.Contains(message.ToString())) {
                        mediatorCallbackMaps.Add(mediatorMessage);
                        WeakAction action = _callbackLookup[mediatorMessage];

                        List<WeakAction> actions;
                        if (_map.TryGetValue(message, out actions))
                        {
                            actions.Remove(action);
                        }
                    }
                }
                foreach (string mediatorMessageCallBackMap in mediatorCallbackMaps) {
                    _callbackLookup.Remove(mediatorMessageCallBackMap);
                }
            }
            catch (Exception ex) {
                throw ex;
            }
        }


        //internal void SuspendAction(MediatorMessages message, string key) {
        //    try {
        //        WeakAction wa = _callbackLookup[key];
        //        _map[message].Remove(wa);
        //        _suspendMap[key] = wa;
        //    }
        //    catch (Exception) {

        //    }
        //}

        //internal void ReinstateAction(MediatorMessages message, string key) {
        //    try {
        //        WeakAction wa = _suspendMap[key];
        //        _map[message].Add(wa);
        //        _suspendMap[key] = null;
        //    }
        //    catch (Exception) {

        //    }
        //}

        internal List<Action<object>> GetActions(MediatorMessages message) {

            if (!_map.ContainsKey(message))
                return null;

            List<WeakAction> weakActions = _map[message];
            List<Action<object>> actions = new List<Action<object>>();
            for (int i = weakActions.Count - 1; i > -1; --i) {
                WeakAction weakAction = weakActions[i];
                if (!weakAction.IsAlive)
                    weakActions.RemoveAt(i);
                else
                    actions.Add(weakAction.CreateAction());
            }

            this.RemoveMessageIfNecessary(weakActions, message);

            return actions;
        }

        void RemoveMessageIfNecessary(List<WeakAction> weakActions, MediatorMessages message) {
            if (weakActions.Count == 0)
                _map.Remove(message);
        }

        #endregion // Methods
    }
}
