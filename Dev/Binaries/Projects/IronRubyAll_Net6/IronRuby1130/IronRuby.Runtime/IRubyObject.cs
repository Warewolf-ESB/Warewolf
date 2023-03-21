using IronRuby.Builtins;

namespace IronRuby.Runtime
{
	public interface IRubyObject : IRubyObjectState
	{
		RubyClass ImmediateClass { get; set; }

		RubyInstanceData TryGetInstanceData();

		RubyInstanceData GetInstanceData();

		int BaseGetHashCode();

		bool BaseEquals(object other);

		string BaseToString();
	}
}
