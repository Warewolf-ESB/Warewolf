
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
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
