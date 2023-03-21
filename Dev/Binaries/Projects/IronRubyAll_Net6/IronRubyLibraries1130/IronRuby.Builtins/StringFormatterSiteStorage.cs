using System;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Builtins
{
	public sealed class StringFormatterSiteStorage : RubyCallSiteStorage
	{
		private CallSite<Func<CallSite, object, int>> _fixnumCast;

		private CallSite<Func<CallSite, object, double>> _tofConversion;

		private CallSite<Func<CallSite, object, MutableString>> _tosConversion;

		private CallSite<Func<CallSite, object, IntegerValue>> _integerConversion;

		public StringFormatterSiteStorage(RubyContext context)
			: base(context)
		{
		}

		public int CastToFixnum(object value)
		{
			CallSite<Func<CallSite, object, int>> callSite = RubyUtils.GetCallSite(ref _fixnumCast, ProtocolConversionAction<ConvertToFixnumAction>.Make(base.Context));
			return callSite.Target(callSite, value);
		}

		public double CastToDouble(object value)
		{
			CallSite<Func<CallSite, object, double>> callSite = RubyUtils.GetCallSite(ref _tofConversion, ProtocolConversionAction<ConvertToFAction>.Make(base.Context));
			return callSite.Target(callSite, value);
		}

		public MutableString ConvertToString(object value)
		{
			CallSite<Func<CallSite, object, MutableString>> callSite = RubyUtils.GetCallSite(ref _tosConversion, ConvertToSAction.Make(base.Context));
			return callSite.Target(callSite, value);
		}

		public IntegerValue ConvertToInteger(object value)
		{
			CallSite<Func<CallSite, object, IntegerValue>> callSite = RubyUtils.GetCallSite(ref _integerConversion, CompositeConversionAction.Make(base.Context, CompositeConversion.ToIntToI));
			return callSite.Target(callSite, value);
		}
	}
}
