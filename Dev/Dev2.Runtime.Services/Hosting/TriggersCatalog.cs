/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Warewolf.Security.Encryption;
using Warewolf.Triggers;

namespace Dev2.Runtime.Hosting
{
    public class TriggersCatalog : ITriggersCatalog
    {
        readonly IDirectory _directoryWrapper;
        readonly ISerializer _serializer;
        readonly IFile _fileWrapper;
        private readonly string _queueTriggersPath;
        static readonly Lazy<TriggersCatalog> _lazyCat = new Lazy<TriggersCatalog>(() =>
        {
            var c = new TriggersCatalog();
            return c;
        }, LazyThreadSafetyMode.PublicationOnly);

        public event TriggerChangeEvent OnChanged;
        public event TriggerChangeEvent OnDeleted;
        public event TriggerChangeEvent OnCreated;

        public static string PathFromResourceId(string triggerId) => Path.Combine(EnvironmentVariables.QueueTriggersPath, triggerId +".bite");

        public static ITriggersCatalog Instance => _lazyCat.Value;

        readonly IFileSystemWatcherWrapper _watcherWrapper;

        public TriggersCatalog(IDirectory directoryWrapper, IFile fileWrapper, string queueTriggersPath, ISerializer serializer, IFileSystemWatcherWrapper watcherWrapper)
        {
            _directoryWrapper = directoryWrapper;
            _fileWrapper = fileWrapper;
            _queueTriggersPath = queueTriggersPath;
            _directoryWrapper.CreateIfNotExists(_queueTriggersPath);
            Queues = new List<ITriggerQueue>();
            _serializer = serializer;
            _watcherWrapper = watcherWrapper;

            SetupFileSystemWatcher();
        }

        private void SetupFileSystemWatcher()
        {
            _watcherWrapper.Path = _queueTriggersPath;
            _watcherWrapper.Filter = "*.bite";
            _watcherWrapper.EnableRaisingEvents = true;
            _watcherWrapper.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite |
                               NotifyFilters.FileName | NotifyFilters.DirectoryName;

            _watcherWrapper.Created += FileSystemWatcher_Created;
            _watcherWrapper.Changed += FileSystemWatcher_Changed;
            _watcherWrapper.Deleted += FileSystemWatcher_Deleted;
            _watcherWrapper.Renamed += FileSystemWatcher_Renamed;
            _watcherWrapper.Error += FileSystemWatcher_Error;
        }

        private TriggersCatalog() : this(new DirectoryWrapper(), new FileWrapper(), EnvironmentVariables.QueueTriggersPath, new Dev2JsonSerializer(), new FileSystemWatcherWrapper())
        {
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            //start
            var guid = Path.GetFileNameWithoutExtension(e.Name);
            if (Guid.TryParse(guid, out var result))
            {
                Dev2Logger.Info($"Trigger created '{guid}'", GlobalConstants.ServerWorkspaceID.ToString());
                OnCreated?.Invoke(result);
            }
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            //restart
            var guid = Path.GetFileNameWithoutExtension(e.Name);
            if (Guid.TryParse(guid, out var result))
            {
                Dev2Logger.Info($"Trigger restarting '{guid}'", GlobalConstants.ServerWorkspaceID.ToString());
                OnChanged?.Invoke(result);
            }
        }

        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            //kill
            var guid = Path.GetFileNameWithoutExtension(e.Name);
            if (Guid.TryParse(guid, out var result))
            {
                Dev2Logger.Info($"Trigger deleted '{guid}'", GlobalConstants.ServerWorkspaceID.ToString());
                OnDeleted?.Invoke(result);
            }
        }

        private static void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            var message = $"Trigger '{e.OldName}' renamed to '{e.Name}'";
            Dev2Logger.Warn(message, GlobalConstants.ServerWorkspaceID.ToString());
        }

        private static void FileSystemWatcher_Error(object sender, ErrorEventArgs e)
        {
            var exception = e.GetException();
            Dev2Logger.Error(exception.Message, GlobalConstants.ServerWorkspaceID.ToString());
        }

        public IList<ITriggerQueue> Queues { get; set; }

        public void DeleteTriggerQueue(ITriggerQueue triggerQueue)
        {
            var queueFilePath = GetQueueFilePath(triggerQueue);

            if (_fileWrapper.Exists(queueFilePath))
            {
                _fileWrapper.Delete(queueFilePath);
            }
            Queues.Remove(triggerQueue);
        }

        public void Load()
        {
            var newQueues = new List<ITriggerQueue>();
            try
            {
                var triggerQueueFileNames = _directoryWrapper.GetFiles(_queueTriggersPath);
                foreach (var triggerQueueFileName in triggerQueueFileNames)
                {
                    try
                    {
                        var triggerQueue = LoadQueueTriggerFromFile(triggerQueueFileName);
                        newQueues.Add(triggerQueue);
                    }
                    catch (Exception ex)
                    {
                        Dev2Logger.Error($"TriggersCatalog - Load - {triggerQueueFileName}", ex, GlobalConstants.WarewolfError);
                    }
                }
            } finally
            {
                Queues = newQueues;
            }
        }

        public ITriggerQueue LoadQueueTriggerFromFile(string filename)
        {
            var fileData = _fileWrapper.ReadAllText(filename);
            var decryptedTrigger = DpapiWrapper.Decrypt(fileData);
            var triggerQueue = _serializer.Deserialize<ITriggerQueue>(decryptedTrigger);
            return triggerQueue;
        }

        public void SaveTriggerQueue(ITriggerQueue triggerQueue)
        {
            if(triggerQueue.TriggerId == Guid.Empty)
            {
                triggerQueue.TriggerId = Guid.NewGuid();
            }

            var serializedData = _serializer.Serialize(triggerQueue);
            var saveData = DpapiWrapper.Encrypt(serializedData);

            var queueFilePath = GetQueueFilePath(triggerQueue);
            _fileWrapper.WriteAllText(queueFilePath, saveData);
        }

        private string GetQueueFilePath(ITriggerQueue triggerQueue)
        {
            var queueFilePath = Path.Combine(_queueTriggersPath, $"{triggerQueue.TriggerId}.bite");
            return queueFilePath;
        }
    }
}
