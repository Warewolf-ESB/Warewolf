using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization.Formatters.Binary;
using System.Timers;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Data.ServiceModel.Messages;

namespace Dev2.Runtime.Hosting
{
    /// <summary>
    /// Used to store compile time message ;)
    /// </summary>
    public class CompileMessageRepo : IDisposable
    {
        // used for storing message about resources ;) 
        readonly IDictionary<Guid, IList<ICompileMessageTO>> _messageRepo = new Dictionary<Guid, IList<ICompileMessageTO>>();
        static Subject<IList<ICompileMessageTO>> _allMessages = new Subject<IList<ICompileMessageTO>>();
        private static readonly object Lock = new object();
        private static bool _changes;
        private static readonly Timer PersistTimer = new Timer(1000 * 5); // wait 5 seconds to fire ;)

        /// <summary>
        /// Gets or sets the persistence path.
        /// </summary>
        /// <value>
        /// The persistence path.
        /// </value>
        public string PersistencePath { get; private set; }

        private static CompileMessageRepo _instance;
        public static CompileMessageRepo Instance
        {
            get
            {
                return _instance ?? (_instance = new CompileMessageRepo());
            }
        }

        public CompileMessageRepo()
            : this(null, false)
        {
        }

        public CompileMessageRepo(string persistPath, bool activateBackgroundWorker = true)
        {
            if(persistPath != null)
            {
                PersistencePath = persistPath;
            }

            var path = PersistencePath;

            if(string.IsNullOrEmpty(PersistencePath))
            {
                path = Path.Combine(EnvironmentVariables.RootPersistencePath, "CompileMessages");
            }

            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Hydrate from disk ;)
            HydrateFromDisk(path);

            if(activateBackgroundWorker)
            {
                // Init Persistence ;)
                InitPersistence(path);
            }

        }

        public bool Ping()
        {
            return true;
        }

        public int MessageCount(Guid wId)
        {
            IList<ICompileMessageTO> messages;

            if(_messageRepo.TryGetValue(wId, out messages))
            {
                return messages.Count;
            }

            return -1;
        }

        #region Private Methods

