using IronRuby.Runtime;
using Microsoft.Scripting.Actions;

namespace IronRuby.Builtins
{
	[RubyClass(Extends = typeof(TypeTracker), Restrictions = ModuleRestrictions.None)]
	public static class TypeTrackerOps
	{
		[RubyMethod("to_module")]
		public static RubyModule ToModule(RubyContext context, TypeTracker self)
		{
			return context.GetModule(self.Type);
		}

		[RubyMethod("to_class")]
		public static RubyClass ToClass(RubyContext context, TypeTracker self)
		{
			if (self.Type.IsInterface)
			{
				RubyExceptions.CreateTypeError("Cannot convert a CLR interface to a Ruby class");
			}
			return context.GetClass(self.Type);
		}
	}
}
