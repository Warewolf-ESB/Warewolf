using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("Range", Extends = typeof(Range), Inherits = typeof(object))]
	[Includes(new Type[] { typeof(Enumerable) })]
	public static class RangeOps
	{
		public class EachStorage : ComparisonStorage
		{
			private CallSite<Func<CallSite, object, MutableString>> _stringCast;

			private CallSite<Func<CallSite, object, object, object>> _equalsSite;

			private CallSite<Func<CallSite, object, object>> _succSite;

			private CallSite<Func<CallSite, object, object, object>> _respondToSite;

			public CallSite<Func<CallSite, object, MutableString>> StringCastSite
			{
				get
				{
					return RubyUtils.GetCallSite(ref _stringCast, ProtocolConversionAction<ConvertToStrAction>.Make(base.Context));
				}
			}

			public CallSite<Func<CallSite, object, object, object>> RespondToSite
			{
				get
				{
					return RubyUtils.GetCallSite(ref _respondToSite, base.Context, "respond_to?", 1);
				}
			}

			public CallSite<Func<CallSite, object, object, object>> EqualsSite
			{
				get
				{
					return RubyUtils.GetCallSite(ref _equalsSite, base.Context, "==", 1);
				}
			}

			public CallSite<Func<CallSite, object, object>> SuccSite
			{
				get
				{
					return RubyUtils.GetCallSite(ref _succSite, base.Context, "succ", 0);
				}
			}

			public EachStorage(RubyContext context)
				: base(context)
			{
			}
		}

		public class StepStorage : EachStorage
		{
			private CallSite<Func<CallSite, object, int>> _fixnumCast;

			private CallSite<Func<CallSite, object, object, object>> _lessThanEqualsSite;

			private CallSite<Func<CallSite, object, object, object>> _addSite;

			public CallSite<Func<CallSite, object, int>> FixnumCastSite
			{
				get
				{
					return RubyUtils.GetCallSite(ref _fixnumCast, ProtocolConversionAction<ConvertToFixnumAction>.Make(base.Context));
				}
			}

			public CallSite<Func<CallSite, object, object, object>> LessThanEqualsSite
			{
				get
				{
					return RubyUtils.GetCallSite(ref _lessThanEqualsSite, base.Context, "<=", 1);
				}
			}

			public CallSite<Func<CallSite, object, object, object>> AddSite
			{
				get
				{
					return RubyUtils.GetCallSite(ref _addSite, base.Context, "+", 1);
				}
			}

			public StepStorage(RubyContext context)
				: base(context)
			{
			}
		}

		[RubyConstructor]
		public static Range CreateRange(BinaryOpStorage comparisonStorage, RubyClass self, object begin, object end, [Optional] bool excludeEnd)
		{
			return new Range(comparisonStorage, self.Context, begin, end, excludeEnd);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static Range Reinitialize(BinaryOpStorage comparisonStorage, RubyContext context, Range self, object begin, object end, [Optional] bool excludeEnd)
		{
			self.Initialize(comparisonStorage, context, begin, end, excludeEnd);
			return self;
		}

		[RubyMethod("first")]
		[RubyMethod("begin")]
		public static object Begin([NotNull] Range self)
		{
			return self.Begin;
		}

		[RubyMethod("last")]
		[RubyMethod("end")]
		public static object End([NotNull] Range self)
		{
			return self.End;
		}

		[RubyMethod("exclude_end?")]
		public static bool ExcludeEnd([NotNull] Range self)
		{
			return self.ExcludeEnd;
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(RubyContext context, Range self)
		{
			return self.Inspect(context);
		}

		[RubyMethod("to_s")]
		public static MutableString ToS(ConversionStorage<MutableString> tosConversion, Range self)
		{
			return self.ToMutableString(tosConversion);
		}

		[RubyMethod("==")]
		[RubyMethod("eql?")]
		public static bool Equals(Range self, object other)
		{
			return false;
		}

		[RubyMethod("==")]
		public static bool Equals(BinaryOpStorage equals, Range self, [NotNull] Range other)
		{
			if (self == other)
			{
				return true;
			}
			if (Protocols.IsEqual(equals, self.Begin, other.Begin) && Protocols.IsEqual(equals, self.End, other.End))
			{
				return self.ExcludeEnd == other.ExcludeEnd;
			}
			return false;
		}

		[RubyMethod("eql?")]
		public static bool Eql(BinaryOpStorage equalsStorage, Range self, [NotNull] Range other)
		{
			if (self == other)
			{
				return true;
			}
			CallSite<Func<CallSite, object, object, object>> callSite = equalsStorage.GetCallSite("eql?");
			if (Protocols.IsTrue(callSite.Target(callSite, self.Begin, other.Begin)) && Protocols.IsTrue(callSite.Target(callSite, self.End, other.End)))
			{
				return self.ExcludeEnd == other.ExcludeEnd;
			}
			return false;
		}

		[RubyMethod("===")]
		[RubyMethod("include?")]
		[RubyMethod("member?")]
		public static bool CaseEquals(ComparisonStorage comparisonStorage, [NotNull] Range self, object value)
		{
			CallSite<Func<CallSite, object, object, object>> compareSite = comparisonStorage.CompareSite;
			object obj = compareSite.Target(compareSite, self.Begin, value);
			if (obj == null || Protocols.ConvertCompareResult(comparisonStorage, obj) > 0)
			{
				return false;
			}
			obj = compareSite.Target(compareSite, value, self.End);
			if (obj == null)
			{
				return false;
			}
			int num = Protocols.ConvertCompareResult(comparisonStorage, obj);
			if (num >= 0)
			{
				if (!self.ExcludeEnd)
				{
					return num == 0;
				}
				return false;
			}
			return true;
		}

		[RubyMethod("hash")]
		public static int GetHashCode(UnaryOpStorage hashStorage, Range self)
		{
			CallSite<Func<CallSite, object, object>> callSite = hashStorage.GetCallSite("hash");
			return Protocols.ToHashCode(callSite.Target(callSite, self.Begin)) ^ Protocols.ToHashCode(callSite.Target(callSite, self.End)) ^ (self.ExcludeEnd ? 179425693 : 1794210891);
		}

		[RubyMethod("each")]
		public static Enumerator GetEachEnumerator(EachStorage storage, Range self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => Each(storage, block, self));
		}

		[RubyMethod("each")]
		public static object Each(EachStorage storage, [NotNull] BlockParam block, Range self)
		{
			if (self.Begin is int && self.End is int)
			{
				return StepFixnum(block, self, (int)self.Begin, (int)self.End, 1);
			}
			if (self.Begin is MutableString)
			{
				return StepString(storage, block, self, (MutableString)self.Begin, (MutableString)self.End, 1);
			}
			return StepObject(storage, block, self, self.Begin, self.End, 1);
		}

		[RubyMethod("step")]
		public static Enumerator GetStepEnumerator(StepStorage storage, Range self, [Optional] object step)
		{
			return new Enumerator((RubyScope _, BlockParam block) => Step(storage, block, self, step));
		}

		[RubyMethod("step")]
		public static object Step(StepStorage storage, [NotNull] BlockParam block, Range self, [Optional] object step)
		{
			if (step == Missing.Value)
			{
				step = ClrInteger.One;
			}
			if (self.Begin is int && self.End is int)
			{
				CallSite<Func<CallSite, object, int>> fixnumCastSite = storage.FixnumCastSite;
				int step2 = fixnumCastSite.Target(fixnumCastSite, step);
				return StepFixnum(block, self, (int)self.Begin, (int)self.End, step2);
			}
			if (self.Begin is MutableString)
			{
				CallSite<Func<CallSite, object, int>> fixnumCastSite2 = storage.FixnumCastSite;
				int step3 = fixnumCastSite2.Target(fixnumCastSite2, step);
				return StepString(storage, block, self, (MutableString)self.Begin, (MutableString)self.End, step3);
			}
			if (storage.Context.IsInstanceOf(self.Begin, storage.Context.GetClass(typeof(Numeric))))
			{
				return StepNumeric(storage, block, self, self.Begin, self.End, step);
			}
			CallSite<Func<CallSite, object, int>> fixnumCastSite3 = storage.FixnumCastSite;
			int step4 = fixnumCastSite3.Target(fixnumCastSite3, step);
			return StepObject(storage, block, self, self.Begin, self.End, step4);
		}

		private static object StepFixnum(BlockParam block, Range self, int begin, int end, int step)
		{
			CheckStep(step);
			int i;
			object blockResult;
			for (i = begin; i < end; i += step)
			{
				if (block.Yield(i, out blockResult))
				{
					return blockResult;
				}
			}
			if (i == end && !self.ExcludeEnd && block.Yield(i, out blockResult))
			{
				return blockResult;
			}
			return self;
		}

		private static object StepString(EachStorage storage, BlockParam block, Range self, MutableString begin, MutableString end, int step)
		{
			CheckStep(step);
			MutableString mutableString = begin;
			int num;
			object blockResult;
			while ((num = Protocols.Compare(storage, mutableString, end)) < 0)
			{
				if (block.Yield(mutableString.Clone(), out blockResult))
				{
					return blockResult;
				}
				if (object.ReferenceEquals(mutableString, begin))
				{
					mutableString = mutableString.Clone();
				}
				for (int i = 0; i < step; i++)
				{
					MutableStringOps.SuccInPlace(mutableString);
				}
				if (mutableString.Length > end.Length)
				{
					return self;
				}
			}
			if (num == 0 && !self.ExcludeEnd && block.Yield(mutableString.Clone(), out blockResult))
			{
				return blockResult;
			}
			return self;
		}

		private static object StepNumeric(StepStorage storage, BlockParam block, Range self, object begin, object end, object step)
		{
			CheckStep(storage, step);
			object obj = begin;
			CallSite<Func<CallSite, object, object, object>> callSite = (self.ExcludeEnd ? storage.LessThanSite : storage.LessThanEqualsSite);
			while (RubyOps.IsTrue(callSite.Target(callSite, obj, end)))
			{
				object blockResult;
				if (block.Yield(obj, out blockResult))
				{
					return blockResult;
				}
				CallSite<Func<CallSite, object, object, object>> addSite = storage.AddSite;
				obj = addSite.Target(addSite, obj, step);
			}
			return self;
		}

		private static object StepObject(EachStorage storage, BlockParam block, Range self, object begin, object end, int step)
		{
			CheckStep(storage, step);
			CheckBegin(storage, self.Begin);
			object obj = begin;
			CallSite<Func<CallSite, object, object>> succSite = storage.SuccSite;
			int num;
			object blockResult;
			while ((num = Protocols.Compare(storage, obj, end)) < 0)
			{
				if (block.Yield(obj, out blockResult))
				{
					return blockResult;
				}
				for (int i = 0; i < step; i++)
				{
					obj = succSite.Target(succSite, obj);
				}
			}
			if (num == 0 && !self.ExcludeEnd && block.Yield(obj, out blockResult))
			{
				return blockResult;
			}
			return self;
		}

		private static void CheckStep(int step)
		{
			if (step == 0)
			{
				throw RubyExceptions.CreateArgumentError("step can't be 0");
			}
			if (step < 0)
			{
				throw RubyExceptions.CreateArgumentError("step can't be negative");
			}
		}

		private static void CheckStep(EachStorage storage, object step)
		{
			CallSite<Func<CallSite, object, object, object>> equalsSite = storage.EqualsSite;
			if (Protocols.IsTrue(equalsSite.Target(equalsSite, step, 0)))
			{
				throw RubyExceptions.CreateArgumentError("step can't be 0");
			}
			CallSite<Func<CallSite, object, object, object>> lessThanSite = storage.LessThanSite;
			if (RubyOps.IsTrue(lessThanSite.Target(lessThanSite, step, 0)))
			{
				throw RubyExceptions.CreateArgumentError("step can't be negative");
			}
		}

		private static void CheckBegin(EachStorage storage, object begin)
		{
			if (!Protocols.RespondTo(storage.RespondToSite, storage.Context, begin, "succ"))
			{
				throw RubyExceptions.CreateTypeError("can't iterate from {0}", storage.Context.GetClassDisplayName(begin));
			}
		}
	}
}
