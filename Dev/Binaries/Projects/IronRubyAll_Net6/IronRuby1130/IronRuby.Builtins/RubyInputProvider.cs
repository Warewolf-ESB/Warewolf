using System.Collections.Generic;
using System.IO;
using System.Threading;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	public sealed class RubyInputProvider
	{
		private readonly RubyContext _context;

		private object _singleton;

		private RubyIO _singletonStream;

		private IOMode _defaultMode;

		private readonly RubyArray _commandLineArguments;

		private int _currentFileIndex;

		private int _lastInputLineNumber;

		public RubyContext Context
		{
			get
			{
				return _context;
			}
		}

		public object Singleton
		{
			get
			{
				return _singleton;
			}
			internal set
			{
				_singleton = value;
			}
		}

		public RubyIO SingletonStream
		{
			get
			{
				return _singletonStream;
			}
			private set
			{
				_singletonStream = value;
			}
		}

		public IOMode DefaultMode
		{
			get
			{
				return _defaultMode;
			}
			set
			{
				_defaultMode = value;
			}
		}

		public RubyArray CommandLineArguments
		{
			get
			{
				return _commandLineArguments;
			}
		}

		public int LastInputLineNumber
		{
			get
			{
				return _lastInputLineNumber;
			}
			set
			{
				_lastInputLineNumber = value;
			}
		}

		public MutableString CurrentFileName
		{
			get
			{
				if (CommandLineArguments.Count == 0)
				{
					return MutableString.CreateAscii("-");
				}
				return (MutableString)CommandLineArguments[_currentFileIndex];
			}
		}

		internal RubyInputProvider(RubyContext context, ICollection<string> arguments, RubyEncoding encoding)
		{
			_context = context;
			RubyArray rubyArray = new RubyArray();
			foreach (string argument in arguments)
			{
				ExpandArgument(rubyArray, argument, encoding);
			}
			_commandLineArguments = rubyArray;
			_lastInputLineNumber = 1;
			_currentFileIndex = -1;
			_singleton = new object();
			_defaultMode = IOMode.ReadOnly;
		}

		public RubyIO GetCurrentStream()
		{
			return GetCurrentStream(false);
		}

		public RubyIO GetCurrentStream(bool reset)
		{
			if (SingletonStream == null || (reset && (SingletonStream.Closed || SingletonStream.IsEndOfStream())))
			{
				IncrementCurrentFileIndex();
				ResetCurrentStream();
			}
			return SingletonStream;
		}

		public RubyIO GetOrResetCurrentStream()
		{
			return GetCurrentStream(true);
		}

		public void ResetCurrentStream()
		{
			string path = CurrentFileName.ToString();
			Stream stream = RubyFile.OpenFileStream(_context, path, _defaultMode);
			SingletonStream = new RubyIO(_context, stream, _defaultMode);
		}

		public void IncrementLastInputLineNumber()
		{
			Interlocked.Increment(ref _lastInputLineNumber);
		}

		public void IncrementCurrentFileIndex()
		{
			Interlocked.Increment(ref _currentFileIndex);
		}

		private void ExpandArgument(RubyArray args, string arg, RubyEncoding encoding)
		{
			if (arg.IndexOf('*') != -1 || arg.IndexOf('?') != -1)
			{
				bool flag = false;
				foreach (string match in Glob.GetMatches(_context.DomainManager.Platform, arg, 0))
				{
					args.Add(MutableString.Create(match, encoding));
					flag = true;
				}
				if (!flag)
				{
					args.Add(MutableString.Create(arg, encoding));
				}
			}
			else
			{
				args.Add(MutableString.Create(arg, encoding));
			}
		}

		public bool HasMoreFiles()
		{
			return !object.Equals(_currentFileIndex, _commandLineArguments.Count - 1);
		}
	}
}
