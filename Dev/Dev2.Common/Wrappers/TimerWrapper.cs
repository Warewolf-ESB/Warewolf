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
}
