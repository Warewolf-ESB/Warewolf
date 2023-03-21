using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins
{
	[Serializable]
	public class SystemStackError : SystemException
	{
		public SystemStackError()
			: this(null, null)
		{
		}

		public SystemStackError(string message)
			: this(message, null)
		{
		}

		public SystemStackError(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected SystemStackError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
