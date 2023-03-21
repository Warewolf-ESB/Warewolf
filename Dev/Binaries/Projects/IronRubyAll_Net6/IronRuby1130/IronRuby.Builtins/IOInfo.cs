using System;
using System.Collections.Generic;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	public struct IOInfo
	{
		private readonly IOMode? _mode;

		private readonly RubyEncoding _externalEncoding;

		private readonly RubyEncoding _internalEncoding;

		public IOMode Mode
		{
			get
			{
				return _mode ?? IOMode.ReadOnly;
			}
		}

		public RubyEncoding ExternalEncoding
		{
			get
			{
				return _externalEncoding;
			}
		}

		public RubyEncoding InternalEncoding
		{
			get
			{
				return _internalEncoding;
			}
		}

		public bool HasEncoding
		{
			get
			{
				return _externalEncoding != null;
			}
		}

		public IOInfo(IOMode mode)
			: this(mode, null, null)
		{
		}

		public IOInfo(IOMode? mode, RubyEncoding externalEncoding, RubyEncoding internalEncoding)
		{
			_mode = mode;
			_externalEncoding = externalEncoding;
			_internalEncoding = internalEncoding;
		}

		public static IOInfo Parse(RubyContext context, MutableString modeAndEncoding)
		{
			if (!modeAndEncoding.IsAscii())
			{
				throw IOModeEnum.IllegalMode(modeAndEncoding.ToAsciiString());
			}
			string[] array = modeAndEncoding.ToString().Split(':');
			return new IOInfo(IOModeEnum.Parse(array[0]), (array.Length > 1) ? TryParseEncoding(context, array[1]) : null, (array.Length > 2) ? TryParseEncoding(context, array[2]) : null);
		}

		public IOInfo AddModeAndEncoding(RubyContext context, MutableString modeAndEncoding)
		{
			IOInfo result = Parse(context, modeAndEncoding);
			if (_mode.HasValue)
			{
				throw RubyExceptions.CreateArgumentError("mode specified twice");
			}
			if (!HasEncoding)
			{
				return result;
			}
			if (!result.HasEncoding)
			{
				return new IOInfo(result.Mode, _externalEncoding, _internalEncoding);
			}
			throw RubyExceptions.CreateArgumentError("encoding specified twice");
		}

		public IOInfo AddEncoding(RubyContext context, MutableString encoding)
		{
			if (!encoding.IsAscii())
			{
				context.ReportWarning(string.Format("Unsupported encoding {0} ignored", encoding.ToAsciiString()));
				return this;
			}
			if (HasEncoding)
			{
				throw RubyExceptions.CreateArgumentError("encoding specified twice");
			}
			string[] array = encoding.ToString().Split(':');
			return new IOInfo(_mode, TryParseEncoding(context, array[0]), (array.Length > 1) ? TryParseEncoding(context, array[1]) : null);
		}

		public static RubyEncoding TryParseEncoding(RubyContext context, string str)
		{
			try
			{
				return context.GetRubyEncoding(str);
			}
			catch (ArgumentException)
			{
				context.ReportWarning(string.Format("Unsupported encoding {0} ignored", str));
				return null;
			}
		}

		public IOInfo AddOptions(ConversionStorage<MutableString> toStr, IDictionary<object, object> options)
		{
			RubyContext context = toStr.Context;
			IOInfo result = this;
			object value;
			if (options.TryGetValue(context.CreateAsciiSymbol("encoding"), out value))
			{
				result = result.AddEncoding(context, Protocols.CastToString(toStr, value));
			}
			if (options.TryGetValue(context.CreateAsciiSymbol("mode"), out value))
			{
				result = result.AddModeAndEncoding(context, Protocols.CastToString(toStr, value));
			}
			return result;
		}
	}
}
