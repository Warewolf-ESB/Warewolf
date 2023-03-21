using System;
using System.Runtime.CompilerServices;

namespace IronRuby.Runtime
{
	public class ComparisonStorage : RubyCallSiteStorage
	{
		private CallSite<Func<CallSite, object, object, object>> _compareSite;

		private CallSite<Func<CallSite, object, object, object>> _lessThanSite;

		private CallSite<Func<CallSite, object, object, object>> _greaterThanSite;

		public CallSite<Func<CallSite, object, object, object>> CompareSite
		{
			get
			{
				return RubyUtils.GetCallSite(ref _compareSite, base.Context, "<=>", 1);
			}
		}

		public CallSite<Func<CallSite, object, object, object>> LessThanSite
		{
			get
			{
				return RubyUtils.GetCallSite(ref _lessThanSite, base.Context, "<", 1);
			}
		}

		public CallSite<Func<CallSite, object, object, object>> GreaterThanSite
		{
			get
			{
				return RubyUtils.GetCallSite(ref _greaterThanSite, base.Context, ">", 1);
			}
		}

		public ComparisonStorage(RubyContext context)
			: base(context)
		{
		}
	}
}
