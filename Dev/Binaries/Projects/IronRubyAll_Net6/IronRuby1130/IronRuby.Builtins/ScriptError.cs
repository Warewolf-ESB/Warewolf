using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins
{
	[Serializable]
	public class ScriptError : Exception
	{
		public ScriptError()
			: this(null, null)
		{
		}

		public ScriptError(string message)
			: this(message, null)
		{
		}

		public ScriptError(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected ScriptError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
