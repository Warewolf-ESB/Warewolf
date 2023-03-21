namespace IronRuby.Compiler
{
	public struct ErrorInfo
	{
		public readonly int Code;

		public readonly string ResourceId;

		public ErrorInfo(int code, string id)
		{
			Code = code;
			ResourceId = id;
		}

		public string GetMessage()
		{
			return ResourceId;
		}

		public string GetMessage(params object[] args)
		{
			return string.Format(GetMessage(), args);
		}
	}
}
