using System;
using System.Collections.Generic;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers.Interfaces
{
    public interface ITriggerCollection : IEnumerable<ITrigger>, IDisposable, IWrappedObject<TriggerCollection>
    {
        /// <summary>
        ///     Add an unbound <see cref="Trigger" /> to the task.
        /// </summary>
        /// <param name="unboundTrigger">
        ///     <see cref="Trigger" /> derivative to add to the task.
        /// </param>
        /// <returns>Bound trigger.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <c>unboundTrigger</c> is <c>null</c>.
        /// </exception>
        ITrigger Add(ITrigger unboundTrigger);
    }
}