using IronRuby.Builtins;

namespace IronRuby.Runtime
{
	public sealed class RubyModuleScope : RubyClosureScope
	{
		private readonly RubyModule _module;

		public override ScopeKind Kind
		{
			get
			{
				return ScopeKind.Module;
			}
		}

		public override bool InheritsLocalVariables
		{
			get
			{
				return false;
			}
		}

		public override RubyModule Module
		{
			get
			{
				return _module;
			}
		}

		internal RubyModuleScope(RubyScope parent, RubyModule module)
		{
			_activeFlowControlScope = parent.FlowControlScope;
			_parent = parent;
			_top = parent.Top;
			_selfObject = module;
			_methodAttributes = RubyMethodAttributes.PrivateInstance;
			_module = module;
			InLoop = parent.InLoop;
			InRescue = parent.InRescue;
			base.MethodAttributes = RubyMethodAttributes.PublicInstance;
		}
	}
}
