using Microsoft.Scripting;
using Microsoft.Scripting.Interpreter;

namespace IronRuby.Runtime
{
	public sealed class RubyBlockScope : RubyScope
	{
		private readonly BlockParam _blockFlowControl;

		public override ScopeKind Kind
		{
			get
			{
				if (_blockFlowControl.IsMethod)
				{
					return ScopeKind.BlockMethod;
				}
				if (_blockFlowControl.MethodLookupModule == null)
				{
					return ScopeKind.Block;
				}
				return ScopeKind.BlockModule;
			}
		}

		public override bool InheritsLocalVariables
		{
			get
			{
				return true;
			}
		}

		public BlockParam BlockFlowControl
		{
			get
			{
				return _blockFlowControl;
			}
		}

		internal RubyBlockScope(MutableTuple locals, string[] variableNames, BlockParam blockFlowControl, object selfObject, InterpretedFrame interpretedFrame)
		{
			RubyScope localScope = blockFlowControl.Proc.LocalScope;
			_activeFlowControlScope = localScope.FlowControlScope;
			_parent = localScope;
			_top = localScope.Top;
			_selfObject = selfObject;
			_methodAttributes = RubyMethodAttributes.PublicInstance;
			_locals = locals;
			_variableNames = variableNames;
			base.InterpretedFrame = interpretedFrame;
			_blockFlowControl = blockFlowControl;
		}
	}
}
