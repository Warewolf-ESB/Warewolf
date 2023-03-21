using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	public static class RuleGenerators
	{
		public static void InstanceConstructor(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			((RubyClass)args.Target).BuildObjectConstruction(metaBuilder, args, name);
		}

		public static void InstanceAllocator(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			((RubyClass)args.Target).BuildObjectAllocation(metaBuilder, args, name);
		}

		public static void MethodCall(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			((RubyMethod)args.Target).BuildInvoke(metaBuilder, args);
		}
	}
}
