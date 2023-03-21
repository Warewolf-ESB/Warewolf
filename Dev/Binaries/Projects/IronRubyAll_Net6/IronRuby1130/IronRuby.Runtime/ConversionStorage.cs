using System;
using System.Runtime.CompilerServices;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Runtime
{
	public sealed class ConversionStorage<TResult> : CallSiteStorage<Func<CallSite, object, TResult>>
	{
		public ConversionStorage(RubyContext context)
			: base(context)
		{
		}

		public CallSite<Func<CallSite, object, TResult>> GetSite(RubyConversionAction conversion)
		{
			return RubyUtils.GetCallSite(ref Site, conversion);
		}

		internal CallSite<Func<CallSite, object, TResult>> GetDefaultConversionSite()
		{
			return RubyUtils.GetCallSite(ref Site, RubyConversionAction.GetConversionAction(base.Context, typeof(TResult), true));
		}
	}
}
