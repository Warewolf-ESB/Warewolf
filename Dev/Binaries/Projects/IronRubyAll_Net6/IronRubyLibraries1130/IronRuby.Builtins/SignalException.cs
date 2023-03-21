using System;
using System.Runtime.Serialization;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Serializable]
	[RubyException("SignalException")]
	public class SignalException : Exception
	{
		public SignalException()
			: this(null, null)
		{
		}

		public SignalException(string message)
			: this(message, null)
		{
		}

		public SignalException(string message, Exception inner)
			: base(message ?? "SignalException", inner)
		{
		}

		protected SignalException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
