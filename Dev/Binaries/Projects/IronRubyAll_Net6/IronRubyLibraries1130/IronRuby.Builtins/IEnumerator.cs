using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	internal interface IEnumerator
	{
		object Each(RubyScope scope, BlockParam block);
	}
}
