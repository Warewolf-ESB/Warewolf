using System.Runtime.CompilerServices;
using IronRuby.Runtime.Calls;

namespace IronRuby.Runtime
{
	public class CallSiteStorage<TCallSiteFunc> : RubyCallSiteStorage where TCallSiteFunc : class
	{
		public CallSite<TCallSiteFunc> Site;

		public CallSiteStorage(RubyContext context)
			: base(context)
		{
		}

		public CallSite<TCallSiteFunc> GetCallSite(string methodName, int argumentCount)
		{
			return RubyUtils.GetCallSite(ref Site, base.Context, methodName, argumentCount);
		}

		public CallSite<TCallSiteFunc> GetCallSite(string methodName, RubyCallSignature signature)
		{
			return RubyUtils.GetCallSite(ref Site, base.Context, methodName, signature);
		}
	}
}
