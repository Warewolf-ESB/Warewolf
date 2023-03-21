using System;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	internal sealed class CheckedMonitor
	{
		private struct CheckedMonitorLocker : IDisposable
		{
			private readonly CheckedMonitor _monitor;

			private bool _lockTaken;

			public CheckedMonitorLocker(CheckedMonitor monitor)
			{
				_monitor = monitor;
				_lockTaken = false;
				monitor.Enter(ref _lockTaken);
			}

			public void Dispose()
			{
				_monitor.Exit(ref _lockTaken);
			}
		}

		private struct CheckedMonitorUnlocker : IDisposable
		{
			private readonly CheckedMonitor _monitor;

			private bool _lockTaken;

			public CheckedMonitorUnlocker(CheckedMonitor monitor)
			{
				_monitor = monitor;
				_lockTaken = true;
				monitor.Exit(ref _lockTaken);
			}

			public void Dispose()
			{
				_monitor.Enter(ref _lockTaken);
			}
		}

		private int _locked;

		public bool IsLocked
		{
			get
			{
				return _locked > 0;
			}
		}

		internal void Enter(ref bool lockTaken)
		{
			try
			{
				MonitorUtils.Enter(this, ref lockTaken);
			}
			finally
			{
				if (lockTaken)
				{
					_locked++;
				}
			}
		}

		internal void Exit(ref bool lockTaken)
		{
			try
			{
				MonitorUtils.Exit(this, ref lockTaken);
			}
			finally
			{
				if (!lockTaken)
				{
					_locked--;
				}
			}
		}

		public IDisposable CreateLocker()
		{
			return new CheckedMonitorLocker(this);
		}

		public IDisposable CreateUnlocker()
		{
			return new CheckedMonitorUnlocker(this);
		}
	}
}
