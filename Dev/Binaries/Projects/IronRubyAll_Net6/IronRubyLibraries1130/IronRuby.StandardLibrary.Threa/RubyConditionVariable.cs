using System.Threading;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.Threading
{
	[RubyClass("ConditionVariable")]
	public class RubyConditionVariable
	{
		private RubyMutex _mutex;

		private readonly AutoResetEvent _signal = new AutoResetEvent(false);

		private readonly object _lock = new object();

		private int _waits;

		[RubyMethod("signal")]
		public static RubyConditionVariable Signal(RubyConditionVariable self)
		{
			RubyMutex mutex = self._mutex;
			if (mutex != null)
			{
				self._signal.Set();
			}
			return self;
		}

		[RubyMethod("broadcast")]
		public static RubyConditionVariable Broadcast(RubyConditionVariable self)
		{
			RubyMutex mutex = self._mutex;
			if (mutex != null)
			{
				lock (self._lock)
				{
					int waits = self._waits;
					for (int i = 0; i < waits; i++)
					{
						self._signal.Set();
						Thread.CurrentThread.Join(1);
					}
					return self;
				}
			}
			return self;
		}

		[RubyMethod("wait")]
		public static RubyConditionVariable Wait(RubyConditionVariable self, [NotNull] RubyMutex mutex)
		{
			self._mutex = mutex;
			RubyMutex.Unlock(mutex);
			lock (self._lock)
			{
				self._waits++;
			}
			self._signal.WaitOne();
			lock (self._lock)
			{
				self._waits--;
			}
			RubyMutex.Lock(mutex);
			return self;
		}
	}
}
