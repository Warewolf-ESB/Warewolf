using System;
using System.Runtime.Serialization;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Serializable]
	[RubyException("ThreadError")]
	public class ThreadError : SystemException
	{
		public ThreadError()
			: this(null, null)
		{
		}

		public ThreadError(string message)
			: this(message, null)
		{
		}

		public ThreadError(string message, Exception inner)
			: base(message ?? "ThreadError", inner)
		{
		}

		protected ThreadError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
