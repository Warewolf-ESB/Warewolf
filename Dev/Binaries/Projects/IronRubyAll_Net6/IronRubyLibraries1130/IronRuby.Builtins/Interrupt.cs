using System;
using System.Runtime.Serialization;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Serializable]
	[RubyException("Interrupt", Inherits = typeof(SignalException))]
	public class Interrupt : Exception
	{
		public Interrupt()
			: this(null, null)
		{
		}

		public Interrupt(string message)
			: this(message, null)
		{
		}

		public Interrupt(string message, Exception inner)
			: base(message ?? "Interrupt", inner)
		{
		}

		protected Interrupt(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
