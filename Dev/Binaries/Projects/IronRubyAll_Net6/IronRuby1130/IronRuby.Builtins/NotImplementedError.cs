using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins
{
	[Serializable]
	public class NotImplementedError : ScriptError
	{
		public NotImplementedError()
			: this(null, null)
		{
		}

		public NotImplementedError(string message)
			: this(message, null)
		{
		}

		public NotImplementedError(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected NotImplementedError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
