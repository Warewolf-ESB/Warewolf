using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Timers;
using Dev2.Common;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.Hosting
{
    /// <summary>
    /// Used to store compile time message ;)
    /// </summary>
    public class CompileMessageRepo : IDisposable
    {
        // used for storing message about resources ;) 
        readonly ConcurrentDictionary<Guid, IList<CompileMessageTO>> _messageRepo = new ConcurrentDictionary<Guid, IList<CompileMessageTO>>();

        private static object _lock = new object();
        private static bool _changes = false;
        private static readonly Timer _persistTimer = new Timer(1000*15); // wait 15 seconds to fire ;)

        /// <summary>
        /// Gets or sets the persistence path.
        /// </summary>
        /// <value>
        /// The persistence path.
        /// </value>
        public string PersistencePath { get; set; }

        private static CompileMessageRepo _instance;
        public static CompileMessageRepo Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new CompileMessageRepo();
                }

                return _instance;
            }
        }

        public CompileMessageRepo()
        {

            var path = PersistencePath;

            if (string.IsNullOrEmpty(PersistencePath))
            {
                path = Path.Combine(EnvironmentVariables.RootPersistencePath, "CompileMessages");    
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            // Hydrate from disk ;)
            HydrateFromDisk(path);

            // Init Persistence ;)
            InitPersistence(path);
           
        } 

        public bool Ping()
        {
            return true;
        }

        #region Private Methods

        /// <summary>
        /// Hydrates from disk.
        /// </summary>
        /// <param name="path">The path.</param>
        public void HydrateFromDisk(string path)
        {
            lock (_lock)
            {

                var files = Directory.GetFiles(path);

                foreach (var f in files)
                {
                    var fname = Path.GetFileName(f);
                    Guid id;
                    if (fname != null)
                    {
                        fname = fname.Replace(".msg","");
                        BinaryFormatter bf = new BinaryFormatter();
                        using (Stream s = new FileStream(f, FileMode.OpenOrCreate))
                        {
                            try
                            {
                                object obj = bf.Deserialize(s);

                                var listOf = (obj as IList<CompileMessageTO>);

                                if (listOf != null)
                                {
                                    if (Guid.TryParse(fname, out id))
                                    {
                                        _messageRepo[id] = listOf;
                                    }
                                    else
                                    {
                                        ServerLogger.LogError("Failed to parse message ID");
                                    }   
                                }
                            }
                            catch (Exception e)
                            {
                                ServerLogger.LogError(e);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Inits the persistence.
        /// </summary>
        /// <param name="path">The path.</param>
        private void InitPersistence(string path)
        {
            _persistTimer.Interval = 1000 * 5; // every 5 seconds
            _persistTimer.Enabled = true;
            _persistTimer.Elapsed += (sender, args) =>
            {
                if (_changes)
                {
                    lock (_lock)
                    {
                        // TODO : Persistence work ;)
                        var keys = _messageRepo.Keys;
                        foreach (var k in keys)
                        {
                            IList<CompileMessageTO> val;
                            if (_messageRepo.TryGetValue(k, out val))
                            {
                                var pPath = Path.Combine(path, k + ".msg");
                                BinaryFormatter bf = new BinaryFormatter();
                                using (Stream s = new FileStream(pPath, FileMode.OpenOrCreate))
                                {
                                    bf.Serialize(s, val);
                                }
                            }
                        }

                        _changes = false;
                    }
                }
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the message.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="msgs">The MSGS.</param>
        /// <returns></returns>
        public bool AddMessage(Guid workspaceID, IList<CompileMessageTO> msgs)
        {
            IList<CompileMessageTO> messages;

            lock (_lock)
            {   
                if (!_messageRepo.TryGetValue(workspaceID, out messages))
                {
                    messages = new List<CompileMessageTO>();
                }

                // clean up any messages with the same id and add
                foreach (var msg in msgs)
                {
                    var existingWithID = messages.Where(c => c.MessageID == msg.MessageID);

                    foreach (var tmp in existingWithID)
                    {
                        messages.Remove(tmp);
                    }

                    messages.Add(msg);
                }

                _changes = true;
            }

            return false;
        }

        /// <summary>
        /// Removes the message.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="serviceID">The service ID.</param>
        /// <returns></returns>
        public bool RemoveMessages(Guid workspaceID, Guid serviceID)
        {
            IList<CompileMessageTO> messages;

            lock (_lock)
            {
                if (_messageRepo.TryGetValue(workspaceID, out messages))
                {
                    var candidateMessage = messages.Where(c => c.ServiceID == serviceID);

                    var compileMessageTos = candidateMessage as IList<CompileMessageTO> ?? candidateMessage.ToList();
                    foreach (var msg in compileMessageTos)
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
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="serviceID">The service ID.</param>
        /// <returns></returns>
        public CompileMessageList FetchMessages(Guid workspaceID, Guid serviceID, List<ResourceForTree> deps, CompileMessageType[] filter = null)
        {
            IList<CompileMessageTO> messages;
            IList<CompileMessageTO> result = new List<CompileMessageTO>();

            lock (_lock)
            {
                if (_messageRepo.TryGetValue(workspaceID, out messages))
                {

                    // Fetch dep list and process ;)
                    if (deps != null)
                    {
                        foreach (var d in deps)
                        {
                            ResourceForTree d1 = d;
                            var candidateMessage = messages.Where(c => c.ServiceID == d1.ResourceID);
                            var compileMessageTos = candidateMessage as IList<CompileMessageTO> ??
                                                    candidateMessage.ToList();

                            foreach (var msg in compileMessageTos)
                            {
                                if (filter != null)
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
                }
            }

            return new CompileMessageList() {MessageList = messages, ServiceID = serviceID};
        }

        public void Dispose()
        {
            if (_persistTimer != null)
            {
                try
                {
                    _persistTimer.Close();
                }
                catch (Exception e)
                {
                }
            }
        }

        #endregion
    }
}
