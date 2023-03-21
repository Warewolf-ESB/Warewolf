using System;
using System.Threading;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Threading
{
	[RubyClass]
	public class SizedQueue : RubyQueue
	{
		private int _limit;

		public SizedQueue([DefaultProtocol] int limit)
		{
			_limit = limit;
		}

		public SizedQueue()
		{
		}

		private void Enqueue(object value)
		{
			lock (_queue)
			{
				_waiting++;
				try
				{
					while (_queue.Count == _limit)
					{
						Monitor.Wait(_queue);
					}
				}
				finally
				{
					_waiting--;
				}
				_queue.Enqueue(value);
				Monitor.PulseAll(_queue);
			}
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static SizedQueue Reinitialize(SizedQueue self, [DefaultProtocol] int limit)
		{
			SetLimit(self, limit);
			return self;
		}

		[RubyMethod("max")]
		public static int GetLimit(SizedQueue self)
		{
			return self._limit;
		}

		[RubyMethod("max=")]
		public static void SetLimit(SizedQueue self, [DefaultProtocol] int limit)
		{
			self._limit = limit;
		}

		[RubyMethod("push")]
		[RubyMethod("<<")]
		[RubyMethod("enq")]
		public static SizedQueue Enqueue(SizedQueue self, object value)
		{
			self.Enqueue(value);
			return self;
		}

		[RubyMethod("deq")]
		[RubyMethod("pop")]
		[RubyMethod("shift")]
		public static object Dequeue(SizedQueue self, params object[] values)
		{
			if (values.Length != 0)
			{
				throw new NotImplementedException();
			}
			return self.Dequeue();
		}
	}
}
