using System.Reflection;

namespace IronRuby.Runtime
{
	public sealed class MethodUnwinder : StackUnwinder
	{
		public readonly RuntimeFlowControl TargetFrame;

		internal static FieldInfo TargetFrameField
		{
			get
			{
				return typeof(MethodUnwinder).GetField("TargetFrame");
			}
		}

		internal MethodUnwinder(RuntimeFlowControl targetFrame, object returnValue)
			: base(returnValue)
		{
			TargetFrame = targetFrame;
		}
	}
}
