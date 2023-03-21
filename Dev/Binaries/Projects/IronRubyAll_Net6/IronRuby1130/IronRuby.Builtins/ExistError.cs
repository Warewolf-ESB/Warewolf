using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Serializable]
	public class ExistError : ExternalException
	{
		private const string M = "File exists";

		public ExistError()
			: this(null, null)
		{
		}

		public ExistError(string message)
			: this(message, null)
		{
		}

		public ExistError(string message, Exception inner)
			: base(RubyExceptions.MakeMessage(message, "File exists"), inner)
		{
		}

		public ExistError(MutableString message)
			: base(RubyExceptions.MakeMessage(ref message, "File exists"))
		{
			RubyExceptionData.InitializeException(this, message);
		}

		protected ExistError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
