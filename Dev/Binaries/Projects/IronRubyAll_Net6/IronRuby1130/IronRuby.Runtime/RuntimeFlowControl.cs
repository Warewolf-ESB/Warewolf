using IronRuby.Builtins;

namespace IronRuby.Runtime
{
	public class RuntimeFlowControl
	{
		internal RuntimeFlowControl _activeFlowControlScope;

		internal bool IsActiveMethod
		{
			get
			{
				return _activeFlowControlScope == this;
			}
		}

		internal RuntimeFlowControl FlowControlScope
		{
			get
			{
				return _activeFlowControlScope ?? this;
			}
		}

		internal void InitializeRfc(Proc proc)
		{
			if (proc != null && proc.Kind == ProcKind.Block)
			{
				proc.Kind = ProcKind.Proc;
				proc.Converter = this;
			}
		}

		internal RuntimeFlowControl()
		{
		}

		internal void LeaveMethod()
		{
			_activeFlowControlScope = null;
		}
	}
}
