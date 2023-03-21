using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Threading
{
	[RubyClass("Queue")]
	public class RubyQueue
	{
		protected readonly Queue<object> _queue;

		protected int _waiting;

		public RubyQueue()
		{
			_queue = new Queue<object>();
		}

		protected RubyQueue(int capacity)
		{
			_queue = new Queue<object>(capacity);
		}

		private void Enqueue(object value)
		{
			lock (_queue)
			{
				_queue.Enqueue(value);
				Monitor.PulseAll(_queue);
			}
		}

		protected object Dequeue()
		{
			lock (_queue)
			{
				_waiting++;
				try
				{
					while (_queue.Count == 0)
					{
						Monitor.Wait(_queue);
					}
				}
				finally
				{
					_waiting--;
				}
				object result = _queue.Dequeue();
				Monitor.PulseAll(_queue);
				return result;
			}
		}

		[RubyMethod("<<")]
		[RubyMethod("enq")]
		[RubyMethod("push")]
		public static RubyQueue Enqueue(RubyQueue self, object value)
		{
			self.Enqueue(value);
			return self;
		}

		[RubyMethod("shift")]
		[RubyMethod("pop")]
		[RubyMethod("deq")]
		public static object Dequeue(RubyQueue self, [Optional] bool nonBlocking)
		{
			if (nonBlocking)
			{
				lock (self._queue)
				{
					if (self._queue.Count == 0)
					{
						throw new ThreadError("queue empty");
					}
					return self._queue.Dequeue();
				}
			}
			return self.Dequeue();
		}

		[RubyMethod("size")]
		[RubyMethod("length")]
		public static int GetCount(RubyQueue self)
		{
			lock (self._queue)
			{
				return self._queue.Count;
			}
		}

		[RubyMethod("clear")]
		public static RubyQueue Clear(RubyQueue self)
		{
			lock (self._queue)
			{
				self._queue.Clear();
				return self;
			}
		}

		[RubyMethod("empty?")]
		public static bool IsEmpty(RubyQueue self)
		{
			return GetCount(self) == 0;
		}

		[RubyMethod("num_waiting")]
		public static int GetNumberOfWaitingThreads(RubyQueue self)
		{
			return self._waiting;
		}
	}
}