        /// <summary>
        /// Hydrates from disk.
        /// </summary>
        /// <param name="path">The path.</param>
        private void HydrateFromDisk(string path)
        {
            lock(Lock)
            {
                try
                {
                    var files = Directory.GetFiles(path);

                    foreach(var f in files)
                    {
                        var fname = Path.GetFileName(f);
                        if(fname != null)
                        {
                            fname = fname.Replace(".msg", "");
                            BinaryFormatter bf = new BinaryFormatter();
                            using(Stream s = new FileStream(f, FileMode.OpenOrCreate))
                            {
                                try
                                {
                                    object obj = bf.Deserialize(s);

                                    var listOf = (obj as IList<ICompileMessageTO>);

                                    if(listOf != null)
                                    {
                                        Guid id;
                                        if(Guid.TryParse(fname, out id))
                                        {
                                            _messageRepo[id] = listOf;
                                            _allMessages.OnNext(listOf);
                                        }
                                        else
                                        {
                                            this.LogError("Failed to parse message ID");
                                        }
                                    }
                                }
                                catch(Exception e)
                                {
                                    this.LogError(e);
                                }
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    this.LogError(e);
                }
            }
        }




        /// <summary>
        /// Inits the persistence.
        /// </summary>
        /// <param name="path">The path.</param>
        private void InitPersistence(string path)
        {
            PersistTimer.Interval = 1000 * 5; // every 5 seconds
            PersistTimer.Enabled = true;
            PersistTimer.Elapsed += (sender, args) => Persist(path);
        }

        #endregion

        #region Public Methods


        /// <summary>
        /// Persists the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void Persist(string path)
        {
            if(_changes)
            {
                lock(Lock)
                {
                    try
                    {
                        // Persistence work ;)
                        var keys = _messageRepo.Keys;
                        foreach(var k in keys)
                        {
                            IList<ICompileMessageTO> val;
                            if(_messageRepo.TryGetValue(k, out val))
                            {
                                var pPath = Path.Combine(path, k + ".msg");
                                BinaryFormatter bf = new BinaryFormatter();
                                using(Stream s = new FileStream(pPath, FileMode.OpenOrCreate))
                                {
                                    bf.Serialize(s, val);
                                }
                            }
                        }

                        _changes = false;
                    }
                    catch(Exception e)
                    {
                        this.LogError(e);
                    }
                }
            }
        }


        /// <summary>
        /// Adds the message.
        /// </summary>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="msgs">The MSGS.</param>
        /// <returns></returns>
        public bool AddMessage(Guid workspaceId, IList<ICompileMessageTO> msgs)
        {
            if(msgs.Count == 0)
            {
                return true;
            }
            lock(Lock)
            {
                IList<ICompileMessageTO> messages;
                if(!_messageRepo.TryGetValue(workspaceId, out messages))
                {
                    messages = new List<ICompileMessageTO>();
                }

                // clean up any messages with the same id and add

                for(int i = (messages.Count - 1); i >= 0; i--)
                {
                    messages.Remove(messages[i]);
                }

                // now add new messages ;)
                foreach(var msg in msgs)
                {
                    messages.Add(msg);
                }
                _allMessages.OnNext(messages);
                _messageRepo[workspaceId] = messages;

                _changes = true;
            }

            return true;
        }

        /// <summary>
        /// Removes the message.
        /// </summary>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="serviceId">The service ID.</param>
        /// <returns></returns>
        public bool RemoveMessages(Guid workspaceId, Guid serviceId)
        {
            lock(Lock)
            {
                IList<ICompileMessageTO> messages;
                if(_messageRepo.TryGetValue(workspaceId, out messages))
                {
                    var candidateMessage = messages.Where(c => c.ServiceID == serviceId);

                    var compileMessageTos = candidateMessage as IList<ICompileMessageTO> ?? candidateMessage.ToList();
                    foreach(var msg in compileMessageTos)
                    {
                        messages.Remove(msg);
                    }

                    return (compileMessageTos.Count > 0);
                }
            }

            return false;
        }

        /// <summary>
        /// Fetches the messages.
        /// </summary>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="serviceId">The service ID.</param>
        /// <param name="deps">The deps.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public CompileMessageList FetchMessages(Guid workspaceId, Guid serviceId, IList<IResourceForTree> deps, CompileMessageType[] filter = null)
        {
            IList<ICompileMessageTO> result = new List<ICompileMessageTO>();

            lock(Lock)
            {
                IList<ICompileMessageTO> messages;
                if(_messageRepo.TryGetValue(workspaceId, out messages))
                {
                    // Fetch dep list and process ;)
                    if(deps != null)
                    {
                        foreach(var d in deps)
                        {
                            IResourceForTree d1 = d;
                            var candidateMessage = messages.Where(c => c.ServiceID == d1.ResourceID);
                            var compileMessageTos = candidateMessage as IList<ICompileMessageTO> ??
                                                    candidateMessage.ToList();

                            foreach(var msg in compileMessageTos)
                            {
                                if(filter != null)
                                {
                                    // TODO : Apply filter logic ;)
                                }
                                else
                                {
                                    // Adjust unique id for return so design surface understands where message goes ;)
                                    var tmpMsg = msg.Clone();
                                    tmpMsg.UniqueID = d1.UniqueID;
                                    result.Add(tmpMsg);
                                }
                            }
                        }
                    }
                }
            }

            return new CompileMessageList { MessageList = result, ServiceID = serviceId };
        }

        public CompileMessageList FetchMessages(Guid workspaceId, Guid serviceId, IList<string> dependants, CompileMessageType[] filter = null)
        {
            IList<ICompileMessageTO> result = new List<ICompileMessageTO>();

            lock(Lock)
            {
                IList<ICompileMessageTO> messages;
                if(_messageRepo.TryGetValue(workspaceId, out messages))
                {

                    var candidateMessage = messages.Where(c => c.ServiceID == serviceId);
                    var compileMessageTos = candidateMessage as IList<ICompileMessageTO> ??
                                            candidateMessage.ToList();

                    foreach(var msg in compileMessageTos)
                    {
                        if(filter != null)
                        {
                            // TODO : Apply filter logic ;)
                        }
                        else
                        {
                            result.Add(msg);
                        }
                    }
                }
            }
            var compileMessageList = new CompileMessageList { MessageList = result, ServiceID = serviceId, Dependants = dependants };
            RemoveMessages(workspaceId, serviceId);
            return compileMessageList;
        }

        public IObservable<IList<ICompileMessageTO>> AllMessages
        {
            get
            {
                return _allMessages;
            }
        }

        public void Dispose()
        {
            if(PersistTimer != null)
            {
                try
                {
                    PersistTimer.Close();
                }
                catch(Exception e)
                {
                    this.LogError(e);
                }
            }
        }

        #endregion

        public void ClearObservable()
        {
            _allMessages.OnCompleted();
            _allMessages = new Subject<IList<ICompileMessageTO>>();
        }
    }
}
