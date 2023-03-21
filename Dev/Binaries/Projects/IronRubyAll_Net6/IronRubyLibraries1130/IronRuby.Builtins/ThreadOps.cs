using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[RubyClass("Thread", Extends = typeof(Thread), Inherits = typeof(object))]
	public static class ThreadOps
	{
		private enum RubyThreadStatus
		{
			Unstarted,
			Running,
			Sleeping,
			Completed,
			Aborting,
			Aborted
		}

		internal class RubyThreadInfo
		{
			private static readonly Dictionary<int, RubyThreadInfo> _mapping = new Dictionary<int, RubyThreadInfo>();

			private readonly Dictionary<RubySymbol, object> _threadLocalStorage;

			private ThreadGroup _group;

			private readonly Thread _thread;

			private bool _blocked;

			private bool _abortOnException;

			private AutoResetEvent _runSignal = new AutoResetEvent(false);

			private bool _isSleeping;

			internal object this[RubySymbol key]
			{
				get
				{
					lock (_threadLocalStorage)
					{
						object value;
						if (!_threadLocalStorage.TryGetValue(key, out value))
						{
							return null;
						}
						return value;
					}
				}
				set
				{
					lock (_threadLocalStorage)
					{
						if (value == null)
						{
							_threadLocalStorage.Remove(key);
						}
						else
						{
							_threadLocalStorage[key] = value;
						}
					}
				}
			}

			internal ThreadGroup Group
			{
				get
				{
					return _group;
				}
				set
				{
					Interlocked.Exchange(ref _group, value);
				}
			}

			internal Thread Thread
			{
				get
				{
					return _thread;
				}
			}

			internal Exception Exception { get; set; }

			internal object Result { get; set; }

			internal bool CreatedFromRuby { get; set; }

			internal bool ExitRequested { get; set; }

			internal bool Blocked
			{
				get
				{
					return _blocked;
				}
				set
				{
					_blocked = value;
				}
			}

			internal bool AbortOnException
			{
				get
				{
					return _abortOnException;
				}
				set
				{
					_abortOnException = value;
				}
			}

			internal static RubyThreadInfo[] Threads
			{
				get
				{
					lock (_mapping)
					{
						List<RubyThreadInfo> list = new List<RubyThreadInfo>(_mapping.Count);
						foreach (KeyValuePair<int, RubyThreadInfo> item in _mapping)
						{
							if (item.Value.Thread.IsAlive)
							{
								list.Add(item.Value);
							}
						}
						return list.ToArray();
					}
				}
			}

			private RubyThreadInfo(Thread thread)
			{
				_threadLocalStorage = new Dictionary<RubySymbol, object>();
				_group = ThreadGroup.Default;
				_thread = thread;
			}

			internal static RubyThreadInfo FromThread(Thread t)
			{
				lock (_mapping)
				{
					int managedThreadId = t.ManagedThreadId;
					RubyThreadInfo value;
					if (!_mapping.TryGetValue(managedThreadId, out value))
					{
						value = new RubyThreadInfo(t);
						_mapping[managedThreadId] = value;
						return value;
					}
					return value;
				}
			}

			internal static void RegisterThread(Thread t)
			{
				FromThread(t);
			}

			internal bool HasKey(RubySymbol key)
			{
				lock (_threadLocalStorage)
				{
					return _threadLocalStorage.ContainsKey(key);
				}
			}

			internal RubyArray GetKeys()
			{
				lock (_threadLocalStorage)
				{
					RubyArray rubyArray = new RubyArray(_threadLocalStorage.Count);
					foreach (RubySymbol key in _threadLocalStorage.Keys)
					{
						rubyArray.Add(key);
					}
					return rubyArray;
				}
			}

			internal void Sleep()
			{
				try
				{
					_isSleeping = true;
					_runSignal.WaitOne();
				}
				finally
				{
					_isSleeping = false;
				}
			}

			internal void Run()
			{
				if (_isSleeping)
				{
					_runSignal.Set();
				}
			}
		}

		private static bool _globalAbortOnException;

		private static Exception MakeKeyTypeException(RubyContext context, object key)
		{
			if (key == null)
			{
				return RubyExceptions.CreateTypeError("nil is not a symbol");
			}
			return RubyExceptions.CreateArgumentError("{0} is not a symbol", context.GetClassOf(key).Name);
		}

		[RubyMethod("[]")]
		public static object GetElement(Thread self, [NotNull] RubySymbol key)
		{
			RubyThreadInfo rubyThreadInfo = RubyThreadInfo.FromThread(self);
			return rubyThreadInfo[key];
		}

		[RubyMethod("[]")]
		public static object GetElement(RubyContext context, Thread self, [NotNull] MutableString key)
		{
			return GetElement(self, context.CreateSymbol(key));
		}

		[RubyMethod("[]")]
		public static object GetElement(RubyContext context, Thread self, object key)
		{
			throw MakeKeyTypeException(context, key);
		}

		[RubyMethod("[]=")]
		public static object SetElement(Thread self, [NotNull] RubySymbol key, object value)
		{
			RubyThreadInfo rubyThreadInfo = RubyThreadInfo.FromThread(self);
			rubyThreadInfo[key] = value;
			return value;
		}

		[RubyMethod("[]=")]
		public static object SetElement(RubyContext context, Thread self, [NotNull] MutableString key, object value)
		{
			return SetElement(self, context.CreateSymbol(key), value);
		}

		[RubyMethod("[]=")]
		public static object SetElement(RubyContext context, Thread self, object key, object value)
		{
			throw MakeKeyTypeException(context, key);
		}

		[RubyMethod("abort_on_exception")]
		public static object AbortOnException(Thread self)
		{
			RubyThreadInfo rubyThreadInfo = RubyThreadInfo.FromThread(self);
			return rubyThreadInfo.AbortOnException;
		}

		[RubyMethod("abort_on_exception=")]
		public static object AbortOnException(Thread self, bool value)
		{
			RubyThreadInfo rubyThreadInfo = RubyThreadInfo.FromThread(self);
			rubyThreadInfo.AbortOnException = value;
			return value;
		}

		[RubyMethod("alive?")]
		public static bool IsAlive(Thread self)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			return self.IsAlive;
		}

		[RubyMethod("group")]
		public static ThreadGroup Group(Thread self)
		{
			RubyThreadInfo rubyThreadInfo = RubyThreadInfo.FromThread(self);
			return rubyThreadInfo.Group;
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(RubyContext context, Thread self)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			MutableString mutableString = MutableString.CreateMutable(context.GetIdentifierEncoding());
			mutableString.Append("#<");
			mutableString.Append(context.GetClassDisplayName(self));
			mutableString.Append(':');
			RubyUtils.AppendFormatHexObjectId(mutableString, RubyUtils.GetObjectId(context, self));
			mutableString.Append(' ');
			switch (GetStatus(self))
			{
			case RubyThreadStatus.Unstarted:
				mutableString.Append("unstarted");
				break;
			case RubyThreadStatus.Running:
				mutableString.Append("run");
				break;
			case RubyThreadStatus.Sleeping:
				mutableString.Append("sleep");
				break;
			case RubyThreadStatus.Aborting:
				mutableString.Append("aborting");
				break;
			case RubyThreadStatus.Completed:
			case RubyThreadStatus.Aborted:
				mutableString.Append("dead");
				break;
			}
			mutableString.Append('>');
			return mutableString;
		}

		[RubyMethod("join")]
		public static Thread Join(Thread self)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			self.Join();
			Exception exception = RubyThreadInfo.FromThread(self).Exception;
			if (exception != null)
			{
				throw exception;
			}
			return self;
		}

		[RubyMethod("join")]
		public static Thread Join(Thread self, double seconds)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			if (self.ThreadState != ThreadState.AbortRequested && self.ThreadState != ThreadState.Aborted)
			{
				double num = seconds * 1000.0;
				int millisecondsTimeout = ((num < -2147483648.0 || num > 2147483647.0) ? (-1) : ((int)num));
				if (!self.Join(millisecondsTimeout))
				{
					return null;
				}
			}
			Exception exception = RubyThreadInfo.FromThread(self).Exception;
			if (exception != null)
			{
				throw exception;
			}
			return self;
		}

		[RubyMethod("kill")]
		[RubyMethod("exit")]
		[RubyMethod("terminate")]
		public static Thread Kill(Thread self)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			RubyThreadInfo rubyThreadInfo = RubyThreadInfo.FromThread(self);
			if (GetStatus(self) == RubyThreadStatus.Sleeping && rubyThreadInfo.ExitRequested)
			{
				rubyThreadInfo.Run();
				return self;
			}
			rubyThreadInfo.ExitRequested = true;
			RubyUtils.ExitThread(self);
			return self;
		}

		[RubyMethod("key?")]
		public static object HasKey(Thread self, [NotNull] RubySymbol key)
		{
			RubyThreadInfo rubyThreadInfo = RubyThreadInfo.FromThread(self);
			return rubyThreadInfo.HasKey(key);
		}

		[RubyMethod("key?")]
		public static object HasKey(RubyContext context, Thread self, [NotNull] MutableString key)
		{
			return HasKey(self, context.CreateSymbol(key));
		}

		[RubyMethod("key?")]
		public static object HasKey(RubyContext context, Thread self, object key)
		{
			throw MakeKeyTypeException(context, key);
		}

		[RubyMethod("keys")]
		public static object Keys(RubyContext context, Thread self)
		{
			RubyThreadInfo rubyThreadInfo = RubyThreadInfo.FromThread(self);
			return rubyThreadInfo.GetKeys();
		}

		[RubyMethod("priority", BuildConfig = "!SILVERLIGHT")]
		public static object Priority(Thread self)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			switch (self.Priority)
			{
			case ThreadPriority.Lowest:
				return -2;
			case ThreadPriority.BelowNormal:
				return -1;
			case ThreadPriority.Normal:
				return 0;
			case ThreadPriority.AboveNormal:
				return 1;
			case ThreadPriority.Highest:
				return 2;
			default:
				return 0;
			}
		}

		[RubyMethod("priority=", BuildConfig = "!SILVERLIGHT")]
		public static Thread Priority(Thread self, int priority)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			if (priority <= -2)
			{
				self.Priority = ThreadPriority.Lowest;
			}
			else
			{
				switch (priority)
				{
				case -1:
					self.Priority = ThreadPriority.BelowNormal;
					break;
				case 0:
					self.Priority = ThreadPriority.Normal;
					break;
				case 1:
					self.Priority = ThreadPriority.AboveNormal;
					break;
				default:
					self.Priority = ThreadPriority.Highest;
					break;
				}
			}
			return self;
		}

		private static void RaiseAsyncException(Thread thread, Exception exception)
		{
			RubyThreadStatus status = GetStatus(thread);
			RubyUtils.RaiseAsyncException(thread, exception);
			int num = 2;
		}

		[RubyMethod("raise")]
		[RubyStackTraceHidden]
		public static void RaiseException(RubyContext context, Thread self)
		{
			if (self == Thread.CurrentThread)
			{
				KernelOps.RaiseException(context, self);
			}
			else
			{
				RaiseAsyncException(self, new RuntimeError());
			}
		}

		[RubyStackTraceHidden]
		[RubyMethod("raise")]
		public static void RaiseException(Thread self, [NotNull] MutableString message)
		{
			if (self == Thread.CurrentThread)
			{
				KernelOps.RaiseException(self, message);
				return;
			}
			Exception exception = RubyExceptionData.InitializeException(new RuntimeError(message.ToString()), message);
			RaiseAsyncException(self, exception);
		}

		[RubyStackTraceHidden]
		[RubyMethod("raise")]
		public static void RaiseException(RespondToStorage respondToStorage, UnaryOpStorage storage0, BinaryOpStorage storage1, CallSiteStorage<Action<CallSite, Exception, RubyArray>> setBackTraceStorage, Thread self, object obj, [Optional] object arg, [Optional] RubyArray backtrace)
		{
			if (self == Thread.CurrentThread)
			{
				KernelOps.RaiseException(respondToStorage, storage0, storage1, setBackTraceStorage, self, obj, arg, backtrace);
				return;
			}
			Exception exception = KernelOps.CreateExceptionToRaise(respondToStorage, storage0, storage1, setBackTraceStorage, obj, arg, backtrace);
			RaiseAsyncException(self, exception);
		}

		[RubyMethod("wakeup", BuildConfig = "!SILVERLIGHT")]
		[RubyMethod("run", BuildConfig = "!SILVERLIGHT")]
		public static Thread Run(Thread self)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			RubyThreadInfo rubyThreadInfo = RubyThreadInfo.FromThread(self);
			rubyThreadInfo.Run();
			return self;
		}

		private static RubyThreadStatus GetStatus(Thread thread)
		{
			ThreadState threadState = thread.ThreadState;
			RubyThreadInfo rubyThreadInfo = RubyThreadInfo.FromThread(thread);
			if ((threadState & ThreadState.Unstarted) == ThreadState.Unstarted)
			{
				if (rubyThreadInfo.CreatedFromRuby)
				{
					return RubyThreadStatus.Running;
				}
				return RubyThreadStatus.Unstarted;
			}
			if ((threadState & (ThreadState.Stopped | ThreadState.Aborted)) != 0)
			{
				if (RubyThreadInfo.FromThread(thread).Exception == null)
				{
					return RubyThreadStatus.Completed;
				}
				return RubyThreadStatus.Aborted;
			}
			if ((threadState & ThreadState.WaitSleepJoin) == ThreadState.WaitSleepJoin)
			{
				return RubyThreadStatus.Sleeping;
			}
			if ((threadState & ThreadState.AbortRequested) != 0)
			{
				return RubyThreadStatus.Aborting;
			}
			if (0 == 0)
			{
				if (rubyThreadInfo.Blocked)
				{
					return RubyThreadStatus.Sleeping;
				}
				return RubyThreadStatus.Running;
			}
			throw new ArgumentException("unknown thread status: " + threadState);
		}

		[RubyMethod("status")]
		public static object Status(Thread self)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			switch (GetStatus(self))
			{
			case RubyThreadStatus.Unstarted:
				return MutableString.CreateAscii("unstarted");
			case RubyThreadStatus.Running:
				return MutableString.CreateAscii("run");
			case RubyThreadStatus.Sleeping:
				return MutableString.CreateAscii("sleep");
			case RubyThreadStatus.Aborting:
				return MutableString.CreateAscii("aborting");
			case RubyThreadStatus.Completed:
				return false;
			case RubyThreadStatus.Aborted:
				return null;
			default:
				throw new ArgumentException("unknown thread status");
			}
		}

		[RubyMethod("value")]
		public static object Value(Thread self)
		{
			Join(self);
			return RubyThreadInfo.FromThread(self).Result;
		}

		[RubyMethod("abort_on_exception", RubyMethodAttributes.PublicSingleton)]
		public static object GlobalAbortOnException(object self)
		{
			return _globalAbortOnException;
		}

		[RubyMethod("abort_on_exception=", RubyMethodAttributes.PublicSingleton)]
		public static object GlobalAbortOnException(object self, bool value)
		{
			_globalAbortOnException = value;
			return value;
		}

		private static void SetCritical(RubyContext context, bool value)
		{
			if (value)
			{
				bool lockTaken = false;
				try
				{
					MonitorUtils.Enter(context.CriticalMonitor, ref lockTaken);
					return;
				}
				finally
				{
					if (lockTaken)
					{
						context.CriticalThread = Thread.CurrentThread;
					}
				}
			}
			Monitor.Exit(context.CriticalMonitor);
			context.CriticalThread = null;
		}

		[RubyMethod("critical", RubyMethodAttributes.PublicSingleton)]
		public static bool Critical(RubyContext context, object self)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			return context.CriticalThread != null;
		}

		[RubyMethod("critical=", RubyMethodAttributes.PublicSingleton)]
		public static void Critical(RubyContext context, object self, bool value)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			SetCritical(context, value);
		}

		[RubyMethod("current", RubyMethodAttributes.PublicSingleton)]
		public static Thread Current(object self)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			return Thread.CurrentThread;
		}

		[RubyMethod("list", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray List(object self)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			RubyThreadInfo[] threads = RubyThreadInfo.Threads;
			RubyArray rubyArray = new RubyArray(threads.Length);
			RubyThreadInfo[] array = threads;
			foreach (RubyThreadInfo rubyThreadInfo in array)
			{
				Thread thread = rubyThreadInfo.Thread;
				if (thread != null)
				{
					rubyArray.Add(thread);
				}
			}
			return rubyArray;
		}

		[RubyMethod("main", RubyMethodAttributes.PublicSingleton)]
		public static Thread GetMainThread(RubyContext context, RubyClass self)
		{
			return context.MainThread;
		}

		[RubyMethod("start", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("new", RubyMethodAttributes.PublicSingleton)]
		public static Thread CreateThread(RubyContext context, BlockParam startRoutine, object self, params object[] args)
		{
			if (startRoutine == null)
			{
				throw new ThreadError("must be called with a block");
			}
			ThreadGroup group = Group(Thread.CurrentThread);
			Thread thread = new Thread((ThreadStart)delegate
			{
				RubyThreadStart(context, startRoutine, args, group);
			});
			thread.IsBackground = true;
			thread.Start();
			return thread;
		}

		private static void RubyThreadStart(RubyContext context, BlockParam startRoutine, object[] args, ThreadGroup group)
		{
			RubyThreadInfo rubyThreadInfo = RubyThreadInfo.FromThread(Thread.CurrentThread);
			rubyThreadInfo.CreatedFromRuby = true;
			rubyThreadInfo.Group = group;
			try
			{
				object blockResult;
				if (startRoutine.Yield(args, out blockResult) && startRoutine.Returning(blockResult, out blockResult))
				{
					rubyThreadInfo.Exception = new ThreadError("return can't jump across threads");
				}
				rubyThreadInfo.Result = blockResult;
			}
			catch (MethodUnwinder)
			{
				rubyThreadInfo.Exception = new ThreadError("return can't jump across threads");
			}
			catch (Exception e)
			{
				if (rubyThreadInfo.ExitRequested)
				{
					rubyThreadInfo.Result = false;
					Thread.ResetAbort();
					return;
				}
				Exception visibleException = RubyUtils.GetVisibleException(e);
				RubyExceptionData.ActiveExceptionHandled(visibleException);
				rubyThreadInfo.Exception = visibleException;
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(visibleException.Message);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.Append(visibleException.StackTrace);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				RubyExceptionData instance = RubyExceptionData.GetInstance(visibleException);
				if (instance.Backtrace != null)
				{
					foreach (object item in instance.Backtrace)
					{
						stringBuilder.Append(item.ToString());
					}
				}
				if (!_globalAbortOnException && !rubyThreadInfo.AbortOnException)
				{
					return;
				}
				throw;
			}
			finally
			{
				if (context.RubyOptions.Compatibility < RubyCompatibility.Default && context.CriticalThread == Thread.CurrentThread)
				{
					SetCritical(context, false);
				}
			}
		}

		[RubyMethod("pass", RubyMethodAttributes.PublicSingleton)]
		public static void Yield(object self)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			Thread.Sleep(0);
		}

		[RubyMethod("stop", RubyMethodAttributes.PublicSingleton)]
		public static void Stop(RubyContext context, object self)
		{
			if (context.CriticalThread == Thread.CurrentThread)
			{
				SetCritical(context, false);
			}
			DoSleep();
		}

		internal static void DoSleep()
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			RubyThreadInfo rubyThreadInfo = RubyThreadInfo.FromThread(Thread.CurrentThread);
			rubyThreadInfo.Sleep();
		}

		[RubyMethod("stop?", RubyMethodAttributes.PublicInstance)]
		public static bool IsStopped(Thread self)
		{
			RubyThreadInfo.RegisterThread(Thread.CurrentThread);
			RubyThreadStatus status = GetStatus(self);
			if (status != RubyThreadStatus.Sleeping && status != RubyThreadStatus.Completed)
			{
				return status == RubyThreadStatus.Aborted;
			}
			return true;
		}
	}
}
