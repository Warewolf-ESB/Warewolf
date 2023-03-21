using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins
{
	[Serializable]
	public class SyntaxError : ScriptError
	{
		private readonly string _file;

		private readonly string _lineSourceCode;

		private readonly int _line;

		private readonly int _column;

		private readonly bool _hasLineInfo;

		internal string File
		{
			get
			{
				return _file;
			}
		}

		internal int Line
		{
			get
			{
				return _line;
			}
		}

		internal int Column
		{
			get
			{
				return _column;
			}
		}

		internal string LineSourceCode
		{
			get
			{
				return _lineSourceCode;
			}
		}

		internal bool HasLineInfo
		{
			get
			{
				return _hasLineInfo;
			}
		}

		public SyntaxError()
			: this(null, null)
		{
		}

		public SyntaxError(string message)
			: this(message, null)
		{
		}

		public SyntaxError(string message, Exception inner)
			: base(message, inner)
		{
		}

		internal SyntaxError(string message, string file, int line, int column, string lineSourceCode)
			: base(message)
		{
			_file = file;
			_line = line;
			_column = column;
			_lineSourceCode = lineSourceCode;
			_hasLineInfo = true;
		}

		protected SyntaxError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
