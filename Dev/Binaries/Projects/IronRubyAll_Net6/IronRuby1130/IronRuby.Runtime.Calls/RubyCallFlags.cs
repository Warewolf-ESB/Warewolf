namespace IronRuby.Runtime.Calls
{
	public enum RubyCallFlags
	{
		None = 0,
		HasScope = 1,
		HasSplattedArgument = 2,
		HasRhsArgument = 4,
		HasBlock = 8,
		HasImplicitSelf = 16,
		IsInteropCall = 32,
		IsVirtualCall = 64,
		IsSuperCall = 128,
		HasImplicitArguments = 128
	}
}
