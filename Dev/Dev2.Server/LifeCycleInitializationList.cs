using Dev2.ServerLifeCycleWorkers;
using System.Collections;
using System.Collections.Generic;

namespace Dev2
{
    class LifeCycleInitializationList : IEnumerable<IServerLifecycleWorker>
    {
        /// <summary>
        /// This method defines the server life cycle workers that will be used
        /// for server initialization
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IServerLifecycleWorker> GetEnumerator()
        {
            yield return new RegisterDependenciesWorker();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
