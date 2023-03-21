using System;
using System.IO;
using System.Runtime.Serialization;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Serializable]
	[RubyException("EOFError")]
	public class EOFError : IOException
	{
		public EOFError()
			: this(null, null)
		{
		}

		public EOFError(string message)
			: this(message, null)
		{
		}

		public EOFError(string message, Exception inner)
			: base(message ?? "EOFError", inner)
		{
		}

		protected EOFError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
