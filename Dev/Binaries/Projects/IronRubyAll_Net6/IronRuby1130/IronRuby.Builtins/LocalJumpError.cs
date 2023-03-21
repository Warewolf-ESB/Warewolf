using System;
using System.Runtime.Serialization;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Serializable]
	public class LocalJumpError : SystemException
	{
		[NonSerialized]
		private readonly RuntimeFlowControl _skipFrame;

		internal RuntimeFlowControl SkipFrame
		{
			get
			{
				return _skipFrame;
			}
		}

		internal LocalJumpError(string message, RuntimeFlowControl skipFrame)
			: this(message, (Exception)null)
		{
			_skipFrame = skipFrame;
		}

		public LocalJumpError()
			: this((string)null, (Exception)null)
		{
		}

		public LocalJumpError(string message)
			: this(message, (Exception)null)
		{
		}

		public LocalJumpError(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected LocalJumpError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
