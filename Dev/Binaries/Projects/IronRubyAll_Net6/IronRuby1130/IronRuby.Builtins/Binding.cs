using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	public sealed class Binding
	{
		private readonly RubyScope _localScope;

		private readonly object _self;

		public RubyScope LocalScope
		{
			get
			{
				return _localScope;
			}
		}

		public object SelfObject
		{
			get
			{
				return _self;
			}
		}

		public Binding(RubyScope localScope)
			: this(localScope, localScope.SelfObject)
		{
		}

		public Binding(RubyScope localScope, object self)
		{
			_localScope = localScope;
			_self = self;
		}
	}
}
