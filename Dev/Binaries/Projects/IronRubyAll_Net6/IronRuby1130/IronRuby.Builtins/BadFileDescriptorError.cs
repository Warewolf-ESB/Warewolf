using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Serializable]
	public class BadFileDescriptorError : ExternalException
	{
		private const string M = "Bad file descriptor";

		public BadFileDescriptorError()
			: this(null, null)
		{
		}

		public BadFileDescriptorError(string message)
			: this(message, null)
		{
		}

		public BadFileDescriptorError(string message, Exception inner)
			: base(RubyExceptions.MakeMessage(message, "Bad file descriptor"), inner)
		{
		}

		public BadFileDescriptorError(MutableString message)
			: base(RubyExceptions.MakeMessage(ref message, "Bad file descriptor"))
		{
			RubyExceptionData.InitializeException(this, message);
		}

		protected BadFileDescriptorError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
