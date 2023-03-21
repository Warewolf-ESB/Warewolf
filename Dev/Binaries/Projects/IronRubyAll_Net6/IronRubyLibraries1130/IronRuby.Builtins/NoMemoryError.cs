using System;
using System.Runtime.Serialization;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Serializable]
	[RubyException("NoMemoryError")]
	public class NoMemoryError : Exception
	{
		public NoMemoryError()
			: this(null, null)
		{
		}

		public NoMemoryError(string message)
			: this(message, null)
		{
		}

		public NoMemoryError(string message, Exception inner)
			: base(message ?? "NoMemoryError", inner)
		{
		}

		protected NoMemoryError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
