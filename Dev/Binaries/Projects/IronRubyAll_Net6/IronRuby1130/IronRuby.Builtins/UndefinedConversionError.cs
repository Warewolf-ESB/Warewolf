using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins
{
	[Serializable]
	public class UndefinedConversionError : EncodingError
	{
		public UndefinedConversionError()
			: this(null, null)
		{
		}

		public UndefinedConversionError(string message)
			: this(message, null)
		{
		}

		public UndefinedConversionError(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected UndefinedConversionError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
