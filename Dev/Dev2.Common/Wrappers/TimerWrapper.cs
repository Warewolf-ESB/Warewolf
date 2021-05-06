/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using System.Threading;

namespace Dev2.Common.Wrappers
{
    public class TimerWrapper : ITimer
    {
        Timer _timer;

        public TimerWrapper(TimerCallback callback, object state, int dueTime, int period)
        {
            _timer = new Timer(callback, state, dueTime, period);
        }

        public void Dispose()
        {
            if (_timer is null)
            {
                return;
            }

            _timer.Dispose();
            _timer = null;
        }
    }

    public class TimerWrapperFactory : ITimerFactory
    {
        public ITimer New(TimerCallback callback, object state, int dueTime, int period)
        {
            return new TimerWrapper(callback, state, dueTime, period);
        }
    }
}
