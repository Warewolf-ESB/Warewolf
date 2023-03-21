using Microsoft.Scripting;

namespace IronRuby.Runtime
{
	public sealed class RubyFileInitializerScope : RubyClosureScope
	{
		public override ScopeKind Kind
		{
			get
			{
				return ScopeKind.FileInitializer;
			}
		}

		public override bool InheritsLocalVariables
		{
			get
			{
				return false;
			}
		}

		internal RubyFileInitializerScope(MutableTuple locals, string[] variableNames, RubyScope parent)
		{
			_activeFlowControlScope = parent.FlowControlScope;
			_parent = parent;
			_top = parent.Top;
			_selfObject = parent.SelfObject;
			_methodAttributes = RubyMethodAttributes.PublicInstance;
			_locals = locals;
			_variableNames = variableNames;
			InLoop = parent.InLoop;
			InRescue = parent.InRescue;
		}

		internal override void DefineDynamicVariable(string name, object value)
		{
			_parent.DefineDynamicVariable(name, value);
		}
	}
}
