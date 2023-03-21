using System.Diagnostics;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	public class RubyObjectDebugView
	{
		private readonly IRubyObject _obj;

		[DebuggerDisplay("{GetModuleName(A),nq}", Name = "{GetClassKind(),nq}", Type = "")]
		public object A
		{
			get
			{
				return _obj.ImmediateClass;
			}
		}

		[DebuggerDisplay("{B}", Name = "tainted?", Type = "")]
		public bool B
		{
			get
			{
				return _obj.IsTainted;
			}
			set
			{
				_obj.IsTainted = value;
			}
		}

		[DebuggerDisplay("{C}", Name = "untrusted?", Type = "")]
		public bool C
		{
			get
			{
				return _obj.IsUntrusted;
			}
			set
			{
				_obj.IsUntrusted = value;
			}
		}

		[DebuggerDisplay("{D}", Name = "frozen?", Type = "")]
		public bool D
		{
			get
			{
				return _obj.IsFrozen;
			}
			set
			{
				if (value)
				{
					_obj.Freeze();
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public object E
		{
			get
			{
				RubyInstanceData rubyInstanceData = _obj.TryGetInstanceData();
				if (rubyInstanceData == null)
				{
					return new RubyInstanceData.VariableDebugView[0];
				}
				return rubyInstanceData.GetInstanceVariablesDebugView(_obj.ImmediateClass.Context);
			}
		}

		public RubyObjectDebugView(IRubyObject obj)
		{
			_obj = obj;
		}

		private string GetClassKind()
		{
			if (!_obj.ImmediateClass.IsSingletonClass)
			{
				return "class";
			}
			return "singleton class";
		}

		private static string GetModuleName(object module)
		{
			RubyModule rubyModule = (RubyModule)module;
			if (rubyModule == null)
			{
				return null;
			}
			return rubyModule.GetDisplayName(rubyModule.Context, false).ToString();
		}
	}
}
