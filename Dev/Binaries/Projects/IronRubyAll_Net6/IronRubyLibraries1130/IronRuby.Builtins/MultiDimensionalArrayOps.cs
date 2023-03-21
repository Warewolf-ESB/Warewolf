using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule("MultiDimensionalArray", DefineIn = typeof(IronRubyOps.Clr), Extends = typeof(MultiDimensionalArray), Restrictions = ModuleRestrictions.NoUnderlyingType)]
	public static class MultiDimensionalArrayOps
	{
	}
}
