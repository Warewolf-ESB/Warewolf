using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("Float", Extends = typeof(double), Inherits = typeof(Numeric))]
	[UndefineMethod("new", IsStatic = true)]
	[Includes(new Type[] { typeof(Precision) })]
	[Includes(new Type[] { typeof(ClrFloat) }, Copy = true)]
	public static class FloatOps
	{
		[RubyConstant]
		public const double EPSILON = 2.2204460492503131E-16;

		[RubyConstant]
		public const double MIN = 2.2250738585072014E-308;

		[RubyConstant]
		public const double MAX = double.MaxValue;

		[RubyConstant]
		public const int DIG = 15;

		[RubyConstant]
		public const int MANT_DIG = 53;

		[RubyConstant]
		public const double MAX_10_EXP = 308.0;

		[RubyConstant]
		public const double MIN_10_EXP = -307.0;

		[RubyConstant]
		public const int MAX_EXP = 1024;

		[RubyConstant]
		public const int MIN_EXP = -1021;

		[RubyConstant]
		public const int RADIX = 2;

		[RubyConstant]
		public const int ROUNDS = 1;
	}
}
