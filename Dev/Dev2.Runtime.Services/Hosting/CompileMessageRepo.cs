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
using System.Collections.Generic;
using System.IO;
using System.Reactive.Subjects;
using System.Runtime.Serialization.Formatters.Binary;
using System.Timers;
using Dev2.Common;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;

namespace Dev2.Runtime.Hosting
{
    public class CompileMessageRepo : IDisposable
    {
        readonly IDictionary<Guid, IList<ICompileMessageTO>> _messageRepo = new Dictionary<Guid, IList<ICompileMessageTO>>();
        static Subject<IList<ICompileMessageTO>> _allMessages = new Subject<IList<ICompileMessageTO>>();
        private static readonly object Lock = new object();
        private static bool _changes;
        private static readonly Timer PersistTimer = new Timer(1000 * 5);
        
        public string PersistencePath { get; private set; }

        private static CompileMessageRepo _instance;
        public static CompileMessageRepo Instance => _instance ?? (_instance = new CompileMessageRepo());

        public CompileMessageRepo()
            : this(null, false)
        {
        }

        public CompileMessageRepo(string persistPath)
            : this(persistPath, true)
        {
        }

        public CompileMessageRepo(string persistPath, bool activateBackgroundWorker)
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


                                    if (obj is IList<ICompileMessageTO> listOf)
                                    {
                                        if (Guid.TryParse(fname, out Guid id))
                                        {
                                            _messageRepo[id] = listOf;
                                            _allMessages.OnNext(listOf);
                                        }
                                        else
                                        {
                                            Dev2Logger.Error("Failed to parse message ID", GlobalConstants.WarewolfError);
                                        }
                                    }
                                }
                                catch(Exception e)
                                {
                                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                                }
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
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
                            if (_messageRepo.TryGetValue(k, out IList<ICompileMessageTO> val))
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
                    catch(Exception e)
                    {
                        Dev2Logger.Error(e, GlobalConstants.WarewolfError);
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
                if (!_messageRepo.TryGetValue(workspaceId, out IList<ICompileMessageTO> messages))
                {
                    messages = new List<ICompileMessageTO>();
                }

                // clean up any messages with the same id and add

                for (int i = messages.Count - 1; i >= 0; i--)
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
        
        public IObservable<IList<ICompileMessageTO>> AllMessages => _allMessages;

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
                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
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
