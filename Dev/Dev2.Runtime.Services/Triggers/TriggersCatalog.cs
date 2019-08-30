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
using Dev2.Common.Interfaces.Triggers;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Communication;
using Dev2.Triggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Warewolf.Security.Encryption;

namespace Dev2.Runtime.Triggers
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

        public static string PathFromResourceId(string triggerId) => Path.Combine(EnvironmentVariables.QueueTriggersPath, triggerId +".bite");

        public static ITriggersCatalog Instance => _lazyCat.Value;

        public TriggersCatalog(IDirectory directoryWrapper, IFile fileWrapper, string queueTriggersPath, ISerializer serializer)
        {
            _directoryWrapper = directoryWrapper;
            _fileWrapper = fileWrapper;
            _queueTriggersPath = queueTriggersPath;
            _directoryWrapper.CreateIfNotExists(_queueTriggersPath);
            Queues = new List<ITriggerQueue>();
            _serializer = serializer;
        }

        private TriggersCatalog():this(new DirectoryWrapper(), new FileWrapper(), EnvironmentVariables.QueueTriggersPath, new Dev2JsonSerializer())
        {
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

        public ITriggerQueue LoadQueueTriggerFromFile(string triggerQueueFileName)
        {
            var fileData = _fileWrapper.ReadAllText(triggerQueueFileName);
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
