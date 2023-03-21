using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using IronRuby.Runtime;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Builtins
{
	[RubyClass("Numeric", Inherits = typeof(object))]
	[Includes(new Type[] { typeof(Comparable) })]
	public class Numeric : RubyObject
	{
		public Numeric(RubyClass cls)
			: base(cls)
		{
		}

		protected Numeric(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		[RubyMethod("+@")]
		public static object UnaryPlus(object self)
		{
			return self;
		}

		[RubyMethod("-@")]
		public static object UnaryMinus(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "-", 0, self);
		}

		[RubyMethod("<=>")]
		public static object Compare(object self, object other)
		{
			if (self == other)
			{
				return 0;
			}
			return null;
		}

		[RubyMethod("abs")]
		public static object Abs(BinaryOpStorage lessThanStorage, UnaryOpStorage minusStorage, object self)
		{
			CallSite<Func<CallSite, object, object, object>> callSite = lessThanStorage.GetCallSite("<");
			if (RubyOps.IsTrue(callSite.Target(callSite, self, 0)))
			{
				CallSite<Func<CallSite, object, object>> callSite2 = minusStorage.GetCallSite("-@");
				return callSite2.Target(callSite2, self);
			}
			return self;
		}

		[RubyMethod("coerce")]
		public static RubyArray Coerce(int self, int other)
		{
			return RubyOps.MakeArray2(other, self);
		}

		[RubyMethod("coerce")]
		public static RubyArray Coerce(double self, double other)
		{
			return RubyOps.MakeArray2(other, self);
		}

		[RubyMethod("coerce")]
		public static RubyArray Coerce(ConversionStorage<double> tof1, ConversionStorage<double> tof2, object self, object other)
		{
			RubyContext context = tof1.Context;
			if (context.GetClassOf(self) == context.GetClassOf(other))
			{
				return RubyOps.MakeArray2(other, self);
			}
			CallSite<Func<CallSite, object, double>> site = tof1.GetSite(ProtocolConversionAction<ConvertToFAction>.Make(context));
			CallSite<Func<CallSite, object, double>> site2 = tof2.GetSite(ProtocolConversionAction<ConvertToFAction>.Make(context));
			return RubyOps.MakeArray2(site.Target(site, other), site2.Target(site2, self));
		}

		[RubyMethod("div")]
		public static object Div(BinaryOpStorage divideStorage, ConversionStorage<double> tofStorage, object self, object other)
		{
			CallSite<Func<CallSite, object, object, object>> callSite = divideStorage.GetCallSite("/");
			CallSite<Func<CallSite, object, double>> site = tofStorage.GetSite(ProtocolConversionAction<ConvertToFAction>.Make(tofStorage.Context));
			return ClrFloat.Floor(site.Target(site, callSite.Target(callSite, self, other)));
		}

		[RubyMethod("divmod")]
		public static RubyArray DivMod(BinaryOpStorage divideStorage, BinaryOpStorage moduloStorage, ConversionStorage<double> tofStorage, object self, object other)
		{
			object item = Div(divideStorage, tofStorage, self, other);
			CallSite<Func<CallSite, object, object, object>> callSite = moduloStorage.GetCallSite("modulo");
			object item2 = callSite.Target(callSite, self, other);
			return RubyOps.MakeArray2(item, item2);
		}

		[RubyMethod("eql?")]
		public static bool Eql(BinaryOpStorage equals, object self, object other)
		{
			if (equals.Context.GetClassOf(self) != equals.Context.GetClassOf(other))
			{
				return false;
			}
			return Protocols.IsEqual(equals, self, other);
		}

		[RubyMethod("round")]
		public static object Round([DefaultProtocol] double self)
		{
			return ClrFloat.Round(self);
		}

		[RubyMethod("floor")]
		public static object Floor([DefaultProtocol] double self)
		{
			return ClrFloat.Floor(self);
		}

		[RubyMethod("ceil")]
		public static object Ceil([DefaultProtocol] double self)
		{
			return ClrFloat.Ceil(self);
		}

		[RubyMethod("truncate")]
		public static object Truncate([DefaultProtocol] double self)
		{
			return ClrFloat.ToInt(self);
		}

		[RubyMethod("integer?")]
		public static bool IsInteger(object self)
		{
			return false;
		}

		[RubyMethod("modulo")]
		public static object Modulo(BinaryOpStorage moduloStorage, object self, object other)
		{
			CallSite<Func<CallSite, object, object, object>> callSite = moduloStorage.GetCallSite("%");
			return callSite.Target(callSite, self, other);
		}

		[RubyMethod("nonzero?")]
		public static object IsNonZero(UnaryOpStorage isZeroStorage, object self)
		{
			CallSite<Func<CallSite, object, object>> callSite = isZeroStorage.GetCallSite("zero?");
			if (!Protocols.IsTrue(callSite.Target(callSite, self)))
			{
				return self;
			}
			return null;
		}

		[RubyMethod("quo")]
		public static object Quo(BinaryOpStorage divideStorage, object self, object other)
		{
			CallSite<Func<CallSite, object, object, object>> callSite = divideStorage.GetCallSite("/");
			return callSite.Target(callSite, self, other);
		}

		[RubyMethod("remainder")]
		public static object Remainder(BinaryOpStorage equals, BinaryOpStorage greaterThanStorage, BinaryOpStorage lessThanStorage, BinaryOpStorage minusStorage, BinaryOpStorage moduloStorage, object self, object other)
		{
			CallSite<Func<CallSite, object, object, object>> callSite = moduloStorage.GetCallSite("%");
			object obj = callSite.Target(callSite, self, other);
			if (!Protocols.IsEqual(equals, obj, 0))
			{
				CallSite<Func<CallSite, object, object, object>> callSite2 = greaterThanStorage.GetCallSite(">");
				CallSite<Func<CallSite, object, object, object>> callSite3 = lessThanStorage.GetCallSite("<");
				if ((RubyOps.IsTrue(callSite3.Target(callSite3, self, 0)) && RubyOps.IsTrue(callSite2.Target(callSite2, other, 0))) || (RubyOps.IsTrue(callSite2.Target(callSite2, self, 0)) && RubyOps.IsTrue(callSite3.Target(callSite3, other, 0))))
				{
					CallSite<Func<CallSite, object, object, object>> callSite4 = minusStorage.GetCallSite("-");
					return callSite4.Target(callSite4, obj, other);
				}
			}
			return obj;
		}

		[RubyMethod("step")]
		public static object Step(BlockParam block, int self, int limit)
		{
			return Step(block, self, limit, 1);
		}

		[RubyMethod("step")]
		public static object Step(BlockParam block, int self, int limit, int step)
		{
			if (step == 0)
			{
				throw RubyExceptions.CreateArgumentError("step can't be 0");
			}
			if (step > 0)
			{
				for (int i = self; i <= limit; i += step)
				{
					object result;
					if (YieldStep(block, i, out result))
					{
						return result;
					}
				}
			}
			else
			{
				for (int j = self; j >= limit; j += step)
				{
					object result2;
					if (YieldStep(block, j, out result2))
					{
						return result2;
					}
				}
			}
			return self;
		}

		[RubyMethod("step")]
		public static object Step(BlockParam block, double self, double limit, double step)
		{
			if (step == 0.0)
			{
				throw RubyExceptions.CreateArgumentError("step can't be 0");
			}
			double num = (limit - self) / step;
			int num2 = (int)Math.Floor(num + num * double.Epsilon) + 1;
			double num3 = self;
			while (num2 > 0)
			{
				object result;
				if (YieldStep(block, num3, out result))
				{
					return result;
				}
				num3 += step;
				num2--;
			}
			return self;
		}

		[RubyMethod("step")]
		public static object Step(BinaryOpStorage equals, BinaryOpStorage greaterThanStorage, BinaryOpStorage lessThanStorage, BinaryOpStorage addStorage, ConversionStorage<double> tofStorage, BlockParam block, object self, object limit, [Optional] object step)
		{
			if (step == Missing.Value)
			{
				step = ClrInteger.One;
			}
			if (self is double || limit is double || step is double)
			{
				CallSite<Func<CallSite, object, double>> site = tofStorage.GetSite(ProtocolConversionAction<ConvertToFAction>.Make(tofStorage.Context));
				double self2 = ((self is double) ? ((double)self) : site.Target(site, self));
				double limit2 = ((limit is double) ? ((double)self) : site.Target(site, limit));
				double step2 = ((step is double) ? ((double)self) : site.Target(site, step));
				return Step(block, self2, limit2, step2);
			}
			if (Protocols.IsEqual(equals, step, 0))
			{
				throw RubyExceptions.CreateArgumentError("step can't be 0");
			}
			CallSite<Func<CallSite, object, object, object>> callSite = greaterThanStorage.GetCallSite(">");
			CallSite<Func<CallSite, object, object, object>> callSite2 = (RubyOps.IsTrue(callSite.Target(callSite, step, 0)) ? callSite : lessThanStorage.GetCallSite("<"));
			object obj = self;
			while (!RubyOps.IsTrue(callSite2.Target(callSite2, obj, limit)))
			{
				object result;
				if (YieldStep(block, obj, out result))
				{
					return result;
				}
				CallSite<Func<CallSite, object, object, object>> callSite3 = addStorage.GetCallSite("+");
				obj = callSite3.Target(callSite3, obj, step);
			}
			return self;
		}

		private static bool YieldStep(BlockParam block, object current, out object result)
		{
			if (block == null)
			{
				throw RubyExceptions.NoBlockGiven();
			}
			return block.Yield(current, out result);
		}

		[RubyMethod("to_int")]
		public static object ToInt(UnaryOpStorage toiStorage, object self)
		{
			CallSite<Func<CallSite, object, object>> callSite = toiStorage.GetCallSite("to_i");
			return callSite.Target(callSite, self);
		}

		[RubyMethod("zero?")]
		public static bool IsZero(BinaryOpStorage equals, object self)
		{
			return Protocols.IsEqual(equals, self, 0);
		}
	}
}
