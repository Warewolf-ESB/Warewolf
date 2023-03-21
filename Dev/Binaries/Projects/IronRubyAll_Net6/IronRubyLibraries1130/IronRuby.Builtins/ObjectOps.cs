using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(Kernel) })]
	[RubyClass("Object", Extends = typeof(object), Inherits = typeof(BasicObject), Restrictions = (ModuleRestrictions.NoNameMapping | ModuleRestrictions.NotPublished))]
	public static class ObjectOps
	{
		[RubyConstant]
		public static readonly bool TRUE = true;

		[RubyConstant]
		public static readonly bool FALSE = false;

		[RubyConstant]
		public static readonly object NIL = null;

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
		public static object Reinitialize(object self, params object[] args)
		{
			return self;
		}

		[RubyConstant("___Numerics__")]
		public static object Numerics(RubyModule self)
		{
			self.SetAutoloadedConstant("Rational", MutableString.CreateAscii("rational18.rb"));
			self.SetAutoloadedConstant("Complex", MutableString.CreateAscii("complex18.rb"));
			return null;
		}
	}
}
