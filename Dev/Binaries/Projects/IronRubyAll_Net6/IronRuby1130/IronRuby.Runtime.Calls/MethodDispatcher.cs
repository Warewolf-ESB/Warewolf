using System;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	public abstract class MethodDispatcher : MemberDispatcher
	{
		internal static MethodDispatcher CreateRubyObjectDispatcher(Type func, Delegate method, int mandatoryParamCount, bool hasScope, bool hasBlock, int version)
		{
			MethodDispatcher methodDispatcher = (MethodDispatcher)MemberDispatcher.CreateDispatcher(func, mandatoryParamCount, hasScope, hasBlock, version, delegate
			{
				if (!hasScope)
				{
					if (!hasBlock)
					{
						return new RubyObjectMethodDispatcher();
					}
					return new RubyObjectMethodDispatcherWithBlock();
				}
				return (!hasBlock) ? ((MethodDispatcher<Func<object, Proc, object>>)new RubyObjectMethodDispatcherWithScope()) : ((MethodDispatcher<Func<object, Proc, object>>)new RubyObjectMethodDispatcherWithScopeAndBlock());
			}, (!hasScope) ? (hasBlock ? MemberDispatcher.RubyObjectMethodDispatchersWithBlock : MemberDispatcher.RubyObjectMethodDispatchers) : (hasBlock ? MemberDispatcher.RubyObjectMethodDispatchersWithScopeAndBlock : MemberDispatcher.RubyObjectMethodDispatchersWithScope));
			if (methodDispatcher != null)
			{
				methodDispatcher.Initialize(method, version);
			}
			return methodDispatcher;
		}

		internal abstract void Initialize(Delegate method, int version);
	}
	public abstract class MethodDispatcher<TRubyFunc> : MethodDispatcher
	{
		internal TRubyFunc Method;

		internal override void Initialize(Delegate method, int version)
		{
			Method = (TRubyFunc)(object)method;
			Version = version;
		}
	}
}
