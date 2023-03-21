using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("Binding", Extends = typeof(Binding))]
	[UndefineMethod("new", IsStatic = true)]
	[UndefineMethod("LocalScope")]
	public static class BindingOps
	{
	}
}
