using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule("Signal", BuildConfig = "!SILVERLIGHT")]
	public static class Signal
	{
		[RubyMethod("list", RubyMethodAttributes.PublicSingleton)]
		public static Hash List(RubyContext context, RubyModule self)
		{
			Hash hash = new Hash(context);
			hash.Add(MutableString.CreateAscii("TERM"), ScriptingRuntimeHelpers.Int32ToObject(15));
			hash.Add(MutableString.CreateAscii("SEGV"), ScriptingRuntimeHelpers.Int32ToObject(11));
			hash.Add(MutableString.CreateAscii("KILL"), ScriptingRuntimeHelpers.Int32ToObject(9));
			hash.Add(MutableString.CreateAscii("EXIT"), ScriptingRuntimeHelpers.Int32ToObject(0));
			hash.Add(MutableString.CreateAscii("INT"), ScriptingRuntimeHelpers.Int32ToObject(2));
			hash.Add(MutableString.CreateAscii("FPE"), ScriptingRuntimeHelpers.Int32ToObject(8));
			hash.Add(MutableString.CreateAscii("ABRT"), ScriptingRuntimeHelpers.Int32ToObject(22));
			hash.Add(MutableString.CreateAscii("ILL"), ScriptingRuntimeHelpers.Int32ToObject(4));
			return hash;
		}

		[RubyMethod("trap", RubyMethodAttributes.PublicSingleton)]
		public static object Trap(RubyContext context, object self, object signalId, Proc proc)
		{
			if (signalId is MutableString && ((MutableString)signalId).ConvertToString() == "INT")
			{
				context.InterruptSignalHandler = delegate
				{
					proc.Call(null);
				};
			}
			return null;
		}

		[RubyMethod("trap", RubyMethodAttributes.PublicSingleton)]
		public static object Trap(RubyContext context, BlockParam block, object self, object signalId)
		{
			if (signalId is MutableString && ((MutableString)signalId).ConvertToString() == "INT")
			{
				context.InterruptSignalHandler = delegate
				{
					object blockResult;
					block.Yield(out blockResult);
				};
			}
			return null;
		}
	}
}
