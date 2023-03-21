using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins
{
	[Serializable]
	public class RuntimeError : SystemException
	{
		public RuntimeError()
			: this(null, null)
		{
		}

		public RuntimeError(string message)
			: this(message, null)
		{
		}

		public RuntimeError(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected RuntimeError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
