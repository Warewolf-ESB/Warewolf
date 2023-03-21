using IronRuby.Builtins;

namespace IronRuby.Runtime
{
	public sealed class RubyModuleEvalScope : RubyClosureScope
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
				return true;
			}
		}

		public override RubyModule Module
		{
			get
			{
				return _module;
			}
		}

		internal RubyModuleEvalScope(RubyScope parent, RubyModule module, object selfObject)
		{
			_activeFlowControlScope = parent.FlowControlScope;
			_parent = parent;
			_top = parent.Top;
			_selfObject = selfObject;
			_methodAttributes = RubyMethodAttributes.PublicInstance;
			_module = module;
			InLoop = parent.InLoop;
			InRescue = parent.InRescue;
			SetEmptyLocals();
		}

		internal override void DefineDynamicVariable(string name, object value)
		{
			_parent.DefineDynamicVariable(name, value);
		}
	}
}
