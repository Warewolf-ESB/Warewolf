using Dev2.Runtime.Triggers;
using Dev2.Triggers;
using System.Collections.Generic;

namespace Dev2
{
    internal class QueueWorkerConfigLoader : IQueueConfigLoader
    {
        public IEnumerable<ITrigger> Configs
        {
            get
            {
                foreach (var queueTrigger in TriggersCatalog.Instance.Queues)
                {
                    yield return queueTrigger;
                }
            }
        }
    }
}