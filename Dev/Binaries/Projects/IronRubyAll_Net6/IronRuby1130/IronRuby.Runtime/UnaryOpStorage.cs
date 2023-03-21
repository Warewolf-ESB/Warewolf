using System;
using System.Runtime.CompilerServices;

namespace IronRuby.Runtime
{
	public sealed class UnaryOpStorage : CallSiteStorage<Func<CallSite, object, object>>
	{
		public UnaryOpStorage(RubyContext context)
			: base(context)
		{
		}

		public CallSite<Func<CallSite, object, object>> GetCallSite(string methodName)
		{
			return GetCallSite(methodName, 0);
		}
	}
}
