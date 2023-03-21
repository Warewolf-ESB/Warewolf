using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins
{
	[Serializable]
	public class RegexpError : SystemException
	{
		public RegexpError()
			: this(null, null)
		{
		}

		public RegexpError(string message)
			: this(message, null)
		{
		}

		public RegexpError(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected RegexpError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
