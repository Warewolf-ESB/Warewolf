using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Dev2.Studio.Core;
using System.Threading;

namespace Dev2.Studio {
    [Export(typeof(IMediatorRepo))]
    public class MediatorRepository : IMediatorRepo {

        private readonly ThreadLocal<IDictionary<string, IList<string>>> _keyCacheThreadLocal = new ThreadLocal<IDictionary<string, IList<string>>>();
        private readonly ThreadLocal<IDictionary<int, IList<MediatorMessages>>> _itemHashToMessageCacheThreadLocal = new ThreadLocal<IDictionary<int, IList<MediatorMessages>>>();

        private IDictionary<string, IList<string>> _keyCache
        {
            get
            {
                if (!_keyCacheThreadLocal.IsValueCreated)
                {
                    _keyCacheThreadLocal.Value = new Dictionary<string, IList<string>>();
                }

                return _keyCacheThreadLocal.Value;
            }
        }

        private IDictionary<int, IList<MediatorMessages>> _itemHashToMessageCache
        {
            get
            {
                if (!_itemHashToMessageCacheThreadLocal.IsValueCreated)
                {
                    _itemHashToMessageCacheThreadLocal.Value = new Dictionary<int, IList<MediatorMessages>>();
                }

                return _itemHashToMessageCacheThreadLocal.Value;
            }
        }

        private string generateKey(int hashCode, MediatorMessages msg) {
            return hashCode + "_" + msg;
        }

        public void addKeyList(int itemHashCode, MediatorMessages msg, IList<string> keyList) {
            string regKey = generateKey(itemHashCode, msg);

            IList<string> keys;

            if (!_keyCache.TryGetValue(regKey, out keys))
            {
                keys = new List<string>();
                _keyCache.Add(regKey, keys);
            }

            // add all keys
            keyList
            .ToList()
            .ForEach(key => {
                keys.Add(key);
            });

            // place into hash to message repo

            IList<MediatorMessages> mediatorMessages;
            if (_itemHashToMessageCache.TryGetValue(itemHashCode, out mediatorMessages))
            {
                if (!mediatorMessages.Contains(msg))
                {
                    mediatorMessages.Add(msg);
                }
            }
        }

        public void addKey(int itemHashCode, MediatorMessages msg, string key) {
            string regKey = generateKey(itemHashCode, msg);

            IList<string> keys;
            if (!_keyCache.TryGetValue(regKey, out keys))
            {
                keys = new List<string>();
                _keyCache.Add(regKey, keys);
            }

            keys.Add(key);

            // add to key to msg registery for this item
            IList<MediatorMessages> keys2;
            if (!_itemHashToMessageCache.TryGetValue(itemHashCode, out keys2))
            {
                keys2 = new List<MediatorMessages>();
                _itemHashToMessageCache.Add(itemHashCode, keys2);
            }

            keys2.Add(msg);
        }

        public void deregisterAllItemMessages(int itemHashCode) {
            IList<MediatorMessages> mediatorMessages;
            if (_itemHashToMessageCache.TryGetValue(itemHashCode, out mediatorMessages))
            {
                mediatorMessages.ToList()
                    .ForEach(msg => {
                    string regKey = generateKey(itemHashCode, msg);

                    IList<string> keys;
                    if (_keyCache.TryGetValue(regKey, out keys))
                    {
                        keys.ToList()
                            .ForEach(key =>
                            {
                                Mediator.DeRegister(msg, key);

                            });

                        _keyCache[regKey] = new List<string>();
                    }
                });

                // remove msg to itemHash
                _itemHashToMessageCache[itemHashCode] = new List<MediatorMessages>();
            }
        }

        public void deregisterItemMessage(int itemHashCode, MediatorMessages msg) {
            string regKey = generateKey(itemHashCode, msg);
            IList<string> keys;// = _keyCache[regKey];

            if (_keyCache.TryGetValue(regKey, out keys))
            {
                keys
                    .ToList()
                    .ForEach(key =>
                    {
                        Mediator.DeRegister(msg, key);
                    });
            }

            // remove msg from itemHash
            IList<MediatorMessages> mediatorMessages;
            if (_itemHashToMessageCache.TryGetValue(itemHashCode, out mediatorMessages))
            {
                mediatorMessages.Remove(msg);
            }
        }

        public void suspendAllItemMessages(int itemHashCode) {
            // suspend all messages for all keys
            IList<MediatorMessages> mediatorMessages;
            if (_itemHashToMessageCache.TryGetValue(itemHashCode, out mediatorMessages))
            {
                mediatorMessages
                    .ToList()
                    .ForEach(msg =>
                    {
                        string regKey = generateKey(itemHashCode, msg);

                        IList<string> keys;// = _keyCache[regKey];
                        if (_keyCache.TryGetValue(regKey, out keys))
                        {
                            keys
                                .ToList()
                                .ForEach(key =>
                                {
                                    Mediator.SuspendRegistration(msg, key);

                                });
                        }
                    });
            }
        }

        public void reregisterAllItemMessages(int itemHashCode) {
            // reinstate all messages for all keys
            IList<MediatorMessages> mediatorMessages;
            if (_itemHashToMessageCache.TryGetValue(itemHashCode, out mediatorMessages))
            {
                mediatorMessages
                    .ToList()
                    .ForEach(msg =>
                    {
                        string regKey = generateKey(itemHashCode, msg);

                        IList<string> keys;// = _keyCache[regKey];
                        if (_keyCache.TryGetValue(regKey, out keys))
                        {
                            keys
                                .ToList()
                                .ForEach(key =>
                                {
                                    Mediator.ReinstateRegistration(msg, key);

                                });
                        }
                    });
            }
        }
    }
}
