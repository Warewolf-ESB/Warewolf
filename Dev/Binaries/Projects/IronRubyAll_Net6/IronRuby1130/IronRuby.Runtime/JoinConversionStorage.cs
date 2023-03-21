using System;
using System.Collections;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Runtime
{
	public class JoinConversionStorage : RubyCallSiteStorage
	{
		private CallSite<Func<CallSite, object, MutableString>> _tosSite;

		private CallSite<Func<CallSite, object, MutableString>> _toStrSite;

		private CallSite<Func<CallSite, object, IList>> _toArySite;

		public CallSite<Func<CallSite, object, MutableString>> ToStr
		{
			get
			{
				return _toStrSite ?? (_toStrSite = RubyUtils.GetCallSite(ref _toStrSite, ProtocolConversionAction<TryConvertToStrAction>.Make(base.Context)));
			}
		}

		public CallSite<Func<CallSite, object, MutableString>> ToS
		{
			get
			{
				return _tosSite ?? (_tosSite = RubyUtils.GetCallSite(ref _tosSite, ConvertToSAction.Make(base.Context)));
			}
		}

		public CallSite<Func<CallSite, object, IList>> ToAry
		{
			get
			{
				return _toArySite ?? (_toArySite = RubyUtils.GetCallSite(ref _toArySite, ProtocolConversionAction<TryConvertToArrayAction>.Make(base.Context)));
			}
		}

		public JoinConversionStorage(RubyContext context)
			: base(context)
		{
		}
	}
}
