using System.Threading;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.Threading
{
	[RubyClass(Extends = typeof(Thread), Inherits = typeof(object))]
	public static class ThreadOps
	{
		[RubyMethod("exclusive", RubyMethodAttributes.PublicSingleton)]
		public static object Exclusive(RubyContext context, [NotNull] BlockParam block, object self)
		{
			IronRuby.Builtins.ThreadOps.Critical(context, self, true);
			try
			{
				object blockResult;
				block.Yield(out blockResult);
				return blockResult;
			}
			finally
			{
				IronRuby.Builtins.ThreadOps.Critical(context, self, false);
			}
		}
	}
}
