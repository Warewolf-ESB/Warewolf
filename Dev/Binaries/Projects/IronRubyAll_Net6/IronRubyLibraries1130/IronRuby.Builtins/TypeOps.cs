using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass(Extends = typeof(Type), Restrictions = ModuleRestrictions.None)]
	public static class TypeOps
	{
		[RubyMethod("to_module")]
		public static RubyModule ToModule(RubyContext context, Type self)
		{
			return context.GetModule(self);
		}

		[RubyMethod("to_class")]
		public static RubyClass ToClass(RubyContext context, Type self)
		{
			if (self.IsInterface)
			{
				RubyExceptions.CreateTypeError("Cannot convert a CLR interface to a Ruby class");
			}
			return context.GetClass(self);
		}
	}
}
