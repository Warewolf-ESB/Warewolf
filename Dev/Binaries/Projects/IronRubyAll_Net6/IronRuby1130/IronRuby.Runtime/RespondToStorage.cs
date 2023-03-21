using System;
using System.Runtime.CompilerServices;

namespace IronRuby.Runtime
{
	public sealed class RespondToStorage : CallSiteStorage<Func<CallSite, object, object, object>>
	{
		public RespondToStorage(RubyContext context)
			: base(context)
		{
		}

		public CallSite<Func<CallSite, object, object, object>> GetCallSite()
		{
			return GetCallSite("respond_to?", 1);
		}
	}
}
