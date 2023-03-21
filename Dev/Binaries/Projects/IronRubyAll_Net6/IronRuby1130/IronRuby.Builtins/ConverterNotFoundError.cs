using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins
{
	[Serializable]
	public class ConverterNotFoundError : EncodingError
	{
		public ConverterNotFoundError()
			: this(null, null)
		{
		}

		public ConverterNotFoundError(string message)
			: this(message, null)
		{
		}

		public ConverterNotFoundError(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected ConverterNotFoundError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
