using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.StandardLibrary.Threading
{
	[RubyClass("Mutex")]
	public class RubyMutex
	{
		private readonly object _mutex;

		private bool _isLocked;

		internal object Mutex
		{
			get
			{
				return _mutex;
			}
		}

		public RubyMutex()
		{
			_mutex = new object();
		}

		[RubyMethod("locked?")]
		public static bool IsLocked(RubyMutex self)
		{
			return self._isLocked;
		}

		[RubyMethod("try_lock")]
		public static bool TryLock(RubyMutex self)
		{
			bool lockTaken = false;
			try
			{
				MonitorUtils.TryEnter(self._mutex, ref lockTaken);
			}
			finally
			{
				if (lockTaken)
				{
					self._isLocked = true;
				}
			}
			return lockTaken;
		}

		[RubyMethod("lock")]
		public static RubyMutex Lock(RubyMutex self)
		{
			bool lockTaken = false;
			try
			{
				MonitorUtils.Enter(self._mutex, ref lockTaken);
			}
			finally
			{
				if (lockTaken)
				{
					self._isLocked = true;
				}
			}
			return self;
		}

		[RubyMethod("unlock")]
		public static RubyMutex Unlock(RubyMutex self)
		{
			bool lockTaken = true;
			try
			{
				MonitorUtils.Exit(self._mutex, ref lockTaken);
			}
			finally
			{
				if (!lockTaken)
				{
					self._isLocked = false;
				}
			}
			return self;
		}

		[RubyMethod("synchronize")]
		public static object Synchronize(BlockParam criticalSection, RubyMutex self)
		{
			bool lockTaken = false;
			try
			{
				MonitorUtils.Enter(self._mutex, ref lockTaken);
				self._isLocked = lockTaken;
				object blockResult;
				criticalSection.Yield(out blockResult);
				return blockResult;
			}
			finally
			{
				if (lockTaken)
				{
					MonitorUtils.Exit(self._mutex, ref lockTaken);
					self._isLocked = lockTaken;
				}
			}
		}

		[RubyMethod("exclusive_unlock")]
		public static bool ExclusiveUnlock(BlockParam criticalSection, RubyMutex self)
		{
			throw new NotImplementedException();
		}
	}
}
