using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins
{
	[Serializable]
	public class InvalidByteSequenceError : EncodingError
	{
		public InvalidByteSequenceError()
			: this(null, null)
		{
		}

		public InvalidByteSequenceError(string message)
			: this(message, null)
		{
		}

		public InvalidByteSequenceError(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected InvalidByteSequenceError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
