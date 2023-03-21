namespace IronRuby.Runtime
{
	public sealed class BlockReturnResult
	{
		internal static BlockReturnResult Retry = new BlockReturnResult();

		internal readonly object ReturnValue;

		internal readonly RuntimeFlowControl TargetFrame;

		private BlockReturnResult()
		{
		}

		internal BlockReturnResult(RuntimeFlowControl targetFrame, object returnValue)
		{
			TargetFrame = targetFrame;
			ReturnValue = returnValue;
		}

		public MethodUnwinder ToUnwinder()
		{
			return new MethodUnwinder(TargetFrame, ReturnValue);
		}
	}
}
