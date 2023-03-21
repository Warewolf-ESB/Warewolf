using System;
using System.Diagnostics;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	internal sealed class MutableStringBuilder
	{
		private RubyEncoding _encoding;

		private bool _isAscii;

		private char[] _chars;

		private byte[] _bytes;

		private int _charCount;

		private int _byteCount;

		public bool IsAscii
		{
			get
			{
				return _isAscii;
			}
		}

		public RubyEncoding Encoding
		{
			get
			{
				return _encoding;
			}
			set
			{
				_encoding = value;
			}
		}

		public MutableStringBuilder(RubyEncoding encoding)
		{
			_encoding = encoding;
			_isAscii = true;
		}

		public void Append(char c)
		{
			if (c >= '\u0080')
			{
				_isAscii = false;
			}
			int charCount = _charCount;
			Ensure(ref _chars, _charCount = charCount + 1);
			_chars[charCount] = c;
		}

		public void AppendAscii(char c)
		{
			int charCount = _charCount;
			Ensure(ref _chars, _charCount = charCount + 1);
			_chars[charCount] = c;
		}

		public void AppendUnicodeCodepoint(int codepoint)
		{
			if (codepoint < 65536)
			{
				Append((char)codepoint);
				return;
			}
			int charCount = _charCount;
			Ensure(ref _chars, _charCount = charCount + 2);
			codepoint -= 65536;
			_chars[charCount] = (char)(codepoint / 1024 + 55296);
			_chars[charCount + 1] = (char)(codepoint % 1024 + 56320);
			_isAscii = false;
		}

		public void AppendAscii(char[] chars, int start, int count)
		{
			int charCount = _charCount;
			Ensure(ref _chars, _charCount = charCount + count);
			Buffer.BlockCopy(chars, start << 1, _chars, charCount << 1, count << 1);
		}

		public void Append(byte b)
		{
			if (b < 128 && (_charCount != 0 || _byteCount == 0))
			{
				AppendAscii((char)b);
			}
			else if (_encoding == RubyEncoding.Binary)
			{
				Append((char)b);
			}
			else
			{
				AppendByte(b);
			}
		}

		private void AppendByte(byte b)
		{
			if (_charCount > 0)
			{
				int byteCount = _byteCount;
				Ensure(ref _bytes, _byteCount = byteCount + _encoding.Encoding.GetByteCount(_chars, 0, _charCount) + 1);
				byteCount += _encoding.Encoding.GetBytes(_chars, 0, _charCount, _bytes, byteCount);
				_bytes[byteCount++] = b;
				_charCount = 0;
			}
			else
			{
				int byteCount = _byteCount;
				Ensure(ref _bytes, _byteCount = byteCount + 1);
				_bytes[byteCount] = b;
			}
			if (b >= 128)
			{
				_isAscii = false;
			}
		}

		private void Ensure<T>(ref T[] array, int size)
		{
			if (array == null)
			{
				array = new T[Math.Max(16, size)];
			}
			else
			{
				Utils.Resize(ref array, size);
			}
		}

		public object ToValue()
		{
			object result;
			if (_byteCount <= 0)
			{
				result = ((_charCount <= 0) ? string.Empty : new string(_chars, 0, _charCount));
			}
			else
			{
				if (_charCount > 0)
				{
					Array.Resize(ref _bytes, _byteCount + _encoding.Encoding.GetByteCount(_chars, 0, _charCount));
					_encoding.Encoding.GetBytes(_chars, 0, _charCount, _bytes, _byteCount);
				}
				else
				{
					Array.Resize(ref _bytes, _byteCount);
				}
				result = _bytes;
			}
			_bytes = null;
			_chars = null;
			_charCount = (_byteCount = 0);
			return result;
		}

		public MutableString ToMutableString()
		{
			object obj = ToValue();
			string text = obj as string;
			if (text != null)
			{
				return MutableString.Create(text, _encoding);
			}
			return MutableString.CreateBinary((byte[])obj, _encoding);
		}

		[Conditional("DEBUG")]
		private void ClearChars()
		{
			_chars = null;
		}
	}
}
