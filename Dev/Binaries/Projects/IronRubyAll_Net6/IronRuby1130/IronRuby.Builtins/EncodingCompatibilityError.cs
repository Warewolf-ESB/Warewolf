using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins
{
	[Serializable]
	public class EncodingCompatibilityError : EncodingError
	{
		public EncodingCompatibilityError()
			: this(null, null)
		{
		}

		public EncodingCompatibilityError(string message)
			: this(message, null)
		{
		}

		public EncodingCompatibilityError(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected EncodingCompatibilityError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
