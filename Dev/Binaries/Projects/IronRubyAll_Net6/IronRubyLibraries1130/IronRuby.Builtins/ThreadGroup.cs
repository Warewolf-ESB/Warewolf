using System.Threading;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("ThreadGroup", Inherits = typeof(object))]
	public class ThreadGroup
	{
		[RubyConstant]
		public static readonly ThreadGroup Default = new ThreadGroup();

		[RubyMethod("add")]
		public static ThreadGroup Add([NotNull] ThreadGroup self, [NotNull] Thread thread)
		{
			ThreadOps.RubyThreadInfo.FromThread(thread).Group = self;
			return self;
		}

		[RubyMethod("list")]
		public static RubyArray List([NotNull] ThreadGroup self)
		{
			ThreadOps.RubyThreadInfo[] threads = ThreadOps.RubyThreadInfo.Threads;
			RubyArray rubyArray = new RubyArray(threads.Length);
			ThreadOps.RubyThreadInfo[] array = threads;
			foreach (ThreadOps.RubyThreadInfo rubyThreadInfo in array)
			{
				Thread thread = rubyThreadInfo.Thread;
				if (thread != null && rubyThreadInfo.Group == self)
				{
					rubyArray.Add(thread);
				}
			}
			return rubyArray;
		}
	}
}
