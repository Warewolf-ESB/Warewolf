using System;
using System.Runtime.CompilerServices;

namespace IronRuby.Runtime
{
	public sealed class BinaryOpStorage : CallSiteStorage<Func<CallSite, object, object, object>>
	{
		public BinaryOpStorage(RubyContext context)
			: base(context)
		{
		}

		public CallSite<Func<CallSite, object, object, object>> GetCallSite(string methodName)
		{
			return GetCallSite(methodName, 1);
		}
	}
}
