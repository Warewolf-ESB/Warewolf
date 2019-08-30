using Dev2.Runtime.Triggers;
using System.Collections.Generic;

namespace Dev2
{
    internal class QueueConfigLoader : IQueueConfigLoader
    {
        public IEnumerable<string> Configs
        {
            get
            {
                foreach (var queueTrigger in TriggersCatalog.Instance.Queues)
                {
                    yield return queueTrigger.TriggerId.ToString();
                }
            }
        }
    }
}