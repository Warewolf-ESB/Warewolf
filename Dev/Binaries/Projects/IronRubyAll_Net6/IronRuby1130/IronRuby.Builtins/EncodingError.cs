using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins
{
	[Serializable]
	public class EncodingError : SystemException
	{
		public EncodingError()
			: this(null, null)
		{
		}

		public EncodingError(string message)
			: this(message, null)
		{
		}

		public EncodingError(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected EncodingError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
