using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Serializable]
	public class ExecFormatError : ExternalException
	{
		private const string M = "Exec format error";

		public ExecFormatError()
			: this(null, null)
		{
		}

		public ExecFormatError(string message)
			: this(message, null)
		{
		}

		public ExecFormatError(string message, Exception inner)
			: base(RubyExceptions.MakeMessage(message, "Exec format error"), inner)
		{
		}

		public ExecFormatError(MutableString message)
			: base(RubyExceptions.MakeMessage(ref message, "Exec format error"))
		{
			RubyExceptionData.InitializeException(this, message);
		}

		protected ExecFormatError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
