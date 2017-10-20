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
        }

        public bool Ping()
        {
            return true;
        }
        
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

                for (int i = messages.Count - 1; i >= 0; i--)
                {
                    messages.Remove(messages[i]);
                }
                
                foreach(var msg in msgs)
                {
                    messages.Add(msg);
                }
                _allMessages.OnNext(messages);
                _messageRepo[workspaceId] = messages;
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

        public void ClearObservable()
        {
            _allMessages.OnCompleted();
            _allMessages = new Subject<IList<ICompileMessageTO>>();
        }
    }
}
