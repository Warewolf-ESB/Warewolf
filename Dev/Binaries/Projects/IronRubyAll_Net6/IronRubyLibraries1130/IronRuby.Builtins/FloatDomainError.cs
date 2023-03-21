using System;
using System.Runtime.Serialization;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Serializable]
	[RubyException("FloatDomainError")]
	public class FloatDomainError : ArgumentOutOfRangeException
	{
		public FloatDomainError()
			: this(null, null)
		{
		}

		public FloatDomainError(string message)
			: this(message, null)
		{
		}

		public FloatDomainError(string message, Exception inner)
			: base(message ?? "FloatDomainError", inner)
		{
		}

		protected FloatDomainError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
