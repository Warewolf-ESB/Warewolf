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
        static readonly Lazy<TriggersCatalog> LazyCat = new Lazy<TriggersCatalog>(() =>
        {
            var c = new TriggersCatalog();
            return c;
        }, LazyThreadSafetyMode.PublicationOnly);

        public static ITriggersCatalog Instance => LazyCat.Value;

        public TriggersCatalog()
        {
            _directoryWrapper = new DirectoryWrapper();
            _fileWrapper = new FileWrapper();
            _queueTriggersPath = EnvironmentVariables.QueueTriggersPath;
            _directoryWrapper.CreateIfNotExists(_queueTriggersPath);
            Queues = new List<ITriggerQueue>();
            _serializer = new Dev2JsonSerializer();
        }

        public List<ITriggerQueue> Queues { get; set; }

        public void DeleteAllTriggerQueues()
        {
            if (_directoryWrapper.Exists(_queueTriggersPath))
            {
                _directoryWrapper.Delete(_queueTriggersPath, true);
                Queues.Clear();
            }
        }

        public void DeleteTriggerQueue(ITriggerQueue triggerQueue)
        {
            var queueFilePath = GetQueueFilePath(triggerQueue);

            if (_fileWrapper.Exists(queueFilePath))
            {
                _fileWrapper.Delete(queueFilePath);
            }
        }

        public void Load()
        {
            Queues = new List<ITriggerQueue>();
            var resourceTestDirectories = _directoryWrapper.GetDirectories(_queueTriggersPath);
            foreach (var resourceTestDirectory in resourceTestDirectories)
            {
                var resIdString = _directoryWrapper.GetDirectoryName(resourceTestDirectory);
                if (Guid.TryParse(resIdString, out Guid resId))
                {
                    //Queues
                }
            }
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
            using (var sw = new StreamWriter(queueFilePath, false))
            {
                sw.Write(saveData);
            }
            
        }

        private string GetQueueFilePath(ITriggerQueue triggerQueue)
        {
            var queueFilePath = Path.Combine(_queueTriggersPath, $"{triggerQueue.TriggerId}.bite");
            return queueFilePath;
        }
    }
}
