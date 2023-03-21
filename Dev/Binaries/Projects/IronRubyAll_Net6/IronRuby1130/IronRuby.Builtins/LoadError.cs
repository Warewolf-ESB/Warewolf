using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins
{
	[Serializable]
	public class LoadError : ScriptError
	{
		public LoadError()
			: this(null, null)
		{
		}

		public LoadError(string message)
			: this(message, null)
		{
		}

		public LoadError(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected LoadError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
