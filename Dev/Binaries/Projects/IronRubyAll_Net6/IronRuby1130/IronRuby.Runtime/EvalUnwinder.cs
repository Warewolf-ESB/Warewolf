using System.Reflection;
using IronRuby.Builtins;

namespace IronRuby.Runtime
{
	public sealed class EvalUnwinder : StackUnwinder
	{
		private readonly RuntimeFlowControl _targetFrame;

		private readonly ProcKind _sourceProcKind;

		public readonly BlockReturnReason Reason;

		internal static FieldInfo ReasonField
		{
			get
			{
				return typeof(EvalUnwinder).GetField("Reason");
			}
		}

		internal RuntimeFlowControl TargetFrame
		{
			get
			{
				return _targetFrame;
			}
		}

		internal ProcKind SourceProcKind
		{
			get
			{
				return _sourceProcKind;
			}
		}

		internal EvalUnwinder(BlockReturnReason reason, object returnValue)
			: this(reason, null, ProcKind.Block, returnValue)
		{
		}

		internal EvalUnwinder(BlockReturnReason reason, RuntimeFlowControl targetFrame, ProcKind sourceProcKind, object returnValue)
			: base(returnValue)
		{
			Reason = reason;
			_targetFrame = targetFrame;
			_sourceProcKind = sourceProcKind;
		}
	}
}
