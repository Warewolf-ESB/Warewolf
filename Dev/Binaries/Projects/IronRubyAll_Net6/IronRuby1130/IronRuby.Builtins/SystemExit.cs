using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins
{
	[Serializable]
	public class SystemExit : Exception
	{
		private readonly int _status;

		public int Status
		{
			get
			{
				return _status;
			}
		}

		public SystemExit(int status, string message)
			: this(message)
		{
			_status = status;
		}

		public SystemExit(int status)
			: this()
		{
			_status = status;
		}

		public SystemExit()
			: this(null, null)
		{
		}

		public SystemExit(string message)
			: this(message, null)
		{
		}

		public SystemExit(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected SystemExit(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
