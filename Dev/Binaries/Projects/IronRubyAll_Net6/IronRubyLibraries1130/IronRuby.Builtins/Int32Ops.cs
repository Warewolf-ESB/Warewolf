using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(ClrInteger) }, Copy = true)]
	[RubyClass("Fixnum", Extends = typeof(int), Inherits = typeof(Integer))]
	[UndefineMethod("new", IsStatic = true)]
	public static class Int32Ops
	{
		[RubyMethod("to_sym")]
		public static object ToSymbol(RubyContext context, int self)
		{
			return context.FindSymbol(self);
		}

		[RubyMethod("id2name")]
		public static object Id2Name(RubyContext context, int self)
		{
			RubySymbol rubySymbol = context.FindSymbol(self);
			if (rubySymbol == null)
			{
				return null;
			}
			return rubySymbol.String.Clone();
		}

		[RubyMethod("size")]
		public static int Size(int self)
		{
			return 4;
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static int InducedFrom(RubyClass self, [DefaultProtocol] int value)
		{
			return value;
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static int InducedFrom(RubyClass self, double value)
		{
			if (value >= -2147483648.0 && value <= 2147483647.0)
			{
				return (int)value;
			}
			throw RubyExceptions.CreateRangeError("Float {0} out of range of {1}", value, self.Name);
		}
	}
}
