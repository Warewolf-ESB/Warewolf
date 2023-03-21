using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Serializable]
	public class InvalidError : ExternalException
	{
		private const string M = "Invalid argument";

		public InvalidError()
			: this(null, null)
		{
		}

		public InvalidError(string message)
			: this(message, null)
		{
		}

		public InvalidError(string message, Exception inner)
			: base(RubyExceptions.MakeMessage(message, "Invalid argument"), inner)
		{
		}

		public InvalidError(MutableString message)
			: base(RubyExceptions.MakeMessage(ref message, "Invalid argument"))
		{
			RubyExceptionData.InitializeException(this, message);
		}

		protected InvalidError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
